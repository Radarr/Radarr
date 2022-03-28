using FluentMigrator;
using Newtonsoft.Json.Linq;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(195)]
    public class update_notifiarr : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Notifications\" SET \"Implementation\" = Replace(\"Implementation\", 'DiscordNotifier', 'Notifiarr'),\"ConfigContract\" = Replace(\"ConfigContract\", 'DiscordNotifierSettings', 'NotifiarrSettings') WHERE \"Implementation\" = 'DiscordNotifier';");
        }
    }
}
