using System;
using System.Collections.Generic;
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
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Trakt
{
    public class Trakt : NotificationBase<TraktSettings>
    {
        private readonly ITraktProxy _proxy;
        private readonly INotificationRepository _notificationRepository;
        private readonly Logger _logger;

        public Trakt(ITraktProxy proxy, INotificationRepository notificationRepository, Logger logger)
        {
            _proxy = proxy;
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public override string Link => "https://trakt.tv/";
        public override string Name => "Trakt";

        public override void OnDownload(DownloadMessage message)
        {
            RefreshTokenIfNecessary();
            AddMovieToCollection(Settings, message.Movie, message.MovieFile);
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            RefreshTokenIfNecessary();
            RemoveMovieFromCollection(Settings, deleteMessage.Movie);
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            RefreshTokenIfNecessary();
            RemoveMovieFromCollection(Settings, deleteMessage.Movie);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            RefreshTokenIfNecessary();

            try
            {
                _proxy.GetUserName(Settings.AccessToken);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "Access Token is invalid: " + ex.Message);

                    failures.Add(new ValidationFailure("Token", "Access Token is invalid"));
                }
                else
                {
                    _logger.Error(ex, "Unable to send test message: " + ex.Message);

                    failures.Add(new ValidationFailure("Token", "Unable to send test message"));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message: " + ex.Message);

                failures.Add(new ValidationFailure("", "Unable to send test message"));
            }

            return new ValidationResult(failures);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                var request = _proxy.GetOAuthRequest(query["callbackUrl"]);

                return new
                {
                    OauthUrl = request.Url.ToString()
                };
            }
            else if (action == "getOAuthToken")
            {
                return new
                {
                    accessToken = query["access_token"],
                    expires = DateTime.UtcNow.AddSeconds(int.Parse(query["expires_in"])),
                    refreshToken = query["refresh_token"],
                    authUser = _proxy.GetUserName(query["access_token"])
                };
            }

            return new { };
        }

        private void RefreshTokenIfNecessary()
        {
            if (Settings.Expires < DateTime.UtcNow.AddMinutes(5))
            {
                RefreshToken();
            }
        }

        private void RefreshToken()
        {
            _logger.Trace("Refreshing Token");

            Settings.Validate().Filter("RefreshToken").ThrowOnError();

            try
            {
                var response = _proxy.RefreshAuthToken(Settings.RefreshToken);

                if (response != null)
                {
                    var token = response;

                    Settings.AccessToken = token.AccessToken;
                    Settings.Expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
                    Settings.RefreshToken = token.RefreshToken ?? Settings.RefreshToken;

                    if (Definition.Id > 0)
                    {
                        _notificationRepository.UpdateSettings((NotificationDefinition)Definition);
                    }
                }
            }
            catch (HttpException ex)
            {
                _logger.Warn(ex, "Error refreshing trakt access token");
            }
        }

        private void AddMovieToCollection(TraktSettings settings, Movie movie, MovieFile movieFile)
        {
            var payload = new TraktCollectMoviesResource
            {
                Movies = new List<TraktCollectMovie>()
            };

            var traktResolution = MapResolution(movieFile.Quality.Quality.Resolution, movieFile.MediaInfo?.ScanType);
            var mediaType = MapMediaType(movieFile.Quality.Quality.Source);
            var audio = MapAudio(movieFile);
            var audioChannels = MapAudioChannels(movieFile);

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

        private void RemoveMovieFromCollection(TraktSettings settings, Movie movie)
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

        private string MapMediaType(QualitySource source)
        {
            var traktSource = string.Empty;

            switch (source)
            {
                case QualitySource.BLURAY:
                    traktSource = "bluray";
                    break;
                case QualitySource.WEBDL:
                    traktSource = "digital";
                    break;
                case QualitySource.WEBRIP:
                    traktSource = "digital";
                    break;
                case QualitySource.DVD:
                    traktSource = "dvd";
                    break;
                case QualitySource.TV:
                    traktSource = "dvd";
                    break;
            }

            return traktSource;
        }

        private string MapResolution(int resolution, string scanType)
        {
            var traktResolution = string.Empty;

            var scanIdentifier = scanType.IsNotNullOrWhiteSpace() && TraktInterlacedTypes.interlacedTypes.Contains(scanType) ? "i" : "p";

            switch (resolution)
            {
                case 2160:
                    traktResolution = "uhd_4k";
                    break;
                case 1080:
                    traktResolution = $"hd_1080{scanIdentifier}";
                    break;
                case 720:
                    traktResolution = "hd_720p";
                    break;
                case 576:
                    traktResolution = $"sd_576{scanIdentifier}";
                    break;
                case 480:
                    traktResolution = $"sd_480{scanIdentifier}";
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

        private string MapAudioChannels(MovieFile movieFile)
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
