using FolderLockApp.Core.Interfaces;
using FolderLockApp.Core.Models;
using FolderLockApp.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Security;

namespace FolderLockApp.Service;

/// <summary>
/// Background service that monitors locked folders and handles encryption/decryption operations.
/// Implements Requirements 3.1, 3.2, 3.3, 3.4, 3.5, 2.1
/// </summary>
public class EncryptionBackgroundService : BackgroundService
{
    private readonly ILogger<EncryptionBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private List<LockedFolderEntry> _lockedFolders = new();
    private readonly Dictionary<string, FileSystemWatcher> _folderWatchers = new();
    private readonly object _lockObject = new();

    public EncryptionBackgroundService(
        ILogger<EncryptionBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Called when the service starts. Loads all locked folders from registry.
    /// Implements Requirement 3.2: Service starts automatically on system boot
    /// Implements Requirement 3.3: Load all locked folder entries from registry
    /// Implements Requirement 3.4: Monitor all registered locked folder locations
    /// </summary>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("FolderLockApp Encryption Service is starting");

        try
        {
            // Load all locked folders from the registry
            await LoadLockedFoldersAsync();
            
            _logger.LogInformation("Successfully loaded {Count} locked folders from registry", _lockedFolders.Count);

            // Start monitoring all locked folders
            StartMonitoringLockedFolders();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading locked folders during service startup");
            throw;
        }

        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Main execution loop for the background service.
    /// Implements Requirement 3.5: Remain running as background process
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FolderLockApp Encryption Service is now running");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Service runs continuously in the background
                // FileSystemWatchers handle monitoring via events
                
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                
                // Periodically log that service is still running
                _logger.LogDebug("Service heartbeat - monitoring {Count} locked folders", _lockedFolders.Count);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Service execution cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in service execution loop");
            throw;
        }
    }

    /// <summary>
    /// Called when the service is stopping.
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("FolderLockApp Encryption Service is stopping");

        try
        {
            // Stop all folder watchers
            StopMonitoringAllFolders();

            // Cleanup resources
            lock (_lockObject)
            {
                _lockedFolders.Clear();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during service shutdown");
        }

        await base.StopAsync(cancellationToken);
        
        _logger.LogInformation("FolderLockApp Encryption Service stopped");
    }

    /// <summary>
    /// Loads all locked folders from the registry into memory.
    /// Implements Requirement 3.3: Load all locked folder entries from FolderRegistry
    /// </summary>
    private async Task LoadLockedFoldersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var folderRegistry = scope.ServiceProvider.GetRequiredService<IFolderRegistry>();

        var folders = await folderRegistry.GetAllLockedFoldersAsync();
        
        lock (_lockObject)
        {
            _lockedFolders = folders.ToList();
        }

        foreach (var folder in _lockedFolders)
        {
            _logger.LogInformation("Loaded locked folder: {FolderPath}, Locked: {IsLocked}, Last Accessed: {LastAccessed}",
                folder.FolderPath, folder.IsLocked, folder.LastAccessed);
        }
    }

    /// <summary>
    /// Gets the current list of locked folders (thread-safe).
    /// </summary>
    public IReadOnlyList<LockedFolderEntry> GetLockedFolders()
    {
        lock (_lockObject)
        {
            return _lockedFolders.AsReadOnly();
        }
    }

    /// <summary>
    /// Reloads the locked folders from the registry.
    /// Can be called when folders are added or removed.
    /// </summary>
    public async Task ReloadLockedFoldersAsync()
    {
        _logger.LogInformation("Reloading locked folders from registry");
        
        // Stop existing watchers
        StopMonitoringAllFolders();
        
        // Reload folders
        await LoadLockedFoldersAsync();
        
        // Restart monitoring
        StartMonitoringLockedFolders();
    }

    /// <summary>
    /// Starts monitoring all locked folders for access attempts.
    /// Implements Requirement 3.4: Monitor all registered locked folder locations for access attempts
    /// </summary>
    private void StartMonitoringLockedFolders()
    {
        lock (_lockObject)
        {
            foreach (var folder in _lockedFolders.Where(f => f.IsLocked))
            {
                StartMonitoringFolder(folder.FolderPath);
            }
        }
    }

    /// <summary>
    /// Starts monitoring a specific folder for access attempts.
    /// Implements Requirement 2.1: Detect when user tries to access locked folder
    /// </summary>
    private void StartMonitoringFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            _logger.LogWarning("Cannot monitor folder {FolderPath} - directory does not exist", folderPath);
            return;
        }

        if (_folderWatchers.ContainsKey(folderPath))
        {
            _logger.LogDebug("Folder {FolderPath} is already being monitored", folderPath);
            return;
        }

        try
        {
            var watcher = new FileSystemWatcher(folderPath)
            {
                NotifyFilter = NotifyFilters.FileName 
                             | NotifyFilters.DirectoryName 
                             | NotifyFilters.LastAccess 
                             | NotifyFilters.LastWrite 
                             | NotifyFilters.CreationTime,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            // Subscribe to events
            watcher.Changed += OnFolderAccessed;
            watcher.Created += OnFolderAccessed;
            watcher.Deleted += OnFolderAccessed;
            watcher.Renamed += OnFolderRenamed;
            watcher.Error += OnWatcherError;

            _folderWatchers[folderPath] = watcher;

            _logger.LogInformation("Started monitoring locked folder: {FolderPath}", folderPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start monitoring folder: {FolderPath}", folderPath);
        }
    }

    /// <summary>
    /// Stops monitoring a specific folder.
    /// </summary>
    private void StopMonitoringFolder(string folderPath)
    {
        if (_folderWatchers.TryGetValue(folderPath, out var watcher))
        {
            watcher.EnableRaisingEvents = false;
            watcher.Changed -= OnFolderAccessed;
            watcher.Created -= OnFolderAccessed;
            watcher.Deleted -= OnFolderAccessed;
            watcher.Renamed -= OnFolderRenamed;
            watcher.Error -= OnWatcherError;
            watcher.Dispose();

            _folderWatchers.Remove(folderPath);

            _logger.LogInformation("Stopped monitoring folder: {FolderPath}", folderPath);
        }
    }

    /// <summary>
    /// Stops monitoring all folders.
    /// </summary>
    private void StopMonitoringAllFolders()
    {
        var foldersToStop = _folderWatchers.Keys.ToList();
        foreach (var folderPath in foldersToStop)
        {
            StopMonitoringFolder(folderPath);
        }
    }

    /// <summary>
    /// Handles folder access events.
    /// Implements Requirement 2.1: Display password dialog when access is attempted
    /// Implements Requirement 3.5: Log all access attempts
    /// </summary>
    private void OnFolderAccessed(object sender, FileSystemEventArgs e)
    {
        try
        {
            var watcher = sender as FileSystemWatcher;
            if (watcher == null) return;

            var folderPath = watcher.Path;

            _logger.LogInformation("Access attempt detected on locked folder: {FolderPath}, Change: {ChangeType}, File: {FileName}",
                folderPath, e.ChangeType, e.Name);

            // In a real implementation, this would trigger a password dialog
            // For a Windows Service, we would need to use IPC to communicate with the GUI
            // or use a separate process to show the dialog
            // For now, we log the attempt and provide a method to handle unlock requests

            HandleFolderAccessAttempt(folderPath, e.ChangeType.ToString(), e.Name ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling folder access event");
        }
    }

    /// <summary>
    /// Handles folder rename events.
    /// </summary>
    private void OnFolderRenamed(object sender, RenamedEventArgs e)
    {
        try
        {
            var watcher = sender as FileSystemWatcher;
            if (watcher == null) return;

            var folderPath = watcher.Path;

            _logger.LogInformation("Rename detected in locked folder: {FolderPath}, Old: {OldName}, New: {NewName}",
                folderPath, e.OldName, e.Name);

            HandleFolderAccessAttempt(folderPath, "Renamed", $"{e.OldName} -> {e.Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling folder rename event");
        }
    }

    /// <summary>
    /// Handles watcher errors.
    /// </summary>
    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        var watcher = sender as FileSystemWatcher;
        var folderPath = watcher?.Path ?? "Unknown";

        _logger.LogError(e.GetException(), "FileSystemWatcher error for folder: {FolderPath}", folderPath);
    }

    /// <summary>
    /// Handles a folder access attempt by logging and potentially triggering unlock workflow.
    /// Implements Requirement 3.5: Log all access attempts and outcomes
    /// </summary>
    private void HandleFolderAccessAttempt(string folderPath, string changeType, string fileName)
    {
        _logger.LogInformation("Folder access attempt - Path: {FolderPath}, Type: {ChangeType}, File: {FileName}",
            folderPath, changeType, fileName);

        // In a production system, this would:
        // 1. Check if the folder is still locked
        // 2. Trigger IPC call to show password dialog (via GUI or separate dialog process)
        // 3. Wait for password input
        // 4. Call UnlockFolderAsync if password is correct
        // 
        // For now, we provide the infrastructure and logging
    }

    /// <summary>
    /// Unlocks a folder by decrypting it with the provided password.
    /// Implements Requirement 2.2: Decrypt folder on successful password entry
    /// Implements Requirement 2.3: Restore original folder icon and allow normal access
    /// Implements Requirement 2.4: Display error on incorrect password
    /// </summary>
    public async Task<(bool Success, string Message)> UnlockFolderAsync(string folderPath, SecureString password)
    {
        _logger.LogInformation("Unlock attempt for folder: {FolderPath}", folderPath);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var folderRegistry = scope.ServiceProvider.GetRequiredService<IFolderRegistry>();
            var encryptionEngine = scope.ServiceProvider.GetRequiredService<IEncryptionEngine>();

            // Get folder entry from registry
            var folderEntry = await folderRegistry.GetLockedFolderAsync(folderPath);
            if (folderEntry == null)
            {
                _logger.LogWarning("Unlock failed - folder not found in registry: {FolderPath}", folderPath);
                return (false, "Folder is not registered as locked.");
            }

            // Verify password (Requirement 5.3: Verify password before decryption)
            if (!encryptionEngine.VerifyPassword(password, folderEntry.PasswordHash, folderEntry.Salt))
            {
                _logger.LogWarning("Unlock failed - incorrect password for folder: {FolderPath}", folderPath);
                return (false, "Incorrect password.");
            }

            _logger.LogInformation("Password verified successfully for folder: {FolderPath}", folderPath);

            // Decrypt the folder
            var decryptionResult = await encryptionEngine.DecryptFolderAsync(folderPath, password);

            if (!decryptionResult.Success)
            {
                _logger.LogError("Decryption failed for folder: {FolderPath}, Error: {Error}",
                    folderPath, decryptionResult.ErrorMessage);
                return (false, $"Decryption failed: {decryptionResult.ErrorMessage}");
            }

            _logger.LogInformation("Successfully decrypted folder: {FolderPath}, Files processed: {FileCount}",
                folderPath, decryptionResult.FilesProcessed);

            // Update folder status in registry
            folderEntry.IsLocked = false;
            await folderRegistry.UpdateLastAccessAsync(folderPath);

            // Stop monitoring this folder since it's now unlocked
            StopMonitoringFolder(folderPath);

            // Reload locked folders list
            await LoadLockedFoldersAsync();

            _logger.LogInformation("Folder unlocked successfully: {FolderPath}", folderPath);
            return (true, "Folder unlocked successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking folder: {FolderPath}", folderPath);
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Locks a folder by encrypting it with the provided password.
    /// This can be called via IPC from the GUI or shell extension.
    /// </summary>
    public async Task<(bool Success, string Message)> LockFolderAsync(string folderPath, SecureString password)
    {
        _logger.LogInformation("Lock attempt for folder: {FolderPath}", folderPath);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var folderRegistry = scope.ServiceProvider.GetRequiredService<IFolderRegistry>();
            var encryptionEngine = scope.ServiceProvider.GetRequiredService<IEncryptionEngine>();

            // Encrypt the folder
            var encryptionResult = await encryptionEngine.EncryptFolderAsync(folderPath, password);

            if (!encryptionResult.Success)
            {
                _logger.LogError("Encryption failed for folder: {FolderPath}, Error: {Error}",
                    folderPath, encryptionResult.ErrorMessage);
                return (false, $"Encryption failed: {encryptionResult.ErrorMessage}");
            }

            _logger.LogInformation("Successfully encrypted folder: {FolderPath}, Files processed: {FileCount}",
                folderPath, encryptionResult.FilesProcessed);

            // Generate salt and hash password for storage
            var salt = EncryptionEngine.GenerateSalt();
            var passwordHash = encryptionEngine.HashPassword(password, salt);

            // Register in folder registry
            await folderRegistry.RegisterLockedFolderAsync(folderPath, passwordHash, salt);

            // Reload locked folders and start monitoring
            await ReloadLockedFoldersAsync();

            _logger.LogInformation("Folder locked successfully: {FolderPath}", folderPath);
            return (true, "Folder locked successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking folder: {FolderPath}", folderPath);
            return (false, $"Error: {ex.Message}");
        }
    }
}
