using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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

    private AddonDataManager _addonDataManager;
    private readonly TimeSpan _scanInterval;

    public Worker(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<Worker> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        if (!int.TryParse(_configuration["ScanIntervalSeconds"], out var scanIntervalSeconds) ||
            scanIntervalSeconds <= 0)
            scanIntervalSeconds = 60;
        _scanInterval = TimeSpan.FromSeconds(scanIntervalSeconds);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var wowPath = new GamePathManager(_configuration).GetGamePath();
        Console.Clear();
        _logger.LogInformation($"Using directory: {wowPath}");

        _addonDataManager = new AddonDataManager(wowPath);
        
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var addonDataToUpload = _addonDataManager.GetAddonDataToUpload();
            await foreach (var addonFileData in addonDataToUpload)
            {
                try
                {
                    _logger.LogInformation($"Uploading race data for {addonFileData.AccountRaceData.BattleTag}");
                    await UploadRaceData(addonFileData.AccountRaceData);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to upload race data: {ex.Message}");
                    
                    // Reset our tracking data for this file to allow additional upload attempts
                    _addonDataManager.ResetFileTracking(addonFileData.FilePath);
                }
            }
            
            await Task.Delay(_scanInterval, cancellationToken);
        }
    }
    
    private async Task UploadRaceData(AccountRaceData accountRaceData)
    {
        var a = JsonSerializer.Serialize(accountRaceData);
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
