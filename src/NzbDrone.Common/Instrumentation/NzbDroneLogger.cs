using System;
using System.Diagnostics;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Sentry;

namespace NzbDrone.Common.Instrumentation
{
    public static class NzbDroneLogger
    {
        private static bool _isConfigured;

        static NzbDroneLogger()
        {
            LogManager.Configuration = new LoggingConfiguration();
        }

        const string FILE_LOG_LAYOUT = @"${date:format=yyyy-M-d HH\:mm\:ss.f}|${level}|${logger}|${message}${onexception:inner=${newline}${newline}[v${assembly-version}] ${exception:format=ToString}${newline}}";

        public static void Register(IStartupContext startupContext, bool updateApp, bool inConsole)
        {
            if (_isConfigured)
            {
                throw new InvalidOperationException("Loggers have already been registered.");
            }

            _isConfigured = true;

            GlobalExceptionHandlers.Register();

            var appFolderInfo = new AppFolderInfo(startupContext);

            if (Debugger.IsAttached)
            {
                RegisterDebugger();
            }

            RegisterSentry(updateApp);

            if (updateApp)
            {
                RegisterUpdateFile(appFolderInfo);
            }
            else
            {
                if (inConsole && (OsInfo.IsNotWindows || RuntimeInfo.IsUserInteractive))
                {
                    RegisterConsole();
                }

                RegisterAppFile(appFolderInfo);
            }

            LogManager.ReconfigExistingLoggers();
        }

        private static void RegisterSentry(bool updateClient)
        {
            string dsn;

            if (updateClient)
            {
                dsn = "https://40e47616afd84525bf49306123cb0926@sentry.radarr.video/10";
            }
            else
            {
                dsn = RuntimeInfo.IsProduction
                    ? "https://ef145e92efdd4155a0771c11c099695e@sentry.radarr.video/2"
                    : "https://dee5b3fe26844368ac4458faa7d00a1f@sentry.radarr.video/9";
            }

            var target = new SentryTarget(dsn)
            {
                Name = "sentryTarget",
                Layout = "${message}"
            };

            var loggingRule = new LoggingRule("*", updateClient ? LogLevel.Trace : LogLevel.Debug, target);
            LogManager.Configuration.AddTarget("sentryTarget", target);
            LogManager.Configuration.LoggingRules.Add(loggingRule);

            // Events logged to Sentry go only to Sentry.
            var loggingRuleSentry = new LoggingRule("Sentry", LogLevel.Debug, target) { Final = true };
            LogManager.Configuration.LoggingRules.Insert(0, loggingRuleSentry);
        }

        private static void RegisterDebugger()
        {
            DebuggerTarget target = new DebuggerTarget();
            target.Name = "debuggerLogger";
            target.Layout = "[${level}] [${threadid}] ${logger}: ${message} ${onexception:inner=${newline}${newline}[v${assembly-version}] ${exception:format=ToString}${newline}}";

            var loggingRule = new LoggingRule("*", LogLevel.Trace, target);
            LogManager.Configuration.AddTarget("debugger", target);
            LogManager.Configuration.LoggingRules.Add(loggingRule);
        }


        private static void RegisterConsole()
        {
            var level = LogLevel.Trace;

            var coloredConsoleTarget = new ColoredConsoleTarget();

            coloredConsoleTarget.Name = "consoleLogger";
            coloredConsoleTarget.Layout = "[${level}] ${logger}: ${message} ${onexception:inner=${newline}${newline}[v${assembly-version}] ${exception:format=ToString}${newline}}";

            var loggingRule = new LoggingRule("*", level, coloredConsoleTarget);

            LogManager.Configuration.AddTarget("console", coloredConsoleTarget);
            LogManager.Configuration.LoggingRules.Add(loggingRule);
        }

        private static void RegisterAppFile(IAppFolderInfo appFolderInfo)
        {
            RegisterAppFile(appFolderInfo, "appFileInfo", "radarr.txt", 5, LogLevel.Info);
            RegisterAppFile(appFolderInfo, "appFileDebug", "radarr.debug.txt", 50, LogLevel.Off);
            RegisterAppFile(appFolderInfo, "appFileTrace", "radarr.trace.txt", 50, LogLevel.Off);
        }

        private static LoggingRule RegisterAppFile(IAppFolderInfo appFolderInfo, string name, string fileName, int maxArchiveFiles, LogLevel minLogLevel)
        {
            var fileTarget = new NzbDroneFileTarget();

            fileTarget.Name = name;
            fileTarget.FileName = Path.Combine(appFolderInfo.GetLogFolder(), fileName);
            fileTarget.AutoFlush = true;
            fileTarget.KeepFileOpen = false;
            fileTarget.ConcurrentWrites = false;
            fileTarget.ConcurrentWriteAttemptDelay = 50;
            fileTarget.ConcurrentWriteAttempts = 10;
            fileTarget.ArchiveAboveSize = 1024000;
            fileTarget.MaxArchiveFiles = maxArchiveFiles;
            fileTarget.EnableFileDelete = true;
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.Rolling;
            fileTarget.Layout = FILE_LOG_LAYOUT;

            var loggingRule = new LoggingRule("*", minLogLevel, fileTarget);

            LogManager.Configuration.AddTarget(name, fileTarget);
            LogManager.Configuration.LoggingRules.Add(loggingRule);

            return loggingRule;
        }

        private static void RegisterUpdateFile(IAppFolderInfo appFolderInfo)
        {
            var fileTarget = new FileTarget();

            fileTarget.Name = "updateFileLogger";
            fileTarget.FileName = Path.Combine(appFolderInfo.GetUpdateLogFolder(), DateTime.Now.ToString("yyyy.MM.dd-HH.mm") + ".txt");
            fileTarget.AutoFlush = true;
            fileTarget.KeepFileOpen = false;
            fileTarget.ConcurrentWrites = false;
            fileTarget.ConcurrentWriteAttemptDelay = 50;
            fileTarget.ConcurrentWriteAttempts = 100;
            fileTarget.Layout = FILE_LOG_LAYOUT;

            var loggingRule = new LoggingRule("*", LogLevel.Trace, fileTarget);

            LogManager.Configuration.AddTarget("updateFile", fileTarget);
            LogManager.Configuration.LoggingRules.Add(loggingRule);
        }

        public static Logger GetLogger(Type obj)
        {
            return LogManager.GetLogger(obj.Name.Replace("NzbDrone.", ""));
        }

        public static Logger GetLogger(object obj)
        {
            return GetLogger(obj.GetType());
        }

    }
}
