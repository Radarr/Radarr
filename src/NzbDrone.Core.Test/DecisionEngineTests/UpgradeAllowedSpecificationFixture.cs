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
    public class UpgradeAllowedSpecificationFixture : CoreTest<UpgradableSpecification>
    {
        [Test]
        public void should_return_false_when_quality_is_better_and_upgrade_allowed_is_false_for_quality_profile()
        {
            Subject.IsUpgradeAllowed(
                new QualityProfile
                {
                    Cutoff = Quality.FLAC.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    UpgradeAllowed = false
                },
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new QualityModel(Quality.FLAC)
            ).Should().BeFalse();
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
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new QualityModel(Quality.FLAC)
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
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new QualityModel(Quality.MP3_320)
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
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new QualityModel(Quality.MP3_320)
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
                new List<QualityModel> { new QualityModel(Quality.MP3_320) },
                new QualityModel(Quality.MP3_256)
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
                new List<QualityModel>{ new QualityModel(Quality.MP3_320) },
                new QualityModel(Quality.MP3_256)
            ).Should().BeTrue();
        }
    }
}
