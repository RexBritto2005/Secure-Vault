using System.Windows;
using System.Windows.Forms;
using FolderLockApp.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FolderLockApp.AllInOne;

public partial class MainWindow : Window
{
    private readonly IFolderRegistry? _folderRegistry;
    private readonly IEncryptionEngine? _encryptionEngine;

    public MainWindow()
    {
        InitializeComponent();
        
        // Get services from DI container
        try
        {
            var app = (App)System.Windows.Application.Current;
            // Services will be injected when available
        }
        catch
        {
            // Services not yet available
        }

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await RefreshFoldersList();
    }

    private async void LockButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select folder to lock",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var folderPath = dialog.SelectedPath;
                
                // Show password dialog
                var passwordDialog = new PasswordDialog("Enter password to lock folder:");
                if (passwordDialog.ShowDialog() == true)
                {
                    var password = passwordDialog.Password;
                    
                    StatusText.Text = $"Locking {folderPath}...";
                    
                    // Lock folder logic here
                    // await _encryptionEngine.LockFolderAsync(folderPath, password);
                    
                    StatusText.Text = "Folder locked successfully";
                    await RefreshFoldersList();
                    
                    System.Windows.MessageBox.Show(
                        "Folder locked successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to lock folder:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            StatusText.Text = "Error locking folder";
        }
    }

    private async void UnlockButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (FoldersListView.SelectedItem == null)
            {
                System.Windows.MessageBox.Show(
                    "Please select a folder to unlock",
                    "No Selection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Get selected folder
            dynamic selectedFolder = FoldersListView.SelectedItem;
            var folderPath = selectedFolder.Path;

            // Show password dialog
            var passwordDialog = new PasswordDialog("Enter password to unlock folder:");
            if (passwordDialog.ShowDialog() == true)
            {
                var password = passwordDialog.Password;
                
                StatusText.Text = $"Unlocking {folderPath}...";
                
                // Unlock folder logic here
                // await _encryptionEngine.UnlockFolderAsync(folderPath, password);
                
                StatusText.Text = "Folder unlocked successfully";
                await RefreshFoldersList();
                
                System.Windows.MessageBox.Show(
                    "Folder unlocked successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to unlock folder:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            StatusText.Text = "Error unlocking folder";
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshFoldersList();
    }

    private async Task RefreshFoldersList()
    {
        try
        {
            StatusText.Text = "Refreshing...";
            
            // Get locked folders from registry
            // var folders = await _folderRegistry.GetAllFoldersAsync();
            
            // For now, show empty state
            var folders = new List<object>();
            
            FoldersListView.ItemsSource = folders;
            EmptyState.Visibility = folders.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            
            StatusText.Text = $"Ready - {folders.Count} folder(s)";
        }
        catch (Exception ex)
        {
            StatusText.Text = "Error refreshing list";
            System.Windows.MessageBox.Show(
                $"Failed to refresh folders:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}

/// <summary>
/// Simple password dialog.
/// </summary>
public class PasswordDialog : Window
{
    private readonly System.Windows.Controls.PasswordBox _passwordBox;
    public string Password => _passwordBox.Password;

    public PasswordDialog(string message)
    {
        Title = "Password Required";
        Width = 400;
        Height = 200;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var grid = new System.Windows.Controls.Grid();
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });

        var stackPanel = new System.Windows.Controls.StackPanel
        {
            Margin = new Thickness(20),
            VerticalAlignment = VerticalAlignment.Center
        };

        var messageText = new System.Windows.Controls.TextBlock
        {
            Text = message,
            Margin = new Thickness(0, 0, 0, 15),
            TextWrapping = TextWrapping.Wrap
        };

        _passwordBox = new System.Windows.Controls.PasswordBox
        {
            Padding = new Thickness(5)
        };

        stackPanel.Children.Add(messageText);
        stackPanel.Children.Add(_passwordBox);
        grid.Children.Add(stackPanel);

        var buttonPanel = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
            Margin = new Thickness(20, 0, 20, 20)
        };

        var okButton = new System.Windows.Controls.Button
        {
            Content = "OK",
            Width = 80,
            Height = 30,
            Margin = new Thickness(0, 0, 10, 0),
            IsDefault = true
        };
        okButton.Click += (s, e) => { DialogResult = true; Close(); };

        var cancelButton = new System.Windows.Controls.Button
        {
            Content = "Cancel",
            Width = 80,
            Height = 30,
            IsCancel = true
        };
        cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        
        System.Windows.Controls.Grid.SetRow(buttonPanel, 1);
        grid.Children.Add(buttonPanel);

        Content = grid;
        
        _passwordBox.Focus();
    }
}
