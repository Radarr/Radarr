using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    /// <summary>
    /// Represents a user's shelf on their Goodreads profile.
    /// </summary>
    public sealed class UserShelfResource : GoodreadsResource
    {
        public override string ElementName => "shelf";

        /// <summary>
        /// The Id of this user shelf.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// The name of this user shelf.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The number of books on this user shelf.
        /// </summary>
        public int BookCount { get; private set; }

        /// <summary>
        /// Determines if this shelf is exclusive or not.
        /// A single book can only be on one exclusive shelf.
        /// </summary>
        public bool IsExclusive { get; private set; }

        /// <summary>
        /// The description of this user shelf.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Determines the default sort column of this user shelf.
        /// </summary>
        public string Sort { get; private set; }

        /// <summary>
        /// Determines the default sort order of this user shelf.
        /// </summary>
        // public Order? Order { get; private set; }

        /// <summary>
        /// Determines if this shelf will be featured on the user's profile.
        /// </summary>
        public bool IsFeatured { get; private set; }

        /// <summary>
        /// Determines if this user shelf is used in recommendations.
        /// </summary>
        public bool IsRecommendedFor { get; private set; }

        /// <summary>
        /// Determines if this user shelf is sticky.
        /// </summary>
        public bool Sticky { get; private set; }

        /// <summary>
        /// Determines if this user shelf is editable.
        /// </summary>
        public bool IsEditable { get; private set; }

        /// <summary>
        /// The shelf created date.
        /// </summary>
        public DateTime? CreatedAt { get; private set; }

        /// <summary>
        /// The shelf updated date.
        /// </summary>
        public DateTime? UpdatedAt { get; private set; }

        public override void Parse(XElement element)
        {
            Id = element.ElementAsLong("id");
            Name = element.ElementAsString("name");
            BookCount = element.ElementAsInt("book_count");
            Description = element.ElementAsString("description");
            Sort = element.ElementAsString("sort");
            IsExclusive = element.ElementAsBool("exclusive_flag");
            IsFeatured = element.ElementAsBool("featured");
            IsRecommendedFor = element.ElementAsBool("recommended_for");
            Sticky = element.ElementAsBool("sticky");
            IsEditable = element.ElementAsBool("editable_flag");
            CreatedAt = element.ElementAsDateTime("created_at");
            UpdatedAt = element.ElementAsDateTime("updated_at");

            var orderElement = element.Element("order");
            if (orderElement != null)
            {
                var orderValue = orderElement.Value;
                if (!string.IsNullOrWhiteSpace(orderValue))
                {
                    // if (orderValue == "a")
                    // {
                    //     Order = Response.Order.Ascending;
                    // }
                    // else if (orderValue == "d")
                    // {
                    //     Order = Response.Order.Descending;
                    // }
                }
            }
        }
    }
}
