using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(028)]
    public class clean_artist_metadata_table : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Remove any artists linked to missing metadata
            Execute.Sql(@"DELETE FROM Artists
                          WHERE Id in (
                          SELECT Artists.Id from Artists
                          LEFT OUTER JOIN ArtistMetadata ON Artists.ArtistMetadataId = ArtistMetadata.Id
                          WHERE ArtistMetadata.Id IS NULL)");

            // Remove any albums linked to missing metadata
            Execute.Sql(@"DELETE FROM Albums
                          WHERE Id in (
                          SELECT Albums.Id from Albums
                          LEFT OUTER JOIN ArtistMetadata ON Albums.ArtistMetadataId = ArtistMetadata.Id
                          WHERE ArtistMetadata.Id IS NULL)");

            // Remove any album releases linked to albums that were deleted
            Execute.Sql(@"DELETE FROM AlbumReleases
                          WHERE Id in (
                          SELECT AlbumReleases.Id from AlbumReleases
                          LEFT OUTER JOIN Albums ON Albums.Id = AlbumReleases.AlbumId
                          WHERE Albums.Id IS NULL)");

            // Remove any tracks linked to album releases that were deleted
            Execute.Sql(@"DELETE FROM Tracks
                          WHERE Id in (
                          SELECT Tracks.Id from Tracks
                          LEFT OUTER JOIN AlbumReleases ON Tracks.AlbumReleaseId = AlbumReleases.Id
                          WHERE AlbumReleases.Id IS NULL)");

            // Remove any tracks linked to the original missing metadata
            Execute.Sql(@"DELETE FROM Tracks
                          WHERE Id in (
                          SELECT Tracks.Id from Tracks
                          LEFT OUTER JOIN ArtistMetadata ON Tracks.ArtistMetadataId = ArtistMetadata.Id
                          WHERE ArtistMetadata.Id IS NULL)");

            // Remove any trackfiles linked to the deleted tracks
            Execute.Sql(@"DELETE FROM TrackFiles
                          WHERE Id IN (
                          SELECT TrackFiles.Id FROM TrackFiles
                          LEFT OUTER JOIN Tracks
                          ON TrackFiles.Id = Tracks.TrackFileId
                          WHERE Tracks.Id IS NULL)");
        }
    }
}
