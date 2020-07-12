using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Notifications.Goodreads
{
    public class GoodreadsOwnedBooks : GoodreadsNotificationBase<GoodreadsOwnedBooksNotificationSettings>
    {
        public GoodreadsOwnedBooks(IHttpClient httpClient,
                                   Logger logger)
        : base(httpClient, logger)
        {
        }

        public override string Name => "Goodreads Owned Books";
        public override string Link => "https://goodreads.com/";

        public override void OnReleaseImport(BookDownloadMessage message)
        {
            var bookId = message.Book.Editions.Value.Single(x => x.Monitored).ForeignEditionId;
            AddOwnedBook(bookId);
        }

        private void AddOwnedBook(string bookId)
        {
            var req = RequestBuilder()
                .Post()
                .SetSegment("route", "owned_books.xml")
                .AddFormParameter("owned_book[book_id]", bookId)
                .AddFormParameter("owned_book[condition_code]", Settings.Condition)
                .AddFormParameter("owned_book[original_purchase_date]", DateTime.Now.ToString("O"));

            if (Settings.Description.IsNotNullOrWhiteSpace())
            {
                req.AddFormParameter("owned_book[condition_description]", Settings.Description);
            }

            if (Settings.Location.IsNotNullOrWhiteSpace())
            {
                req.AddFormParameter("owned_book[original_purchase_location]", Settings.Location);
            }

            OAuthExecute(req);
        }
    }
}
