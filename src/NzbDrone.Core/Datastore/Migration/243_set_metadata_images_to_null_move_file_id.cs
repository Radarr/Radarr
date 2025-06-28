using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(243)]
    public class set_metadata_images_to_null_move_file_id : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"MetadataFiles\" SET \"MovieFileId\" = NULL WHERE \"Type\" = 2");
        }
    }
}
