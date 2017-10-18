using Moq;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetArtistFixture : CoreTest<ParsingService>
    {
        [Test]
        public void should_use_passed_in_title_when_it_cannot_be_parsed()
        {
            const string title = "30 Rock";

            Subject.GetArtist(title);

            Mocker.GetMock<IArtistService>()
                  .Verify(s => s.FindByName(title), Times.Once());
        }

        [Test]
        public void should_use_parsed_artist_title()
        {
            const string title = "30 Rock - Get Some [FLAC]";

            Subject.GetArtist(title);

            Mocker.GetMock<IArtistService>()
                  .Verify(s => s.FindByName(Parser.Parser.ParseAlbumTitle(title).ArtistName), Times.Once());
        }
    }
}
