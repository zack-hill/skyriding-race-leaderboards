using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CompanionApp.Services;

public class AddonInstallationService
{
    private readonly IConfiguration _configuration;
    private readonly GamePathService _gamePathService;
    private readonly ILogger<AddonInstallationService> _logger;

    public AddonInstallationService(
        IConfiguration configuration,
        GamePathService gamePathService,
        ILogger<AddonInstallationService> logger)
    {
        _configuration = configuration;
        _gamePathService = gamePathService;
        _logger = logger;
    }

    public async Task InstallAddon()
    {
        if (!_gamePathService.IsGamePathValid)
        {
            _logger.LogWarning("Cannot install addon, game path is invalid");
            return;
        }

        try
        {
            var gamePath = _gamePathService.GamePath!;
            var addonPath = Path.Combine(gamePath, "_retail_\\Interface\\Addons\\SkyridingRaceLeaderboards");
        
            var downloadUrl = $"{_configuration["WebsiteUrl"]}/download?file=addon";
            using var client = new HttpClient();
            _logger.LogInformation("Downloading file");
            using var result = await client.GetAsync(downloadUrl);
            var bytes = result.IsSuccessStatusCode
                ? await result.Content.ReadAsByteArrayAsync()
                : null;
            if (bytes == null)
            {
                _logger.LogError("Failed to download addon");
                return;
            }
        
            var tempPath = Path.GetTempFileName();
            try
            {
                _logger.LogInformation($"Writing file to {tempPath}");
                await File.WriteAllBytesAsync(tempPath, bytes);
                
                if (Directory.Exists(addonPath))
                {
                    _logger.LogInformation($"Deleting {addonPath}");
                    Directory.Delete(addonPath, true);
                }
                
                _logger.LogInformation($"Unzipping {tempPath} to {addonPath}");
                ZipFile.ExtractToDirectory(tempPath, addonPath);
            }
            finally
            {
                _logger.LogInformation($"Deleting {tempPath}");
                File.Delete(tempPath);
            }
            
            _logger.LogInformation($"Addon installed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install addon");
            MessageBox.Show(
                "Failed to install addon. View log for more details.",
                "Skyriding Race Leaderboards Companion App",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}