# FolderLockApp Testing Guide

This guide will walk you through testing all implemented features of the FolderLockApp.

## ✅ Prerequisites

- Windows 10/11
- .NET 8 Runtime
- Administrator privileges (for service installation)
- Application built in Release mode

## 🧪 Test Plan

### Test 1: GUI Application Launch

**Objective:** Verify the GUI application starts correctly

**Steps:**
1. Navigate to `FolderLockApp.GUI\bin\Release\net8.0-windows\`
2. Double-click `FolderLockApp.GUI.exe`
3. Application window should appear

**Expected Result:**
- Window titled "Folder Lock Application" appears
- Empty list view is displayed
- Buttons visible: Lock Folder, Unlock, Remove, Refresh
- Status bar at bottom
- No error messages

**Status:** ✅ PASS / ❌ FAIL

---

### Test 2: Folder Encryption

**Objective:** Test encrypting a folder with password

**Steps:**
1. Create a test folder on your desktop: `TestFolder`
2. Add 2-3 test files (txt, images, etc.)
3. In FolderLockApp, click "Lock Folder"
4. Select your `TestFolder`
5. Enter password: `Test1234`
6. Confirm password: `Test1234`
7. Wait for encryption to complete

**Expected Result:**
- Progress bar shows encryption progress
- Success message appears
- Folder appears in the locked folders list
- Files in folder now have `.locked` extension
- Files are encrypted (unreadable when opened)

**Status:** ✅ PASS / ❌ FAIL

---

### Test 3: Folder Decryption

**Objective:** Test decrypting a locked folder

**Steps:**
1. Select the locked folder from the list
2. Click "Unlock" button
3. Enter the correct password: `Test1234`
4. Wait for decryption to complete

**Expected Result:**
- Password dialog appears
- Progress bar shows decryption progress
- Success message appears
- Files restored to original names (no `.locked` extension)
- Files are readable again
- Folder disappears from locked folders list

**Status:** ✅ PASS / ❌ FAIL

---

### Test 4: Wrong Password

**Objective:** Verify incorrect password is rejected

**Steps:**
1. Lock a test folder with password `Test1234`
2. Try to unlock with wrong password `WrongPass`

**Expected Result:**
- Error message: "Incorrect password" or "HMAC verification failed"
- Folder remains locked
- Files still encrypted

**Status:** ✅ PASS / ❌ FAIL

---

### Test 5: Database Persistence

**Objective:** Verify locked folders persist across app restarts

**Steps:**
1. Lock a test folder
2. Close the FolderLockApp GUI
3. Reopen the FolderLockApp GUI
4. Click "Refresh" button

**Expected Result:**
- Previously locked folder still appears in the list
- Folder details (path, date) are correct

**Status:** ✅ PASS / ❌ FAIL

---

### Test 6: Service Installation

**Objective:** Install the background service

**Steps:**
1. Right-click `Install-Service.bat`
2. Select "Run as Administrator"
3. Wait for installation to complete

**Expected Result:**
- Success message: "Service installed and started successfully!"
- Service details displayed
- No errors

**Verification:**
```powershell
.\Manage-Service.ps1 -Action Status
```

**Expected Output:**
- Status: Running
- Startup Type: Automatic

**Status:** ✅ PASS / ❌ FAIL

---

### Test 7: Service Auto-Start

**Objective:** Verify service starts automatically on boot

**Steps:**
1. Ensure service is installed (Test 6)
2. Restart your computer
3. After boot, check service status:
   ```powershell
   .\Manage-Service.ps1 -Action Status
   ```

**Expected Result:**
- Service status: Running
- Service started automatically without manual intervention

**Status:** ✅ PASS / ❌ FAIL

---

### Test 8: Service Management

**Objective:** Test starting/stopping the service

**Steps:**
1. Stop the service:
   ```powershell
   .\Manage-Service.ps1 -Action Stop
   ```
2. Verify it's stopped:
   ```powershell
   .\Manage-Service.ps1 -Action Status
   ```
3. Start the service:
   ```powershell
   .\Manage-Service.ps1 -Action Start
   ```
4. Verify it's running:
   ```powershell
   .\Manage-Service.ps1 -Action Status
   ```

**Expected Result:**
- Service stops successfully
- Service starts successfully
- Status changes correctly

**Status:** ✅ PASS / ❌ FAIL

---

### Test 9: Nested Folders

**Objective:** Test encryption of folders with subfolders

**Steps:**
1. Create folder structure:
   ```
   TestFolder/
   ├── file1.txt
   ├── SubFolder1/
   │   ├── file2.txt
   │   └── SubFolder2/
   │       └── file3.txt
   ```
2. Lock `TestFolder`
3. Verify all files are encrypted

**Expected Result:**
- All files in all subfolders have `.locked` extension
- Directory structure preserved
- All files encrypted

**Status:** ✅ PASS / ❌ FAIL

---

### Test 10: Large Files

**Objective:** Test encryption of larger files

**Steps:**
1. Create a test folder with a 10MB+ file
2. Lock the folder
3. Unlock the folder

**Expected Result:**
- Large file encrypts successfully
- Large file decrypts successfully
- File content intact after decryption

**Status:** ✅ PASS / ❌ FAIL

---

### Test 11: Special Characters in Filenames

**Objective:** Test files with special characters

**Steps:**
1. Create files with names like:
   - `file with spaces.txt`
   - `file-with-dashes.txt`
   - `file_with_underscores.txt`
2. Lock the folder
3. Unlock the folder

**Expected Result:**
- All files encrypt successfully
- All files decrypt successfully
- Filenames preserved correctly

**Status:** ✅ PASS / ❌ FAIL

---

### Test 12: Empty Folder

**Objective:** Test locking an empty folder

**Steps:**
1. Create an empty folder
2. Try to lock it

**Expected Result:**
- Either: Success message (0 files processed)
- Or: Warning message about empty folder

**Status:** ✅ PASS / ❌ FAIL

---

### Test 13: Deleted Folder Cleanup

**Objective:** Test handling of deleted folders

**Steps:**
1. Lock a test folder
2. Manually delete the folder from disk
3. In FolderLockApp, try to unlock the deleted folder

**Expected Result:**
- Error message: "Folder no longer exists"
- Option to remove from list
- Clicking "Yes" removes it from the list

**Status:** ✅ PASS / ❌ FAIL

---

### Test 14: Service Logs

**Objective:** Verify service logging works

**Steps:**
1. Ensure service is running
2. Lock/unlock a folder
3. Check logs at: `C:\ProgramData\FolderLockApp\Logs\`

**Expected Result:**
- Log files exist
- Log entries for service start
- Log entries for operations

**Status:** ✅ PASS / ❌ FAIL

---

### Test 15: Database Location

**Objective:** Verify database is created correctly

**Steps:**
1. Lock a folder
2. Check: `C:\ProgramData\FolderLockApp\folderlock.db`

**Expected Result:**
- Database file exists
- File size > 0 bytes

**Status:** ✅ PASS / ❌ FAIL

---

## 📊 Test Summary

| Test # | Test Name | Status | Notes |
|--------|-----------|--------|-------|
| 1 | GUI Launch | ⬜ | |
| 2 | Folder Encryption | ⬜ | |
| 3 | Folder Decryption | ⬜ | |
| 4 | Wrong Password | ⬜ | |
| 5 | Database Persistence | ⬜ | |
| 6 | Service Installation | ⬜ | |
| 7 | Service Auto-Start | ⬜ | |
| 8 | Service Management | ⬜ | |
| 9 | Nested Folders | ⬜ | |
| 10 | Large Files | ⬜ | |
| 11 | Special Characters | ⬜ | |
| 12 | Empty Folder | ⬜ | |
| 13 | Deleted Folder | ⬜ | |
| 14 | Service Logs | ⬜ | |
| 15 | Database Location | ⬜ | |

## 🐛 Known Issues

1. **Shell Extension not implemented** - Right-click context menu not available
2. **Icon overlay not implemented** - No visual indicator on locked folders in Explorer
3. **Decryption may fail** - If you encounter issues, ensure:
   - Using the correct password
   - Folder still exists
   - Files have `.locked` extension

## 📝 Reporting Issues

When reporting issues, please include:
- Test number that failed
- Exact error message
- Steps to reproduce
- Log files from `C:\ProgramData\FolderLockApp\Logs\`

## ✅ Quick Test Checklist

For a quick smoke test, run these essential tests:
- [ ] Test 1: GUI Launch
- [ ] Test 2: Folder Encryption
- [ ] Test 3: Folder Decryption
- [ ] Test 6: Service Installation
- [ ] Test 7: Service Auto-Start

If all 5 pass, the core functionality is working!
