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
        private const string FILE_LOG_LAYOUT = @"${date:format=yyyy-MM-dd HH\:mm\:ss.f}|${level}|${logger}|${message}${onexception:inner=${newline}${newline}[v${assembly-version}] ${exception:format=ToString}${newline}${exception:format=Data}${newline}}";

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

            RegisterAuthLogger();

            LogManager.ReconfigExistingLoggers();
        }

        private static void RegisterSentry(bool updateClient)
        {
            string dsn;

            if (updateClient)
            {
                dsn = "https://fdefcf5a6a4a4a0db914b5712b5480e0@sentry.servarr.com/11";
            }
            else
            {
                dsn = RuntimeInfo.IsProduction
                    ? "https://39b572f7f3f04899b2c3254c7ac126d0@sentry.servarr.com/9"
                    : "https://998b4673d4c849ccb5277b5966ed5bc2@sentry.servarr.com/10";
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
            target.Layout = "[${level}] [${threadid}] ${logger}: ${message} ${onexception:inner=${newline}${newline}[v${assembly-version}] ${exception:format=ToString}${newline}${exception:format=Data}${newline}}";

            var loggingRule = new LoggingRule("*", LogLevel.Trace, target);
            LogManager.Configuration.AddTarget("debugger", target);
            LogManager.Configuration.LoggingRules.Add(loggingRule);
        }

        private static void RegisterConsole()
        {
            var level = LogLevel.Trace;

            var coloredConsoleTarget = new ColoredConsoleTarget();

            coloredConsoleTarget.Name = "consoleLogger";
            coloredConsoleTarget.Layout = "[${level}] ${logger}: ${message} ${onexception:inner=${newline}${newline}[v${assembly-version}] ${exception:format=ToString}${newline}${exception:format=Data}${newline}}";

            var loggingRule = new LoggingRule("*", level, coloredConsoleTarget);

            LogManager.Configuration.AddTarget("console", coloredConsoleTarget);
            LogManager.Configuration.LoggingRules.Add(loggingRule);
        }

        private static void RegisterAppFile(IAppFolderInfo appFolderInfo)
        {
            RegisterAppFile(appFolderInfo, "appFileInfo", "radarr.txt", 50, LogLevel.Info);
            RegisterAppFile(appFolderInfo, "appFileDebug", "radarr.debug.txt", 500, LogLevel.Off);
            RegisterAppFile(appFolderInfo, "appFileTrace", "radarr.trace.txt", 500, LogLevel.Off);
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

        private static void RegisterAuthLogger()
        {
            var consoleTarget = LogManager.Configuration.FindTargetByName("console");
            var fileTarget = LogManager.Configuration.FindTargetByName("appFileInfo");

            var target = consoleTarget ?? fileTarget ?? new NullTarget();

            // Send Auth to Console and info app file, but not the log database
            var rule = new LoggingRule("Auth", LogLevel.Info, target) { Final = true };
            if (consoleTarget != null && fileTarget != null)
            {
                rule.Targets.Add(fileTarget);
            }

            LogManager.Configuration.LoggingRules.Insert(0, rule);
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
