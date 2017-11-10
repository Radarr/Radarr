﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using NLog;

namespace NzbDrone.Common.EnvironmentInfo
{
    public class RuntimeInfo : IRuntimeInfo
    {
        private readonly Logger _logger;

        public RuntimeInfo(IServiceProvider serviceProvider, Logger logger)
        {
            _logger = logger;

            IsWindowsService = !IsUserInteractive &&
                               OsInfo.IsWindows &&
                               serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME) &&
                               serviceProvider.GetStatus(ServiceProvider.NZBDRONE_SERVICE_NAME) == ServiceControllerStatus.StartPending;

            //Guarded to avoid issues when running in a non-managed process
            var entry = Assembly.GetEntryAssembly();

            if (entry != null)
            {
                ExecutingApplication = entry.Location;
            }
        }

        static RuntimeInfo()
        {
            IsProduction = InternalIsProduction();
        }

        public static bool IsUserInteractive => Environment.UserInteractive;

        bool IRuntimeInfo.IsUserInteractive => IsUserInteractive;

        public bool IsAdmin
        {
            get
            {
                try
                {
                    var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Error checking if the current user is an administrator.");
                    return false;
                }
            }
        }

        public bool IsWindowsService { get; private set; }

        public bool IsExiting { get; set; }
        public bool RestartPending { get; set; }
        public string ExecutingApplication { get; }

        public static bool IsProduction { get; }

        private static bool InternalIsProduction()
        {
            if (BuildInfo.IsDebug || Debugger.IsAttached) return false;

            //Official builds will never have such a high revision
            if (BuildInfo.Version.Revision > 10000) return false;

            try
            {
                var lowerProcessName = Process.GetCurrentProcess().ProcessName.ToLower();

                if (lowerProcessName.Contains("vshost")) return false;
                if (lowerProcessName.Contains("nunit")) return false;
                if (lowerProcessName.Contains("jetbrain")) return false;
                if (lowerProcessName.Contains("resharper")) return false;
            }
            catch
            {

            }

            try
            {
                var currentAssemblyLocation = typeof(RuntimeInfo).Assembly.Location;
                if (currentAssemblyLocation.ToLower().Contains("_output")) return false;
            }
            catch
            {

            }

            var lowerCurrentDir = Directory.GetCurrentDirectory().ToLower();
            if (lowerCurrentDir.Contains("teamcity")) return false;
            if (lowerCurrentDir.Contains("_output")) return false;

            return true;
        }
    }
}
