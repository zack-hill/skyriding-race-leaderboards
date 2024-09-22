using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CompanionApp.Models;

namespace CompanionApp.Services;

public class AddonDataService
{
    private readonly GamePathService _gamePathService;
    private readonly Dictionary<string, DateTime> _lastWriteTimeLookup = new();
    
    public AddonDataService(GamePathService gamePathService)
    {
        _gamePathService = gamePathService;
    }
    
    public async IAsyncEnumerable<AddonFileData> GetAddonDataToUpload()
    {
        foreach (var addonFile in EnumerateAddonFiles())
        {
            var fileInfo = new FileInfo(addonFile);
            
            if (!HasFileBeenUpdatedSinceLastRead(fileInfo))
                continue;
        
            var accountRaceData = await ReadAccountRaceData(fileInfo.FullName);
            
            _lastWriteTimeLookup[fileInfo.FullName] = fileInfo.LastWriteTime;
            
            if (accountRaceData.CharacterRaceData.Count == 0)
                continue;
            
            var addonFileData = new AddonFileData
            {
                FilePath = addonFile,
                AccountRaceData = accountRaceData,
            };
        
            yield return addonFileData;
        }
    }

    public void ResetFileTracking(string filePath)
    {
        _lastWriteTimeLookup.Remove(filePath);
    }

    private IEnumerable<string> EnumerateAddonFiles()
    {
        var gamePath = _gamePathService.GamePath;
        if (gamePath == null)
            return Enumerable.Empty<string>();
        
        var wtfAccountDirectory = Path.Combine(gamePath, @"_retail_\WTF\Account");
        return Directory.EnumerateFiles(
            wtfAccountDirectory,
            "SkyridingRaceLeaderboardDataCollector.lua",
            SearchOption.AllDirectories);
    }

    private bool HasFileBeenUpdatedSinceLastRead(FileSystemInfo fileInfo)
    {
        var lastWriteTime = fileInfo.LastWriteTime;

        return !_lastWriteTimeLookup.TryGetValue(fileInfo.FullName, out var previousLastWriteTime) ||
               lastWriteTime > previousLastWriteTime;
    }
    
    private static async Task<AccountRaceData> ReadAccountRaceData(string filePath)
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
                if (raceDataDictionary != null)
                {
                    foreach (var (key, value) in raceDataDictionary)
                    {
                        var raceTime = new RaceTime
                        {
                            RaceId = int.Parse(key),
                            TimeMs = value,
                        };
                        characterRaceData.RaceTimes.Add(raceTime);
                    }
                }

                if (characterRaceData.RaceTimes.Count == 0)
                    continue;

                accountRaceData.CharacterRaceData.Add(characterRaceData);
            }
        }
        
        return accountRaceData;
    }
}