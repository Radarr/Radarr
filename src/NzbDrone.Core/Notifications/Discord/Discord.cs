using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications.Discord.Payloads;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Discord
{
    public class Discord : NotificationBase<DiscordSettings>
    {
        private readonly IDiscordProxy _proxy;
        private readonly ITagRepository _tagRepository;
        private readonly ILocalizationService _localizationService;

        public Discord(IDiscordProxy proxy, ITagRepository tagRepository, ILocalizationService localizationService)
        {
            _proxy = proxy;
            _tagRepository = tagRepository;
            _localizationService = localizationService;
        }

        public override string Name => "Discord";
        public override string Link => "https://support.discordapp.com/hc/en-us/articles/228383668-Intro-to-Webhooks";

        public override void OnGrab(GrabMessage message)
        {
            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Radarr/Radarr/develop/Logo/256.png"
                },
                Url = $"https://www.themoviedb.org/movie/{message.Movie.MovieMetadata.Value.TmdbId}",
                Description = "Movie Grabbed",
                Title = GetTitle(message.Movie),
                Color = (int)DiscordColors.Standard,
                Fields = new List<DiscordField>(),
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            if (Settings.GrabFields.Contains((int)DiscordGrabFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = message.Movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl
                };
            }

            if (Settings.GrabFields.Contains((int)DiscordGrabFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = message.Movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.RemoteUrl
                };
            }

            foreach (var field in Settings.GrabFields)
            {
                var discordField = new DiscordField();

                switch ((DiscordGrabFieldType)field)
                {
                    case DiscordGrabFieldType.Overview:
                        var overview = message.Movie.MovieMetadata.Value.Overview ?? "";
                        discordField.Name = "Overview";
                        discordField.Value = overview.Length <= 300 ? overview : $"{overview.AsSpan(0, 300)}...";
                        break;
                    case DiscordGrabFieldType.Rating:
                        discordField.Name = "Rating";
                        discordField.Value = message.Movie.MovieMetadata.Value.Ratings.Tmdb?.Value.ToString() ?? string.Empty;
                        break;
                    case DiscordGrabFieldType.Genres:
                        discordField.Name = "Genres";
                        discordField.Value = message.Movie.MovieMetadata.Value.Genres.Take(5).Join(", ");
                        break;
                    case DiscordGrabFieldType.Quality:
                        discordField.Name = "Quality";
                        discordField.Inline = true;
                        discordField.Value = message.Quality.Quality.Name;
                        break;
                    case DiscordGrabFieldType.Group:
                        discordField.Name = "Group";
                        discordField.Value = message.RemoteMovie.ParsedMovieInfo.ReleaseGroup;
                        break;
                    case DiscordGrabFieldType.Size:
                        discordField.Name = "Size";
                        discordField.Value = BytesToString(message.RemoteMovie.Release.Size);
                        discordField.Inline = true;
                        break;
                    case DiscordGrabFieldType.Release:
                        discordField.Name = "Release";
                        discordField.Value = string.Format("```{0}```", message.RemoteMovie.Release.Title);
                        break;
                    case DiscordGrabFieldType.Links:
                        discordField.Name = "Links";
                        discordField.Value = GetLinksString(message.Movie);
                        break;
                    case DiscordGrabFieldType.CustomFormats:
                        discordField.Name = "Custom Formats";
                        discordField.Value = string.Join("|", message.RemoteMovie.CustomFormats);
                        break;
                    case DiscordGrabFieldType.CustomFormatScore:
                        discordField.Name = "Custom Format Score";
                        discordField.Value = message.RemoteMovie.CustomFormatScore.ToString();
                        break;
                    case DiscordGrabFieldType.Indexer:
                        discordField.Name = "Indexer";
                        discordField.Value = message.RemoteMovie.Release.Indexer;
                        break;
                    case DiscordGrabFieldType.Tags:
                        discordField.Name = "Tags";
                        discordField.Value = GetTagLabels(message.Movie)?.Join(", ") ?? string.Empty;
                        break;
                }

                if (discordField.Name.IsNotNullOrWhiteSpace() && discordField.Value.IsNotNullOrWhiteSpace())
                {
                    embed.Fields.Add(discordField);
                }
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var isUpgrade = message.OldMovieFiles.Count > 0;
            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Radarr/Radarr/develop/Logo/256.png"
                },
                Url = $"https://www.themoviedb.org/movie/{message.Movie.MovieMetadata.Value.TmdbId}",
                Description = isUpgrade ? "Movie Upgraded" : "Movie Imported",
                Title = GetTitle(message.Movie),
                Color = isUpgrade ? (int)DiscordColors.Upgrade : (int)DiscordColors.Success,
                Fields = new List<DiscordField>(),
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = message.Movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl
                };
            }

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = message.Movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.RemoteUrl
                };
            }

            foreach (var field in Settings.ImportFields)
            {
                var discordField = new DiscordField();

                switch ((DiscordImportFieldType)field)
                {
                    case DiscordImportFieldType.Overview:
                        var overview = message.Movie.MovieMetadata.Value.Overview ?? "";
                        discordField.Name = "Overview";
                        discordField.Value = overview.Length <= 300 ? overview : $"{overview.AsSpan(0, 300)}...";
                        break;
                    case DiscordImportFieldType.Rating:
                        discordField.Name = "Rating";
                        discordField.Value = message.Movie.MovieMetadata.Value.Ratings.Tmdb?.Value.ToString() ?? string.Empty;
                        break;
                    case DiscordImportFieldType.Genres:
                        discordField.Name = "Genres";
                        discordField.Value = message.Movie.MovieMetadata.Value.Genres.Take(5).Join(", ");
                        break;
                    case DiscordImportFieldType.Quality:
                        discordField.Name = "Quality";
                        discordField.Inline = true;
                        discordField.Value = message.MovieFile.Quality.Quality.Name;
                        break;
                    case DiscordImportFieldType.Codecs:
                        discordField.Name = "Codecs";
                        discordField.Inline = true;
                        discordField.Value = string.Format("{0} / {1} {2}",
                            MediaInfoFormatter.FormatVideoCodec(message.MovieFile.MediaInfo, null),
                            MediaInfoFormatter.FormatAudioCodec(message.MovieFile.MediaInfo, null),
                            MediaInfoFormatter.FormatAudioChannels(message.MovieFile.MediaInfo));
                        break;
                    case DiscordImportFieldType.Group:
                        discordField.Name = "Group";
                        discordField.Value = message.MovieFile.ReleaseGroup;
                        break;
                    case DiscordImportFieldType.Size:
                        discordField.Name = "Size";
                        discordField.Value = BytesToString(message.MovieFile.Size);
                        discordField.Inline = true;
                        break;
                    case DiscordImportFieldType.Languages:
                        discordField.Name = "Languages";
                        discordField.Value = message.MovieFile.MediaInfo.AudioLanguages.ConcatToString("/");
                        break;
                    case DiscordImportFieldType.Subtitles:
                        discordField.Name = "Subtitles";
                        discordField.Value = message.MovieFile.MediaInfo.Subtitles.ConcatToString("/");
                        break;
                    case DiscordImportFieldType.Release:
                        discordField.Name = "Release";
                        discordField.Value = string.Format("```{0}```", message.MovieFile.SceneName);
                        break;
                    case DiscordImportFieldType.Links:
                        discordField.Name = "Links";
                        discordField.Value = GetLinksString(message.Movie);
                        break;
                    case DiscordImportFieldType.Tags:
                        discordField.Name = "Tags";
                        discordField.Value = GetTagLabels(message.Movie)?.Join(", ") ?? string.Empty;
                        break;
                }

                if (discordField.Name.IsNotNullOrWhiteSpace() && discordField.Value.IsNotNullOrWhiteSpace())
                {
                    embed.Fields.Add(discordField);
                }
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnMovieAdded(Movie movie)
        {
            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Radarr/Radarr/develop/Logo/256.png"
                },
                Url = $"https://www.themoviedb.org/movie/{movie.MovieMetadata.Value.TmdbId}",
                Title = movie.Title,
                Description = "Movie Added",
                Color = (int)DiscordColors.Success,
                Fields = new List<DiscordField> { new () { Name = "Links", Value = GetLinksString(movie) } }
            };

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.Url
                };
            }

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.Url
                };
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnMovieRename(Movie movie, List<RenamedMovieFile> renamedFiles)
        {
            var attachments = new List<Embed>();

            foreach (var renamedFile in renamedFiles)
            {
                attachments.Add(new Embed
                {
                    Title = movie.MovieMetadata.Value.Title,
                    Description = renamedFile.PreviousRelativePath + " renamed to " + renamedFile.MovieFile.RelativePath,
                });
            }

            var payload = CreatePayload("Renamed", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            var movie = deleteMessage.Movie;

            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Radarr/Radarr/develop/Logo/256.png"
                },
                Url = $"https://www.themoviedb.org/movie/{movie.MovieMetadata.Value.TmdbId}",
                Title = movie.Title,
                Description = deleteMessage.DeletedFilesMessage,
                Color = (int)DiscordColors.Danger,
                Fields = new List<DiscordField> { new () { Name = "Links", Value = GetLinksString(movie) } }
            };

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.Url
                };
            }

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.Url
                };
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            var movie = deleteMessage.Movie;
            var deletedFile = deleteMessage.MovieFile.Path;
            var reason = deleteMessage.Reason;

            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Radarr/Radarr/develop/Logo/256.png"
                },
                Url = $"https://www.themoviedb.org/movie/{movie.MovieMetadata.Value.TmdbId}",
                Title = GetTitle(movie),
                Description = "Movie File Deleted",
                Color = (int)DiscordColors.Danger,
                Fields = new List<DiscordField>
                {
                    new () { Name = "Reason", Value = reason.ToString() },
                    new () { Name = "File name", Value = string.Format("```{0}```", deletedFile) }
                },
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            };

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Radarr/Radarr/develop/Logo/256.png"
                },
                Title = healthCheck.Source.Name,
                Description = healthCheck.Message,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Color = healthCheck.Type == HealthCheck.HealthCheckResult.Warning ? (int)DiscordColors.Warning : (int)DiscordColors.Danger
            };

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Radarr/Radarr/develop/Logo/256.png"
                },
                Title = "Health Issue Resolved: " + previousCheck.Source.Name,
                Description = $"The following issue is now resolved: {previousCheck.Message}",
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Color = (int)DiscordColors.Success
            };

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Radarr/Radarr/develop/Logo/256.png"
                },
                Title = APPLICATION_UPDATE_TITLE,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Color = (int)DiscordColors.Standard,
                Fields = new List<DiscordField>
                {
                    new ()
                    {
                        Name = "Previous Version",
                        Value = updateMessage.PreviousVersion.ToString()
                    },
                    new ()
                    {
                        Name = "New Version",
                        Value = updateMessage.NewVersion.ToString()
                    }
                },
            };

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            var movie = message.Movie;

            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Radarr/Radarr/develop/Logo/256.png"
                },
                Url = $"https://www.themoviedb.org/movie/{movie.MovieMetadata.Value.TmdbId}",
                Description = "Manual interaction needed",
                Title = GetTitle(movie),
                Color = (int)DiscordColors.Standard,
                Fields = new List<DiscordField>(),
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            if (Settings.ManualInteractionFields.Contains((int)DiscordManualInteractionFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl
                };
            }

            if (Settings.ManualInteractionFields.Contains((int)DiscordManualInteractionFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.RemoteUrl
                };
            }

            foreach (var field in Settings.ManualInteractionFields)
            {
                var discordField = new DiscordField();

                switch ((DiscordManualInteractionFieldType)field)
                {
                    case DiscordManualInteractionFieldType.Overview:
                        var overview = movie.MovieMetadata.Value.Overview ?? "";
                        discordField.Name = "Overview";
                        discordField.Value = overview.Length <= 300 ? overview : $"{overview.AsSpan(0, 300)}...";
                        break;
                    case DiscordManualInteractionFieldType.Rating:
                        discordField.Name = "Rating";
                        discordField.Value = movie.MovieMetadata.Value.Ratings.Tmdb?.Value.ToString() ?? string.Empty;
                        break;
                    case DiscordManualInteractionFieldType.Genres:
                        discordField.Name = "Genres";
                        discordField.Value = movie.MovieMetadata.Value.Genres.Take(5).Join(", ");
                        break;
                    case DiscordManualInteractionFieldType.Quality:
                        discordField.Name = "Quality";
                        discordField.Inline = true;
                        discordField.Value = message.Quality.Quality.Name;
                        break;
                    case DiscordManualInteractionFieldType.Group:
                        discordField.Name = "Group";
                        discordField.Value = message.RemoteMovie.ParsedMovieInfo.ReleaseGroup;
                        break;
                    case DiscordManualInteractionFieldType.Size:
                        discordField.Name = "Size";
                        discordField.Value = BytesToString(message.TrackedDownload.DownloadItem.TotalSize);
                        discordField.Inline = true;
                        break;
                    case DiscordManualInteractionFieldType.DownloadTitle:
                        discordField.Name = "Download";
                        discordField.Value = string.Format("```{0}```", message.TrackedDownload.DownloadItem.Title);
                        break;
                    case DiscordManualInteractionFieldType.Links:
                        discordField.Name = "Links";
                        discordField.Value = GetLinksString(message.Movie);
                        break;
                    case DiscordManualInteractionFieldType.Tags:
                        discordField.Name = "Tags";
                        discordField.Value = GetTagLabels(message.Movie)?.Join(", ") ?? string.Empty;
                        break;
                }

                if (discordField.Name.IsNotNullOrWhiteSpace() && discordField.Value.IsNotNullOrWhiteSpace())
                {
                    embed.Fields.Add(discordField);
                }
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(TestMessage());

            return new ValidationResult(failures);
        }

        public ValidationFailure TestMessage()
        {
            try
            {
                var message = $"Test message from Radarr posted at {DateTime.Now}";
                var payload = CreatePayload(message);

                _proxy.SendPayload(payload, Settings);
            }
            catch (DiscordException ex)
            {
                return new NzbDroneValidationFailure(string.Empty, _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }

        private DiscordPayload CreatePayload(string message, List<Embed> embeds = null)
        {
            var avatar = Settings.Avatar;

            var payload = new DiscordPayload
            {
                Username = Settings.Username,
                Content = message,
                Embeds = embeds
            };

            if (avatar.IsNotNullOrWhiteSpace())
            {
                payload.AvatarUrl = avatar;
            }

            if (Settings.Username.IsNotNullOrWhiteSpace())
            {
                payload.Username = Settings.Username;
            }

            return payload;
        }

        private static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; // Longs run out around EB
            if (byteCount == 0)
            {
                return "0 " + suf[0];
            }

            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return string.Format("{0} {1}", (Math.Sign(byteCount) * num).ToString(), suf[place]);
        }

        private static string GetLinksString(Movie movie)
        {
            var links = string.Format("[{0}]({1})", "TMDb", $"https://themoviedb.org/movie/{movie.MovieMetadata.Value.TmdbId}");
            links += string.Format(" / [{0}]({1})", "Trakt", $"https://trakt.tv/search/tmdb/{movie.MovieMetadata.Value.TmdbId}?id_type=movie");
            if (movie.MovieMetadata.Value.ImdbId.IsNotNullOrWhiteSpace())
            {
                links += string.Format(" / [{0}]({1})", "IMDb", $"https://imdb.com/title/{movie.MovieMetadata.Value.ImdbId}/");
            }

            if (movie.MovieMetadata.Value.YouTubeTrailerId.IsNotNullOrWhiteSpace())
            {
                links += string.Format(" / [{0}]({1})", "YouTube", $"https://www.youtube.com/watch?v={movie.MovieMetadata.Value.YouTubeTrailerId}");
            }

            if (movie.MovieMetadata.Value.Website.IsNotNullOrWhiteSpace())
            {
                links += string.Format(" / [{0}]({1})", "Website", movie.MovieMetadata.Value.Website);
            }

            return links;
        }

        private string GetTitle(Movie movie)
        {
            var title = movie.MovieMetadata.Value.Year > 0 ? $"{movie.MovieMetadata.Value.Title} ({movie.MovieMetadata.Value.Year})" : movie.MovieMetadata.Value.Title;

            return title.Length > 256 ? $"{title.AsSpan(0, 253)}..." : title;
        }

        private IEnumerable<string> GetTagLabels(Movie movie)
        {
            return movie.Tags?.Select(t => _tagRepository.Get(t)?.Label).OrderBy(t => t).Take(5);
        }
    }
}
