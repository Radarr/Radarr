using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class UpgradeDiskSpecificationFixture : CoreTest<UpgradeDiskSpecification>
    {
        private UpgradeDiskSpecification _upgradeDisk;

        private RemoteMovie _parseResultSingle;
        private MovieFile _firstFile;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();
            _upgradeDisk = Mocker.Resolve<UpgradeDiskSpecification>();

            CustomFormatsTestHelpers.GivenCustomFormats();

            _firstFile = new MovieFile { Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 2)), DateAdded = DateTime.Now };

            var fakeMovie = Builder<Movie>.CreateNew()
                .With(c => c.QualityProfile = new QualityProfile
                {
                    UpgradeAllowed = true,
                    Cutoff = Quality.Bluray1080p.Id,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                    FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems("None"),
                    MinFormatScore = 0,
                })
                .With(e => e.MovieFile = _firstFile)
                .Build();

            _parseResultSingle = new RemoteMovie
            {
                Movie = fakeMovie,
                ParsedMovieInfo = new ParsedMovieInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
                CustomFormats = new List<CustomFormat>()
            };

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(x => x.ParseCustomFormat(It.IsAny<MovieFile>()))
                .Returns(new List<CustomFormat>());
        }

        private void GivenProfile(QualityProfile profile)
        {
            CustomFormatsTestHelpers.GivenCustomFormats();
            profile.FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems();
            profile.MinFormatScore = 0;
            _parseResultSingle.Movie.QualityProfile = profile;

            Console.WriteLine(profile.ToJson());
        }

        private void GivenFileQuality(QualityModel quality)
        {
            _firstFile.Quality = quality;
        }

        private void GivenNewQuality(QualityModel quality)
        {
            _parseResultSingle.ParsedMovieInfo.Quality = quality;
        }

        private void GivenOldCustomFormats(List<CustomFormat> formats)
        {
            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(x => x.ParseCustomFormat(It.IsAny<MovieFile>()))
                .Returns(formats);
        }

        private void GivenNewCustomFormats(List<CustomFormat> formats)
        {
            _parseResultSingle.CustomFormats = formats;
        }

        private void WithFirstFileUpgradable()
        {
            _firstFile.Quality = new QualityModel(Quality.SDTV);
        }

        [Test]
        public void should_return_true_if_movie_has_no_existing_file()
        {
            _parseResultSingle.Movie.MovieFile = null;
            _upgradeDisk.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_only_movie_is_upgradable()
        {
            WithFirstFileUpgradable();
            _upgradeDisk.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_not_be_upgradable_if_qualities_are_the_same()
        {
            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(x => x.ParseCustomFormat(It.IsAny<MovieFile>()))
                .Returns(new List<CustomFormat>());

            _firstFile.Quality = new QualityModel(Quality.WEBDL1080p);
            _parseResultSingle.ParsedMovieInfo.Quality = new QualityModel(Quality.WEBDL1080p);
            _upgradeDisk.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_revision_downgrade_if_propers_are_preferred()
        {
            _firstFile.Quality = new QualityModel(Quality.WEBDL1080p, new Revision(2));
            _parseResultSingle.ParsedMovieInfo.Quality = new QualityModel(Quality.WEBDL1080p);
            _upgradeDisk.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_current_movie_is_equal_to_cutoff()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_current_movie_is_greater_than_cutoff()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_new_movie_is_proper_but_existing_is_not()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 1)));
            GivenNewQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));
            GivenNewQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_quality_cutoff_is_met_and_quality_is_higher_but_language_is_met()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));
            GivenNewQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher_and_language_is_higher()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));
            GivenNewQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_cutoff_is_not_met_and_new_quality_is_higher_and_language_is_higher()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.SDTV, new Revision(version: 2)));
            GivenNewQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_cutoff_is_not_met_and_language_is_higher()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.SDTV, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_custom_formats_is_met_and_quality_and_format_higher()
        {
            var customFormat = new CustomFormat("My Format", new ResolutionSpecification { Value = (int)Resolution.R1080p }) { Id = 1 };

            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                MinFormatScore = 0,
                FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems("My Format"),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.HDTV720p));
            GivenNewQuality(new QualityModel(Quality.Bluray1080p));

            GivenOldCustomFormats(new List<CustomFormat>());
            GivenNewCustomFormats(new List<CustomFormat> { customFormat });

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_cutoffs_are_met_but_is_a_revision_upgrade()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.HDTV1080p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true
            });

            GivenFileQuality(new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)));
            GivenNewQuality(new QualityModel(Quality.WEBDL1080p, new Revision(version: 2)));

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_quality_profile_does_not_allow_upgrades_but_cutoff_is_set_to_highest_quality()
        {
            GivenProfile(new QualityProfile
            {
                Cutoff = Quality.RAWHD.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = false
            });

            GivenFileQuality(new QualityModel(Quality.WEBDL1080p));
            GivenNewQuality(new QualityModel(Quality.Bluray1080p));

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }
    }
}
