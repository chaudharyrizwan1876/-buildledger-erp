; Inno Setup script for BuildLedger ERP.
; Build the self-contained release first (scripts/publish.ps1), then compile this with Inno Setup
; (https://jrsoftware.org/isinfo.php) to produce a single Windows installer .exe.
;
; ISCC.exe scripts\installer.iss

#define MyAppName "BuildLedger ERP"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "BuildLedger"
#define MyAppExeName "BuildLedger.App.exe"

[Setup]
AppId={{B1A6C8B0-0B7C-4B7B-9B7F-BUILDLEDGERERP}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=..\dist
OutputBaseFilename=BuildLedgerERP-Setup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
