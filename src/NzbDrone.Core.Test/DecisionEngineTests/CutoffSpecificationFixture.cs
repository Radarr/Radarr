using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Test.CustomFormat;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class CutoffSpecificationFixture : CoreTest<UpgradableSpecification>
    {

        private CustomFormats.CustomFormat _customFormat;

        [SetUp]
        public void Setup()
        {

        }

        private void GivenCustomFormatHigher()
        {
            _customFormat = new CustomFormats.CustomFormat("My Format", "L_ENGLISH") {Id = 1};

            CustomFormatsFixture.GivenCustomFormats(_customFormat, CustomFormats.CustomFormat.None);
        }

        [Test]
        public void should_return_true_if_current_episode_is_less_than_cutoff()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.Bluray1080p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() },
                new QualityModel(Quality.DVD, new Revision(version: 2))).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_current_episode_is_equal_to_cutoff()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.HDTV720p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() },
                               new QualityModel(Quality.HDTV720p, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_current_episode_is_greater_than_cutoff()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.HDTV720p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() },
                                new QualityModel(Quality.Bluray1080p, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_new_episode_is_proper_but_existing_is_not()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.HDTV720p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() },
                                new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                                new QualityModel(Quality.HDTV720p, new Revision(version: 2))).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher()
        {
            Subject.CutoffNotMet(new Profile { Cutoff = Quality.HDTV720p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() },
                                new QualityModel(Quality.HDTV720p, new Revision(version: 2)),
                                new QualityModel(Quality.Bluray1080p, new Revision(version: 2))).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_custom_formats_is_met_and_quality_and_format_higher()
        {
            GivenCustomFormatHigher();
            var old = new QualityModel(Quality.HDTV720p);
            old.CustomFormats = new List<CustomFormats.CustomFormat> {CustomFormats.CustomFormat.None};
            var newQ = new QualityModel(Quality.Bluray1080p);
            newQ.CustomFormats = new List<CustomFormats.CustomFormat> {_customFormat};
            Subject.CutoffNotMet(
                new Profile
                {
                    Cutoff = Quality.HDTV720p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    FormatCutoff = CustomFormats.CustomFormat.None.Id,
                    FormatItems = CustomFormatsFixture.GetSampleFormatItems("None", "My Format")
                }, old, newQ).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_cutoffs_are_met_but_is_a_revision_upgrade()
        {
            Profile _profile = new Profile
            {
                Cutoff = Quality.HDTV1080p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            Subject.CutoffNotMet(
                _profile,
                new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)),
                new QualityModel(Quality.WEBDL1080p, new Revision(version: 2))).Should().BeTrue();
        }
    }
}
