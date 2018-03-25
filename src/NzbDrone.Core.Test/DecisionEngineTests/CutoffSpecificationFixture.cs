using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Test.Qualities;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class CutoffSpecificationFixture : CoreTest<QualityUpgradableSpecification>
    {
        [SetUp]
        public void Setup()
        {
            QualityDefinitionServiceFixture.SetupDefaultDefinitions();
        }

        [Test]
        public void should_return_true_if_current_episode_is_less_than_cutoff()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = QualityWrapper.Dynamic.Bluray1080p, Items = Qualities.QualityFixture.GetDefaultQualities() },
                new QualityModel(QualityWrapper.Dynamic.DVD, new Revision(version: 2))).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_current_episode_is_equal_to_cutoff()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = QualityWrapper.Dynamic.HDTV720p, Items = Qualities.QualityFixture.GetDefaultQualities() },
                               new QualityModel(QualityWrapper.Dynamic.HDTV720p, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_current_episode_is_greater_than_cutoff()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = QualityWrapper.Dynamic.HDTV720p, Items = Qualities.QualityFixture.GetDefaultQualities() },
                                new QualityModel(QualityWrapper.Dynamic.Bluray1080p, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_new_episode_is_proper_but_existing_is_not()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = QualityWrapper.Dynamic.HDTV720p, Items = Qualities.QualityFixture.GetDefaultQualities() },
                                new QualityModel(QualityWrapper.Dynamic.HDTV720p, new Revision(version: 1)),
                                new QualityModel(QualityWrapper.Dynamic.HDTV720p, new Revision(version: 2))).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = QualityWrapper.Dynamic.HDTV720p, Items = Qualities.QualityFixture.GetDefaultQualities() },
                                new QualityModel(QualityWrapper.Dynamic.HDTV720p, new Revision(version: 2)),
                                new QualityModel(QualityWrapper.Dynamic.Bluray1080p, new Revision(version: 2))).Should().BeFalse();
        }
    }
}
