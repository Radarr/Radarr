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

            var metadata = Builder<AuthorMetadata>.CreateNew()
                .With(v => v.Id = 0)
                .Build();
            Db.Insert(metadata);

            var artist = Builder<Author>.CreateListOfSize(1)
                .All()
                .With(v => v.Id = 0)
                .With(v => v.QualityProfileId = profile.Id)
                .With(v => v.AuthorMetadataId = metadata.Id)
                .BuildListOfNew();

            Db.InsertMany(artist);

            var albums = Builder<Book>.CreateListOfSize(3)
                .All()
                .With(v => v.Id = 0)
                .With(v => v.AuthorMetadataId = metadata.Id)
                .BuildListOfNew();

            Db.InsertMany(albums);

            var trackFiles = Builder<BookFile>.CreateListOfSize(1)
                .All()
                .With(v => v.Id = 0)
                .With(v => v.BookId = albums[0].Id)
                .With(v => v.Quality = new QualityModel())
                .BuildListOfNew();

            Db.InsertMany(trackFiles);
        }

        [Test]
        public void should_lazy_load_artist_for_trackfile()
        {
            var db = Mocker.Resolve<IDatabase>();
            var tracks = db.Query<BookFile>(new SqlBuilder()).ToList();

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
            var tracks = db.Query<Book>(new SqlBuilder()).ToList();

            foreach (var track in tracks)
            {
                Assert.IsFalse(track.BookFiles.IsLoaded);
                Assert.IsNotNull(track.BookFiles.Value);
                Assert.IsTrue(track.BookFiles.IsLoaded);
            }
        }

        [Test]
        public void should_explicit_load_everything_if_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var files = MediaFileRepository.Query(db,
                                                  new SqlBuilder()
                                                  .Join<BookFile, Book>((t, a) => t.BookId == a.Id)
                                                  .Join<Book, Author>((album, artist) => album.AuthorMetadataId == artist.AuthorMetadataId)
                                                  .Join<Author, AuthorMetadata>((a, m) => a.AuthorMetadataId == m.Id));

            Assert.IsNotEmpty(files);
            foreach (var file in files)
            {
                Assert.IsTrue(file.Album.IsLoaded);
                Assert.IsTrue(file.Artist.IsLoaded);
                Assert.IsTrue(file.Artist.Value.Metadata.IsLoaded);
            }
        }

        [Test]
        public void should_lazy_load_tracks_if_not_joined_to_trackfile()
        {
            var db = Mocker.Resolve<IDatabase>();
            var files = db.QueryJoined<BookFile, Book, Author, AuthorMetadata>(
                new SqlBuilder()
                .Join<BookFile, Book>((t, a) => t.BookId == a.Id)
                .Join<Book, Author>((album, artist) => album.AuthorMetadataId == artist.AuthorMetadataId)
                .Join<Author, AuthorMetadata>((a, m) => a.AuthorMetadataId == m.Id),
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
                Assert.IsTrue(file.Album.IsLoaded);
                Assert.IsTrue(file.Artist.IsLoaded);
                Assert.IsTrue(file.Artist.Value.Metadata.IsLoaded);
            }
        }
    }
}
