#define MyAppName "SwitchyLingus"
#define MyAppExeName "SwitchyLingus.UI.exe"
#define MyAppId "{{E7A1B2C3-4D5E-6F70-8192-A3B4C5D6E7F8}"
#define MyAppParentDir "..\.."
#define MyAppBinDir MyAppParentDir + "\SwitchyLingus.UI\bin\Release\net9.0-windows\win-x64"
#define MyAppIconPath MyAppParentDir + "\SwitchyLingus.UI\Logo.ico"
#define MyAppVersion GetStringFileInfo(MyAppBinDir + "\" + MyAppExeName, "FileVersion")
#define MyAppPublisher "ATC"

[Setup]
AppId={#MyAppId}
AppMutex={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://github.com/ATC/SwitchyLingus
AppSupportURL=https://github.com/ATC/SwitchyLingus/issues
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile={#MyAppIconPath}
OutputDir=Output
OutputBaseFilename=SwitchyLingusSetup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=none
MinVersion=10.0
CloseApplications=yes
RestartApplications=no
DisableProgramGroupPage=yes
UsedUserAreasWarning=no

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; Flags: unchecked; Check: not IsUpdating
Name: "startup"; Description: "Run {#MyAppName} at Windows startup"; Check: not IsUpdating

[Files]
Source: "{#MyAppBinDir}\*"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent; Check: not IsNoRun

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\SwitchyLingus"

[UninstallRun]
Filename: "taskkill.exe"; Parameters: "/F /IM {#MyAppExeName}"; Flags: runhidden; RunOnceId: "KillApp"

[UninstallRegistry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "{#MyAppName}"; Flags: uninsdeletevalue

[Code]
function CmdLineParamExists(const Value: string): Boolean;
var
  I: Integer;
begin
  Result := False;
  for I := 1 to ParamCount do
    if CompareText(ParamStr(I), Value) = 0 then
    begin
      Result := True;
      Exit;
    end;
end;

function IsUpdating(): Boolean;
begin
  Result := CmdLineParamExists('/UPDATE');
end;

function IsNoRun(): Boolean;
begin
  Result := CmdLineParamExists('/NORUN');
end;

procedure InitializeWizard();
begin
  if not IsAdmin then
    WizardForm.DirEdit.Text := ExpandConstant('{userpf}\{#MyAppName}');
end;
