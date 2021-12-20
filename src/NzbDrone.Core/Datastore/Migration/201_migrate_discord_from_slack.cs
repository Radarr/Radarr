using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(201)]
    public class migrate_discord_from_slack : NzbDroneMigrationBase
    {
        private readonly JsonSerializerOptions _serializerSettings;

        public migrate_discord_from_slack()
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

            _serializerSettings.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, true));
        }

        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(MigrateDiscordFromSlack);
        }

        private void MigrateDiscordFromSlack(IDbConnection conn, IDbTransaction tran)
        {
            var notificationRows = conn.Query<NotificationEntity201>($"SELECT Id,ConfigContract,Implementation,Name,Settings FROM Notifications WHERE Implementation = 'Slack'");

            var discordSlackNotifications = notificationRows.Where(n => JsonSerializer.Deserialize<SlackNotificationSettings201>(n.Settings, _serializerSettings).WebHookUrl.Contains("discord"));

            if (!discordSlackNotifications.Any())
            {
                return;
            }

            foreach (NotificationEntity201 notification in discordSlackNotifications)
            {
                SlackNotificationSettings201 settings = JsonSerializer.Deserialize<SlackNotificationSettings201>(notification.Settings, _serializerSettings);
                DiscordNotificationSettings201 discordSettings = new DiscordNotificationSettings201
                {
                    Avatar = settings.Icon,
                    GrabFields = new List<int> { 0, 1, 2, 3, 5, 6, 7, 8, 9 },
                    ImportFields = new List<int> { 0, 1, 2, 3, 4, 6, 7, 8, 9, 10, 11, 12 },
                    Username = settings.Username,
                    WebHookUrl = settings.WebHookUrl.Replace("/slack", "")
                };

                notification.ConfigContract = "DiscordSettings";
                notification.Implementation = "Discord";
                notification.Name = $"{notification.Name}-Slack_Migrated";
                notification.Settings = JsonSerializer.Serialize(discordSettings, _serializerSettings);
            }

            var updateSql = "UPDATE Notifications SET ConfigContract = @ConfigContract, " +
                "Implementation = @Implementation, " +
                "Name = @Name, " +
                "Settings = @Settings " +
                "WHERE Id = @Id";

            conn.Execute(updateSql, discordSlackNotifications, transaction: tran);
        }
    }

    public class NotificationEntity201
    {
        public int Id { get; set; }
        public string ConfigContract { get; set; }
        public string Implementation { get; set; }
        public string Name { get; set; }
        public string Settings { get; set; }
    }

    public class SlackNotificationSettings201
    {
        public string Channel { get; set; }
        public string Icon { get; set; }
        public string Username { get; set; }
        public string WebHookUrl { get; set; }
    }

    public class DiscordNotificationSettings201
    {
        public string Avatar { get; set; }
        public string Username { get; set; }
        public string WebHookUrl { get; set; }
        public IEnumerable<int> GrabFields { get; set; }
        public IEnumerable<int> ImportFields { get; set; }
    }
}
