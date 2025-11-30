# How FolderLock Works

A visual guide to understanding the FolderLock application architecture and workflow.

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     FolderLock System                        │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────┐         ┌──────────────┐                  │
│  │   GUI App    │◄───────►│   Service    │                  │
│  │ (User Interface)        │ (Background) │                  │
│  └──────┬───────┘         └──────┬───────┘                  │
│         │                         │                           │
│         │                         │                           │
│         ▼                         ▼                           │
│  ┌──────────────────────────────────────┐                   │
│  │         Core Library                  │                   │
│  │  • Encryption Engine                  │                   │
│  │  • Folder Registry                    │                   │
│  │  • Database (SQLite)                  │                   │
│  └──────────────────────────────────────┘                   │
│         ▲                                                     │
│         │                                                     │
│  ┌──────┴───────┐                                           │
│  │ Shell Extension│                                          │
│  │ (Right-Click)  │                                          │
│  └────────────────┘                                          │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

## Component Roles

### 1. GUI Application (FolderLockApp.GUI)
**Purpose**: User interface for managing encrypted folders

**Features**:
- Add/remove folders
- Lock/unlock operations
- View locked folders list
- Configure settings
- Monitor service status

**Technology**: WPF (Windows Presentation Foundation)

### 2. Background Service (FolderLockApp.Service)
**Purpose**: Handles encryption operations and monitoring

**Features**:
- Runs as Windows Service
- Automatic folder locking (timers)
- File system monitoring
- IPC communication with GUI
- Event logging

**Technology**: .NET Worker Service

### 3. Core Library (FolderLockApp.Core)
**Purpose**: Shared business logic and data access

**Components**:
- **Encryption Engine**: AES-256 encryption/decryption
- **Folder Registry**: Tracks locked folders
- **Database Context**: SQLite database operations
- **Helper Classes**: Admin privileges, code integrity

**Technology**: .NET Class Library

### 4. Shell Extension (FolderLockApp.ShellExtension)
**Purpose**: Windows Explorer integration

**Features**:
- Right-click context menu
- Quick lock/unlock
- Password prompts
- Registry integration

**Technology**: SharpShell COM

---

## Workflow: Locking a Folder

```
User Action: Lock Folder
         │
         ▼
┌────────────────────┐
│  1. User Input     │
│  • Select folder   │
│  • Enter password  │
└────────┬───────────┘
         │
         ▼
┌────────────────────┐
│  2. Validation     │
│  • Check folder    │
│  • Verify password │
│  • Check service   │
└────────┬───────────┘
         │
         ▼
┌────────────────────┐
│  3. Registration   │
│  • Add to database │
│  • Store metadata  │
└────────┬───────────┘
         │
         ▼
┌────────────────────┐
│  4. Encryption     │
│  • Generate key    │
│  • Encrypt files   │
│  • Add .locked ext │
└────────┬───────────┘
         │
         ▼
┌────────────────────┐
│  5. Completion     │
│  • Update UI       │
│  • Log event       │
│  • Notify user     │
└────────────────────┘
```

### Detailed Steps

#### Step 1: User Input
- User selects folder via GUI or right-click
- Enters password (minimum requirements enforced)
- Confirms password

#### Step 2: Validation
- Checks if folder exists and is accessible
- Verifies folder isn't already locked
- Validates password strength
- Confirms service is running

#### Step 3: Registration
- Creates database entry with:
  - Folder path
  - Lock timestamp
  - Settings (auto-lock, etc.)
  - Metadata (file count, size)

#### Step 4: Encryption
- Derives encryption key from password (PBKDF2)
- For each file in folder:
  - Reads file content
  - Encrypts with AES-256
  - Writes encrypted content
  - Renames to `filename.ext.locked`
- Stores IV (Initialization Vector) with each file

#### Step 5: Completion
- Updates GUI to show locked status
- Logs security event
- Displays success notification

---

## Workflow: Unlocking a Folder

```
User Action: Unlock Folder
         │
         ▼
┌────────────────────┐
│  1. User Input     │
│  • Select folder   │
│  • Enter password  │
└────────┬───────────┘
         │
         ▼
┌────────────────────┐
│  2. Verification   │
│  • Check database  │
│  • Verify password │
└────────┬───────────┘
         │
         ▼
┌────────────────────┐
│  3. Decryption     │
│  • Derive key      │
│  • Decrypt files   │
│  • Remove .locked  │
└────────┬───────────┘
         │
         ▼
┌────────────────────┐
│  4. Completion     │
│  • Update database │
│  • Update UI       │
│  • Log event       │
└────────────────────┘
```

### Detailed Steps

#### Step 1: User Input
- User selects locked folder
- Enters password

#### Step 2: Verification
- Looks up folder in database
- Derives key from entered password
- Attempts to decrypt first file as test
- If successful, password is correct

#### Step 3: Decryption
- For each `.locked` file:
  - Reads encrypted content
  - Extracts IV
  - Decrypts with AES-256
  - Writes original content
  - Removes `.locked` extension

#### Step 4: Completion
- Updates database (unlock timestamp)
- Refreshes GUI
- Logs security event
- Displays success notification

---

## Encryption Details

### Algorithm: AES-256-CBC
- **Key Size**: 256 bits
- **Block Size**: 128 bits
- **Mode**: CBC (Cipher Block Chaining)
- **Padding**: PKCS7

### Key Derivation: PBKDF2
- **Hash**: SHA-256
- **Iterations**: 100,000+
- **Salt**: Random per folder (stored in database)
- **Output**: 256-bit key

### File Structure (Encrypted)
```
┌─────────────────────────────────┐
│  IV (16 bytes)                   │  ← Initialization Vector
├─────────────────────────────────┤
│  Encrypted Data                  │  ← AES-256 encrypted content
│  (variable length)               │
└─────────────────────────────────┘
```

### Security Properties
✅ **Confidentiality**: AES-256 encryption
✅ **Key Security**: PBKDF2 with high iteration count
✅ **Unique IVs**: Each file has unique IV
✅ **Salt**: Unique salt per folder prevents rainbow tables
✅ **No Key Storage**: Keys derived from password, never stored

---

## Database Schema

### Table: LockedFolders
```sql
CREATE TABLE LockedFolders (
    Id              INTEGER PRIMARY KEY,
    FolderPath      TEXT NOT NULL UNIQUE,
    IsLocked        BOOLEAN NOT NULL,
    LockedAt        DATETIME,
    UnlockedAt      DATETIME,
    PasswordHash    TEXT NOT NULL,
    Salt            BLOB NOT NULL,
    AutoLockEnabled BOOLEAN DEFAULT 0,
    AutoLockMinutes INTEGER DEFAULT 0,
    FileCount       INTEGER,
    TotalSize       INTEGER
);
```

### Stored Information
- **FolderPath**: Full path to folder
- **IsLocked**: Current lock status
- **Timestamps**: When locked/unlocked
- **PasswordHash**: Hashed password (not plaintext!)
- **Salt**: Unique salt for key derivation
- **Auto-Lock Settings**: Timer configuration
- **Metadata**: File count, total size

---

## Inter-Process Communication (IPC)

### GUI ↔ Service Communication

```
┌─────────────┐                    ┌─────────────┐
│   GUI App   │                    │   Service   │
└──────┬──────┘                    └──────┬──────┘
       │                                  │
       │  1. Connect (Named Pipe)         │
       ├─────────────────────────────────►│
       │                                  │
       │  2. Request (Lock Folder)        │
       ├─────────────────────────────────►│
       │                                  │
       │         3. Processing...         │
       │                                  │
       │  4. Response (Success/Failure)   │
       │◄─────────────────────────────────┤
       │                                  │
       │  5. Status Updates               │
       │◄─────────────────────────────────┤
       │                                  │
```

### Named Pipe Protocol
- **Pipe Name**: `FolderLockApp_IPC`
- **Format**: JSON messages
- **Security**: Admin-only access
- **Timeout**: 30 seconds per operation

### Message Types
- `LockFolder`: Request to lock a folder
- `UnlockFolder`: Request to unlock a folder
- `GetLockedFolders`: Query locked folders list
- `GetStatus`: Service health check
- `UpdateSettings`: Modify folder settings

---

## Security Features

### 1. Administrator Privileges
```
Application Start
       │
       ▼
┌──────────────┐
│ Check Admin? │
└──────┬───────┘
       │
   ┌───┴───┐
   │  Yes  │  No
   │       │
   ▼       ▼
Continue  Request UAC
          Elevation
```

**Why Required**:
- File system access
- Service management
- Registry modifications
- Protected resource access

### 2. Code Integrity Verification
```
Application Start
       │
       ▼
┌──────────────────┐
│ Verify Signature │
└────────┬─────────┘
         │
    ┌────┴────┐
    │ Valid?  │
    └────┬────┘
         │
   ┌─────┴─────┐
   │ Yes   No  │
   │           │
   ▼           ▼
Continue    Block Start
            (Release Mode)
```

**Checks Performed**:
- Authenticode signature
- Strong name signature
- SHA256 file hash
- Assembly integrity

### 3. Password Security
- **Never Stored**: Only hash stored
- **Salt**: Unique per folder
- **Iterations**: 100,000+ PBKDF2
- **Validation**: On unlock attempt only

---

## Auto-Lock Feature

### How It Works

```
Folder Locked with Auto-Lock Enabled
              │
              ▼
    ┌─────────────────┐
    │ Service Monitors│
    │ File Activity   │
    └────────┬────────┘
             │
             ▼
    ┌─────────────────┐
    │ Timer Started   │
    │ (e.g., 30 min)  │
    └────────┬────────┘
             │
        ┌────┴────┐
        │Activity?│
        └────┬────┘
             │
      ┌──────┴──────┐
      │ Yes     No  │
      │             │
      ▼             ▼
  Reset Timer   Timer Expires
                     │
                     ▼
              ┌──────────────┐
              │ Auto-Lock    │
              │ Folder       │
              └──────────────┘
```

### Configuration
- **Minimum**: 1 minute
- **Maximum**: 24 hours
- **Default**: 30 minutes
- **Activity Detection**: File access, modification

---

## File System Operations

### Lock Operation
```
Original Folder:
├── document.txt
├── image.jpg
└── data.xlsx

After Locking:
├── document.txt.locked
├── image.jpg.locked
└── data.xlsx.locked
```

### Unlock Operation
```
Locked Folder:
├── document.txt.locked
├── image.jpg.locked
└── data.xlsx.locked

After Unlocking:
├── document.txt
├── image.jpg
└── data.xlsx
```

### File Handling
- **Preserves**: Timestamps, attributes
- **Maintains**: Folder structure
- **Handles**: Subdirectories recursively
- **Skips**: System files, hidden files (optional)

---

## Error Handling

### Graceful Degradation
```
Operation Failed
       │
       ▼
┌──────────────┐
│ Log Error    │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ Rollback     │
│ Changes      │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ Notify User  │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ Restore State│
└──────────────┘
```

### Error Types
- **File Access**: Permission denied, file in use
- **Encryption**: Corruption, invalid key
- **Service**: Not running, communication failure
- **Database**: Locked, corrupted

### Recovery Strategies
- **Automatic Retry**: Transient failures
- **Rollback**: Partial operations
- **User Notification**: Clear error messages
- **Logging**: Detailed error information

---

## Performance Considerations

### Optimization Strategies

1. **Parallel Processing**
   - Multiple files encrypted simultaneously
   - Thread pool management
   - CPU core utilization

2. **Buffered I/O**
   - Large buffer sizes (64KB+)
   - Reduced disk I/O operations
   - Memory-efficient streaming

3. **Progress Reporting**
   - Real-time updates
   - Cancellation support
   - Estimated time remaining

### Typical Performance
- **Small Files** (< 1MB): ~100 files/second
- **Medium Files** (1-10MB): ~10 files/second
- **Large Files** (> 100MB): ~50 MB/second

*Performance varies based on hardware, file types, and system load*

---

## Logging and Monitoring

### Log Levels
- **Information**: Normal operations
- **Warning**: Potential issues
- **Error**: Operation failures
- **Fatal**: Critical system errors

### Log Locations
```
%ProgramData%\FolderLockApp\Logs\
├── service-2024-01-15.log      (Daily rotation)
├── security-2024-01-15.log     (Security events)
└── [older logs...]             (Retained 30 days)
```

### Monitored Events
- Lock/unlock operations
- Service start/stop
- Security checks
- Error conditions
- Performance metrics

---

## Summary

FolderLock provides enterprise-grade folder encryption through:

✅ **Strong Encryption**: AES-256 with proper key derivation
✅ **User-Friendly**: GUI, right-click menu, automatic operations
✅ **Secure**: Admin privileges, code integrity, no key storage
✅ **Reliable**: Background service, error handling, logging
✅ **Flexible**: Manual and automatic locking, configurable settings

The modular architecture ensures maintainability while the security-first design protects your sensitive data.
