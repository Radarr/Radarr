using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NzbDrone.Core.Test.MusicTests.AlbumRepositoryTests
{
    [TestFixture]
    public class AlbumRepositoryFixture : DbTest<AlbumService, Album>
    {
        private Artist _artist;
        private Album _album;
        private AlbumRepository _albumRepo;

        [SetUp]
        public void Setup()
        {
            _artist = new Artist
            {
                Name = "Alien Ant Farm",
                Monitored = true,
                MBId = "this is a fake id",
                Id = 1
            };

            _album = new Album
            {
                Title = "ANThology",
                ForeignAlbumId = "1",
                CleanTitle = "anthology",
                Artist = _artist,
                AlbumType = "",
                Releases = new List<AlbumRelease>
                {
                    new AlbumRelease
                    {
                        Id = "e00e40a3-5ed5-4ed3-9c22-0a8ff4119bdf"
                    }
                }
                
            };

            _albumRepo = Mocker.Resolve<AlbumRepository>();

            _albumRepo.Insert(_album);
        }


        [Test]
        public void should_find_album_in_db_by_releaseid()
        {
            var id = "e00e40a3-5ed5-4ed3-9c22-0a8ff4119bdf";

            var album = _albumRepo.FindAlbumByRelease(id);

            album.Title.Should().Be(_album.Title);
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
