using FluentAssertions;
using NUnit.Framework;
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
    public class UpgradeAllowedSpecificationFixture : CoreTest<UpgradableSpecification>
    {
        [Test]
        public void should_return_false_when_quality_are_the_same_language_is_better_and_upgrade_allowed_is_false_for_language_profile()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.FLAC.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English, Language.French),
                    Cutoff = Language.French,
                    UpgradeAllowed = false
                },
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new List<Language> { Language.English },
                new QualityModel(Quality.MP3_320),
                Language.French
            ).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_is_better_languages_are_the_same_and_upgrade_allowed_is_false_for_quality_profile()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.FLAC.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = false
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English,
                    UpgradeAllowed = true
                },
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new List<Language> { Language.English },
                new QualityModel(Quality.FLAC),
                Language.English
            ).Should().BeFalse();
        }

        [Test]
        public void should_return_true_for_language_upgrade_when_upgrading_is_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.FLAC.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English, Language.French),
                    Cutoff = Language.French,
                    UpgradeAllowed = true
                },
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new List<Language> { Language.English },
                new QualityModel(Quality.MP3_320),
                Language.French
            ).Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_same_language_when_upgrading_is_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.FLAC.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English, Language.French),
                    Cutoff = Language.French,
                    UpgradeAllowed = true
                },
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new List<Language> { Language.English },
                new QualityModel(Quality.MP3_320),
                Language.English
            ).Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_same_language_when_upgrading_is_not_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.FLAC.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English, Language.French),
                    Cutoff = Language.French,
                    UpgradeAllowed = false
                },
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new List<Language> { Language.French },
                new QualityModel(Quality.MP3_320),
                Language.English
            ).Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_lower_language_when_upgrading_is_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.FLAC.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English, Language.French),
                    Cutoff = Language.French,
                    UpgradeAllowed = true
                },
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new List<Language> { Language.French },
                new QualityModel(Quality.MP3_320),
                Language.English
            ).Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_lower_language_when_upgrading_is_not_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.FLAC.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English, Language.French),
                    Cutoff = Language.French,
                    UpgradeAllowed = false
                },
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new List<Language> { Language.French },
                new QualityModel(Quality.MP3_320),
                Language.English
            ).Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_quality_upgrade_when_upgrading_is_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.FLAC.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English,
                    UpgradeAllowed = true
                },
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new List<Language> { Language.English },
                new QualityModel(Quality.FLAC),
                Language.English
            ).Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_same_quality_when_upgrading_is_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.FLAC.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English,
                    UpgradeAllowed = true
                },
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new List<Language> { Language.English },
                new QualityModel(Quality.MP3_320),
                Language.English
            ).Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_same_quality_when_upgrading_is_not_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.FLAC.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = false
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English,
                    UpgradeAllowed = true
                },
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new List<Language> { Language.English },
                new QualityModel(Quality.MP3_320),
                Language.English
            ).Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_lower_quality_when_upgrading_is_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.FLAC.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = true
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English,
                    UpgradeAllowed = true
                },
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new List<Language> { Language.English },
                new QualityModel(Quality.MP3_256),
                Language.English
            ).Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_lower_quality_when_upgrading_is_not_allowed()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.FLAC.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = false
                },
                new LanguageProfile
                {
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English,
                    UpgradeAllowed = true
                },
                new List<QualityModel>{ new QualityModel(Quality.MP3_320) },
                new List<Language> { Language.English },
                new QualityModel(Quality.MP3_256),
                Language.English
            ).Should().BeTrue();
        }
    }
}
