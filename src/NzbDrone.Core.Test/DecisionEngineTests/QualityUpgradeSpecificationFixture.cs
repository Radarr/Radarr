using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    
    public class QualityUpgradeSpecificationFixture : CoreTest<QualityUpgradableSpecification>
    {
        public static object[] IsUpgradeTestCases =
        {
            new object[] { Quality.MP3192, 1, Quality.MP3192, 2, Quality.MP3192, true },
            new object[] { Quality.MP3320, 1, Quality.MP3320, 2, Quality.MP3320, true },
            new object[] { Quality.MP3192, 1, Quality.MP3192, 1, Quality.MP3192, false },
            new object[] { Quality.MP3320, 1, Quality.MP3256, 2, Quality.MP3320, false },
            new object[] { Quality.MP3320, 1, Quality.MP3256, 2, Quality.MP3320, false },
            new object[] { Quality.MP3320, 1, Quality.MP3320, 1, Quality.MP3320, false },
            new object[] { Quality.MP3512, 1, Quality.MP3512, 1, Quality.MP3512, false }
        };
        
        [SetUp]
        public void Setup()
        {

        }

        private void GivenAutoDownloadPropers(bool autoDownloadPropers)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoDownloadPropers)
                  .Returns(autoDownloadPropers);
        }

        [Test, TestCaseSource(nameof(IsUpgradeTestCases))]
        public void IsUpgradeTest(Quality current, int currentVersion, Quality newQuality, int newVersion, Quality cutoff, bool expected)
        {
            GivenAutoDownloadPropers(true);

            var profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() };

            Subject.IsUpgradable(profile, new QualityModel(current, new Revision(version: currentVersion)), new QualityModel(newQuality, new Revision(version: newVersion)))
                    .Should().Be(expected);
        }

        [Test]
        public void should_return_false_if_proper_and_autoDownloadPropers_is_false()
        {
            GivenAutoDownloadPropers(false);

            var profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() };

            Subject.IsUpgradable(profile, new QualityModel(Quality.MP3192, new Revision(version: 2)), new QualityModel(Quality.MP3192, new Revision(version: 1)))
                    .Should().BeFalse();
        }
    }
}