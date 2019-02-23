using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class QualityAllowedByProfileSpecificationFixture : CoreTest<QualityAllowedByProfileSpecification>
    {
        private RemoteAlbum remoteAlbum;

        public static object[] AllowedTestCases =
        {
            new object[] { Quality.MP3_192 },
            new object[] { Quality.MP3_256 },
            new object[] { Quality.MP3_320 }
        };

        public static object[] DeniedTestCases =
        {
            new object[] { Quality.MP3_VBR },
            new object[] { Quality.FLAC },
            new object[] { Quality.Unknown }
        };

        [SetUp]
        public void Setup()
        {
            var fakeArtist = Builder<Artist>.CreateNew()
                         .With(c => c.QualityProfile = (LazyLoaded<QualityProfile>)new QualityProfile { Cutoff = Quality.MP3_320.Id })
                         .Build();

            remoteAlbum = new RemoteAlbum
            {
                Artist = fakeArtist,
                ParsedAlbumInfo = new ParsedAlbumInfo { Quality = new QualityModel(Quality.MP3_192, new Revision(version: 2)) },
            };
        }

        [Test, TestCaseSource(nameof(AllowedTestCases))]
        public void should_allow_if_quality_is_defined_in_profile(Quality qualityType)
        {
            remoteAlbum.ParsedAlbumInfo.Quality.Quality = qualityType;
            remoteAlbum.Artist.QualityProfile.Value.Items = Qualities.QualityFixture.GetDefaultQualities(Quality.MP3_192, Quality.MP3_256, Quality.MP3_320);

            Subject.IsSatisfiedBy(remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test, TestCaseSource(nameof(DeniedTestCases))]
        public void should_not_allow_if_quality_is_not_defined_in_profile(Quality qualityType)
        {
            remoteAlbum.ParsedAlbumInfo.Quality.Quality = qualityType;
            remoteAlbum.Artist.QualityProfile.Value.Items = Qualities.QualityFixture.GetDefaultQualities(Quality.MP3_192, Quality.MP3_256, Quality.MP3_320);

            Subject.IsSatisfiedBy(remoteAlbum, null).Accepted.Should().BeFalse();
        }
    }
}
