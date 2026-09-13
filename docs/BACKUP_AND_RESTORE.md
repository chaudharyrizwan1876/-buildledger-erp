# Backup & Restore

## How backups work
BuildLedger ERP uses SQLite's `VACUUM INTO` command to create backups. This copies the database
into a new file inside a single atomic operation, so a backup taken while the app is in use is
always transaction-consistent — never a half-written or corrupted copy, which is the main risk of
plain file-copying a live SQLite database.

- **Automatic backup**: runs once daily at the time configured in Settings (default 20:00), saved to
  `%LOCALAPPDATA%\BuildLedgerERP\Backups\`.
- **Manual backup**: from the Backup & Restore screen, "Create Backup Now" (same default folder) or
  "Backup To Folder..." to choose any destination (USB drive, network share, cloud-synced folder).
- Every backup — automatic or manual — is recorded in **Backup History** with its file path, size,
  timestamp, and success/failure status.

## Important: off-drive copies
A backup saved to the same hard drive as the live database does **not** protect you against a
drive failure, theft, or a ransomware/malware incident. Treat the in-app backup as step one only.
Recommended practice:
1. Use "Backup To Folder..." at least weekly to copy onto a USB drive or external disk kept
   off-site (or in a fireproof box).
2. Point backups at a folder synced by OneDrive/Google Drive/Dropbox for an automatic off-site copy.
3. Periodically verify a backup file actually opens (see Restore below) — an untested backup is not
   a backup.

## Restoring a backup
From Backup & Restore, click "Restore From File...", pick a `.db` backup file, and confirm.
Before overwriting anything, the app automatically takes a **safety backup** of the current
database, so a bad restore can itself be undone. After a restore completes, restart the application
so it opens the newly restored file cleanly.

## Manual recovery (advanced)
If the app cannot start, the database file lives at:
```
%LOCALAPPDATA%\BuildLedgerERP\buildledger.db
```
You can replace this file directly with any backup `.db` file (with the app closed) and relaunch.
