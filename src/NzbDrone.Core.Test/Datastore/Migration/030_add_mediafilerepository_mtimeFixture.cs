using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;
using System.Collections.Generic;
using System;
using NzbDrone.Core.Qualities;
using NzbDrone.Common.Serializer;
using NzbDrone.Test.Common;
using System.Linq;
using FluentAssertions;
using System.IO;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_mediafilerepository_mtimeFixture : MigrationTest<add_mediafilerepository_mtime>
    {
        private string _artistPath = null;
        
        private void GivenArtist(add_mediafilerepository_mtime c, int id, string name)
        {
            _artistPath = $"/mnt/data/path/{name}".AsOsAgnostic();
            c.Insert.IntoTable("Artists").Row(new
                {
                    Id = id,
                    CleanName = name,
                    Path = _artistPath,
                    Monitored = 1,
                    AlbumFolder = 1,
                    LanguageProfileId = 1,
                    MetadataProfileId = 1,
                    ArtistMetadataId = id
                });
        }
        
        private void GivenAlbum(add_mediafilerepository_mtime c, int id, int artistMetadataId, string title)
        {
            c.Insert.IntoTable("Albums").Row(new
                {
                    Id = id,
                    ForeignAlbumId = id.ToString(),
                    ArtistMetadataId = artistMetadataId,
                    Title = title,
                    CleanTitle = title,
                    Images = "",
                    Monitored = 1,
                    AlbumType = "Studio",
                    AnyReleaseOk = 1
                });
        }

        private void GivenAlbumRelease(add_mediafilerepository_mtime c, int id, int albumId, bool monitored)
        {
            c.Insert.IntoTable("AlbumReleases").Row(new
                {
                    Id = id,
                    ForeignReleaseId = id.ToString(),
                    AlbumId = albumId,
                    Title = "Title",
                    Status = "Status",
                    Duration = 0,
                    Monitored = monitored
                });
        }

        private void GivenTrackFiles(add_mediafilerepository_mtime c, List<string> tracks, int albumReleaseId, int albumId, int firstId = 1, bool addTracks = true)
        {
            int id = firstId;
            foreach (var track in tracks)
            {
                c.Insert.IntoTable("TrackFiles").Row(new
                    {
                        Id = id,
                        RelativePath = track?.AsOsAgnostic(),
                        Size = 100,
                        DateAdded = DateTime.UtcNow,
                        Quality = new QualityModel(Quality.FLAC).ToJson(),
                        Language = 1,
                        AlbumId = albumId
                    });
                
                if (addTracks) 
                {
                    c.Insert.IntoTable("Tracks").Row(new
                        {
                            Id = id,
                            ForeignTrackId = id.ToString(),
                            Explicit = 0,
                            TrackFileId = id,
                            Duration = 100,
                            MediumNumber = 1,
                            AbsoluteTrackNumber = 1,
                            ForeignRecordingId = id.ToString(),
                            AlbumReleaseId = albumReleaseId,
                            ArtistMetadataId = 0
                        });
                }

                id++;
            }
        }
        
        private void VerifyTracksFiles(IDirectDataMapper db, int albumId, List<string> expectedPaths)
        {
            var tracks = db.Query("SELECT TrackFiles.* FROM TrackFiles " +
                                  "WHERE TrackFiles.AlbumId = " + albumId);
            
            TestLogger.Debug($"Got {tracks.Count} tracks");

            tracks.Select(x => x["Path"]).Should().BeEquivalentTo(expectedPaths);
        }

        [Test]
        public void migration_030_simple_case()
        {
            var tracks = new List<string> {
                "folder/track1.mp3",
                "folder/track2.mp3",
            };
            
            var db = WithMigrationTestDb(c => {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum");
                    GivenAlbumRelease(c, 1, 1, true);
                    GivenTrackFiles(c, tracks, 1, 1);
                });
            
            var expected = tracks.Select(x => Path.Combine(_artistPath, x)).ToList();
            
            VerifyTracksFiles(db, 1, expected);
        }

        [Test]
        public void migration_030_missing_path()
        {
            var tracks = new List<string> {
                "folder/track1.mp3",
                null,
            };
            
            var db = WithMigrationTestDb(c => {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum");
                    GivenAlbumRelease(c, 1, 1, true);
                    GivenTrackFiles(c, tracks, 1, 1);
                });
            
            var expected = tracks.GetRange(0, 1).Select(x => Path.Combine(_artistPath, x)).ToList();
            
            VerifyTracksFiles(db, 1, expected);
        }
        
        [Test]
        public void migration_030_bad_albumrelease_id()
        {
            var tracks = new List<string> {
                "folder/track1.mp3",
                "folder/track2.mp3"
            };
            
            var db = WithMigrationTestDb(c => {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum");
                    GivenAlbumRelease(c, 1, 1, true);
                    GivenTrackFiles(c, tracks, 2, 1);
                });
            
            VerifyTracksFiles(db, 1, new List<string>());
        }

        [Test]
        public void migration_030_bad_album_id()
        {
            var tracks = new List<string> {
                "folder/track1.mp3",
                "folder/track2.mp3"
            };
            
            var db = WithMigrationTestDb(c => {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum");
                    GivenAlbumRelease(c, 1, 1, true);
                    GivenTrackFiles(c, tracks, 1, 2);
                });
            
            VerifyTracksFiles(db, 1, new List<string>());
        }

        [Test]
        public void migration_030_bad_artist_metadata_id()
        {
            var tracks = new List<string> {
                "folder/track1.mp3",
                "folder/track2.mp3"
            };
            
            var db = WithMigrationTestDb(c => {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 2, "TestAlbum");
                    GivenAlbumRelease(c, 1, 1, true);
                    GivenTrackFiles(c, tracks, 1, 1);
                });
            
            VerifyTracksFiles(db, 1, new List<string>());
        }

        [Test]
        public void migration_030_missing_artist()
        {
            var tracks = new List<string> {
                "folder/track1.mp3",
                "folder/track2.mp3"
            };
            
            var db = WithMigrationTestDb(c => {
                    GivenAlbum(c, 1, 1, "TestAlbum");
                    GivenAlbumRelease(c, 1, 1, true);
                    GivenTrackFiles(c, tracks, 1, 1);
                });
            
            VerifyTracksFiles(db, 1, new List<string>());
        }

        [Test]
        public void migration_030_missing_tracks()
        {
            var tracks = new List<string> {
                "folder/track1.mp3",
                "folder/track2.mp3"
            };
            
            var db = WithMigrationTestDb(c => {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum");
                    GivenAlbumRelease(c, 1, 1, true);
                    GivenTrackFiles(c, tracks, 1, 1, addTracks: false);
                });
            
            VerifyTracksFiles(db, 1, new List<string>());
        }

        [Test]
        public void migration_030_duplicate_files()
        {
            var tracks = new List<string> {
                "folder/track1.mp3",
                "folder/track2.mp3",
                "folder/track1.mp3",
            };
            
            var db = WithMigrationTestDb(c => {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum");
                    GivenAlbumRelease(c, 1, 1, true);
                    GivenTrackFiles(c, tracks, 1, 1);
                });

            var expected = tracks.GetRange(0, 2).Select(x => Path.Combine(_artistPath, x)).ToList();
            
            VerifyTracksFiles(db, 1, expected);
        }

        [Test]
        public void migration_030_unmonitored_release_duplicate()
        {
            var monitored_tracks = new List<string> {
                "folder/track1.mp3",
                "folder/track2.mp3",
            };

            var unmonitored_tracks = new List<string> {
                "folder/track1.mp3",
                "folder/track2.mp3",
            };
            
            var db = WithMigrationTestDb(c => {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum");

                    GivenAlbumRelease(c, 1, 1, true);
                    GivenTrackFiles(c, monitored_tracks, 1, 1);

                    GivenAlbumRelease(c, 2, 1, false);
                    GivenTrackFiles(c, unmonitored_tracks, 2, 1, firstId: 100);
                });

            var expected = monitored_tracks.Select(x => Path.Combine(_artistPath, x)).ToList();
            
            VerifyTracksFiles(db, 1, expected);
        }

        [Test]
        public void migration_030_unmonitored_release_distinct()
        {
            var monitored_tracks = new List<string> {
                "folder/track1.mp3",
                "folder/track2.mp3",
            };

            var unmonitored_tracks = new List<string> {
                "folder/track3.mp3",
                "folder/track4.mp3",
            };
            
            var db = WithMigrationTestDb(c => {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum");

                    GivenAlbumRelease(c, 1, 1, true);
                    GivenTrackFiles(c, monitored_tracks, 1, 1);

                    GivenAlbumRelease(c, 2, 1, false);
                    GivenTrackFiles(c, unmonitored_tracks, 2, 1, firstId: 100);
                });

            var expected = monitored_tracks.Select(x => Path.Combine(_artistPath, x)).ToList();
            
            VerifyTracksFiles(db, 1, expected);
        }
    }        
}
