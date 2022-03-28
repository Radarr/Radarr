using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(174)]
    public class email_multiple_addresses : NzbDroneMigrationBase
    {
        private readonly JsonSerializerOptions _serializerSettings;

        public email_multiple_addresses()
        {
            _serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ChangeEmailAddressType);
        }

        private void ChangeEmailAddressType(IDbConnection conn, IDbTransaction tran)
        {
            var rows = conn.Query<ProviderDefinition166>($"SELECT \"Id\", \"Settings\" FROM \"Notifications\" WHERE \"Implementation\" = 'Email'");

            var corrected = new List<ProviderDefinition166>();

            foreach (var row in rows)
            {
                var settings = JsonSerializer.Deserialize<EmailSettings173>(row.Settings, _serializerSettings);

                var newSettings = new EmailSettings174
                {
                    Server = settings.Server,
                    Port = settings.Port,
                    Ssl = settings.Ssl,
                    Username = settings.Username,
                    Password = settings.Password,
                    From = settings.From,
                    To = new string[] { settings.To },
                    CC = Array.Empty<string>(),
                    Bcc = Array.Empty<string>()
                };

                corrected.Add(new ProviderDefinition166
                {
                    Id = row.Id,
                    Settings = JsonSerializer.Serialize(newSettings, _serializerSettings)
                });
            }

            var updateSql = "UPDATE \"Notifications\" SET \"Settings\" = @Settings WHERE \"Id\" = @Id";
            conn.Execute(updateSql, corrected, transaction: tran);
        }

        private class ProviderDefinition166 : ModelBase
        {
            public string Settings { get; set; }
        }

        private class EmailSettings173
        {
            public string Server { get; set; }
            public int Port { get; set; }
            public bool Ssl { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string From { get; set; }
            public string To { get; set; }
        }

        private class EmailSettings174
        {
            public string Server { get; set; }
            public int Port { get; set; }
            public bool Ssl { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string From { get; set; }
            public string[] To { get; set; }
            public string[] CC { get; set; }
            public string[] Bcc { get; set; }
        }
    }
}
