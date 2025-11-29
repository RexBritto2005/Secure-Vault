# FolderLockApp Service Management Script
# Must be run as Administrator

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Start", "Stop", "Restart", "Status")]
    [string]$Action
)

$ServiceName = "FolderLockAppService"

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Please right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    pause
    exit 1
}

# Check if service exists
$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if (-not $service) {
    Write-Host "ERROR: Service '$ServiceName' is not installed" -ForegroundColor Red
    Write-Host "Please run Install-Service.ps1 first" -ForegroundColor Yellow
    pause
    exit 1
}

switch ($Action) {
    "Start" {
        Write-Host "Starting FolderLock Service..." -ForegroundColor Cyan
        if ($service.Status -eq 'Running') {
            Write-Host "Service is already running" -ForegroundColor Yellow
        } else {
            Start-Service -Name $ServiceName
            Start-Sleep -Seconds 2
            $service = Get-Service -Name $ServiceName
            if ($service.Status -eq 'Running') {
                Write-Host "✓ Service started successfully!" -ForegroundColor Green
            } else {
                Write-Host "✗ Failed to start service" -ForegroundColor Red
            }
        }
    }
    
    "Stop" {
        Write-Host "Stopping FolderLock Service..." -ForegroundColor Cyan
        if ($service.Status -eq 'Stopped') {
            Write-Host "Service is already stopped" -ForegroundColor Yellow
        } else {
            Stop-Service -Name $ServiceName -Force
            Start-Sleep -Seconds 2
            $service = Get-Service -Name $ServiceName
            if ($service.Status -eq 'Stopped') {
                Write-Host "✓ Service stopped successfully!" -ForegroundColor Green
            } else {
                Write-Host "✗ Failed to stop service" -ForegroundColor Red
            }
        }
    }
    
    "Restart" {
        Write-Host "Restarting FolderLock Service..." -ForegroundColor Cyan
        Restart-Service -Name $ServiceName -Force
        Start-Sleep -Seconds 2
        $service = Get-Service -Name $ServiceName
        if ($service.Status -eq 'Running') {
            Write-Host "✓ Service restarted successfully!" -ForegroundColor Green
        } else {
            Write-Host "✗ Failed to restart service" -ForegroundColor Red
        }
    }
    
    "Status" {
        Write-Host "FolderLock Service Status:" -ForegroundColor Cyan
        Write-Host "  Name: $($service.Name)"
        Write-Host "  Display Name: $($service.DisplayName)"
        Write-Host "  Status: $($service.Status)" -ForegroundColor $(if ($service.Status -eq 'Running') { 'Green' } else { 'Yellow' })
        Write-Host "  Startup Type: $($service.StartType)"
    }
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
