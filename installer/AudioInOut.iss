; Build: ISCC /DAppVersion=0.4.0 installer\AudioInOut.iss  (needs Build\Release from a Release x86 build)
#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif

[Setup]
AppId={{6F1B7C52-3A0E-4D6B-9C47-1D2E8A5B0F31}
AppName=AudioInOut
AppVersion={#AppVersion}
AppPublisher=jjffjjff
AppPublisherURL=https://github.com/jjffjjff/AudioInOut
DefaultDirName={autopf32}\AudioInOut
DefaultGroupName=AudioInOut
PrivilegesRequired=admin
OutputDir=..\Output
OutputBaseFilename=AudioInOut-Setup
SetupIconFile=..\AudioInOut\Assets\Icon-Light.ico
UninstallDisplayIcon={app}\AudioInOut.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
MinVersion=10.0

[Tasks]
Name: startup; Description: "Start AudioInOut when I sign in to Windows"
Name: desktopicon; Description: "Create a desktop shortcut"; Flags: unchecked

[Files]
Source: "..\Build\Release\*"; DestDir: "{app}"; Excludes: "*.pdb,*.winmd,XamlAnimatedGif.*"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\AudioInOut"; Filename: "{app}\AudioInOut.exe"
Name: "{autodesktop}\AudioInOut"; Filename: "{app}\AudioInOut.exe"; Tasks: desktopicon

; Same key the in-app "Start with Windows" toggle uses, so the two stay in sync.
[Registry]
Root: HKCU; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "AudioInOut"; ValueData: """{app}\AudioInOut.exe"""; Tasks: startup; Flags: uninsdeletevalue

[Run]
Filename: "{app}\AudioInOut.exe"; Description: "Launch AudioInOut"; Flags: nowait postinstall skipifsilent
