using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using FolderLockApp.Core.Services;
using System.Text.Json;

namespace FolderLockApp.ShellExtension;

/// <summary>
/// Shell context menu extension for FolderLockApp.
/// Adds "Lock Folder" and "Unlock Folder" options to Windows Explorer context menu.
/// Implements Requirements: 4.1, 4.2, 4.3, 4.4
/// </summary>
[ComVisible(true)]
[COMServerAssociation(AssociationType.Directory)]
public class FolderLockContextMenu : SharpContextMenu
{
    private const string LockMenuText = "🔒 Lock Folder with FolderLock";
    private const string UnlockMenuText = "🔓 Unlock Folder with FolderLock";

    /// <summary>
    /// Determines whether the context menu should be shown for the selected items.
    /// </summary>
    protected override bool CanShowMenu()
    {
        // Only show menu for single folder selection
        return SelectedItemPaths.Count() == 1 && Directory.Exists(SelectedItemPaths.First());
    }

    /// <summary>
    /// Creates the context menu items.
    /// Implements Requirement 4.1: Display context menu options
    /// Implements Requirement 4.3: Show "Unlock Folder" for locked folders
    /// </summary>
    protected override ContextMenuStrip CreateMenu()
    {
        var menu = new ContextMenuStrip();
        var folderPath = SelectedItemPaths.First();

        try
        {
            // Check if folder is locked by looking for .locked files
            bool isLocked = HasLockedFiles(folderPath);

            if (isLocked)
            {
                // Show "Unlock Folder" option
                var unlockItem = new ToolStripMenuItem
                {
                    Text = UnlockMenuText,
                    Image = Properties.Resources.UnlockIcon // We'll create this
                };
                unlockItem.Click += (sender, args) => UnlockFolder(folderPath);
                menu.Items.Add(unlockItem);
            }
            else
            {
                // Show "Lock Folder" option
                var lockItem = new ToolStripMenuItem
                {
                    Text = LockMenuText,
                    Image = Properties.Resources.LockIcon // We'll create this
                };
                lockItem.Click += (sender, args) => LockFolder(folderPath);
                menu.Items.Add(lockItem);
            }
        }
        catch (Exception ex)
        {
            // If there's an error, show an error menu item
            var errorItem = new ToolStripMenuItem
            {
                Text = $"FolderLock Error: {ex.Message}",
                Enabled = false
            };
            menu.Items.Add(errorItem);
        }

        return menu;
    }

    /// <summary>
    /// Checks if a folder contains locked (.locked) files.
    /// </summary>
    private bool HasLockedFiles(string folderPath)
    {
        try
        {
            return Directory.EnumerateFiles(folderPath, "*.locked", SearchOption.AllDirectories).Any();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Locks a folder by communicating with the background service.
    /// Implements Requirement 4.2: Invoke encryption workflow from context menu
    /// </summary>
    private async void LockFolder(string folderPath)
    {
        try
        {
            // Show password input dialog
            using var passwordForm = new PasswordInputForm("Enter password to lock this folder:");
            if (passwordForm.ShowDialog() != DialogResult.OK)
                return;

            string password = passwordForm.Password;

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Password cannot be empty.", "FolderLock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirm password
            using var confirmForm = new PasswordInputForm("Confirm password:");
            if (confirmForm.ShowDialog() != DialogResult.OK)
                return;

            if (password != confirmForm.Password)
            {
                MessageBox.Show("Passwords do not match.", "FolderLock", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Communicate with service via Named Pipes
            var client = new NamedPipeClient();
            var request = new
            {
                Action = "LockFolder",
                FolderPath = folderPath,
                Password = password
            };

            var requestJson = JsonSerializer.Serialize(request);
            var responseJson = await client.SendRequestAsync(requestJson);
            var response = JsonSerializer.Deserialize<ServiceResponse>(responseJson);

            if (response?.Success == true)
            {
                MessageBox.Show($"Folder locked successfully!\n\n{folderPath}", 
                    "FolderLock", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Failed to lock folder:\n{response?.Message}", 
                    "FolderLock", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error locking folder:\n{ex.Message}\n\nMake sure the FolderLock service is running.", 
                "FolderLock", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Unlocks a folder by communicating with the background service.
    /// Implements Requirement 4.4: Display password dialog for unlock
    /// </summary>
    private async void UnlockFolder(string folderPath)
    {
        try
        {
            // Show password input dialog
            using var passwordForm = new PasswordInputForm("Enter password to unlock this folder:");
            if (passwordForm.ShowDialog() != DialogResult.OK)
                return;

            string password = passwordForm.Password;

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Password cannot be empty.", "FolderLock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Communicate with service via Named Pipes
            var client = new NamedPipeClient();
            var request = new
            {
                Action = "UnlockFolder",
                FolderPath = folderPath,
                Password = password
            };

            var requestJson = JsonSerializer.Serialize(request);
            var responseJson = await client.SendRequestAsync(requestJson);
            var response = JsonSerializer.Deserialize<ServiceResponse>(responseJson);

            if (response?.Success == true)
            {
                MessageBox.Show($"Folder unlocked successfully!\n\n{folderPath}", 
                    "FolderLock", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Failed to unlock folder:\n{response?.Message}", 
                    "FolderLock", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error unlocking folder:\n{ex.Message}\n\nMake sure the FolderLock service is running.", 
                "FolderLock", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private class ServiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
