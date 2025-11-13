# Testing Guide: Pre-Import Feature for qBittorrent

This guide will walk you through testing the Pre-Import feature on Ubuntu (including Raspberry Pi 4).

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Download and Build](#download-and-build)
3. [Run Unit Tests](#run-unit-tests)
4. [Set Up Test Environment](#set-up-test-environment)
5. [Manual Testing](#manual-testing)
6. [Verification Checklist](#verification-checklist)
7. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

```bash
# Update package list
sudo apt update

# Install .NET SDK 6.0 (required for building Radarr)
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 6.0
export PATH="$PATH:$HOME/.dotnet"
echo 'export PATH="$PATH:$HOME/.dotnet"' >> ~/.bashrc

# Verify installation
dotnet --version  # Should show 6.0.x

# Install Node.js 20.x (for frontend)
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs

# Verify Node version
node --version  # Should show v20.x

# Enable Yarn (comes with Node 20+)
corepack enable

# Install Git (if not already installed)
sudo apt install -y git

# Install qBittorrent (for testing)
sudo apt install -y qbittorrent-nox

# Optional: Install build essentials
sudo apt install -y build-essential
```

### Disk Space Requirements
- **Source code**: ~500MB
- **Build output**: ~200MB
- **Test data**: ~100MB
- **Total**: ~1GB recommended

---

## Download and Build

### Step 1: Clone the Repository

```bash
# Create a workspace directory
mkdir -p ~/radarr-test
cd ~/radarr-test

# Clone your fork
git clone https://github.com/philhar88/Radarr.git
cd Radarr

# Checkout the Pre-Import feature branch
git checkout claude/radarr-qbitorrent-integration-011CV4uLuxwDNwXo6xv8FPXC

# Verify you're on the correct branch
git branch  # Should show * claude/radarr-qbitorrent-integration-011CV4uLuxwDNwXo6xv8FPXC

# View the commits
git log --oneline -5
```

Expected output:
```
3180263 Add comprehensive PR description template
6e4eb7f Add unit tests and code documentation for Pre-Import feature
908c221 Add Pre-Import feature for qBittorrent download client
...
```

### Step 2: Install Frontend Dependencies

```bash
# Install Node packages
yarn install

# This will take a few minutes on first run
```

### Step 3: Build the Backend

```bash
# Clean any previous builds
dotnet clean src/Radarr.sln -c Debug

# Restore and build for Linux ARM64 (Raspberry Pi 4)
dotnet msbuild -restore src/Radarr.sln \
  -p:Configuration=Debug \
  -p:Platform=Posix \
  -t:PublishAllRids

# For regular x64 Linux, use:
# dotnet msbuild -restore src/Radarr.sln -p:Configuration=Debug -p:Platform=Posix

# This will take 10-20 minutes on Raspberry Pi 4
```

**Build output location**: `_output/`

### Step 4: Build the Frontend

```bash
# Start webpack build (in a separate terminal if you want live reload)
yarn build

# Or for production build:
yarn build --production
```

---

## Run Unit Tests

### Step 1: Run All Tests

```bash
# Run all unit tests
./test.sh linux unit

# Or run just the qBittorrent tests
dotnet test src/NzbDrone.Core.Test/Radarr.Core.Test.csproj \
  --filter "FullyQualifiedName~QBittorrent"
```

### Step 2: Run Specific Pre-Import Tests

```bash
# Run only the new Pre-Import tests
dotnet test src/NzbDrone.Core.Test/Radarr.Core.Test.csproj \
  --filter "FullyQualifiedName~Download_should_use_savepath" \
  --logger "console;verbosity=detailed"
```

**Expected results**: All 8 Pre-Import tests should pass ✅

Example output:
```
Passed Download_should_not_use_savepath_when_preimport_disabled
Passed Download_should_use_savepath_when_preimport_enabled_with_valid_movie_path
Passed Download_should_not_use_savepath_when_preimport_enabled_but_movie_path_is_null
Passed Download_should_not_use_savepath_when_preimport_enabled_but_movie_path_is_empty
Passed Download_from_magnet_should_use_savepath_when_preimport_enabled
Passed Download_from_magnet_should_not_use_savepath_when_preimport_disabled
Passed Download_should_not_use_savepath_when_movie_is_null

Test Run Successful.
Total tests: 8
     Passed: 8
```

---

## Set Up Test Environment

### Step 1: Configure qBittorrent

```bash
# Start qBittorrent Web UI
qbittorrent-nox &

# Default credentials:
# URL: http://localhost:8080
# Username: admin
# Password: adminadmin (change on first login!)

# Configure qBittorrent:
# 1. Go to Options → Downloads
# 2. Set "Default Save Path" to: /home/YOUR_USER/downloads/qbittorrent/
# 3. (Optional) Enable "Append .!qB extension to incomplete files"
# 4. Go to Options → Web UI
# 5. Note the port (default 8080)
```

### Step 2: Create Test Directory Structure

```bash
# Create directories
mkdir -p ~/movies/test-movie-2024
mkdir -p ~/downloads/qbittorrent/complete
mkdir -p ~/test-torrents

# Set permissions
chmod -R 755 ~/movies
chmod -R 755 ~/downloads
```

### Step 3: Start Radarr

```bash
# Navigate to output directory
cd ~/radarr-test/Radarr/_output

# Find the correct binary for your architecture
# For Raspberry Pi 4 (ARM64):
ls -la linux-arm64/

# Run Radarr
./linux-arm64/Radarr

# Or for x64:
# ./linux-x64/Radarr

# Radarr will start on: http://localhost:7878
```

**First-time setup:**
1. Open browser to `http://localhost:7878`
2. Complete the setup wizard
3. Skip authentication for now (local testing)

### Step 4: Configure Radarr

#### Add Root Folder
1. Settings → Media Management
2. Root Folders → Add Root Folder
3. Path: `/home/YOUR_USER/movies`
4. Click "OK"

#### Add qBittorrent Download Client
1. Settings → Download Clients
2. Click the "+" button
3. Select "qBittorrent"
4. Configure:
   - **Name**: qBittorrent Test
   - **Host**: localhost
   - **Port**: 8080
   - **Username**: admin
   - **Password**: (your password)
   - **Category**: radarr-test
   - **✅ Pre-Import**: **UNCHECKED** (test default behavior first)
5. Click "Test" - should show success ✅
6. Click "Save"

#### Add a Test Movie
1. Movies → Add New Movie
2. Search for: "Big Buck Bunny" (open source test movie)
3. Select it
4. Root Folder: `/home/YOUR_USER/movies`
5. Monitor: Yes
6. Add Movie

---

## Manual Testing

### Test 1: Normal Behavior (Pre-Import Disabled)

**Purpose**: Verify existing behavior still works

```bash
# Download a test torrent
cd ~/test-torrents
wget https://webtorrent.io/torrents/big-buck-bunny.torrent
```

**Steps:**
1. In Radarr, go to System → Tasks → RSS Sync → Run Now
2. Or manually add the torrent via qBittorrent Web UI
3. Watch the download in qBittorrent
4. **Verify**: File downloads to `/home/YOUR_USER/downloads/qbittorrent/complete/`
5. After completion, Radarr should import it
6. **Verify**: File is moved to `/home/YOUR_USER/movies/Big Buck Bunny (2008)/`

**Expected behavior:**
- ✅ Downloads to category folder
- ✅ Radarr imports after completion
- ✅ File is moved to movie folder
- ✅ Torrent continues seeding from movie folder

---

### Test 2: Pre-Import Enabled (Main Feature Test)

**Purpose**: Test the new Pre-Import feature

**Steps:**
1. Settings → Download Clients → qBittorrent Test → Edit
2. **✅ Enable "Pre-Import" checkbox**
3. Save
4. Delete the previous test movie (if imported)
5. Re-add the movie to Radarr
6. Add the same test torrent again

**Verify:**
1. Check qBittorrent Web UI
2. **Expected**: Torrent save path shows `/home/YOUR_USER/movies/Big Buck Bunny (2008)/`
3. **NOT**: `/home/YOUR_USER/downloads/qbittorrent/complete/`

**Expected behavior:**
- ✅ Download occurs directly in `/home/YOUR_USER/movies/Big Buck Bunny (2008)/`
- ✅ No file move after completion
- ✅ Radarr imports successfully
- ✅ Torrent seeds from final location

**Check Radarr Logs:**
```bash
# View Radarr logs
tail -f ~/.config/Radarr/logs/radarr.txt | grep -i "pre-import"
```

Expected log line:
```
Debug Pre-import enabled, setting save path to: /home/YOUR_USER/movies/Big Buck Bunny (2008)
```

---

### Test 3: Edge Cases

#### Test 3a: Pre-Import with Magnet Link

**Steps:**
1. Find a magnet link for a test torrent
2. Add to Radarr or directly to qBittorrent with radarr-test category
3. Verify it downloads to movie folder

#### Test 3b: Pre-Import with Missing Movie

**Steps:**
1. Try to download a torrent without adding the movie to Radarr first
2. Expected: Should fail or download to category folder (fallback behavior)

#### Test 3c: Pre-Import with Permission Issues

**Steps:**
1. Make movie folder read-only: `chmod 555 ~/movies/test-movie-2024/`
2. Try to download
3. Expected: qBittorrent should show permission error
4. Fix permissions: `chmod 755 ~/movies/test-movie-2024/`

---

## Verification Checklist

### Unit Tests
- [ ] All 8 Pre-Import unit tests pass
- [ ] No existing tests broken
- [ ] Tests run on ARM64 architecture (Raspberry Pi)

### Feature Tests (Pre-Import Disabled)
- [ ] Downloads to category folder
- [ ] Import works correctly
- [ ] File is moved to movie folder after import
- [ ] Seeding continues after move

### Feature Tests (Pre-Import Enabled)
- [ ] Downloads directly to movie destination folder
- [ ] No file move occurs after completion
- [ ] Import recognizes file in correct location
- [ ] Seeding works from final location
- [ ] Logs show "Pre-import enabled" debug message

### Edge Cases
- [ ] Magnet links work with Pre-Import
- [ ] Torrent files work with Pre-Import
- [ ] Handles missing movie gracefully
- [ ] Handles permission errors gracefully
- [ ] Setting can be toggled on/off without restart

### UI/UX
- [ ] "Pre-Import" checkbox appears in qBittorrent settings
- [ ] Help text is clear and helpful
- [ ] Test button works after enabling Pre-Import
- [ ] Setting persists after save and restart

### Performance
- [ ] No noticeable performance impact
- [ ] Import speed same or faster (no file move)
- [ ] qBittorrent responds normally

---

## Troubleshooting

### Build Fails

**Error**: "dotnet command not found"
```bash
# Ensure .NET is in PATH
export PATH="$PATH:$HOME/.dotnet"
dotnet --version
```

**Error**: "Node version too old"
```bash
# Reinstall Node.js 20
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs
node --version
```

**Error**: Out of memory during build (Raspberry Pi)
```bash
# Add swap space
sudo fallocate -l 4G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
```

### Runtime Issues

**Error**: qBittorrent "Permission denied"
```bash
# Ensure qBittorrent user can write to movie folders
sudo usermod -aG YOUR_USER qbittorrent-nox
# Or run qBittorrent as your user
```

**Error**: Radarr can't connect to qBittorrent
```bash
# Check qBittorrent is running
ps aux | grep qbittorrent

# Check qBittorrent Web UI is accessible
curl http://localhost:8080

# Verify credentials in Radarr settings
```

**Error**: "Pre-Import" checkbox doesn't appear
```bash
# Ensure you built the correct branch
cd ~/radarr-test/Radarr
git branch
git log --oneline -3

# Rebuild if needed
yarn build
dotnet clean src/Radarr.sln -c Debug
dotnet msbuild -restore src/Radarr.sln -p:Configuration=Debug -p:Platform=Posix
```

### Testing Issues

**Issue**: Unit tests fail
```bash
# Run with verbose output
dotnet test src/NzbDrone.Core.Test/Radarr.Core.Test.csproj \
  --filter "FullyQualifiedName~QBittorrent" \
  --logger "console;verbosity=detailed"

# Check if it's ARM-specific
# Try running on x64 machine for comparison
```

**Issue**: File not downloading to movie folder
```bash
# Check Radarr logs
tail -f ~/.config/Radarr/logs/radarr.txt | grep -i "save path"

# Verify Pre-Import is enabled
# Check qBittorrent API response
curl -u admin:PASSWORD http://localhost:8080/api/v2/torrents/info
```

---

## Collecting Test Results

### Generate Test Report

```bash
# Run tests with detailed output
dotnet test src/NzbDrone.Core.Test/Radarr.Core.Test.csproj \
  --filter "FullyQualifiedName~QBittorrent" \
  --logger "trx;LogFileName=test-results.trx" \
  --logger "console;verbosity=detailed" > test-output.txt 2>&1

# Test results are in:
# - test-results.trx (XML format)
# - test-output.txt (console output)
```

### Capture Logs

```bash
# Radarr logs
cp ~/.config/Radarr/logs/radarr.txt ~/radarr-test/radarr-test-logs.txt

# qBittorrent logs
journalctl -u qbittorrent-nox > ~/radarr-test/qbittorrent-logs.txt
```

### Screenshot Checklist

Take screenshots of:
1. [ ] qBittorrent settings showing "Pre-Import" checkbox
2. [ ] qBittorrent Web UI showing download in movie folder path
3. [ ] Radarr Activity showing successful import
4. [ ] File manager showing file in correct movie folder
5. [ ] Radarr logs showing "Pre-import enabled" message

---

## Performance Testing (Optional)

### Compare Download Times

**Without Pre-Import:**
```bash
# Record time for: Download + Move + Import
time wget <test-torrent>
# Then measure import time in Radarr
```

**With Pre-Import:**
```bash
# Record time for: Download + Import (no move)
# Should be faster with large files on different drives
```

### Monitor System Resources

```bash
# Install monitoring tools
sudo apt install -y htop iotop

# Monitor during download
htop  # CPU and RAM
sudo iotop  # Disk I/O

# Compare Pre-Import vs normal behavior
```

---

## Reporting Results

When reporting your test results, include:

1. **Environment**:
   - Hardware: Raspberry Pi 4, 4GB RAM, etc.
   - OS: Ubuntu 22.04 ARM64
   - qBittorrent version: `qbittorrent-nox --version`
   - .NET version: `dotnet --version`

2. **Test Results**:
   - Unit test output (pass/fail)
   - Manual test checklist (completed items)
   - Any errors or issues encountered

3. **Logs**:
   - Relevant Radarr log entries
   - qBittorrent API responses (if applicable)

4. **Screenshots**:
   - Key UI elements
   - Download paths in qBittorrent
   - Successful imports

---

## Quick Start Commands Summary

```bash
# Full test cycle
cd ~/radarr-test
git clone https://github.com/philhar88/Radarr.git
cd Radarr
git checkout claude/radarr-qbitorrent-integration-011CV4uLuxwDNwXo6xv8FPXC
yarn install
dotnet msbuild -restore src/Radarr.sln -p:Configuration=Debug -p:Platform=Posix
yarn build
./test.sh linux unit
cd _output/linux-arm64
./Radarr
```

---

## Next Steps

After successful testing:
1. Document any issues found
2. Capture screenshots for PR
3. Report results to the PR on GitHub
4. Celebrate! 🎉

---

## Additional Resources

- **Radarr Wiki**: https://wiki.servarr.com/radarr
- **qBittorrent API**: https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API
- **Test Torrents**: https://webtorrent.io/free-torrents
- **.NET Documentation**: https://docs.microsoft.com/en-us/dotnet/

---

**Last Updated**: 2025-01-13
**Feature Branch**: `claude/radarr-qbitorrent-integration-011CV4uLuxwDNwXo6xv8FPXC`
**Tested On**: Ubuntu 22.04 ARM64 (Raspberry Pi 4)
