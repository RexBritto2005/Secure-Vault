# FolderLock - Complete Setup & Usage Guide

Everything you need to build, install, and use FolderLock in one place.

## 🎯 What You Have

You now have **THREE ways** to run FolderLock:

1. **Separate Components** (Original) - GUI + Service + Shell Extension
2. **Unified Launcher** (Recommended) - One launcher that starts everything
3. **All-in-One** (Simplest) - Everything in a single executable

---

## 🚀 Quick Start (Choose One Method)

### Method 1: Unified Launcher (Recommended)

**Step 1: Build**
```cmd
dotnet build --configuration Release
```

**Step 2: Run the Launcher**
```cmd
cd FolderLockApp.Launcher\bin\Release\net8.0-windows
FolderLockApp.Launcher.exe
```

The launcher will:
- ✅ Check admin privileges (auto-elevate if needed)
- ✅ Verify code integrity
- ✅ Install the background service
- ✅ Start the service
- ✅ Launch the GUI

**That's it!** Everything is automated.

### Method 2: All-in-One Executable

**Step 1: Build**
```cmd
dotnet publish FolderLockApp.AllInOne\FolderLockApp.AllInOne.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output .\Output\AllInOne ^
    /p:PublishSingleFile=true
```

**Step 2: Run**
```cmd
Output\AllInOne\FolderLockApp.AllInOne.exe
```

**Benefits:**
- Single file (~90 MB)
- No service installation needed
- Portable
- Self-contained

### Method 3: Automated Build Script

**Step 1: Run the build script**
```cmd
build-standalone.bat
```

**Step 2: Navigate to output**
```cmd
cd Standalone-Release\FolderLock-Portable
```

**Step 3: Install**
```cmd
Install.bat
```

**Step 4: Run**
```cmd
FolderLock.exe
```

---

## 📦 What Gets Built

### Core Components
- **FolderLockApp.Core.dll** - Shared library (encryption, database)
- **FolderLockApp.GUI.exe** - Desktop application
- **FolderLockApp.Service.exe** - Background Windows service
- **FolderLockApp.ShellExtension.exe** - Right-click menu

### New Components
- **FolderLockApp.Launcher.exe** - Unified launcher (manages everything)
- **FolderLockApp.AllInOne.exe** - Everything in one file

---

## 🔧 Detailed Setup

### Option A: Development Setup

For development and testing:

```cmd
# 1. Build all projects
dotnet build --configuration Release

# 2. Install service manually
cd FolderLockApp.Service\bin\Release\net8.0
$path = (Get-Location).Path + "\FolderLockApp.Service.exe"
sc.exe create "FolderLockApp Encryption Service" binPath=$path
sc.exe start "FolderLockApp Encryption Service"

# 3. Run GUI
cd ..\..\..\FolderLockApp.GUI\bin\Release\net8.0-windows
.\FolderLockApp.GUI.exe
```

### Option B: Standalone Deployment

For distribution to end users:

```cmd
# 1. Run build script
build-standalone.bat

# 2. Package is created in:
Standalone-Release\FolderLock-Portable\

# 3. Distribute the entire folder
# Users run: Install.bat then FolderLock.exe
```

### Option C: Single-File Deployment

For simplest distribution:

```cmd
# 1. Build all-in-one
dotnet publish FolderLockApp.AllInOne\FolderLockApp.AllInOne.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    /p:PublishSingleFile=true

# 2. Distribute single file:
Output\AllInOne\FolderLockApp.AllInOne.exe
```

---

## 💻 Using FolderLock

### First Time Setup

1. **Launch the application** (any method above)
2. **Accept UAC prompt** (admin privileges required)
3. **Wait for service check** (automatic)
4. **Main window opens**

### Locking a Folder

**Method 1: Using GUI**
1. Click "Lock Folder" button
2. Browse to folder
3. Enter password (twice)
4. Click "Lock"
5. Wait for encryption to complete

**Method 2: Using Right-Click** (if shell extension installed)
1. Right-click folder in Windows Explorer
2. Select "Lock Folder"
3. Enter password
4. Done!

### Unlocking a Folder

**Method 1: Using GUI**
1. Select folder from list
2. Click "Unlock"
3. Enter password
4. Wait for decryption

**Method 2: Using Right-Click**
1. Right-click locked folder
2. Select "Unlock Folder"
3. Enter password
4. Done!

---

## 🛠️ Advanced Configuration

### Installing Shell Extension

```cmd
# As Administrator
cd FolderLockApp.ShellExtension\bin\Release\net8.0-windows
FolderLockApp.ShellExtension.exe register

# Restart Explorer
taskkill /f /im explorer.exe
start explorer.exe
```

### Service Management

**Check Status:**
```powershell
Get-Service "FolderLockApp Encryption Service"
```

**Start Service:**
```powershell
Start-Service "FolderLockApp Encryption Service"
```

**Stop Service:**
```powershell
Stop-Service "FolderLockApp Encryption Service"
```

**View Logs:**
```powershell
Get-Content "$env:ProgramData\FolderLockApp\Logs\service-*.log" -Tail 50
```

### Database Location

```
%ProgramData%\FolderLockApp\folderlock.db
```

To view:
```cmd
explorer %ProgramData%\FolderLockApp
```

---

## 📊 Comparison of Methods

| Method | File Size | Setup Complexity | Service Install | Best For |
|--------|-----------|------------------|-----------------|----------|
| **Launcher** | ~70 MB | Low | Automatic | Most users |
| **All-in-One** | ~90 MB | Very Low | Not needed | Portable use |
| **Separate** | ~5-10 MB each | Medium | Manual | Development |
| **Standalone Package** | ~250 MB total | Low | Semi-automatic | Distribution |

---

## 🔒 Security Features

All methods include:
- ✅ **AES-256 Encryption** - Military-grade
- ✅ **Admin Privileges** - Automatic UAC prompts
- ✅ **Code Integrity** - Verifies files aren't tampered
- ✅ **Secure Logging** - Audit trail
- ✅ **No Password Storage** - Only hashes stored

---

## 🐛 Troubleshooting

### "Administrator Privileges Required"
**Fix:** Right-click and "Run as administrator"

### "Service Not Running"
**Fix:**
```powershell
Start-Service "FolderLockApp Encryption Service"
```

### "Code Integrity Failed"
**Fix:** Normal for debug builds. For production, sign assemblies.

### Launcher Won't Start Service
**Fix:** Install service manually:
```powershell
$path = "C:\path\to\FolderLockApp.Service.exe"
sc.exe create "FolderLockApp Encryption Service" binPath=$path
sc.exe start "FolderLockApp Encryption Service"
```

### All-in-One Crashes
**Fix:** Check logs at `%ProgramData%\FolderLockApp\Logs\`

### Can't Unlock - Wrong Password
**Fix:** Password cannot be recovered. Restore from backup.

---

## 📁 File Structure

```
FolderLockApp/
├── FolderLockApp.Core/              # Shared library
├── FolderLockApp.GUI/               # Desktop application
├── FolderLockApp.Service/           # Background service
├── FolderLockApp.ShellExtension/    # Right-click menu
├── FolderLockApp.Launcher/          # Unified launcher ⭐ NEW
├── FolderLockApp.AllInOne/          # All-in-one app ⭐ NEW
├── build-standalone.ps1             # Build script ⭐ NEW
├── build-standalone.bat             # Build script wrapper ⭐ NEW
└── Documentation/
    ├── README.md
    ├── GET_STARTED.md
    ├── QUICK_START.md
    ├── USER_GUIDE.md
    ├── BUILD_GUIDE.md               # ⭐ NEW
    └── COMPLETE_SETUP_GUIDE.md      # ⭐ NEW (this file)
```

---

## 📚 Documentation Index

- **README.md** - Project overview
- **GET_STARTED.md** - Simple step-by-step guide
- **QUICK_START.md** - 5-minute setup
- **USER_GUIDE.md** - Complete usage documentation
- **BUILD_GUIDE.md** - Building and deployment
- **HOW_IT_WORKS.md** - Technical architecture
- **COMPLETE_SETUP_GUIDE.md** - This file (everything in one place)

---

## ⚡ Quick Commands Reference

### Build Everything
```cmd
dotnet build --configuration Release
```

### Build Standalone Package
```cmd
build-standalone.bat
```

### Build All-in-One
```cmd
dotnet publish FolderLockApp.AllInOne\FolderLockApp.AllInOne.csproj ^
    --configuration Release --runtime win-x64 --self-contained true ^
    /p:PublishSingleFile=true
```

### Run Launcher
```cmd
FolderLockApp.Launcher\bin\Release\net8.0-windows\FolderLockApp.Launcher.exe
```

### Install Service
```powershell
$path = "C:\path\to\FolderLockApp.Service.exe"
sc.exe create "FolderLockApp Encryption Service" binPath=$path start=auto
sc.exe start "FolderLockApp Encryption Service"
```

### Register Shell Extension
```cmd
FolderLockApp.ShellExtension.exe register
```

### View Logs
```powershell
Get-Content "$env:ProgramData\FolderLockApp\Logs\service-*.log" -Tail 50
```

---

## ✅ Recommended Workflow

### For End Users:
1. Download the standalone package
2. Run `Install.bat` as Administrator
3. Run `FolderLock.exe`
4. Start locking folders!

### For Developers:
1. Clone the repository
2. Run `dotnet build`
3. Use the Launcher for testing
4. Build standalone package for distribution

### For Portable Use:
1. Build the All-in-One executable
2. Copy single file to USB drive
3. Run on any Windows PC
4. No installation needed!

---

## 🎯 Next Steps

1. **Choose your method** (Launcher, All-in-One, or Separate)
2. **Build the application**
3. **Test with dummy files first**
4. **Read the security warnings**
5. **Start protecting your folders!**

---

## ⚠️ Important Warnings

🚨 **NEVER FORGET YOUR PASSWORD** - Files cannot be recovered without it

🚨 **BACKUP FIRST** - Always backup important files before locking

🚨 **TEST UNLOCK** - After locking, immediately test unlocking

🚨 **KEEP SERVICE RUNNING** - Required for auto-lock features

🚨 **ADMIN RIGHTS** - Always run with administrator privileges

---

## 🆘 Getting Help

1. Check the documentation files
2. Review log files in `%ProgramData%\FolderLockApp\Logs\`
3. Search for error messages
4. Check Windows Event Log

---

## 🎉 You're Ready!

You now have everything you need to:
- ✅ Build FolderLock in multiple configurations
- ✅ Install and configure all components
- ✅ Use the application to protect your folders
- ✅ Troubleshoot common issues
- ✅ Distribute to end users

**Choose your preferred method and get started!**

For the simplest experience, we recommend:
1. Run `build-standalone.bat`
2. Navigate to `Standalone-Release\FolderLock-Portable\`
3. Run `Install.bat`
4. Run `FolderLock.exe`

**Happy encrypting! 🔒**
