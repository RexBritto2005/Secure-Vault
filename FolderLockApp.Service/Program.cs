using FolderLockApp.Service;
using FolderLockApp.Core.Interfaces;
using FolderLockApp.Core.Services;
using FolderLockApp.Core.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

// Configure Serilog with enhanced logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(
        path: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
            "FolderLockApp", "Logs", "service-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10_485_760, // 10 MB per file
        rollOnFileSizeLimit: true,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
            "FolderLockApp", "Logs", "security-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 90, // Keep security logs longer
        fileSizeLimitBytes: 10_485_760,
        rollOnFileSizeLimit: true,
        restrictedToMinimumLevel: LogEventLevel.Warning,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [Security] {Message:lj}{NewLine}{Exception}")
    .WriteTo.EventLog(
        source: "FolderLockApp Service",
        logName: "Application",
        restrictedToMinimumLevel: LogEventLevel.Warning)
    .CreateLogger();

try
{
    Log.Information("Starting FolderLockApp Service");

    var builder = Host.CreateApplicationBuilder(args);
    
    // Configure Windows Service
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "FolderLockApp Encryption Service";
    });

    // Add Serilog
    builder.Services.AddSerilog();

    // Configure database
    var appDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "FolderLockApp");
    
    Directory.CreateDirectory(appDataPath);
    
    var dbPath = Path.Combine(appDataPath, "folderlock.db");
    
    builder.Services.AddDbContext<FolderLockDbContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));

    // Register services
    builder.Services.AddScoped<IFolderRegistry, FolderRegistry>();
    builder.Services.AddScoped<IEncryptionEngine, EncryptionEngine>();
    
    // Add the background service
    builder.Services.AddHostedService<EncryptionBackgroundService>();

    var host = builder.Build();
    
    // Initialize database
    using (var scope = host.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<FolderLockDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        Log.Information("Database initialized successfully");
    }
    
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "FolderLockApp Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
