using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.MusicTests.AlbumRepositoryTests
{
    [TestFixture]
    public class AlbumRepositoryFixture : DbTest<AlbumService, Album>
    {
        private Artist _artist;
        private Album _album;
        private Album _albumSpecial;
        private Album _albumSimilar;
        private AlbumRelease _release;
        private AlbumRepository _albumRepo;
        private ReleaseRepository _releaseRepo;

        [SetUp]
        public void Setup()
        {
            _artist = new Artist
            {
                Name = "Alien Ant Farm",
                Monitored = true,
                ForeignArtistId = "this is a fake id",
                Id = 1,
                Metadata = new ArtistMetadata {
                    Id = 1
                }
            };

            _albumRepo = Mocker.Resolve<AlbumRepository>();
            _releaseRepo = Mocker.Resolve<ReleaseRepository>();

            _release = Builder<AlbumRelease>
                .CreateNew()
                .With(e => e.Id = 0)
                .With(e => e.ForeignReleaseId = "e00e40a3-5ed5-4ed3-9c22-0a8ff4119bdf" )
                .With(e => e.Monitored = true)
                .Build();

            _album = new Album
            {
                Title = "ANThology",
                ForeignAlbumId = "1",
                CleanTitle = "anthology",
                Artist = _artist,
                AlbumType = "",
                AlbumReleases = new List<AlbumRelease> {_release },
            };

            _albumRepo.Insert(_album);
            _release.AlbumId = _album.Id;
            _releaseRepo.Insert(_release);
            _albumRepo.Update(_album);

            _albumSpecial = new Album
            {
                Title = "+",
                ForeignAlbumId = "2",
                CleanTitle = "",
                Artist = _artist,
                ArtistId = _artist.ArtistMetadataId,
                AlbumType = "",
                AlbumReleases = new List<AlbumRelease>
                {
                    new AlbumRelease
                    {
                        ForeignReleaseId = "fake id"
                    }
                }
                
            };

            _albumRepo.Insert(_albumSpecial);

            _albumSimilar = new Album
            {
                Title = "ANThology2",
                ForeignAlbumId = "3",
                CleanTitle = "anthology2",
                Artist = _artist,
                ArtistId = _artist.ArtistMetadataId,
                AlbumType = "",
                AlbumReleases = new List<AlbumRelease>
                {
                    new AlbumRelease
                    {
                        ForeignReleaseId = "fake id 2"
                    }
                }
                
            };

        }

        [Test]
        public void should_find_album_in_db_by_releaseid()
        {
            var id = "e00e40a3-5ed5-4ed3-9c22-0a8ff4119bdf";

            var album = _albumRepo.FindAlbumByRelease(id);

            album.Should().NotBeNull();
            album.Title.Should().Be(_album.Title);
        }

        [TestCase("ANThology")]
        [TestCase("anthology")]
        [TestCase("anthology!")]
        public void should_find_album_in_db_by_title(string title)
        {
            var album = _albumRepo.FindByTitle(_artist.ArtistMetadataId, title);

            album.Should().NotBeNull();
            album.Title.Should().Be(_album.Title);
        }

        [Test]
        public void should_find_album_in_db_by_title_all_special_characters()
        {
            var album = _albumRepo.FindByTitle(_artist.ArtistMetadataId, "+");

            album.Should().NotBeNull();
            album.Title.Should().Be(_albumSpecial.Title);
        }

        [TestCase("ANTholog")]
        [TestCase("nthology")]
        [TestCase("antholoyg")]
        [TestCase("÷")]
        public void should_not_find_album_in_db_by_incorrect_title(string title)
        {
            var album = _albumRepo.FindByTitle(_artist.ArtistMetadataId, title);

            album.Should().BeNull();
        }

        [Test]
        public void should_not_find_album_in_db_by_partial_releaseid()
        {
            var id = "e00e40a3-5ed5-4ed3-9c22";

            var album = _albumRepo.FindAlbumByRelease(id);

            album.Should().BeNull();
        }
    }
}
