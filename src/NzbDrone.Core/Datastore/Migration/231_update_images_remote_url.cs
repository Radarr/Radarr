using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(231)]
    public class update_images_remote_url : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"MovieMetadata\" SET \"Images\" = REPLACE(\"Images\", '\"url\"', '\"remoteUrl\"')");
            Execute.Sql("UPDATE \"Credits\" SET \"Images\" = REPLACE(\"Images\", '\"url\"', '\"remoteUrl\"')");
            Execute.Sql("UPDATE \"Collections\" SET \"Images\" = REPLACE(\"Images\", '\"url\"', '\"remoteUrl\"')");
        }
    }
}
