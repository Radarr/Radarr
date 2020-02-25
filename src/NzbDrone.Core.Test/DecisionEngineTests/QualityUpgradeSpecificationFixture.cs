using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class QualityUpgradeSpecificationFixture : CoreTest<UpgradableSpecification>
    {
        private static CustomFormat _customFormat1 = new CustomFormat("My Format 1", new ResolutionSpecification { Value = (int)Resolution.R1080p }) { Id = 1 };
        private static CustomFormat _customFormat2 = new CustomFormat("My Format 2", new ResolutionSpecification { Value = (int)Resolution.R480p }) { Id = 2 };

        public static object[] IsUpgradeTestCases =
        {
            // Quality upgrade trumps custom format
            new object[] { Quality.SDTV, 1, new List<CustomFormat>(), Quality.SDTV, 2, new List<CustomFormat>(), true },
            new object[] { Quality.SDTV, 1, new List<CustomFormat> { _customFormat1 }, Quality.SDTV, 2, new List<CustomFormat> { _customFormat1 }, true },
            new object[] { Quality.SDTV, 1, new List<CustomFormat> { _customFormat1 }, Quality.SDTV, 2, new List<CustomFormat> { _customFormat2 }, true },
            new object[] { Quality.SDTV, 1, new List<CustomFormat> { _customFormat2 }, Quality.SDTV, 2, new List<CustomFormat> { _customFormat1 }, true },

            // Revision upgrade trumps custom format
            new object[] { Quality.WEBDL720p, 1, new List<CustomFormat>(), Quality.WEBDL720p, 2, new List<CustomFormat>(), true },
            new object[] { Quality.WEBDL720p, 1, new List<CustomFormat> { _customFormat1 }, Quality.WEBDL720p, 2, new List<CustomFormat> { _customFormat1 }, true },
            new object[] { Quality.WEBDL720p, 1, new List<CustomFormat> { _customFormat1 }, Quality.WEBDL720p, 2, new List<CustomFormat> { _customFormat2 }, true },
            new object[] { Quality.WEBDL720p, 1, new List<CustomFormat> { _customFormat2 }, Quality.WEBDL720p, 2, new List<CustomFormat> { _customFormat1 }, true },

            // Custom formats apply if quality same
            new object[] { Quality.SDTV, 1, new List<CustomFormat>(), Quality.SDTV, 1, new List<CustomFormat>(), false },
            new object[] { Quality.SDTV, 1, new List<CustomFormat> { _customFormat1 }, Quality.SDTV, 1, new List<CustomFormat> { _customFormat1 }, false },
            new object[] { Quality.SDTV, 1, new List<CustomFormat> { _customFormat1 }, Quality.SDTV, 1, new List<CustomFormat> { _customFormat2 }, true },
            new object[] { Quality.SDTV, 1, new List<CustomFormat> { _customFormat2 }, Quality.SDTV, 1, new List<CustomFormat> { _customFormat1 }, false },

            new object[] { Quality.WEBDL720p, 1, new List<CustomFormat>(), Quality.HDTV720p, 2, new List<CustomFormat>(), false },
            new object[] { Quality.WEBDL720p, 1, new List<CustomFormat>(), Quality.HDTV720p, 2, new List<CustomFormat>(), false },
            new object[] { Quality.WEBDL720p, 1, new List<CustomFormat>(), Quality.WEBDL720p, 1, new List<CustomFormat>(), false },
            new object[] { Quality.WEBDL1080p, 1, new List<CustomFormat>(), Quality.WEBDL1080p, 1, new List<CustomFormat>(), false }
        };

        [SetUp]
        public void Setup()
        {
            CustomFormatsFixture.GivenCustomFormats(_customFormat1, _customFormat2);
        }

        private void GivenAutoDownloadPropers(bool autoDownloadPropers)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoDownloadPropers)
                  .Returns(autoDownloadPropers);
        }

        [Test]
        [TestCaseSource("IsUpgradeTestCases")]
        public void IsUpgradeTest(Quality current,
                                  int currentVersion,
                                  List<CustomFormat> currentFormats,
                                  Quality newQuality,
                                  int newVersion,
                                  List<CustomFormat> newFormats,
                                  bool expected)
        {
            GivenAutoDownloadPropers(true);

            var profile = new Profile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                FormatItems = CustomFormatsFixture.GetSampleFormatItems(_customFormat1.Name, _customFormat2.Name),
                MinFormatScore = 0
            };

            Subject.IsUpgradable(profile,
                                 new QualityModel(current, new Revision(version: currentVersion)),
                                 currentFormats,
                                 new QualityModel(newQuality, new Revision(version: newVersion)),
                                 newFormats)
                    .Should().Be(expected);
        }

        [Test]
        public void should_return_false_if_proper_and_autoDownloadPropers_is_false()
        {
            GivenAutoDownloadPropers(false);

            var profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() };

            Subject.IsUpgradable(profile,
                                 new QualityModel(Quality.DVD, new Revision(version: 2)),
                                 new List<CustomFormat>(),
                                 new QualityModel(Quality.DVD, new Revision(version: 1)),
                                 new List<CustomFormat>())
                    .Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_release_and_existing_file_are_the_same()
        {
            var profile = new Profile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                       new List<CustomFormat>(),
                       new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                       new List<CustomFormat>())
                   .Should().BeFalse();
        }
    }
}
