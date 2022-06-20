using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Config;
using NLog.Targets.Syslog;
using NLog.Targets.Syslog.Settings;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Instrumentation.Sentry;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Instrumentation
{
    public class ReconfigureLogging : IHandleAsync<ConfigFileSavedEvent>
    {
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileOptions;

        public ReconfigureLogging(IOptionsMonitor<ConfigFileOptions> configFileOptions)
        {
            _configFileOptions = configFileOptions;
        }

        public void Reconfigure()
        {
            var minimumLogLevel = LogLevel.FromString(_configFileOptions.CurrentValue.LogLevel);
            LogLevel minimumConsoleLogLevel;

            if (_configFileOptions.CurrentValue.ConsoleLogLevel.IsNotNullOrWhiteSpace())
            {
                minimumConsoleLogLevel = LogLevel.FromString(_configFileOptions.CurrentValue.ConsoleLogLevel);
            }
            else if (minimumLogLevel > LogLevel.Info)
            {
                minimumConsoleLogLevel = minimumLogLevel;
            }
            else
            {
                minimumConsoleLogLevel = LogLevel.Info;
            }

            if (_configFileOptions.CurrentValue.SyslogServer.IsNotNullOrWhiteSpace())
            {
                var syslogLevel = LogLevel.FromString(_configFileOptions.CurrentValue.SyslogLevel);
                SetSyslogParameters(_configFileOptions.CurrentValue.SyslogServer, _configFileOptions.CurrentValue.SyslogPort, syslogLevel);
            }

            var rules = LogManager.Configuration.LoggingRules;

            //Console
            SetMinimumLogLevel(rules, "consoleLogger", minimumConsoleLogLevel);

            //Log Files
            SetMinimumLogLevel(rules, "appFileInfo", minimumLogLevel <= LogLevel.Info ? LogLevel.Info : LogLevel.Off);
            SetMinimumLogLevel(rules, "appFileDebug", minimumLogLevel <= LogLevel.Debug ? LogLevel.Debug : LogLevel.Off);
            SetMinimumLogLevel(rules, "appFileTrace", minimumLogLevel <= LogLevel.Trace ? LogLevel.Trace : LogLevel.Off);
            SetLogRotation();

            //Log Sql
            SqlBuilderExtensions.LogSql = _configFileOptions.CurrentValue.LogSql;

            //Sentry
            ReconfigureSentry();

            LogManager.ReconfigExistingLoggers();
        }

        private void SetMinimumLogLevel(IList<LoggingRule> rules, string target, LogLevel minimumLogLevel)
        {
            foreach (var rule in rules.Where(v => v.Targets.Any(t => t.Name == target)))
            {
                SetMinimumLogLevel(rule, minimumLogLevel);
            }
        }

        private void SetMinimumLogLevel(LoggingRule rule, LogLevel minimumLogLevel)
        {
            foreach (var logLevel in GetLogLevels())
            {
                if (logLevel < minimumLogLevel)
                {
                    rule.DisableLoggingForLevel(logLevel);
                }
                else
                {
                    rule.EnableLoggingForLevel(logLevel);
                }
            }
        }

        private void SetLogRotation()
        {
            foreach (var target in LogManager.Configuration.AllTargets.OfType<NzbDroneFileTarget>())
            {
                target.MaxArchiveFiles = _configFileOptions.CurrentValue.LogRotate;
            }
        }

        private void ReconfigureSentry()
        {
            var sentryTarget = LogManager.Configuration.AllTargets.OfType<SentryTarget>().FirstOrDefault();
            if (sentryTarget != null)
            {
                sentryTarget.SentryEnabled = (RuntimeInfo.IsProduction && _configFileOptions.CurrentValue.AnalyticsEnabled) || RuntimeInfo.IsDevelopment;
                sentryTarget.FilterEvents = _configFileOptions.CurrentValue.FilterSentryEvents;
            }
        }

        private void SetSyslogParameters(string syslogServer, int syslogPort, LogLevel minimumLogLevel)
        {
            var syslogTarget = new SyslogTarget();

            syslogTarget.Name = "syslogTarget";
            syslogTarget.MessageSend.Protocol = ProtocolType.Udp;
            syslogTarget.MessageSend.Udp.Port = syslogPort;
            syslogTarget.MessageSend.Udp.Server = syslogServer;
            syslogTarget.MessageSend.Udp.ReconnectInterval = 500;
            syslogTarget.MessageCreation.Rfc = RfcNumber.Rfc5424;
            syslogTarget.MessageCreation.Rfc5424.AppName = _configFileOptions.CurrentValue.InstanceName;

            var loggingRule = new LoggingRule("*", minimumLogLevel, syslogTarget);

            LogManager.Configuration.AddTarget("syslogTarget", syslogTarget);
            LogManager.Configuration.LoggingRules.Add(loggingRule);
        }

        private List<LogLevel> GetLogLevels()
        {
            return new List<LogLevel>
                       {
                           LogLevel.Trace,
                           LogLevel.Debug,
                           LogLevel.Info,
                           LogLevel.Warn,
                           LogLevel.Error,
                           LogLevel.Fatal
                       };
        }

        public void HandleAsync(ConfigFileSavedEvent message)
        {
            Reconfigure();
        }
    }
}
