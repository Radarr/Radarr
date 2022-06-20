using System;
using Microsoft.Extensions.Configuration;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Configuration
{
    public class ConfigFileOptions
    {
        [Persist]
        public string BindAddress { get; set; } = "*";
        [Persist]
        public int Port { get; set; } = 7878;
        [Persist]
        public int SslPort { get; set; } = 9898;
        [Persist]
        public bool EnableSsl { get; set; }
        [Persist]
        public bool LaunchBrowser { get; set; } = true;
        public AuthenticationType AuthenticationMethod { get; set; }
        public bool AnalyticsEnabled { get; set; } = true;
        [Persist]
        public string Branch { get; set; } = "master";
        [Persist]
        public string LogLevel { get; set; } = "info";
        public string ConsoleLogLevel { get; set; } = string.Empty;
        public bool LogSql { get; set; }
        public int LogRotate { get; set; } = 50;
        public bool FilterSentryEvents { get; set; } = true;
        [Persist]
        public string ApiKey { get; set; } = GenerateApiKey();
        [Persist]
        public string SslCertPath { get; set; }
        [Persist]
        public string SslCertPassword { get; set; }
        [Persist]
        public string UrlBase { get; set; } = string.Empty;
        [Persist]
        public string InstanceName { get; set; } = BuildInfo.AppName;
        public bool UpdateAutomatically { get; set; }
        public UpdateMechanism UpdateMechanism { get; set; } = UpdateMechanism.BuiltIn;
        public string UpdateScriptPath { get; set; } = string.Empty;
        public string SyslogServer { get; set; } = string.Empty;
        public int SyslogPort { get; set; } = 514;
        public string SyslogLevel { get; set; } = "info";
        public string PostgresHost { get; set; }
        public int PostgresPort { get; set; }
        public string PostgresUser { get; set; }
        public string PostgresPassword { get; set; }
        public string PostgresMainDb { get; set; } = BuildInfo.AppName.ToLower() + "-main";
        public string PostgresLogDb { get; set; } = BuildInfo.AppName.ToLower() + "-log";

        private static string GenerateApiKey()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        public static ConfigFileOptions GetOptions()
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables($"{BuildInfo.AppName}:")
                .Build();

            var options = new ConfigFileOptions();
            config.Bind(options);

            return options;
        }
    }
}
