using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]

    public class TruncatedMovieTitleFixture : CoreTest<FileNameBuilder>
    {
        private Movie _movie;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>
                    .CreateNew()
                    .With(s => s.Title = "Movie Title")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameMovies = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        [TestCase("{Movie Title:16}", "The Fantastic...")]
        [TestCase("{Movie TitleThe:17}", "Fantastic Life...")]
        [TestCase("{Movie CleanTitle:-13}", "...Mr. Sisko")]
        public void should_truncate_series_title(string format, string expected)
        {
            _movie.Title = "The Fantastic Life of Mr. Sisko";
            _namingConfig.MovieFolderFormat = format;

            var result = Subject.GetMovieFolder(_movie, _namingConfig);
            result.Should().Be(expected);
        }
    }
}
