using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Test.MusicTests.AlbumRepositoryTests
{
    [TestFixture]
    public class AlbumRepositoryFixture : DbTest<AlbumService, Album>
    {
        private Artist _artist;
        private Album _album;
        private Album _albumSpecial;
        private List<Album> _albums;
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
                ArtistMetadataId = _artist.ArtistMetadataId,
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
                ArtistMetadataId = _artist.ArtistMetadataId,
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
        public void should_not_find_album_when_two_albums_have_same_name()
        {
            var albums = Builder<Album>.CreateListOfSize(2)
                .All()
                .With(x => x.Id = 0)
                .With(x => x.Artist = _artist)
                .With(x => x.ArtistMetadataId = _artist.ArtistMetadataId)
                .With(x => x.Title = "Weezer")
                .With(x => x.CleanTitle = "weezer")
                .Build();
            
            _albumRepo.InsertMany(albums);
            
            var album = _albumRepo.FindByTitle(_artist.ArtistMetadataId, "Weezer");
            
            _albumRepo.All().Should().HaveCount(4);
            album.Should().BeNull();
        }
        
        [Test]
        public void should_not_find_album_in_db_by_partial_releaseid()
        {
            var id = "e00e40a3-5ed5-4ed3-9c22";

            var album = _albumRepo.FindAlbumByRelease(id);

            album.Should().BeNull();
        }

        private void GivenMultipleAlbums()
        {
            _albums = Builder<Album>.CreateListOfSize(4)
                .All()
                .With(x => x.Id = 0)
                .With(x => x.Artist = _artist)
                .With(x => x.ArtistMetadataId = _artist.ArtistMetadataId)
                .TheFirst(1)
                // next
                .With(x => x.ReleaseDate = DateTime.UtcNow.AddDays(1))
                .TheNext(1)
                // another future one
                .With(x => x.ReleaseDate = DateTime.UtcNow.AddDays(2))
                .TheNext(1)
                // most recent
                .With(x => x.ReleaseDate = DateTime.UtcNow.AddDays(-1))
                .TheNext(1)
                // an older one
                .With(x => x.ReleaseDate = DateTime.UtcNow.AddDays(-2))
                .BuildList();

            _albumRepo.InsertMany(_albums);
        }

        [Test]
        public void get_next_albums_should_return_next_album()
        {
            GivenMultipleAlbums();

            var result = _albumRepo.GetNextAlbums(new [] { _artist.ArtistMetadataId });
            result.Should().BeEquivalentTo(_albums.Take(1));
        }

        [Test]
        public void get_last_albums_should_return_next_album()
        {
            GivenMultipleAlbums();

            var result = _albumRepo.GetLastAlbums(new [] { _artist.ArtistMetadataId });
            result.Should().BeEquivalentTo(_albums.Skip(2).Take(1));
        }
    }
}
