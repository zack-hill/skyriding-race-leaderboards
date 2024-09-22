using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using H.NotifyIcon.Core;

namespace CompanionApp.Services;

public class TrayIconService : IDisposable
{
    private readonly GamePathService _gamePathService;
    private TrayIconWithContextMenu? _trayIcon;

    public TrayIconService(GamePathService gamePathService)
    {
        _gamePathService = gamePathService;
        _gamePathService.GamePathUpdated += GamePathServiceOnGamePathUpdated;
    }
    
    public void CreateTrayIcon()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var iconStream = assembly.GetManifestResourceStream("CompanionApp.Resources.favicon.ico");
        var icon = new Icon(iconStream!);
        _trayIcon = new TrayIconWithContextMenu
        {
            Icon = icon.Handle,
            ToolTip = "Skyriding Race Leaderboards Companion App",
        };
        RebuildTrayIconContextMenu();
        _trayIcon.Create();
    }

    private void RebuildTrayIconContextMenu()
    {
        if (_trayIcon == null)
            return;
        
        const string warningIcon = "\u26a0\ufe0f ";
        var selectGameDirectoryButtonLabel = _gamePathService.GamePath == null
            ? "Select Game Directory"
            : (!_gamePathService.IsGamePathValid ? warningIcon : "") + _gamePathService.GamePath;
        _trayIcon.ContextMenu = new PopupMenu
        {
            Items =
            {
                new PopupMenuItem($" {selectGameDirectoryButtonLabel}", (_, _) => _gamePathService.BrowseForGamePath()),
                new PopupMenuSeparator(),
                new PopupMenuItem("View Log", (_, _) => ViewLog()),
                new PopupMenuSeparator(),
                new PopupMenuItem("Exit", (_, _) =>
                {
                    Environment.Exit(0);
                }),
            },
        };
    }

    private void ViewLog()
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SkyridingRaceLeaderboards\\log.txt");
        Process.Start("notepad.exe", logPath);
    }
    
    private void GamePathServiceOnGamePathUpdated(object? sender, EventArgs e)
    {
        RebuildTrayIconContextMenu();
    }

    public void Dispose()
    {
        _trayIcon?.Dispose();
    }
}