using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(131)]
    public class make_parsed_episode_info_nullable : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("PendingReleases").AlterColumn("ParsedEpisodeInfo").AsString().Nullable();
        }
    }
}
