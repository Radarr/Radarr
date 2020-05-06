using System;
using System.Xml.Linq;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    /// <summary>
    /// This class models a Review as defined by the Goodreads API.
    /// </summary>
    public class ReviewResource : GoodreadsResource
    {
        public override string ElementName => "review";

        /// <summary>
        /// The Goodreads review id.
        /// </summary>
        public long Id { get; protected set; }

        /// <summary>
        /// The summary information for the book this review is for.
        /// </summary>
        public BookSummaryResource Book { get; protected set; }

        /// <summary>
        /// The rating the user gave the book in this review.
        /// </summary>
        public int Rating { get; protected set; }

        /// <summary>
        /// The number of votes this review received from other Goodreads users.
        /// </summary>
        public int Votes { get; protected set; }

        /// <summary>
        /// A flag determining if the review contains spoilers.
        /// </summary>
        public bool IsSpoiler { get; protected set; }

        /// <summary>
        /// The state of the spoilers for this review.
        /// </summary>
        public string SpoilersState { get; protected set; }

        /// <summary>
        /// The shelves the user has added this review to.
        /// </summary>
        // public IReadOnlyList<ReviewShelf> Shelves { get; protected set; }

        /// <summary>
        /// Who the user would recommend reading this book.
        /// </summary>
        public string RecommendedFor { get; protected set; }

        /// <summary>
        /// Who recommended the user to read this book.
        /// </summary>
        public string RecommendedBy { get; protected set; }

        /// <summary>
        /// The date the user started reading this book.
        /// </summary>
        public DateTime? DateStarted { get; protected set; }

        /// <summary>
        /// The date the user finished reading this book.
        /// </summary>
        public DateTime? DateRead { get; protected set; }

        /// <summary>
        /// The date the user added this book to their shelves.
        /// </summary>
        public DateTime? DateAdded { get; protected set; }

        /// <summary>
        /// The date the user last updated this book on their shelves.
        /// </summary>
        public DateTime? DateUpdated { get; protected set; }

        /// <summary>
        /// The number of times this book has been read.
        /// </summary>
        public int? ReadCount { get; protected set; }

        /// <summary>
        /// The main text of this review. May contain HTML.
        /// </summary>
        public string Body { get; protected set; }

        /// <summary>
        /// The number of comments on this review.
        /// </summary>
        public int CommentsCount { get; protected set; }

        /// <summary>
        /// The Goodreads URL of this review.
        /// </summary>
        public string Url { get; protected set; }

        /// <summary>
        /// The owned count of the book.
        /// </summary>
        public int Owned { get; protected set; }

        public override void Parse(XElement element)
        {
            Id = element.ElementAsLong("id");

            var bookElement = element.Element("book");
            if (bookElement != null)
            {
                Book = new BookSummaryResource();
                Book.Parse(bookElement);
            }

            Rating = element.ElementAsInt("rating");
            Votes = element.ElementAsInt("votes");
            IsSpoiler = element.ElementAsBool("spoiler_flag");
            SpoilersState = element.ElementAsString("spoilers_state");
            RecommendedFor = element.ElementAsString("recommended_for");
            RecommendedBy = element.ElementAsString("recommended_by");
            DateStarted = element.ElementAsDateTime("started_at");
            DateRead = element.ElementAsDateTime("read_at");
            DateAdded = element.ElementAsDateTime("date_added");
            DateUpdated = element.ElementAsDateTime("date_updated");
            ReadCount = element.ElementAsInt("read_count");
            Body = element.ElementAsString("body");
            CommentsCount = element.ElementAsInt("comments_count");
            Url = element.ElementAsString("url");
            Owned = element.ElementAsInt("owned");
        }
    }
}
