# FolderLock - Build Guide

Complete guide for building standalone executables and deployment packages.

## Table of Contents
1. [Quick Build](#quick-build)
2. [Standalone Executable](#standalone-executable)
3. [All-in-One Application](#all-in-one-application)
4. [Manual Build Steps](#manual-build-steps)
5. [Deployment](#deployment)

---

## Quick Build

### Option 1: Automated Build Script (Recommended)

**Windows (Double-click):**
```
build-standalone.bat
```

**PowerShell:**
```powershell
.\build-standalone.ps1
```

This creates a complete portable package in `Standalone-Release\FolderLock-Portable\`

### Option 2: Simple Build

```cmd
dotnet build --configuration Release
```

Builds all projects to their respective `bin\Release` folders.

---

## Standalone Executable

### What is it?
A single `.exe` file that includes:
- .NET 8.0 runtime (no installation needed)
- All dependencies
- Self-extracting

### Build Commands

#### GUI Application (Standalone)
```cmd
dotnet publish FolderLockApp.GUI\FolderLockApp.GUI.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output .\Output\GUI ^
    /p:PublishSingleFile=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true ^
    /p:EnableCompressionInSingleFile=true
```

Output: `Output\GUI\FolderLockApp.GUI.exe` (~70-90 MB)

#### Background Service (Standalone)
```cmd
dotnet publish FolderLockApp.Service\FolderLockApp.Service.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output .\Output\Service ^
    /p:PublishSingleFile=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true ^
    /p:EnableCompressionInSingleFile=true
```

Output: `Output\Service\FolderLockApp.Service.exe` (~60-80 MB)

#### Shell Extension (Standalone)
```cmd
dotnet publish FolderLockApp.ShellExtension\FolderLockApp.ShellExtension.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output .\Output\ShellExtension ^
    /p:PublishSingleFile=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true ^
    /p:EnableCompressionInSingleFile=true
```

Output: `Output\ShellExtension\FolderLockApp.ShellExtension.exe` (~60-80 MB)

#### Unified Launcher (Standalone)
```cmd
dotnet publish FolderLockApp.Launcher\FolderLockApp.Launcher.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output .\Output\Launcher ^
    /p:PublishSingleFile=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true ^
    /p:EnableCompressionInSingleFile=true
```

Output: `Output\Launcher\FolderLockApp.Launcher.exe` (~70-90 MB)

---

## All-in-One Application

### What is it?
A **single executable** that combines:
- GUI interface
- Background service
- All functionality

No separate service installation needed!

### Build Command

```cmd
dotnet publish FolderLockApp.AllInOne\FolderLockApp.AllInOne.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output .\Output\AllInOne ^
    /p:PublishSingleFile=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true ^
    /p:EnableCompressionInSingleFile=true
```

Output: `Output\AllInOne\FolderLockApp.AllInOne.exe` (~80-100 MB)

### Usage
Just run the single `.exe` file:
```cmd
FolderLockApp.AllInOne.exe
```

No installation, no service setup - everything in one file!

---

## Manual Build Steps

### Step 1: Build Core Library
```cmd
dotnet build FolderLockApp.Core\FolderLockApp.Core.csproj --configuration Release
```

### Step 2: Build GUI
```cmd
dotnet build FolderLockApp.GUI\FolderLockApp.GUI.csproj --configuration Release
```

### Step 3: Build Service
```cmd
dotnet build FolderLockApp.Service\FolderLockApp.Service.csproj --configuration Release
```

### Step 4: Build Shell Extension
```cmd
dotnet build FolderLockApp.ShellExtension\FolderLockApp.ShellExtension.csproj --configuration Release
```

### Step 5: Build Launcher
```cmd
dotnet build FolderLockApp.Launcher\FolderLockApp.Launcher.csproj --configuration Release
```

### Output Locations
- Core: `FolderLockApp.Core\bin\Release\net8.0\`
- GUI: `FolderLockApp.GUI\bin\Release\net8.0-windows\`
- Service: `FolderLockApp.Service\bin\Release\net8.0\`
- Shell Extension: `FolderLockApp.ShellExtension\bin\Release\net8.0-windows\`
- Launcher: `FolderLockApp.Launcher\bin\Release\net8.0-windows\`

---

## Deployment

### Option 1: Portable Package (Recommended)

Run the build script:
```cmd
build-standalone.bat
```

This creates: `Standalone-Release\FolderLock-Portable\`

**Contents:**
- `FolderLock.exe` - Main launcher
- `FolderLockApp.GUI.exe` - GUI application
- `FolderLockApp.Service.exe` - Background service
- `FolderLockApp.ShellExtension.exe` - Shell extension
- `Install.bat` - Installation script
- `Uninstall.bat` - Uninstallation script
- `README.txt` - Quick start guide
- Documentation files

**To Deploy:**
1. Copy the entire `FolderLock-Portable` folder
2. Run `Install.bat` as Administrator
3. Run `FolderLock.exe`

### Option 2: All-in-One Executable

Build the all-in-one version:
```cmd
dotnet publish FolderLockApp.AllInOne\FolderLockApp.AllInOne.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output .\Output\AllInOne ^
    /p:PublishSingleFile=true
```

**To Deploy:**
1. Copy `FolderLockApp.AllInOne.exe`
2. Run it (no installation needed)

### Option 3: Framework-Dependent

Smaller files, but requires .NET 8.0 Runtime installed on target machine.

```cmd
dotnet publish FolderLockApp.GUI\FolderLockApp.GUI.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained false ^
    --output .\Output\GUI-FD
```

Output: ~5-10 MB (requires .NET 8.0 Runtime)

---

## Build Configurations

### Debug Build
```cmd
dotnet build --configuration Debug
```
- Includes debug symbols
- No optimizations
- Larger file size
- Easier debugging

### Release Build
```cmd
dotnet build --configuration Release
```
- Optimized code
- Smaller file size
- No debug symbols
- Production-ready

### Publish Options

#### Self-Contained
```cmd
--self-contained true
```
- Includes .NET runtime
- Larger file size (~60-100 MB)
- No runtime installation needed
- **Recommended for distribution**

#### Framework-Dependent
```cmd
--self-contained false
```
- Requires .NET runtime on target
- Smaller file size (~5-10 MB)
- User must install .NET 8.0

#### Single File
```cmd
/p:PublishSingleFile=true
```
- Everything in one `.exe`
- Self-extracting
- Easier distribution

#### Compression
```cmd
/p:EnableCompressionInSingleFile=true
```
- Compresses the single file
- Smaller download size
- Slightly slower first launch

#### Trimming (Advanced)
```cmd
/p:PublishTrimmed=true
```
- Removes unused code
- Much smaller size
- May break reflection-based code
- **Test thoroughly!**

---

## Reducing File Size

### Method 1: Trimming
```cmd
dotnet publish ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    /p:PublishSingleFile=true ^
    /p:PublishTrimmed=true ^
    /p:TrimMode=link
```

Can reduce size by 30-50%, but may break some features.

### Method 2: Ready-to-Run (R2R)
```cmd
dotnet publish ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    /p:PublishSingleFile=true ^
    /p:PublishReadyToRun=true
```

Faster startup, but larger file size.

### Method 3: Framework-Dependent
```cmd
dotnet publish ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained false
```

Smallest size, but requires .NET 8.0 Runtime.

---

## Troubleshooting

### Build Errors

**Error: SDK not found**
```
Solution: Install .NET 8.0 SDK
Download: https://dotnet.microsoft.com/download/dotnet/8.0
```

**Error: Project not found**
```
Solution: Ensure you're in the correct directory
Check: Directory should contain .csproj files
```

**Error: Restore failed**
```cmd
Solution: Restore packages manually
dotnet restore
```

### Publish Errors

**Error: Runtime not found**
```
Solution: Specify correct runtime identifier
Use: --runtime win-x64
```

**Error: Out of memory**
```
Solution: Close other applications
Or: Build without compression
```

**Error: Access denied**
```
Solution: Run as Administrator
Or: Close any running instances
```

### File Size Issues

**Files too large (>100 MB)**
```
Solutions:
1. Use framework-dependent build
2. Enable trimming (test thoroughly)
3. Remove unnecessary dependencies
```

**Files too small (< 5 MB)**
```
Check: May be framework-dependent
Verify: .NET runtime required on target
```

---

## Verification

### After Building

1. **Check file exists:**
   ```cmd
   dir Output\GUI\FolderLockApp.GUI.exe
   ```

2. **Check file size:**
   ```cmd
   dir Output\GUI\FolderLockApp.GUI.exe | findstr "FolderLockApp"
   ```

3. **Test execution:**
   ```cmd
   Output\GUI\FolderLockApp.GUI.exe
   ```

4. **Verify admin prompt:**
   - Should show UAC prompt
   - Should request elevation

5. **Check integrity:**
   - Application should verify code integrity
   - Should show security checks

---

## Distribution Checklist

Before distributing:

- [ ] Build in Release configuration
- [ ] Test on clean Windows installation
- [ ] Verify admin privileges work
- [ ] Test code integrity verification
- [ ] Check all features work
- [ ] Include documentation
- [ ] Create installation instructions
- [ ] Test uninstallation
- [ ] Verify no sensitive data included
- [ ] Sign executables (production)

---

## Quick Reference

### Build Everything (Development)
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

### Clean Build
```cmd
dotnet clean
dotnet build --configuration Release
```

### Rebuild Everything
```cmd
dotnet clean
dotnet restore
dotnet build --configuration Release
```

---

## File Size Comparison

| Build Type | Size | Runtime Required | Notes |
|------------|------|------------------|-------|
| Debug | ~10 MB | Yes (.NET 8.0) | Development only |
| Release | ~8 MB | Yes (.NET 8.0) | Requires runtime |
| Self-Contained | ~70 MB | No | Includes runtime |
| Single File | ~70 MB | No | One executable |
| Trimmed | ~40 MB | No | May break features |
| All-in-One | ~90 MB | No | Everything included |

---

## Next Steps

After building:
1. Test the executable
2. Read deployment documentation
3. Create installation package
4. Test on target systems
5. Distribute to users

For usage instructions, see:
- `GET_STARTED.md` - Quick start
- `USER_GUIDE.md` - Complete guide
- `README.md` - Overview
