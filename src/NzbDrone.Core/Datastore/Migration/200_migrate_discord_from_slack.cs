using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Slack;
using NzbDrone.Core.Notifications.Discord;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(200)]
    public class migrate_discord_from_slack : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            
        }
    }

    public class DiscordFromSlackMigration
    {
        private readonly INotificationFactory _notificationFactory;

        public DiscordFromSlackMigration(INotificationFactory notificationFactory)
        {
            _notificationFactory = notificationFactory;
        }

        
    }
}
