using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Unix;
using Mono.Unix.Native;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Mono.Disk
{
    public class DiskProvider : DiskProviderBase
    {
        // Mono supports sending -1 for a uint to indicate that the owner or group should not be set
        // `unchecked((uint)-1)` and `uint.MaxValue` are the same thing.
        private const uint UNCHANGED_ID = uint.MaxValue;

        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(DiskProvider));

        private readonly IProcMountProvider _procMountProvider;
        private readonly ISymbolicLinkResolver _symLinkResolver;

        public DiskProvider(IProcMountProvider procMountProvider, ISymbolicLinkResolver symLinkResolver)
        {
            _procMountProvider = procMountProvider;
            _symLinkResolver = symLinkResolver;
        }

        public override IMount GetMount(string path)
        {
            path = _symLinkResolver.GetCompleteRealPath(path);

            return base.GetMount(path);
        }

        public override long? GetAvailableSpace(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            var mount = GetMount(path);

            if (mount == null)
            {
                Logger.Debug("Unable to get free space for '{0}', unable to find suitable drive", path);
                return null;
            }

            return mount.AvailableFreeSpace;
        }

        public override void InheritFolderPermissions(string filename)
        {
            Ensure.That(filename, () => filename).IsValidPath();

            try
            {
                var file = new FileInfo(filename);
                var fs = file.GetAccessControl();
                fs.SetAccessRuleProtection(false, false);
                file.SetAccessControl(fs);
            }
            catch (NotImplementedException)
            {
            }
            catch (PlatformNotSupportedException)
            {
            }
        }

        public override void SetPermissions(string path, string mask, string user, string group)
        {
            SetPermissions(path, mask);
            SetOwner(path, user, group);
        }

        protected override List<IMount> GetAllMounts()
        {
            return _procMountProvider.GetMounts()
                                     .Concat(GetDriveInfoMounts()
                                                 .Select(d => new DriveInfoMount(d, FindDriveType.Find(d.DriveFormat)))
                                                 .Where(d => d.DriveType == DriveType.Fixed ||
                                                             d.DriveType == DriveType.Network ||
                                                             d.DriveType == DriveType.Removable))
                                     .DistinctBy(v => v.RootDirectory)
                                     .ToList();
        }

        protected override bool IsSpecialMount(IMount mount)
        {
            var root = mount.RootDirectory;

            if (root.StartsWith("/var/lib/"))
            {
                // Could be /var/lib/docker when docker uses zfs. Very unlikely that a useful mount is located in /var/lib.
                return true;
            }

            if (root.StartsWith("/snap/"))
            {
                // Mount point for snap packages
                return true;
            }

            return false;
        }

        public override long? GetTotalSize(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            var mount = GetMount(path);

            return mount?.TotalSize;
        }

        protected override void CopyFileInternal(string source, string destination, bool overwrite)
        {
            var sourceInfo = UnixFileSystemInfo.GetFileSystemEntry(source);

            if (sourceInfo.IsSymbolicLink)
            {
                var isSameDir = UnixPath.GetDirectoryName(source) == UnixPath.GetDirectoryName(destination);
                var symlinkInfo = (UnixSymbolicLinkInfo)sourceInfo;
                var symlinkPath = symlinkInfo.ContentsPath;

                var newFile = new UnixSymbolicLinkInfo(destination);

                if (FileExists(destination) && overwrite)
                {
                    DeleteFile(destination);
                }

                if (isSameDir)
                {
                    // We're in the same dir, so we can preserve relative symlinks.
                    newFile.CreateSymbolicLinkTo(symlinkInfo.ContentsPath);
                }
                else
                {
                    var fullPath = UnixPath.Combine(UnixPath.GetDirectoryName(source), symlinkPath);
                    newFile.CreateSymbolicLinkTo(fullPath);
                }
            }
            else
            {
                base.CopyFileInternal(source, destination, overwrite);
            }
        }

        protected override void MoveFileInternal(string source, string destination, bool overwrite)
        {
            var sourceInfo = UnixFileSystemInfo.GetFileSystemEntry(source);

            if (sourceInfo.IsSymbolicLink)
            {
                var isSameDir = UnixPath.GetDirectoryName(source) == UnixPath.GetDirectoryName(destination);
                var symlinkInfo = (UnixSymbolicLinkInfo)sourceInfo;
                var symlinkPath = symlinkInfo.ContentsPath;

                var newFile = new UnixSymbolicLinkInfo(destination);

                if (isSameDir)
                {
                    // We're in the same dir, so we can preserve relative symlinks.
                    newFile.CreateSymbolicLinkTo(symlinkInfo.ContentsPath);
                }
                else
                {
                    var fullPath = UnixPath.Combine(UnixPath.GetDirectoryName(source), symlinkPath);
                    newFile.CreateSymbolicLinkTo(fullPath);
                }

                try
                {
                    // Finally remove the original symlink.
                    symlinkInfo.Delete();
                }
                catch
                {
                    // Removing symlink failed, so rollback the new link and throw.
                    newFile.Delete();
                    throw;
                }
            }
            else
            {
                base.MoveFileInternal(source, destination, overwrite);
            }
        }

        public override bool TryCreateHardLink(string source, string destination)
        {
            try
            {
                var fileInfo = UnixFileSystemInfo.GetFileSystemEntry(source);

                if (fileInfo.IsSymbolicLink)
                {
                    return false;
                }

                fileInfo.CreateLink(destination);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, string.Format("Hardlink '{0}' to '{1}' failed.", source, destination));
                return false;
            }
        }

        private void SetPermissions(string path, string mask)
        {
            Logger.Debug("Setting permissions: {0} on {1}", mask, path);

            var filePermissions = NativeConvert.FromOctalPermissionString(mask);

            if (Syscall.chmod(path, filePermissions) < 0)
            {
                var error = Stdlib.GetLastError();

                throw new LinuxPermissionsException("Error setting file permissions: " + error);
            }
        }

        private void SetOwner(string path, string user, string group)
        {
            if (string.IsNullOrWhiteSpace(user) && string.IsNullOrWhiteSpace(group))
            {
                Logger.Debug("User and Group for chown not configured, skipping chown.");
                return;
            }

            var userId = GetUserId(user);
            var groupId = GetGroupId(group);

            if (Syscall.chown(path, userId, groupId) < 0)
            {
                var error = Stdlib.GetLastError();

                throw new LinuxPermissionsException("Error setting file owner and/or group: " + error);
            }
        }

        private uint GetUserId(string user)
        {
            if (user.IsNullOrWhiteSpace())
            {
                return UNCHANGED_ID;
            }

            uint userId;

            if (uint.TryParse(user, out userId))
            {
                return userId;
            }

            var u = Syscall.getpwnam(user);

            if (u == null)
            {
                throw new LinuxPermissionsException("Unknown user: {0}", user);
            }

            return u.pw_uid;
        }

        private uint GetGroupId(string group)
        {
            if (group.IsNullOrWhiteSpace())
            {
                return UNCHANGED_ID;
            }

            uint groupId;

            if (uint.TryParse(group, out groupId))
            {
                return groupId;
            }

            var g = Syscall.getgrnam(group);

            if (g == null)
            {
                throw new LinuxPermissionsException("Unknown group: {0}", group);
            }

            return g.gr_gid;
        }
    }
}
