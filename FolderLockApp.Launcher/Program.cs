using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using FolderLockApp.Core.Helpers;

namespace FolderLockApp.Launcher;

/// <summary>
/// Unified launcher for FolderLock application.
/// Manages service installation, startup, and GUI launch.
/// </summary>
class Program
{
    private const string ServiceName = "FolderLockApp Encryption Service";
    private static string? _serviceExePath;
    private static string? _guiExePath;

    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  FolderLock Application Launcher");
            Console.WriteLine("===========================================\n");

            // Verify code integrity
            if (!VerifyCodeIntegrity())
            {
                ShowError("Code integrity verification failed!\n\n" +
                         "The application files may have been tampered with.\n" +
                         "Please reinstall from a trusted source.");
                return;
            }

            Console.WriteLine("✓ Code integrity verified\n");

            // Check admin privileges
            if (!AdminPrivilegeHelper.IsRunningAsAdmin())
            {
                Console.WriteLine("Administrator privileges required. Requesting elevation...");
                if (AdminPrivilegeHelper.RestartAsAdmin(args))
                {
                    return;
                }
                else
                {
                    ShowError("Failed to obtain administrator privileges.\n\n" +
                             "Please right-click and select 'Run as administrator'.");
                    return;
                }
            }

            Console.WriteLine("✓ Running with administrator privileges\n");

            // Locate component executables
            if (!LocateComponents())
            {
                ShowError("Failed to locate application components.\n\n" +
                         "Please ensure all files are present.");
                return;
            }

            // Check and install service if needed
            Console.WriteLine("Checking background service...");
            EnsureServiceInstalled();

            // Start service if not running
            EnsureServiceRunning();

            // Launch GUI
            Console.WriteLine("\nLaunching GUI application...");
            LaunchGUI();

            Console.WriteLine("\n✓ FolderLock launched successfully!");
            Console.WriteLine("\nYou can close this window.");
        }
        catch (Exception ex)
        {
            ShowError($"Launcher failed:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
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
                Console.WriteLine("WARNING: Code integrity check failed (DEBUG mode - continuing anyway)");
                Console.WriteLine($"Reason: {result.ErrorMessage}");
            }
            return true;
            #else
            return result.IsValid;
            #endif
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Code integrity verification error: {ex.Message}");
            #if DEBUG
            return true;
            #else
            return false;
            #endif
        }
    }

    private static bool LocateComponents()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        
        // Look for service executable
        var servicePaths = new[]
        {
            Path.Combine(baseDir, "FolderLockApp.Service.exe"),
            Path.Combine(baseDir, "..", "FolderLockApp.Service", "bin", "Release", "net8.0", "FolderLockApp.Service.exe"),
            Path.Combine(baseDir, "..", "FolderLockApp.Service", "bin", "Debug", "net8.0", "FolderLockApp.Service.exe")
        };

        foreach (var path in servicePaths)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                _serviceExePath = fullPath;
                Console.WriteLine($"✓ Found service: {fullPath}");
                break;
            }
        }

        // Look for GUI executable
        var guiPaths = new[]
        {
            Path.Combine(baseDir, "FolderLockApp.GUI.exe"),
            Path.Combine(baseDir, "..", "FolderLockApp.GUI", "bin", "Release", "net8.0-windows", "FolderLockApp.GUI.exe"),
            Path.Combine(baseDir, "..", "FolderLockApp.GUI", "bin", "Debug", "net8.0-windows", "FolderLockApp.GUI.exe")
        };

        foreach (var path in guiPaths)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                _guiExePath = fullPath;
                Console.WriteLine($"✓ Found GUI: {fullPath}");
                break;
            }
        }

        if (string.IsNullOrEmpty(_serviceExePath))
        {
            Console.WriteLine("✗ Service executable not found");
            return false;
        }

        if (string.IsNullOrEmpty(_guiExePath))
        {
            Console.WriteLine("✗ GUI executable not found");
            return false;
        }

        return true;
    }

    private static void EnsureServiceInstalled()
    {
        if (IsServiceInstalled())
        {
            Console.WriteLine("✓ Service is already installed");
            return;
        }

        Console.WriteLine("Installing background service...");
        
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"create \"{ServiceName}\" binPath=\"{_serviceExePath}\" start=auto",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                process.WaitForExit();
                
                if (process.ExitCode == 0)
                {
                    Console.WriteLine("✓ Service installed successfully");
                }
                else
                {
                    var error = process.StandardError.ReadToEnd();
                    Console.WriteLine($"✗ Service installation failed: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Service installation error: {ex.Message}");
        }
    }

    private static void EnsureServiceRunning()
    {
        try
        {
            using var service = new ServiceController(ServiceName);
            
            if (service.Status == ServiceControllerStatus.Running)
            {
                Console.WriteLine("✓ Service is already running");
                return;
            }

            Console.WriteLine("Starting background service...");
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
            Console.WriteLine("✓ Service started successfully");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("✗ Service not found. Installation may have failed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to start service: {ex.Message}");
            Console.WriteLine("You may need to start it manually from Services (services.msc)");
        }
    }

    private static bool IsServiceInstalled()
    {
        try
        {
            using var service = new ServiceController(ServiceName);
            var status = service.Status; // This will throw if service doesn't exist
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void LaunchGUI()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _guiExePath,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(_guiExePath)
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            ShowError($"Failed to launch GUI:\n\n{ex.Message}");
        }
    }

    private static void ShowError(string message)
    {
        Console.WriteLine($"\nERROR: {message}");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
