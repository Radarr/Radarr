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
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.CustomFormats;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class CutoffSpecificationFixture : CoreTest<CutoffSpecification>
    {
        private CustomFormat _customFormat;
        private RemoteMovie _remoteMovie;

        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IUpgradableSpecification>(Mocker.Resolve<UpgradableSpecification>());

            _remoteMovie = new RemoteMovie()
            {
                Movie = Builder<Movie>.CreateNew().Build(),
                ParsedMovieInfo = Builder<ParsedMovieInfo>.CreateNew().With(x => x.Quality = null).Build()
            };

            GivenOldCustomFormats(new List<CustomFormat>());
        }

        private void GivenProfile(Profile profile)
        {
            CustomFormatsFixture.GivenCustomFormats(CustomFormat.None);
            profile.FormatItems = CustomFormatsFixture.GetSampleFormatItems("None");
            profile.FormatCutoff = CustomFormat.None.Id;
            _remoteMovie.Movie.Profile = profile;

            Console.WriteLine(profile.ToJson());
        }

        private void GivenFileQuality(QualityModel quality)
        {
            _remoteMovie.Movie.MovieFile = Builder<MovieFile>.CreateNew().With(x => x.Quality = quality).Build();
        }

        private void GivenNewQuality(QualityModel quality)
        {
            _remoteMovie.ParsedMovieInfo.Quality = quality;
        }

        private void GivenOldCustomFormats(List<CustomFormat> formats)
        {
            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(x => x.ParseCustomFormat(It.IsAny<MovieFile>()))
                .Returns(formats);
        }

        private void GivenNewCustomFormats(List<CustomFormat> formats)
        {
            _remoteMovie.CustomFormats = formats;
        }

        private void GivenCustomFormatHigher()
        {
            _customFormat = new CustomFormat("My Format", "L_ENGLISH") { Id = 1 };

            CustomFormatsFixture.GivenCustomFormats(_customFormat, CustomFormat.None);
        }

        [Test]
        public void should_return_true_if_current_episode_is_less_than_cutoff()
        {
            GivenProfile(new Profile { Cutoff = Quality.Bluray1080p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() });
            GivenFileQuality(new QualityModel(Quality.DVD, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_current_episode_is_equal_to_cutoff()
        {
            GivenProfile(new Profile { Cutoff = Quality.HDTV720p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() });
            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_current_episode_is_greater_than_cutoff()
        {
            GivenProfile(new Profile { Cutoff = Quality.HDTV720p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() });
            GivenFileQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_new_episode_is_proper_but_existing_is_not()
        {
            GivenProfile(new Profile { Cutoff = Quality.HDTV720p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() });
            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 1)));
            GivenNewQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_is_met_and_quality_is_higher()
        {
            GivenProfile(new Profile { Cutoff = Quality.HDTV720p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() });
            GivenFileQuality(new QualityModel(Quality.HDTV720p, new Revision(version: 2)));
            GivenNewQuality(new QualityModel(Quality.Bluray1080p, new Revision(version: 2)));
            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_custom_formats_is_met_and_quality_and_format_higher()
        {
            GivenProfile(new Profile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                FormatCutoff = CustomFormat.None.Id,
                FormatItems = CustomFormatsFixture.GetSampleFormatItems("None", "My Format")
            });

            GivenFileQuality(new QualityModel(Quality.HDTV720p));
            GivenNewQuality(new QualityModel(Quality.Bluray1080p));

            GivenCustomFormatHigher();

            GivenOldCustomFormats(new List<CustomFormat> { CustomFormat.None });
            GivenNewCustomFormats(new List<CustomFormat> { _customFormat });

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_cutoffs_are_met_but_is_a_revision_upgrade()
        {
            GivenProfile(new Profile
            {
                Cutoff = Quality.HDTV1080p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            });

            GivenFileQuality(new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)));
            GivenNewQuality(new QualityModel(Quality.WEBDL1080p, new Revision(version: 2)));

            Subject.IsSatisfiedBy(_remoteMovie, null).Accepted.Should().BeTrue();
        }
    }
}
