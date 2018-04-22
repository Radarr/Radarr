using NUnit.Framework;
using NzbDrone.Core.Parser.Augmenters;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Test.Qualities;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests.AugmentersTests
{
    [TestFixture]
    public abstract class AugmentMovieInfoFixture<TAugmenter> : CoreTest<TAugmenter> where TAugmenter : class, IAugmentParsedMovieInfo
    {
        protected ParsedMovieInfo MovieInfo;
        [SetUp]
        public void Setup()
        {
            QualityDefinitionServiceFixture.SetupDefaultDefinitions();
            MovieInfo = new ParsedMovieInfo
            {
                MovieTitle = "A Movie",
                Year = 1998,
                SimpleReleaseTitle = "A Movie Title 1998 Bluray 1080p",
                Quality = new QualityModel(QualityWrapper.Dynamic.Bluray1080p)
            };
        }
    }
}
