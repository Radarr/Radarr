using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    /// <summary>
    /// This class models areas of the API where Goodreads returns
    /// very brief information about a Book instead of their entire object.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class BookSummaryResource : GoodreadsResource
    {
        public override string ElementName => "book";

        /// <summary>
        /// The Id of this book.
        /// </summary>
        public long Id { get; private set; }

        public string Uri { get; set; }

        /// <summary>
        /// The title of this book.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The title of this book without series information in it.
        /// </summary>
        public string TitleWithoutSeries { get; private set; }

        /// <summary>
        /// The link to the Goodreads page for this book.
        /// </summary>
        public string Link { get; private set; }

        /// <summary>
        /// The cover image of this book, regular size.
        /// </summary>
        public string ImageUrl { get; private set; }

        /// <summary>
        /// The cover image of this book, small size.
        /// </summary>
        public string SmallImageUrl { get; private set; }

        /// <summary>
        /// The work id of this book.
        /// </summary>
        public long? WorkId { get; private set; }

        /// <summary>
        /// The ISBN of this book.
        /// </summary>
        public string Isbn { get; private set; }

        /// <summary>
        /// The ISBN13 of this book.
        /// </summary>
        public string Isbn13 { get; private set; }

        /// <summary>
        /// The average rating of the book.
        /// </summary>
        public decimal? AverageRating { get; private set; }

        /// <summary>
        /// The count of all ratings for the book.
        /// </summary>
        public int? RatingsCount { get; private set; }

        /// <summary>
        /// The date this book was published.
        /// </summary>
        public DateTime? PublicationDate { get; private set; }

        /// <summary>
        /// Summary information about the authors of this book.
        /// </summary>
        public IReadOnlyList<AuthorSummaryResource> Authors { get; private set; }

        /// <summary>
        /// The edition information about book.
        /// </summary>
        public string EditionInformation { get; private set; }

        /// <summary>
        /// The book format.
        /// </summary>
        public string Format { get; private set; }

        /// <summary>
        /// The book description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Number of pages.
        /// </summary>
        public int NumberOfPages { get; private set; }

        /// <summary>
        /// The book publisher.
        /// </summary>
        public string Publisher { get; private set; }

        /// <summary>
        /// The image url, large size.
        /// </summary>
        public string LargeImageUrl { get; private set; }

        /// <summary>
        /// A count of text reviews for this book.
        /// </summary>
        public int TextReviewsCount { get; private set; }

        public override void Parse(XElement element)
        {
            Id = element.ElementAsLong("id");
            Uri = element.ElementAsString("uri");
            Title = element.ElementAsString("title");
            TitleWithoutSeries = element.ElementAsString("title_without_series");
            Link = element.ElementAsString("link");
            ImageUrl = element.ElementAsString("image_url");
            SmallImageUrl = element.ElementAsString("small_image_url");
            Isbn = element.ElementAsString("isbn");
            Isbn13 = element.ElementAsString("isbn13");
            AverageRating = element.ElementAsNullableDecimal("average_rating");
            RatingsCount = element.ElementAsNullableInt("ratings_count");
            PublicationDate = element.ElementAsMultiDateField("publication");
            Authors = element.ParseChildren<AuthorSummaryResource>("authors", "author");

            var workElement = element.Element("work");
            if (workElement != null)
            {
                WorkId = workElement.ElementAsNullableInt("id");
            }

            EditionInformation = element.ElementAsString("edition_information");
            Format = element.ElementAsString("format");
            Description = element.ElementAsString("description");
            NumberOfPages = element.ElementAsInt("num_pages");
            Publisher = element.ElementAsString("publisher");
            LargeImageUrl = element.ElementAsString("large_image_url");
            TextReviewsCount = element.ElementAsInt("text_reviews_count");
        }
    }
}
