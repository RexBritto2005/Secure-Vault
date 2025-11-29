using System;

namespace FolderLockApp.ShellExtension;

/// <summary>
/// Console application for registering/unregistering the shell extension.
/// Must be run as Administrator.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("FolderLock Shell Extension Registration Tool");
        Console.WriteLine("=============================================\n");

        if (!ShellExtensionRegistration.IsAdministrator())
        {
            Console.WriteLine("ERROR: This tool must be run as Administrator!");
            Console.WriteLine("Please right-click and select 'Run as Administrator'.");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
            return;
        }

        if (args.Length == 0)
        {
            ShowUsage();
            return;
        }

        string command = args[0].ToLower();

        switch (command)
        {
            case "register":
            case "/register":
            case "-register":
                RegisterExtension();
                break;

            case "unregister":
            case "/unregister":
            case "-unregister":
                UnregisterExtension();
                break;

            default:
                Console.WriteLine($"Unknown command: {command}\n");
                ShowUsage();
                break;
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static void ShowUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  FolderLockApp.ShellExtension.exe register   - Register the shell extension");
        Console.WriteLine("  FolderLockApp.ShellExtension.exe unregister - Unregister the shell extension");
        Console.WriteLine("\nNote: Must be run as Administrator!");
    }

    static void RegisterExtension()
    {
        Console.WriteLine("Registering FolderLock shell extension...");
        
        if (ShellExtensionRegistration.Register())
        {
            Console.WriteLine("✓ Shell extension registered successfully!");
            Console.WriteLine("\nYou can now right-click on folders in Windows Explorer");
            Console.WriteLine("to see 'Lock Folder' and 'Unlock Folder' options.");
        }
        else
        {
            Console.WriteLine("✗ Failed to register shell extension.");
            Console.WriteLine("Check the error messages above for details.");
        }
    }

    static void UnregisterExtension()
    {
        Console.WriteLine("Unregistering FolderLock shell extension...");
        
        if (ShellExtensionRegistration.Unregister())
        {
            Console.WriteLine("✓ Shell extension unregistered successfully!");
        }
        else
        {
            Console.WriteLine("✗ Failed to unregister shell extension.");
            Console.WriteLine("Check the error messages above for details.");
        }
    }
}
