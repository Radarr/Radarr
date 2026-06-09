using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.CustomFormats
{
    [TestFixture]
    public class CustomFormatCalculationServiceFixture : CoreTest<CustomFormatCalculationService>
    {
        private CustomFormat _yearFormat;
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _yearFormat = new CustomFormat("2020s",
                new YearSpecification
                {
                    Min = 2020,
                    Max = 2029
                });

            _movie = new Movie
            {
                Title = "Test Movie",
                Year = 2025
            };

            Mocker.GetMock<ICustomFormatService>()
                .Setup(s => s.All())
                .Returns(new List<CustomFormat> { _yearFormat });
        }

        [Test]
        public void should_match_year_condition_when_parsing_remote_movie()
        {
            var remoteMovie = new RemoteMovie
            {
                Movie = _movie,
                ParsedMovieInfo = new ParsedMovieInfo
                {
                    Year = 2025
                }
            };

            Subject.ParseCustomFormat(remoteMovie, 0).Should().Contain(_yearFormat);
        }

        [Test]
        public void should_match_year_condition_when_parsing_imported_movie_file()
        {
            var movieFile = new MovieFile
            {
                RelativePath = "Test.Movie.2025.mkv"
            };

            Subject.ParseCustomFormat(movieFile, _movie).Should().Contain(_yearFormat);
        }
    }
}
