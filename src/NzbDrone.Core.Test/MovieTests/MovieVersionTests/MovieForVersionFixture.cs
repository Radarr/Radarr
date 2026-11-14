using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Versions;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests.MovieVersionTests
{
    [TestFixture]
    public class MovieForVersionFixture : CoreTest
    {
        [Test]
        public void should_use_path_profile_and_file_of_the_version()
        {
            var movie = new Movie
            {
                Id = 5,
                Path = "/Movies/Movie (2022)",
                QualityProfileId = 1,
                QualityProfile = new QualityProfile { Id = 1 },
                MovieFileId = 10
            };

            var view = movie.ForVersion(new MovieVersion { Id = 7, MovieId = 5, Path = "/Movies 4K/Movie (2022)", QualityProfileId = 2, MovieFileId = 20 });

            view.Id.Should().Be(5);
            view.MovieVersionId.Should().Be(7);
            view.IsVersionView.Should().BeTrue();
            view.Path.Should().Be("/Movies 4K/Movie (2022)");
            view.QualityProfileId.Should().Be(2);
            view.QualityProfile.Should().BeNull();
            view.MovieFileId.Should().Be(20);
        }

        [Test]
        public void should_not_change_the_movie()
        {
            var movie = new Movie { Id = 5, Path = "/Movies/Movie (2022)", QualityProfileId = 1, MovieFileId = 10 };

            movie.ForVersion(new MovieVersion { Id = 7, MovieId = 5, Path = "/Movies 4K/Movie (2022)", QualityProfileId = 2, MovieFileId = 20 });

            movie.IsVersionView.Should().BeFalse();
            movie.Path.Should().Be("/Movies/Movie (2022)");
            movie.QualityProfileId.Should().Be(1);
            movie.MovieFileId.Should().Be(10);
        }
    }
}
