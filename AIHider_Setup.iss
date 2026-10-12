[Setup]
AppName=AI Hider
AppVersion=1.0.0
AppPublisher=W.K. Amila Sandaruwan
AppPublisherURL=https://github.com/amila-sandaruwan-git
DefaultDirName={autopf}\AI Hider
DefaultGroupName=AI Hider
OutputDir=Output
OutputBaseFilename=AIHider_Setup
SetupIconFile=app.ico
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "bin\Release\net10.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "app.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\AI Hider"; Filename: "{app}\AIHider.exe"; IconFilename: "{app}\app.ico"
Name: "{autodesktop}\AI Hider"; Filename: "{app}\AIHider.exe"; IconFilename: "{app}\app.ico"; Tasks: desktopicon
Name: "{group}\Uninstall AI Hider"; Filename: "{uninstallexe}"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
Filename: "{app}\AIHider.exe"; Description: "{cm:LaunchProgram,AI Hider}"; Flags: nowait postinstall skipifsilent