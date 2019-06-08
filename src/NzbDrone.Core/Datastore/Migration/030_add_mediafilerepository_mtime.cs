using System;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(30)]
    public class add_mediafilerepository_mtime : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("TrackFiles").AddColumn("Modified").AsDateTime().WithDefaultValue(new DateTime(2000, 1, 1));
            Alter.Table("TrackFiles").AddColumn("Path").AsString().Nullable();

            // Remove anything where RelativePath is null
            Execute.Sql(@"DELETE FROM TrackFiles WHERE RelativePath IS NULL");

            // Remove anything not linked to a track (these shouldn't be present in version < 30)
            Execute.Sql(@"DELETE FROM TrackFiles
                          WHERE Id IN (
                            SELECT TrackFiles.Id FROM TrackFiles
                            LEFT JOIN Tracks ON TrackFiles.Id = Tracks.TrackFileId
                            WHERE Tracks.Id IS NULL)");

            // Remove anything where we can't get an artist path (i.e. we don't know where it is)
            Execute.Sql(@"DELETE FROM TrackFiles
                          WHERE Id IN (
                            SELECT TrackFiles.Id FROM TrackFiles
                            LEFT JOIN Albums ON TrackFiles.AlbumId = Albums.Id
                            LEFT JOIN Artists on Artists.ArtistMetadataId = Albums.ArtistMetadataId
                            WHERE Artists.Path IS NULL)");

            // Remove anything linked to unmonitored or unidentified releases.  This should ensure uniqueness of track files.
            Execute.Sql(@"DELETE FROM TrackFiles
                          WHERE Id IN (
                            SELECT TrackFiles.Id FROM TrackFiles
                            LEFT JOIN Tracks ON TrackFiles.Id = Tracks.TrackFileId
                            LEFT JOIN AlbumReleases ON Tracks.AlbumReleaseId = AlbumReleases.Id
                            WHERE AlbumReleases.Monitored = 0
                            OR AlbumReleases.Monitored IS NULL)");

            // Populate the full paths
            Execute.Sql(@"UPDATE TrackFiles
                          SET Path = (SELECT Artists.Path || '" + System.IO.Path.DirectorySeparatorChar + @"' || TrackFiles.RelativePath
                                      FROM Artists
                                      JOIN Albums ON Albums.ArtistMetadataId = Artists.ArtistMetadataId
                                      WHERE TrackFiles.AlbumId = Albums.Id)");
            
            // Belt and braces to ensure uniqueness
            Execute.Sql(@"DELETE FROM TrackFiles 
                          WHERE rowid NOT IN (
                            SELECT min(rowid)
                            FROM TrackFiles
                            GROUP BY Path
                          )");
            
            // Now enforce the uniqueness constraint
            Alter.Table("TrackFiles").AlterColumn("Path").AsString().NotNullable().Unique();

            // Finally delete the relative path column
            Delete.Column("RelativePath").FromTable("TrackFiles");
        }
    }
}
