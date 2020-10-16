using System.Collections.Generic;
using NUnit.Framework;
using NzbDrone.Core.Parser.Augmenters;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests.AugmentersTests
{
    [TestFixture]
    public abstract class AugmentMovieInfoFixture<TAugmenter> : CoreTest<TAugmenter>
        where TAugmenter : class, IAugmentParsedMovieInfo
    {
        protected ParsedMovieInfo MovieInfo;

        [SetUp]
        public virtual void Setup()
        {
            MovieInfo = new ParsedMovieInfo
            {
                MovieTitles = new List<string> { "A Movie" },
                Year = 1998,
                SimpleReleaseTitle = "A Movie Title 1998 Bluray 1080p",
                Quality = new QualityModel(Quality.Bluray1080p)
            };
        }
    }
}
