using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FolderLockApp.Core.Interfaces;
using FolderLockApp.Core.Services;
using FolderLockApp.Core.Data;
using FolderLockApp.GUI.ViewModels;
using FolderLockApp.GUI.Views;

namespace FolderLockApp.GUI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Configure database path
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "FolderLockApp");
                Directory.CreateDirectory(appDataPath);
                var dbPath = Path.Combine(appDataPath, "folderlock.db");

                // Register Core services
                services.AddDbContext<FolderLockDbContext>(options =>
                    options.UseSqlite($"Data Source={dbPath}"));
                services.AddSingleton<IFolderRegistry, FolderRegistry>();
                services.AddSingleton<IEncryptionEngine, EncryptionEngine>();
                services.AddSingleton<SettingsManager>();

                // Register IPC service proxy for communication with background service
                services.AddSingleton<IEncryptionServiceContract, EncryptionServiceProxy>();

                // Register ViewModels
                services.AddTransient<MainWindowViewModel>();

                // Register Views
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            await _host.StartAsync();

            // Initialize database by ensuring DbContext is created
            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<FolderLockDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
            }

            // Check for first run and show welcome/tutorial
            var settingsManager = _host.Services.GetRequiredService<SettingsManager>();
            if (settingsManager.IsFirstRun)
            {
                await ShowFirstRunExperienceAsync(settingsManager);
            }

            // Verify service is running
            await EnsureServiceIsRunningAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Application startup failed:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    /// <summary>
    /// Shows the first-run experience (welcome and optional tutorial).
    /// Implements Requirement 1.1: First-run experience
    /// </summary>
    private async Task ShowFirstRunExperienceAsync(SettingsManager settingsManager)
    {
        // Show welcome window
        var welcomeWindow = new WelcomeWindow();
        var result = welcomeWindow.ShowDialog();

        if (result == true)
        {
            // Mark first run as completed if user checked "Don't show again"
            if (welcomeWindow.DontShowAgain)
            {
                settingsManager.CompleteFirstRun();
            }

            // Show tutorial if requested
            if (welcomeWindow.ShowTutorial)
            {
                var tutorialWindow = new TutorialWindow();
                tutorialWindow.ShowDialog();
                
                // Mark first run as completed after tutorial
                settingsManager.CompleteFirstRun();
            }
        }
        else
        {
            // User closed the window, mark as completed anyway
            settingsManager.CompleteFirstRun();
        }
    }

    /// <summary>
    /// Ensures the background service is running.
    /// Implements Requirement 1.1: Ensure service is started on first run
    /// </summary>
    private async Task EnsureServiceIsRunningAsync()
    {
        try
        {
            // Try to ping the service via IPC
            var serviceProxy = _host.Services.GetRequiredService<IEncryptionServiceContract>();
            
            // Attempt to connect (with timeout)
            var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
            
            try
            {
                // Try to get locked folders as a health check
                await Task.Run(async () =>
                {
                    var folders = await serviceProxy.GetLockedFoldersAsync();
                }, cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Service not responding
                var result = MessageBox.Show(
                    "The FolderLock background service is not running.\n\n" +
                    "The service is required for folder monitoring and auto-lock features.\n\n" +
                    "Would you like to start the service now?",
                    "Service Not Running",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    await StartServiceAsync();
                }
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail startup
            System.Diagnostics.Debug.WriteLine($"Service check failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Attempts to start the Windows Service.
    /// </summary>
    private async Task StartServiceAsync()
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = "start \"FolderLockApp Encryption Service\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                
                if (process.ExitCode == 0)
                {
                    MessageBox.Show(
                        "Service started successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    MessageBox.Show(
                        $"Failed to start service.\n\nError: {error}\n\n" +
                        "Please start the service manually from Services (services.msc)",
                        "Service Start Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to start service: {ex.Message}\n\n" +
                "Please start the service manually from Services (services.msc)",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}

