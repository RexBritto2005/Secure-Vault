# FolderLockApp Service Installation Script
# Must be run as Administrator

param(
    [switch]$Uninstall
)

$ServiceName = "FolderLockAppService"
$ServiceDisplayName = "FolderLock Encryption Service"
$ServiceDescription = "Background service that monitors and manages encrypted folders for FolderLockApp"
$ServicePath = Join-Path $PSScriptRoot "FolderLockApp.Service\bin\Release\net8.0\FolderLockApp.Service.exe"

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Please right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    pause
    exit 1
}

if ($Uninstall) {
    Write-Host "Uninstalling FolderLock Service..." -ForegroundColor Cyan
    
    # Stop the service if running
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($service) {
        if ($service.Status -eq 'Running') {
            Write-Host "Stopping service..." -ForegroundColor Yellow
            Stop-Service -Name $ServiceName -Force
            Start-Sleep -Seconds 2
        }
        
        # Delete the service
        Write-Host "Removing service..." -ForegroundColor Yellow
        sc.exe delete $ServiceName
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Service uninstalled successfully!" -ForegroundColor Green
        } else {
            Write-Host "✗ Failed to uninstall service" -ForegroundColor Red
        }
    } else {
        Write-Host "Service is not installed" -ForegroundColor Yellow
    }
} else {
    Write-Host "Installing FolderLock Service..." -ForegroundColor Cyan
    
    # Check if service executable exists
    if (-not (Test-Path $ServicePath)) {
        Write-Host "ERROR: Service executable not found at: $ServicePath" -ForegroundColor Red
        Write-Host "Please build the solution first using: dotnet build --configuration Release" -ForegroundColor Yellow
        pause
        exit 1
    }
    
    # Check if service already exists
    $existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($existingService) {
        Write-Host "Service already exists. Stopping and removing..." -ForegroundColor Yellow
        if ($existingService.Status -eq 'Running') {
            Stop-Service -Name $ServiceName -Force
            Start-Sleep -Seconds 2
        }
        sc.exe delete $ServiceName
        Start-Sleep -Seconds 2
    }
    
    # Create the service
    Write-Host "Creating service..." -ForegroundColor Yellow
    sc.exe create $ServiceName binPath= $ServicePath start= auto DisplayName= $ServiceDisplayName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Failed to create service" -ForegroundColor Red
        pause
        exit 1
    }
    
    # Set service description
    sc.exe description $ServiceName $ServiceDescription
    
    # Configure service recovery options (restart on failure)
    Write-Host "Configuring service recovery options..." -ForegroundColor Yellow
    sc.exe failure $ServiceName reset= 86400 actions= restart/60000/restart/60000/restart/60000
    
    # Start the service
    Write-Host "Starting service..." -ForegroundColor Yellow
    Start-Service -Name $ServiceName
    
    # Wait a moment and check status
    Start-Sleep -Seconds 2
    $service = Get-Service -Name $ServiceName
    
    if ($service.Status -eq 'Running') {
        Write-Host "✓ Service installed and started successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Service Details:" -ForegroundColor Cyan
        Write-Host "  Name: $ServiceName"
        Write-Host "  Display Name: $ServiceDisplayName"
        Write-Host "  Status: $($service.Status)"
        Write-Host "  Startup Type: Automatic"
        Write-Host ""
        Write-Host "The service will now start automatically when Windows boots." -ForegroundColor Green
    } else {
        Write-Host "✗ Service installed but failed to start" -ForegroundColor Red
        Write-Host "Status: $($service.Status)" -ForegroundColor Yellow
        Write-Host "Check the Windows Event Log for error details" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
