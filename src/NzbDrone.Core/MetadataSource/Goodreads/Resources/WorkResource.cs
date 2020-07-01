using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    /// <summary>
    /// This class models a work as defined by the Goodreads API.
    /// A work is the root concept of something written. Each book
    /// is a published edition of a piece of work. Most work properties
    /// are aggregate information over all the editions of a work.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class WorkResource : GoodreadsResource
    {
        public override string ElementName => "work";

        /// <summary>
        /// The Goodreads Id for this work.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// The number of books for this work.
        /// </summary>
        public int BooksCount { get; private set; }

        /// <summary>
        /// The Goodreads Book Id that is considered the best version of this work.
        /// Might not be populated. See the <see cref="BestBook"/> property for details, if provided.
        /// </summary>
        public long? BestBookId { get; private set; }

        /// <summary>
        /// The details for the best book of this work. Only populated
        /// if Goodreads provides it as part of the response.
        /// </summary>
        public BestBookResource BestBook { get; private set; }

        public long SeriesLinkId { get; private set; }

        /// <summary>
        /// If included in a list, this defines this work's position.
        /// </summary>
        public string UserPosition { get; private set; }

        /// <summary>
        /// The number of reviews of this work.
        /// </summary>
        public int ReviewsCount { get; private set; }

        /// <summary>
        /// The average rating of this work.
        /// </summary>
        public decimal AverageRating { get; private set; }

        /// <summary>
        /// The number of ratings of this work.
        /// </summary>
        public int RatingsCount { get; private set; }

        /// <summary>
        /// The number of text reviews of this work.
        /// </summary>
        public int TextReviewsCount { get; private set; }

        /// <summary>
        /// The original publication date of this work.
        /// </summary>
        public DateTime? OriginalPublicationDate { get; private set; }

        /// <summary>
        /// The original title of this work.
        /// </summary>
        public string OriginalTitle { get; private set; }

        /// <summary>
        /// The original language of this work.
        /// </summary>
        public int? OriginalLanguageId { get; private set; }

        /// <summary>
        /// The type of media for this work.
        /// </summary>
        public string MediaType { get; private set; }

        /// <summary>
        /// The distribution of all the ratings for this work.
        /// A dictionary of star rating -> number of ratings.
        /// </summary>
        public IReadOnlyDictionary<int, int> RatingDistribution { get; private set; }

        public override void Parse(XElement element)
        {
            Id = element.ElementAsLong("id");

            var bestBookElement = element.Element("best_book");
            if (bestBookElement != null)
            {
                BestBook = new BestBookResource();
                BestBook.Parse(bestBookElement);
            }

            BestBookId = element.ElementAsNullableLong("best_book_id");
            BooksCount = element.ElementAsInt("books_count");
            ReviewsCount = element.ElementAsInt("reviews_count");

            RatingsCount = element.ElementAsInt("ratings_count");

            var average = element.ElementAsDecimal("average_rating");
            if (average == 0 && RatingsCount > 0)
            {
                average = element.ElementAsDecimal("ratings_sum") / RatingsCount;
            }

            AverageRating = average;

            TextReviewsCount = element.ElementAsInt("text_reviews_count");

            OriginalPublicationDate = element.ElementAsMultiDateField("original_publication");
            OriginalTitle = element.ElementAsString("original_title");
            OriginalLanguageId = element.ElementAsNullableInt("original_language_id");
            MediaType = element.ElementAsString("media_type");

            // Parse out the rating distribution
            var ratingDistributionElement = element.ElementAsString("rating_dist");
            if (ratingDistributionElement != null)
            {
                var parts = ratingDistributionElement.Split('|');
                if (parts.Length > 0)
                {
                    var ratingDistribution = new Dictionary<int, int>();

                    var ratings = parts.Select(x => x.Split(':'))
                                       .Where(x => x[0] != "total")
                                       .OrderBy(x => x[0]);

                    foreach (var rating in ratings)
                    {
                        int star = 0, count = 0;
                        int.TryParse(rating[0], out star);
                        int.TryParse(rating[1], out count);

                        ratingDistribution.Add(star, count);
                    }

                    RatingDistribution = ratingDistribution;
                }
            }
        }

        internal void SetSeriesInfo(XElement element)
        {
            SeriesLinkId = element.ElementAsLong("id");
            UserPosition = element.ElementAsString("user_position");
        }
    }
}
