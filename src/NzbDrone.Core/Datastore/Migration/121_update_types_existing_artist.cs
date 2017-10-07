using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(121)]
    public class update_types_existing_artist : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE Artists SET PrimaryAlbumTypes = '[]', SecondaryAlbumTypes = '[]'");
        }
    }
}
