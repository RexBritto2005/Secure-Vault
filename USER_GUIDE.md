# FolderLock Application - Complete User Guide

## Table of Contents
1. [Installation](#installation)
2. [First-Time Setup](#first-time-setup)
3. [Using the GUI Application](#using-the-gui-application)
4. [Using Shell Extension (Right-Click Menu)](#using-shell-extension)
5. [Managing the Background Service](#managing-the-background-service)
6. [Common Tasks](#common-tasks)
7. [Troubleshooting](#troubleshooting)

---

## Installation

### Prerequisites
- Windows 10 or Windows 11
- .NET 8.0 Runtime installed
- Administrator privileges

### Step 1: Build the Application
```cmd
# Open Command Prompt in the project directory
dotnet build --configuration Release
```

### Step 2: Locate Built Files
After building, find the executables in:
- GUI: `FolderLockApp.GUI\bin\Release\net8.0-windows\`
- Service: `FolderLockApp.Service\bin\Release\net8.0\`
- Shell Extension: `FolderLockApp.ShellExtension\bin\Release\net8.0-windows\`

### Step 3: Install the Windows Service
```powershell
# Open PowerShell as Administrator
cd path\to\FolderLockApp.Service\bin\Release\net8.0

# Install the service
sc.exe create "FolderLockApp Encryption Service" binPath="C:\full\path\to\FolderLockApp.Service.exe"

# Set to start automatically
sc.exe config "FolderLockApp Encryption Service" start=auto

# Start the service
sc.exe start "FolderLockApp Encryption Service"
```

### Step 4: Register Shell Extension (Optional)
```cmd
# Open Command Prompt as Administrator
cd path\to\FolderLockApp.ShellExtension\bin\Release\net8.0-windows

# Register the extension
FolderLockApp.ShellExtension.exe register

# Restart Windows Explorer
taskkill /f /im explorer.exe
start explorer.exe
```

---

## First-Time Setup

### 1. Launch the GUI Application
- Navigate to the GUI build folder
- Double-click `FolderLockApp.GUI.exe`
- Accept the UAC prompt (required for admin privileges)

### 2. Welcome Screen
On first launch, you'll see:
- Welcome message
- Optional tutorial
- "Don't show again" checkbox

### 3. Service Check
The application will automatically check if the background service is running:
- If running: Continues normally
- If not running: Prompts you to start it

---

## Using the GUI Application

### Main Window Overview
The GUI provides a central interface for managing encrypted folders.

### Locking a Folder

**Method 1: Using the GUI**
1. Launch `FolderLockApp.GUI.exe`
2. Click "Add Folder" or "Lock Folder" button
3. Browse and select the folder you want to lock
4. Enter a strong password (you'll need this to unlock)
5. Confirm the password
6. Click "Lock"

The application will:
- Encrypt all files in the folder
- Add `.locked` extension to files
- Register the folder in the database
- Show the folder in your locked folders list

**Method 2: Using Shell Extension (Right-Click)**
1. Right-click any folder in Windows Explorer
2. Select "Lock Folder" from the context menu
3. Enter password when prompted
4. Folder will be encrypted

### Unlocking a Folder

**Method 1: Using the GUI**
1. Open FolderLockApp.GUI
2. Find the folder in your "Locked Folders" list
3. Click "Unlock" button
4. Enter the password
5. Click "Unlock"

**Method 2: Using Shell Extension**
1. Right-click the locked folder
2. Select "Unlock Folder"
3. Enter the password
4. Folder will be decrypted

### Viewing Locked Folders
The main window displays:
- Folder path
- Lock status
- Date locked
- Auto-lock settings (if enabled)

### Settings and Configuration
Access settings to configure:
- Auto-lock timers
- Password policies
- Notification preferences
- Service connection settings

---

## Using Shell Extension

### What is Shell Extension?
Shell extension adds FolderLock commands directly to Windows Explorer's right-click menu.

### Available Commands

#### Lock Folder
1. Right-click any folder
2. Select "Lock Folder"
3. Enter password (twice for confirmation)
4. Folder encrypts immediately

#### Unlock Folder
1. Right-click a locked folder
2. Select "Unlock Folder"
3. Enter password
4. Folder decrypts immediately

### Benefits
- Quick access without opening GUI
- Integrates with Windows workflow
- Faster for one-off operations

---

## Managing the Background Service

### What Does the Service Do?
The background service handles:
- Automatic folder locking based on timers
- File system monitoring
- Encryption/decryption operations
- IPC communication with GUI

### Checking Service Status

**Method 1: Services Manager**
1. Press `Win + R`
2. Type `services.msc`
3. Find "FolderLockApp Encryption Service"
4. Check status (Running/Stopped)

**Method 2: PowerShell**
```powershell
Get-Service "FolderLockApp Encryption Service"
```

**Method 3: Command Prompt**
```cmd
sc query "FolderLockApp Encryption Service"
```

### Starting the Service
```powershell
# PowerShell (as Administrator)
Start-Service "FolderLockApp Encryption Service"

# OR Command Prompt (as Administrator)
sc.exe start "FolderLockApp Encryption Service"
```

### Stopping the Service
```powershell
# PowerShell (as Administrator)
Stop-Service "FolderLockApp Encryption Service"

# OR Command Prompt (as Administrator)
sc.exe stop "FolderLockApp Encryption Service"
```

### Viewing Service Logs
```powershell
# Navigate to logs folder
cd $env:ProgramData\FolderLockApp\Logs

# View latest service log
Get-Content service-*.log -Tail 50

# View security log
Get-Content security-*.log -Tail 50
```

---

## Common Tasks

### Task 1: Lock Multiple Folders
1. Open FolderLockApp.GUI
2. For each folder:
   - Click "Add Folder"
   - Select folder
   - Enter password
   - Click "Lock"
3. All folders appear in the locked list

### Task 2: Set Up Auto-Lock
1. Lock a folder first
2. Select the folder in the list
3. Click "Settings" or "Auto-Lock"
4. Enable auto-lock
5. Set timer (e.g., lock after 30 minutes of inactivity)
6. Save settings

The background service will automatically lock the folder when the timer expires.

### Task 3: Change Folder Password
1. Unlock the folder first
2. Lock it again with a new password
3. The new password replaces the old one

### Task 4: Remove Folder from Monitoring
1. Unlock the folder
2. Select it in the GUI
3. Click "Remove" or "Unregister"
4. Folder is no longer tracked (but files remain decrypted)

### Task 5: Backup Encrypted Folders
Encrypted folders can be backed up normally:
- Copy the entire folder (with .locked files)
- Store on external drive or cloud
- To restore: Copy back and unlock with password

### Task 6: Check Application Integrity
The application automatically verifies integrity on startup. To manually check:
1. Launch any FolderLock application
2. Check the startup messages
3. Look for "Code integrity verification passed"
4. If failed, reinstall from trusted source

---

## Troubleshooting

### Problem: "Administrator Privileges Required" Error

**Solution:**
1. Right-click the application
2. Select "Run as administrator"
3. Accept UAC prompt

**Or configure permanent elevation:**
1. Right-click the .exe file
2. Properties → Compatibility tab
3. Check "Run this program as an administrator"
4. Click OK

### Problem: "Code Integrity Verification Failed"

**Possible Causes:**
- Unsigned development build
- File corruption
- Tampering

**Solutions:**
1. If development build: This is expected, use DEBUG mode
2. If production: Reinstall from official source
3. Verify file hashes against known good values

### Problem: Service Won't Start

**Check 1: Is it installed?**
```cmd
sc query "FolderLockApp Encryption Service"
```

**Check 2: View error logs**
```powershell
Get-EventLog -LogName Application -Source "FolderLockApp Service" -Newest 10
```

**Check 3: Reinstall service**
```powershell
# Stop and delete
sc.exe stop "FolderLockApp Encryption Service"
sc.exe delete "FolderLockApp Encryption Service"

# Reinstall
sc.exe create "FolderLockApp Encryption Service" binPath="C:\path\to\FolderLockApp.Service.exe"
sc.exe start "FolderLockApp Encryption Service"
```

### Problem: Can't Unlock Folder - Wrong Password

**Solutions:**
1. Try password again (check Caps Lock)
2. If forgotten: Password cannot be recovered (encryption security)
3. Restore from backup if available

**Prevention:**
- Use a password manager
- Write down passwords securely
- Test unlock immediately after locking

### Problem: Shell Extension Not Appearing

**Solution 1: Re-register**
```cmd
# As Administrator
cd path\to\FolderLockApp.ShellExtension
FolderLockApp.ShellExtension.exe unregister
FolderLockApp.ShellExtension.exe register
```

**Solution 2: Restart Explorer**
```cmd
taskkill /f /im explorer.exe
start explorer.exe
```

**Solution 3: Check Registry**
1. Press `Win + R`
2. Type `regedit`
3. Navigate to: `HKEY_CLASSES_ROOT\Directory\shellex\ContextMenuHandlers`
4. Look for FolderLock entry

### Problem: GUI Can't Connect to Service

**Check 1: Is service running?**
```cmd
sc query "FolderLockApp Encryption Service"
```

**Check 2: Start the service**
```cmd
sc.exe start "FolderLockApp Encryption Service"
```

**Check 3: Check firewall**
- Ensure Windows Firewall isn't blocking IPC communication
- Add exception if needed

### Problem: Slow Encryption/Decryption

**Normal for:**
- Large folders (many GB)
- Many small files
- Slow storage (HDD vs SSD)

**Tips:**
- Close other applications
- Ensure adequate free disk space
- Check antivirus isn't scanning during operation
- Monitor progress in GUI

### Problem: Database Errors

**Location:** `%ProgramData%\FolderLockApp\folderlock.db`

**Solution 1: Backup and reset**
```powershell
# Backup current database
Copy-Item "$env:ProgramData\FolderLockApp\folderlock.db" "$env:ProgramData\FolderLockApp\folderlock.db.backup"

# Delete database (will be recreated)
Remove-Item "$env:ProgramData\FolderLockApp\folderlock.db"

# Restart application
```

**Solution 2: Restore from backup**
```powershell
Copy-Item "$env:ProgramData\FolderLockApp\folderlock.db.backup" "$env:ProgramData\FolderLockApp\folderlock.db"
```

---

## Best Practices

### Security
1. **Use Strong Passwords**
   - Minimum 12 characters
   - Mix of letters, numbers, symbols
   - Unique per folder

2. **Regular Backups**
   - Backup encrypted folders
   - Store passwords securely
   - Test restore procedures

3. **Keep Software Updated**
   - Install updates promptly
   - Check for security patches
   - Verify signatures after updates

### Performance
1. **Folder Size**
   - Lock smaller folders for faster operations
   - Consider splitting very large folders

2. **Storage**
   - Use SSD for better performance
   - Ensure adequate free space (2x folder size)

3. **Monitoring**
   - Check service logs regularly
   - Monitor for errors
   - Review security logs

### Maintenance
1. **Weekly**
   - Check service status
   - Review locked folders list
   - Test unlock on critical folders

2. **Monthly**
   - Review logs for errors
   - Backup database
   - Update passwords if needed

3. **Quarterly**
   - Full backup of encrypted folders
   - Test restore procedures
   - Review security settings

---

## Advanced Usage

### Command Line Operations
While the GUI is recommended, you can interact with the service programmatically using the IPC interface.

### Batch Operations
For locking multiple folders:
1. Create a list of folders
2. Use GUI's batch import feature (if available)
3. Or script using the service API

### Integration with Other Tools
- Backup software: Include encrypted folders
- Cloud sync: Sync encrypted folders safely
- Monitoring: Track service status

---

## Getting Help

### Log Files
- Service logs: `%ProgramData%\FolderLockApp\Logs\service-*.log`
- Security logs: `%ProgramData%\FolderLockApp\Logs\security-*.log`
- Windows Event Log: Application log, source "FolderLockApp Service"

### Debug Information
When reporting issues, include:
- Windows version
- .NET version
- Error messages
- Log file excerpts
- Steps to reproduce

### Documentation
- `SECURITY_FEATURES.md` - Security details
- `ADMIN_SETUP_GUIDE.md` - Installation guide
- `IMPLEMENTATION_SUMMARY.md` - Technical details

---

## Uninstallation

### Step 1: Unlock All Folders
**Important:** Unlock all folders before uninstalling!
1. Open FolderLockApp.GUI
2. Unlock each folder in the list
3. Verify files are accessible

### Step 2: Stop and Remove Service
```powershell
# As Administrator
sc.exe stop "FolderLockApp Encryption Service"
sc.exe delete "FolderLockApp Encryption Service"
```

### Step 3: Unregister Shell Extension
```cmd
# As Administrator
cd path\to\FolderLockApp.ShellExtension
FolderLockApp.ShellExtension.exe unregister
```

### Step 4: Delete Application Files
1. Delete application folders
2. Delete data folder: `%ProgramData%\FolderLockApp`
3. (Optional) Backup database first if needed

### Step 5: Clean Registry (Optional)
Shell extension entries are removed during unregistration, but you can verify:
- `HKEY_CLASSES_ROOT\Directory\shellex\ContextMenuHandlers`

---

## Quick Reference

### Essential Commands
```powershell
# Check service status
Get-Service "FolderLockApp Encryption Service"

# Start service
Start-Service "FolderLockApp Encryption Service"

# View logs
Get-Content "$env:ProgramData\FolderLockApp\Logs\service-*.log" -Tail 50

# Register shell extension
FolderLockApp.ShellExtension.exe register

# Unregister shell extension
FolderLockApp.ShellExtension.exe unregister
```

### File Locations
- Database: `%ProgramData%\FolderLockApp\folderlock.db`
- Logs: `%ProgramData%\FolderLockApp\Logs\`
- GUI: `FolderLockApp.GUI\bin\Release\net8.0-windows\`
- Service: `FolderLockApp.Service\bin\Release\net8.0\`

### Keyboard Shortcuts (GUI)
- `Ctrl + L` - Lock selected folder
- `Ctrl + U` - Unlock selected folder
- `Ctrl + A` - Add new folder
- `F5` - Refresh folder list
- `Ctrl + S` - Open settings

---

## Safety Reminders

⚠️ **Important Warnings:**

1. **Password Recovery**: If you forget your password, files CANNOT be recovered. This is by design for security.

2. **Backup Before Locking**: Always backup important data before locking for the first time.

3. **Test Unlock**: After locking, immediately test unlocking to verify password works.

4. **Service Dependency**: Keep the background service running for auto-lock features.

5. **Admin Rights**: Always run with administrator privileges for proper operation.

6. **File Modifications**: Don't manually modify .locked files - always use the application.

---

## Support

For issues, questions, or feature requests:
1. Check this user guide
2. Review log files
3. Check documentation files
4. Report issues with detailed information

**Remember:** FolderLock is designed to protect your data. Take time to understand how it works before encrypting critical files.
