using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Test.Languages;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class UpgradeSpecificationFixture : CoreTest<UpgradableSpecification>
    {
        public static object[] IsUpgradeTestCases =
        {
            new object[] { Quality.MP3_192, 1, Quality.MP3_192, 2, Quality.MP3_192, true },
            new object[] { Quality.MP3_320, 1, Quality.MP3_320, 2, Quality.MP3_320, true },
            new object[] { Quality.MP3_192, 1, Quality.MP3_192, 1, Quality.MP3_192, false },
            new object[] { Quality.MP3_320, 1, Quality.MP3_256, 2, Quality.MP3_320, false },
            new object[] { Quality.MP3_320, 1, Quality.MP3_256, 2, Quality.MP3_320, false },
            new object[] { Quality.MP3_320, 1, Quality.MP3_320, 1, Quality.MP3_320, false }
        };

        public static object[] IsUpgradeTestCasesLanguages =
        {
            new object[] { Quality.MP3_192, 1, Language.English, Quality.MP3_192, 2, Language.English, Quality.MP3_192, Language.Spanish, true },
            new object[] { Quality.MP3_192, 1, Language.English, Quality.MP3_192, 1, Language.Spanish, Quality.MP3_192, Language.Spanish, true },
            new object[] { Quality.MP3_320, 1, Language.French, Quality.MP3_320, 2, Language.English, Quality.MP3_320, Language.Spanish, true },
            new object[] { Quality.MP3_192, 1, Language.English, Quality.MP3_192, 1, Language.English, Quality.MP3_192, Language.English, false },
            new object[] { Quality.MP3_320, 1, Language.English, Quality.MP3_256, 2, Language.Spanish, Quality.FLAC, Language.Spanish, false },
            new object[] { Quality.MP3_320, 1, Language.Spanish, Quality.MP3_256, 2, Language.French, Quality.MP3_320, Language.Spanish, false }
        };

        private static readonly int NoPreferredWordScore = 0;

        private void GivenAutoDownloadPropers(ProperDownloadTypes type)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.DownloadPropersAndRepacks)
                  .Returns(type);
        }

        [Test, TestCaseSource(nameof(IsUpgradeTestCases))]
        public void IsUpgradeTest(Quality current, int currentVersion, Quality newQuality, int newVersion, Quality cutoff, bool expected)
        {
            GivenAutoDownloadPropers(ProperDownloadTypes.PreferAndUpgrade);

            var profile = new QualityProfile
            {
                UpgradeAllowed = true,
                Items = Qualities.QualityFixture.GetDefaultQualities()
            };

            var langProfile = new LanguageProfile
            {
                UpgradeAllowed = true,
                Languages = LanguageFixture.GetDefaultLanguages(),
                Cutoff = Language.English
            };

            Subject.IsUpgradable(
                        profile,
                        langProfile,
                        new List<QualityModel> { new QualityModel(current, new Revision(version: currentVersion)) },
                        new List<Language> { Language.English },
                        NoPreferredWordScore,
                        new QualityModel(newQuality, new Revision(version: newVersion)),
                        Language.English,
                        NoPreferredWordScore)
                   .Should().Be(expected);
        }

        [Test, TestCaseSource(nameof(IsUpgradeTestCasesLanguages))]
        public void IsUpgradeTestLanguage(Quality current, int currentVersion, Language currentLanguage, Quality newQuality, int newVersion, Language newLanguage, Quality cutoff, Language languageCutoff, bool expected)
        {
            GivenAutoDownloadPropers(ProperDownloadTypes.PreferAndUpgrade);

            var profile = new QualityProfile
            {
                UpgradeAllowed = true,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                Cutoff = cutoff.Id,
            };

            var langProfile = new LanguageProfile
            {
                UpgradeAllowed = true,
                Languages = LanguageFixture.GetDefaultLanguages(),
                Cutoff = languageCutoff
            };

            Subject.IsUpgradable(
                        profile,
                        langProfile,
                        new List<QualityModel> { new QualityModel(current, new Revision(version: currentVersion)) },
                        new List<Language> { currentLanguage },
                        NoPreferredWordScore,
                        new QualityModel(newQuality, new Revision(version: newVersion)),
                        newLanguage,
                        NoPreferredWordScore)
                   .Should().Be(expected);
        }

        [Test]
        public void should_return_true_if_proper_and_download_propers_is_do_not_download()
        {
            GivenAutoDownloadPropers(ProperDownloadTypes.DoNotUpgrade);

            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            var langProfile = new LanguageProfile

            {
                Languages = LanguageFixture.GetDefaultLanguages(),
                Cutoff = Language.English
            };

            Subject.IsUpgradable(
                        profile,
                        langProfile,
                        new List<QualityModel> { new QualityModel(Quality.MP3_256, new Revision(version: 1)) },
                        new List<Language> { Language.English },
                        NoPreferredWordScore,
                        new QualityModel(Quality.MP3_256, new Revision(version: 2)),
                        Language.English,
                        NoPreferredWordScore)
                    .Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_proper_and_autoDownloadPropers_is_do_not_prefer()
        {
            GivenAutoDownloadPropers(ProperDownloadTypes.DoNotPrefer);

            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            var langProfile = new LanguageProfile
            {
                Languages = LanguageFixture.GetDefaultLanguages(),
                Cutoff = Language.English
            };

            Subject.IsUpgradable(
                        profile,
                        langProfile,
                        new List<QualityModel> { new QualityModel(Quality.MP3_256, new Revision(version: 1)) },
                        new List<Language> { Language.English },
                        NoPreferredWordScore,
                        new QualityModel(Quality.MP3_256, new Revision(version: 2)),
                        Language.English,
                        NoPreferredWordScore)
                    .Should().BeFalse();
        }
    }
}
