using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(145)]
    public class banner_to_fanart : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Movies\" SET \"Images\" = replace(\"Images\", \'\"coverType\": \"banner\"\', \'\"coverType\": \"fanart\"\')");

            // Remove Link for images to specific MovieFiles, Images are now related to the Movie object only
            Execute.Sql("UPDATE \"MetadataFiles\" SET \"MovieFileId\" = null WHERE \"Type\" = 2");
        }
    }
}
