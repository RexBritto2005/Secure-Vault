# FolderLockApp Installer

This directory contains the installer configuration for FolderLockApp.

## Building the Installer

### Prerequisites

1. **Inno Setup 6.x** - Download from: https://jrsoftware.org/isdl.php
   - Install Inno Setup with the default options
   - The installer compiler will be available at: `C:\Program Files (x86)\Inno Setup 6\ISCC.exe`

2. **Build the Application** - Before creating the installer, build all projects in Release mode:
   ```powershell
   dotnet build FolderLockApp.sln --configuration Release
   ```

### Building the Installer

#### Option 1: Using Inno Setup GUI
1. Open `FolderLockApp-Setup.iss` in Inno Setup
2. Click **Build** → **Compile**
3. The installer will be created in `Installer\Output\`

#### Option 2: Using Command Line
```powershell
# From the repository root
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "Installer\FolderLockApp-Setup.iss"
```

#### Option 3: Using PowerShell Script
```powershell
.\Build-Installer.ps1
```

### Output

The installer will be created as:
```
Installer\Output\FolderLockApp-Setup-1.0.0.exe
```

## What the Installer Does

### Installation Steps
1. ✅ **Installs GUI Application** to `C:\Program Files\FolderLock\`
2. ✅ **Installs Windows Service** to `C:\Program Files\FolderLock\Service\`
3. ✅ **Installs Shell Extension** to `C:\Program Files\FolderLock\ShellExtension\`
4. ✅ **Creates Data Directory** at `C:\ProgramData\FolderLockApp\`
5. ✅ **Installs and Starts Service** - Registers as Windows Service
6. ✅ **Creates Shortcuts** - Start Menu and optional Desktop shortcut
7. ✅ **Launches Application** - Optional post-install launch

### Uninstallation Steps
1. ⚠️ **Checks for Locked Folders** - Warns if encrypted folders exist
2. ✅ **Stops Windows Service**
3. ✅ **Uninstalls Service**
4. ✅ **Removes Application Files**
5. ✅ **Removes Shortcuts**
6. ℹ️ **Preserves User Data** - Database and logs are kept for safety

## Installer Features

### User Experience
- Modern wizard-style interface
- Clear installation progress
- Optional desktop shortcut
- Post-installation launch option

### Safety Features
- **Admin Privileges Required** - Ensures proper service installation
- **Uninstall Warning** - Alerts users about locked folders
- **Data Preservation** - Keeps database during uninstall
- **Service Management** - Properly stops/starts service

### Customization
- Installation directory selection
- Desktop shortcut option
- Quick launch icon option (Windows 7 and below)

## Directory Structure After Installation

```
C:\Program Files\FolderLock\
├── FolderLockApp.GUI.exe          # Main application
├── FolderLockApp.Core.dll         # Core library
├── Service\
│   ├── FolderLockApp.Service.exe  # Windows Service
│   └── ...                        # Service dependencies
├── ShellExtension\
│   ├── FolderLockApp.ShellExtension.dll
│   └── ...                        # Extension dependencies
├── README.md
├── SERVICE-INSTALLATION.md
└── TESTING-GUIDE.md

C:\ProgramData\FolderLockApp\
├── folderlock.db                  # SQLite database
└── Logs\
    ├── service-YYYYMMDD.log       # Service logs
    └── security-YYYYMMDD.log      # Security logs
```

## Requirements

### System Requirements
- **OS:** Windows 10 or later (64-bit)
- **Framework:** .NET 8.0 Runtime (included in installer)
- **Privileges:** Administrator rights for installation
- **Disk Space:** ~50 MB for application + space for encrypted files

### Runtime Requirements
- .NET 8.0 Desktop Runtime
- .NET 8.0 ASP.NET Core Runtime (for service)

## Troubleshooting

### Build Errors

**Error: "Cannot find source file"**
- Solution: Build the solution in Release mode first
- Command: `dotnet build FolderLockApp.sln --configuration Release`

**Error: "Inno Setup not found"**
- Solution: Install Inno Setup from https://jrsoftware.org/isdl.php
- Verify installation path matches the script

### Installation Errors

**Error: "Service installation failed"**
- Solution: Run installer as Administrator
- Check Windows Event Log for details

**Error: "Access denied"**
- Solution: Close any running instances of FolderLock
- Ensure no files are locked in the installation directory

### Uninstallation Warnings

**Warning: "Locked folders detected"**
- This is a safety feature
- Unlock all folders before uninstalling
- Or keep the database file to unlock later

## Advanced Configuration

### Custom Installation Path
Users can choose a custom installation directory during setup.

### Silent Installation
```powershell
FolderLockApp-Setup-1.0.0.exe /SILENT
```

### Very Silent Installation (no UI)
```powershell
FolderLockApp-Setup-1.0.0.exe /VERYSILENT
```

### Silent Uninstallation
```powershell
"C:\Program Files\FolderLock\unins000.exe" /SILENT
```

## Building for Distribution

### Code Signing (Recommended)
For production releases, sign the installer with a code signing certificate:

```powershell
signtool sign /f "certificate.pfx" /p "password" /t "http://timestamp.digicert.com" "FolderLockApp-Setup-1.0.0.exe"
```

### Creating a Portable Version
To create a portable version without installer:
1. Build in Release mode
2. Copy all files from `bin\Release\` to a folder
3. Include a `portable.txt` file to indicate portable mode
4. Distribute as ZIP file

## Support

For issues with the installer:
1. Check the installation log: `%TEMP%\Setup Log YYYY-MM-DD #XXX.txt`
2. Check Windows Event Viewer for service errors
3. Review the TESTING-GUIDE.md for common issues

## License

This installer is part of the FolderLockApp project.
See LICENSE.txt for details.
