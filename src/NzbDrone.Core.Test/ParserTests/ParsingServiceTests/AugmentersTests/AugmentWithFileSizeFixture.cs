using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser.Augmenters;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests.AugmentersTests
{
    [TestFixture]
    public class AugmentWithFileSizeFixture : AugmentMovieInfoFixture<AugmentWithFileSize>
    {
        [Test]
        public void should_add_file_size()
        {
            var localMovie = new LocalMovie
            {
                Size = 1500
            };

            var movieInfo = Subject.AugmentMovieInfo(MovieInfo, localMovie);
            movieInfo.ExtraInfo["Size"].ShouldBeEquivalentTo(1500);
        }
    }
}
