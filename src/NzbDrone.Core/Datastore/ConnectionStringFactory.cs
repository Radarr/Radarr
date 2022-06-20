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
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileOptions;

        // Catch legacy config, to be removed soon
        private readonly PostgresOptions _postgresOptions;

        public ConnectionStringFactory(IAppFolderInfo appFolderInfo,
            IOptionsMonitor<ConfigFileOptions> configFileOptions)
        {
            _configFileOptions = configFileOptions;
            _postgresOptions = PostgresOptions.GetOptions();

            var isPostgres = _configFileOptions.CurrentValue.PostgresHost.IsNotNullOrWhiteSpace() || _postgresOptions.Host.IsNotNullOrWhiteSpace();
            var mainDb = _configFileOptions.CurrentValue.PostgresMainDb ?? _postgresOptions.MainDb;
            var logDb = _configFileOptions.CurrentValue.PostgresLogDb ?? _postgresOptions.LogDb;

            MainDbConnectionString = isPostgres ? GetPostgresConnectionString(mainDb) :
                GetConnectionString(appFolderInfo.GetDatabase());

            LogDbConnectionString = isPostgres ? GetPostgresConnectionString(logDb) :
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
            connectionBuilder.Host = _configFileOptions.CurrentValue.PostgresHost ?? _postgresOptions.Host;
            connectionBuilder.Username = _configFileOptions.CurrentValue.PostgresUser ?? _postgresOptions.User;
            connectionBuilder.Password = _configFileOptions.CurrentValue.PostgresPassword ?? _postgresOptions.Password;
            connectionBuilder.Port = _configFileOptions.CurrentValue.PostgresPort > 0 ? _configFileOptions.CurrentValue.PostgresPort : _postgresOptions.Port;
            connectionBuilder.Enlist = false;

            return connectionBuilder.ConnectionString;
        }
    }
}
