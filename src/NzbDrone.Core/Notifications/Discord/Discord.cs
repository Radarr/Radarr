using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications.Discord.Payloads;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Discord
{
    public class Discord : NotificationBase<DiscordSettings>
    {
        private readonly IDiscordProxy _proxy;

        public Discord(IDiscordProxy proxy)
        {
            _proxy = proxy;
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
                Title = message.Movie.MovieMetadata.Value.Year > 0 ? $"{message.Movie.MovieMetadata.Value.Title} ({message.Movie.MovieMetadata.Value.Year})" : message.Movie.MovieMetadata.Value.Title,
                Color = (int)DiscordColors.Standard,
                Fields = new List<DiscordField>(),
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            if (Settings.GrabFields.Contains((int)DiscordGrabFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = message.Movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.Url
                };
            }

            if (Settings.GrabFields.Contains((int)DiscordGrabFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = message.Movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.Url
                };
            }

            foreach (var field in Settings.GrabFields)
            {
                var discordField = new DiscordField();

                switch ((DiscordGrabFieldType)field)
                {
                    case DiscordGrabFieldType.Overview:
                        discordField.Name = "Overview";
                        discordField.Value = message.Movie.MovieMetadata.Value.Overview.Length <= 300 ? message.Movie.MovieMetadata.Value.Overview : message.Movie.MovieMetadata.Value.Overview.Substring(0, 300) + "...";
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
                Title = message.Movie.MovieMetadata.Value.Year > 0 ? $"{message.Movie.MovieMetadata.Value.Title} ({message.Movie.MovieMetadata.Value.Year})" : message.Movie.MovieMetadata.Value.Title,
                Color = isUpgrade ? (int)DiscordColors.Upgrade : (int)DiscordColors.Success,
                Fields = new List<DiscordField>(),
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = message.Movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.Url
                };
            }

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = message.Movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.Url
                };
            }

            foreach (var field in Settings.ImportFields)
            {
                var discordField = new DiscordField();

                switch ((DiscordImportFieldType)field)
                {
                    case DiscordImportFieldType.Overview:
                        discordField.Name = "Overview";
                        discordField.Value = message.Movie.MovieMetadata.Value.Overview.Length <= 300 ? message.Movie.MovieMetadata.Value.Overview : message.Movie.MovieMetadata.Value.Overview.Substring(0, 300) + "...";
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
                }

                if (discordField.Name.IsNotNullOrWhiteSpace() && discordField.Value.IsNotNullOrWhiteSpace())
                {
                    embed.Fields.Add(discordField);
                }
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnMovieRename(Movie movie, List<RenamedMovieFile> renamedFiles)
        {
            var attachments = new List<Embed>();

            foreach (RenamedMovieFile renamedFile in renamedFiles)
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

            var attachments = new List<Embed>
                              {
                                  new Embed
                                  {
                                      Title = movie.MovieMetadata.Value.Title,
                                      Description = deleteMessage.DeletedFilesMessage
                                  }
                              };

            var payload = CreatePayload("Movie Deleted", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            var movie = deleteMessage.Movie;

            var fullPath = Path.Combine(deleteMessage.Movie.Path, deleteMessage.MovieFile.RelativePath);
            var attachments = new List<Embed>
                              {
                                  new Embed
                                  {
                                      Title = movie.MovieMetadata.Value.Title,
                                      Description = deleteMessage.MovieFile.Path
                                  }
                              };

            var payload = CreatePayload("Movie File Deleted", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var attachments = new List<Embed>
                              {
                                  new Embed
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
                                  }
                              };

            var payload = CreatePayload(null, attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var attachments = new List<Embed>
                              {
                                  new Embed
                                  {
                                      Author = new DiscordAuthor
                                      {
                                          Name = Settings.Author.IsNullOrWhiteSpace() ? Environment.MachineName : Settings.Author,
                                          IconUrl = "https://raw.githubusercontent.com/Radarr/Radarr/develop/Logo/256.png"
                                      },
                                      Title = APPLICATION_UPDATE_TITLE,
                                      Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                                      Color = (int)DiscordColors.Standard,
                                      Fields = new List<DiscordField>()
                                      {
                                          new DiscordField()
                                          {
                                              Name = "Previous Version",
                                              Value = updateMessage.PreviousVersion.ToString()
                                          },
                                          new DiscordField()
                                          {
                                              Name = "New Version",
                                              Value = updateMessage.NewVersion.ToString()
                                          }
                                      },
                                  }
                              };

            var payload = CreatePayload(null, attachments);

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
                return new NzbDroneValidationFailure("Unable to post", ex.Message);
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
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
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
    }
}
