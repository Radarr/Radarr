using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(151)]
    public class update_movie_files_media_info : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE MovieFiles " +
                        "SET MediaInfo = REPLACE(MediaInfo, 'videoCodec', 'videoFormat') " +
                        "WHERE MediaInfo LIKE '%videoCodec%'");
        }
    }
}
