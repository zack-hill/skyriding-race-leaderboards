using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using CompanionApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using FolderBrowserDialog = FolderBrowserEx.FolderBrowserDialog;

namespace CompanionApp.Services;

public class GamePathService
{
    public string? GamePath
    {
        get => _gamePath;
        private set
        {
            _gamePath = value;
            IsGamePathValid = ValidateWoWDirectory(value);
            GamePathUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
    private string? _gamePath;

    public bool IsGamePathValid { get; private set; }
    
    private const string WowDirectoryConfigKey = "WoW.Directory";
    
    private readonly ILogger<GamePathService> _logger;

    private static readonly string[] RegistryKeyLocations =
    [
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\World of Warcraft",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\World of Warcraft",
        @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\World of Warcraft",
        @"HKEY_CURRENT_USER\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\World of Warcraft"
    ];

    public event EventHandler? GamePathUpdated;

    public GamePathService(ILogger<GamePathService> logger)
    {
        _logger = logger;
    }

    public void AutoDetectGamePathIfNecessary()
    {
        GamePath = GetGamePathFromConfig();

        if (string.IsNullOrEmpty(GamePath))
        {
            GamePath = FindWowInstall();
        }
        else
        {
            _logger.LogInformation($"Game path set to {GamePath}");
        }
    }

    public void BrowseForGamePath()
    {
        var folderBrowserDialog = new FolderBrowserDialog
        {
            Title = "Select your WoW directory (contains _retail_)",
            InitialFolder = GamePath,
            AllowMultiSelect = false
        };
        if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
        {
            GamePath = folderBrowserDialog.SelectedFolder;
            _logger.LogInformation($"Game path set to {GamePath}");
        }
    }
    
    private string? GetGamePathFromConfig()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "userSettings.json");
        try
        {
            var json = File.ReadAllText(filePath);
            var userSettings = JsonSerializer.Deserialize<UserSettings>(json);
            return userSettings?.GamePath;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, $"Error reading user settings {GamePath}");
            return null;
        }
    }

    private static void SetGamePathInConfig(string value)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "userSettings.json");
        var serialized = JsonSerializer.Serialize(new UserSettings { GamePath = value});
        File.WriteAllText(filePath, serialized);
    }
    
    private string? FindWowInstall()
    {
        foreach (var registryKeyLocation in RegistryKeyLocations)
        {
            var path = Registry.GetValue(registryKeyLocation, "InstallLocation", null)?.ToString();
            if (ValidateWoWDirectory(path))
            {
                _logger.LogInformation($"Auto detected game path of {path}");
                SetGamePathInConfig(path!);
                return path;
            }
        }

        return null;
    }

    private bool ValidateWoWDirectory(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }
        
        if (!Directory.Exists(path))
        {
            _logger.LogError($"Unable to locate directory '{path}'");
            return false;
        }

        var retailDir = Path.Combine(path, "_retail_");
        if (!Directory.Exists(retailDir))
        {
            _logger.LogError($"Unable to locate _retail_ folder at '{path}'");
            return false;
        }

        return true;
    }
}