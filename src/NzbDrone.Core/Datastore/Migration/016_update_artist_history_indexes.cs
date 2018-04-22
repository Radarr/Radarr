using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(16)]
    public class update_artist_history_indexes : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Index().OnTable("Albums").OnColumn("ArtistId");
            Create.Index().OnTable("Albums").OnColumn("ArtistId").Ascending()
                                            .OnColumn("ReleaseDate").Ascending();

            Delete.Index().OnTable("History").OnColumn("AlbumId");
            Create.Index().OnTable("History").OnColumn("AlbumId").Ascending()
                                             .OnColumn("Date").Descending();

            Delete.Index().OnTable("History").OnColumn("DownloadId");
            Create.Index().OnTable("History").OnColumn("DownloadId").Ascending()
                                             .OnColumn("Date").Descending();
        }
    }
}
