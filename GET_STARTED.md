# How to Use FolderLock - Simple Guide

## What You Have

Your FolderLock application has **4 main components**:

1. **GUI Application** - The main program you interact with
2. **Background Service** - Runs in the background, handles encryption
3. **Shell Extension** - Adds right-click menu to Windows Explorer
4. **Core Library** - Shared code used by all components

## Step-by-Step: Using the Complete Application

### STEP 1: Build Everything (One Time)

Open Command Prompt in your project folder:

```cmd
dotnet build --configuration Release
```

Wait for it to finish. You should see "Build succeeded."

### STEP 2: Install the Background Service (One Time)

Open **PowerShell as Administrator**:

```powershell
# Go to the service folder
cd FolderLockApp.Service\bin\Release\net8.0

# Install the service
$path = (Get-Location).Path + "\FolderLockApp.Service.exe"
sc.exe create "FolderLockApp Encryption Service" binPath=$path

# Start it
sc.exe start "FolderLockApp Encryption Service"

# Make it start automatically with Windows
sc.exe config "FolderLockApp Encryption Service" start=auto
```

Verify it's running:
```powershell
Get-Service "FolderLockApp Encryption Service"
```

You should see: **Status: Running** ✅

### STEP 3: Use the GUI Application (Daily Use)

1. Navigate to: `FolderLockApp.GUI\bin\Release\net8.0-windows\`
2. Double-click `FolderLockApp.GUI.exe`
3. Click "Yes" when Windows asks for admin permission
4. The application opens!

### STEP 4: Lock Your First Folder

**Create a test folder first:**
1. Create a folder on your desktop called "TestFolder"
2. Put some files in it (documents, images, etc.)

**Now lock it:**
1. In FolderLockApp.GUI, click "Lock Folder" or "Add Folder"
2. Browse to your TestFolder
3. Enter a password (e.g., "MyPassword123!")
4. Confirm the password
5. Click "Lock"

**What happens:**
- All files get encrypted
- Files get `.locked` extension
- Folder appears in your "Locked Folders" list

### STEP 5: Unlock the Folder

1. In the locked folders list, select your TestFolder
2. Click "Unlock"
3. Enter your password
4. Click "Unlock"

**What happens:**
- Files get decrypted
- `.locked` extension removed
- Files are normal again

### STEP 6 (Optional): Install Right-Click Menu

Open **Command Prompt as Administrator**:

```cmd
cd FolderLockApp.ShellExtension\bin\Release\net8.0-windows
FolderLockApp.ShellExtension.exe register
```

Restart Windows Explorer:
```cmd
taskkill /f /im explorer.exe
start explorer.exe
```

**Now you can:**
- Right-click any folder
- See "Lock Folder" option
- Right-click locked folders
- See "Unlock Folder" option

## Daily Usage

### To Lock a Folder:
**Option A - Using GUI:**
1. Open FolderLockApp.GUI.exe
2. Click "Lock Folder"
3. Select folder, enter password
4. Done!

**Option B - Using Right-Click:**
1. Right-click folder in Windows Explorer
2. Click "Lock Folder"
3. Enter password
4. Done!

### To Unlock a Folder:
**Option A - Using GUI:**
1. Open FolderLockApp.GUI.exe
2. Select folder from list
3. Click "Unlock"
4. Enter password
5. Done!

**Option B - Using Right-Click:**
1. Right-click locked folder
2. Click "Unlock Folder"
3. Enter password
4. Done!

## Important Things to Know

### ⚠️ Critical Warnings

1. **NEVER FORGET YOUR PASSWORD**
   - If you forget it, files CANNOT be recovered
   - This is by design for security
   - Write it down somewhere safe!

2. **BACKUP BEFORE FIRST USE**
   - Always backup important files before locking
   - Test with non-critical files first

3. **TEST UNLOCK IMMEDIATELY**
   - After locking, unlock right away to verify password works
   - Don't wait until you need the files!

4. **KEEP SERVICE RUNNING**
   - The background service must be running
   - Check it's running if you have issues

### ✅ Good Practices

1. **Use Strong Passwords**
   - At least 12 characters
   - Mix letters, numbers, symbols
   - Example: "MyFolder2024!Secure"

2. **Test First**
   - Create a test folder with dummy files
   - Practice locking and unlocking
   - Get comfortable before using on real data

3. **Regular Backups**
   - Backup your encrypted folders
   - You can copy locked folders normally
   - Store backups on external drive or cloud

## Troubleshooting

### Problem: "Administrator Privileges Required"
**Fix:** Right-click the .exe and select "Run as administrator"

### Problem: "Service Not Running"
**Fix:**
```powershell
Start-Service "FolderLockApp Encryption Service"
```

### Problem: GUI Won't Open
**Fix:**
1. Check if .NET 8.0 Runtime is installed
2. Try running from command line to see errors:
   ```cmd
   cd FolderLockApp.GUI\bin\Release\net8.0-windows
   .\FolderLockApp.GUI.exe
   ```

### Problem: Can't Unlock - Wrong Password
**Fix:**
- Try again (check Caps Lock!)
- If truly forgotten, files cannot be recovered
- Restore from backup if available

### Problem: Right-Click Menu Not Showing
**Fix:**
```cmd
# As Administrator
cd FolderLockApp.ShellExtension\bin\Release\net8.0-windows
FolderLockApp.ShellExtension.exe unregister
FolderLockApp.ShellExtension.exe register
taskkill /f /im explorer.exe
start explorer.exe
```

## Quick Commands Reference

### Check Service Status
```powershell
Get-Service "FolderLockApp Encryption Service"
```

### Start Service
```powershell
Start-Service "FolderLockApp Encryption Service"
```

### Stop Service
```powershell
Stop-Service "FolderLockApp Encryption Service"
```

### View Logs
```powershell
Get-Content "$env:ProgramData\FolderLockApp\Logs\service-*.log" -Tail 20
```

### Register Shell Extension
```cmd
FolderLockApp.ShellExtension.exe register
```

### Unregister Shell Extension
```cmd
FolderLockApp.ShellExtension.exe unregister
```

## File Locations

### Application Files
- **GUI**: `FolderLockApp.GUI\bin\Release\net8.0-windows\FolderLockApp.GUI.exe`
- **Service**: `FolderLockApp.Service\bin\Release\net8.0\FolderLockApp.Service.exe`
- **Shell Extension**: `FolderLockApp.ShellExtension\bin\Release\net8.0-windows\FolderLockApp.ShellExtension.exe`

### Data Files
- **Database**: `%ProgramData%\FolderLockApp\folderlock.db`
- **Logs**: `%ProgramData%\FolderLockApp\Logs\`

To open data folder:
```cmd
explorer %ProgramData%\FolderLockApp
```

## What Each Component Does

### GUI Application (FolderLockApp.GUI.exe)
- Main interface you use
- Shows list of locked folders
- Lock/unlock buttons
- Settings and configuration
- **When to use:** Daily operations, managing multiple folders

### Background Service (FolderLockApp Encryption Service)
- Runs in background
- Handles actual encryption/decryption
- Monitors folders for auto-lock
- Communicates with GUI
- **When to use:** Always running (automatic)

### Shell Extension (Right-Click Menu)
- Adds menu items to Windows Explorer
- Quick lock/unlock without opening GUI
- **When to use:** Quick operations on single folders

## Security Features

Your application includes:

✅ **AES-256 Encryption** - Military-grade security
✅ **Password Protection** - Only you can unlock
✅ **Admin Privileges** - Automatic UAC prompts
✅ **Code Integrity** - Verifies files aren't tampered with
✅ **Secure Logging** - Tracks all operations
✅ **No Password Storage** - Passwords never saved

## Next Steps

1. **Read the full guides:**
   - `README.md` - Overview and features
   - `QUICK_START.md` - 5-minute setup
   - `USER_GUIDE.md` - Complete documentation
   - `HOW_IT_WORKS.md` - Technical details

2. **Practice:**
   - Create test folders
   - Lock and unlock them
   - Try the right-click menu
   - Check the logs

3. **Use it:**
   - Start with non-critical files
   - Lock folders you want to protect
   - Remember your passwords!
   - Backup regularly

## Need Help?

1. Check the documentation files
2. Look at the log files
3. Try the troubleshooting section
4. Search for error messages online

## Summary

**To use FolderLock:**
1. Build it once: `dotnet build --configuration Release`
2. Install service once: `sc.exe create ...` (see Step 2)
3. Run GUI: Double-click `FolderLockApp.GUI.exe`
4. Lock folders: Click "Lock Folder", select folder, enter password
5. Unlock folders: Select from list, click "Unlock", enter password

**Remember:**
- Always backup first
- Never forget passwords
- Test unlock immediately
- Keep service running

**You're ready to secure your folders!** 🔒
