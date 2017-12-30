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

            var artist = Builder<Artist>.CreateListOfSize(1)
                .All()
                .With(v => v.ProfileId = profile.Id)
                .With(v => v.LanguageProfileId = languageProfile.Id)
                .BuildListOfNew();

            Db.InsertMany(artist);

            var albums = Builder<Album>.CreateListOfSize(3)
                .All()
                .With(v => v.ArtistId = artist[0].Id)
                .BuildListOfNew();

            Db.InsertMany(albums);

            var trackFiles = Builder<TrackFile>.CreateListOfSize(1)
                .All()
                .With(v => v.ArtistId = artist[0].Id)
                .With(v => v.Quality = new QualityModel())
                .BuildListOfNew();

            Db.InsertMany(trackFiles);

            var tracks = Builder<Track>.CreateListOfSize(10)
                .All()
                .With(v => v.Monitored = true)
                .With(v => v.TrackFileId = trackFiles[0].Id)
                .With(v => v.ArtistId = artist[0].Id)
                .BuildListOfNew();

            Db.InsertMany(tracks);
        }

        [Test]
        [Ignore("This does not currently join correctly, however we are not using the joined info")]
        public void should_join_artist_when_query_for_albums()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var albums = DataMapper.Query<Album>()
                .Join<Album, Artist>(Marr.Data.QGen.JoinType.Inner, v => v.Artist, (l, r) => l.ArtistId == r.Id)
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
                .Join<Track, Artist>(Marr.Data.QGen.JoinType.Inner, v => v.Artist, (l, r) => l.ArtistId == r.Id)
                .ToList();

            foreach (var track in tracks)
            {
                Assert.IsNotNull(track.Artist);
                Assert.IsFalse(track.Artist.Profile.IsLoaded);
                Assert.IsFalse(track.Artist.LanguageProfile.IsLoaded);
            }
        }

        [Test]
        public void should_explicit_load_trackfile_if_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var tracks = DataMapper.Query<Track>()
                .Join<Track, TrackFile>(Marr.Data.QGen.JoinType.Inner, v => v.TrackFile, (l, r) => l.TrackFileId == r.Id)
                .ToList();

            foreach (var track in tracks)
            {
                Assert.IsNull(track.Artist);
                Assert.IsTrue(track.TrackFile.IsLoaded);
            }
        }

        [Test]
        public void should_explicit_load_profile_if_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var tracks = DataMapper.Query<Track>()
                .Join<Track, Artist>(Marr.Data.QGen.JoinType.Inner, v => v.Artist, (l, r) => l.ArtistId == r.Id)
                .Join<Artist, Profile>(Marr.Data.QGen.JoinType.Inner, v => v.Profile, (l, r) => l.ProfileId == r.Id)
                .ToList();

            foreach (var track in tracks)
            {
                Assert.IsNotNull(track.Artist);
                Assert.IsTrue(track.Artist.Profile.IsLoaded);
                Assert.IsFalse(track.Artist.LanguageProfile.IsLoaded);
            }
        }

        [Test]
        public void should_explicit_load_languageprofile_if_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var tracks = DataMapper.Query<Track>()
                                     .Join<Track, Artist>(Marr.Data.QGen.JoinType.Inner, v => v.Artist, (l, r) => l.ArtistId == r.Id)
                                     .Join<Artist, LanguageProfile>(Marr.Data.QGen.JoinType.Inner, v => v.LanguageProfile, (l, r) => l.ProfileId == r.Id)
                                     .ToList();

            foreach (var track in tracks)
            {
                Assert.IsNotNull(track.Artist);
                Assert.IsFalse(track.Artist.Profile.IsLoaded);
                Assert.IsTrue(track.Artist.LanguageProfile.IsLoaded);
            }
        }

    }
}
