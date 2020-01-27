﻿using System;
using System.IO;
using System.Reflection;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IAppFolderInfo
    {
        string AppDataFolder { get; }
        string TempFolder { get; }
        string StartUpFolder { get; }
    }

    public class AppFolderInfo : IAppFolderInfo
    {
        private readonly Environment.SpecialFolder _dataSpecialFolder = Environment.SpecialFolder.CommonApplicationData;

        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(AppFolderInfo));

        public AppFolderInfo(IStartupContext startupContext)
        {
            if (OsInfo.IsNotWindows)
            {
                _dataSpecialFolder = Environment.SpecialFolder.ApplicationData;
            }

            if (startupContext.Args.ContainsKey(StartupContext.APPDATA))
            {
                AppDataFolder = startupContext.Args[StartupContext.APPDATA];
                Logger.Info("Data directory is being overridden to [{0}]", AppDataFolder);
            }
            else
            {
                AppDataFolder = Path.Combine(Environment.GetFolderPath(_dataSpecialFolder, Environment.SpecialFolderOption.DoNotVerify), "Radarr");
            }

            StartUpFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            TempFolder = Path.GetTempPath();
        }

        public string AppDataFolder { get; private set; }

        public string StartUpFolder { get; private set; }

        public string TempFolder { get; private set; }
    }
}
