using System.Collections.ObjectModel;
using System.Security;
using System.Windows;
using System.Windows.Input;
using FolderLockApp.Core.Interfaces;
using FolderLockApp.Core.Models;

namespace FolderLockApp.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IFolderRegistry _folderRegistry;
    private readonly IEncryptionEngine _encryptionEngine;
    private LockedFolderEntry? _selectedFolder;
    private bool _isOperationInProgress;
    private int _progressValue;
    private string _statusMessage = string.Empty;

    public MainWindowViewModel(IFolderRegistry folderRegistry, IEncryptionEngine encryptionEngine)
    {
        _folderRegistry = folderRegistry;
        _encryptionEngine = encryptionEngine;
        LockedFolders = new ObservableCollection<LockedFolderEntry>();

        // Initialize commands
        LockFolderCommand = new RelayCommand(async () => await LockFolderAsync(), () => !IsOperationInProgress);
        UnlockFolderCommand = new RelayCommand(async () => await UnlockFolderAsync(), () => SelectedFolder != null && !IsOperationInProgress);
        RemoveFolderCommand = new RelayCommand(async () => await RemoveFolderAsync(), () => SelectedFolder != null && !IsOperationInProgress);
        RefreshCommand = new RelayCommand(async () => await LoadLockedFoldersAsync(), () => !IsOperationInProgress);
    }

    public ObservableCollection<LockedFolderEntry> LockedFolders { get; }

    public LockedFolderEntry? SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            if (SetProperty(ref _selectedFolder, value))
            {
                ((RelayCommand)UnlockFolderCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RemoveFolderCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsOperationInProgress
    {
        get => _isOperationInProgress;
        set
        {
            if (SetProperty(ref _isOperationInProgress, value))
            {
                ((RelayCommand)LockFolderCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UnlockFolderCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RemoveFolderCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RefreshCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public int ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand LockFolderCommand { get; }
    public ICommand UnlockFolderCommand { get; }
    public ICommand RemoveFolderCommand { get; }
    public ICommand RefreshCommand { get; }

    public async Task LoadLockedFoldersAsync()
    {
        try
        {
            StatusMessage = "Loading locked folders...";
            var folders = await _folderRegistry.GetAllLockedFoldersAsync();
            LockedFolders.Clear();
            foreach (var folder in folders)
            {
                LockedFolders.Add(folder);
            }
            StatusMessage = $"Loaded {LockedFolders.Count} locked folder(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading folders: {ex.Message}";
            MessageBox.Show($"Failed to load locked folders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LockFolderAsync()
    {
        try
        {
            // Show folder browser dialog
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Folder to Lock"
            };

            if (dialog.ShowDialog() != true)
                return;

            string folderPath = dialog.FolderName;

            // Check if folder is already locked
            var existingEntry = await _folderRegistry.GetLockedFolderAsync(folderPath);
            if (existingEntry != null)
            {
                MessageBox.Show("This folder is already locked.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Show password dialog
            var passwordDialog = new Views.PasswordDialog
            {
                Message = "Enter a password to lock this folder:",
                RequireConfirmation = true,
                Owner = Application.Current.MainWindow
            };

            if (passwordDialog.ShowDialog() != true || passwordDialog.Password == null)
                return;

            IsOperationInProgress = true;
            ProgressValue = 0;
            StatusMessage = "Encrypting folder...";

            var progress = new Progress<int>(value =>
            {
                ProgressValue = value;
            });

            // Encrypt the folder
            var result = await _encryptionEngine.EncryptFolderAsync(folderPath, passwordDialog.Password, progress);

            if (!result.Success)
            {
                MessageBox.Show($"Encryption failed: {result.ErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Encryption failed";
                return;
            }

            // Generate password hash and salt
            byte[] salt = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            byte[] passwordHash = _encryptionEngine.HashPassword(passwordDialog.Password, salt);

            // Register in folder registry
            await _folderRegistry.RegisterLockedFolderAsync(folderPath, passwordHash, salt);

            StatusMessage = $"Successfully locked folder: {folderPath}";
            MessageBox.Show($"Folder locked successfully!\nFiles processed: {result.FilesProcessed}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reload the list
            await LoadLockedFoldersAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsOperationInProgress = false;
            ProgressValue = 0;
        }
    }

    private async Task UnlockFolderAsync()
    {
        if (SelectedFolder == null)
            return;

        try
        {
            // Show password dialog
            var passwordDialog = new Views.PasswordDialog
            {
                Message = $"Enter password to unlock:\n{SelectedFolder.FolderPath}",
                RequireConfirmation = false,
                Owner = Application.Current.MainWindow
            };

            if (passwordDialog.ShowDialog() != true || passwordDialog.Password == null)
                return;

            // Verify password
            bool isPasswordCorrect = _encryptionEngine.VerifyPassword(
                passwordDialog.Password,
                SelectedFolder.PasswordHash,
                SelectedFolder.Salt);

            if (!isPasswordCorrect)
            {
                MessageBox.Show("Incorrect password.", "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsOperationInProgress = true;
            ProgressValue = 0;
            StatusMessage = "Decrypting folder...";

            var progress = new Progress<int>(value =>
            {
                ProgressValue = value;
            });

            // Decrypt the folder
            var result = await _encryptionEngine.DecryptFolderAsync(SelectedFolder.FolderPath, passwordDialog.Password, progress);

            if (!result.Success)
            {
                MessageBox.Show($"Decryption failed: {result.ErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Decryption failed";
                return;
            }

            // Update last accessed time
            await _folderRegistry.UpdateLastAccessAsync(SelectedFolder.FolderPath);

            StatusMessage = $"Successfully unlocked folder: {SelectedFolder.FolderPath}";
            MessageBox.Show($"Folder unlocked successfully!\nFiles processed: {result.FilesProcessed}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reload the list
            await LoadLockedFoldersAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsOperationInProgress = false;
            ProgressValue = 0;
        }
    }

    private async Task RemoveFolderAsync()
    {
        if (SelectedFolder == null)
            return;

        try
        {
            var confirmResult = MessageBox.Show(
                $"Are you sure you want to remove this folder from management?\n\n{SelectedFolder.FolderPath}\n\nThe folder will be decrypted first.",
                "Confirm Removal",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes)
                return;

            // Show password dialog for confirmation
            var passwordDialog = new Views.PasswordDialog
            {
                Message = $"Enter password to confirm removal:\n{SelectedFolder.FolderPath}",
                RequireConfirmation = false,
                Owner = Application.Current.MainWindow
            };

            if (passwordDialog.ShowDialog() != true || passwordDialog.Password == null)
                return;

            // Verify password
            bool isPasswordCorrect = _encryptionEngine.VerifyPassword(
                passwordDialog.Password,
                SelectedFolder.PasswordHash,
                SelectedFolder.Salt);

            if (!isPasswordCorrect)
            {
                MessageBox.Show("Incorrect password.", "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsOperationInProgress = true;
            ProgressValue = 0;
            StatusMessage = "Decrypting and removing folder...";

            var progress = new Progress<int>(value =>
            {
                ProgressValue = value;
            });

            // Decrypt the folder first
            var result = await _encryptionEngine.DecryptFolderAsync(SelectedFolder.FolderPath, passwordDialog.Password, progress);

            if (!result.Success)
            {
                MessageBox.Show($"Decryption failed: {result.ErrorMessage}\n\nFolder will not be removed from registry.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Removal failed";
                return;
            }

            // Remove from registry
            await _folderRegistry.RemoveLockedFolderAsync(SelectedFolder.FolderPath);

            StatusMessage = $"Successfully removed folder: {SelectedFolder.FolderPath}";
            MessageBox.Show("Folder has been decrypted and removed from management.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reload the list
            await LoadLockedFoldersAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsOperationInProgress = false;
            ProgressValue = 0;
        }
    }
}
