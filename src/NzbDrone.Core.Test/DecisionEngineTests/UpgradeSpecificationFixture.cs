using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class UpgradeSpecificationFixture : CoreTest<UpgradableSpecification>
    {
        private static readonly CustomFormat CustomFormat1 = new ("My Format 1", new ResolutionSpecification { Value = (int)Resolution.R1080p }) { Id = 1 };
        private static readonly CustomFormat CustomFormat2 = new ("My Format 2", new ResolutionSpecification { Value = (int)Resolution.R480p }) { Id = 2 };

        public static object[] IsUpgradeTestCases =
        {
            // Quality upgrade trumps custom format
            new object[] { Quality.SDTV, 1, new List<CustomFormat>(), Quality.SDTV, 2, new List<CustomFormat>(), UpgradeableRejectReason.None },
            new object[] { Quality.SDTV, 1, new List<CustomFormat> { CustomFormat1 }, Quality.SDTV, 2, new List<CustomFormat> { CustomFormat1 }, UpgradeableRejectReason.None },
            new object[] { Quality.SDTV, 1, new List<CustomFormat> { CustomFormat1 }, Quality.SDTV, 2, new List<CustomFormat> { CustomFormat2 }, UpgradeableRejectReason.None },
            new object[] { Quality.SDTV, 1, new List<CustomFormat> { CustomFormat2 }, Quality.SDTV, 2, new List<CustomFormat> { CustomFormat1 }, UpgradeableRejectReason.None },

            // Revision upgrade trumps custom format
            new object[] { Quality.WEBDL720p, 1, new List<CustomFormat>(), Quality.WEBDL720p, 2, new List<CustomFormat>(), UpgradeableRejectReason.None },
            new object[] { Quality.WEBDL720p, 1, new List<CustomFormat> { CustomFormat1 }, Quality.WEBDL720p, 2, new List<CustomFormat> { CustomFormat1 }, UpgradeableRejectReason.None },
            new object[] { Quality.WEBDL720p, 1, new List<CustomFormat> { CustomFormat1 }, Quality.WEBDL720p, 2, new List<CustomFormat> { CustomFormat2 }, UpgradeableRejectReason.None },
            new object[] { Quality.WEBDL720p, 1, new List<CustomFormat> { CustomFormat2 }, Quality.WEBDL720p, 2, new List<CustomFormat> { CustomFormat1 }, UpgradeableRejectReason.None },

            // Custom formats apply if quality same
            new object[] { Quality.SDTV, 1, new List<CustomFormat>(), Quality.SDTV, 1, new List<CustomFormat>(), UpgradeableRejectReason.CustomFormatScore },
            new object[] { Quality.SDTV, 1, new List<CustomFormat> { CustomFormat1 }, Quality.SDTV, 1, new List<CustomFormat> { CustomFormat1 }, UpgradeableRejectReason.CustomFormatScore },
            new object[] { Quality.SDTV, 1, new List<CustomFormat> { CustomFormat1 }, Quality.SDTV, 1, new List<CustomFormat> { CustomFormat2 }, UpgradeableRejectReason.CustomFormatCutoff },
            new object[] { Quality.SDTV, 1, new List<CustomFormat> { CustomFormat2 }, Quality.SDTV, 1, new List<CustomFormat> { CustomFormat1 }, UpgradeableRejectReason.CustomFormatScore },

            new object[] { Quality.WEBDL720p, 1, new List<CustomFormat>(), Quality.HDTV720p, 2, new List<CustomFormat>(), UpgradeableRejectReason.BetterQuality },
            new object[] { Quality.WEBDL720p, 1, new List<CustomFormat>(), Quality.HDTV720p, 2, new List<CustomFormat>(), UpgradeableRejectReason.BetterQuality },
            new object[] { Quality.WEBDL720p, 1, new List<CustomFormat>(), Quality.WEBDL720p, 1, new List<CustomFormat>(), UpgradeableRejectReason.CustomFormatScore },
            new object[] { Quality.WEBDL1080p, 1, new List<CustomFormat>(), Quality.WEBDL1080p, 1, new List<CustomFormat>(), UpgradeableRejectReason.CustomFormatScore }
        };

        [SetUp]
        public void Setup()
        {
            CustomFormatsTestHelpers.GivenCustomFormats(CustomFormat1, CustomFormat2);
        }

        private void GivenAutoDownloadPropers(ProperDownloadTypes type)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.DownloadPropersAndRepacks)
                  .Returns(type);
        }

        [Test]
        [TestCaseSource(nameof(IsUpgradeTestCases))]
        public void IsUpgradeTest(Quality current,
                                  int currentVersion,
                                  List<CustomFormat> currentFormats,
                                  Quality newQuality,
                                  int newVersion,
                                  List<CustomFormat> newFormats,
                                  UpgradeableRejectReason expected)
        {
            GivenAutoDownloadPropers(ProperDownloadTypes.PreferAndUpgrade);

            var profile = new QualityProfile
            {
                UpgradeAllowed = true,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems(CustomFormat1.Name, CustomFormat2.Name)
            };

            Subject.IsUpgradable(
                        profile,
                        new QualityModel(current, new Revision(version: currentVersion)),
                        currentFormats,
                        new QualityModel(newQuality, new Revision(version: newVersion)),
                        newFormats)
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

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(Quality.DVD, new Revision(version: 1)),
                       new List<CustomFormat>(),
                       new QualityModel(Quality.DVD, new Revision(version: 2)),
                       new List<CustomFormat>())
                    .Should().Be(UpgradeableRejectReason.None);
        }

        [Test]
        public void should_return_false_if_release_and_existing_file_are_the_same()
        {
            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                       new List<CustomFormat>(),
                       new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                       new List<CustomFormat>())
                   .Should().Be(UpgradeableRejectReason.CustomFormatScore);
        }

        [Test]
        public void should_return_true_if_release_has_higher_quality_and_cutoff_is_not_already_met()
        {
            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true,
                Cutoff = Quality.HDTV1080p.Id
            };

            Subject.IsUpgradable(
                    profile,
                    new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                    new List<CustomFormat>(),
                    new QualityModel(Quality.HDTV1080p, new Revision(version: 1)),
                    new List<CustomFormat>())
                .Should().Be(UpgradeableRejectReason.None);
        }

        [Test]
        public void should_return_false_if_release_has_higher_quality_and_cutoff_is_already_met()
        {
            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true,
                Cutoff = Quality.HDTV720p.Id
            };

            Subject.IsUpgradable(
                    profile,
                    new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                    new List<CustomFormat>(),
                    new QualityModel(Quality.HDTV1080p, new Revision(version: 1)),
                    new List<CustomFormat>())
                .Should().Be(UpgradeableRejectReason.QualityCutoff);
        }

        [Test]
        public void should_return_false_if_minimum_custom_score_is_not_met()
        {
            var customFormatOne = new CustomFormat
            {
                Id = 1,
                Name = "One"
            };

            var customFormatTwo = new CustomFormat
            {
                Id = 2,
                Name = "Two"
            };

            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true,
                MinUpgradeFormatScore = 11,
                CutoffFormatScore = 100,
                FormatItems = new List<ProfileFormatItem>
                {
                    new ProfileFormatItem
                    {
                        Format = customFormatOne,
                        Score = 10
                    },
                    new ProfileFormatItem
                    {
                        Format = customFormatTwo,
                        Score = 20
                    }
                }
            };

            Subject.IsUpgradable(
                    profile,
                    new QualityModel(Quality.DVD),
                    new List<CustomFormat> { customFormatOne },
                    new QualityModel(Quality.DVD),
                    new List<CustomFormat> { customFormatTwo })
                .Should().Be(UpgradeableRejectReason.MinCustomFormatScore);
        }

        [Test]
        public void should_return_true_if_minimum_custom_score_is_met()
        {
            var customFormatOne = new CustomFormat
            {
                Id = 1,
                Name = "One"
            };

            var customFormatTwo = new CustomFormat
            {
                Id = 2,
                Name = "Two"
            };

            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true,
                MinUpgradeFormatScore = 10,
                CutoffFormatScore = 100,
                FormatItems = new List<ProfileFormatItem>
                {
                    new ProfileFormatItem
                    {
                        Format = customFormatOne,
                        Score = 10
                    },
                    new ProfileFormatItem
                    {
                        Format = customFormatTwo,
                        Score = 20
                    }
                }
            };

            Subject.IsUpgradable(
                    profile,
                    new QualityModel(Quality.DVD),
                    new List<CustomFormat> { customFormatOne },
                    new QualityModel(Quality.DVD),
                    new List<CustomFormat> { customFormatTwo })
                .Should().Be(UpgradeableRejectReason.None);
        }
    }
}
