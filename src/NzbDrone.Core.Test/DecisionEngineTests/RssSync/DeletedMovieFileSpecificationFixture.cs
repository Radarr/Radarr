using System;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]
    public class DeletedMovieFileSpecificationFixture : CoreTest<DeletedMovieFileSpecification>
    {
        private RemoteMovie _parseResult;
        private MovieFile _movieFile;

        [SetUp]
        public void Setup()
        {
            _movieFile = new MovieFile
            {
                Id = 1,
                RelativePath = "My.Movie.2026.mkv",
                Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)),
                DateAdded = DateTime.Now
            };

            var fakeMovie = Builder<Movie>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.Bluray1080p.Id })
                         .With(c => c.Path = @"C:\Movie\My.Movie".AsOsAgnostic())
                         .With(c => c.MovieFile = _movieFile)
                         .With(c => c.MovieFileId = 1)
                         .Build();

            _parseResult = new RemoteMovie
            {
                Movie = fakeMovie,
                ParsedMovieInfo = new ParsedMovieInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
            };

            GivenUnmonitorDeletedMovies(true);
        }

        private void GivenUnmonitorDeletedMovies(bool enabled)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(v => v.AutoUnmonitorPreviouslyDownloadedMovies)
                  .Returns(enabled);
        }

        private void WithExistingFile(MovieFile movieFile)
        {
            var path = Path.Combine(@"C:\Movie\My.Movie".AsOsAgnostic(), movieFile.RelativePath);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(path))
                  .Returns(true);
        }

        [Test]
        public void should_return_true_when_unmonitor_deleted_movies_is_off()
        {
            GivenUnmonitorDeletedMovies(false);

            Subject.IsSatisfiedBy(_parseResult, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_searching()
        {
            Subject.IsSatisfiedBy(_parseResult, new ReleaseDecisionInformation(false, new MovieSearchCriteria())).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_file_exists()
        {
            WithExistingFile(_movieFile);

            Subject.IsSatisfiedBy(_parseResult, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_file_is_missing()
        {
            Subject.IsSatisfiedBy(_parseResult, new()).Accepted.Should().BeFalse();
        }
    }
}
