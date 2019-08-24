using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace NzbDrone.Common.EnvironmentInfo
{
    public class OsInfo : IOsInfo
    {
        public static Os Os { get; }

        public static bool IsNotWindows => !IsWindows;
        public static bool IsLinux => Os == Os.Linux;
        public static bool IsOsx => Os == Os.Osx;
        public static bool IsWindows => Os == Os.Windows;

        // this needs to not be static so we can mock it
        public bool IsDocker { get; }
        
        public string Version { get; }
        public string Name { get; }
        public string FullName { get; }

        static OsInfo()
        {
            var platform = Environment.OSVersion.Platform;

            switch (platform)
            {
                case PlatformID.Win32NT:
                    {
                        Os = Os.Windows;
                        break;
                    }
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    {
                        // Sometimes Mac OS reports itself as Unix
                        if (Directory.Exists("/System/Library/CoreServices/") &&
                            (File.Exists("/System/Library/CoreServices/SystemVersion.plist") ||
                            File.Exists("/System/Library/CoreServices/ServerVersion.plist"))
                            )
                        {
                            Os = Os.Osx;
                        }
                        else
                        {
                            Os = Os.Linux;
                        }
                        break;
                    }
            }
        }

        public OsInfo(IEnumerable<IOsVersionAdapter> versionAdapters, Logger logger)
        {
            OsVersionModel osInfo = null;

            foreach (var osVersionAdapter in versionAdapters.Where(c => c.Enabled))
            {
                try
                {
                    osInfo = osVersionAdapter.Read();
                }
                catch (Exception e)
                {
                    logger.Error(e, "Couldn't get OS Version info");
                }

                if (osInfo != null)
                {
                    break;
                }
            }

            if (osInfo != null)
            {
                Name = osInfo.Name;
                Version = osInfo.Version;
                FullName = osInfo.FullName;
            }
            else
            {
                Name = Os.ToString();
                FullName = Name;
            }

            if (IsLinux && File.Exists("/proc/1/cgroup") && File.ReadAllText("/proc/1/cgroup").Contains("/docker/"))
            {
                IsDocker = true;
            }
        }
    }

    public interface IOsInfo
    {
        string Version { get; }
        string Name { get; }
        string FullName { get; }

        bool IsDocker { get; }
    }

    public enum Os
    {
        Windows,
        Linux,
        Osx
    }
}
