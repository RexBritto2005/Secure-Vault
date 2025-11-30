# FolderLock - Quick Start Guide

Get up and running with FolderLock in 5 minutes!

## Step 1: Build the Application (2 minutes)

Open Command Prompt or PowerShell in the project directory:

```cmd
dotnet build --configuration Release
```

Wait for the build to complete.

## Step 2: Install the Background Service (1 minute)

Open PowerShell **as Administrator**:

```powershell
# Navigate to the service folder
cd FolderLockApp.Service\bin\Release\net8.0

# Install and start the service (replace with your actual path)
$servicePath = (Get-Location).Path + "\FolderLockApp.Service.exe"
sc.exe create "FolderLockApp Encryption Service" binPath=$servicePath
sc.exe config "FolderLockApp Encryption Service" start=auto
sc.exe start "FolderLockApp Encryption Service"
```

Verify it's running:
```powershell
Get-Service "FolderLockApp Encryption Service"
```

You should see: **Status: Running**

## Step 3: Launch the GUI Application (30 seconds)

1. Navigate to: `FolderLockApp.GUI\bin\Release\net8.0-windows\`
2. Double-click `FolderLockApp.GUI.exe`
3. Accept the UAC prompt (click "Yes")
4. If you see a welcome screen, click through it

## Step 4: Lock Your First Folder (1 minute)

### Using the GUI:
1. Click "Add Folder" or "Lock Folder" button
2. Browse to a test folder (create a test folder with some files first!)
3. Enter a password (e.g., "TestPassword123!")
4. Confirm the password
5. Click "Lock"

Watch as your files get encrypted with `.locked` extension!

### Or Use Right-Click (if shell extension is registered):
1. Right-click any folder in Windows Explorer
2. Select "Lock Folder"
3. Enter password
4. Done!

## Step 5: Unlock the Folder (30 seconds)

### Using the GUI:
1. Find the folder in your locked folders list
2. Click "Unlock"
3. Enter the password
4. Click "Unlock"

### Or Use Right-Click:
1. Right-click the locked folder
2. Select "Unlock Folder"
3. Enter password
4. Done!

---

## Optional: Install Shell Extension

For right-click menu integration:

```cmd
# Open Command Prompt as Administrator
cd FolderLockApp.ShellExtension\bin\Release\net8.0-windows
FolderLockApp.ShellExtension.exe register

# Restart Windows Explorer
taskkill /f /im explorer.exe
start explorer.exe
```

Now you can right-click any folder and see "Lock Folder" / "Unlock Folder" options!

---

## What Just Happened?

✅ **Background Service**: Runs in the background, handles encryption/decryption
✅ **GUI Application**: Your control center for managing locked folders
✅ **Shell Extension** (optional): Quick access from Windows Explorer
✅ **Security**: All apps run with admin privileges and verify code integrity

---

## Quick Tips

💡 **Password Safety**: Write down your password! If you forget it, files cannot be recovered.

💡 **Test First**: Try locking/unlocking a test folder before using on important data.

💡 **Backup**: Always backup important files before locking them for the first time.

💡 **Service Status**: If the GUI can't connect, check if the service is running:
```powershell
Get-Service "FolderLockApp Encryption Service"
```

💡 **Logs**: If something goes wrong, check logs at:
```
%ProgramData%\FolderLockApp\Logs\
```

---

## Common First-Time Issues

### "Administrator Privileges Required"
**Fix**: Right-click the .exe and select "Run as administrator"

### "Service Not Running"
**Fix**: 
```powershell
Start-Service "FolderLockApp Encryption Service"
```

### "Code Integrity Verification Failed"
**Fix**: This is normal for debug builds. For production, sign your assemblies.

---

## Next Steps

📖 Read the full **USER_GUIDE.md** for detailed instructions

🔒 Review **SECURITY_FEATURES.md** to understand security features

⚙️ Check **ADMIN_SETUP_GUIDE.md** for advanced configuration

---

## Quick Command Reference

```powershell
# Check service status
Get-Service "FolderLockApp Encryption Service"

# Start service
Start-Service "FolderLockApp Encryption Service"

# Stop service
Stop-Service "FolderLockApp Encryption Service"

# View logs
Get-Content "$env:ProgramData\FolderLockApp\Logs\service-*.log" -Tail 20

# Register shell extension
FolderLockApp.ShellExtension.exe register

# Unregister shell extension
FolderLockApp.ShellExtension.exe unregister
```

---

## You're Ready! 🎉

You now have a fully functional folder encryption system:
- Lock folders with passwords
- Automatic encryption/decryption
- Right-click menu integration (if installed)
- Background service monitoring

**Remember**: Always test with non-critical data first!

For detailed usage, troubleshooting, and advanced features, see **USER_GUIDE.md**.
