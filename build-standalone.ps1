# PowerShell script to build standalone FolderLock application
# This creates a single-file executable with all dependencies included

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  FolderLock Standalone Builder" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

# Configuration
$OutputDir = ".\Standalone-Release"
$Configuration = "Release"
$Runtime = "win-x64"

# Clean output directory
Write-Host "Cleaning output directory..." -ForegroundColor Yellow
if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir | Out-Null

# Build Core Library
Write-Host "`nBuilding Core Library..." -ForegroundColor Yellow
dotnet build FolderLockApp.Core\FolderLockApp.Core.csproj `
    --configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host "Core build failed!" -ForegroundColor Red
    exit 1
}

# Build and Publish Service as Single File
Write-Host "`nBuilding Background Service (Single File)..." -ForegroundColor Yellow
dotnet publish FolderLockApp.Service\FolderLockApp.Service.csproj `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    --output "$OutputDir\Service" `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:EnableCompressionInSingleFile=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "Service build failed!" -ForegroundColor Red
    exit 1
}

# Build and Publish GUI as Single File
Write-Host "`nBuilding GUI Application (Single File)..." -ForegroundColor Yellow
dotnet publish FolderLockApp.GUI\FolderLockApp.GUI.csproj `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    --output "$OutputDir\GUI" `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:EnableCompressionInSingleFile=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "GUI build failed!" -ForegroundColor Red
    exit 1
}

# Build and Publish Shell Extension
Write-Host "`nBuilding Shell Extension..." -ForegroundColor Yellow
dotnet publish FolderLockApp.ShellExtension\FolderLockApp.ShellExtension.csproj `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    --output "$OutputDir\ShellExtension" `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:EnableCompressionInSingleFile=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "Shell Extension build failed!" -ForegroundColor Red
    exit 1
}

# Build Launcher
Write-Host "`nBuilding Unified Launcher..." -ForegroundColor Yellow
dotnet publish FolderLockApp.Launcher\FolderLockApp.Launcher.csproj `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    --output "$OutputDir\Launcher" `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:EnableCompressionInSingleFile=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "Launcher build failed!" -ForegroundColor Red
    exit 1
}

# Create unified package
Write-Host "`nCreating unified package..." -ForegroundColor Yellow
$PackageDir = "$OutputDir\FolderLock-Portable"
New-Item -ItemType Directory -Path $PackageDir | Out-Null

# Copy main executables
Copy-Item "$OutputDir\Launcher\FolderLockApp.Launcher.exe" "$PackageDir\FolderLock.exe"
Copy-Item "$OutputDir\Service\FolderLockApp.Service.exe" "$PackageDir\FolderLockApp.Service.exe"
Copy-Item "$OutputDir\GUI\FolderLockApp.GUI.exe" "$PackageDir\FolderLockApp.GUI.exe"
Copy-Item "$OutputDir\ShellExtension\FolderLockApp.ShellExtension.exe" "$PackageDir\FolderLockApp.ShellExtension.exe"

# Copy documentation
Write-Host "Copying documentation..." -ForegroundColor Yellow
Copy-Item "README.md" "$PackageDir\" -ErrorAction SilentlyContinue
Copy-Item "GET_STARTED.md" "$PackageDir\" -ErrorAction SilentlyContinue
Copy-Item "QUICK_START.md" "$PackageDir\" -ErrorAction SilentlyContinue
Copy-Item "USER_GUIDE.md" "$PackageDir\" -ErrorAction SilentlyContinue

# Create installation script
Write-Host "Creating installation script..." -ForegroundColor Yellow
$InstallScript = @"
@echo off
echo =========================================
echo   FolderLock Installation
echo =========================================
echo.

REM Check for admin privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script must be run as Administrator!
    echo Right-click and select 'Run as administrator'
    pause
    exit /b 1
)

echo Installing FolderLock Background Service...
sc create "FolderLockApp Encryption Service" binPath="%~dp0FolderLockApp.Service.exe" start=auto
if %errorLevel% neq 0 (
    echo WARNING: Service installation failed. It may already be installed.
)

echo Starting service...
sc start "FolderLockApp Encryption Service"

echo.
echo Installation complete!
echo.
echo To use FolderLock:
echo   1. Run FolderLock.exe (the main launcher)
echo   2. Or run FolderLockApp.GUI.exe directly
echo.
echo To register shell extension (right-click menu):
echo   Run: FolderLockApp.ShellExtension.exe register
echo.
pause
"@

$InstallScript | Out-File -FilePath "$PackageDir\Install.bat" -Encoding ASCII

# Create uninstall script
$UninstallScript = @"
@echo off
echo =========================================
echo   FolderLock Uninstallation
echo =========================================
echo.

REM Check for admin privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script must be run as Administrator!
    echo Right-click and select 'Run as administrator'
    pause
    exit /b 1
)

echo WARNING: Make sure all folders are unlocked before uninstalling!
echo.
pause

echo Stopping service...
sc stop "FolderLockApp Encryption Service"

echo Removing service...
sc delete "FolderLockApp Encryption Service"

echo Unregistering shell extension...
"%~dp0FolderLockApp.ShellExtension.exe" unregister

echo.
echo Uninstallation complete!
echo.
echo You can now delete this folder.
echo.
pause
"@

$UninstallScript | Out-File -FilePath "$PackageDir\Uninstall.bat" -Encoding ASCII

# Create README for the package
$PackageReadme = @"
# FolderLock Portable Package

## Quick Start

1. **Install** (Run as Administrator):
   - Double-click `Install.bat`
   - This installs and starts the background service

2. **Launch Application**:
   - Double-click `FolderLock.exe` (main launcher)
   - Or run `FolderLockApp.GUI.exe` directly

3. **Optional - Shell Extension**:
   - Run as Administrator: `FolderLockApp.ShellExtension.exe register`
   - Adds right-click menu to Windows Explorer

## Files Included

- **FolderLock.exe** - Main launcher (starts everything)
- **FolderLockApp.GUI.exe** - GUI application
- **FolderLockApp.Service.exe** - Background service
- **FolderLockApp.ShellExtension.exe** - Shell extension
- **Install.bat** - Installation script
- **Uninstall.bat** - Uninstallation script
- **Documentation** - User guides and help files

## System Requirements

- Windows 10 or Windows 11
- Administrator privileges
- ~100 MB disk space

## Usage

### Lock a Folder:
1. Open FolderLock.exe
2. Click "Lock Folder"
3. Select folder and enter password
4. Done!

### Unlock a Folder:
1. Open FolderLock.exe
2. Select folder from list
3. Click "Unlock"
4. Enter password

## Important Notes

⚠️ **Never forget your password!** Files cannot be recovered without it.
⚠️ **Backup first!** Always backup important files before locking.
⚠️ **Test unlock!** After locking, immediately test unlocking.

## Uninstallation

1. Unlock all folders first!
2. Run `Uninstall.bat` as Administrator
3. Delete this folder

## Documentation

See the included markdown files for detailed documentation:
- GET_STARTED.md - Simple guide
- QUICK_START.md - 5-minute setup
- USER_GUIDE.md - Complete documentation
- README.md - Overview

## Support

For issues or questions, check the documentation files.
"@

$PackageReadme | Out-File -FilePath "$PackageDir\README.txt" -Encoding UTF8

# Create a simple launcher batch file
$LauncherBat = @"
@echo off
start "" "%~dp0FolderLock.exe"
"@

$LauncherBat | Out-File -FilePath "$PackageDir\Launch-FolderLock.bat" -Encoding ASCII

# Calculate sizes
Write-Host "`nCalculating package sizes..." -ForegroundColor Yellow
$ServiceSize = (Get-Item "$PackageDir\FolderLockApp.Service.exe").Length / 1MB
$GuiSize = (Get-Item "$PackageDir\FolderLockApp.GUI.exe").Length / 1MB
$LauncherSize = (Get-Item "$PackageDir\FolderLock.exe").Length / 1MB
$ShellSize = (Get-Item "$PackageDir\FolderLockApp.ShellExtension.exe").Length / 1MB

# Summary
Write-Host "`n=========================================" -ForegroundColor Green
Write-Host "  Build Complete!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output Location: $PackageDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "File Sizes:" -ForegroundColor Yellow
Write-Host "  - FolderLock.exe (Launcher): $([math]::Round($LauncherSize, 2)) MB"
Write-Host "  - FolderLockApp.GUI.exe: $([math]::Round($GuiSize, 2)) MB"
Write-Host "  - FolderLockApp.Service.exe: $([math]::Round($ServiceSize, 2)) MB"
Write-Host "  - FolderLockApp.ShellExtension.exe: $([math]::Round($ShellSize, 2)) MB"
Write-Host ""
Write-Host "To use:" -ForegroundColor Yellow
Write-Host "  1. Navigate to: $PackageDir"
Write-Host "  2. Run Install.bat as Administrator"
Write-Host "  3. Run FolderLock.exe"
Write-Host ""
Write-Host "All executables are self-contained and include .NET runtime." -ForegroundColor Cyan
Write-Host ""
