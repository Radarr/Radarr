using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(DiskProvider));

        private readonly IProcMountProvider _procMountProvider;
        private readonly ISymbLinkResolver _symLinkResolver;
	private readonly Logger _logger;

        // Mono supports sending -1 for a uint to indicate that the owner or group should not be set
        // `unchecked((uint)-1)` and `uint.MaxValue` are the same thing.
        private const uint UNCHANGED_ID = uint.MaxValue;

        public DiskProvider(IProcMountProvider procMountProvider, ISymbLinkResolver symLinkResolver, Logger logger)
        {
            _procMountProvider = procMountProvider;
            _symLinkResolver = symLinkResolver;
		_logger = logger;
        }

        public override IMount GetMount(string path)
        {
		if (path == null) return null;

		try
		{
			string[] dirs;
			int lastIndex;
			GetPathComponents(path, out dirs, out lastIndex);

			var realPath = new StringBuilder();
			if (dirs.Length > 0)
			{
				var dir = UnixPath.IsPathRooted(path) ? "/" : "";
				dir += dirs[0];
				realPath.Append(GetRealPath(dir));
			}
			for (var i = 1; i < lastIndex; ++i)
			{
				realPath.Append("/").Append(dirs[i]);
				var realSubPath = GetRealPath(realPath.ToString());
				realPath.Remove(0, realPath.Length);
				realPath.Append(realSubPath);
			}
			path = realPath.ToString();
		}
		catch (Exception ex)
		{
			_logger.Debug(ex, string.Format("Failed to check for symlinks in the path {0}", path));
		}

            return base.GetMount(path);
        }

        public override long? GetAvailableSpace(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            try
            {
                var mount = GetMount(path);

                if (mount == null)
                {
                    Logger.Debug("Unable to get free space for '{0}', unable to find suitable drive", path);
                    return null;
                }

                return mount.AvailableFreeSpace;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(ex, "Couldn't get free space for " + path);
            }

            return null;
        }

        public override void InheritFolderPermissions(string filename)
        {
            Ensure.That(filename, () => filename).IsValidPath();

            try
            {
                var fs = File.GetAccessControl(filename);
                fs.SetAccessRuleProtection(false, false);
                File.SetAccessControl(filename, fs);
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

        public override List<IMount> GetMounts()
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

        public override long? GetTotalSize(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            try
            {
                var mount = GetMount(path);

                if (mount == null) return null;

                return mount.TotalSize;
            }
            catch (InvalidOperationException e)
            {
                Logger.Error(e, "Couldn't get total space for " + path);
            }

            return null;
        }

        public override bool TryCreateHardLink(string source, string destination)
        {
            try
            {
                UnixFileSystemInfo.GetFileSystemEntry(source).CreateLink(destination);
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

	private static void GetPathComponents(string path, out string[] components, out int lastIndex)
	{
		var dirs = path.Split(UnixPath.DirectorySeparatorChar);
		var target = 0;
		for (var i = 0; i < dirs.Length; ++i)
		{
			if (dirs[i] == "." || dirs[i] == string.Empty)
			{
				continue;
			}

			if (dirs[i] == "..")
			{
				if (target != 0)
				{
					target--;
				}
				else
				{
					target++;
				}
			}
			else
			{
				dirs[target++] = dirs[i];
			}
		}
		components = dirs;
		lastIndex = target;
	}

	public string GetRealPath(string path)
	{
		do
		{
			var link = UnixPath.TryReadLink(path);

			if (link == null)
			{
				var errno = Stdlib.GetLastError();
				if (errno != Errno.EINVAL)
				{
					_logger.Trace("Checking path {0} for symlink returned error {1}, assuming it's not a symlink.", path, errno);
				}

				return path;
			}

			if (UnixPath.IsPathRooted(link))
			{
				path = link;
			}
			else
			{
				path = UnixPath.GetDirectoryName(path) + UnixPath.DirectorySeparatorChar + link;
				path = UnixPath.GetCanonicalPath(path);
			}
		} while (true);
	}
    }
}
