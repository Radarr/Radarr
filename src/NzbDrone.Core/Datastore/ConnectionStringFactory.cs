using System;
using System.Data.SQLite;
using Microsoft.Extensions.Options;
using Npgsql;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Datastore
{
    public interface IConnectionStringFactory
    {
        string MainDbConnectionString { get; }
        string LogDbConnectionString { get; }
        string GetDatabasePath(string connectionString);
    }

    public class ConnectionStringFactory : IConnectionStringFactory
    {
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileProvider;

        public ConnectionStringFactory(IAppFolderInfo appFolderInfo, IOptionsMonitor<ConfigFileOptions> configFileProvider)
        {
            _configFileProvider = configFileProvider;

            MainDbConnectionString = _configFileProvider.CurrentValue.PostgresHost.IsNotNullOrWhiteSpace() ? GetPostgresConnectionString(_configFileProvider.CurrentValue.PostgresMainDb) :
                GetConnectionString(appFolderInfo.GetDatabase());

            LogDbConnectionString = _configFileProvider.CurrentValue.PostgresHost.IsNotNullOrWhiteSpace() ? GetPostgresConnectionString(_configFileProvider.CurrentValue.PostgresLogDb) :
                GetConnectionString(appFolderInfo.GetLogDatabase());
        }

        public string MainDbConnectionString { get; private set; }
        public string LogDbConnectionString { get; private set; }

        public string GetDatabasePath(string connectionString)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder(connectionString);

            return connectionBuilder.DataSource;
        }

        private static string GetConnectionString(string dbPath)
        {
            var connectionBuilder = new SQLiteConnectionStringBuilder();

            connectionBuilder.DataSource = dbPath;
            connectionBuilder.CacheSize = (int)-20000;
            connectionBuilder.DateTimeKind = DateTimeKind.Utc;
            connectionBuilder.JournalMode = OsInfo.IsOsx ? SQLiteJournalModeEnum.Truncate : SQLiteJournalModeEnum.Wal;
            connectionBuilder.Pooling = true;
            connectionBuilder.Version = 3;

            if (OsInfo.IsOsx)
            {
                connectionBuilder.Add("Full FSync", true);
            }

            return connectionBuilder.ConnectionString;
        }

        private string GetPostgresConnectionString(string dbName)
        {
            var connectionBuilder = new NpgsqlConnectionStringBuilder();

            connectionBuilder.Database = dbName;
            connectionBuilder.Host = _configFileProvider.CurrentValue.PostgresHost;
            connectionBuilder.Username = _configFileProvider.CurrentValue.PostgresUser;
            connectionBuilder.Password = _configFileProvider.CurrentValue.PostgresPassword;
            connectionBuilder.Port = _configFileProvider.CurrentValue.PostgresPort;
            connectionBuilder.Enlist = false;

            return connectionBuilder.ConnectionString;
        }
    }
}
