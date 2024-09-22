#define MyAppName "Skyriding Race Leaderboards Companion App"
#define MyAppExeName "SkyridingRaceLeaderboardsCompanionApp.exe"
#define SourceDir = "bin\net8.0-windows\win-x64\publish"
#define AppExeVersion = GetVersionNumbersString("D:\dev\repos\skyriding-race-leaderboards\CompanionApp\bin\Debug\net8.0-windows\SkyridingRaceLeaderboardsCompanionApp.exe")

[Setup]
AppName={#MyAppName}
AppVersion={#AppExeVersion}
WizardStyle=modern
SourceDir=..\
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
OutputDir=Setup\Output\
OutputBaseFilename={#MyAppName} Setup
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
SetupIconFile=Resources\favicon.ico

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Parameters: "/auto"; Tasks: autostarticon

[Tasks]
Name: "autostarticon"; Description: "{cm:AutoStartProgram,{#MyAppName}}"; GroupDescription: "{cm:AdditionalIcons}";

[Run]
Filename: {app}\{#MyAppExeName}; Description: {cm:LaunchProgram,{#MyAppName}}; Flags: nowait postinstall skipifsilent
