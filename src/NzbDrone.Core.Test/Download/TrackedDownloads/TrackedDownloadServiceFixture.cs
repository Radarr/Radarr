using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.History;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.TorrentRss;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download.TrackedDownloads
{
    [TestFixture]
    public class TrackedDownloadServiceFixture : CoreTest<TrackedDownloadService>
    {
        [SetUp]
        public void Setup()
        {
        }

        private void GivenDownloadHistory()
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(It.Is<string>(sr => sr == "35238")))
                .Returns(new List<MovieHistory>()
                {
                    new MovieHistory()
                    {
                        DownloadId = "35238",
                        SourceTitle = "TV Series S01",
                        MovieId = 3,
                    }
                });
        }

        private static DownloadClientDefinition CreateDownloadClient()
        {
            return new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Usenet
            };
        }

        private static DownloadClientItem CreateDownloadItem(DownloadItemStatus status)
        {
            return new DownloadClientItem()
            {
                Title = "A Movie 1998",
                DownloadId = "35238",
                Category = "radarr",
                TotalSize = 1000,
                RemainingSize = 500,
                Status = status,
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Id = 1,
                    Type = "NZBGet",
                    Name = "NZBGet",
                    Protocol = DownloadProtocol.Usenet
                }
            };
        }

        private void GivenTrackedDownloadCanBeMapped()
        {
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<MovieHistory>());

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedMovieInfo>(), It.IsAny<string>(), It.IsAny<int>(), null))
                  .Returns(new RemoteMovie
                  {
                      Release = new ReleaseInfo { Title = "A Movie 1998" },
                      Movie = new Movie() { Id = 3 },
                      ParsedMovieInfo = new ParsedMovieInfo()
                      {
                          MovieTitles = new List<string> { "A Movie" },
                          Year = 1998
                      }
                  });
        }

        [TestCase(DownloadItemStatus.Queued)]
        [TestCase(DownloadItemStatus.Paused)]
        public void should_reuse_stable_waiting_downloading_tracked_download(DownloadItemStatus status)
        {
            GivenTrackedDownloadCanBeMapped();

            var client = CreateDownloadClient();
            var item = CreateDownloadItem(status);
            var updatedItem = CreateDownloadItem(status);
            updatedItem.RemainingSize = 250;

            var trackedDownload = Subject.TrackDownload(client, item);
            var refreshedTrackedDownload = Subject.TrackDownload(client, updatedItem);

            trackedDownload.State.Should().Be(TrackedDownloadState.Downloading);
            refreshedTrackedDownload.Should().BeSameAs(trackedDownload);
            refreshedTrackedDownload.DownloadItem.Should().BeSameAs(updatedItem);

            Mocker.GetMock<IHistoryService>()
                  .Verify(s => s.FindByDownloadId(It.IsAny<string>()), Times.Once());

            Mocker.GetMock<IParsingService>()
                  .Verify(s => s.Map(It.IsAny<ParsedMovieInfo>(), It.IsAny<string>(), It.IsAny<int>(), null), Times.Once());
        }

        [Test]
        public void should_reprocess_when_waiting_download_starts_downloading()
        {
            GivenTrackedDownloadCanBeMapped();

            var client = CreateDownloadClient();
            var item = CreateDownloadItem(DownloadItemStatus.Queued);
            var updatedItem = CreateDownloadItem(DownloadItemStatus.Downloading);

            Subject.TrackDownload(client, item);
            Subject.TrackDownload(client, updatedItem);

            Mocker.GetMock<IHistoryService>()
                  .Verify(s => s.FindByDownloadId(It.IsAny<string>()), Times.Exactly(2));

            Mocker.GetMock<IParsingService>()
                  .Verify(s => s.Map(It.IsAny<ParsedMovieInfo>(), It.IsAny<string>(), It.IsAny<int>(), null), Times.Exactly(2));
        }

        [Test]
        public void should_reprocess_when_waiting_download_identity_changes()
        {
            GivenTrackedDownloadCanBeMapped();

            var client = CreateDownloadClient();
            var item = CreateDownloadItem(DownloadItemStatus.Queued);
            var updatedItem = CreateDownloadItem(DownloadItemStatus.Queued);
            updatedItem.TotalSize = 2000;

            Subject.TrackDownload(client, item);
            Subject.TrackDownload(client, updatedItem);

            Mocker.GetMock<IHistoryService>()
                  .Verify(s => s.FindByDownloadId(It.IsAny<string>()), Times.Exactly(2));

            Mocker.GetMock<IParsingService>()
                  .Verify(s => s.Map(It.IsAny<ParsedMovieInfo>(), It.IsAny<string>(), It.IsAny<int>(), null), Times.Exactly(2));
        }

        [Test]
        public void should_reprocess_when_waiting_download_has_warning_status()
        {
            GivenTrackedDownloadCanBeMapped();

            var client = CreateDownloadClient();
            var item = CreateDownloadItem(DownloadItemStatus.Queued);
            var updatedItem = CreateDownloadItem(DownloadItemStatus.Queued);
            updatedItem.RemainingSize = 250;

            var trackedDownload = Subject.TrackDownload(client, item);
            trackedDownload.Warn("Temporary warning");

            var refreshedTrackedDownload = Subject.TrackDownload(client, updatedItem);

            refreshedTrackedDownload.Should().NotBeSameAs(trackedDownload);

            Mocker.GetMock<IHistoryService>()
                  .Verify(s => s.FindByDownloadId(It.IsAny<string>()), Times.Exactly(2));

            Mocker.GetMock<IParsingService>()
                  .Verify(s => s.Map(It.IsAny<ParsedMovieInfo>(), It.IsAny<string>(), It.IsAny<int>(), null), Times.Exactly(2));
        }

        [Test]
        public void should_reprocess_when_waiting_download_is_not_mapped()
        {
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<MovieHistory>());

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedMovieInfo>(), It.IsAny<string>(), It.IsAny<int>(), null))
                  .Returns(new RemoteMovie
                  {
                      ParsedMovieInfo = new ParsedMovieInfo
                      {
                          MovieTitles = new List<string> { "A Movie" },
                          Year = 1998
                      }
                  });

            var client = CreateDownloadClient();
            var item = CreateDownloadItem(DownloadItemStatus.Queued);
            var updatedItem = CreateDownloadItem(DownloadItemStatus.Queued);
            updatedItem.RemainingSize = 250;

            var trackedDownload = Subject.TrackDownload(client, item);
            var refreshedTrackedDownload = Subject.TrackDownload(client, updatedItem);

            refreshedTrackedDownload.Should().NotBeSameAs(trackedDownload);

            Mocker.GetMock<IHistoryService>()
                  .Verify(s => s.FindByDownloadId(It.IsAny<string>()), Times.Exactly(2));

            Mocker.GetMock<IParsingService>()
                  .Verify(s => s.Map(It.IsAny<ParsedMovieInfo>(), It.IsAny<string>(), It.IsAny<int>(), null), Times.Exactly(2));
        }

        [Test]
        public void should_track_downloads_using_the_source_title_if_it_cannot_be_found_using_the_download_title()
        {
            GivenDownloadHistory();

            var remoteMovie = new RemoteMovie
            {
                Movie = new Movie() { Id = 3 },

                ParsedMovieInfo = new ParsedMovieInfo()
                {
                    MovieTitles = new List<string> { "A Movie" },
                    Year = 1998
                }
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.Is<ParsedMovieInfo>(i => i.PrimaryMovieTitle == "A Movie"), It.IsAny<string>(), It.IsAny<int>(), null))
                  .Returns(remoteMovie);

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "A Movie 1998",
                DownloadId = "35238",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Protocol = client.Protocol,
                    Id = client.Id,
                    Name = client.Name
                }
            };

            var trackedDownload = Subject.TrackDownload(client, item);

            trackedDownload.Should().NotBeNull();
            trackedDownload.RemoteMovie.Should().NotBeNull();
            trackedDownload.RemoteMovie.Movie.Should().NotBeNull();
            trackedDownload.RemoteMovie.Movie.Id.Should().Be(3);
        }

        [Test]
        public void should_set_indexer()
        {
            var episodeHistory = new MovieHistory()
            {
                DownloadId = "35238",
                SourceTitle = "TV Series S01",
                MovieId = 3,
                EventType = MovieHistoryEventType.Grabbed,
            };
            episodeHistory.Data.Add("indexer", "MyIndexer (Prowlarr)");
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(It.Is<string>(sr => sr == "35238")))
                .Returns(new List<MovieHistory>()
                {
                    episodeHistory
                });

            var indexerDefinition = new IndexerDefinition
            {
                Id = 1,
                Name = "MyIndexer (Prowlarr)",
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Get(indexerDefinition.Id))
                .Returns(indexerDefinition);
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.All())
                .Returns(new List<IndexerDefinition>() { indexerDefinition });

            var remoteEpisode = new RemoteMovie
            {
                Movie = new Movie() { Id = 3 },
                ParsedMovieInfo = new ParsedMovieInfo()
                {
                    MovieTitles = new List<string> { "A Movie" },
                    Year = 1998
                }
            };

            Mocker.GetMock<IParsingService>()
                .Setup(s => s.Map(It.IsAny<ParsedMovieInfo>(), It.IsAny<string>(), It.IsAny<int>(), null))
                .Returns(remoteEpisode);

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "A Movie 1998",
                DownloadId = "35238",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Protocol = client.Protocol,
                    Id = client.Id,
                    Name = client.Name
                }
            };

            var trackedDownload = Subject.TrackDownload(client, item);

            trackedDownload.Should().NotBeNull();
            trackedDownload.RemoteMovie.Should().NotBeNull();
            trackedDownload.RemoteMovie.Release.Should().NotBeNull();
            trackedDownload.RemoteMovie.Release.Indexer.Should().Be("MyIndexer (Prowlarr)");
        }

        [Test]
        public void should_unmap_tracked_download_if_movie_deleted()
        {
            GivenDownloadHistory();

            var remoteMovie = new RemoteMovie
            {
                Movie = new Movie() { Id = 3 },

                ParsedMovieInfo = new ParsedMovieInfo()
                {
                    MovieTitles = { "A Movie" },
                    Year = 1998
                }
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedMovieInfo>(), It.IsAny<string>(), It.IsAny<int>(), null))
                  .Returns(remoteMovie);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<MovieHistory>());

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "A Movie 1998",
                DownloadId = "12345",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Id = 1,
                    Type = "Blackhole",
                    Name = "Blackhole Client",
                    Protocol = DownloadProtocol.Torrent
                }
            };

            Subject.TrackDownload(client, item);
            Subject.GetTrackedDownloads().Should().HaveCount(1);

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedMovieInfo>(), It.IsAny<string>(), It.IsAny<int>(), null))
                  .Returns(default(RemoteMovie));

            Subject.Handle(new MoviesDeletedEvent(new List<Movie> { remoteMovie.Movie }, false, false));

            var trackedDownloads = Subject.GetTrackedDownloads();
            trackedDownloads.Should().HaveCount(1);
            trackedDownloads.First().RemoteMovie.Should().BeNull();
        }

        [Test]
        public void should_update_tracked_download_when_movie_edited()
        {
            var originalMovie = new Movie { Id = 3, TmdbId = 10, Title = "A Movie" };
            var updatedMovie = new Movie { Id = 3, TmdbId = 10, Title = "A Movie Updated" };

            var remoteMovie = new RemoteMovie
            {
                Movie = originalMovie,
                ParsedMovieInfo = new ParsedMovieInfo
                {
                    MovieTitles = { "A Movie" },
                    Year = 1998
                }
            };

            var updatedRemoteMovie = new RemoteMovie
            {
                Movie = updatedMovie,
                ParsedMovieInfo = new ParsedMovieInfo
                {
                    MovieTitles = { "A Movie" },
                    Year = 1998
                }
            };

            Mocker.GetMock<IParsingService>()
                  .SetupSequence(s => s.Map(It.IsAny<ParsedMovieInfo>(), It.IsAny<string>(), It.IsAny<int>(), null))
                  .Returns(remoteMovie)
                  .Returns(updatedRemoteMovie);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<MovieHistory>());

            var client = new DownloadClientDefinition
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem
            {
                Title = "A Movie 1998",
                DownloadId = "12345",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Id = 1,
                    Type = "Blackhole",
                    Name = "Blackhole Client",
                    Protocol = DownloadProtocol.Torrent
                }
            };

            Subject.TrackDownload(client, item);

            Subject.Handle(new MovieEditedEvent(updatedMovie, originalMovie));

            var trackedDownloads = Subject.GetTrackedDownloads();
            trackedDownloads.Should().HaveCount(1);
            trackedDownloads.First().RemoteMovie.Should().BeSameAs(updatedRemoteMovie);
            trackedDownloads.First().RemoteMovie.Movie.Title.Should().Be("A Movie Updated");
        }

        [Test]
        public void should_not_throw_when_processing_deleted_movie()
        {
            GivenDownloadHistory();

            var remoteMovie = new RemoteMovie
            {
                Movie = new Movie() { Id = 3 },

                ParsedMovieInfo = new ParsedMovieInfo()
                {
                    MovieTitles = { "A Movie" },
                    Year = 1998
                }
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedMovieInfo>(), It.IsAny<string>(), It.IsAny<int>(), null))
                  .Returns(default(RemoteMovie));

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<MovieHistory>());

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "A Movie 1998",
                DownloadId = "12345",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Id = 1,
                    Type = "Blackhole",
                    Name = "Blackhole Client",
                    Protocol = DownloadProtocol.Torrent
                }
            };

            Subject.TrackDownload(client, item);
            Subject.GetTrackedDownloads().Should().HaveCount(1);

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedMovieInfo>(), It.IsAny<string>(), It.IsAny<int>(), null))
                  .Returns(default(RemoteMovie));

            Subject.Handle(new MoviesDeletedEvent(new List<Movie> { remoteMovie.Movie }, false, false));

            var trackedDownloads = Subject.GetTrackedDownloads();
            trackedDownloads.Should().HaveCount(1);
            trackedDownloads.First().RemoteMovie.Should().BeNull();
        }

        [Test]
        public void should_track_downloads_using_the_movie_id_for_already_imported_downloads()
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(It.Is<string>(sr => sr == "35238")))
                .Returns([]);

            Mocker.GetMock<IDownloadHistoryService>()
                .Setup(s => s.GetLatestDownloadHistoryItem(It.Is<string>(sr => sr == "35238")))
                .Returns(new DownloadHistory
                {
                    MovieId = 5,
                    EventType = DownloadHistoryEventType.DownloadImported,
                });

            var remoteMovie = new RemoteMovie
            {
                Movie = new Movie { Id = 5 },
                ParsedMovieInfo = new ParsedMovieInfo
                {
                    MovieTitles = { "A Movie" },
                    Year = 1998
                },
            };

            Mocker.GetMock<IParsingService>()
                .Setup(s => s.Map(It.Is<ParsedMovieInfo>(i => i.Year == 1998 && i.MovieTitle == "A Movie"), It.IsAny<int>()))
                .Returns(remoteMovie);

            var client = new DownloadClientDefinition
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem
            {
                Title = "A Movie 1998",
                DownloadId = "35238",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Protocol = client.Protocol,
                    Id = client.Id,
                    Name = client.Name
                }
            };

            var trackedDownload = Subject.TrackDownload(client, item);

            trackedDownload.Should().NotBeNull();
            trackedDownload.RemoteMovie.Should().NotBeNull();
            trackedDownload.RemoteMovie.Movie.Should().NotBeNull();
            trackedDownload.RemoteMovie.Movie.Id.Should().Be(5);
            trackedDownload.RemoteMovie.ParsedMovieInfo.Year.Should().Be(1998);
        }
    }
}
