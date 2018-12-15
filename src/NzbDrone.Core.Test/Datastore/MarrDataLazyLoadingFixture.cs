using FizzWare.NBuilder;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Test.Languages;
using Marr.Data.QGen;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.Datastore
{

    [TestFixture]
    public class MarrDataLazyLoadingFixture : DbTest
    {
        [SetUp]
        public void Setup()
        {
            var profile = new Profile
            {
                Name = "Test",
                Cutoff = Quality.MP3_320.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities()
            };

            var languageProfile = new LanguageProfile
            {
                Name = "Test",
                Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                Cutoff = Language.English
            };



            profile = Db.Insert(profile);
            languageProfile = Db.Insert(languageProfile);

            var metadata = Builder<ArtistMetadata>.CreateNew()
                .With(v => v.Id = 0)
                .Build();
            Db.Insert(metadata);

            var artist = Builder<Artist>.CreateListOfSize(1)
                .All()
                .With(v => v.Id = 0)
                .With(v => v.ProfileId = profile.Id)
                .With(v => v.LanguageProfileId = languageProfile.Id)
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
                Assert.IsFalse(track.AlbumRelease.Value.Album.Value.Artist.Value.Profile.IsLoaded);
                Assert.IsFalse(track.AlbumRelease.Value.Album.Value.Artist.Value.LanguageProfile.IsLoaded);
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
        public void should_explicit_load_profile_if_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var tracks = DataMapper.Query<Track>()
                .Join<Track, AlbumRelease>(JoinType.Inner, v => v.AlbumRelease, (l, r) => l.AlbumReleaseId == r.Id)
                .Join<AlbumRelease, Album>(JoinType.Inner, v => v.Album, (l, r) => l.AlbumId == r.Id)
                .Join<Album, Artist>(JoinType.Inner, v => v.Artist, (l, r) => l.ArtistMetadataId == r.ArtistMetadataId)
                .Join<Artist, Profile>(JoinType.Inner, v => v.Profile, (l, r) => l.ProfileId == r.Id)
                .ToList();

            foreach (var track in tracks)
            {
                Assert.IsTrue(track.AlbumRelease.IsLoaded);
                Assert.IsTrue(track.AlbumRelease.Value.Album.IsLoaded);
                Assert.IsTrue(track.AlbumRelease.Value.Album.Value.Artist.IsLoaded);
                Assert.IsNotNull(track.AlbumRelease.Value.Album.Value.Artist.Value);
                Assert.IsTrue(track.AlbumRelease.Value.Album.Value.Artist.Value.Profile.IsLoaded);
                Assert.IsFalse(track.AlbumRelease.Value.Album.Value.Artist.Value.LanguageProfile.IsLoaded);
            }
        }

        [Test]
        public void should_explicit_load_languageprofile_if_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var tracks = DataMapper.Query<Track>()
                .Join<Track, AlbumRelease>(JoinType.Inner, v => v.AlbumRelease, (l, r) => l.AlbumReleaseId == r.Id)
                .Join<AlbumRelease, Album>(JoinType.Inner, v => v.Album, (l, r) => l.AlbumId == r.Id)
                .Join<Album, Artist>(JoinType.Inner, v => v.Artist, (l, r) => l.ArtistMetadataId == r.ArtistMetadataId)
                .Join<Artist, LanguageProfile>(JoinType.Inner, v => v.LanguageProfile, (l, r) => l.LanguageProfileId == r.Id)
                .ToList();

            foreach (var track in tracks)
            {
                Assert.IsTrue(track.AlbumRelease.IsLoaded);
                Assert.IsTrue(track.AlbumRelease.Value.Album.IsLoaded);
                Assert.IsTrue(track.AlbumRelease.Value.Album.Value.Artist.IsLoaded);
                Assert.IsNotNull(track.AlbumRelease.Value.Album.Value.Artist.Value);
                Assert.IsFalse(track.AlbumRelease.Value.Album.Value.Artist.Value.Profile.IsLoaded);
                Assert.IsTrue(track.AlbumRelease.Value.Album.Value.Artist.Value.LanguageProfile.IsLoaded);
            }
        }

    }
}
