using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MusicTests.ArtistServiceTests
{
    [TestFixture]

    public class FindByNameInexactFixture : CoreTest<AuthorService>
    {
        private List<Author> _artists;

        private Author CreateArtist(string name)
        {
            return Builder<Author>.CreateNew()
                .With(a => a.Name = name)
                .With(a => a.CleanName = Parser.Parser.CleanAuthorName(name))
                .With(a => a.ForeignAuthorId = name)
                .BuildNew();
        }

        [SetUp]
        public void Setup()
        {
            _artists = new List<Author>();
            _artists.Add(CreateArtist("The Black Eyed Peas"));
            _artists.Add(CreateArtist("The Black Keys"));

            Mocker.GetMock<IAuthorRepository>()
                .Setup(s => s.All())
                .Returns(_artists);
        }

        [TestCase("The Black Eyde Peas", "The Black Eyed Peas")]
        [TestCase("Black Eyed Peas", "The Black Eyed Peas")]
        [TestCase("The Black eys", "The Black Keys")]
        [TestCase("Black Keys", "The Black Keys")]
        public void should_find_artist_in_db_by_name_inexact(string name, string expected)
        {
            var artist = Subject.FindByNameInexact(name);

            artist.Should().NotBeNull();
            artist.Name.Should().Be(expected);
        }

        [Test]
        public void should_find_artist_when_the_is_omitted_from_start()
        {
            _artists = new List<Author>();
            _artists.Add(CreateArtist("Black Keys"));
            _artists.Add(CreateArtist("The Black Eyed Peas"));

            Mocker.GetMock<IAuthorRepository>()
                .Setup(s => s.All())
                .Returns(_artists);

            Subject.FindByNameInexact("The Black Keys").Should().NotBeNull();
        }

        [TestCase("The Black Peas")]
        public void should_not_find_artist_in_db_by_ambiguous_name(string name)
        {
            var artist = Subject.FindByNameInexact(name);

            artist.Should().BeNull();
        }
    }
}
