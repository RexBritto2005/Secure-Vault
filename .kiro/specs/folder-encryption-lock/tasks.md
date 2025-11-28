# Implementation Plan

- [ ] 1. Set up project structure and core solution
  - Create .NET solution with multiple projects: WPF GUI app, Windows Service, Shell Extension library, and shared core library
  - Configure project references and dependencies
  - Set up NuGet packages: Entity Framework Core, SQLite, FsCheck, xUnit, Serilog, SharpShell
  - Create folder structure for models, services, repositories, and views
  - _Requirements: All_

- [x] 2. Implement encryption engine core








  - Create IEncryptionEngine interface and implementation
  - Implement AES-256 encryption in CBC mode with random IV generation
  - Implement PBKDF2 key derivation with configurable iteration count (minimum 100,000)
  - Implement HMAC-SHA256 for file integrity verification
  - Create encrypted file format with header structure (magic number, version, salt, IV)
  - _Requirements: 1.3, 5.1, 5.4_

- [x] 2.1 Write property test for AES-256 CBC with random IV






  - **Property 8: AES-256 CBC with Random IV**
  - **Validates: Requirements 5.4**

- [x] 2.2 Write property test for PBKDF2 key derivation parameters






  - **Property 5: PBKDF2 Key Derivation Parameters**
  - **Validates: Requirements 5.1**

- [x] 3. Implement password handling and verification





  - Create password hashing using PBKDF2 with salt generation
  - Implement password verification against stored hashes
  - Use SecureString for in-memory password handling
  - Create password validation rules (minimum 8 characters)
  - _Requirements: 5.1, 5.2, 5.3_

- [x] 3.1 Write property test for password storage security






  - **Property 6: Password Storage Security**
  - **Validates: Requirements 5.2**

- [x] 3.2 Write property test for password verification before decryption







  - **Property 7: Password Verification Before Decryption**
  - **Validates: Requirements 5.3**

- [x] 4. Implement folder registry with SQLite database





  - Create LockedFolderEntry model class
  - Create IFolderRegistry interface and implementation
  - Set up Entity Framework Core with SQLite provider
  - Implement database initialization and migration
  - Implement CRUD operations for locked folder entries
  - Create indexes for folder path lookups
  - _Requirements: 1.4, 3.3, 7.1_

- [x] 4.1 Write property test for folder registry consistency






  - **Property 2: Folder Registry Consistency**
  - **Validates: Requirements 1.4**

- [x] 4.2 Write property test for service initialization loads all entries





  - **Property 4: Service Initialization Loads All Entries**
  - **Validates: Requirements 3.3**

- [x] 4.3 Write unit tests for folder registry operations






  - Test adding, retrieving, updating, and removing locked folder entries
  - Test database initialization and schema creation
  - _Requirements: 1.4, 7.1_

- [x] 5. Implement file system operations with backup and rollback










  - Create recursive file enumeration for folder traversal
  - Implement backup creation for folder structure metadata
  - Implement rollback mechanism to restore from backup on failure
  - Create file encryption with .locked extension handling
  - Create file decryption with extension removal
  - Implement atomic file operations to prevent corruption
  - _Requirements: 6.1, 6.2, 8.1, 8.4, 8.5_

- [x] 5.1 Write property test for backup creation before encryption






  - **Property 9: Backup Creation Before Encryption**
  - **Validates: Requirements 6.1**

- [x] 5.2 Write property test for rollback on encryption failure






  - **Property 10: Rollback on Encryption Failure**
  - **Validates: Requirements 6.2**
-

- [x] 5.3 Write property test for recursive subfolder encryption





  - **Property 14: Recursive Subfolder Encryption**
  - **Validates: Requirements 8.1**

- [x] 5.4 Write property test for directory structure preservation











  - **Property 15: Directory Structure Preservation**
  - **Validates: Requirements 8.2**

- [ ] 6. Implement folder encryption workflow

  - Create EncryptFolderAsync method with progress reporting
  - Implement file-by-file encryption with error handling
  - Add folder to registry after successful encryption
  - Implement integrity verification with HMAC
  - Handle encryption failures with automatic rollback
  - _Requirements: 1.3, 1.4, 6.1, 6.2, 8.1, 8.2_

- [ ]* 6.1 Write property test for integrity verification before decryption
  - **Property 11: Integrity Verification Before Decryption**
  - **Validates: Requirements 6.3, 6.5**

- [ ] 7. Implement folder decryption workflow
  - Create DecryptFolderAsync method with progress reporting
  - Implement password verification before decryption
  - Implement file-by-file decryption with error handling
  - Restore original filenames and directory structure
  - Handle corrupted files gracefully (skip and log)
  - _Requirements: 2.2, 2.3, 6.3, 6.4, 6.5, 8.3, 8.5_

- [ ]* 7.1 Write property test for encryption-decryption round trip
  - **Property 1: Encryption-Decryption Round Trip**
  - **Validates: Requirements 1.3, 2.2, 2.3, 8.2, 8.3, 8.5**

- [ ]* 7.2 Write property test for incorrect password rejection
  - **Property 3: Incorrect Password Rejection**
  - **Validates: Requirements 2.4, 6.4**

- [ ] 8. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 9. Implement WCF service contract for IPC
  - Create IEncryptionServiceContract interface with service operations
  - Implement LockFolderAsync, UnlockFolderAsync, RemoveFolderAsync methods
  - Implement GetLockedFoldersAsync for GUI synchronization
  - Configure Named Pipes binding for local IPC
  - Add error handling and timeout configuration
  - _Requirements: 1.3, 2.2, 7.1, 7.5_

- [ ]* 9.1 Write unit tests for service contract operations
  - Test service method invocations and responses
  - Test error propagation across IPC boundary
  - _Requirements: 1.3, 2.2_

- [ ] 10. Implement Windows background service
  - Create EncryptionService class inheriting from ServiceBase
  - Implement service startup and shutdown logic
  - Load all locked folders from registry on startup
  - Host WCF service endpoint for IPC communication
  - Implement folder monitoring for access attempts
  - Configure Serilog logging to file and event log
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

- [ ] 11. Implement password dialog for unlock requests
  - Create WPF password dialog window
  - Implement password input with masking
  - Add OK and Cancel button handlers
  - Show error messages for incorrect passwords
  - Return SecureString to calling service
  - _Requirements: 1.2, 2.1, 2.4, 2.5_

- [ ] 12. Implement WPF GUI main window
  - Create main window XAML layout with folder list view
  - Implement folder selection dialog
  - Create password input dialog with confirmation
  - Display list of locked folders with path, status, and timestamp
  - Implement Lock Folder button and workflow
  - Implement Unlock Folder button and workflow
  - Implement Remove Folder button with password confirmation
  - Add progress indicators for long-running operations
  - _Requirements: 1.1, 1.2, 7.1, 7.2, 7.3, 7.4, 7.5_

- [ ]* 12.1 Write property test for registry display completeness
  - **Property 12: Registry Display Completeness**
  - **Validates: Requirements 7.1, 7.2**

- [ ]* 12.2 Write property test for folder removal cleanup
  - **Property 13: Folder Removal Cleanup**
  - **Validates: Requirements 7.5**

- [ ] 13. Implement WCF client in GUI application
  - Create service client proxy for IEncryptionServiceContract
  - Implement connection management and retry logic
  - Handle service unavailable scenarios
  - Implement async/await patterns for service calls
  - Add timeout handling for long operations
  - _Requirements: 1.3, 2.2, 7.1, 7.5_

- [ ] 14. Implement Shell Extension with SharpShell
  - Create COM-visible shell context menu handler class
  - Implement IShellContextMenuHandler interface
  - Add "Lock Folder" menu item for unlocked folders
  - Add "Unlock Folder" menu item for locked folders
  - Invoke WCF service for lock/unlock operations
  - Register COM component in Windows registry
  - _Requirements: 4.1, 4.2, 4.3, 4.4_

- [ ]* 14.1 Write integration tests for shell extension
  - Test context menu item visibility based on folder state
  - Test service invocation from shell extension
  - _Requirements: 4.1, 4.2, 4.3, 4.4_

- [ ] 15. Implement service auto-start configuration
  - Configure Windows Service to start automatically on boot
  - Create service installer with TopShelf
  - Set service recovery options for automatic restart
  - Configure service to run with appropriate privileges
  - _Requirements: 3.1, 3.2_

- [ ] 16. Implement locked folder icon overlay
  - Create icon overlay handler for locked folders
  - Register overlay handler with Windows Shell
  - Display lock icon badge on locked folder icons
  - Update icon when folder is locked/unlocked
  - _Requirements: 1.5_

- [ ] 17. Implement error handling and logging
  - Add try-catch blocks for all critical operations
  - Implement specific error handlers for file system, encryption, and service errors
  - Configure Serilog with file rolling and event log sinks
  - Log all security-relevant operations (lock, unlock, failed attempts)
  - Create user-friendly error messages for common scenarios
  - _Requirements: 6.2, 6.4, 6.5_

- [ ]* 17.1 Write unit tests for error handling scenarios
  - Test insufficient permissions handling
  - Test disk space exhaustion handling
  - Test corrupted file handling
  - _Requirements: 6.2, 6.4, 6.5_

- [ ] 18. Create WiX installer project
  - Create WiX installer project for application deployment
  - Install GUI application to Program Files
  - Install and register Windows Service
  - Register Shell Extension COM component
  - Create application data directory for SQLite database
  - Add uninstaller with option to decrypt all folders
  - _Requirements: 3.1_

- [ ] 19. Implement first-run experience
  - Detect first application launch
  - Show welcome screen with feature overview
  - Provide quick tutorial for locking first folder
  - Initialize database and service on first run
  - _Requirements: 1.1_

- [ ] 20. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.
