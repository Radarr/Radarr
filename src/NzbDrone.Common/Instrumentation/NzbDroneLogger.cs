using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public static void UnRegisterRemoteLoggers()
        {
            var sentryRules = LogManager.Configuration.LoggingRules.Where(r => r.Targets.Any(t => t.Name == "sentryTarget"));

            foreach (var rules in sentryRules)
            {
                rules.Targets.Clear();
            }

            LogManager.ReconfigExistingLoggers();
        }

        private static void RegisterSentry(bool updateClient)
        {
            string dsn;

            if (updateClient)
            {
                dsn = "https://89744ec1d2a14a36856f601bdf78e69f@sentry.io/1523535";

            }
            else
            {
                dsn = RuntimeInfo.IsProduction
                    ? "https://fd45b801b1af445f919dbca8ca0ead89@sentry.io/1485284"
                    : "https://e4e7bfd3cc0e45a4814613a096c79cc3@sentry.io/1523529";
            }

            var target = new SentryTarget(dsn)
            {
                Name = "sentryTarget",
                Layout = "${message}"
            };

            var loggingRule = new LoggingRule("*", updateClient ? LogLevel.Trace : LogLevel.Warn, target);
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

        private const string FILE_LOG_LAYOUT = @"${date:format=yy-M-d HH\:mm\:ss.f}|${level}|${logger}|${message}${onexception:inner=${newline}${newline}[v${assembly-version}] ${exception:format=ToString}${newline}}";

        private static void RegisterAppFile(IAppFolderInfo appFolderInfo)
        {
            RegisterAppFile(appFolderInfo, "appFileInfo", "radarr.txt", 5, LogLevel.Info);
            RegisterAppFile(appFolderInfo, "appFileDebug", "radarr.debug.txt", 50, LogLevel.Off);
            RegisterAppFile(appFolderInfo, "appFileTrace", "radarr.trace.txt", 50, LogLevel.Off);
        }

        private static void RegisterAppFile(IAppFolderInfo appFolderInfo, string name, string fileName, int maxArchiveFiles, LogLevel minLogLevel)
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
