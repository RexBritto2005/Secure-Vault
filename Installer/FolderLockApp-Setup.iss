; FolderLockApp Installer Script for Inno Setup
; This script creates a Windows installer for the FolderLockApp application
; Requirements: 3.1 - Service installation and registration

#define MyAppName "FolderLock"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "FolderLock Team"
#define MyAppURL "https://github.com/folderlock"
#define MyAppExeName "FolderLockApp.GUI.exe"
#define MyServiceName "FolderLockApp.Service.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
AppId={{A7B8C9D0-E1F2-4A5B-8C9D-0E1F2A3B4C5D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\LICENSE.txt
OutputDir=..\Installer\Output
OutputBaseFilename=FolderLockApp-Setup-{#MyAppVersion}
SetupIconFile=..\FolderLockApp.GUI\Assets\lock-icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\{#MyAppExeName}
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; GUI Application
Source: "..\FolderLockApp.GUI\bin\Release\net8.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Windows Service
Source: "..\FolderLockApp.Service\bin\Release\net8.0\*"; DestDir: "{app}\Service"; Flags: ignoreversion recursesubdirs createallsubdirs
; Shell Extension
Source: "..\FolderLockApp.ShellExtension\bin\Release\net8.0\*"; DestDir: "{app}\ShellExtension"; Flags: ignoreversion recursesubdirs createallsubdirs
; Documentation
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\SERVICE-INSTALLATION.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TESTING-GUIDE.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Dirs]
; Create application data directory for SQLite database
Name: "{commonappdata}\FolderLockApp"; Permissions: users-modify
Name: "{commonappdata}\FolderLockApp\Logs"; Permissions: users-modify

[Run]
; Install and start the Windows Service
Filename: "{app}\Service\{#MyServiceName}"; Parameters: "install"; StatusMsg: "Installing FolderLock Service..."; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "start ""FolderLockApp Encryption Service"""; StatusMsg: "Starting FolderLock Service..."; Flags: runhidden waituntilterminated
; Register Shell Extension (if needed)
; Filename: "regsvr32.exe"; Parameters: "/s ""{app}\ShellExtension\FolderLockApp.ShellExtension.dll"""; StatusMsg: "Registering Shell Extension..."; Flags: runhidden waituntilterminated
; Launch application after installation
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Stop and uninstall the Windows Service
Filename: "sc.exe"; Parameters: "stop ""FolderLockApp Encryption Service"""; Flags: runhidden waituntilterminated
Filename: "{app}\Service\{#MyServiceName}"; Parameters: "uninstall"; Flags: runhidden waituntilterminated
; Unregister Shell Extension (if needed)
; Filename: "regsvr32.exe"; Parameters: "/u /s ""{app}\ShellExtension\FolderLockApp.ShellExtension.dll"""; Flags: runhidden waituntilterminated

[Code]
var
  UnlockBeforeUninstallPage: TInputOptionWizardPage;
  HasLockedFolders: Boolean;

// Check if there are any locked folders in the database
function CheckForLockedFolders(): Boolean;
var
  DatabasePath: String;
begin
  Result := False;
  DatabasePath := ExpandConstant('{commonappdata}\FolderLockApp\folderlock.db');
  
  // If database exists, assume there might be locked folders
  if FileExists(DatabasePath) then
  begin
    Result := True;
  end;
end;

// Initialize uninstall wizard
function InitializeUninstall(): Boolean;
begin
  Result := True;
  HasLockedFolders := CheckForLockedFolders();
end;

// Create custom uninstall page
procedure InitializeUninstallProgressForm();
begin
  if HasLockedFolders then
  begin
    if MsgBox('Warning: You have locked folders managed by FolderLock.' + #13#10 + #13#10 +
              'If you uninstall without unlocking them first, you will NOT be able to access the encrypted files!' + #13#10 + #13#10 +
              'Do you want to continue with uninstallation?', mbConfirmation, MB_YESNO) = IDNO then
    begin
      Abort;
    end;
  end;
end;

// Post-installation message
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Create initial configuration if needed
    // This could initialize default settings
  end;
end;

// Custom messages
procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpWelcome then
  begin
    WizardForm.WelcomeLabel2.Caption := 
      'This will install FolderLock on your computer.' + #13#10 + #13#10 +
      'FolderLock provides secure folder encryption with AES-256 encryption.' + #13#10 + #13#10 +
      'The installer will:' + #13#10 +
      '  • Install the FolderLock application' + #13#10 +
      '  • Install and start the background service' + #13#10 +
      '  • Create necessary data directories' + #13#10 +
      '  • Add shortcuts to your Start Menu' + #13#10 + #13#10 +
      'Click Next to continue, or Cancel to exit Setup.';
  end;
end;
