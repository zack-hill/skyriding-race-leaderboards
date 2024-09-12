using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace CompanionApp;

public class GamePathManager
{
    private const string WowDirectoryConfigKey = "WoW.Directory";
    
    private readonly IConfiguration _configuration;
    
    private static readonly string[] RegistryKeyLocations =
    [
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\World of Warcraft",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\World of Warcraft",
        @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\World of Warcraft",
        @"HKEY_CURRENT_USER\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\World of Warcraft"
    ];

    public GamePathManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string GetGamePath()
    {
        var wowDirectory = GetGamePathFromConfig();
        
        if (string.IsNullOrEmpty(wowDirectory))
        {
            wowDirectory = FindWowInstall();
            
            if (ValidateWoWDirectory(wowDirectory))
            {
                Console.WriteLine($"Auto detected WoW directory of {wowDirectory}");
                Console.WriteLine("Is this correct? (Y/N)");
                while (true)
                {
                    var input = Console.ReadLine();
                    
                    if (input?.Trim().ToUpper() == "Y")
                    {
                        SetGamePathInConfig(wowDirectory!);
                        return wowDirectory!;
                    }
                    
                    if (input?.Trim().ToUpper() == "N")
                        break;

                    Console.WriteLine("Unrecognized input. Please input 'Y' or 'N'.");
                }
            }
            else
            {
                Console.WriteLine("Unable to auto-detect WoW directory");
            }
        }
        else if (ValidateWoWDirectory(wowDirectory))
        {
            return wowDirectory;
        }

        do
        {
            Console.WriteLine("Enter your WoW directory, choose the directory that contains the _retail_ folder:");
            wowDirectory = Console.ReadLine();
        } while (!ValidateWoWDirectory(wowDirectory));

        SetGamePathInConfig(wowDirectory!);
        return wowDirectory!;
    }
    
    private string? GetGamePathFromConfig()
    {
        return _configuration[WowDirectoryConfigKey];
    }

    private static void SetGamePathInConfig(string value)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "appSettings.json");
        var json = File.ReadAllText(filePath);
        var jsonObj = JsonSerializer.Deserialize<JsonNode>(json);
        var sectionPath = WowDirectoryConfigKey.Split(":")[0];
        
        jsonObj![sectionPath] = value;

        var asd = JsonSerializer.Serialize(jsonObj);
        File.WriteAllText(filePath, asd);
    }
    
    private static string? FindWowInstall()
    {
        foreach (var registryKeyLocation in RegistryKeyLocations)
        {
            var path = Registry.GetValue(registryKeyLocation, "InstallLocation", null)?.ToString();
            if (ValidateWoWDirectory(path))
            {
                return path;
            }
        }

        return null;
    }

    private static bool ValidateWoWDirectory(string? path)
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
}