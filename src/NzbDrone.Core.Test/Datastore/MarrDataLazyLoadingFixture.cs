using FizzWare.NBuilder;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles;
using Marr.Data.QGen;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Test.Datastore
{

    [TestFixture]
    public class MarrDataLazyLoadingFixture : DbTest
    {
        [SetUp]
        public void Setup()
        {
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
        public void should_join_artist_when_query_for_albums()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var albums = DataMapper.Query<Album>()
                .Join<Album, Artist>(JoinType.Inner, v => v.Artist, (l, r) => l.ArtistMetadataId == r.ArtistMetadataId)
                .ToList();

            foreach (var album in albums)
            {
                Assert.IsNotNull(album.Artist);
            }
        }

        [Test]
        public void should_lazy_load_profile_if_not_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var tracks = DataMapper.Query<Track>()
                .Join<Track, AlbumRelease>(JoinType.Inner, v => v.AlbumRelease, (l, r) => l.AlbumReleaseId == r.Id)
                .Join<AlbumRelease, Album>(JoinType.Inner, v => v.Album, (l, r) => l.AlbumId == r.Id)
                .Join<Album, Artist>(JoinType.Inner, v => v.Artist, (l, r) => l.ArtistMetadataId == r.ArtistMetadataId)
                .ToList();

            foreach (var track in tracks)
            {
                Assert.IsTrue(track.AlbumRelease.IsLoaded);
                Assert.IsTrue(track.AlbumRelease.Value.Album.IsLoaded);
                Assert.IsTrue(track.AlbumRelease.Value.Album.Value.Artist.IsLoaded);
                Assert.IsNotNull(track.AlbumRelease.Value.Album.Value.Artist.Value);
                Assert.IsFalse(track.AlbumRelease.Value.Album.Value.Artist.Value.QualityProfile.IsLoaded);
            }
        }

        [Test]
        public void should_explicit_load_trackfile_if_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var tracks = DataMapper.Query<Track>()
                .Join<Track, TrackFile>(JoinType.Inner, v => v.TrackFile, (l, r) => l.TrackFileId == r.Id)
                .ToList();

            foreach (var track in tracks)
            {
                Assert.IsFalse(track.Artist.IsLoaded);
                Assert.IsTrue(track.TrackFile.IsLoaded);
            }
        }

        [Test]
        public void should_lazy_load_artist_for_track()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var tracks = DataMapper.Query<Track>()
                .ToList();

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
            var DataMapper = db.GetDataMapper();

            var tracks = DataMapper.Query<TrackFile>()
                .ToList();

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
            var DataMapper = db.GetDataMapper();

            var tracks = DataMapper.Query<Track>()
                .ToList();

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
            var DataMapper = db.GetDataMapper();

            var files = DataMapper.Query<TrackFile>()
                .Join<TrackFile, Track>(JoinType.Inner, f => f.Tracks, (f, t) => f.Id == t.TrackFileId)
                .Join<TrackFile, Album>(JoinType.Inner, t => t.Album, (t, a) => t.AlbumId == a.Id)
                .Join<TrackFile, Artist>(JoinType.Inner, t => t.Artist, (t, a) => t.Album.Value.ArtistMetadataId == a.ArtistMetadataId)
                .Join<Artist, ArtistMetadata>(JoinType.Inner, a => a.Metadata, (a, m) => a.ArtistMetadataId == m.Id)
                .ToList();

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
            var DataMapper = db.GetDataMapper();

            var files = DataMapper.Query<TrackFile>()
                .Join<TrackFile, Album>(JoinType.Inner, t => t.Album, (t, a) => t.AlbumId == a.Id)
                .Join<TrackFile, Artist>(JoinType.Inner, t => t.Artist, (t, a) => t.Album.Value.ArtistMetadataId == a.ArtistMetadataId)
                .Join<Artist, ArtistMetadata>(JoinType.Inner, a => a.Metadata, (a, m) => a.ArtistMetadataId == m.Id)
                .ToList();

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
            var DataMapper = db.GetDataMapper();

            var release = DataMapper.Query<AlbumRelease>().Where(x => x.Id == 1).SingleOrDefault();

            Assert.IsFalse(release.Tracks.IsLoaded);
            Assert.IsNotNull(release.Tracks.Value);
            Assert.IsNotEmpty(release.Tracks.Value);
            Assert.IsTrue(release.Tracks.IsLoaded);
        }

        [Test]
        public void should_lazy_load_track_if_not_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var tracks = DataMapper.Query<TrackFile>()
                .ToList();

            foreach (var track in tracks)
            {
                Assert.IsFalse(track.Tracks.IsLoaded);
                Assert.IsNotNull(track.Tracks.Value);
                Assert.IsTrue(track.Tracks.IsLoaded);
            }
        }

        [Test]
        public void should_explicit_load_profile_if_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var tracks = DataMapper.Query<Track>()
                .Join<Track, AlbumRelease>(JoinType.Inner, v => v.AlbumRelease, (l, r) => l.AlbumReleaseId == r.Id)
                .Join<AlbumRelease, Album>(JoinType.Inner, v => v.Album, (l, r) => l.AlbumId == r.Id)
                .Join<Album, Artist>(JoinType.Inner, v => v.Artist, (l, r) => l.ArtistMetadataId == r.ArtistMetadataId)
                .Join<Artist, QualityProfile>(JoinType.Inner, v => v.QualityProfile, (l, r) => l.QualityProfileId == r.Id)
                .ToList();

            foreach (var track in tracks)
            {
                Assert.IsTrue(track.AlbumRelease.IsLoaded);
                Assert.IsTrue(track.AlbumRelease.Value.Album.IsLoaded);
                Assert.IsTrue(track.AlbumRelease.Value.Album.Value.Artist.IsLoaded);
                Assert.IsNotNull(track.AlbumRelease.Value.Album.Value.Artist.Value);
                Assert.IsTrue(track.AlbumRelease.Value.Album.Value.Artist.Value.QualityProfile.IsLoaded);
            }
        }
    }
}
