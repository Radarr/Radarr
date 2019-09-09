using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Common.Serializer;
using System.Collections.Generic;
using NzbDrone.Core.Music;
using System.Data;
using System;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(023)]
    public class add_release_groups_etc : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // ARTISTS TABLE
            
            Create.TableForModel("ArtistMetadata")
                .WithColumn("ForeignArtistId").AsString().Unique()
                .WithColumn("Name").AsString()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Disambiguation").AsString().Nullable()
                .WithColumn("Type").AsString().Nullable()
                .WithColumn("Status").AsInt32()
                .WithColumn("Images").AsString()
                .WithColumn("Links").AsString().Nullable()
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Members").AsString().Nullable();

            // we want to preserve the artist ID.  Shove all the metadata into the metadata table.
            Execute.Sql(@"INSERT INTO ArtistMetadata (ForeignArtistId, Name, Overview, Disambiguation, Type, Status, Images, Links, Genres, Ratings, Members)
                          SELECT ForeignArtistId, Name, Overview, Disambiguation, ArtistType, Status, Images, Links, Genres, Ratings, Members
                          FROM Artists");
            
            // Add an ArtistMetadataId column to Artists
            Alter.Table("Artists").AddColumn("ArtistMetadataId").AsInt32().WithDefaultValue(0);

            // Update artistmetadataId
            Execute.Sql(@"UPDATE Artists
                          SET ArtistMetadataId = (SELECT ArtistMetadata.Id 
                                                  FROM ArtistMetadata 
                                                  WHERE ArtistMetadata.ForeignArtistId = Artists.ForeignArtistId)");

            // ALBUM RELEASES TABLE - Do this before we mess with the Albums table

            Create.TableForModel("AlbumReleases")
                .WithColumn("ForeignReleaseId").AsString().Unique()
                .WithColumn("AlbumId").AsInt32().Indexed()
                .WithColumn("Title").AsString()
                .WithColumn("Status").AsString()
                .WithColumn("Duration").AsInt32().WithDefaultValue(0)
                .WithColumn("Label").AsString().Nullable()
                .WithColumn("Disambiguation").AsString().Nullable()
                .WithColumn("Country").AsString().Nullable()
                .WithColumn("ReleaseDate").AsDateTime().Nullable()
                .WithColumn("Media").AsString().Nullable()
                .WithColumn("TrackCount").AsInt32().Nullable()
                .WithColumn("Monitored").AsBoolean();

            Execute.WithConnection(PopulateReleases);

            // ALBUMS TABLE

            // Add in the extra columns and update artist metadata id
            Alter.Table("Albums").AddColumn("ArtistMetadataId").AsInt32().WithDefaultValue(0);
            Alter.Table("Albums").AddColumn("AnyReleaseOk").AsBoolean().WithDefaultValue(true);
            Alter.Table("Albums").AddColumn("Links").AsString().Nullable();

            // Set metadata ID
            Execute.Sql(@"UPDATE Albums
                          SET ArtistMetadataId = (SELECT ArtistMetadata.Id 
                                                  FROM ArtistMetadata 
                                                  JOIN Artists ON ArtistMetadata.Id = Artists.ArtistMetadataId
                                                  WHERE Albums.ArtistId = Artists.Id)");
            
            // TRACKS TABLE
            Alter.Table("Tracks").AddColumn("ForeignRecordingId").AsString().WithDefaultValue("0");
            Alter.Table("Tracks").AddColumn("AlbumReleaseId").AsInt32().WithDefaultValue(0);
            Alter.Table("Tracks").AddColumn("ArtistMetadataId").AsInt32().WithDefaultValue(0);
            
            // Set track release to the only release we've bothered populating
            Execute.Sql(@"UPDATE Tracks
                          SET AlbumReleaseId = (SELECT AlbumReleases.Id 
                                                FROM AlbumReleases
                                                JOIN Albums ON AlbumReleases.AlbumId = Albums.Id
                                                WHERE Albums.Id = Tracks.AlbumId)");

            // Set metadata ID
            Execute.Sql(@"UPDATE Tracks
                          SET ArtistMetadataId = (SELECT ArtistMetadata.Id 
                                                  FROM ArtistMetadata 
                                                  JOIN Albums ON ArtistMetadata.Id = Albums.ArtistMetadataId
                                                  WHERE Tracks.AlbumId = Albums.Id)");

            // CLEAR OUT OLD COLUMNS

            // Remove the columns in Artists now in ArtistMetadata
            Delete.Column("ForeignArtistId")
                .Column("Name")
                .Column("Overview")
                .Column("Disambiguation")
                .Column("ArtistType")
                .Column("Status")
                .Column("Images")
                .Column("Links")
                .Column("Genres")
                .Column("Ratings")
                .Column("Members")
                // as well as the ones no longer used
                .Column("MBId")
                .Column("AMId")
                .Column("TADBId")
                .Column("DiscogsId")
                .Column("NameSlug")
                .Column("LastDiskSync")
                .Column("DateFormed")
                .FromTable("Artists");

            // Remove old columns from Albums
            Delete.Column("ArtistId")
                .Column("MBId")
                .Column("AMId")
                .Column("TADBId")
                .Column("DiscogsId")
                .Column("TitleSlug")
                .Column("Label")
                .Column("SortTitle")
                .Column("Tags")
                .Column("Duration")
                .Column("Media")
                .Column("Releases")
                .Column("CurrentRelease")
                .Column("LastDiskSync")
                .FromTable("Albums");

            // Remove old columns from Tracks
            Delete.Column("ArtistId")
                .Column("AlbumId")
                .Column("Compilation")
                .Column("DiscNumber")
                .Column("Monitored")
                .FromTable("Tracks");

            // Remove old columns from TrackFiles
            Delete.Column("ArtistId").FromTable("TrackFiles");
            
            // Add indices
            Create.Index().OnTable("Artists").OnColumn("ArtistMetadataId").Ascending();
            Create.Index().OnTable("Artists").OnColumn("Monitored").Ascending();
            Create.Index().OnTable("Albums").OnColumn("ArtistMetadataId").Ascending();
            Create.Index().OnTable("Tracks").OnColumn("ArtistMetadataId").Ascending();
            Create.Index().OnTable("Tracks").OnColumn("AlbumReleaseId").Ascending();
            Create.Index().OnTable("Tracks").OnColumn("ForeignRecordingId").Ascending();

            // Force a metadata refresh
            Update.Table("Artists").Set(new { LastInfoSync = new System.DateTime(2018, 1, 1, 0, 0, 1)}).AllRows();
            Update.Table("Albums").Set(new { LastInfoSync = new System.DateTime(2018, 1, 1, 0, 0, 1)}).AllRows();
            Update.Table("ScheduledTasks")
                .Set(new { LastExecution = new System.DateTime(2018, 1, 1, 0, 0, 1)})
                .Where(new { TypeName = "NzbDrone.Core.Music.Commands.RefreshArtistCommand" });

        }

        private void PopulateReleases(IDbConnection conn, IDbTransaction tran)
        {
            var releases = ReadReleasesFromAlbums(conn, tran);
            var dupeFreeReleases = releases.DistinctBy(x => x.ForeignReleaseId).ToList();
            var duplicates = releases.Except(dupeFreeReleases);
            foreach (var release in duplicates)
            {
                release.ForeignReleaseId = release.AlbumId.ToString();
            }
            WriteReleasesToReleases(releases, conn, tran);
        }

        public class LegacyAlbumRelease : IEmbeddedDocument
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public DateTime? ReleaseDate { get; set; }
            public int TrackCount { get; set; }
            public int MediaCount { get; set; }
            public string Disambiguation { get; set; }
            public List<string> Country { get; set; }
            public string Format { get; set; }
            public List<string> Label { get; set; }
        }

        private List<AlbumRelease> ReadReleasesFromAlbums(IDbConnection conn, IDbTransaction tran)
        {

            // need to get all the old albums
            var releases = new List<AlbumRelease>();

            using (var getReleasesCmd = conn.CreateCommand())
            {
                getReleasesCmd.Transaction = tran;
                getReleasesCmd.CommandText = @"SELECT Id, CurrentRelease FROM Albums";

                using (var releaseReader = getReleasesCmd.ExecuteReader())
                {
                    while (releaseReader.Read())
                    {
                        int albumId = releaseReader.GetInt32(0);
                        var albumRelease = Json.Deserialize<LegacyAlbumRelease>(releaseReader.GetString(1));

                        AlbumRelease toInsert = null;
                        if (albumRelease != null)
                        {
                            var media = new List<Medium>();
                            for (var i = 1; i <= Math.Max(albumRelease.MediaCount, 1); i++)
                            {
                                media.Add(new Medium { Number = i, Name = "", Format = albumRelease.Format ?? "Unknown" } );
                            }
                        
                            toInsert = new AlbumRelease {
                                    AlbumId = albumId,
                                    ForeignReleaseId = albumRelease.Id.IsNotNullOrWhiteSpace() ? albumRelease.Id : albumId.ToString(),
                                    Title = albumRelease.Title.IsNotNullOrWhiteSpace() ? albumRelease.Title : "",
                                    Status = "",
                                    Duration = 0,
                                    Label = albumRelease.Label,
                                    Disambiguation = albumRelease.Disambiguation,
                                    Country = albumRelease.Country,
                                    Media = media,
                                    TrackCount = albumRelease.TrackCount,
                                    Monitored = true
                                };
                        }
                        else
                        {
                            toInsert = new AlbumRelease {
                                AlbumId = albumId,
                                ForeignReleaseId = albumId.ToString(),
                                Title = "",
                                Status = "",
                                Label = new List<string>(),
                                Country = new List<string>(),
                                Media = new List<Medium> { new Medium { Name = "Unknown", Number = 1, Format = "Unknown" } },
                                Monitored = true
                            };
                        }

                        releases.Add(toInsert);
                    }
                }
            }

            return releases;
        }

        private void WriteReleasesToReleases(List<AlbumRelease> releases, IDbConnection conn, IDbTransaction tran)
        {
            foreach (var release in releases)
            {
                using (var writeReleaseCmd = conn.CreateCommand())
                {
                    writeReleaseCmd.Transaction = tran;
                    writeReleaseCmd.CommandText =
                        "INSERT INTO AlbumReleases (AlbumId, ForeignReleaseId, Title, Status, Duration, Label, Disambiguation, Country, Media, TrackCount, Monitored) " +
                        "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                    writeReleaseCmd.AddParameter(release.AlbumId);
                    writeReleaseCmd.AddParameter(release.ForeignReleaseId);
                    writeReleaseCmd.AddParameter(release.Title);
                    writeReleaseCmd.AddParameter(release.Status);
                    writeReleaseCmd.AddParameter(release.Duration);
                    writeReleaseCmd.AddParameter(release.Label.ToJson());
                    writeReleaseCmd.AddParameter(release.Disambiguation);
                    writeReleaseCmd.AddParameter(release.Country.ToJson());
                    writeReleaseCmd.AddParameter(release.Media.ToJson());
                    writeReleaseCmd.AddParameter(release.TrackCount);
                    writeReleaseCmd.AddParameter(release.Monitored);

                    writeReleaseCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
