@echo off
REM FolderLock - Single Command Build Script
REM Builds a standalone executable with everything included

echo =========================================
echo   FolderLock - Building Release
echo =========================================
echo.

REM Check for admin privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo WARNING: Not running as Administrator.
    echo Some features may not work without admin privileges.
    echo.
)

echo Building FolderLock All-in-One executable...
echo.

dotnet publish FolderLockApp.AllInOne\FolderLockApp.AllInOne.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output .\Release ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true

if %errorLevel% neq 0 (
    echo.
    echo Build FAILED! Check the errors above.
    pause
    exit /b 1
)

echo.
echo =========================================
echo   Build Complete!
echo =========================================
echo.
echo Output: Release\FolderLockApp.AllInOne.exe
echo.
echo To run:
echo   1. Navigate to Release folder
echo   2. Run FolderLockApp.AllInOne.exe
echo   3. Accept UAC prompt (admin required)
echo.
echo File size: 
dir Release\FolderLockApp.AllInOne.exe | findstr "FolderLockApp"
echo.
pause
