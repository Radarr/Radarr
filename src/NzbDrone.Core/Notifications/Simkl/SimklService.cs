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
using NzbDrone.Core.Notifications.Simkl.Resource;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Notifications.Simkl
{
    public interface ISimklService
    {
        HttpRequest GetOAuthRequest(string callbackUrl);
        SimklAuthRefreshResource RefreshAuthToken(string refreshToken);
        void AddMovieToCollection(SimklSettings settings, Movie movie, MovieFile movieFile);
        void RemoveMovieFromCollection(SimklSettings settings, Movie movie, MovieFile movieFile);
        string GetUserName(string accessToken);
        ValidationFailure Test(SimklSettings settings);
    }

    public class SimklService : ISimklService
    {
        private readonly ISimklProxy _proxy;
        private readonly Logger _logger;

        public SimklService(ISimklProxy proxy,
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

        public SimklAuthRefreshResource RefreshAuthToken(string refreshToken)
        {
            return _proxy.RefreshAuthToken(refreshToken);
        }

        public ValidationFailure Test(SimklSettings settings)
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

        public void RemoveMovieFromCollection(SimklSettings settings, Movie movie, MovieFile movieFile)
        {
            var payload = new SimklCollectMoviesResource
            {
                Movies = new List<SimklCollectMovie>()
            };

            payload.Movies.Add(new SimklCollectMovie
            {
                Title = movie.Title,
                Year = movie.Year,
                Ids = new SimklMovieIdsResource
                {
                    Tmdb = movie.TmdbId,
                    Imdb = movie.ImdbId ?? "",
                }
            });

            _proxy.RemoveFromCollection(payload, settings.AccessToken);
        }

        public void AddMovieToCollection(SimklSettings settings, Movie movie, MovieFile movieFile)
        {
            var payload = new SimklCollectMoviesResource
            {
                Movies = new List<SimklCollectMovie>()
            };

            payload.Movies.Add(new SimklCollectMovie
            {
                Title = movie.Title,
                Year = movie.Year,
                CollectedAt = DateTime.Now,
                Ids = new SimklMovieIdsResource
                {
                    Tmdb = movie.TmdbId,
                    Imdb = movie.ImdbId ?? "",
                }
            });

            _proxy.AddToCollection(payload, settings.AccessToken);
        }

        private string MapMediaType(Source source)
        {
            var simklSource = string.Empty;

            switch (source)
            {
                case Source.BLURAY:
                    simklSource = "bluray";
                    break;
                case Source.WEBDL:
                    simklSource = "digital";
                    break;
                case Source.WEBRIP:
                    simklSource = "digital";
                    break;
                case Source.DVD:
                    simklSource = "dvd";
                    break;
                case Source.TV:
                    simklSource = "dvd";
                    break;
            }

            return simklSource;
        }

        private string MapResolution(int resolution, string scanType)
        {
            var simklResolution = string.Empty;
            var interlacedTypes = new string[] { "Interlaced", "MBAFF", "PAFF" };

            var scanIdentifier = scanType.IsNotNullOrWhiteSpace() && interlacedTypes.Contains(scanType) ? "i" : "p";

            switch (resolution)
            {
                case 2160:
                    simklResolution = "uhd_4k";
                    break;
                case 1080:
                    simklResolution = string.Format("hd_1080{0}", scanIdentifier);
                    break;
                case 720:
                    simklResolution = "hd_720p";
                    break;
                case 576:
                    simklResolution = string.Format("sd_576{0}", scanIdentifier);
                    break;
                case 480:
                    simklResolution = string.Format("sd_480{0}", scanIdentifier);
                    break;
            }

            return simklResolution;
        }

        private string MapAudio(MovieFile movieFile)
        {
            var simklAudioFormat = string.Empty;

            var audioCodec = movieFile.MediaInfo != null ? MediaInfoFormatter.FormatAudioCodec(movieFile.MediaInfo, movieFile.SceneName) : string.Empty;

            switch (audioCodec)
            {
                case "AC3":
                    simklAudioFormat = "dolby_digital";
                    break;
                case "EAC3":
                    simklAudioFormat = "dolby_digital_plus";
                    break;
                case "TrueHD":
                    simklAudioFormat = "dolby_truehd";
                    break;
                case "EAC3 Atmos":
                    simklAudioFormat = "dolby_digital_plus_atmos";
                    break;
                case "TrueHD Atmos":
                    simklAudioFormat = "dolby_atmos";
                    break;
                case "DTS":
                case "DTS-ES":
                    simklAudioFormat = "dts";
                    break;
                case "DTS-HD MA":
                    simklAudioFormat = "dts_ma";
                    break;
                case "DTS-HD HRA":
                    simklAudioFormat = "dts_hr";
                    break;
                case "DTS-X":
                    simklAudioFormat = "dts_x";
                    break;
                case "MP3":
                    simklAudioFormat = "mp3";
                    break;
                case "MP2":
                    simklAudioFormat = "mp2";
                    break;
                case "Vorbis":
                    simklAudioFormat = "ogg";
                    break;
                case "WMA":
                    simklAudioFormat = "wma";
                    break;
                case "AAC":
                    simklAudioFormat = "aac";
                    break;
                case "PCM":
                    simklAudioFormat = "lpcm";
                    break;
                case "FLAC":
                    simklAudioFormat = "flac";
                    break;
                case "Opus":
                    simklAudioFormat = "ogg_opus";
                    break;
            }

            return simklAudioFormat;
        }

        private string MapAudioChannels(MovieFile movieFile, string audioFormat)
        {
            var audioChannels = movieFile.MediaInfo != null ? MediaInfoFormatter.FormatAudioChannels(movieFile.MediaInfo).ToString("0.0") : string.Empty;

            // Map cases where Radarr doesn't handle MI correctly, can purge once mediainfo handling is improved
            if (audioChannels == "8.0")
            {
                audioChannels = "7.1";
            }
            else if (audioChannels == "6.0" && audioFormat == "dts_ma")
            {
                audioChannels = "7.1";
            }
            else if (audioChannels == "6.0" && audioFormat != "dts_ma")
            {
                audioChannels = "5.1";
            }
            else if (audioChannels == "0.0")
            {
                audioChannels = string.Empty;
            }

            return audioChannels;
        }
    }
}
