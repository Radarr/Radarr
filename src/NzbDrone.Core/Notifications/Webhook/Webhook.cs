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
            var remoteAlbum = message.Book;
            var quality = message.Quality;

            var payload = new WebhookGrabPayload
            {
                EventType = "Grab",
                Author = new WebhookAuthor(message.Author),
                Books = remoteAlbum.Books.ConvertAll(x => new WebhookBook(x)
                {
                    // TODO: Stop passing these parameters inside an album v3
                    Quality = quality.Quality.Name,
                    QualityVersion = quality.Revision.Version,
                    ReleaseGroup = remoteAlbum.ParsedBookInfo.ReleaseGroup
                }),
                Release = new WebhookRelease(quality, remoteAlbum)
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnReleaseImport(BookDownloadMessage message)
        {
            var bookFiles = message.BookFiles;

            var payload = new WebhookImportPayload
            {
                EventType = "Download",
                Author = new WebhookAuthor(message.Author),
                Book = new WebhookBook(message.Book),
                BookFiles = bookFiles.ConvertAll(x => new WebhookBookFile(x)),
                IsUpgrade = message.OldFiles.Any()
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnRename(Author author)
        {
            var payload = new WebhookPayload
            {
                EventType = "Rename",
                Author = new WebhookAuthor(author)
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnTrackRetag(BookRetagMessage message)
        {
            var payload = new WebhookPayload
            {
                EventType = "Retag",
                Author = new WebhookAuthor(message.Author)
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
                    EventType = "Test",
                    Author = new WebhookAuthor()
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
