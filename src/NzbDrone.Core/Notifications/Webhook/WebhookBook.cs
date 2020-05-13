using System;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookBook
    {
        public WebhookBook()
        {
        }

        public WebhookBook(Book book)
        {
            Id = book.Id;
            Title = book.Title;
            ReleaseDate = book.ReleaseDate;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime? ReleaseDate { get; set; }

        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
        public string SceneName { get; set; }
    }
}
