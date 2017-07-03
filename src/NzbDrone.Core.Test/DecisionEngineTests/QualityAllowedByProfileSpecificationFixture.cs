using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class QualityAllowedByProfileSpecificationFixture : CoreTest<QualityAllowedByProfileSpecification>
    {
        private RemoteEpisode remoteEpisode;

        public static object[] AllowedTestCases =
        {
            new object[] { Quality.MP3_192 },
            new object[] { Quality.MP3_256 },
            new object[] { Quality.MP3_512 }
        };

        public static object[] DeniedTestCases =
        {
            new object[] { Quality.MP3_192 },
            new object[] { Quality.MP3_320 },
            new object[] { Quality.MP3_320 }
        };

        [SetUp]
        public void Setup()
        {
            var fakeSeries = Builder<Series>.CreateNew()
                         .With(c => c.Profile = (LazyLoaded<Profile>)new Profile { Cutoff = Quality.MP3_512 })
                         .Build();

            remoteEpisode = new RemoteEpisode
            {
                Series = fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.MP3_192, new Revision(version: 2)) },
            };
        }

        [Test, TestCaseSource(nameof(AllowedTestCases))]
        public void should_allow_if_quality_is_defined_in_profile(Quality qualityType)
        {
            remoteEpisode.ParsedEpisodeInfo.Quality.Quality = qualityType;
            remoteEpisode.Series.Profile.Value.Items = Qualities.QualityFixture.GetDefaultQualities(Quality.MP3_192, Quality.MP3_256, Quality.MP3_512);

            Subject.IsSatisfiedBy(remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test, TestCaseSource(nameof(DeniedTestCases))]
        public void should_not_allow_if_quality_is_not_defined_in_profile(Quality qualityType)
        {
            remoteEpisode.ParsedEpisodeInfo.Quality.Quality = qualityType;
            remoteEpisode.Series.Profile.Value.Items = Qualities.QualityFixture.GetDefaultQualities(Quality.MP3_192, Quality.MP3_256, Quality.MP3_512);

            Subject.IsSatisfiedBy(remoteEpisode, null).Accepted.Should().BeFalse();
        }
    }
}