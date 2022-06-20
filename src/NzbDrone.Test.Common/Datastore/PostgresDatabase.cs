using System;
using Npgsql;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Test.Common.Datastore
{
    public static class PostgresDatabase
    {
        public static ConfigFileOptions GetTestOptions()
        {
            var options = ConfigFileOptions.GetOptions();

            var uid = TestBase.GetUID();
            options.PostgresMainDb = uid + "_main";
            options.PostgresLogDb = uid + "_log";

            return options;
        }

        public static void Create(ConfigFileOptions options, MigrationType migrationType)
        {
            var db = GetDatabaseName(options, migrationType);
            var connectionString = GetConnectionString(options);
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE \"{db}\" WITH OWNER = {options.PostgresUser} ENCODING = 'UTF8' CONNECTION LIMIT = -1;";
            cmd.ExecuteNonQuery();
        }

        public static void Drop(ConfigFileOptions options, MigrationType migrationType)
        {
            var db = GetDatabaseName(options, migrationType);
            var connectionString = GetConnectionString(options);
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"DROP DATABASE \"{db}\" WITH (FORCE);";
            cmd.ExecuteNonQuery();
        }

        private static string GetConnectionString(ConfigFileOptions options)
        {
            var builder = new NpgsqlConnectionStringBuilder()
            {
                Host = options.PostgresHost,
                Port = options.PostgresPort,
                Username = options.PostgresUser,
                Password = options.PostgresPassword,
                Enlist = false
            };

            return builder.ConnectionString;
        }

        private static string GetDatabaseName(ConfigFileOptions options, MigrationType migrationType)
        {
            return migrationType switch
            {
                MigrationType.Main => options.PostgresMainDb,
                MigrationType.Log => options.PostgresLogDb,
                _ => throw new NotImplementedException("Unknown migration type")
            };
        }
    }
}
