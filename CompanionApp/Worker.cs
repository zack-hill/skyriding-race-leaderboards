using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace CompanionApp;

public class Worker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<Worker> _logger;

    public Worker(IConfiguration configuration, ILogger<Worker> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var wowDirectoryConfigKey = "WoW.Directory";
        var wowDirectory = _configuration[wowDirectoryConfigKey];
        if (!Directory.Exists(wowDirectory))
        {
            Console.WriteLine($"Unable to auto-detect WoW directory: {wowDirectory}");
            Console.WriteLine("Enter your WoW directory, choose the directory that contains the _retail_ folder:");
            wowDirectory = Console.ReadLine();
            UpdateAppSettings(wowDirectoryConfigKey, wowDirectory);
            // SetValueInAppSettings(wowDirectoryConfigKey, wowDirectory);
        }
        else
        {
            Console.WriteLine($"Using directory: {wowDirectory}");
        }
        
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }

    private static void SetValueInAppSettings(string key, string value)
    {
        try
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            if (settings[key] == null)
            {
                settings.Add(key, value);
            }
            else
            {
                settings[key].Value = value;
            }
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }
        catch (ConfigurationErrorsException)
        {
            Console.WriteLine("Error writing app settings");
        }
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
}
