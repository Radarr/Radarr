using System.Linq;
using FluentAssertions;
using FizzWare.NBuilder;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Common.Serializer;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_release_groups_etcFixture : MigrationTest<add_release_groups_etc>
    {
        private void GivenArtist(add_release_groups_etc c, int id, string name)
        {
            c.Insert.IntoTable("Artists").Row(new
                {
                    Id = id,
                    ForeignArtistId = id.ToString(),
                    Name = name,
                    CleanName = name,
                    Status = 1,
                    Images = "",
                    Path = $"/mnt/data/path/{name}",
                    Monitored = 1,
                    AlbumFolder = 1,
                    LanguageProfileId = 1,
                    MetadataProfileId = 1
                });
        }

        private void GivenAlbum(add_release_groups_etc c, int id, int artistId, string title, string currentRelease)
        {
            c.Insert.IntoTable("Albums").Row(new
                {
                    Id = id,
                    ForeignAlbumId = id.ToString(),
                    ArtistId = artistId,
                    Title = title,
                    CleanTitle = title,
                    Images = "",
                    Monitored = 1,
                    AlbumType = "Studio",
                    Duration = 100,
                    Media = "",
                    Releases = "",
                    CurrentRelease = currentRelease
                });
        }

        private void GivenTracks(add_release_groups_etc c, int artistid, int albumid, int firstId, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var id = firstId + i;
                c.Insert.IntoTable("Tracks").Row(new
                    {
                        Id = id,
                        ForeignTrackId = id.ToString(),
                        ArtistId = artistid,
                        AlbumId = albumid,
                        Explicit = 0,
                        Compilation = 0,
                        Monitored = 0,
                        Duration = 100,
                        MediumNumber = 1,
                        AbsoluteTrackNumber = i,
                        TrackNumber = i.ToString()
                    });
            }
        }

        private IEnumerable<AlbumRelease> VerifyAlbumReleases(IDirectDataMapper db)
        {
            var releases = db.Query<AlbumRelease>("SELECT * FROM AlbumReleases");
            var albums = db.Query<Album>("SELECT * FROM Albums");

            // we only put in one release per album
            releases.Count().Should().Be(albums.Count());

            // each album should be linked to exactly one release
            releases.Select(x => x.AlbumId).SequenceEqual(albums.Select(x => x.Id)).Should().Be(true);

            // each release should have at least one medium
            releases.Select(x => x.Media.Count).Min().Should().BeGreaterOrEqualTo(1);

            return releases;
        }

        private void VerifyTracks(IDirectDataMapper db, int albumId, int albumReleaseId, int expectedCount)
        {
            var tracks = db.Query<Track>("SELECT Tracks.* FROM Tracks " +
                                         "JOIN AlbumReleases ON Tracks.AlbumReleaseId = AlbumReleases.Id " +
                                         "JOIN Albums ON AlbumReleases.AlbumId = Albums.Id " +
                                         "WHERE Albums.Id = " + albumId).ToList();

            var album = db.Query<Album>("SELECT * FROM Albums WHERE Albums.Id = " + albumId).ToList().Single();

            tracks.Count.Should().Be(expectedCount);
            tracks.First().AlbumReleaseId.Should().Be(albumReleaseId);
            tracks.All(t => t.ArtistMetadataId == album.ArtistMetadataId).Should().BeTrue();
        }
        
        [Test]
        public void migration_023_simple_case()
        {
            var release = Builder<add_release_groups_etc.LegacyAlbumRelease>
                .CreateNew()
                .Build();
            
            var db = WithMigrationTestDb(c =>
                {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum", release.ToJson());
                    GivenTracks(c, 1, 1, 1, 10);
                });

            VerifyAlbumReleases(db);
            VerifyTracks(db, 1, 1, 10);
        }

        [Test]
        public void migration_023_multiple_media()
        {
            var release = Builder<add_release_groups_etc.LegacyAlbumRelease>
                .CreateNew()
                .With(e => e.MediaCount = 2)
                .Build();
            
            var db = WithMigrationTestDb(c =>
                {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum", release.ToJson());
                    GivenTracks(c, 1, 1, 1, 10);
                });

            var migrated = VerifyAlbumReleases(db);
            migrated.First().Media.Count.Should().Be(2);
            
            VerifyTracks(db, 1, 1, 10);
        }

        [Test]
        public void migration_023_null_title()
        {
            var release = Builder<add_release_groups_etc.LegacyAlbumRelease>
                .CreateNew()
                .With(e => e.Title = null)
                .Build();

            var db = WithMigrationTestDb(c =>
                {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum", release.ToJson());
                    GivenTracks(c, 1, 1, 1, 10);
                });

            VerifyAlbumReleases(db);
            VerifyTracks(db, 1, 1, 10);
        }

        [Test]
        public void migration_023_all_default_entries()
        {
            var release = new add_release_groups_etc.LegacyAlbumRelease();

            var db = WithMigrationTestDb(c =>
                {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum", release.ToJson());
                    GivenTracks(c, 1, 1, 1, 10);
                });

            VerifyAlbumReleases(db);
            VerifyTracks(db, 1, 1, 10);
        }

        [Test]
        public void migration_023_empty_albumrelease()
        {
            var db = WithMigrationTestDb(c =>
                {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum", "");
                    GivenTracks(c, 1, 1, 1, 10);
                });

            VerifyAlbumReleases(db);
            VerifyTracks(db, 1, 1, 10);
        }

        [Test]
        public void migration_023_duplicate_albumrelease()
        {
            var release = Builder<add_release_groups_etc.LegacyAlbumRelease>
                .CreateNew()
                .Build();
            
            var db = WithMigrationTestDb(c =>
                {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum1", release.ToJson());
                    GivenTracks(c, 1, 1, 1, 10);
                    GivenAlbum(c, 2, 1, "TestAlbum2", release.ToJson());
                    GivenTracks(c, 1, 2, 100, 10);

                });

            VerifyAlbumReleases(db);
            VerifyTracks(db, 1, 1, 10);
            VerifyTracks(db, 2, 2, 10);
        }

        [Test]
        public void migration_023_duplicate_foreignreleaseid()
        {
            var releases = Builder<add_release_groups_etc.LegacyAlbumRelease>
                .CreateListOfSize(2)
                .All()
                .With(e => e.Id = "TestForeignId")
                .Build();
            
            var db = WithMigrationTestDb(c =>
                {
                    GivenArtist(c, 1, "TestArtist");
                    GivenAlbum(c, 1, 1, "TestAlbum1", releases[0].ToJson());
                    GivenTracks(c, 1, 1, 1, 10);
                    GivenAlbum(c, 2, 1, "TestAlbum2", releases[1].ToJson());
                    GivenTracks(c, 1, 2, 100, 10);

                });

            VerifyAlbumReleases(db);
            VerifyTracks(db, 1, 1, 10);
            VerifyTracks(db, 2, 2, 10);
        }
    }
}
