using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CompanionApp.Models;
using CompanionApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CompanionApp;

public class Worker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Worker> _logger;

    private readonly GamePathService _gamePathService;
    private readonly TrayIconService _trayIconService;
    private readonly AddonDataService _addonDataService;
    private readonly TimeSpan _scanInterval;

    public Worker(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        GamePathService gamePathService,
        TrayIconService trayIconService,
        AddonDataService addonDataService,
        ILogger<Worker> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _gamePathService = gamePathService;
        _trayIconService = trayIconService;
        _addonDataService = addonDataService;
        _logger = logger;

        if (!int.TryParse(_configuration["ScanIntervalSeconds"], out var scanIntervalSeconds) ||
            scanIntervalSeconds <= 0)
            scanIntervalSeconds = 60;
        _scanInterval = TimeSpan.FromSeconds(scanIntervalSeconds);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting up");
        _trayIconService.CreateTrayIcon();
        _gamePathService.AutoDetectGamePathIfNecessary();
        
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var addonDataToUpload = _addonDataService.GetAddonDataToUpload();
            try
            {
                await foreach (var addonFileData in addonDataToUpload.WithCancellation(cancellationToken))
                {
                    try
                    {
                        _logger.LogInformation($"Uploading race data for {addonFileData.AccountRaceData.BattleTag}");
                        await UploadRaceData(addonFileData.AccountRaceData);
                        _logger.LogInformation($"Race data uploaded successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to upload race data: {ex.Message}");

                        // Reset our tracking data for this file to allow additional upload attempts
                        _addonDataService.ResetFileTracking(addonFileData.FilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to parse addon data: {ex.Message}");
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
