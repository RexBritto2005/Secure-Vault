# FolderLockApp Windows Service

This is the background service component of FolderLockApp that runs as a Windows Service to monitor and manage encrypted folders.

## Features

- Runs automatically on system startup
- Loads all locked folders from the registry on startup
- Monitors locked folder locations for access attempts
- Runs as a background process without displaying a window
- Comprehensive logging to file and Windows Event Log

## Installation

### Install as Windows Service

To install the service, run the following command in an elevated (Administrator) PowerShell:

```powershell
sc.exe create "FolderLockApp Service" binPath="C:\Path\To\FolderLockApp.Service.exe" start=auto
```

Or use the `New-Service` cmdlet:

```powershell
New-Service -Name "FolderLockApp Service" `
    -BinaryPathName "C:\Path\To\FolderLockApp.Service.exe" `
    -DisplayName "Folder Lock Encryption Service" `
    -Description "Monitors and manages encrypted folders with password protection" `
    -StartupType Automatic
```

### Start the Service

```powershell
Start-Service "FolderLockApp Service"
```

### Check Service Status

```powershell
Get-Service "FolderLockApp Service"
```

### Stop the Service

```powershell
Stop-Service "FolderLockApp Service"
```

### Uninstall the Service

```powershell
Stop-Service "FolderLockApp Service"
sc.exe delete "FolderLockApp Service"
```

## Running for Development

For development and testing, you can run the service as a console application:

```powershell
dotnet run --project FolderLockApp.Service
```

## Logging

The service logs to two locations:

1. **File Logs**: `%ProgramData%\FolderLockApp\Logs\service-YYYYMMDD.log`
   - Rolling daily logs
   - Retained for 30 days
   - Contains detailed information, warnings, and errors

2. **Windows Event Log**: Application log with source "FolderLockApp Service"
   - Only warnings and errors
   - Viewable in Windows Event Viewer

## Database

The service uses SQLite database located at:
`%ProgramData%\FolderLockApp\folderlock.db`

The database is automatically created on first run.

## Requirements

- .NET 8.0 Runtime
- Windows operating system
- Administrator privileges for service installation

## Configuration

Service configuration is stored in `appsettings.json`. The service automatically creates necessary directories in `%ProgramData%\FolderLockApp\`.
