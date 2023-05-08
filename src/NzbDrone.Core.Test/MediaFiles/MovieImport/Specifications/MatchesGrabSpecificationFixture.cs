using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.MovieImport.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Specifications
{
    [TestFixture]
    public class MatchesGrabSpecificationFixture : CoreTest<MatchesGrabSpecification>
    {
        private Movie _movie1;
        private Movie _movie2;
        private Movie _movie3;
        private LocalMovie _localMovie;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _movie1 = Builder<Movie>.CreateNew()
                .With(e => e.Id = 1)
                .Build();

            _movie2 = Builder<Movie>.CreateNew()
                .With(e => e.Id = 2)
                .Build();

            _movie3 = Builder<Movie>.CreateNew()
                .With(e => e.Id = 3)
                .Build();

            _localMovie = Builder<LocalMovie>.CreateNew()
                                                 .With(l => l.Path = @"C:\Test\Unsorted\Series.Title.S01E01.720p.HDTV-Sonarr\S01E05.mkv".AsOsAgnostic())
                                                 .With(l => l.Movie = _movie1)
                                                 .With(l => l.Release = null)
                                                 .Build();

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew().Build();
        }

        private void GivenHistoryForMovies(params Movie[] movies)
        {
            if (movies.Empty())
            {
                return;
            }

            var grabbedHistories = Builder<MovieHistory>.CreateListOfSize(movies.Length)
                .All()
                .With(h => h.EventType == MovieHistoryEventType.Grabbed)
                .BuildList();

            for (var i = 0; i < grabbedHistories.Count; i++)
            {
                grabbedHistories[i].MovieId = movies[i].Id;
            }

            _localMovie.Release = new GrabbedReleaseInfo(grabbedHistories);
        }

        [Test]
        public void should_be_accepted_for_existing_file()
        {
            _localMovie.ExistingFile = true;

            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_no_download_client_item()
        {
            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_no_grab_release_info()
        {
            GivenHistoryForMovies();

            Subject.IsSatisfiedBy(_localMovie, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_episode_matches_single_grab_release_info()
        {
            GivenHistoryForMovies(_movie1);

            Subject.IsSatisfiedBy(_localMovie, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_file_episode_does_not_match_single_grab_release_info()
        {
            GivenHistoryForMovies(_movie2);

            Subject.IsSatisfiedBy(_localMovie, _downloadClientItem).Accepted.Should().BeFalse();
        }
    }
}
