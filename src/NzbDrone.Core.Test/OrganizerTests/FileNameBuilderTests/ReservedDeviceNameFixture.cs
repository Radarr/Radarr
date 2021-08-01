using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

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
        }

        [Test]
        public void should_replace_reserved_device_name_in_movies_folder()
        {
            _movie.Title = "Con Man";
            _movie.Year = 2021;
            _namingConfig.MovieFolderFormat = "{Movie.Title} ({Release Year})";

            Subject.GetMovieFolder(_movie).Should().Be("Con_Man (2021)");
        }

        [Test]
        public void should_replace_reserved_device_name_in_file_name()
        {
            _movie.Title = "Con Man";
            _movie.Year = 2021;
            _namingConfig.StandardMovieFormat = "{Movie.Title} ({Release Year})";

            Subject.BuildFileName(_movie, _movieFile).Should().Be("Con_Man (2021)");
        }
    }
}
