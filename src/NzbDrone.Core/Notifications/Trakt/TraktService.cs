using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications.Trakt.Resource;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Notifications.Trakt
{
    public interface ITraktService
    {
        HttpRequest GetOAuthRequest(string callbackUrl);
        TraktAuthRefreshResource RefreshAuthToken(string refreshToken);
        void AddMovieToCollection(TraktSettings settings, Movie movie, MovieFile movieFile);
        void RemoveMovieFromCollection(TraktSettings settings, Movie movie);
        string GetUserName(string accessToken);
        ValidationFailure Test(TraktSettings settings);
    }

    public class TraktService : ITraktService
    {
        private readonly ITraktProxy _proxy;
        private readonly Logger _logger;

        public TraktService(ITraktProxy proxy,
                           Logger logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public string GetUserName(string accessToken)
        {
            return _proxy.GetUserName(accessToken);
        }

        public HttpRequest GetOAuthRequest(string callbackUrl)
        {
            return _proxy.GetOAuthRequest(callbackUrl);
        }

        public TraktAuthRefreshResource RefreshAuthToken(string refreshToken)
        {
            return _proxy.RefreshAuthToken(refreshToken);
        }

        public ValidationFailure Test(TraktSettings settings)
        {
            try
            {
                GetUserName(settings.AccessToken);
                return null;
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "Access Token is invalid: " + ex.Message);
                    return new ValidationFailure("Token", "Access Token is invalid");
                }

                _logger.Error(ex, "Unable to send test message: " + ex.Message);
                return new ValidationFailure("Token", "Unable to send test message");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message: " + ex.Message);
                return new ValidationFailure("", "Unable to send test message");
            }
        }

        public void RemoveMovieFromCollection(TraktSettings settings, Movie movie)
        {
            var payload = new TraktCollectMoviesResource
            {
                Movies = new List<TraktCollectMovie>()
            };

            payload.Movies.Add(new TraktCollectMovie
            {
                Title = movie.Title,
                Year = movie.Year,
                Ids = new TraktMovieIdsResource
                {
                    Tmdb = movie.MovieMetadata.Value.TmdbId,
                    Imdb = movie.MovieMetadata.Value.ImdbId ?? "",
                }
            });

            _proxy.RemoveFromCollection(payload, settings.AccessToken);
        }

        public void AddMovieToCollection(TraktSettings settings, Movie movie, MovieFile movieFile)
        {
            var payload = new TraktCollectMoviesResource
            {
                Movies = new List<TraktCollectMovie>()
            };

            var traktResolution = MapResolution(movieFile.Quality.Quality.Resolution, movieFile.MediaInfo?.ScanType);
            var mediaType = MapMediaType(movieFile.Quality.Quality.Source);
            var audio = MapAudio(movieFile);
            var audioChannels = MapAudioChannels(movieFile, audio);

            payload.Movies.Add(new TraktCollectMovie
            {
                Title = movie.Title,
                Year = movie.Year,
                CollectedAt = DateTime.Now,
                Resolution = traktResolution,
                MediaType = mediaType,
                AudioChannels = audioChannels,
                Audio = audio,
                Ids = new TraktMovieIdsResource
                {
                    Tmdb = movie.MovieMetadata.Value.TmdbId,
                    Imdb = movie.MovieMetadata.Value.ImdbId ?? "",
                }
            });

            _proxy.AddToCollection(payload, settings.AccessToken);
        }

        private string MapMediaType(Source source)
        {
            var traktSource = string.Empty;

            switch (source)
            {
                case Source.BLURAY:
                    traktSource = "bluray";
                    break;
                case Source.WEBDL:
                    traktSource = "digital";
                    break;
                case Source.WEBRIP:
                    traktSource = "digital";
                    break;
                case Source.DVD:
                    traktSource = "dvd";
                    break;
                case Source.TV:
                    traktSource = "dvd";
                    break;
            }

            return traktSource;
        }

        private string MapResolution(int resolution, string scanType)
        {
            var traktResolution = string.Empty;
            var interlacedTypes = new string[] { "Interlaced", "MBAFF", "PAFF" };

            var scanIdentifier = scanType.IsNotNullOrWhiteSpace() && interlacedTypes.Contains(scanType) ? "i" : "p";

            switch (resolution)
            {
                case 2160:
                    traktResolution = "uhd_4k";
                    break;
                case 1080:
                    traktResolution = string.Format("hd_1080{0}", scanIdentifier);
                    break;
                case 720:
                    traktResolution = "hd_720p";
                    break;
                case 576:
                    traktResolution = string.Format("sd_576{0}", scanIdentifier);
                    break;
                case 480:
                    traktResolution = string.Format("sd_480{0}", scanIdentifier);
                    break;
            }

            return traktResolution;
        }

        private string MapAudio(MovieFile movieFile)
        {
            var traktAudioFormat = string.Empty;

            var audioCodec = movieFile.MediaInfo != null ? MediaInfoFormatter.FormatAudioCodec(movieFile.MediaInfo, movieFile.SceneName) : string.Empty;

            switch (audioCodec)
            {
                case "AC3":
                    traktAudioFormat = "dolby_digital";
                    break;
                case "EAC3":
                    traktAudioFormat = "dolby_digital_plus";
                    break;
                case "TrueHD":
                    traktAudioFormat = "dolby_truehd";
                    break;
                case "EAC3 Atmos":
                    traktAudioFormat = "dolby_digital_plus_atmos";
                    break;
                case "TrueHD Atmos":
                    traktAudioFormat = "dolby_atmos";
                    break;
                case "DTS":
                case "DTS-ES":
                    traktAudioFormat = "dts";
                    break;
                case "DTS-HD MA":
                    traktAudioFormat = "dts_ma";
                    break;
                case "DTS-HD HRA":
                    traktAudioFormat = "dts_hr";
                    break;
                case "DTS-X":
                    traktAudioFormat = "dts_x";
                    break;
                case "MP3":
                    traktAudioFormat = "mp3";
                    break;
                case "MP2":
                    traktAudioFormat = "mp2";
                    break;
                case "Vorbis":
                    traktAudioFormat = "ogg";
                    break;
                case "WMA":
                    traktAudioFormat = "wma";
                    break;
                case "AAC":
                    traktAudioFormat = "aac";
                    break;
                case "PCM":
                    traktAudioFormat = "lpcm";
                    break;
                case "FLAC":
                    traktAudioFormat = "flac";
                    break;
                case "Opus":
                    traktAudioFormat = "ogg_opus";
                    break;
            }

            return traktAudioFormat;
        }

        private string MapAudioChannels(MovieFile movieFile, string audioFormat)
        {
            var audioChannels = movieFile.MediaInfo != null ? MediaInfoFormatter.FormatAudioChannels(movieFile.MediaInfo).ToString("0.0") : string.Empty;

            if (audioChannels == "0.0")
            {
                audioChannels = string.Empty;
            }

            return audioChannels;
        }
    }
}
