using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(188)]
    public class mediainfo_channels : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"MovieFiles\" SET \"MediaInfo\" = Replace(\"MediaInfo\", '\"audioChannels\"', '\"audioChannelsContainer\"');");
            Execute.Sql("UPDATE \"MovieFiles\" SET \"MediaInfo\" = Replace(\"MediaInfo\", '\"audioChannelPositionsText\"', '\"audioChannelPositionsTextContainer\"');");

            //Change List Interval from Min to Hour
            IfDatabase("sqlite").Execute.Sql("UPDATE \"Config\" SET \"Value\" = max((\"Value\" / 60) + 1, 6) WHERE \"Key\" = 'importlistsyncinterval'");
            IfDatabase("postgres").Execute.Sql("UPDATE \"Config\" SET \"Value\" = greatest((\"Value\"::int / 60) + 1, 6) WHERE \"Key\" = 'importlistsyncinterval'");
        }
    }
}
