using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Specifications
{
    [TestFixture]
    public class UpgradeSpecificationFixture : CoreTest<UpgradeSpecification>
    {
        private Movie _movie;
        private LocalMovie _localMovie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .With(e => e.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                     .Build();

            _localMovie = new LocalMovie()
            {
                Path = @"C:\Test\30 Rock\30.rock.s01e01.avi",
                Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                Movie = _movie
            };
        }

        [Test]
        public void should_return_true_if_no_existing_episodeFile()
        {
            _localMovie.Movie.MovieFile = null;
            _localMovie.Movie.MovieFileId = 0;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_upgrade_for_existing_episodeFile()
        {
            _localMovie.Movie.MovieFileId = 1;
            _localMovie.Movie.MovieFile =
                    new MovieFile
                    {
                        Quality = new QualityModel(Quality.SDTV, new Revision(version: 1))
                    };

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_existing_episodeFile()
        {
            _localMovie.Movie.MovieFileId = 1;
            _localMovie.Movie.MovieFile =
                new MovieFile
                {
                    Quality = new QualityModel(Quality.Bluray720p, new Revision(version: 1))
                };

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_a_revision_upgrade_and_prefers_propers()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.PreferAndUpgrade);

            _localMovie.Movie.MovieFileId = 1;
            _localMovie.Movie.MovieFile =
                new MovieFile
                {
                    Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2))
                };

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_not_a_revision_upgrade_and_does_not_prefer_propers()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _localMovie.Movie.MovieFileId = 1;
            _localMovie.Movie.MovieFile =
                new MovieFile
                {
                    Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2))
                };

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_comparing_to_a_lower_quality_proper()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _localMovie.Quality = new QualityModel(Quality.Bluray1080p);

            _localMovie.Movie.MovieFileId = 1;
            _localMovie.Movie.MovieFile =
                new MovieFile
                {
                    Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 2))
                };

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_movie_file_is_null()
        {
            _localMovie.Movie.MovieFile = null;
            _localMovie.Movie.MovieFileId = 1;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_upgrade_to_custom_format_score()
        {
            var movieFileCustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();

            var movieFile = new MovieFile
            {
                Quality = new QualityModel(Quality.Bluray1080p)
            };

            _movie.Profile.FormatItems = movieFileCustomFormats.Select(c => new ProfileFormatItem
            {
                Format = c,
                Score = 10
            })
                .ToList();

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(movieFile))
                .Returns(movieFileCustomFormats);

            _localMovie.Quality = new QualityModel(Quality.Bluray1080p);
            _localMovie.CustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();
            _localMovie.CustomFormatScore = 20;

            _localMovie.Movie.MovieFileId = 1;
            _localMovie.Movie.MovieFile = movieFile;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_not_upgrade_to_custom_format_score_but_is_upgrade_to_quality()
        {
            var movieFileCustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();

            var movieFile = new MovieFile
            {
                Quality = new QualityModel(Quality.Bluray720p)
            };

            _movie.Profile.FormatItems = movieFileCustomFormats.Select(c => new ProfileFormatItem
            {
                Format = c,
                Score = 50
            })
                .ToList();

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(movieFile))
                .Returns(movieFileCustomFormats);

            _localMovie.Quality = new QualityModel(Quality.Bluray1080p);
            _localMovie.CustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();
            _localMovie.CustomFormatScore = 20;

            _localMovie.Movie.MovieFileId = 1;
            _localMovie.Movie.MovieFile = movieFile;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_not_upgrade_to_custom_format_score()
        {
            var movieFileCustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();

            var movieFile = new MovieFile
            {
                Quality = new QualityModel(Quality.Bluray1080p)
            };

            _movie.Profile.FormatItems = movieFileCustomFormats.Select(c => new ProfileFormatItem
            {
                Format = c,
                Score = 50
            })
                .ToList();

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(movieFile))
                .Returns(movieFileCustomFormats);

            _localMovie.Quality = new QualityModel(Quality.Bluray1080p);
            _localMovie.CustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();
            _localMovie.CustomFormatScore = 20;

            _localMovie.Movie.MovieFileId = 1;
            _localMovie.Movie.MovieFile = movieFile;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeFalse();
        }
    }
}
