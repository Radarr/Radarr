using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetAlbumsFixture : CoreTest<ParsingService>
    {
        [Test]
        public void should_not_fail_if_search_criteria_contains_multiple_albums_with_the_same_name()
        {
            var artist = Builder<Artist>.CreateNew().Build();
            var albums = Builder<Album>.CreateListOfSize(2).All().With(x => x.Title = "IdenticalTitle").Build().ToList();
            var criteria = new AlbumSearchCriteria
            {
                Artist = artist,
                Albums = albums
            };

            var parsed = new ParsedAlbumInfo
            {
                AlbumTitle = "IdenticalTitle"
            };

            Subject.GetAlbums(parsed, artist, criteria).Should().BeEquivalentTo(new List<Album>());

            Mocker.GetMock<IAlbumService>()
                .Verify(s => s.FindByTitle(artist.ArtistMetadataId, "IdenticalTitle"), Times.Once());
        }
    }
}
