using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common.Disk
{
    public abstract class DiskProviderBase : IDiskProvider
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(DiskProviderBase));
        protected readonly IFileSystem _fileSystem;

        public DiskProviderBase(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public static StringComparison PathStringComparison
        {
            get
            {
                if (OsInfo.IsWindows)
                {
                    return StringComparison.OrdinalIgnoreCase;
                }

                return StringComparison.Ordinal;
            }
        }

        public abstract long? GetAvailableSpace(string path);
        public abstract void InheritFolderPermissions(string filename);
        public abstract void SetPermissions(string path, string mask, string user, string group);
        public abstract long? GetTotalSize(string path);

        public DateTime FolderGetCreationTime(string path)
        {
            CheckFolderExists(path);

            return _fileSystem.DirectoryInfo.FromDirectoryName(path).CreationTimeUtc;
        }

        public DateTime FolderGetLastWrite(string path)
        {
            CheckFolderExists(path);

            var dirFiles = GetFiles(path, SearchOption.AllDirectories).ToList();

            if (!dirFiles.Any())
            {
                return _fileSystem.DirectoryInfo.FromDirectoryName(path).LastWriteTimeUtc;
            }

            return dirFiles.Select(f => _fileSystem.FileInfo.FromFileName(f)).Max(c => c.LastWriteTimeUtc);
        }

        public DateTime FileGetLastWrite(string path)
        {
            CheckFileExists(path);

            return _fileSystem.FileInfo.FromFileName(path).LastWriteTimeUtc;
        }

        private void CheckFolderExists(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            if (!FolderExists(path))
            {
                throw new DirectoryNotFoundException("Directory doesn't exist. " + path);
            }
        }

        private void CheckFileExists(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            if (!FileExists(path))
            {
                throw new FileNotFoundException("File doesn't exist: " + path);
            }
        }

        public void EnsureFolder(string path)
        {
            if (!FolderExists(path))
            {
                CreateFolder(path);
            }
        }

        public bool FolderExists(string path)
        {
            Ensure.That(path, () => path).IsValidPath();
            return _fileSystem.Directory.Exists(path);
        }

        public bool FileExists(string path)
        {
            Ensure.That(path, () => path).IsValidPath();
            return FileExists(path, PathStringComparison);
        }

        public bool FileExists(string path, StringComparison stringComparison)
        {
            Ensure.That(path, () => path).IsValidPath();

            switch (stringComparison)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.InvariantCulture:
                case StringComparison.Ordinal:
                    {
                        return _fileSystem.File.Exists(path) && path == path.GetActualCasing();
                    }
                default:
                    {
                        return _fileSystem.File.Exists(path);
                    }
            }
        }

        public bool FolderWritable(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            try
            {
                var testPath = Path.Combine(path, "lidarr_write_test.txt");
                var testContent = $"This file was created to verify if '{path}' is writable. It should've been automatically deleted. Feel free to delete it.";
                _fileSystem.File.WriteAllText(testPath, testContent);
                _fileSystem.File.Delete(testPath);
                return true;
            }
            catch (Exception e)
            {
                Logger.Trace("Directory '{0}' isn't writable. {1}", path, e.Message);
                return false;
            }
        }

        public string[] GetDirectories(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            return _fileSystem.Directory.GetDirectories(path);
        }

        public string[] GetDirectories(string path, SearchOption searchOption)
        {
            Ensure.That(path, () => path).IsValidPath();

            return _fileSystem.Directory.GetDirectories(path, "*", searchOption);
        }

        public string[] GetFiles(string path, SearchOption searchOption)
        {
            Ensure.That(path, () => path).IsValidPath();

            return _fileSystem.Directory.GetFiles(path, "*.*", searchOption);
        }

        public long GetFolderSize(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            return GetFiles(path, SearchOption.AllDirectories).Sum(e => _fileSystem.FileInfo.FromFileName(e).Length);
        }

        public long GetFileSize(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            if (!FileExists(path))
            {
                throw new FileNotFoundException("File doesn't exist: " + path);
            }

            var fi = _fileSystem.FileInfo.FromFileName(path);
            return fi.Length;
        }

        public void CreateFolder(string path)
        {
            Ensure.That(path, () => path).IsValidPath();
            _fileSystem.Directory.CreateDirectory(path);
        }

        public void DeleteFile(string path)
        {
            Ensure.That(path, () => path).IsValidPath();
            Logger.Trace("Deleting file: {0}", path);

            RemoveReadOnly(path);

            _fileSystem.File.Delete(path);
        }

        public void CopyFile(string source, string destination, bool overwrite = false)
        {
            Ensure.That(source, () => source).IsValidPath();
            Ensure.That(destination, () => destination).IsValidPath();

            if (source.PathEquals(destination))
            {
                throw new IOException(string.Format("Source and destination can't be the same {0}", source));
            }

            CopyFileInternal(source, destination, overwrite);
        }

        protected virtual void CopyFileInternal(string source, string destination, bool overwrite = false)
        {
            _fileSystem.File.Copy(source, destination, overwrite);
        }

        public void MoveFile(string source, string destination, bool overwrite = false)
        {
            Ensure.That(source, () => source).IsValidPath();
            Ensure.That(destination, () => destination).IsValidPath();

            if (source.PathEquals(destination))
            {
                throw new IOException(string.Format("Source and destination can't be the same {0}", source));
            }

            if (FileExists(destination) && overwrite)
            {
                DeleteFile(destination);
            }

            RemoveReadOnly(source);
            MoveFileInternal(source, destination);
        }

        public void MoveFolder(string source, string destination)
        {
            Ensure.That(source, () => source).IsValidPath();
            Ensure.That(destination, () => destination).IsValidPath();

            Directory.Move(source, destination);
        }

        protected virtual void MoveFileInternal(string source, string destination)
        {
            _fileSystem.File.Move(source, destination);
        }

        public abstract bool TryCreateHardLink(string source, string destination);

        public void DeleteFolder(string path, bool recursive)
        {
            Ensure.That(path, () => path).IsValidPath();

            var files = _fileSystem.Directory.GetFiles(path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            Array.ForEach(files, RemoveReadOnly);

            _fileSystem.Directory.Delete(path, recursive);
        }

        public string ReadAllText(string filePath)
        {
            Ensure.That(filePath, () => filePath).IsValidPath();

            return _fileSystem.File.ReadAllText(filePath);
        }

        public void WriteAllText(string filename, string contents)
        {
            Ensure.That(filename, () => filename).IsValidPath();
            RemoveReadOnly(filename);
            _fileSystem.File.WriteAllText(filename, contents);
        }

        public void FolderSetLastWriteTime(string path, DateTime dateTime)
        {
            Ensure.That(path, () => path).IsValidPath();

            _fileSystem.Directory.SetLastWriteTimeUtc(path, dateTime);
        }

        public void FileSetLastWriteTime(string path, DateTime dateTime)
        {
            Ensure.That(path, () => path).IsValidPath();

            _fileSystem.File.SetLastWriteTime(path, dateTime);
        }

        public bool IsFileLocked(string file)
        {
            try
            {
                using (_fileSystem.File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }

        public string GetPathRoot(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            return Path.GetPathRoot(path);
        }

        public string GetParentFolder(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            var parent = _fileSystem.Directory.GetParent(path.TrimEnd(Path.DirectorySeparatorChar));

            if (parent == null)
            {
                return null;
            }

            return parent.FullName;
        }

        public void SetPermissions(string filename, WellKnownSidType accountSid, FileSystemRights rights, AccessControlType controlType)
        {
            try
            {
                var sid = new SecurityIdentifier(accountSid, null);

                var directoryInfo = _fileSystem.DirectoryInfo.FromDirectoryName(filename);
                var directorySecurity = directoryInfo.GetAccessControl(AccessControlSections.Access);

                var rules = directorySecurity.GetAccessRules(true, false, typeof(SecurityIdentifier));

                if (rules.OfType<FileSystemAccessRule>().Any(acl => acl.AccessControlType == controlType && (acl.FileSystemRights & rights) == rights && acl.IdentityReference.Equals(sid)))
                {
                    return;
                }

                var accessRule = new FileSystemAccessRule(sid, rights,
                                                          InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                          PropagationFlags.InheritOnly, controlType);

                bool modified;
                directorySecurity.ModifyAccessRule(AccessControlModification.Add, accessRule, out modified);

                if (modified)
                {
                    directoryInfo.SetAccessControl(directorySecurity);
                }
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Couldn't set permission for {0}. account:{1} rights:{2} accessControlType:{3}", filename, accountSid, rights, controlType);
                throw;
            }

        }

        private static void RemoveReadOnly(string path)
        {
            if (File.Exists(path))
            {
                var attributes = File.GetAttributes(path);

                if (attributes.HasFlag(FileAttributes.ReadOnly))
                {
                    var newAttributes = attributes & ~(FileAttributes.ReadOnly);
                    File.SetAttributes(path, newAttributes);
                }
            }
        }

        public FileAttributes GetFileAttributes(string path)
        {
            return _fileSystem.File.GetAttributes(path);
        }

        public void EmptyFolder(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            foreach (var file in GetFiles(path, SearchOption.TopDirectoryOnly))
            {
                DeleteFile(file);
            }

            foreach (var directory in GetDirectories(path))
            {
                DeleteFolder(directory, true);
            }
        }

        public string[] GetFixedDrives()
        {
            return GetMounts().Where(x => x.DriveType == DriveType.Fixed).Select(x => x.RootDirectory).ToArray();
        }

        public string GetVolumeLabel(string path)
        {
            var driveInfo = GetMounts().SingleOrDefault(d => d.RootDirectory.PathEquals(path));

            if (driveInfo == null)
            {
                return null;
            }

            return driveInfo.VolumeLabel;
        }

        public FileStream OpenReadStream(string path)
        {
            if (!FileExists(path))
            {
                throw new FileNotFoundException("Unable to find file: " + path, path);
            }

            return (FileStream) _fileSystem.FileStream.Create(path, FileMode.Open, FileAccess.Read);
        }

        public FileStream OpenWriteStream(string path)
        {
            return (FileStream) _fileSystem.FileStream.Create(path, FileMode.Create);
        }

        public List<IMount> GetMounts()
        {
            return GetAllMounts().Where(d => !IsSpecialMount(d)).ToList();
        }

        protected virtual List<IMount> GetAllMounts()
        {
            return GetDriveInfoMounts().Where(d => d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Network || d.DriveType == DriveType.Removable)
                                       .Select(d => new DriveInfoMount(d))
                                       .Cast<IMount>()
                                       .ToList();
        }

        protected virtual bool IsSpecialMount(IMount mount)
        {
            return false;
        }

        public virtual IMount GetMount(string path)
        {
            try
            {
                var mounts = GetAllMounts();

                return mounts.Where(drive => drive.RootDirectory.PathEquals(path) ||
                                             drive.RootDirectory.IsParentPath(path))
                          .OrderByDescending(drive => drive.RootDirectory.Length)
                          .FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, string.Format("Failed to get mount for path {0}", path));
                return null;
            }
        }

        protected List<IDriveInfo> GetDriveInfoMounts()
        {
            return _fileSystem.DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .ToList();
        }

        public List<IDirectoryInfo> GetDirectoryInfos(string path)
        {
            Ensure.That(path, () => path).IsValidPath();

            var di = _fileSystem.DirectoryInfo.FromDirectoryName(path);

            return di.GetDirectories().ToList();
        }

        public IDirectoryInfo GetDirectoryInfo(string path)
        {
            Ensure.That(path, () => path).IsValidPath();
            return _fileSystem.DirectoryInfo.FromDirectoryName(path);
        }

        public List<IFileInfo> GetFileInfos(string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            Ensure.That(path, () => path).IsValidPath();

            var di = _fileSystem.DirectoryInfo.FromDirectoryName(path);

            return di.GetFiles("*", searchOption).ToList();
        }

        public IFileInfo GetFileInfo(string path)
        {
            Ensure.That(path, () => path).IsValidPath();
            return _fileSystem.FileInfo.FromFileName(path);
        }

        public void RemoveEmptySubfolders(string path)
        {
            var subfolders = GetDirectories(path, SearchOption.AllDirectories);
            var files = GetFiles(path, SearchOption.AllDirectories);

            // By sorting by length descending we ensure we always delete children before parents
            foreach (var subfolder in subfolders.OrderByDescending(x => x.Length))
            {
                if (files.None(f => subfolder.IsParentPath(f)))
                {
                    DeleteFolder(subfolder, false);
                }
            }
        }

        public void SaveStream(Stream stream, string path)
        {
            using (var fileStream = OpenWriteStream(path))
            {
                stream.CopyTo(fileStream);
            }
        }
    }
}
