using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers.Rarbg;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Augmenters;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests.AugmentersTests
{
    [TestFixture]
    public class AugmentWithReleaseInfoFixture : AugmentMovieInfoFixture<AugmentWithReleaseInfo>
    {
        private ReleaseInfo ReleaseInfoWithLanguages(params Language[] languages)
        {
            return new ReleaseInfo
            {
                IndexerSettings = new RarbgSettings
                {
                    MultiLanguages = languages.ToList().Select(l => (int) l)
                }
            };
        }

        [Test]
        public void should_add_language_from_indexer()
        {
            var releaseInfo = ReleaseInfoWithLanguages(Language.English, Language.French);
            MovieInfo.SimpleReleaseTitle = "A Movie Title 1998 Bluray 1080p MULTI";
            var movieInfo = Subject.AugmentMovieInfo(MovieInfo, releaseInfo);
            movieInfo.Languages.Count.Should().Be(2);
            movieInfo.Languages.Should().BeEquivalentTo(Language.English, Language.French);
        }

        [Test]
        public void should_add_size_info()
        {
            var releaseInfo = new ReleaseInfo
            {
                Size = 1500
            };

            var movieInfo = Subject.AugmentMovieInfo(MovieInfo, releaseInfo);
            movieInfo.ExtraInfo["Size"].ShouldBeEquivalentTo(1500);
        }

        [Test]
        public void should_not_add_size_when_already_present()
        {
            var releaseInfo = new ReleaseInfo
            {
                Size = 1500
            };

            MovieInfo.ExtraInfo["Size"] = 1600;

            var movieInfo = Subject.AugmentMovieInfo(MovieInfo, releaseInfo);
            movieInfo.ExtraInfo["Size"].ShouldBeEquivalentTo(1600);
        }

        [Test]
        public void should_add_indexer_flags()
        {
            var releaseInfo = new ReleaseInfo
            {
                IndexerFlags = IndexerFlags.PTP_Approved | IndexerFlags.PTP_Golden
            };

            var movieInfo = Subject.AugmentMovieInfo(MovieInfo, releaseInfo);
            movieInfo.ExtraInfo["IndexerFlags"].ShouldBeEquivalentTo(IndexerFlags.PTP_Approved | IndexerFlags.PTP_Golden);
        }

    }
}
