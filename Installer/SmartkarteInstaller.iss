#define public Dependency_Path_NetCoreCheck "dependencies\"
#include "SmartkarteDependencies.iss"


[Setup]
#define MyAppSetupName 'SmartKarteApp'
#define MyAppVersion '1.0.0.3'
#define MyAppPublisher 'SmartKarte Setup'
#define MyAppCopyright 'Copyright © Sotatek'
#define MyAppURL 'https://smartkarte.sotatek.works/'

AppName={#MyAppSetupName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppSetupName} {#MyAppVersion}
AppCopyright={#MyAppCopyright}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
OutputBaseFilename={#MyAppSetupName}-{#MyAppVersion}
DefaultGroupName={#MyAppSetupName}
DefaultDirName={commonappdata}\{#MyAppSetupName}
UninstallDisplayIcon={app}\SmartKarteApp.exe
SetupIconFile={#SourcePath}\Images\Hayabusa_icon.ico
OutputDir={#SourcePath}\bin
AllowNoIcons=yes
PrivilegesRequired=lowest

; remove next line if you only deploy 32-bit binaries and dependencies
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: ja; MessagesFile: "compiler:Languages\Japanese.isl"
//Name: en; MessagesFile: "compiler:Default.isl"


[Files]
Source: "..\bin\Release\net6.0-windows\OnlineService.exe"; DestDir: "{app}"; DestName: "OnlineService.exe"; Flags: ignoreversion; Permissions: users-modify
Source: "..\bin\Release\net6.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Permissions: users-modify
Source: "..\Installer\SetupHttps.ps1"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Permissions: users-modify
Source: "..\Installer\download_dotnet6.bat"; DestDir: "{tmp}"; Flags: ignoreversion recursesubdirs createallsubdirs; Permissions: users-modify

[Icons]
Name: "{group}\{#MyAppSetupName}"; Filename: "{app}\OnlineService.exe"
Name: "{group}\{cm:UninstallProgram,{#MyAppSetupName}}"; Filename: "{uninstallexe}"
; Name: "{commondesktop}\{#MyAppSetupName}"; Filename: "{app}\OnlineService.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"

[Run]
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\SetupHttps.ps1"""; WorkingDir: {app}; Flags: runhidden
Filename: "{app}\OnlineService.exe"; Description: "{cm:LaunchProgram,{#MyAppSetupName}}"; Flags: nowait postinstall skipifsilent


[Code]
function InitializeSetup: Boolean;
begin  
  #ifdef Dependency_Path_NetCoreCheck
    
    Dependency_AddDotNet60;
    Dependency_AddDotNet60Asp;
    Dependency_AddDotNet60Desktop;
  #endif
  Result := True;
end;