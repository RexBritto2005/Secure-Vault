# 🔒 FolderLock - Secure Folder Encryption

A Windows desktop application for encrypting folders with AES-256 encryption. Simple, secure, and standalone.

---

## 🚀 Quick Start

### Build the Application

**Single Command:**
```cmd
build.bat
```

This creates: `Release\FolderLockApp.AllInOne.exe` (~80-100 MB)

### Run the Application

1. Navigate to `Release` folder
2. Double-click `FolderLockApp.AllInOne.exe`
3. Accept UAC prompt (admin privileges required)
4. Start locking folders!

---

## 📋 What is FolderLock?

FolderLock encrypts your folders with military-grade AES-256 encryption. Only you can unlock them with your password.

### Features

- ✅ **AES-256 Encryption** - Military-grade security
- ✅ **Password Protected** - Only you can unlock
- ✅ **Standalone Executable** - No installation needed
- ✅ **Self-Contained** - Includes .NET runtime
- ✅ **Admin Privileges** - Automatic UAC elevation
- ✅ **Code Integrity** - Verifies files aren't tampered
- ✅ **Background Service** - Built-in encryption service
- ✅ **Auto-Lock** - Automatic folder locking (timer-based)

---

## 💻 Usage

### Lock a Folder

1. Click **"Lock Folder"** button
2. Browse to folder you want to protect
3. Enter a strong password (twice)
4. Click **"Lock"**
5. Wait for encryption to complete

**Result:** All files encrypted with `.locked` extension

### Unlock a Folder

1. Select folder from the list
2. Click **"Unlock"** button
3. Enter your password
4. Wait for decryption

**Result:** Files restored to original state

---

## ⚠️ CRITICAL WARNINGS

### 🚨 NEVER FORGET YOUR PASSWORD

**If you forget your password, your files CANNOT be recovered.**

This is by design for security. There is no password recovery, no backdoor, no way to decrypt without the correct password.

### 🚨 BACKUP FIRST

Always backup important files before locking them for the first time.

### 🚨 TEST IMMEDIATELY

After locking a folder, **immediately unlock it** to verify your password works.

### 🚨 WRITE IT DOWN

Store your password in a safe place:
- Password manager
- Secure note
- Physical safe

**DO NOT** rely on memory alone for critical files.

---

## 🔧 System Requirements

- **OS:** Windows 10 or Windows 11
- **Privileges:** Administrator (automatic UAC prompt)
- **Disk Space:** ~100 MB for application
- **RAM:** 512 MB minimum
- **.NET:** Included (self-contained)

---

## 🏗️ Project Structure

```
FolderLockApp/
├── FolderLockApp.Core/          # Encryption engine & database
├── FolderLockApp.GUI/           # Desktop interface (WPF)
├── FolderLockApp.Service/       # Background service
├── FolderLockApp.AllInOne/      # Combined application
├── build.bat                    # Build script
└── README.md                    # This file
```

### Components

**FolderLockApp.Core**
- AES-256 encryption engine
- SQLite database
- Folder registry
- Security helpers

**FolderLockApp.GUI**
- WPF desktop interface
- Lock/unlock controls
- Folder list view
- Settings management

**FolderLockApp.Service**
- Background encryption service
- Auto-lock monitoring
- File system watching

**FolderLockApp.AllInOne**
- Combines GUI + Service
- Single executable
- No separate installation

---

## 🔐 Security Details

### Encryption

- **Algorithm:** AES-256-CBC
- **Key Size:** 256 bits
- **Block Size:** 128 bits
- **Mode:** CBC (Cipher Block Chaining)
- **Padding:** PKCS7

### Key Derivation

- **Method:** PBKDF2
- **Hash:** SHA-256
- **Iterations:** 100,000+
- **Salt:** Unique per folder
- **Output:** 256-bit key

### Security Features

- ✅ Passwords never stored (only hashes)
- ✅ Unique salt per folder
- ✅ Unique IV per file
- ✅ Admin privilege enforcement
- ✅ Code integrity verification
- ✅ Secure audit logging

---

## 📁 File Locations

### Application

- **Executable:** `Release\FolderLockApp.AllInOne.exe`
- **Size:** ~80-100 MB (includes .NET runtime)

### Data Files

- **Database:** `%ProgramData%\FolderLockApp\folderlock.db`
- **Logs:** `%ProgramData%\FolderLockApp\Logs\`

To open data folder:
```cmd
explorer %ProgramData%\FolderLockApp
```

---

## 🛠️ Building from Source

### Prerequisites

- .NET 8.0 SDK
- Windows 10/11
- Visual Studio 2022 (optional)

### Build Commands

**Simple Build (Development):**
```cmd
dotnet build --configuration Release
```

**Standalone Executable (Distribution):**
```cmd
build.bat
```

**Manual Build:**
```cmd
dotnet publish FolderLockApp.AllInOne\FolderLockApp.AllInOne.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output .\Release ^
    /p:PublishSingleFile=true
```

### Build Output

- **Location:** `Release\FolderLockApp.AllInOne.exe`
- **Type:** Self-contained single file
- **Runtime:** Included (.NET 8.0)
- **Size:** ~80-100 MB

---

## 🐛 Troubleshooting

### "Administrator Privileges Required"

**Problem:** Application won't start or shows privilege error

**Solution:** Right-click `FolderLockApp.AllInOne.exe` and select "Run as administrator"

### "Code Integrity Verification Failed"

**Problem:** Security warning on startup

**Solution:** 
- Normal for debug builds
- For production, sign the executable with a code signing certificate
- In development, this warning can be ignored

### Can't Unlock - Wrong Password

**Problem:** Entered password doesn't work

**Solution:**
- Try again (check Caps Lock)
- If truly forgotten, files **cannot be recovered**
- Restore from backup if available

### Application Crashes on Startup

**Problem:** Application closes immediately

**Solution:**
1. Check logs: `%ProgramData%\FolderLockApp\Logs\`
2. Ensure .NET 8.0 runtime is available (should be included)
3. Run as administrator
4. Check Windows Event Viewer for errors

### Slow Encryption/Decryption

**Problem:** Taking too long to lock/unlock

**Normal for:**
- Large folders (many GB)
- Many small files
- Slow storage (HDD vs SSD)

**Tips:**
- Close other applications
- Ensure adequate free disk space
- Use SSD for better performance

---

## 📊 Performance

Typical performance on modern hardware:

| File Type | Speed |
|-----------|-------|
| Small files (< 1MB) | ~100 files/second |
| Medium files (1-10MB) | ~10 files/second |
| Large files (> 100MB) | ~50 MB/second |

**Factors affecting performance:**
- CPU speed
- Storage type (SSD vs HDD)
- File count
- Total size
- System load

---

## 🔒 Best Practices

### Password Security

✅ **DO:**
- Use strong passwords (12+ characters)
- Mix letters, numbers, symbols
- Use unique passwords per folder
- Store in password manager
- Write down for critical files

❌ **DON'T:**
- Use simple passwords
- Reuse passwords
- Share passwords
- Rely only on memory

### File Management

✅ **DO:**
- Backup before locking
- Test unlock immediately
- Keep backups of encrypted folders
- Document which folders are locked

❌ **DON'T:**
- Lock without backup
- Forget to test unlock
- Manually modify .locked files
- Delete database without unlocking

### Usage Tips

1. **Start Small:** Test with non-critical files first
2. **Verify Password:** Unlock immediately after locking
3. **Regular Backups:** Backup encrypted folders regularly
4. **Document Passwords:** Keep secure password records
5. **Monitor Logs:** Check logs for errors

---

## 📝 How It Works

### Locking Process

1. User selects folder and enters password
2. Application generates encryption key from password (PBKDF2)
3. Each file is encrypted with AES-256
4. Files renamed with `.locked` extension
5. Folder registered in database
6. Original files securely deleted

### Unlocking Process

1. User selects folder and enters password
2. Application derives key from password
3. Each `.locked` file is decrypted
4. Original filenames restored
5. Database updated
6. Encrypted files deleted

### File Structure

**Before Locking:**
```
MyFolder/
├── document.txt
├── image.jpg
└── data.xlsx
```

**After Locking:**
```
MyFolder/
├── document.txt.locked
├── image.jpg.locked
└── data.xlsx.locked
```

---

## 🔄 Updates & Maintenance

### Updating the Application

1. Unlock all folders
2. Close the application
3. Replace executable with new version
4. Run new version
5. Re-lock folders if needed

### Database Backup

```powershell
# Backup database
Copy-Item "$env:ProgramData\FolderLockApp\folderlock.db" `
          "$env:ProgramData\FolderLockApp\folderlock.db.backup"
```

### Viewing Logs

```powershell
# View recent logs
Get-Content "$env:ProgramData\FolderLockApp\Logs\allinone-*.log" -Tail 50
```

---

## 🗑️ Uninstallation

### Before Uninstalling

**⚠️ CRITICAL:** Unlock all folders first!

1. Open FolderLock
2. Unlock every folder in the list
3. Verify files are accessible
4. Close application

### Uninstall Steps

1. Delete `FolderLockApp.AllInOne.exe`
2. Delete data folder (optional):
   ```cmd
   rmdir /s "%ProgramData%\FolderLockApp"
   ```

---

## ❓ FAQ

### Q: Is this really secure?

**A:** Yes. AES-256 is military-grade encryption used by governments and banks. With a strong password, your files are extremely secure.

### Q: Can I recover my password?

**A:** No. This is by design. If password recovery existed, it would be a security vulnerability.

### Q: Can I move encrypted folders?

**A:** Yes. You can copy/move folders with `.locked` files. Unlock them with FolderLock on any computer.

### Q: Does this work on network drives?

**A:** Yes, but performance may be slower. Local drives recommended.

### Q: Can I lock system folders?

**A:** Not recommended. Only lock user data folders.

### Q: What happens if I lose the database?

**A:** You can still decrypt files if you remember the password, but you'll need to manually decrypt each file.

### Q: Is this open source?

**A:** Check the repository license for details.

---

## 📞 Support

### Getting Help

1. **Check this README** - Most questions answered here
2. **Check logs** - `%ProgramData%\FolderLockApp\Logs\`
3. **Windows Event Viewer** - Application log
4. **Search error messages** - In this document

### Reporting Issues

When reporting issues, include:
- Windows version
- Error messages
- Log file excerpts
- Steps to reproduce

---

## ⚡ Quick Reference

### Build
```cmd
build.bat
```

### Run
```cmd
Release\FolderLockApp.AllInOne.exe
```

### View Logs
```cmd
explorer %ProgramData%\FolderLockApp\Logs
```

### Backup Database
```cmd
copy "%ProgramData%\FolderLockApp\folderlock.db" backup.db
```

---

## 📜 License

[Specify your license here]

---

## 🎉 You're Ready!

1. Run `build.bat`
2. Run `Release\FolderLockApp.AllInOne.exe`
3. Lock your first folder
4. Remember your password!

**🔒 Happy encrypting!**

---

**Version:** 1.0  
**Last Updated:** 2024  
**Platform:** Windows 10/11  
**Framework:** .NET 8.0
