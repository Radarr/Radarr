using System.Diagnostics;
using System.Xml.Linq;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    /// <summary>
    /// This class models areas of the API where Goodreads returns
    /// very brief information about an Author instead of their entire profile.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class AuthorSummaryResource : GoodreadsResource
    {
        public override string ElementName => "author";

        /// <summary>
        /// The Goodreads Author Id.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// The name of this author.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The role of this author.
        /// </summary>
        public string Role { get; private set; }

        /// <summary>
        /// The image of this author, regular size.
        /// </summary>
        public string ImageUrl { get; private set; }

        /// <summary>
        /// The image of this author, small size.
        /// </summary>
        public string SmallImageUrl { get; private set; }

        /// <summary>
        /// The link to the Goodreads page for this author.
        /// </summary>
        public string Link { get; private set; }

        /// <summary>
        /// The average rating for all of this author's books.
        /// </summary>
        public decimal? AverageRating { get; private set; }

        /// <summary>
        /// The total count of all ratings of this author's books.
        /// </summary>
        public int? RatingsCount { get; private set; }

        /// <summary>
        /// The total count of all the text reviews of this author's books.
        /// </summary>
        public int? TextReviewsCount { get; private set; }

        public override void Parse(XElement element)
        {
            Id = element.ElementAsLong("id");
            Name = element.ElementAsString("name");
            Role = element.ElementAsString("role");
            ImageUrl = element.ElementAsString("image_url");
            SmallImageUrl = element.ElementAsString("small_image_url");
            Link = element.ElementAsString("link");
            AverageRating = element.ElementAsNullableDecimal("average_rating");
            RatingsCount = element.ElementAsNullableInt("ratings_count");
            TextReviewsCount = element.ElementAsNullableInt("text_reviews_count");
        }
    }
}
