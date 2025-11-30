@echo off
REM Batch file wrapper for PowerShell build script

echo =========================================
echo   FolderLock Standalone Builder
echo =========================================
echo.

REM Check for admin privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo WARNING: Not running as Administrator.
    echo Some operations may fail without admin privileges.
    echo.
    pause
)

echo Running PowerShell build script...
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0build-standalone.ps1"

if %errorLevel% neq 0 (
    echo.
    echo Build failed! Check the errors above.
    pause
    exit /b 1
)

echo.
echo Build completed successfully!
echo.
pause
