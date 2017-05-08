using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class CutoffSpecificationFixture : CoreTest<QualityUpgradableSpecification>
    {
        [Test]
        public void should_return_true_if_current_episode_is_less_than_cutoff()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.MP3512, Items = Qualities.QualityFixture.GetDefaultQualities() },
                new QualityModel(Quality.MP3192, new Revision(version: 2))).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_current_episode_is_equal_to_cutoff()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.MP3256, Items = Qualities.QualityFixture.GetDefaultQualities() },
                               new QualityModel(Quality.MP3256, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_current_episode_is_greater_than_cutoff()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.MP3256, Items = Qualities.QualityFixture.GetDefaultQualities() },
                                new QualityModel(Quality.MP3512, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_new_episode_is_proper_but_existing_is_not()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.MP3256, Items = Qualities.QualityFixture.GetDefaultQualities() },
                                new QualityModel(Quality.MP3256, new Revision(version: 1)),
                                new QualityModel(Quality.MP3256, new Revision(version: 2))).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.MP3256, Items = Qualities.QualityFixture.GetDefaultQualities() },
                                new QualityModel(Quality.MP3256, new Revision(version: 2)),
                                new QualityModel(Quality.MP3512, new Revision(version: 2))).Should().BeFalse();
        }
    }
}
