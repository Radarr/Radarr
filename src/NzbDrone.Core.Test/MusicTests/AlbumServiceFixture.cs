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
using NLog;
using Moq;

namespace NzbDrone.Core.Test.MusicTests.AlbumRepositoryTests
{
    [TestFixture]
    public class AlbumServiceFixture : CoreTest<AlbumService>
    {
        private List<Album> _albums;

        [SetUp]
        public void Setup()
        {
            _albums = new List<Album>();
            _albums.Add(new Album
                {
                    Title = "ANThology",
                    CleanTitle = "anthology",
                });

            _albums.Add(new Album
                {
                    Title = "+",
                    CleanTitle = "",
                });

            Mocker.GetMock<IAlbumRepository>()
                .Setup(s => s.GetAlbumsByArtistMetadataId(It.IsAny<int>()))
                .Returns(_albums);
        }

        private void GivenSimilarAlbum()
        {
            _albums.Add(new Album
                {
                    Title = "ANThology2",
                    CleanTitle = "anthology2",
                });
        }

        [TestCase("ANTholog", "ANThology")]
        [TestCase("antholoyg", "ANThology")]
        [TestCase("ANThology CD", "ANThology")]
        [TestCase("ANThology CD xxxx (Remastered) - [Oh please why do they do this?]", "ANThology")]
        [TestCase("+ (Plus) - I feel the need for redundant information in the title field", "+")]
        public void should_find_album_in_db_by_inexact_title(string title, string expected)
        {
            var album = Subject.FindByTitleInexact(0, title);

            album.Should().NotBeNull();
            album.Title.Should().Be(expected);
        }

        [TestCase("ANTholog")]
        [TestCase("antholoyg")]
        [TestCase("ANThology CD")]
        [TestCase("÷")]
        [TestCase("÷ (Divide)")]
        public void should_not_find_album_in_db_by_inexact_title_when_two_similar_matches(string title)
        {
            GivenSimilarAlbum();
            var album = Subject.FindByTitleInexact(0, title);

            album.Should().BeNull();
        }
    }
}
