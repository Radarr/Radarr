using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.CustomFormats.Specifications
{
    [TestFixture]
    public class FullReleaseTitleSpecificationFixture : CoreTest<FullReleaseTitleSpecification>
    {
        private CustomFormatInput _input;

        [SetUp]
        public void Setup()
        {
            _input = new CustomFormatInput
            {
                MovieInfo = new ParsedMovieInfo
                {
                    ReleaseTitle = "Example.Feature.2026.1080p.WEB-DL",
                    SimpleReleaseTitle = "A.Movie.2026.1080p.WEB-DL"
                }
            };
        }

        [Test]
        public void should_match_the_movie_title()
        {
            Subject.Value = @"example[ ._-]+feature";

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void should_not_change_the_existing_release_title_matching_behavior()
        {
            var specification = new ReleaseTitleSpecification
            {
                Value = @"example[ ._-]+feature"
            };

            specification.IsSatisfiedBy(_input).Should().BeFalse();
        }

        [Test]
        public void should_match_the_filename_when_the_full_release_title_is_not_available()
        {
            _input.MovieInfo.ReleaseTitle = null;
            _input.Filename = "Example.Feature.2026.1080p.WEB-DL.mkv";
            Subject.Value = @"example[ ._-]+feature";

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }
    }
}
