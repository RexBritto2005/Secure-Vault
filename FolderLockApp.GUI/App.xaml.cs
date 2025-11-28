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

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}

