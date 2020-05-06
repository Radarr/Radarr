using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class QualityAllowedByProfileSpecificationFixture : CoreTest<QualityAllowedByProfileSpecification>
    {
        private RemoteAlbum _remoteAlbum;

        public static object[] AllowedTestCases =
        {
            new object[] { Quality.MP3_320 },
            new object[] { Quality.MP3_320 },
            new object[] { Quality.MP3_320 }
        };

        public static object[] DeniedTestCases =
        {
            new object[] { Quality.FLAC },
            new object[] { Quality.Unknown }
        };

        [SetUp]
        public void Setup()
        {
            var fakeArtist = Builder<Author>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.MP3_320.Id })
                         .Build();

            _remoteAlbum = new RemoteAlbum
            {
                Artist = fakeArtist,
                ParsedAlbumInfo = new ParsedAlbumInfo { Quality = new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
            };
        }

        [Test]
        [TestCaseSource(nameof(AllowedTestCases))]
        public void should_allow_if_quality_is_defined_in_profile(Quality qualityType)
        {
            _remoteAlbum.ParsedAlbumInfo.Quality.Quality = qualityType;
            _remoteAlbum.Artist.QualityProfile.Value.Items = Qualities.QualityFixture.GetDefaultQualities(Quality.MP3_320, Quality.MP3_320, Quality.MP3_320);

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        [TestCaseSource(nameof(DeniedTestCases))]
        public void should_not_allow_if_quality_is_not_defined_in_profile(Quality qualityType)
        {
            _remoteAlbum.ParsedAlbumInfo.Quality.Quality = qualityType;
            _remoteAlbum.Artist.QualityProfile.Value.Items = Qualities.QualityFixture.GetDefaultQualities(Quality.MP3_320, Quality.MP3_320, Quality.MP3_320);

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }
    }
}
