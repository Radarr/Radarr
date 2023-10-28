using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
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
    public class AlreadyImportedSpecificationFixture : CoreTest<AlreadyImportedSpecification>
    {
        private Movie _movie;
        private LocalMovie _localMovie;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                .With(s => s.Path = @"C:\Test\Movies\Casablanca".AsOsAgnostic())
                .Build();

            _localMovie = new LocalMovie
            {
                Path = @"C:\Test\Unsorted\Casablanca\Casablanca.1942.avi".AsOsAgnostic(),
                Movie = _movie
            };

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew()
                .Build();
        }

        private void GivenHistory(List<MovieHistory> history)
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.GetByMovieId(It.IsAny<int>(), null))
                .Returns(history);
        }

        [Test]
        public void should_accepted_if_download_client_item_is_null()
        {
            Subject.IsSatisfiedBy(_localMovie, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_episode_does_not_have_file()
        {
            _movie.MovieFileId = 0;

            Subject.IsSatisfiedBy(_localMovie, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_episode_has_not_been_imported()
        {
            var history = Builder<MovieHistory>.CreateListOfSize(1)
                .All()
                .With(h => h.MovieId = _movie.Id)
                .With(h => h.EventType = MovieHistoryEventType.Grabbed)
                .Build()
                .ToList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localMovie, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_episode_was_grabbed_after_being_imported()
        {
            var history = Builder<MovieHistory>.CreateListOfSize(3)
                .All()
                .With(h => h.MovieId = _movie.Id)
                .TheFirst(1)
                .With(h => h.EventType = MovieHistoryEventType.Grabbed)
                .With(h => h.Date = DateTime.UtcNow)
                .TheNext(1)
                .With(h => h.EventType = MovieHistoryEventType.DownloadFolderImported)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-1))
                .TheNext(1)
                .With(h => h.EventType = MovieHistoryEventType.Grabbed)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-2))
                .Build()
                .ToList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localMovie, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_if_episode_imported_after_being_grabbed()
        {
            var history = Builder<MovieHistory>.CreateListOfSize(2)
                .All()
                .With(h => h.MovieId = _movie.Id)
                .TheFirst(1)
                .With(h => h.EventType = MovieHistoryEventType.DownloadFolderImported)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-1))
                .TheNext(1)
                .With(h => h.EventType = MovieHistoryEventType.Grabbed)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-2))
                .Build()
                .ToList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localMovie, _downloadClientItem).Accepted.Should().BeFalse();
        }
    }
}
