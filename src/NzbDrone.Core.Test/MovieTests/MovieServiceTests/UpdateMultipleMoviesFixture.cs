using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MovieTests.MovieServiceTests
{
    [TestFixture]
    public class UpdateMultipleMoviesFixture : CoreTest<MovieService>
    {
        private List<Movie> _movies;

        [SetUp]
        public void Setup()
        {
            _movies = Builder<Movie>.CreateListOfSize(5)
                .All()
                .With(s => s.QualityProfileId = 1)
                .With(s => s.Monitored)
                .With(s => s.Path = @"C:\Test\name".AsOsAgnostic())
                .With(s => s.RootFolderPath = "")
                .Build().ToList();
        }

        [Test]
        public void should_call_repo_updateMany()
        {
            Subject.UpdateMovie(_movies, false);

            Mocker.GetMock<IMovieRepository>().Verify(v => v.UpdateMany(_movies), Times.Once());
        }

        [Test]
        public void should_update_path_when_rootFolderPath_is_supplied()
        {
            var newRoot = @"C:\Test\TV2".AsOsAgnostic();
            _movies.ForEach(s => s.RootFolderPath = newRoot);

            Mocker.GetMock<IBuildMoviePaths>()
                .Setup(s => s.BuildPath(It.IsAny<Movie>(), false))
                .Returns<Movie, bool>((s, u) => Path.Combine(s.RootFolderPath, s.Title));

            Subject.UpdateMovie(_movies, false).ForEach(s => s.Path.Should().StartWith(newRoot));
        }

        [Test]
        public void should_not_update_path_when_rootFolderPath_is_empty()
        {
            Subject.UpdateMovie(_movies, false).ForEach(s =>
            {
                var expectedPath = _movies.Single(ser => ser.Id == s.Id).Path;
                s.Path.Should().Be(expectedPath);
            });
        }

        [Test]
        public void should_be_able_to_update_many_movies()
        {
            var movies = Builder<Movie>.CreateListOfSize(50)
                                        .All()
                                        .With(s => s.Path = (@"C:\Test\Movies\" + s.Path).AsOsAgnostic())
                                        .Build()
                                        .ToList();

            var newRoot = @"C:\Test\Movies2".AsOsAgnostic();
            movies.ForEach(s => s.RootFolderPath = newRoot);

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetMovieFolder(It.IsAny<Movie>(), (NamingConfig)null))
                  .Returns<Movie, NamingConfig>((s, n) => s.Title);

            Subject.UpdateMovie(movies, false);
        }
    }
}
