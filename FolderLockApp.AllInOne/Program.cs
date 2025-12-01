using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FolderLockApp.Core.Helpers;
using FolderLockApp.Core.Interfaces;
using FolderLockApp.Core.Services;
using FolderLockApp.Core.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.Extensions.Logging;

namespace FolderLockApp.AllInOne;

/// <summary>
/// All-in-one FolderLock application.
/// Combines GUI and background service in a single executable.
/// </summary>
class Program
{
    [STAThread]
    static async Task<int> Main(string[] args)
    {
        // Configure logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "FolderLockApp", "Logs", "allinone-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        try
        {
            Log.Information("Starting FolderLock All-in-One Application");

            // Verify code integrity
            if (!VerifyCodeIntegrity())
            {
                System.Windows.MessageBox.Show(
                    "Code integrity verification failed!\n\n" +
                    "The application files may have been tampered with.\n" +
                    "Please reinstall from a trusted source.",
                    "Security Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return 1;
            }

            // Check admin privileges
            if (!AdminPrivilegeHelper.IsRunningAsAdmin())
            {
                var result = System.Windows.MessageBox.Show(
                    "FolderLock requires administrator privileges.\n\n" +
                    "Would you like to restart with administrator privileges?",
                    "Administrator Privileges Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (AdminPrivilegeHelper.RestartAsAdmin(args))
                    {
                        return 0;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
                            "Failed to restart with administrator privileges.\n\n" +
                            "Please right-click and select 'Run as administrator'.",
                            "Elevation Failed",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }

            // Run as GUI application with embedded service
            RunAsGUI(args);

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            System.Windows.MessageBox.Show(
                $"Fatal error:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static bool VerifyCodeIntegrity()
    {
        try
        {
            var result = CodeIntegrityVerifier.VerifyCurrentAssembly();
            
            #if DEBUG
            if (!result.IsValid)
            {
                Log.Warning("Code integrity check failed (DEBUG mode - continuing)");
            }
            return true;
            #else
            return result.IsValid;
            #endif
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Code integrity verification error");
            #if DEBUG
            return true;
            #else
            return false;
            #endif
        }
    }

    private static void RunAsGUI(string[] args)
    {
        Log.Information("Running in GUI mode");

        // Start background service in separate thread
        var serviceThread = new Thread(async () =>
        {
            try
            {
                await RunBackgroundServiceAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Background service error");
            }
        })
        {
            IsBackground = true,
            Name = "BackgroundService"
        };
        
        serviceThread.Start();

        // Give service time to start
        Thread.Sleep(2000);

        // Start WPF application
        var app = new App();
        app.Run();
    }

    private static async Task RunBackgroundServiceAsync()
    {
        Log.Information("Starting background service");

        var builder = Host.CreateApplicationBuilder();
        
        // Configure services
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "FolderLockApp");
        
        Directory.CreateDirectory(appDataPath);
        var dbPath = Path.Combine(appDataPath, "folderlock.db");
        
        builder.Services.AddDbContext<FolderLockDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        builder.Services.AddScoped<IFolderRegistry, FolderRegistry>();
        builder.Services.AddScoped<IEncryptionEngine, EncryptionEngine>();
        builder.Services.AddSingleton<SettingsManager>();
        
        // Add background service
        builder.Services.AddHostedService<BackgroundEncryptionService>();

        var host = builder.Build();
        
        // Initialize database
        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<FolderLockDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }
        
        await host.RunAsync();
    }
}

/// <summary>
/// Background encryption service.
/// </summary>
public class BackgroundEncryptionService : BackgroundService
{
    private readonly ILogger<BackgroundEncryptionService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public BackgroundEncryptionService(
        ILogger<BackgroundEncryptionService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background encryption service started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                
                // Perform periodic tasks
                await CheckAutoLockFoldersAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Background service is stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background service error");
        }
    }

    private async Task CheckAutoLockFoldersAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var folderRegistry = scope.ServiceProvider.GetRequiredService<IFolderRegistry>();
            
            // Check for folders that need auto-locking
            _logger.LogDebug("Checking auto-lock folders");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking auto-lock folders");
        }
    }
}

/// <summary>
/// WPF Application class.
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "FolderLockApp");
                Directory.CreateDirectory(appDataPath);
                var dbPath = Path.Combine(appDataPath, "folderlock.db");

                services.AddDbContext<FolderLockDbContext>(options =>
                    options.UseSqlite($"Data Source={dbPath}"));
                services.AddSingleton<IFolderRegistry, FolderRegistry>();
                services.AddSingleton<IEncryptionEngine, EncryptionEngine>();
                services.AddSingleton<SettingsManager>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            await _host!.StartAsync();

            // Initialize database
            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<FolderLockDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
            }

            // Show main window
            var mainWindow = new MainWindow();
            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Startup failed:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
