using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Versions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class MovieFileVersionExtensionsFixture : CoreTest
    {
        private Movie _movie;
        private MovieFile _movieFile;
        private MovieFile _versionFile;
        private MovieFile _orphanFile;

        [SetUp]
        public void Setup()
        {
            _movie = new Movie { Id = 1, Path = "/Movies/Movie (2022)" };
            _movieFile = new MovieFile { Id = 1, MovieId = 1, MovieVersionId = 0 };
            _versionFile = new MovieFile { Id = 2, MovieId = 1, MovieVersionId = 7 };
            _orphanFile = new MovieFile { Id = 3, MovieId = 1, MovieVersionId = 99 };
        }

        private List<MovieFile> AllFiles => new List<MovieFile> { _movieFile, _versionFile, _orphanFile };

        [Test]
        public void movie_should_get_every_file_not_belonging_to_an_additional_version()
        {
            AllFiles.ForVersionOf(_movie, new[] { 7 }).Should().BeEquivalentTo(new[] { _movieFile, _orphanFile });
        }

        [Test]
        public void version_view_should_only_get_its_own_files()
        {
            var versionView = _movie.ForVersion(new MovieVersion { Id = 7, MovieId = 1, Path = "/Movies 4K/Movie (2022)" });

            AllFiles.ForVersionOf(versionView, new[] { 7 }).Should().BeEquivalentTo(new[] { _versionFile });
        }

        [Test]
        public void should_treat_files_of_unknown_versions_as_primary()
        {
            _orphanFile.GetVersionId(new[] { 7 }).Should().Be(0);
            _versionFile.GetVersionId(new[] { 7 }).Should().Be(7);
        }
    }
}
