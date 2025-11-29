@echo off
REM FolderLockApp Installer Build Script (Batch Wrapper)
REM This script runs the PowerShell build script with Administrator privileges

echo =====================================
echo FolderLockApp Installer Builder
echo =====================================
echo.

REM Check for Administrator privileges
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Running with Administrator privileges...
    echo.
) else (
    echo Warning: Not running as Administrator
    echo Some operations may require elevation
    echo.
)

REM Run the PowerShell script
powershell.exe -ExecutionPolicy Bypass -File "%~dp0Build-Installer.ps1" %*

if %errorLevel% == 0 (
    echo.
    echo =====================================
    echo Build completed successfully!
    echo =====================================
) else (
    echo.
    echo =====================================
    echo Build failed with error code %errorLevel%
    echo =====================================
)

echo.
pause
