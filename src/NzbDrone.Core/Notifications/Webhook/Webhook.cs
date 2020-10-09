using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class Webhook : NotificationBase<WebhookSettings>
    {
        private readonly IWebhookProxy _proxy;

        public Webhook(IWebhookProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Link => "https://github.com/Readarr/Readarr/wiki/Webhook";

        public override void OnGrab(GrabMessage message)
        {
            var remoteBook = message.Book;
            var quality = message.Quality;

            var payload = new WebhookGrabPayload
            {
                EventType = WebhookEventType.Grab,
                Artist = new WebhookAuthor(message.Author),
                Albums = remoteBook.Books.ConvertAll(x => new WebhookBook(x)
                {
                    // TODO: Stop passing these parameters inside an album v3
                    Quality = quality.Quality.Name,
                    QualityVersion = quality.Revision.Version,
                    ReleaseGroup = remoteBook.ParsedBookInfo.ReleaseGroup
                }),
                Release = new WebhookRelease(quality, remoteBook),
                DownloadClient = message.DownloadClient,
                DownloadId = message.DownloadId
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnReleaseImport(BookDownloadMessage message)
        {
            var bookFiles = message.BookFiles;

            var payload = new WebhookImportPayload
            {
                EventType = WebhookEventType.Download,
                Artist = new WebhookAuthor(message.Author),
                Book = new WebhookBook(message.Book),
                BookFiles = bookFiles.ConvertAll(x => new WebhookBookFile(x)),
                IsUpgrade = message.OldFiles.Any(),
                DownloadClient = message.DownloadClient,
                DownloadId = message.DownloadId
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnRename(Author author)
        {
            var payload = new WebhookRenamePayload
            {
                EventType = WebhookEventType.Rename,
                Artist = new WebhookAuthor(author)
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnBookRetag(BookRetagMessage message)
        {
            var payload = new WebhookRetagPayload
            {
                EventType = WebhookEventType.Retag,
                Artist = new WebhookAuthor(message.Author)
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var payload = new WebhookHealthPayload
                          {
                              EventType = WebhookEventType.Health,
                              Level = healthCheck.Type,
                              Message = healthCheck.Message,
                              Type = healthCheck.Source.Name,
                              WikiUrl = healthCheck.WikiUrl?.ToString()
                          };

            _proxy.SendWebhook(payload, Settings);
        }

        public override string Name => "Webhook";

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(SendWebhookTest());

            return new ValidationResult(failures);
        }

        private ValidationFailure SendWebhookTest()
        {
            try
            {
                var payload = new WebhookGrabPayload
                {
                    EventType = WebhookEventType.Test,
                    Artist = new WebhookAuthor()
                    {
                        Id = 1,
                        Name = "Test Name",
                        Path = "C:\\testpath",
                        MBId = "aaaaa-aaa-aaaa-aaaaaa"
                    },
                    Books = new List<WebhookBook>()
                    {
                            new WebhookBook()
                            {
                                Id = 123,
                                Title = "Test title"
                            }
                    }
                };

                _proxy.SendWebhook(payload, Settings);
            }
            catch (WebhookException ex)
            {
                return new NzbDroneValidationFailure("Url", ex.Message);
            }

            return null;
        }
    }
}
