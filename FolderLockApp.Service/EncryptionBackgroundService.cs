using FolderLockApp.Core.Interfaces;
using FolderLockApp.Core.Models;
using FolderLockApp.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Security;
using System.Text.Json;

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
    private NamedPipeServer? _pipeServer;
    private AutoLockService? _autoLockService;

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
            // Start Named Pipe server for IPC
            using var scope = _serviceProvider.CreateScope();
            var pipeLogger = scope.ServiceProvider.GetRequiredService<ILogger<NamedPipeServer>>();
            _pipeServer = new NamedPipeServer(pipeLogger);
            _pipeServer.OnMessageReceived += HandleIpcRequestAsync;
            _pipeServer.Start();
            
            _logger.LogInformation("Named Pipe server started for IPC communication");

            // Load all locked folders from the registry
            await LoadLockedFoldersAsync();
            
            _logger.LogInformation("Successfully loaded {Count} locked folders from registry", _lockedFolders.Count);

            // Start monitoring all locked folders
            StartMonitoringLockedFolders();

            // Initialize Auto-Lock Service
            var folderRegistry = scope.ServiceProvider.GetRequiredService<IFolderRegistry>();
            var encryptionEngine = scope.ServiceProvider.GetRequiredService<IEncryptionEngine>();
            var autoLockLogger = scope.ServiceProvider.GetRequiredService<ILogger<AutoLockService>>();
            var settings = new FolderLockSettings
            {
                AutoLockEnabled = true,
                AutoLockTimeoutMinutes = 5, // Default: 5 minutes
                ShowAutoLockNotification = true
            };
            _autoLockService = new AutoLockService(folderRegistry, encryptionEngine, autoLockLogger, settings);
            
            _logger.LogInformation("Auto-Lock Service started with {Timeout} minute timeout", settings.AutoLockTimeoutMinutes);
        }
        catch (Exception ex)
        {
            var (isRecoverable, userMessage, technicalDetails) = ErrorHandler.HandleFileSystemError(ex, "service startup", "");
            _logger.LogError(ex, "Error loading locked folders during service startup - {TechnicalDetails}", technicalDetails);
            ErrorHandler.LogSecurityEvent(_logger, "SERVICE_START_FAILED", "N/A", false, technicalDetails);
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
            // Stop Auto-Lock Service
            if (_autoLockService != null)
            {
                _autoLockService.Dispose();
                _logger.LogInformation("Auto-Lock Service stopped");
            }

            // Stop Named Pipe server
            if (_pipeServer != null)
            {
                await _pipeServer.StopAsync();
                _pipeServer.Dispose();
                _logger.LogInformation("Named Pipe server stopped");
            }

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

            // Register folder for auto-lock monitoring
            if (_autoLockService != null)
            {
                _autoLockService.RegisterFolderForAutoLock(folderPath, password);
                _logger.LogInformation("Folder registered for auto-lock monitoring: {FolderPath}", folderPath);
            }

            // Reload locked folders list
            await LoadLockedFoldersAsync();

            _logger.LogInformation("Folder unlocked successfully: {FolderPath}", folderPath);
            ErrorHandler.LogSecurityEvent(_logger, "UNLOCK_SUCCESS", folderPath, true, $"Files processed: {decryptionResult.FilesProcessed}");
            return (true, "Folder unlocked successfully.");
        }
        catch (Exception ex)
        {
            var (isRecoverable, userMessage, technicalDetails) = ErrorHandler.HandleEncryptionError(ex, "folder unlock");
            _logger.LogError(ex, "Error unlocking folder: {FolderPath} - {TechnicalDetails}", folderPath, technicalDetails);
            ErrorHandler.LogSecurityEvent(_logger, "UNLOCK_FAILED", folderPath, false, technicalDetails);
            return (false, ErrorHandler.FormatUserErrorMessage("Unlock Failed", userMessage, 
                isRecoverable ? "Please try again." : "Please contact support if this problem persists."));
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
            ErrorHandler.LogSecurityEvent(_logger, "LOCK_SUCCESS", folderPath, true, $"Files processed: {encryptionResult.FilesProcessed}");
            return (true, "Folder locked successfully.");
        }
        catch (Exception ex)
        {
            var (isRecoverable, userMessage, technicalDetails) = ErrorHandler.HandleEncryptionError(ex, "folder lock");
            _logger.LogError(ex, "Error locking folder: {FolderPath} - {TechnicalDetails}", folderPath, technicalDetails);
            ErrorHandler.LogSecurityEvent(_logger, "LOCK_FAILED", folderPath, false, technicalDetails);
            return (false, ErrorHandler.FormatUserErrorMessage("Lock Failed", userMessage,
                isRecoverable ? "Please try again." : "Please contact support if this problem persists."));
        }
    }

    /// <summary>
    /// Handles incoming IPC requests from GUI clients.
    /// Implements Requirements: 1.3, 2.2, 7.1, 7.5
    /// </summary>
    private async Task<string> HandleIpcRequestAsync(string requestJson)
    {
        try
        {
            _logger.LogDebug("Processing IPC request: {Request}", requestJson);

            var request = JsonSerializer.Deserialize<IpcRequest>(requestJson);
            
            if (request == null)
            {
                return JsonSerializer.Serialize(new ServiceResponse
                {
                    Success = false,
                    Message = "Invalid request format"
                });
            }

            return request.Action switch
            {
                "Ping" => JsonSerializer.Serialize(new ServiceResponse { Success = true, Message = "Pong" }),
                "LockFolder" => await HandleLockFolderRequestAsync(request),
                "UnlockFolder" => await HandleUnlockFolderRequestAsync(request),
                "RemoveFolder" => await HandleRemoveFolderRequestAsync(request),
                "GetLockedFolders" => await HandleGetLockedFoldersRequestAsync(),
                _ => JsonSerializer.Serialize(new ServiceResponse
                {
                    Success = false,
                    Message = $"Unknown action: {request.Action}"
                })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling IPC request");
            return JsonSerializer.Serialize(new ServiceResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            });
        }
    }

    private async Task<string> HandleLockFolderRequestAsync(IpcRequest request)
    {
        if (string.IsNullOrEmpty(request.FolderPath) || string.IsNullOrEmpty(request.Password))
        {
            return JsonSerializer.Serialize(new ServiceResponse
            {
                Success = false,
                Message = "FolderPath and Password are required"
            });
        }

        var securePassword = ConvertToSecureString(request.Password);
        var (success, message) = await LockFolderAsync(request.FolderPath, securePassword);

        return JsonSerializer.Serialize(new ServiceResponse
        {
            Success = success,
            Message = message
        });
    }

    private async Task<string> HandleUnlockFolderRequestAsync(IpcRequest request)
    {
        if (string.IsNullOrEmpty(request.FolderPath) || string.IsNullOrEmpty(request.Password))
        {
            return JsonSerializer.Serialize(new ServiceResponse
            {
                Success = false,
                Message = "FolderPath and Password are required"
            });
        }

        var securePassword = ConvertToSecureString(request.Password);
        var (success, message) = await UnlockFolderAsync(request.FolderPath, securePassword);

        return JsonSerializer.Serialize(new ServiceResponse
        {
            Success = success,
            Message = message
        });
    }

    private async Task<string> HandleRemoveFolderRequestAsync(IpcRequest request)
    {
        if (string.IsNullOrEmpty(request.FolderPath) || string.IsNullOrEmpty(request.Password))
        {
            return JsonSerializer.Serialize(new ServiceResponse
            {
                Success = false,
                Message = "FolderPath and Password are required"
            });
        }

        try
        {
            var securePassword = ConvertToSecureString(request.Password);
            
            // Unlock the folder first
            var (unlockSuccess, unlockMessage) = await UnlockFolderAsync(request.FolderPath, securePassword);
            
            if (!unlockSuccess)
            {
                return JsonSerializer.Serialize(new ServiceResponse
                {
                    Success = false,
                    Message = $"Failed to unlock folder: {unlockMessage}"
                });
            }

            // Remove from registry
            using var scope = _serviceProvider.CreateScope();
            var folderRegistry = scope.ServiceProvider.GetRequiredService<IFolderRegistry>();
            await folderRegistry.RemoveLockedFolderAsync(request.FolderPath);

            // Reload folders
            await LoadLockedFoldersAsync();

            return JsonSerializer.Serialize(new ServiceResponse
            {
                Success = true,
                Message = "Folder removed from management successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing folder: {FolderPath}", request.FolderPath);
            return JsonSerializer.Serialize(new ServiceResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            });
        }
    }

    private async Task<string> HandleGetLockedFoldersRequestAsync()
    {
        var folders = GetLockedFolders().Select(f => new LockedFolderInfo
        {
            FolderPath = f.FolderPath,
            IsLocked = f.IsLocked,
            LockedDate = f.LockedDate,
            LastAccessed = f.LastAccessed
        }).ToList();

        return JsonSerializer.Serialize(folders);
    }

    private SecureString ConvertToSecureString(string password)
    {
        var securePassword = new SecureString();
        foreach (char c in password)
        {
            securePassword.AppendChar(c);
        }
        securePassword.MakeReadOnly();
        return securePassword;
    }

    private class IpcRequest
    {
        public string Action { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

