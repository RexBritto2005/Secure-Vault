# FolderLock - Secure Folder Encryption Application

A comprehensive Windows application for encrypting and managing folder security with AES-256 encryption.

## 🚀 Quick Links

- **[Quick Start Guide](QUICK_START.md)** - Get running in 5 minutes
- **[Complete User Guide](USER_GUIDE.md)** - Detailed usage instructions
- **[How It Works](HOW_IT_WORKS.md)** - Architecture and technical details
- **[Security Features](SECURITY_FEATURES.md)** - Security implementation
- **[Admin Setup Guide](ADMIN_SETUP_GUIDE.md)** - Installation and configuration

## 📋 Overview

FolderLock is a Windows desktop application that provides military-grade encryption for your folders. It features:

- **🔒 AES-256 Encryption** - Industry-standard encryption
- **🖥️ User-Friendly GUI** - Easy-to-use interface
- **⚡ Background Service** - Automatic folder monitoring
- **🖱️ Shell Integration** - Right-click menu in Windows Explorer
- **⏰ Auto-Lock** - Automatic locking based on timers
- **🔐 Secure** - Admin privileges and code integrity verification

## 🎯 Key Features

### For Users
- Lock/unlock folders with password protection
- Right-click context menu integration
- Automatic folder locking (timer-based)
- Visual status indicators
- Password-protected access
- Backup-friendly (encrypted files can be backed up)

### For Security
- AES-256-CBC encryption
- PBKDF2 key derivation (100,000+ iterations)
- Unique salt per folder
- No password storage (only hashes)
- Code integrity verification
- Administrator privilege enforcement
- Comprehensive audit logging

### For Developers
- Modular architecture
- Clean separation of concerns
- SQLite database
- IPC communication (Named Pipes)
- Extensive error handling
- Property-based testing

## 🏗️ Architecture

```
┌─────────────────────────────────────────┐
│         FolderLock System               │
├─────────────────────────────────────────┤
│                                         │
│  GUI App  ←→  Background Service        │
│     ↓              ↓                    │
│  Core Library (Encryption, Database)    │
│     ↑                                   │
│  Shell Extension (Right-Click Menu)     │
│                                         │
└─────────────────────────────────────────┘
```

### Components

1. **FolderLockApp.GUI** - WPF desktop application
2. **FolderLockApp.Service** - Windows background service
3. **FolderLockApp.Core** - Shared business logic
4. **FolderLockApp.ShellExtension** - Windows Explorer integration

## 🚀 Getting Started

### Prerequisites
- Windows 10 or Windows 11
- .NET 8.0 SDK (for building)
- .NET 8.0 Runtime (for running)
- Administrator privileges

### Quick Installation

1. **Build the application:**
```cmd
dotnet build --configuration Release
```

2. **Install the service:**
```powershell
# Run as Administrator
cd FolderLockApp.Service\bin\Release\net8.0
$servicePath = (Get-Location).Path + "\FolderLockApp.Service.exe"
sc.exe create "FolderLockApp Encryption Service" binPath=$servicePath
sc.exe start "FolderLockApp Encryption Service"
```

3. **Launch the GUI:**
```cmd
# Navigate to GUI folder
cd FolderLockApp.GUI\bin\Release\net8.0-windows
# Double-click FolderLockApp.GUI.exe or run:
.\FolderLockApp.GUI.exe
```

4. **Optional - Install shell extension:**
```cmd
# Run as Administrator
cd FolderLockApp.ShellExtension\bin\Release\net8.0-windows
FolderLockApp.ShellExtension.exe register
```

For detailed instructions, see **[QUICK_START.md](QUICK_START.md)**

## 📖 Usage

### Lock a Folder

**Using GUI:**
1. Launch FolderLockApp.GUI
2. Click "Lock Folder"
3. Select folder and enter password
4. Click "Lock"

**Using Right-Click:**
1. Right-click any folder
2. Select "Lock Folder"
3. Enter password
4. Done!

### Unlock a Folder

**Using GUI:**
1. Select locked folder from list
2. Click "Unlock"
3. Enter password

**Using Right-Click:**
1. Right-click locked folder
2. Select "Unlock Folder"
3. Enter password

For complete usage instructions, see **[USER_GUIDE.md](USER_GUIDE.md)**

## 🔒 Security

### Encryption
- **Algorithm**: AES-256-CBC
- **Key Derivation**: PBKDF2-SHA256 (100,000+ iterations)
- **Unique IVs**: Each file has unique initialization vector
- **Salt**: Unique salt per folder

### Application Security
- **Admin Privileges**: Required and enforced
- **Code Integrity**: Digital signature verification
- **No Key Storage**: Keys derived from password, never stored
- **Audit Logging**: All operations logged

### Best Practices
✅ Use strong, unique passwords
✅ Backup encrypted folders regularly
✅ Test unlock immediately after locking
✅ Keep the application updated
✅ Review security logs periodically

For detailed security information, see **[SECURITY_FEATURES.md](SECURITY_FEATURES.md)**

## 📁 Project Structure

```
FolderLockApp/
├── FolderLockApp.Core/          # Core business logic
│   ├── Data/                    # Database context
│   ├── Interfaces/              # Service contracts
│   ├── Models/                  # Data models
│   ├── Services/                # Business services
│   ├── Helpers/                 # Utility classes
│   └── Tests/                   # Unit tests
│
├── FolderLockApp.GUI/           # WPF desktop application
│   ├── Views/                   # XAML views
│   ├── ViewModels/              # View models
│   └── app.manifest             # UAC manifest
│
├── FolderLockApp.Service/       # Background Windows service
│   ├── Program.cs               # Service entry point
│   └── EncryptionBackgroundService.cs
│
├── FolderLockApp.ShellExtension/  # Windows Explorer integration
│   ├── FolderLockContextMenu.cs   # Context menu handler
│   ├── ShellExtensionRegistration.cs
│   └── app.manifest               # UAC manifest
│
└── Documentation/
    ├── README.md                # This file
    ├── QUICK_START.md           # Quick start guide
    ├── USER_GUIDE.md            # Complete user guide
    ├── HOW_IT_WORKS.md          # Technical details
    ├── SECURITY_FEATURES.md     # Security documentation
    └── ADMIN_SETUP_GUIDE.md     # Admin guide
```

## 🛠️ Development

### Building from Source

```cmd
# Clone the repository
git clone <repository-url>
cd FolderLockApp

# Restore dependencies
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test FolderLockApp.Core
```

### Running in Development

```cmd
# Run the GUI (Debug mode)
cd FolderLockApp.GUI
dotnet run

# Run the service (Debug mode)
cd FolderLockApp.Service
dotnet run
```

### Technology Stack
- **.NET 8.0** - Framework
- **WPF** - Desktop UI
- **Entity Framework Core** - ORM
- **SQLite** - Database
- **Serilog** - Logging
- **SharpShell** - Shell extension
- **xUnit + FsCheck** - Testing

## 🧪 Testing

### Run Unit Tests
```cmd
dotnet test FolderLockApp.Core
```

### Test Coverage
- Encryption engine tests
- File system operation tests
- Database operation tests
- Property-based tests (FsCheck)
- Integration tests

## 📊 Performance

Typical performance on modern hardware:
- **Small files** (< 1MB): ~100 files/second
- **Medium files** (1-10MB): ~10 files/second
- **Large files** (> 100MB): ~50 MB/second

Performance varies based on:
- CPU speed
- Storage type (SSD vs HDD)
- File types
- System load

## 🐛 Troubleshooting

### Common Issues

**"Administrator Privileges Required"**
- Right-click and "Run as administrator"

**"Service Not Running"**
```powershell
Start-Service "FolderLockApp Encryption Service"
```

**"Code Integrity Verification Failed"**
- Normal for debug builds
- For production, sign assemblies

**Can't Unlock - Wrong Password**
- Password cannot be recovered (by design)
- Restore from backup if available

For detailed troubleshooting, see **[USER_GUIDE.md](USER_GUIDE.md#troubleshooting)**

## 📝 Logging

### Log Locations
- **Service Logs**: `%ProgramData%\FolderLockApp\Logs\service-*.log`
- **Security Logs**: `%ProgramData%\FolderLockApp\Logs\security-*.log`
- **Windows Event Log**: Application log, source "FolderLockApp Service"

### View Logs
```powershell
# View service logs
Get-Content "$env:ProgramData\FolderLockApp\Logs\service-*.log" -Tail 50

# View security logs
Get-Content "$env:ProgramData\FolderLockApp\Logs\security-*.log" -Tail 50

# View Windows Event Log
Get-EventLog -LogName Application -Source "FolderLockApp Service" -Newest 20
```

## 🔄 Updates and Maintenance

### Updating the Application
1. Stop the service
2. Backup the database
3. Replace executables
4. Restart the service

### Database Backup
```powershell
Copy-Item "$env:ProgramData\FolderLockApp\folderlock.db" `
          "$env:ProgramData\FolderLockApp\folderlock.db.backup"
```

### Maintenance Tasks
- **Weekly**: Check service status
- **Monthly**: Review logs, backup database
- **Quarterly**: Full backup, test restore

## 🗑️ Uninstallation

### Important: Unlock All Folders First!
Before uninstalling, unlock all folders or you'll lose access to encrypted files.

### Uninstall Steps
```powershell
# 1. Stop and remove service
sc.exe stop "FolderLockApp Encryption Service"
sc.exe delete "FolderLockApp Encryption Service"

# 2. Unregister shell extension
cd path\to\FolderLockApp.ShellExtension
FolderLockApp.ShellExtension.exe unregister

# 3. Delete application files
# 4. Delete data folder (optional)
Remove-Item "$env:ProgramData\FolderLockApp" -Recurse
```

## ⚠️ Important Warnings

🚨 **Password Recovery**: If you forget your password, files CANNOT be recovered. This is by design for security.

🚨 **Backup First**: Always backup important data before locking for the first time.

🚨 **Test Unlock**: After locking, immediately test unlocking to verify password works.

🚨 **Service Dependency**: Keep the background service running for auto-lock features.

🚨 **Admin Rights**: Always run with administrator privileges.

## 📄 License

[Specify your license here]

## 🤝 Contributing

[Specify contribution guidelines here]

## 📞 Support

For issues, questions, or feature requests:
1. Check the documentation
2. Review log files
3. Search existing issues
4. Create a new issue with details

## 🙏 Acknowledgments

- Built with .NET 8.0
- Uses SharpShell for shell extension
- Encryption powered by .NET Cryptography
- Testing with xUnit and FsCheck

## 📚 Additional Resources

- **[Quick Start Guide](QUICK_START.md)** - Get started in 5 minutes
- **[User Guide](USER_GUIDE.md)** - Complete usage documentation
- **[How It Works](HOW_IT_WORKS.md)** - Technical architecture
- **[Security Features](SECURITY_FEATURES.md)** - Security details
- **[Admin Setup](ADMIN_SETUP_GUIDE.md)** - Installation guide
- **[Implementation Summary](IMPLEMENTATION_SUMMARY.md)** - Recent changes

---

**FolderLock** - Secure your folders with confidence. 🔒

*Remember: With great encryption comes great responsibility. Always backup your data and remember your passwords!*
