# Implementation Plan

- [x] 1. Set up project structure and core solution
  - Create .NET solution with core library project
  - Set up NuGet packages: Entity Framework Core, SQLite, FsCheck, xUnit
  - Create folder structure for models, services, interfaces, data, and tests
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

- [x] 5.3 Write property test for recursive subfolder encryption
  - **Property 14: Recursive Subfolder Encryption**
  - **Validates: Requirements 8.1**

- [x] 5.4 Write property test for directory structure preservation
  - **Property 15: Directory Structure Preservation**
  - **Validates: Requirements 8.2**

- [x] 6. Implement folder encryption workflow
  - EncryptFolderAsync method with progress reporting already implemented
  - File-by-file encryption with error handling complete
  - Integrity verification with HMAC implemented
  - Automatic rollback on failure implemented
  - _Requirements: 1.3, 1.4, 6.1, 6.2, 8.1, 8.2_

- [x] 7. Implement folder decryption workflow
  - DecryptFolderAsync method with progress reporting already implemented
  - Password verification before decryption implemented
  - File-by-file decryption with error handling complete
  - Original filenames and directory structure restoration implemented
  - Corrupted file handling implemented (HMAC verification)
  - _Requirements: 2.2, 2.3, 6.3, 6.4, 6.5, 8.3, 8.5_

- [x] 8. Checkpoint - Core encryption functionality complete
  - All core encryption/decryption tests passing
  - Property-based tests validating correctness properties

- [x] 9. Create WPF GUI application project





  - Create new WPF application project in solution
  - Add reference to FolderLockApp.Core
  - Set up MVVM structure with ViewModels and Views folders
  - Configure dependency injection for services
  - _Requirements: 1.1, 7.1_

- [x] 10. Implement main window UI and folder management





  - Create MainWindow XAML with folder list view
  - Implement MainWindowViewModel with observable collection of locked folders
  - Add folder selection dialog using FolderBrowserDialog
  - Create password input dialog with confirmation
  - Display locked folders with path, status, and timestamp
  - Implement Lock Folder command and workflow
  - Implement Unlock Folder command and workflow
  - Implement Remove Folder command with password confirmation
  - Add progress indicators for encryption/decryption operations
  - Wire up FolderRegistry and EncryptionEngine services
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 2.2, 2.3, 7.1, 7.2, 7.3, 7.4, 7.5_

- [ ]* 10.1 Write property test for registry display completeness
  - **Property 12: Registry Display Completeness**
  - **Validates: Requirements 7.1, 7.2**

- [ ]* 10.2 Write property test for folder removal cleanup
  - **Property 13: Folder Removal Cleanup**
  - **Validates: Requirements 7.5**

- [-] 11. Implement password dialog window


  - Create PasswordDialog WPF window
  - Implement password input with masking (PasswordBox)
  - Add OK and Cancel button handlers
  - Show error messages for incorrect passwords
  - Return SecureString to calling code
  - Add password strength indicator
  - _Requirements: 1.2, 2.1, 2.4, 2.5_

- [ ] 12. Implement Windows background service project
  - Create new Windows Service project using Worker Service template
  - Add reference to FolderLockApp.Core
  - Implement BackgroundService with StartAsync and StopAsync
  - Load all locked folders from registry on startup
  - Configure service to run as Windows Service
  - Add Serilog logging to file and Windows Event Log
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

- [ ] 13. Implement folder monitoring in background service
  - Use FileSystemWatcher to monitor locked folder access attempts
  - Detect when user tries to access locked folder
  - Show password dialog when access is attempted
  - Decrypt folder on successful password entry
  - Log all access attempts and outcomes
  - _Requirements: 2.1, 3.4, 3.5_

- [ ] 14. Implement IPC communication between GUI and service
  - Create shared IPC contract interface
  - Implement Named Pipes server in background service
  - Implement Named Pipes client in GUI application
  - Add methods: LockFolder, UnlockFolder, RemoveFolder, GetLockedFolders
  - Handle connection failures and timeouts gracefully
  - _Requirements: 1.3, 2.2, 7.1, 7.5_

- [ ]* 14.1 Write integration tests for IPC communication
  - Test service method invocations and responses
  - Test error propagation across IPC boundary
  - Test connection retry logic
  - _Requirements: 1.3, 2.2_

- [ ] 15. Implement Shell Extension with SharpShell
  - Create new Class Library project for shell extension
  - Add SharpShell NuGet package
  - Create COM-visible context menu handler class
  - Add "Lock Folder" menu item for unlocked folders
  - Add "Unlock Folder" menu item for locked folders
  - Communicate with background service via Named Pipes
  - Register COM component using SharpShell server registration
  - _Requirements: 4.1, 4.2, 4.3, 4.4_

- [ ]* 15.1 Write integration tests for shell extension
  - Test context menu item visibility based on folder state
  - Test service invocation from shell extension
  - _Requirements: 4.1, 4.2, 4.3, 4.4_

- [ ] 16. Implement locked folder icon overlay
  - Create icon overlay handler class in shell extension project
  - Implement IShellIconOverlayIdentifier interface
  - Check folder lock status from registry
  - Display lock icon badge on locked folder icons
  - Register overlay handler with Windows Shell
  - _Requirements: 1.5_

- [ ] 17. Configure service auto-start and installation
  - Configure Windows Service to start automatically on boot
  - Create service installer using sc.exe or PowerShell scripts
  - Set service recovery options for automatic restart on failure
  - Configure service to run with LocalSystem or appropriate privileges
  - Add service installation to setup process
  - _Requirements: 3.1, 3.2_

- [ ] 18. Enhance error handling and logging
  - Review all critical operations for proper try-catch blocks
  - Implement specific error handlers for file system errors (permissions, disk space)
  - Implement specific error handlers for encryption errors (corrupted files)
  - Configure Serilog with file rolling policy and size limits
  - Log all security-relevant operations (lock, unlock, failed password attempts)
  - Create user-friendly error message dialogs for common scenarios
  - _Requirements: 6.2, 6.4, 6.5_

- [ ]* 18.1 Write unit tests for error handling scenarios
  - Test insufficient permissions handling
  - Test disk space exhaustion handling
  - Test corrupted file handling
  - _Requirements: 6.2, 6.4, 6.5_

- [ ] 19. Create installer package
  - Create WiX installer project or use ClickOnce
  - Install GUI application to Program Files
  - Install and register Windows Service
  - Register Shell Extension COM component
  - Create application data directory for SQLite database
  - Add uninstaller with option to decrypt all folders before removal
  - Create desktop shortcut and start menu entry
  - _Requirements: 3.1_

- [ ] 20. Implement first-run experience
  - Detect first application launch using registry or settings file
  - Show welcome dialog with feature overview
  - Provide quick tutorial for locking first folder
  - Initialize database on first run
  - Ensure service is started on first run
  - _Requirements: 1.1_

- [ ] 21. Final integration testing and polish
  - Test complete lock/unlock workflow from GUI
  - Test complete lock/unlock workflow from Explorer context menu
  - Test service restart and folder state persistence
  - Test multiple folders locked simultaneously
  - Test edge cases (empty folders, large folders, deep nesting)
  - Verify all property-based tests pass
  - Verify all unit tests pass
  - _Requirements: All_
