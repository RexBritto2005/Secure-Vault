using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                // Register Core services
                services.AddDbContext<FolderLockDbContext>();
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
        await _host.StartAsync();

        // Initialize database
        var folderRegistry = _host.Services.GetRequiredService<IFolderRegistry>();
        await folderRegistry.InitializeDatabaseAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}

