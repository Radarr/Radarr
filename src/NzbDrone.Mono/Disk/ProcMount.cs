using System;
using System.IO;
using Mono.Unix;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Mono.Disk
{
    public class ProcMount : IMount
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(ProcMount));
        private readonly UnixDriveInfo _unixDriveInfo;

        public ProcMount(DriveType driveType, string name, string mount, string type, MountOptions mountOptions)
        {
            DriveType = driveType;
            Name = name;
            RootDirectory = mount;
            DriveFormat = type;
            MountOptions = mountOptions;

            _unixDriveInfo = new UnixDriveInfo(mount);
        }

        public long AvailableFreeSpace => _unixDriveInfo.AvailableFreeSpace;

        public string DriveFormat { get; private set; }

        public DriveType DriveType { get; private set; }

        public bool IsReady => _unixDriveInfo.IsReady;

        public MountOptions MountOptions { get; private set; }

        public string Name { get; private set; }

        public string RootDirectory { get; private set; }

        public long TotalFreeSpace
        {
            get
            {
                try
                {
                    return _unixDriveInfo.TotalFreeSpace;
                }
                catch (OverflowException ex)
                {
                    Logger.Warn(ex, "Failed to get total free space");
                    return long.MaxValue;
                }
            }
        }

        public long TotalSize
        {
            get
            {
                try
                {
                    return _unixDriveInfo.TotalSize;
                }
                catch (OverflowException ex)
                {
                    Logger.Warn(ex, "Failed to get total size");
                    return long.MaxValue;
                }
            }
        }

        public string VolumeLabel => _unixDriveInfo.VolumeLabel;

        public string VolumeName
        {
            get
            {
                if (VolumeLabel.IsNullOrWhiteSpace() || VolumeLabel.StartsWith("UUID=") || Name == VolumeLabel)
                {
                    return Name;
                }

                return string.Format("{0} ({1})", Name, VolumeLabel);
            }
        }
    }
}
