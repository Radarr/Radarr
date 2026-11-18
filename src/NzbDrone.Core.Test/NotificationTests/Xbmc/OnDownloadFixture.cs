using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc
{
    [TestFixture]
    public class OnDownloadFixture : CoreTest<Notifications.Xbmc.Xbmc>
    {
        private DownloadMessage _downloadMessage;

        [SetUp]
        public void Setup()
        {
            var movie = Builder<Movie>.CreateNew()
                                        .Build();

            var movieFile = Builder<MovieFile>.CreateNew()
                                                   .Build();

            _downloadMessage = Builder<DownloadMessage>.CreateNew()
                                                       .With(d => d.Movie = movie)
                                                       .With(d => d.MovieFile = movieFile)
                                                       .With(d => d.OldMovieFiles = new List<DeletedMovieFile>())
                                                       .Build();

            Subject.Definition = new NotificationDefinition();
            Subject.Definition.Settings = new XbmcSettings
                                          {
                                              Host = "localhost",
                                              UpdateLibrary = true
                                          };
        }

        private void GivenOldFiles()
        {
            _downloadMessage.OldMovieFiles = GivenDeletedMovieFiles();

            GivenSettings(updateLibrary: true, cleanLibrary: true);
        }

        private void GivenSettings(bool updateLibrary, bool cleanLibrary)
        {
            Subject.Definition.Settings = new XbmcSettings
                                          {
                                              Host = "localhost",
                                              UpdateLibrary = updateLibrary,
                                              CleanLibrary = cleanLibrary
                                          };
        }

        private static List<DeletedMovieFile> GivenDeletedMovieFiles()
        {
            return Builder<DeletedMovieFile>
                .CreateListOfSize(1)
                .All()
                .WithFactory(() => new DeletedMovieFile(Builder<MovieFile>.CreateNew().Build(), null))
                .Build()
                .ToList();
        }

        private DownloadMessage GivenDownloadMessage(int movieId, List<DeletedMovieFile> oldMovieFiles)
        {
            return Builder<DownloadMessage>.CreateNew()
                                           .With(d => d.Movie = Builder<Movie>.CreateNew().With(m => m.Id = movieId).Build())
                                           .With(d => d.MovieFile = _downloadMessage.MovieFile)
                                           .With(d => d.OldMovieFiles = oldMovieFiles)
                                           .Build();
        }

        [Test]
        public void should_not_clean_if_no_movie_was_replaced()
        {
            GivenSettings(updateLibrary: true, cleanLibrary: true);

            Subject.OnDownload(_downloadMessage);
            Subject.ProcessQueue();

            Mocker.GetMock<IXbmcService>().Verify(v => v.Clean(It.IsAny<XbmcSettings>()), Times.Never());
        }

        [Test]
        public void should_clean_if_movie_was_replaced()
        {
            GivenOldFiles();
            Subject.OnDownload(_downloadMessage);
            Subject.ProcessQueue();

            Mocker.GetMock<IXbmcService>().Verify(v => v.Clean(It.IsAny<XbmcSettings>()), Times.Once());
        }

        [Test]
        public void should_not_clean_if_clean_library_is_disabled()
        {
            GivenOldFiles();
            GivenSettings(updateLibrary: true, cleanLibrary: false);

            Subject.OnDownload(_downloadMessage);
            Subject.ProcessQueue();

            Mocker.GetMock<IXbmcService>().Verify(v => v.Clean(It.IsAny<XbmcSettings>()), Times.Never());
        }

        [Test]
        public void should_clean_once_after_updating_all_movies()
        {
            var calls = new List<string>();

            Mocker.GetMock<IXbmcService>()
                  .Setup(s => s.Update(It.IsAny<XbmcSettings>(), It.IsAny<Movie>()))
                  .Callback<XbmcSettings, Movie>((_, movie) => calls.Add($"update {movie.Id}"));

            Mocker.GetMock<IXbmcService>()
                  .Setup(s => s.Clean(It.IsAny<XbmcSettings>()))
                  .Callback(() => calls.Add("clean"));

            GivenOldFiles();
            Subject.OnDownload(_downloadMessage);
            Subject.OnDownload(GivenDownloadMessage(2, new List<DeletedMovieFile>()));
            Subject.OnDownload(GivenDownloadMessage(3, GivenDeletedMovieFiles()));
            Subject.ProcessQueue();

            calls.Should().Equal("update 1", "update 2", "update 3", "clean");
        }

        [Test]
        public void should_clean_once_if_update_library_is_disabled()
        {
            GivenOldFiles();
            GivenSettings(updateLibrary: false, cleanLibrary: true);

            Subject.OnDownload(_downloadMessage);
            Subject.OnDownload(GivenDownloadMessage(2, GivenDeletedMovieFiles()));
            Subject.ProcessQueue();

            Mocker.GetMock<IXbmcService>().Verify(v => v.Update(It.IsAny<XbmcSettings>(), It.IsAny<Movie>()), Times.Never());
            Mocker.GetMock<IXbmcService>().Verify(v => v.Clean(It.IsAny<XbmcSettings>()), Times.Once());
        }

        [Test]
        public void should_clean_if_update_is_unable_to_connect()
        {
            Mocker.GetMock<IXbmcService>()
                  .Setup(s => s.Update(It.IsAny<XbmcSettings>(), It.IsAny<Movie>()))
                  .Throws(new SocketException());

            GivenOldFiles();
            Subject.OnDownload(_downloadMessage);
            Subject.ProcessQueue();

            Mocker.GetMock<IXbmcService>().Verify(v => v.Clean(It.IsAny<XbmcSettings>()), Times.Once());
        }
    }
}
