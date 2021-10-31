using System.Linq;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Trakt;
using NzbDrone.Core.Notifications.Trakt.Resource;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests
{
    [TestFixture]
    public class TraktServiceFixture : CoreTest<TraktService>
    {
        private DownloadMessage _downloadMessage;
        private TraktSettings _traktSettings;

        [SetUp]
        public void Setup()
        {
            _downloadMessage = new DownloadMessage
            {
                Movie = new Movie(),
                MovieFile = new MovieFile
                {
                    MediaInfo = null,
                    Quality = new QualityModel
                    {
                        Quality = Quality.Unknown
                    }
                }
            };

            _traktSettings = new TraktSettings
            {
                AccessToken = "",
                RefreshToken = ""
            };
        }

        private void GiventValidMediaInfo(Quality quality, string audioChannels, string audioFormat, string scanType)
        {
            _downloadMessage.MovieFile.MediaInfo = new MediaInfoModel
            {
                AudioChannelPositions = audioChannels,
                AudioFormat = audioFormat,
                ScanType = scanType
            };

            _downloadMessage.MovieFile.Quality.Quality = quality;
        }

        [Test]
        public void should_add_collection_movie_if_null_mediainfo()
        {
            Subject.AddMovieToCollection(_traktSettings, _downloadMessage.Movie, _downloadMessage.MovieFile);

            Mocker.GetMock<ITraktProxy>()
                  .Verify(v => v.AddToCollection(It.IsAny<TraktCollectMoviesResource>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_add_collection_movie_if_valid_mediainfo()
        {
            GiventValidMediaInfo(Quality.Bluray1080p, "5.1", "DTS", "Progressive");

            Subject.AddMovieToCollection(_traktSettings, _downloadMessage.Movie, _downloadMessage.MovieFile);

            Mocker.GetMock<ITraktProxy>()
                  .Verify(v => v.AddToCollection(It.Is<TraktCollectMoviesResource>(t =>
                    t.Movies.First().Audio == "dts" &&
                    t.Movies.First().AudioChannels == "5.1" &&
                    t.Movies.First().Resolution == "hd_1080p" &&
                    t.Movies.First().MediaType == "bluray"),
                  It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_format_audio_channels_to_one_decimal_when_adding_collection_movie()
        {
            GiventValidMediaInfo(Quality.Bluray1080p, "2.0", "DTS", "Progressive");

            Subject.AddMovieToCollection(_traktSettings, _downloadMessage.Movie, _downloadMessage.MovieFile);

            Mocker.GetMock<ITraktProxy>()
                  .Verify(v => v.AddToCollection(It.Is<TraktCollectMoviesResource>(t =>
                    t.Movies.First().Audio == "dts" &&
                    t.Movies.First().AudioChannels == "2.0" &&
                    t.Movies.First().Resolution == "hd_1080p" &&
                    t.Movies.First().MediaType == "bluray"),
                  It.IsAny<string>()), Times.Once());
        }
    }
}
