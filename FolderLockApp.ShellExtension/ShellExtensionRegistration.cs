using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using SharpShell.ServerRegistration;

namespace FolderLockApp.ShellExtension;

/// <summary>
/// Utility class for registering and unregistering the shell extension.
/// </summary>
public static class ShellExtensionRegistration
{
    /// <summary>
    /// Registers the shell extension with Windows Explorer.
    /// Requires administrator privileges.
    /// </summary>
    public static bool Register()
    {
        try
        {
            var serverType = typeof(FolderLockContextMenu);
            var registrationType = RegistrationType.OS64Bit; // Use OS64Bit for 64-bit Windows

            // Register the server
            ServerRegistrationManager.RegisterServer(serverType, registrationType);

            // Restart Explorer to apply changes
            RestartExplorer();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to register shell extension: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Unregisters the shell extension from Windows Explorer.
    /// Requires administrator privileges.
    /// </summary>
    public static bool Unregister()
    {
        try
        {
            var serverType = typeof(FolderLockContextMenu);
            var registrationType = RegistrationType.OS64Bit;

            // Unregister the server
            ServerRegistrationManager.UnregisterServer(serverType, registrationType);

            // Restart Explorer to apply changes
            RestartExplorer();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to unregister shell extension: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Restarts Windows Explorer to apply shell extension changes.
    /// </summary>
    private static void RestartExplorer()
    {
        try
        {
            // Kill all explorer processes
            foreach (var process in Process.GetProcessesByName("explorer"))
            {
                process.Kill();
                process.WaitForExit();
            }

            // Start explorer again
            Process.Start("explorer.exe");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to restart Explorer: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if the current process is running with administrator privileges.
    /// </summary>
    public static bool IsAdministrator()
    {
        try
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }
}
