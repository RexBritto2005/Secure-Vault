# Design Document

## Overview

The Folder Encryption Lock application is a .NET-based security solution that provides transparent folder encryption with password protection. The system consists of three main components: a WPF GUI application for user interaction, a Windows background service for monitoring and handling unlock requests, and a Shell Extension for Windows Explorer integration. The application uses AES-256 encryption with PBKDF2 key derivation to ensure strong security while maintaining a seamless user experience.

## Architecture

The system follows a multi-process architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                     User Interaction Layer                   │
│  ┌──────────────────┐         ┌─────────────────────────┐  │
│  │  WPF GUI App     │         │  Shell Extension (COM)  │  │
│  │  (FolderLockApp) │         │  (Context Menu Handler) │  │
│  └────────┬─────────┘         └───────────┬─────────────┘  │
└───────────┼─────────────────────────────────┼───────────────┘
            │                                 │
            │ Named Pipes / WCF              │
            │                                 │
┌───────────┼─────────────────────────────────┼───────────────┐
│           ▼                                 ▼                │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         Background Service (EncryptionService)        │  │
│  │  - Folder Monitoring                                  │  │
│  │  - Encryption/Decryption Engine                       │  │
│  │  - Password Dialog Host                               │  │
│  └──────────────────────┬───────────────────────────────┘  │
│                         │                                   │
│                         ▼                                   │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Data Persistence Layer                   │  │
│  │  - FolderRegistry (SQLite)                            │  │
│  │  - Encrypted File Storage                             │  │
│  └──────────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────────┘
```

### Component Responsibilities

1. **WPF GUI Application (FolderLockApp.exe)**
   - Main user interface for folder selection and management
   - Displays list of locked folders
   - Initiates encryption/decryption operations
   - Communicates with EncryptionService via IPC

2. **Background Service (EncryptionService.exe)**
   - Runs as Windows Service with auto-start
   - Monitors locked folder access attempts
   - Handles encryption/decryption operations
   - Displays password dialogs when needed
   - Maintains folder registry

3. **Shell Extension (FolderLockShell.dll)**
   - COM-based Windows Explorer integration
   - Provides context menu items
   - Delegates operations to EncryptionService

## Components and Interfaces

### 1. Encryption Engine

```csharp
public interface IEncryptionEngine
{
    Task<EncryptionResult> EncryptFolderAsync(string folderPath, SecureString password, IProgress<int> progress);
    Task<DecryptionResult> DecryptFolderAsync(string folderPath, SecureString password, IProgress<int> progress);
    byte[] DeriveKey(SecureString password, byte[] salt);
    bool VerifyPassword(SecureString password, byte[] storedHash, byte[] salt);
}

public class EncryptionResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public int FilesProcessed { get; set; }
    public string BackupPath { get; set; }
}
```

### 2. Folder Registry

```csharp
public interface IFolderRegistry
{
    Task<LockedFolderEntry> RegisterLockedFolderAsync(string folderPath, byte[] passwordHash, byte[] salt);
    Task<LockedFolderEntry> GetLockedFolderAsync(string folderPath);
    Task<IEnumerable<LockedFolderEntry>> GetAllLockedFoldersAsync();
    Task<bool> RemoveLockedFolderAsync(string folderPath);
    Task UpdateLastAccessAsync(string folderPath);
}

public class LockedFolderEntry
{
    public string FolderPath { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] Salt { get; set; }
    public DateTime LockedDate { get; set; }
    public DateTime LastAccessed { get; set; }
    public bool IsLocked { get; set; }
}
```

### 3. Service Communication

```csharp
[ServiceContract]
public interface IEncryptionServiceContract
{
    [OperationContract]
    Task<ServiceResponse> LockFolderAsync(string folderPath, string password);
    
    [OperationContract]
    Task<ServiceResponse> UnlockFolderAsync(string folderPath, string password);
    
    [OperationContract]
    Task<ServiceResponse> RemoveFolderAsync(string folderPath, string password);
    
    [OperationContract]
    Task<List<LockedFolderInfo>> GetLockedFoldersAsync();
}
```

### 4. Shell Extension Interface

```csharp
public interface IShellContextMenuHandler
{
    void Initialize(string folderPath);
    IEnumerable<ContextMenuItem> GetMenuItems();
    void InvokeCommand(string command);
}
```

## Data Models

### Encrypted File Format

Each encrypted file will have the following structure:

```
┌─────────────────────────────────────────────┐
│ Header (64 bytes)                           │
│  - Magic Number (4 bytes): "FLCK"           │
│  - Version (4 bytes): 1                     │
│  - Salt (32 bytes)                          │
│  - IV (16 bytes)                            │
│  - Reserved (8 bytes)                       │
├─────────────────────────────────────────────┤
│ Encrypted Content (variable length)         │
│  - Original filename length (4 bytes)       │
│  - Original filename (variable)             │
│  - File content (variable)                  │
├─────────────────────────────────────────────┤
│ HMAC (32 bytes)                             │
│  - SHA-256 HMAC for integrity verification  │
└─────────────────────────────────────────────┘
```

### Database Schema (SQLite)

```sql
CREATE TABLE LockedFolders (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FolderPath TEXT UNIQUE NOT NULL,
    PasswordHash BLOB NOT NULL,
    Salt BLOB NOT NULL,
    LockedDate TEXT NOT NULL,
    LastAccessed TEXT NOT NULL,
    IsLocked INTEGER NOT NULL DEFAULT 1
);

CREATE INDEX idx_folder_path ON LockedFolders(FolderPath);
```

## Co
rrectness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

After reviewing all acceptance criteria, I've identified properties that can be combined and eliminated redundancy. Properties 8.3 and 8.5 are subsumed by the comprehensive round-trip property (Property 1). Properties about file extension handling (8.4, 8.5) are implementation details covered by the round-trip test.

**Property 1: Encryption-Decryption Round Trip**
*For any* folder structure with files, encrypting the folder with a password and then decrypting it with the same password should restore all files to their original content, names, and directory locations.
**Validates: Requirements 1.3, 2.2, 2.3, 8.2, 8.3, 8.5**

**Property 2: Folder Registry Consistency**
*For any* folder that successfully completes encryption, that folder's path should appear in the FolderRegistry with IsLocked status set to true.
**Validates: Requirements 1.4**

**Property 3: Incorrect Password Rejection**
*For any* locked folder and any password that differs from the original encryption password, decryption attempts should fail and leave all encrypted files unchanged.
**Validates: Requirements 2.4, 6.4**

**Property 4: Service Initialization Loads All Entries**
*For any* state of the FolderRegistry containing N locked folder entries, when the EncryptionService starts, it should load exactly N entries into memory.
**Validates: Requirements 3.3**

**Property 5: PBKDF2 Key Derivation Parameters**
*For any* password used for encryption, the derived encryption key should be generated using PBKDF2 with at least 100,000 iterations and a unique salt.
**Validates: Requirements 5.1**

**Property 6: Password Storage Security**
*For any* password stored in the FolderRegistry, the stored data should contain only a salted hash and never the plaintext password.
**Validates: Requirements 5.2**

**Property 7: Password Verification Before Decryption**
*For any* unlock attempt, password verification against the stored hash should complete before any decryption operations begin.
**Validates: Requirements 5.3**

**Property 8: AES-256 CBC with Random IV**
*For any* encryption operation, the encrypted output should use AES-256 in CBC mode, and each encrypted file should have a unique, randomly generated initialization vector.
**Validates: Requirements 5.4**

**Property 9: Backup Creation Before Encryption**
*For any* folder encryption operation, a backup of the folder structure metadata should be created before any files are modified.
**Validates: Requirements 6.1**

**Property 10: Rollback on Encryption Failure**
*For any* encryption operation that fails partway through, all original files should be restored from backup, leaving the folder in its pre-encryption state.
**Validates: Requirements 6.2**

**Property 11: Integrity Verification Before Decryption**
*For any* encrypted file, the HMAC integrity check should be verified before attempting decryption, and files failing verification should be skipped.
**Validates: Requirements 6.3, 6.5**

**Property 12: Registry Display Completeness**
*For any* set of locked folders in the FolderRegistry, the GUI should display all entries with their folder path, lock status, and last modified timestamp.
**Validates: Requirements 7.1, 7.2**

**Property 13: Folder Removal Cleanup**
*For any* locked folder removed from management, both the folder decryption and the FolderRegistry entry removal should complete successfully.
**Validates: Requirements 7.5**

**Property 14: Recursive Subfolder Encryption**
*For any* folder containing N files across all subdirectories, encryption should process all N files regardless of their depth in the folder hierarchy.
**Validates: Requirements 8.1**

**Property 15: Directory Structure Preservation**
*For any* folder with a specific directory tree structure, the relative paths of all subdirectories should remain identical after encryption.
**Validates: Requirements 8.2**

## Error Handling

### Error Categories

1. **User Input Errors**
   - Empty or invalid folder paths
   - Weak passwords (less than 8 characters)
   - Incorrect password attempts
   - Handling: Display user-friendly error messages, maintain current state

2. **File System Errors**
   - Insufficient permissions
   - Disk space exhaustion
   - File in use by another process
   - Handling: Rollback changes, restore from backup, log detailed error

3. **Encryption Errors**
   - Corrupted encrypted files
   - HMAC verification failures
   - Key derivation failures
   - Handling: Skip corrupted files, log errors, continue with valid files

4. **Service Communication Errors**
   - Service not running
   - IPC connection failures
   - Timeout errors
   - Handling: Attempt service restart, queue operations, notify user

### Error Recovery Strategy

```csharp
public class EncryptionOperation
{
    private string backupPath;
    
    public async Task<EncryptionResult> ExecuteWithRollback(Func<Task> operation)
    {
        try
        {
            // Create backup
            backupPath = await CreateBackupAsync();
            
            // Execute operation
            await operation();
            
            // Cleanup backup on success
            await DeleteBackupAsync(backupPath);
            
            return EncryptionResult.Success();
        }
        catch (Exception ex)
        {
            // Rollback from backup
            await RestoreFromBackupAsync(backupPath);
            
            return EncryptionResult.Failure(ex.Message);
        }
    }
}
```

## Testing Strategy

### Unit Testing Approach

The application will use xUnit as the testing framework with the following focus areas:

1. **Encryption Engine Tests**
   - Test AES-256 encryption/decryption with known inputs
   - Test PBKDF2 key derivation with specific parameters
   - Test HMAC generation and verification
   - Test password hash generation and verification

2. **Folder Registry Tests**
   - Test CRUD operations on locked folder entries
   - Test SQLite database initialization
   - Test concurrent access handling

3. **File System Operations Tests**
   - Test recursive file enumeration
   - Test backup creation and restoration
   - Test file extension handling (.locked)

4. **Service Communication Tests**
   - Test WCF service contract implementation
   - Test IPC message handling
   - Test error propagation across process boundaries

### Property-Based Testing Approach

The application will use FsCheck for .NET as the property-based testing library. Each property test will run a minimum of 100 iterations to ensure comprehensive coverage across random inputs.

**Property Test Requirements:**
- Each property-based test MUST be tagged with a comment referencing the correctness property from this design document
- Tag format: `// Feature: folder-encryption-lock, Property {number}: {property_text}`
- Each correctness property MUST be implemented by a SINGLE property-based test
- Tests should use FsCheck generators to create random folder structures, passwords, and file contents

**Key Property Tests:**

1. **Round-Trip Property Test**
   - Generate random folder structures with varying depths and file counts
   - Generate random passwords
   - Verify encryption followed by decryption restores original state
   - Tag: `// Feature: folder-encryption-lock, Property 1: Encryption-Decryption Round Trip`

2. **Password Rejection Property Test**
   - Generate random folders and two different passwords
   - Encrypt with password A, attempt decrypt with password B
   - Verify decryption fails and files remain encrypted
   - Tag: `// Feature: folder-encryption-lock, Property 3: Incorrect Password Rejection`

3. **Recursive Processing Property Test**
   - Generate random folder trees with varying depths (0-10 levels)
   - Count total files before encryption
   - Verify all files are encrypted regardless of depth
   - Tag: `// Feature: folder-encryption-lock, Property 14: Recursive Subfolder Encryption`

4. **Structure Preservation Property Test**
   - Generate random directory structures
   - Encrypt all files
   - Verify directory tree structure remains identical
   - Tag: `// Feature: folder-encryption-lock, Property 15: Directory Structure Preservation`

### Integration Testing

Integration tests will verify:
- GUI to Service communication via WCF
- Shell Extension to Service communication
- Service to Database persistence
- End-to-end lock/unlock workflows

### Test Data Generators

```csharp
public static class Generators
{
    public static Arbitrary<FolderStructure> FolderStructureGenerator()
    {
        return Arb.From(
            from depth in Gen.Choose(0, 5)
            from fileCount in Gen.Choose(1, 20)
            from folderName in Gen.Elements("TestFolder", "Data", "Documents")
            select CreateRandomFolderStructure(depth, fileCount, folderName)
        );
    }
    
    public static Arbitrary<SecureString> PasswordGenerator()
    {
        return Arb.From(
            from length in Gen.Choose(8, 32)
            from chars in Gen.ArrayOf(length, Gen.Elements(
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%"))
            select CreateSecureString(new string(chars))
        );
    }
}
```

## Technology Stack

- **Framework**: .NET 6.0 or later
- **GUI**: WPF (Windows Presentation Foundation)
- **Service**: Windows Service with TopShelf for easy installation
- **IPC**: WCF (Windows Communication Foundation) with Named Pipes
- **Database**: SQLite with Entity Framework Core
- **Encryption**: System.Security.Cryptography (AES, PBKDF2, HMAC-SHA256)
- **Shell Extension**: SharpShell library for COM interop
- **Testing**: xUnit for unit tests, FsCheck for property-based tests
- **Logging**: Serilog with file and event log sinks

## Security Considerations

1. **Key Management**
   - Keys are derived on-demand and never persisted
   - SecureString used for password handling in memory
   - Explicit memory clearing after operations

2. **File Integrity**
   - HMAC-SHA256 for tamper detection
   - Version headers for format compatibility
   - Atomic file operations to prevent corruption

3. **Access Control**
   - Service runs with minimal required privileges
   - File system ACLs preserved during encryption
   - Audit logging for all security-relevant operations

4. **Attack Mitigation**
   - Rate limiting on password attempts
   - Secure random number generation for salts and IVs
   - Protection against timing attacks in password verification

## Deployment and Installation

1. **Installer Package** (WiX Toolset)
   - Install GUI application to Program Files
   - Install and register Windows Service
   - Register Shell Extension COM component
   - Create application data directory for database
   - Configure auto-start for service

2. **First-Run Experience**
   - Service starts automatically
   - GUI shows welcome screen with tutorial
   - User can immediately lock folders

3. **Uninstallation**
   - Prompt to decrypt all locked folders
   - Unregister Shell Extension
   - Stop and remove Windows Service
   - Optionally remove database and logs
