using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests.AlternativeTitleServiceTests
{
    [TestFixture]
    public class AlternativeTitleFixture : CoreTest
    {
        private AlternativeTitle CreateFakeTitle(SourceType source, int votes)
        {
            return Builder<AlternativeTitle>.CreateNew().With(t => t.SourceType = source).With(t => t.Votes = votes)
                .Build();
        }

        [TestCase(SourceType.TMDB, -1, true)]
        [TestCase(SourceType.TMDB, 1000, true)]
        [TestCase(SourceType.Mappings, 0, false)]
        [TestCase(SourceType.Mappings, 4, true)]
        [TestCase(SourceType.Mappings, -1, false)]
        [TestCase(SourceType.Indexer, 0, true)]
        [TestCase(SourceType.User, 0, true)]
        public void should_be_trusted(SourceType source, int votes, bool trusted)
        {
            var fakeTitle = CreateFakeTitle(source, votes);

            fakeTitle.IsTrusted().Should().Be(trusted);
        }
    }
}
