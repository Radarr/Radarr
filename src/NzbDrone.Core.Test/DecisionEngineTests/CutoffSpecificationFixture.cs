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
        public void should_return_true_if_current_album_is_less_than_cutoff()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.MP3_512, Items = Qualities.QualityFixture.GetDefaultQualities() },
                new QualityModel(Quality.MP3_192, new Revision(version: 2))).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_current_album_is_equal_to_cutoff()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.MP3_256, Items = Qualities.QualityFixture.GetDefaultQualities() },
                               new QualityModel(Quality.MP3_256, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_current_album_is_greater_than_cutoff()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.MP3_256, Items = Qualities.QualityFixture.GetDefaultQualities() },
                                new QualityModel(Quality.MP3_512, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_new_album_is_proper_but_existing_is_not()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.MP3_256, Items = Qualities.QualityFixture.GetDefaultQualities() },
                                new QualityModel(Quality.MP3_256, new Revision(version: 1)),
                                new QualityModel(Quality.MP3_256, new Revision(version: 2))).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.MP3_256, Items = Qualities.QualityFixture.GetDefaultQualities() },
                                new QualityModel(Quality.MP3_256, new Revision(version: 2)),
                                new QualityModel(Quality.MP3_512, new Revision(version: 2))).Should().BeFalse();
        }
    }
}
