using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NLog;

namespace NzbDrone.Common.EnvironmentInfo
{
    public class OsInfo : IOsInfo
    {
        public static Os Os { get; }

        public static bool IsNotWindows => !IsWindows;
        public static bool IsLinux => Os == Os.Linux || Os == Os.LinuxMusl || Os == Os.Bsd;
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
                        Os = GetPosixFlavour();
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

        private static Os GetPosixFlavour()
        {
            var output = RunAndCapture("uname", "-s");

            if (output.StartsWith("Darwin"))
            {
                return Os.Osx;
            }
            else if (output.Contains("BSD"))
            {
                return Os.Bsd;
            }
            else
            {
                return IsMusl() ? Os.LinuxMusl : Os.Linux;
            }
        }

        private static bool IsMusl()
        {
            var output = RunAndCapture("ldd", "/bin/ls");
            return output.Contains("libc.musl");
        }

        private static string RunAndCapture(string filename, string args)
        {
            Process p = new Process();
            p.StartInfo.FileName = filename;
            p.StartInfo.Arguments = args;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;

            p.Start();

            // To avoid deadlocks, always read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(1000);

            return output;
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
        Osx,
        LinuxMusl,
        Bsd
    }
}
