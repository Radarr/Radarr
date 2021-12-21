using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications.Slack.Payloads;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Slack
{
    public class Slack : NotificationBase<SlackSettings>
    {
        private readonly ISlackProxy _proxy;

        public Slack(ISlackProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Name => "Slack";
        public override string Link => "https://my.slack.com/services/new/incoming-webhook/";

        public override void OnGrab(GrabMessage message)
        {
            var attachments = new List<Attachment>
                            {
                                new Attachment
                                {
                                    Fallback = message.Message,
                                    Title = message.Movie.Title,
                                    Text = message.Message,
                                    Color = "warning"
                                }
                            };
            var payload = CreatePayload($"Grabbed: {message.Message}", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var attachments = new List<Attachment>
                                {
                                    new Attachment
                                    {
                                        Fallback = message.Message,
                                        Title = message.Movie.Title,
                                        Text = message.Message,
                                        Color = "good"
                                    }
                                };
            var payload = CreatePayload($"Imported: {message.Message}", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnMovieRename(Movie movie, List<RenamedMovieFile> renamedFiles)
        {
            var attachments = new List<Attachment>();

            foreach (RenamedMovieFile renamedFile in renamedFiles)
            {
                attachments.Add(new Attachment
                {
                    Title = movie.Title,
                    Text = renamedFile.PreviousRelativePath + " renamed to " + renamedFile.MovieFile.RelativePath,
                });
            }

            var payload = CreatePayload("Renamed", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            var attachments = new List<Attachment>
                              {
                                  new Attachment
                                  {
                                      Title = deleteMessage.Movie.Title,
                                      Text = Path.Combine(deleteMessage.Movie.Path, deleteMessage.MovieFile.RelativePath)
                                  }
                              };

            var payload = CreatePayload("Movie File Deleted", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            var attachments = new List<Attachment>
                              {
                                  new Attachment
                                  {
                                      Title = deleteMessage.Movie.Title,
                                      Text = deleteMessage.DeletedFilesMessage
                                  }
                              };

            var payload = CreatePayload("Movie Deleted", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var attachments = new List<Attachment>
                              {
                                  new Attachment
                                  {
                                      Title = healthCheck.Source.Name,
                                      Text = healthCheck.Message,
                                      Color = healthCheck.Type == HealthCheck.HealthCheckResult.Warning ? "warning" : "danger"
                                  }
                              };

            var payload = CreatePayload("Health Issue", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var attachments = new List<Attachment>
                              {
                                  new Attachment
                                  {
                                      Title = Environment.MachineName,
                                      Text = updateMessage.Message,
                                      Color = "good"
                                  }
                              };

            var payload = CreatePayload("Application Updated", attachments);

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
            catch (SlackExeption ex)
            {
                return new NzbDroneValidationFailure("Unable to post", ex.Message);
            }

            return null;
        }

        private SlackPayload CreatePayload(string message, List<Attachment> attachments = null)
        {
            var icon = Settings.Icon;
            var channel = Settings.Channel;

            var payload = new SlackPayload
            {
                Username = Settings.Username,
                Text = message,
                Attachments = attachments
            };

            if (icon.IsNotNullOrWhiteSpace())
            {
                // Set the correct icon based on the value
                if (icon.StartsWith(":") && icon.EndsWith(":"))
                {
                    payload.IconEmoji = icon;
                }
                else
                {
                    payload.IconUrl = icon;
                }
            }

            if (channel.IsNotNullOrWhiteSpace())
            {
                payload.Channel = channel;
            }

            return payload;
        }
    }
}
