using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Augmenters;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests.AugmentersTests
{
    [TestFixture]
    public class AugmentWithOriginalLanguageFixture : AugmentMovieInfoFixture<AugmentWithOriginalLanguage>
    {
        [Test]
        public void should_add_movie_original_language()
        {
            var releaseInfo = new ParsedMovieInfo();
            var movie = new Movies.Movie
            {
                MovieMetadata = new Movies.MovieMetadata
                {
                    OriginalLanguage = Language.English
                }
            };
            var result = Subject.AugmentMovieInfo(releaseInfo, movie);
            result.ExtraInfo.Should().ContainKey("OriginalLanguage");
            result.ExtraInfo["OriginalLanguage"].Should().Be(Language.English);
        }
    }
}
