using System.Collections.Generic;
using System.Linq;
using Dapper;
using FizzWare.NBuilder;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class LazyLoadingFixture : DbTest
    {
        [SetUp]
        public void Setup()
        {
            SqlBuilderExtensions.LogSql = true;

            var profile = new QualityProfile
            {
                Name = "Test",
                Cutoff = Quality.MP3_320.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities()
            };

            profile = Db.Insert(profile);

            var metadata = Builder<ArtistMetadata>.CreateNew()
                .With(v => v.Id = 0)
                .Build();
            Db.Insert(metadata);

            var artist = Builder<Artist>.CreateListOfSize(1)
                .All()
                .With(v => v.Id = 0)
                .With(v => v.QualityProfileId = profile.Id)
                .With(v => v.ArtistMetadataId = metadata.Id)
                .BuildListOfNew();

            Db.InsertMany(artist);

            var albums = Builder<Album>.CreateListOfSize(3)
                .All()
                .With(v => v.Id = 0)
                .With(v => v.ArtistMetadataId = metadata.Id)
                .BuildListOfNew();

            Db.InsertMany(albums);

            var releases = new List<AlbumRelease>();
            foreach (var album in albums)
            {
                releases.Add(
                    Builder<AlbumRelease>.CreateNew()
                    .With(v => v.Id = 0)
                    .With(v => v.AlbumId = album.Id)
                    .With(v => v.ForeignReleaseId = "test" + album.Id)
                    .Build());
            }

            Db.InsertMany(releases);

            var trackFiles = Builder<TrackFile>.CreateListOfSize(1)
                .All()
                .With(v => v.Id = 0)
                .With(v => v.AlbumId = albums[0].Id)
                .With(v => v.Quality = new QualityModel())
                .BuildListOfNew();

            Db.InsertMany(trackFiles);

            var tracks = Builder<Track>.CreateListOfSize(10)
                .All()
                .With(v => v.Id = 0)
                .With(v => v.TrackFileId = trackFiles[0].Id)
                .With(v => v.AlbumReleaseId = releases[0].Id)
                .BuildListOfNew();

            Db.InsertMany(tracks);
        }

        [Test]
        public void should_lazy_load_artist_for_track()
        {
            var db = Mocker.Resolve<TrackRepository>();

            var tracks = db.All();

            Assert.IsNotEmpty(tracks);
            foreach (var track in tracks)
            {
                Assert.IsFalse(track.Artist.IsLoaded);
                Assert.IsNotNull(track.Artist.Value);
                Assert.IsTrue(track.Artist.IsLoaded);
                Assert.IsTrue(track.Artist.Value.Metadata.IsLoaded);
            }
        }

        [Test]
        public void should_lazy_load_artist_for_trackfile()
        {
            var db = Mocker.Resolve<IDatabase>();
            var tracks = db.Query<TrackFile>(new SqlBuilder()).ToList();

            Assert.IsNotEmpty(tracks);
            foreach (var track in tracks)
            {
                Assert.IsFalse(track.Artist.IsLoaded);
                Assert.IsNotNull(track.Artist.Value);
                Assert.IsTrue(track.Artist.IsLoaded);
                Assert.IsTrue(track.Artist.Value.Metadata.IsLoaded);
            }
        }

        [Test]
        public void should_lazy_load_trackfile_if_not_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var tracks = db.Query<Track>(new SqlBuilder()).ToList();

            foreach (var track in tracks)
            {
                Assert.IsFalse(track.TrackFile.IsLoaded);
                Assert.IsNotNull(track.TrackFile.Value);
                Assert.IsTrue(track.TrackFile.IsLoaded);
            }
        }

        [Test]
        public void should_explicit_load_everything_if_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var files = MediaFileRepository.Query(db,
                                                  new SqlBuilder()
                                                  .Join<TrackFile, Track>((f, t) => f.Id == t.TrackFileId)
                                                  .Join<TrackFile, Album>((t, a) => t.AlbumId == a.Id)
                                                  .Join<Album, Artist>((album, artist) => album.ArtistMetadataId == artist.ArtistMetadataId)
                                                  .Join<Artist, ArtistMetadata>((a, m) => a.ArtistMetadataId == m.Id));

            Assert.IsNotEmpty(files);
            foreach (var file in files)
            {
                Assert.IsTrue(file.Tracks.IsLoaded);
                Assert.IsNotEmpty(file.Tracks.Value);
                Assert.IsTrue(file.Album.IsLoaded);
                Assert.IsTrue(file.Artist.IsLoaded);
                Assert.IsTrue(file.Artist.Value.Metadata.IsLoaded);
            }
        }

        [Test]
        public void should_lazy_load_tracks_if_not_joined_to_trackfile()
        {
            var db = Mocker.Resolve<IDatabase>();
            var files = db.QueryJoined<TrackFile, Album, Artist, ArtistMetadata>(
                new SqlBuilder()
                .Join<TrackFile, Album>((t, a) => t.AlbumId == a.Id)
                .Join<Album, Artist>((album, artist) => album.ArtistMetadataId == artist.ArtistMetadataId)
                .Join<Artist, ArtistMetadata>((a, m) => a.ArtistMetadataId == m.Id),
                (file, album, artist, metadata) =>
                {
                    file.Album = album;
                    file.Artist = artist;
                    file.Artist.Value.Metadata = metadata;
                    return file;
                });

            Assert.IsNotEmpty(files);
            foreach (var file in files)
            {
                Assert.IsFalse(file.Tracks.IsLoaded);
                Assert.IsNotNull(file.Tracks.Value);
                Assert.IsNotEmpty(file.Tracks.Value);
                Assert.IsTrue(file.Tracks.IsLoaded);
                Assert.IsTrue(file.Album.IsLoaded);
                Assert.IsTrue(file.Artist.IsLoaded);
                Assert.IsTrue(file.Artist.Value.Metadata.IsLoaded);
            }
        }

        [Test]
        public void should_lazy_load_tracks_if_not_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var release = db.Query<AlbumRelease>(new SqlBuilder().Where<AlbumRelease>(x => x.Id == 1)).SingleOrDefault();

            Assert.IsFalse(release.Tracks.IsLoaded);
            Assert.IsNotNull(release.Tracks.Value);
            Assert.IsNotEmpty(release.Tracks.Value);
            Assert.IsTrue(release.Tracks.IsLoaded);
        }

        [Test]
        public void should_lazy_load_track_if_not_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var tracks = db.Query<TrackFile>(new SqlBuilder()).ToList();

            foreach (var track in tracks)
            {
                Assert.IsFalse(track.Tracks.IsLoaded);
                Assert.IsNotNull(track.Tracks.Value);
                Assert.IsTrue(track.Tracks.IsLoaded);
            }
        }
    }
}
