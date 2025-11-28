# Requirements Document

## Introduction

This document specifies the requirements for a .NET GUI application that provides folder encryption and password-based locking functionality. The system enables users to encrypt folders, lock them with passwords, and unlock them through a seamless interface without requiring the main application to be running. The application operates as a background service with auto-start capabilities and integrates with Windows Explorer for direct folder interaction.

## Glossary

- **FolderLockApp**: The main .NET GUI application that manages folder encryption and locking operations
- **EncryptionService**: The background service component that handles encryption, decryption, and folder monitoring
- **LockedFolder**: A folder that has been encrypted and is currently in a locked state
- **ShellExtension**: A Windows Explorer integration component that enables right-click context menu actions
- **AutoStartService**: A Windows service or startup task that launches the EncryptionService automatically on system boot
- **PasswordDialog**: A GUI dialog that prompts users to enter passwords for unlock operations
- **EncryptionKey**: A cryptographic key derived from the user's password used for AES encryption
- **FolderRegistry**: A persistent storage mechanism that tracks all locked folders and their metadata

## Requirements

### Requirement 1

**User Story:** As a user, I want to select a folder through a GUI and encrypt it with a password, so that I can protect sensitive data from unauthorized access.

#### Acceptance Criteria

1. WHEN a user launches the FolderLockApp THEN the FolderLockApp SHALL display a main window with folder selection functionality
2. WHEN a user selects a folder for encryption THEN the FolderLockApp SHALL prompt for a password with confirmation
3. WHEN a user provides a valid password THEN the FolderLockApp SHALL encrypt all files within the selected folder using AES-256 encryption
4. WHEN encryption completes THEN the FolderLockApp SHALL mark the folder as locked in the FolderRegistry
5. WHEN a folder is locked THEN the FolderLockApp SHALL replace the folder icon with a locked icon indicator

### Requirement 2

**User Story:** As a user, I want to unlock an encrypted folder by clicking on it and entering my password, so that I can access my files without reopening the main application.

#### Acceptance Criteria

1. WHEN a user double-clicks a LockedFolder THEN the EncryptionService SHALL display the PasswordDialog
2. WHEN a user enters the correct password in the PasswordDialog THEN the EncryptionService SHALL decrypt all files in the LockedFolder
3. WHEN decryption completes successfully THEN the EncryptionService SHALL restore the original folder icon and allow normal access
4. WHEN a user enters an incorrect password THEN the EncryptionService SHALL display an error message and keep the folder locked
5. WHEN a user cancels the PasswordDialog THEN the EncryptionService SHALL keep the folder in locked state

### Requirement 3

**User Story:** As a user, I want the encryption service to run automatically when my computer starts, so that locked folders remain protected without manual intervention.

#### Acceptance Criteria

1. WHEN the FolderLockApp is installed THEN the AutoStartService SHALL register itself to launch on system startup
2. WHEN the system boots THEN the AutoStartService SHALL start the EncryptionService automatically
3. WHEN the EncryptionService starts THEN the EncryptionService SHALL load all LockedFolder entries from the FolderRegistry
4. WHEN the EncryptionService is running THEN the EncryptionService SHALL monitor all registered LockedFolder locations for access attempts
5. WHILE the EncryptionService is active THEN the EncryptionService SHALL remain running as a background process without displaying a window

### Requirement 4

**User Story:** As a user, I want to interact with locked folders through Windows Explorer context menus, so that I can lock and unlock folders conveniently.

#### Acceptance Criteria

1. WHEN a user right-clicks on any folder THEN the ShellExtension SHALL display a "Lock Folder" option in the context menu
2. WHEN a user selects "Lock Folder" from the context menu THEN the ShellExtension SHALL invoke the FolderLockApp encryption workflow
3. WHEN a user right-clicks on a LockedFolder THEN the ShellExtension SHALL display an "Unlock Folder" option in the context menu
4. WHEN a user selects "Unlock Folder" from the context menu THEN the ShellExtension SHALL display the PasswordDialog for authentication

### Requirement 5

**User Story:** As a user, I want my password to be securely handled and never stored in plain text, so that my encryption remains secure even if the system is compromised.

#### Acceptance Criteria

1. WHEN a user creates a password for encryption THEN the FolderLockApp SHALL derive an EncryptionKey using PBKDF2 with at least 100000 iterations
2. WHEN the FolderLockApp stores password verification data THEN the FolderLockApp SHALL store only a salted hash and never the plain text password
3. WHEN a user enters a password for unlocking THEN the EncryptionService SHALL verify the password against the stored hash before decryption
4. WHEN encryption or decryption occurs THEN the EncryptionService SHALL use AES-256 in CBC mode with a random initialization vector
5. WHEN the EncryptionService completes any operation THEN the EncryptionService SHALL clear all password and key data from memory

### Requirement 6

**User Story:** As a user, I want the application to handle errors gracefully during encryption and decryption, so that I don't lose data if something goes wrong.

#### Acceptance Criteria

1. WHEN encryption begins THEN the FolderLockApp SHALL create a backup of the folder structure metadata
2. IF encryption fails during processing THEN the FolderLockApp SHALL restore the original files from backup and notify the user
3. WHEN decryption begins THEN the EncryptionService SHALL verify the integrity of encrypted files before processing
4. IF decryption fails due to incorrect password THEN the EncryptionService SHALL preserve the encrypted files and display an error message
5. IF the EncryptionService encounters corrupted encrypted files THEN the EncryptionService SHALL log the error and skip the corrupted files while processing others

### Requirement 7

**User Story:** As a user, I want to see the status of my locked folders in the main application, so that I can manage all my encrypted folders from one place.

#### Acceptance Criteria

1. WHEN a user opens the FolderLockApp THEN the FolderLockApp SHALL display a list of all LockedFolder entries from the FolderRegistry
2. WHEN displaying locked folders THEN the FolderLockApp SHALL show the folder path, lock status, and last modified timestamp
3. WHEN a user selects a LockedFolder from the list THEN the FolderLockApp SHALL enable unlock and remove options
4. WHEN a user chooses to remove a LockedFolder THEN the FolderLockApp SHALL prompt for password confirmation before removing the entry
5. WHEN a LockedFolder is removed from management THEN the FolderLockApp SHALL decrypt the folder and remove it from the FolderRegistry

### Requirement 8

**User Story:** As a user, I want the application to handle subfolder encryption properly, so that all nested content is protected.

#### Acceptance Criteria

1. WHEN a folder contains subfolders THEN the FolderLockApp SHALL recursively encrypt all files in all subdirectories
2. WHEN encrypting files THEN the FolderLockApp SHALL preserve the original directory structure
3. WHEN decrypting a LockedFolder THEN the EncryptionService SHALL restore all files to their original locations within the folder hierarchy
4. WHEN encryption processes files THEN the FolderLockApp SHALL replace original files with encrypted versions using a .locked extension
5. WHEN decryption completes THEN the EncryptionService SHALL remove the .locked extension and restore original file names
