﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Disk
{
    public interface IDiskTransferService
    {
        TransferMode TransferFolder(string sourcePath, string targetPath, TransferMode mode, bool verified = true);
        TransferMode TransferFile(string sourcePath, string targetPath, TransferMode mode, bool overwrite = false, bool verified = true);
        int MirrorFolder(string sourcePath, string targetPath);
    }

    public enum DiskTransferVerificationMode
    {
        None,
        VerifyOnly,
        TryTransactional,
        Transactional
    }

    public class DiskTransferService : IDiskTransferService
    {
        private const int RetryCount = 2;

        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public DiskTransferVerificationMode VerificationMode { get; set; }

        public DiskTransferService(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;

            // TODO: Atm we haven't seen partial transfers on windows so we disable verified transfer.
            // (If enabled in the future, be sure to check specifically for ReFS, which doesn't support hardlinks.)
            VerificationMode = OsInfo.IsWindows ? DiskTransferVerificationMode.VerifyOnly : DiskTransferVerificationMode.TryTransactional;
        }

        public TransferMode TransferFolder(string sourcePath, string targetPath, TransferMode mode, bool verified = true)
        {
            var verificationMode = verified ? VerificationMode : DiskTransferVerificationMode.VerifyOnly;

            return TransferFolder(sourcePath, targetPath, mode, verificationMode);
        }

        public TransferMode TransferFolder(string sourcePath, string targetPath, TransferMode mode, DiskTransferVerificationMode verificationMode)
        {
            Ensure.That(sourcePath, () => sourcePath).IsValidPath();
            Ensure.That(targetPath, () => targetPath).IsValidPath();

            if (mode == TransferMode.Move && !_diskProvider.FolderExists(targetPath))
            {
                if (verificationMode == DiskTransferVerificationMode.TryTransactional || verificationMode == DiskTransferVerificationMode.VerifyOnly)
                {
                    var sourceMount = _diskProvider.GetMount(sourcePath);
                    var targetMount = _diskProvider.GetMount(targetPath);

                    // If we're on the same mount, do a simple folder move.
                    if (sourceMount != null && targetMount != null && sourceMount.RootDirectory == targetMount.RootDirectory)
                    {
                        _logger.Debug("Move Directory [{0}] > [{1}]", sourcePath, targetPath);
                        _diskProvider.MoveFolder(sourcePath, targetPath);
                        return mode;
                    }
                }
            }

            if (!_diskProvider.FolderExists(targetPath))
            {
                _diskProvider.CreateFolder(targetPath);
            }

            var result = mode;

            foreach (var subDir in _diskProvider.GetDirectoryInfos(sourcePath))
            {
                if (ShouldIgnore(subDir))
                {
                    continue;
                }

                result &= TransferFolder(subDir.FullName, Path.Combine(targetPath, subDir.Name), mode, verificationMode);
            }

            foreach (var sourceFile in _diskProvider.GetFileInfos(sourcePath))
            {
                if (ShouldIgnore(sourceFile))
                {
                    continue;
                }

                var destFile = Path.Combine(targetPath, sourceFile.Name);

                result &= TransferFile(sourceFile.FullName, destFile, mode, true, verificationMode);
            }

            if (mode.HasFlag(TransferMode.Move))
            {
                _diskProvider.DeleteFolder(sourcePath, true);
            }

            return result;
        }

        public int MirrorFolder(string sourcePath, string targetPath)
        {
            var filesCopied = 0;

            Ensure.That(sourcePath, () => sourcePath).IsValidPath();
            Ensure.That(targetPath, () => targetPath).IsValidPath();

            _logger.Debug("Mirror [{0}] > [{1}]", sourcePath, targetPath);

            if (!_diskProvider.FolderExists(targetPath))
            {
                _diskProvider.CreateFolder(targetPath);
            }

            var sourceFolders = _diskProvider.GetDirectoryInfos(sourcePath);
            var targetFolders = _diskProvider.GetDirectoryInfos(targetPath);

            foreach (var subDir in targetFolders.Where(v => !sourceFolders.Any(d => d.Name == v.Name)))
            {
                if (ShouldIgnore(subDir))
                {
                    continue;
                }

                _diskProvider.DeleteFolder(subDir.FullName, true);
            }

            foreach (var subDir in sourceFolders)
            {
                if (ShouldIgnore(subDir))
                {
                    continue;
                }

                filesCopied += MirrorFolder(subDir.FullName, Path.Combine(targetPath, subDir.Name));
            }

            var sourceFiles = _diskProvider.GetFileInfos(sourcePath);
            var targetFiles = _diskProvider.GetFileInfos(targetPath);

            foreach (var targetFile in targetFiles.Where(v => !sourceFiles.Any(d => d.Name == v.Name)))
            {
                if (ShouldIgnore(targetFile))
                {
                    continue;
                }

                _diskProvider.DeleteFile(targetFile.FullName);
            }

            foreach (var sourceFile in sourceFiles)
            {
                if (ShouldIgnore(sourceFile))
                {
                    continue;
                }

                var targetFile = Path.Combine(targetPath, sourceFile.Name);

                if (CompareFiles(sourceFile.FullName, targetFile))
                {
                    continue;
                }

                TransferFile(sourceFile.FullName, targetFile, TransferMode.Copy, true, true);
                filesCopied++;
            }

            return filesCopied;
        }

        private bool CompareFiles(string sourceFile, string targetFile)
        {
            if (!_diskProvider.FileExists(sourceFile) || !_diskProvider.FileExists(targetFile))
            {
                return false;
            }

            if (_diskProvider.GetFileSize(sourceFile) != _diskProvider.GetFileSize(targetFile))
            {
                return false;
            }

            var sourceBuffer = new byte[64 * 1024];
            var targetBuffer = new byte[64 * 1024];
            using (var sourceStream = _diskProvider.OpenReadStream(sourceFile))
            using (var targetStream = _diskProvider.OpenReadStream(targetFile))
            {
                while (true)
                {
                    var sourceLength = sourceStream.Read(sourceBuffer, 0, sourceBuffer.Length);
                    var targetLength = targetStream.Read(targetBuffer, 0, targetBuffer.Length);

                    if (sourceLength != targetLength)
                    {
                        return false;
                    }

                    if (sourceLength == 0)
                    {
                        return true;
                    }

                    for (var i = 0; i < sourceLength; i++)
                    {
                        if (sourceBuffer[i] != targetBuffer[i])
                        {
                            return false;
                        }
                    }
                }
            }
        }

        public TransferMode TransferFile(string sourcePath, string targetPath, TransferMode mode, bool overwrite = false, bool verified = true)
        {
            var verificationMode = verified ? VerificationMode : DiskTransferVerificationMode.None;

            return TransferFile(sourcePath, targetPath, mode, overwrite, verificationMode);
        }

        public TransferMode TransferFile(string sourcePath, string targetPath, TransferMode mode, bool overwrite, DiskTransferVerificationMode verificationMode)
        {
            Ensure.That(sourcePath, () => sourcePath).IsValidPath();
            Ensure.That(targetPath, () => targetPath).IsValidPath();

            _logger.Debug("{0} [{1}] > [{2}]", mode, sourcePath, targetPath);

            var originalSize = _diskProvider.GetFileSize(sourcePath);

            if (sourcePath == targetPath)
            {
                throw new IOException(string.Format("Source and destination can't be the same {0}", sourcePath));
            }

            if (sourcePath.PathEquals(targetPath, StringComparison.InvariantCultureIgnoreCase))
            {
                if (mode.HasFlag(TransferMode.HardLink) || mode.HasFlag(TransferMode.Copy))
                {
                    throw new IOException(string.Format("Source and destination can't be the same {0}", sourcePath));
                }

                if (mode.HasFlag(TransferMode.Move))
                {
                    var tempPath = sourcePath + ".backup~";

                    _diskProvider.MoveFile(sourcePath, tempPath, true);
                    try
                    {
                        ClearTargetPath(sourcePath, targetPath, overwrite);

                        _diskProvider.MoveFile(tempPath, targetPath);

                        return TransferMode.Move;
                    }
                    catch
                    {
                        RollbackMove(sourcePath, tempPath);
                        throw;
                    }
                }

                return TransferMode.None;
            }

            if (sourcePath.GetParentPath() == targetPath.GetParentPath())
            {
                if (mode.HasFlag(TransferMode.Move))
                {
                    TryMoveFileVerified(sourcePath, targetPath, originalSize);
                    return TransferMode.Move;
                }
            }

            if (sourcePath.IsParentPath(targetPath))
            {
                throw new IOException(string.Format("Destination cannot be a child of the source [{0}] => [{1}]", sourcePath, targetPath));
            }

            ClearTargetPath(sourcePath, targetPath, overwrite);

            if (mode.HasFlag(TransferMode.HardLink))
            {
                var createdHardlink = _diskProvider.TryCreateHardLink(sourcePath, targetPath);
                if (createdHardlink)
                {
                    return TransferMode.HardLink;
                }

                if (!mode.HasFlag(TransferMode.Copy))
                {
                    throw new IOException("Hardlinking from '" + sourcePath + "' to '" + targetPath + "' failed.");
                }
            }

            // Adjust the transfer mode depending on the filesystems
            if (verificationMode == DiskTransferVerificationMode.TryTransactional)
            {
                var sourceMount = _diskProvider.GetMount(sourcePath);
                var targetMount = _diskProvider.GetMount(targetPath);

                var isSameMount = sourceMount != null && targetMount != null && sourceMount.RootDirectory == targetMount.RootDirectory;

                var sourceDriveFormat = sourceMount?.DriveFormat ?? string.Empty;
                var targetDriveFormat = targetMount?.DriveFormat ?? string.Empty;

                if (isSameMount)
                {
                    // No transaction needed for operations on same mount, force VerifyOnly
                    verificationMode = DiskTransferVerificationMode.VerifyOnly;
                }
                else if (sourceDriveFormat.Contains("mergerfs") || sourceDriveFormat.Contains("rclone") ||
                         targetDriveFormat.Contains("mergerfs") || targetDriveFormat.Contains("rclone"))
                {
                    // Cloud storage filesystems don't need any Transactional stuff and it hurts performance, force VerifyOnly
                    verificationMode = DiskTransferVerificationMode.VerifyOnly;
                }
                else if ((sourceDriveFormat == "cifs" || targetDriveFormat == "cifs") && OsInfo.IsNotWindows)
                {
                    // Force Transactional on a cifs mount due to the likeliness of move failures on certain scenario's on mono
                    verificationMode = DiskTransferVerificationMode.Transactional;
                }
            }

            if (mode.HasFlag(TransferMode.Copy))
            {
                if (verificationMode == DiskTransferVerificationMode.Transactional || verificationMode == DiskTransferVerificationMode.TryTransactional)
                {
                    if (TryCopyFileTransactional(sourcePath, targetPath, originalSize))
                    {
                        return TransferMode.Copy;
                    }

                    throw new IOException(string.Format("Failed to completely transfer [{0}] to [{1}], aborting.", sourcePath, targetPath));
                }
                else if (verificationMode == DiskTransferVerificationMode.VerifyOnly)
                {
                    TryCopyFileVerified(sourcePath, targetPath, originalSize);
                    return TransferMode.Copy;
                }
                else
                {
                    _diskProvider.CopyFile(sourcePath, targetPath);
                    return TransferMode.Copy;
                }
            }

            if (mode.HasFlag(TransferMode.Move))
            {
                if (verificationMode == DiskTransferVerificationMode.Transactional || verificationMode == DiskTransferVerificationMode.TryTransactional)
                {
                    if (TryMoveFileTransactional(sourcePath, targetPath, originalSize, verificationMode))
                    {
                        return TransferMode.Move;
                    }

                    throw new IOException(string.Format("Failed to completely transfer [{0}] to [{1}], aborting.", sourcePath, targetPath));
                }
                else if (verificationMode == DiskTransferVerificationMode.VerifyOnly)
                {
                    TryMoveFileVerified(sourcePath, targetPath, originalSize);
                    return TransferMode.Move;
                }
                else
                {
                    _diskProvider.MoveFile(sourcePath, targetPath);
                    return TransferMode.Move;
                }
            }

            return TransferMode.None;
        }

        private void ClearTargetPath(string sourcePath, string targetPath, bool overwrite)
        {
            if (_diskProvider.FileExists(targetPath))
            {
                if (overwrite)
                {
                    _diskProvider.DeleteFile(targetPath);
                }
                else
                {
                    throw new DestinationAlreadyExistsException($"Destination {targetPath} already exists.");
                }
            }
        }

        private void RollbackPartialMove(string sourcePath, string targetPath)
        {
            try
            {
                _logger.Debug("Rolling back incomplete file move [{0}] to [{1}].", sourcePath, targetPath);

                WaitForIO();

                if (_diskProvider.FileExists(sourcePath))
                {
                    _diskProvider.DeleteFile(targetPath);
                }
                else
                {
                    _logger.Error("Failed to properly rollback the file move [{0}] to [{1}], incomplete file may be left in target path.", sourcePath, targetPath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to properly rollback the file move [{0}] to [{1}], incomplete file may be left in target path.", sourcePath, targetPath);
            }
        }

        private void RollbackMove(string sourcePath, string targetPath)
        {
            try
            {
                _logger.Debug("Rolling back file move [{0}] to [{1}].", sourcePath, targetPath);

                WaitForIO();

                _diskProvider.MoveFile(targetPath, sourcePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to properly rollback the file move [{0}] to [{1}], file may be left in target path.", sourcePath, targetPath);
            }
        }

        private void RollbackCopy(string sourcePath, string targetPath)
        {
            try
            {
                _logger.Debug("Rolling back file copy [{0}] to [{1}].", sourcePath, targetPath);

                WaitForIO();

                if (_diskProvider.FileExists(targetPath))
                {
                    _diskProvider.DeleteFile(targetPath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to properly rollback the file copy [{0}] to [{1}], file may be left in target path.", sourcePath, targetPath);
            }
        }

        private void WaitForIO()
        {
            // This delay is intended to give the IO stack a bit of time to recover, this is especially required if remote NAS devices are involved.
            Thread.Sleep(3000);
        }

        private bool TryCopyFileTransactional(string sourcePath, string targetPath, long originalSize)
        {
            var tempTargetPath = targetPath + ".partial~";

            if (_diskProvider.FileExists(tempTargetPath))
            {
                _logger.Trace("Removing old partial.");
                _diskProvider.DeleteFile(tempTargetPath);
            }

            try
            {
                for (var i = 0; i <= RetryCount; i++)
                {
                    _diskProvider.CopyFile(sourcePath, tempTargetPath);

                    if (_diskProvider.FileExists(tempTargetPath))
                    {
                        var targetSize = _diskProvider.GetFileSize(tempTargetPath);
                        if (targetSize == originalSize)
                        {
                            _diskProvider.MoveFile(tempTargetPath, targetPath);
                            return true;
                        }
                    }

                    WaitForIO();

                    _diskProvider.DeleteFile(tempTargetPath);

                    if (i == RetryCount)
                    {
                        _logger.Error("Failed to completely transfer [{0}] to [{1}], aborting.", sourcePath, targetPath);
                    }
                    else
                    {
                        _logger.Warn("Failed to completely transfer [{0}] to [{1}], retrying [{2}/{3}].", sourcePath, targetPath, i + 1, RetryCount);
                    }
                }
            }
            catch
            {
                WaitForIO();

                if (_diskProvider.FileExists(tempTargetPath))
                {
                    _diskProvider.DeleteFile(tempTargetPath);
                }

                throw;
            }

            return false;
        }

        private bool TryMoveFileTransactional(string sourcePath, string targetPath, long originalSize, DiskTransferVerificationMode verificationMode)
        {
            var backupPath = sourcePath + ".backup~";
            var tempTargetPath = targetPath + ".partial~";

            if (_diskProvider.FileExists(backupPath))
            {
                _logger.Trace("Removing old backup.");
                _diskProvider.DeleteFile(backupPath);
            }

            if (_diskProvider.FileExists(tempTargetPath))
            {
                _logger.Trace("Removing old partial.");
                _diskProvider.DeleteFile(tempTargetPath);
            }

            try
            {
                _logger.Trace("Attempting to move hardlinked backup.");
                if (_diskProvider.TryCreateHardLink(sourcePath, backupPath))
                {
                    _diskProvider.MoveFile(backupPath, tempTargetPath);

                    if (_diskProvider.FileExists(tempTargetPath))
                    {
                        var targetSize = _diskProvider.GetFileSize(tempTargetPath);

                        if (targetSize == originalSize)
                        {
                            _diskProvider.MoveFile(tempTargetPath, targetPath);
                            if (_diskProvider.FileExists(tempTargetPath))
                            {
                                throw new IOException(string.Format("Temporary file '{0}' still exists, aborting.", tempTargetPath));
                            }

                            _logger.Trace("Hardlink move succeeded, deleting source.");
                            _diskProvider.DeleteFile(sourcePath);
                            return true;
                        }
                    }

                    Thread.Sleep(5000);

                    _diskProvider.DeleteFile(tempTargetPath);
                }
            }
            finally
            {
                if (_diskProvider.FileExists(backupPath))
                {
                    _diskProvider.DeleteFile(backupPath);
                }
            }

            if (verificationMode == DiskTransferVerificationMode.Transactional)
            {
                _logger.Trace("Hardlink move failed, reverting to copy.");
                if (TryCopyFileTransactional(sourcePath, targetPath, originalSize))
                {
                    _logger.Trace("Copy succeeded, deleting source.");
                    _diskProvider.DeleteFile(sourcePath);
                    return true;
                }
            }
            else
            {
                _logger.Trace("Hardlink move failed, reverting to move.");
                TryMoveFileVerified(sourcePath, targetPath, originalSize);
                return true;
            }

            _logger.Trace("Move failed.");
            return false;
        }

        private void TryCopyFileVerified(string sourcePath, string targetPath, long originalSize)
        {
            try
            {
                _diskProvider.CopyFile(sourcePath, targetPath);

                var targetSize = _diskProvider.GetFileSize(targetPath);
                if (targetSize != originalSize)
                {
                    throw new IOException(string.Format("File copy incomplete. [{0}] was {1} bytes long instead of {2} bytes.", targetPath, targetSize, originalSize));
                }
            }
            catch
            {
                RollbackCopy(sourcePath, targetPath);
                throw;
            }
        }

        private void TryMoveFileVerified(string sourcePath, string targetPath, long originalSize)
        {
            try
            {
                _diskProvider.MoveFile(sourcePath, targetPath);

                var targetSize = _diskProvider.GetFileSize(targetPath);
                if (targetSize != originalSize)
                {
                    throw new IOException(string.Format("File move incomplete, data loss may have occurred. [{0}] was {1} bytes long instead of the expected {2}.", targetPath, targetSize, originalSize));
                }
            }
            catch
            {
                RollbackPartialMove(sourcePath, targetPath);
                throw;
            }
        }

        private bool ShouldIgnore(DirectoryInfo folder)
        {
            if (folder.Name.StartsWith(".nfs"))
            {
                _logger.Trace("Ignoring folder {0}", folder.FullName);
                return true;
            }

            return false;
        }

        private bool ShouldIgnore(FileInfo file)
        {
            if (file.Name.StartsWith(".nfs") || file.Name == "debug.log" || file.Name.EndsWith(".socket"))
            {
                _logger.Trace("Ignoring file {0}", file.FullName);
                return true;
            }

            return false;
        }
    }
}
