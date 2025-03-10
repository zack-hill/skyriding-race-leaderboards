#define MyAppName "Skyriding Race Leaderboards Companion App"
#define MyAppNameNoSpaces "SkyridingRaceLeaderboardsCompanionApp"
#define MyAppExeName "SkyridingRaceLeaderboardsCompanionApp.exe"
#define SourceDir = "bin\net8.0-windows\win-x64\publish"
#define AppExeVersion = GetVersionNumbersString("..\" + SourceDir + "\SkyridingRaceLeaderboardsCompanionApp.exe")

[Setup]
AppName={#MyAppName}
AppVersion={#AppExeVersion}
WizardStyle=modern
SourceDir=..\
DefaultDirName={autopf}\{#MyAppNameNoSpaces}
DefaultGroupName={#MyAppName}
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
OutputDir=Setup\Output\
OutputBaseFilename={#MyAppNameNoSpaces}-{#AppExeVersion}-Setup
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
SetupIconFile=Resources\favicon.ico
CloseApplications=force

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Parameters: "/auto"; Tasks: autostarticon

[Tasks]
Name: "autostarticon"; Description: "{cm:AutoStartProgram,{#MyAppName}}"; GroupDescription: "{cm:AdditionalIcons}";

[Run]
Filename: {app}\{#MyAppExeName}; Description: {cm:LaunchProgram,{#MyAppName}}; Flags: nowait postinstall skipifsilent
