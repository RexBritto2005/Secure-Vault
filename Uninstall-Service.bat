@echo off
REM FolderLockApp Service Uninstallation Batch Script

echo FolderLock Service Uninstaller
echo ==============================
echo.

REM Check if running as Administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script must be run as Administrator!
    echo Please right-click this file and select "Run as Administrator"
    echo.
    pause
    exit /b 1
)

REM Run the PowerShell script with Uninstall flag
powershell.exe -ExecutionPolicy Bypass -File "%~dp0Install-Service.ps1" -Uninstall

exit /b %errorLevel%
