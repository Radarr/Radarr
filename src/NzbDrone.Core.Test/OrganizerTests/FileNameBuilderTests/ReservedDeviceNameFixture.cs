using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]

    public class ReservedDeviceNameFixture : CoreTest<FileNameBuilder>
    {
        private Movie _movie;
        private MovieFile _movieFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>
                    .CreateNew()
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameMovies = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _movieFile = new MovieFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "SonarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                .Setup(v => v.All())
                .Returns(new List<CustomFormat>());
        }

        [TestCase("Con Game", 2021, "Con_Game (2021)")]
        [TestCase("Com1 Sat", 2021, "Com1_Sat (2021)")]
        public void should_replace_reserved_device_name_in_movies_folder(string title, int year, string expected)
        {
            _movie.Title = title;
            _movie.Year = year;
            _namingConfig.MovieFolderFormat = "{Movie.Title} ({Release Year})";

            Subject.GetMovieFolder(_movie).Should().Be($"{expected}");
        }

        [TestCase("Con Game", 2021, "Con_Game (2021)")]
        [TestCase("Com1 Sat", 2021, "Com1_Sat (2021)")]
        public void should_replace_reserved_device_name_in_file_name(string title, int year, string expected)
        {
            _movie.Title = title;
            _movie.Year = year;
            _namingConfig.StandardMovieFormat = "{Movie.Title} ({Release Year})";

            Subject.BuildFileName(_movie, _movieFile).Should().Be($"{expected}");
        }
    }
}
