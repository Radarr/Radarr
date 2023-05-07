using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MovieTests
{
    [TestFixture]
    public class MovieFolderPathBuilderFixture : CoreTest<MoviePathBuilder>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .With(s => s.Title = "Movie Title")
                                     .With(s => s.Path = @"C:\Test\Movies\Movie.Title".AsOsAgnostic())
                                     .With(s => s.RootFolderPath = null)
                                     .Build();
        }

        public void GivenMovieFolderName(string name)
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetMovieFolder(_movie, null))
                  .Returns(name);
        }

        public void GivenExistingRootFolder(string rootFolder)
        {
            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>(), null))
                  .Returns(rootFolder);
        }

        [Test]
        public void should_create_new_movie_path()
        {
            var rootFolder = @"C:\Test\Movies2".AsOsAgnostic();

            GivenMovieFolderName(_movie.Title);
            _movie.RootFolderPath = rootFolder;

            Subject.BuildPath(_movie, false).Should().Be(Path.Combine(rootFolder, _movie.Title));
        }

        [Test]
        public void should_reuse_existing_relative_folder_name()
        {
            var folderName = Path.GetFileName(_movie.Path);
            var rootFolder = @"C:\Test\Movies2".AsOsAgnostic();

            GivenExistingRootFolder(Path.GetDirectoryName(_movie.Path));
            GivenMovieFolderName(_movie.Title);
            _movie.RootFolderPath = rootFolder;

            Subject.BuildPath(_movie, true).Should().Be(Path.Combine(rootFolder, folderName));
        }

        [Test]
        public void should_reuse_existing_relative_folder_structure()
        {
            var existingRootFolder = @"C:\Test\Movies".AsOsAgnostic();
            var existingRelativePath = @"M\Movie.Title";
            var rootFolder = @"C:\Test\Movies2".AsOsAgnostic();

            GivenExistingRootFolder(existingRootFolder);
            GivenMovieFolderName(_movie.Title);
            _movie.RootFolderPath = rootFolder;
            _movie.Path = Path.Combine(existingRootFolder, existingRelativePath);

            Subject.BuildPath(_movie, true).Should().Be(Path.Combine(rootFolder, existingRelativePath));
        }

        [Test]
        public void should_use_built_path_for_new_movie()
        {
            var rootFolder = @"C:\Test\Movies2".AsOsAgnostic();

            GivenMovieFolderName(_movie.Title);
            _movie.RootFolderPath = rootFolder;
            _movie.Path = null;

            Subject.BuildPath(_movie, true).Should().Be(Path.Combine(rootFolder, _movie.Title));
        }
    }
}
