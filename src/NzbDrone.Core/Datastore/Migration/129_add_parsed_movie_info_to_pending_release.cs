using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(129)]
    public class add_parsed_movie_info_to_pending_release : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("PendingReleases").AddColumn("ParsedMovieInfo").AsString().Nullable();
        }
    }
}
