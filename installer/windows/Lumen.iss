; Inno Setup-script voor Lumen (Windows-installer)
; Bouwen: installer\windows\build.ps1  (of automatisch via GitHub Actions)

#define AppName "Lumen"
#define AppExe "Lumen.exe"
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\..\artifacts\publish\win-x64"
#endif

[Setup]
AppId={{8F3B2C71-5E4A-4C2B-9D61-3A7E2F1B9C40}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=Kyell
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=..\..\artifacts\installer
OutputBaseFilename=Lumen-Setup-{#AppVersion}-x64
SetupIconFile=..\..\src\Lumen\Assets\lumen.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequiredOverridesAllowed=dialog
ChangesAssociations=yes
CloseApplications=yes

[Languages]
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "associate"; Description: "Lumen tonen bij 'Openen met' voor video- en muziekbestanden"; GroupDescription: "Bestandskoppelingen:"

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Registry]
; Bestandstype van Lumen
Root: HKA; Subkey: "Software\Classes\Lumen.Media"; ValueType: string; ValueName: ""; ValueData: "Mediabestand"; Flags: uninsdeletekey; Tasks: associate
Root: HKA; Subkey: "Software\Classes\Lumen.Media\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#AppExe},0"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\Lumen.Media\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExe}"" ""%1"""; Tasks: associate
; Registratie bij Windows "Standaard-apps"
Root: HKA; Subkey: "Software\Lumen"; Flags: uninsdeletekey; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities"; ValueType: string; ValueName: "ApplicationName"; ValueData: "{#AppName}"; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities"; ValueType: string; ValueName: "ApplicationDescription"; ValueData: "Moderne mediaspeler op basis van de VLC-engine"; Tasks: associate
Root: HKA; Subkey: "Software\RegisteredApplications"; ValueType: string; ValueName: "{#AppName}"; ValueData: "Software\Lumen\Capabilities"; Flags: uninsdeletevalue; Tasks: associate
; Per extensie
Root: HKA; Subkey: "Software\Classes\.mp4\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".mp4"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.mkv\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".mkv"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.avi\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".avi"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.mov\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".mov"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.wmv\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".wmv"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.webm\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".webm"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.m4v\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".m4v"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.flv\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".flv"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.mpg\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".mpg"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.mpeg\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".mpeg"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.ts\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".ts"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.m2ts\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".m2ts"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.3gp\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".3gp"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.ogv\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".ogv"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.mp3\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".mp3"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.flac\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".flac"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.wav\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".wav"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.aac\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".aac"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.m4a\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".m4a"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.ogg\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".ogg"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.opus\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".opus"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.wma\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".wma"; ValueData: "Lumen.Media"; Tasks: associate
Root: HKA; Subkey: "Software\Classes\.aiff\OpenWithProgids"; ValueType: string; ValueName: "Lumen.Media"; ValueData: ""; Flags: uninsdeletevalue; Tasks: associate
Root: HKA; Subkey: "Software\Lumen\Capabilities\FileAssociations"; ValueType: string; ValueName: ".aiff"; ValueData: "Lumen.Media"; Tasks: associate

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent
