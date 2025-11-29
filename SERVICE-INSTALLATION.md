# FolderLockApp Service Installation Guide

This guide explains how to install and configure the FolderLock background service to start automatically with Windows.

## Prerequisites

- Windows 10/11 or Windows Server
- Administrator privileges
- .NET 8 Runtime installed
- FolderLockApp built in Release configuration

## Installation Steps

### 1. Build the Application

First, build the application in Release mode:

```powershell
dotnet build --configuration Release
```

### 2. Install the Service

Run PowerShell **as Administrator** and execute:

```powershell
.\Install-Service.ps1
```

This will:
- Create a Windows Service named "FolderLockAppService"
- Configure it to start automatically on boot
- Set up automatic restart on failure
- Start the service immediately

### 3. Verify Installation

Check the service status:

```powershell
.\Manage-Service.ps1 -Action Status
```

Or use Windows Services Manager:
1. Press `Win + R`
2. Type `services.msc`
3. Look for "FolderLock Encryption Service"

## Service Management

### Start the Service

```powershell
.\Manage-Service.ps1 -Action Start
```

### Stop the Service

```powershell
.\Manage-Service.ps1 -Action Stop
```

### Restart the Service

```powershell
.\Manage-Service.ps1 -Action Restart
```

### Check Service Status

```powershell
.\Manage-Service.ps1 -Action Status
```

## Uninstallation

To remove the service:

```powershell
.\Install-Service.ps1 -Uninstall
```

## Service Configuration

### Service Details

- **Name:** FolderLockAppService
- **Display Name:** FolderLock Encryption Service
- **Startup Type:** Automatic
- **Recovery:** Restart on failure (3 attempts)
- **Account:** Local System

### Service Behavior

The service will:
- Start automatically when Windows boots
- Monitor all registered locked folders
- Handle lock/unlock requests from the GUI
- Log all operations to `%ProgramData%\FolderLockApp\Logs\`
- Restart automatically if it crashes

### Logs Location

Service logs are stored in:
```
C:\ProgramData\FolderLockApp\Logs\service-YYYYMMDD.log
```

## Troubleshooting

### Service Won't Start

1. Check the Windows Event Log:
   - Open Event Viewer (`eventvwr.msc`)
   - Navigate to Windows Logs > Application
   - Look for errors from "FolderLockApp Service"

2. Check service logs:
   ```
   C:\ProgramData\FolderLockApp\Logs\
   ```

3. Verify .NET 8 Runtime is installed:
   ```powershell
   dotnet --list-runtimes
   ```

### Permission Issues

The service runs as Local System and should have sufficient permissions. If you encounter permission issues:

1. Check folder permissions
2. Ensure the database file is accessible
3. Check Windows Event Log for details

### Service Crashes

The service is configured to restart automatically on failure. Check logs to identify the cause:

```powershell
Get-Content "C:\ProgramData\FolderLockApp\Logs\service-*.log" -Tail 50
```

## Manual Service Commands

You can also manage the service using Windows commands:

### Using sc.exe

```cmd
# Start service
sc start FolderLockAppService

# Stop service
sc stop FolderLockAppService

# Query service status
sc query FolderLockAppService

# Delete service
sc delete FolderLockAppService
```

### Using PowerShell

```powershell
# Start service
Start-Service -Name FolderLockAppService

# Stop service
Stop-Service -Name FolderLockAppService

# Get service status
Get-Service -Name FolderLockAppService

# Restart service
Restart-Service -Name FolderLockAppService
```

## Security Considerations

- The service runs with Local System privileges
- All encryption operations are logged
- Password hashes are stored securely in SQLite
- Service communicates via Named Pipes (local only)

## Next Steps

After installing the service:

1. Run the FolderLockApp GUI
2. Lock a test folder
3. Verify the service is monitoring it
4. Reboot your computer to test auto-start
5. Check that locked folders are still monitored after reboot

## Support

For issues or questions:
- Check the logs in `C:\ProgramData\FolderLockApp\Logs\`
- Review Windows Event Log
- Ensure all prerequisites are met
