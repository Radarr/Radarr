using System;
using System.Xml.Linq;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    /// <summary>
    /// This class models areas of the API where Goodreads returns
    /// information about an user owned books.
    /// </summary>
    public sealed class OwnedBookResource : GoodreadsResource
    {
        public override string ElementName => "owned_book";

        /// <summary>
        /// The owner book id.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// The owner id.
        /// </summary>
        public long OwnerId { get; private set; }

        /// <summary>
        /// The original date when owner has bought a book.
        /// </summary>
        public DateTime? OriginalPurchaseDate { get; private set; }

        /// <summary>
        /// The original location where owner has bought a book.
        /// </summary>
        public string OriginalPurchaseLocation { get; private set; }

        /// <summary>
        /// The owned book condition.
        /// </summary>
        public string Condition { get; private set; }

        /// <summary>
        /// The traded count.
        /// </summary>
        public int TradedCount { get; private set; }

        /// <summary>
        /// The link to the owned book.
        /// </summary>
        public string Link { get; private set; }

        /// <summary>
        /// The book.
        /// </summary>
        public BookSummaryResource Book { get; private set; }

        /// <summary>
        /// The owned book review.
        /// </summary>
        public ReviewResource Review { get; private set; }

        public override void Parse(XElement element)
        {
            Id = element.ElementAsLong("id");
            OwnerId = element.ElementAsLong("current_owner_id");
            OriginalPurchaseDate = element.ElementAsDateTime("original_purchase_date");
            OriginalPurchaseLocation = element.ElementAsString("original_purchase_location");
            Condition = element.ElementAsString("condition");

            var review = element.Element("review");
            if (review != null)
            {
                Review = new ReviewResource();
                Review.Parse(review);
            }

            var book = element.Element("book");
            if (book != null)
            {
                Book = new BookSummaryResource();
                Book.Parse(book);
            }
        }
    }
}
