using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]

    public class GetMovieFolderFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [TestCase("Arrival", 2016, "{Movie Title} ({Release Year})", "Arrival (2016)")]
        [TestCase("The Big Short", 2015, "{Movie TitleThe} ({Release Year})", "Big Short, The (2015)")]
        [TestCase("The Big Short", 2015, "{Movie Title} ({Release Year})", "The Big Short (2015)")]
        public void should_use_movieFolderFormat_to_build_folder_name(string movieTitle, int year, string format, string expected)
        {
            _namingConfig.MovieFolderFormat = format;

            var movie = new Movie { Title = movieTitle, Year = year };

            Subject.GetMovieFolder(movie).Should().Be(expected);
        }
    }
}
