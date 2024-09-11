using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using CompanionApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CompanionApp;

public class Worker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<Worker> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        DetermineWoWDirectory();
        
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, cancellationToken);
            
            // read file
            var accountRaceData = await ParseData(@"C:\Program Files (x86)\World of Warcraft\_retail_\WTF\Account\64201889#2\SavedVariables\SkyRidingRaceLeaderboardDataCollector.lua");
            
            Console.WriteLine("Uploading race data");
            await UploadRaceData(accountRaceData);
        }
    }

    private void DetermineWoWDirectory()
    {
        const string wowDirectoryConfigKey = "WoW.Directory";
        var wowDirectory = _configuration[wowDirectoryConfigKey];
        
        // TODO: Try auto-detect

        if (string.IsNullOrEmpty(wowDirectory))
        {
            Console.WriteLine("Unable to auto-detect WoW directory");
        }

        var updated = false;
        while (!ValidateWoWDirectory(wowDirectory))
        {
            Console.WriteLine("Enter your WoW directory, choose the directory that contains the _retail_ folder:");
            wowDirectory = Console.ReadLine();
            updated = true;
        }

        if (updated)
        {
            UpdateAppSettings(wowDirectoryConfigKey, wowDirectory);
        }
        
        Console.WriteLine($"Using directory: {wowDirectory}");
    }

    private static bool ValidateWoWDirectory(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }
        
        if (!Directory.Exists(path))
        {
            Console.WriteLine($"Unable to locate directory '{path}'");
            return false;
        }

        var retailDir = Path.Combine(path, "_retail_");
        if (!Directory.Exists(retailDir))
        {
            Console.WriteLine($"Unable to locate _retail_ folder at '{retailDir}'");
            return false;
        }

        return true;
    }

    private static void UpdateAppSettings(string key, string value)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "appSettings.json");
        var json = File.ReadAllText(filePath);
        var jsonObj = JsonSerializer.Deserialize<JsonNode>(json);
        var sectionPath = key.Split(":")[0];
        
        jsonObj![sectionPath] = value;

        var asd = JsonSerializer.Serialize(jsonObj);
        File.WriteAllText(filePath, asd);
    }

    private static async Task<AccountRaceData> ParseData(string filePath)
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        var accountRaceData = new AccountRaceData();
        foreach (var line in lines)
        {
            var parts = line.Split('=', 2);
            if (parts.Length < 2)
                continue;

            if (parts[0].Contains("BattleTag"))
            {
                accountRaceData.BattleTag = parts[1].Trim().Trim(',').Trim('"');
            }
            else if (parts[0].Contains("CharacterRaceData-"))
            {
                var characterRaceData = new CharacterRaceData
                {
                    CharacterName =  parts[0].Trim().Trim('[').Trim(']').Trim('"').Split('-', 2)[1],
                };

                var base64String = parts[1].Trim().Trim(',').Trim('"');
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
                var raceDataDictionary = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                foreach (var (key, value) in raceDataDictionary)
                {
                    var raceTime = new RaceTime
                    {
                        RaceId = int.Parse(key),
                        TimeMs = value,
                    };
                    characterRaceData.RaceTimes.Add(raceTime);
                }
                
                accountRaceData.CharacterRaceData.Add(characterRaceData);
            }
        }
        return accountRaceData;
    }
    
    private async Task UploadRaceData(AccountRaceData accountRaceData)
    {
        var uploadUrl = _configuration["UploadUrl"];
        var client = _httpClientFactory.CreateClient();
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(uploadUrl!),
            Content = JsonContent.Create(accountRaceData)
        };
        await client.SendAsync(httpRequestMessage);
    }
}
