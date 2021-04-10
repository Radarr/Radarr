using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
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

            CustomFormatsFixture.GivenCustomFormats();

            _firstFile = new MovieFile { Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 2)), DateAdded = DateTime.Now };

            var fakeSeries = Builder<Movie>.CreateNew()
                .With(c => c.Profile = new Profile
                {
                    Cutoff = Quality.Bluray1080p.Id, Items = Qualities.QualityFixture.GetDefaultQualities(),
                    FormatItems = CustomFormatsFixture.GetSampleFormatItems(),
                    MinFormatScore = 0
                })
                .With(e => e.MovieFile = _firstFile)
                .Build();

            _parseResultSingle = new RemoteMovie
            {
                Movie = fakeSeries,
                ParsedMovieInfo = new ParsedMovieInfo() { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
                CustomFormats = new List<CustomFormat>()
            };

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(x => x.ParseCustomFormat(It.IsAny<MovieFile>()))
                .Returns(new List<CustomFormat>());
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
    }
}
