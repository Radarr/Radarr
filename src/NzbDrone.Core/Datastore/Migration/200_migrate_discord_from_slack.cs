using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(200)]
    public class migrate_discord_from_slack : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(MigrateDiscordFromSlack);
        }

        private void MigrateDiscordFromSlack(IDbConnection conn, IDbTransaction tran)
        {
            var notificationRows = conn.Query<NotificationEntity200>($"SELECT * FROM Notifications WHERE Implementation = 'Slack'");

            var discordSlackNotifications = notificationRows.Where(n => (n.Settings as SlackNotificationSettings200).WebHookUrl.Contains("discord"));
            var discordNotifications = new List<NotificationEntity200>();

            if (!discordSlackNotifications.Any())
            {
                return;
            }

            foreach (NotificationEntity200 notification in discordSlackNotifications)
            {
                SlackNotificationSettings200 settings = notification.Settings as SlackNotificationSettings200;
                DiscordNotificationSettings200 discordSettings = new DiscordNotificationSettings200
                {
                    Avatar = settings.Icon,
                    Username = settings.Username,
                    WebHookURL = settings.WebHookUrl.Replace("/slack", "")
                };

                discordNotifications.Add(new NotificationEntity200
                {
                    Name = notification.Name,
                    OnGrab = notification.OnGrab,
                    OnDownload = notification.OnDownload,
                    Implementation = "Discord",
                    ConfigContract = "DiscordSettings",
                    OnUpgrade = notification.OnUpgrade,
                    Tags = notification.Tags,
                    OnRename = notification.OnRename,
                    OnHealthIssue = notification.OnHealthIssue,
                    IncludeHealthWarnings = notification.IncludeHealthWarnings,
                    OnMovieDelete = notification.OnMovieDelete,
                    OnMovieFileDelete = notification.OnMovieFileDelete,
                    OnMovieFileDeleteForUpgrade = notification.OnMovieFileDeleteForUpgrade,
                    Settings = discordSettings
                });
            }
        }

        public class NotificationEntity200
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int OnGrab { get; set; }
            public int OnDownload { get; set; }
            public object Settings { get; set; }
            public string Implementation { get; set; }
            public string ConfigContract { get; set; }
            public int OnUpgrade { get; set; }
            public string Tags { get; set; }
            public int OnRename { get; set; }
            public int OnHealthIssue { get; set; }
            public int IncludeHealthWarnings { get; set; }
            public int OnMovieDelete { get; set; }
            public int OnMovieFileDelete { get; set; }
            public int OnMovieFileDeleteForUpgrade { get; set; }
        }

        public class SlackNotificationSettings200
        {
            public string WebHookUrl { get; set; }
            public string Username { get; set; }
            public string Icon { get; set; }
            public string Channel { get; set; }
        }

        public class DiscordNotificationSettings200
        {
            public string WebHookURL { get; set; }
            public string Username { get; set; }
            public string Avatar { get; set; }
            public IEnumerable<int> GrabFields = new List<int> { 0, 1, 2, 3, 5, 6, 7, 8, 9 };
            public IEnumerable<int> ImportFields = new List<int> { 0, 1, 2, 3, 4, 6, 7, 8, 9, 10, 11, 12 };
        }
    }
}
