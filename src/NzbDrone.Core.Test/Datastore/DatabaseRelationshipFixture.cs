using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using System;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class DatabaseRelationshipFixture : DbTest
    {
        [Test]
        public void one_to_one()
        {
            var album = Builder<Album>.CreateNew()
                .With(c => c.Id = 0)
                .With(x => x.ReleaseDate = DateTime.UtcNow)
                .With(x => x.LastInfoSync = DateTime.UtcNow)
                .With(x => x.Added = DateTime.UtcNow)
                .BuildNew();
            Db.Insert(album);

            var albumRelease = Builder<AlbumRelease>.CreateNew()
                .With(c => c.Id = 0)
                .With(c => c.AlbumId = album.Id)
                .BuildNew();
            Db.Insert(albumRelease);

            var loadedAlbum = Db.Single<AlbumRelease>().Album.Value;

            loadedAlbum.Should().NotBeNull();
            loadedAlbum.ShouldBeEquivalentTo(album,
                                             options => options
                                             .IncludingAllRuntimeProperties()
                                             .Excluding(c => c.Artist)
                                             .Excluding(c => c.ArtistId)
                                             .Excluding(c => c.ArtistMetadata)
                                             .Excluding(c => c.AlbumReleases));
        }

        [Test]
        public void one_to_one_should_not_query_db_if_foreign_key_is_zero()
        {
            var track = Builder<Track>.CreateNew()
                .With(c => c.TrackFileId = 0)
                .BuildNew();

            Db.Insert(track);

            Db.Single<Track>().TrackFile.Value.Should().BeNull();
        }


        [Test]
        public void embedded_document_as_json()
        {
            var quality = new QualityModel { Quality = Quality.MP3_320, Revision = new Revision(version: 2 )};

            var history = Builder<History.History>.CreateNew()
                            .With(c => c.Id = 0)
                            .With(c => c.Quality = quality)
                            .Build();

            Db.Insert(history);

            var loadedQuality = Db.Single<History.History>().Quality;
            loadedQuality.Should().Be(quality);
        }

        [Test]
        public void embedded_list_of_document_with_json()
        {
            var history = Builder<History.History>.CreateListOfSize(2)
                            .All().With(c => c.Id = 0)
                            .Build().ToList();

            history[0].Quality = new QualityModel(Quality.MP3_320, new Revision(version: 2));
            history[1].Quality = new QualityModel(Quality.MP3_256, new Revision(version: 2));


            Db.InsertMany(history);

            var returnedHistory = Db.All<History.History>();

            returnedHistory[0].Quality.Quality.Should().Be(Quality.MP3_320);
        }
    }
}
