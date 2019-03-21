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
    public class CutoffSpecificationFixture : CoreTest<UpgradableSpecification>
    {
        private static readonly int NoPreferredWordScore = 0;

        [Test]
        public void should_return_true_if_current_album_is_less_than_cutoff()
        {
            Subject.CutoffNotMet(
             new QualityProfile

             {
                 Cutoff = Quality.MP3_256.Id,
                 Items = Qualities.QualityFixture.GetDefaultQualities()
             },
             new LanguageProfile
             {
                 Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                 Cutoff = Language.English
             },
             new List<QualityModel> { new QualityModel(Quality.MP3_192, new Revision(version: 2)) },
             new List<Language> { Language.English }, NoPreferredWordScore).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_current_album_is_equal_to_cutoff()
        {
            Subject.CutoffNotMet(
            new QualityProfile
            {
                Cutoff = Quality.MP3_256.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities()
            },
            new LanguageProfile
            {
                Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                Cutoff = Language.English
            },
            new List<QualityModel> { new QualityModel(Quality.MP3_256, new Revision(version: 2)) },
            new List<Language> { Language.English }, NoPreferredWordScore).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_current_album_is_greater_than_cutoff()
        {
            Subject.CutoffNotMet(
            new QualityProfile

            {
                Cutoff = Quality.MP3_256.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities()
            },
            new LanguageProfile
            {
                Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                Cutoff = Language.English
            },
            new List<QualityModel> { new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
            new List<Language> { Language.English }, NoPreferredWordScore).Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_new_album_is_proper_but_existing_is_not()
        {
            Subject.CutoffNotMet(
            new QualityProfile

            {
                Cutoff = Quality.MP3_320.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities()
            },
            new LanguageProfile
            {
                Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                Cutoff = Language.English
            },
            new List<QualityModel> { new QualityModel(Quality.MP3_320, new Revision(version: 1)) },
            new List<Language> { Language.English },
            NoPreferredWordScore,
            new QualityModel(Quality.MP3_320, new Revision(version: 2))).Should().BeTrue();

        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher()
        {
            Subject.CutoffNotMet(
            new QualityProfile

            {
                Cutoff = Quality.MP3_320.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities()
            },
            new LanguageProfile
            {
                Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                Cutoff = Language.English
            },
            new List<QualityModel> { new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
            new List<Language> { Language.English },
            NoPreferredWordScore,
            new QualityModel(Quality.FLAC, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_quality_cutoff_is_met_and_quality_is_higher_but_language_is_not_met()
        {

            QualityProfile _profile = new QualityProfile
            {
                Cutoff = Quality.MP3_320.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            LanguageProfile _langProfile = new LanguageProfile
            {
                Cutoff = Language.Spanish,
                Languages = LanguageFixture.GetDefaultLanguages()
            };

            Subject.CutoffNotMet(_profile,
                _langProfile,
                 new List<QualityModel> { new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
                 new List<Language> { Language.English },
                 NoPreferredWordScore,
                 new QualityModel(Quality.FLAC, new Revision(version: 2))).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher_and_language_is_met()
        {

            QualityProfile _profile = new QualityProfile
            {
                Cutoff = Quality.MP3_320.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            LanguageProfile _langProfile = new LanguageProfile
            {
                Cutoff = Language.Spanish,
                Languages = LanguageFixture.GetDefaultLanguages()
            };

            Subject.CutoffNotMet(
                _profile,
                _langProfile,
                new List<QualityModel> { new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
                new List<Language> { Language.Spanish },
                NoPreferredWordScore,
                new QualityModel(Quality.FLAC, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher_and_language_is_higher()
        {

            QualityProfile _profile = new QualityProfile
            {
                Cutoff = Quality.MP3_320.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            LanguageProfile _langProfile = new LanguageProfile
            {
                Cutoff = Language.Spanish,
                Languages = LanguageFixture.GetDefaultLanguages()
            };

            Subject.CutoffNotMet(
                _profile,
                _langProfile,
                new List<QualityModel> { new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
                new List<Language> { Language.French },
                NoPreferredWordScore,
                new QualityModel(Quality.FLAC, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_cutoff_is_not_met_and_new_quality_is_higher_and_language_is_higher()
        {

            QualityProfile _profile = new QualityProfile
            {
                Cutoff = Quality.MP3_320.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            LanguageProfile _langProfile = new LanguageProfile
            {
                Cutoff = Language.Spanish,
                Languages = LanguageFixture.GetDefaultLanguages()
            };

            Subject.CutoffNotMet(
                _profile,
                _langProfile,
                new List<QualityModel> { new QualityModel(Quality.MP3_256, new Revision(version: 2)) },
                new List<Language> { Language.French },
                NoPreferredWordScore,
                new QualityModel(Quality.FLAC, new Revision(version: 2))).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_cutoff_is_not_met_and_language_is_higher()
        {

            QualityProfile _profile = new QualityProfile
            {
                Cutoff = Quality.MP3_320.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            LanguageProfile _langProfile = new LanguageProfile
            {
                Cutoff = Language.Spanish,
                Languages = LanguageFixture.GetDefaultLanguages()
            };

            Subject.CutoffNotMet(
                _profile,
                _langProfile,
                new List<QualityModel> { new QualityModel(Quality.MP3_256, new Revision(version: 2)) },
                new List<Language> { Language.French },
                NoPreferredWordScore).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_cutoffs_are_met_and_score_is_higher()
        {
            QualityProfile _profile = new QualityProfile
            {
                Cutoff = Quality.MP3_320.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            LanguageProfile _langProfile = new LanguageProfile
            {
                Cutoff = Language.Spanish,
                Languages = LanguageFixture.GetDefaultLanguages()
            };

            Subject.CutoffNotMet(
                _profile,
                _langProfile,
                new List<QualityModel> { new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
                new List<Language> { Language.Spanish },
                NoPreferredWordScore,
                new QualityModel(Quality.FLAC, new Revision(version: 2)),
                10).Should().BeTrue();
        }
    }
}
