using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Test.Framework;
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
             new List<QualityModel> { new QualityModel(Quality.MP3_192, new Revision(version: 2)) },
             NoPreferredWordScore).Should().BeTrue();
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
            new List<QualityModel> { new QualityModel(Quality.MP3_256, new Revision(version: 2)) },
            NoPreferredWordScore).Should().BeFalse();
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
            new List<QualityModel> { new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
            NoPreferredWordScore).Should().BeFalse();
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
            new List<QualityModel> { new QualityModel(Quality.MP3_320, new Revision(version: 1)) },
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
            new List<QualityModel> { new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
            NoPreferredWordScore,
            new QualityModel(Quality.FLAC, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_cutoffs_are_met_and_score_is_higher()
        {
            QualityProfile _profile = new QualityProfile
            {
                Cutoff = Quality.MP3_320.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            Subject.CutoffNotMet(
                _profile,
                new List<QualityModel> { new QualityModel(Quality.MP3_320, new Revision(version: 2)) },
                NoPreferredWordScore,
                new QualityModel(Quality.FLAC, new Revision(version: 2)),
                10).Should().BeTrue();
        }


        [Test]
        public void should_return_true_if_cutoffs_are_met_but_is_a_revision_upgrade()
        {
            QualityProfile _profile = new QualityProfile
            {
                Cutoff = Quality.MP3_320.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            Subject.CutoffNotMet(
                _profile,
                new List<QualityModel> { new QualityModel(Quality.FLAC, new Revision(version: 1)) },
                NoPreferredWordScore,
                new QualityModel(Quality.FLAC, new Revision(version: 2)),
                NoPreferredWordScore).Should().BeTrue();
        }
    }
}
