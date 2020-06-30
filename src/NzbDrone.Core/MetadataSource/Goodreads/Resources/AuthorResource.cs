using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    /// <summary>
    /// This class models an Author as defined by the Goodreads API.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class AuthorResource : GoodreadsResource
    {
        public override string ElementName => "author";

        /// <summary>
        /// The Goodreads Author Id.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// The full name of the author.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The Url to the Goodreads author page.
        /// </summary>
        public string Link { get; private set; }

        /// <summary>
        /// The number of fans for this author.
        /// The Goodreads Fan API has been replaced by Followers.
        /// For this property, use FollowersCount instead.
        /// </summary>
        [Obsolete("Fans API has been deprecated by Goodreads. Use Followers instead.")]
        public int FansCount { get; private set; }

        /// <summary>
        /// The number of Goodreads users that are following this author.
        /// </summary>
        public int FollowersCount { get; private set; }

        /// <summary>
        /// The Url to the author's image, large size.
        /// </summary>
        public string LargeImageUrl { get; private set; }

        /// <summary>
        /// The Url to the author's image.
        /// </summary>
        public string ImageUrl { get; private set; }

        /// <summary>
        /// The Url to the author's image, small size.
        /// </summary>
        public string SmallImageUrl { get; private set; }

        /// <summary>
        /// A brief description about this author. This field may contain HTML.
        /// </summary>
        public string About { get; private set; }

        /// <summary>
        /// People that may have influenced this author. This field may contain HTML.
        /// </summary>
        public string Influences { get; private set; }

        /// <summary>
        /// The total number of items the author has worked on and are listed within Goodreads.
        /// </summary>
        public int WorksCount { get; private set; }

        /// <summary>
        /// The gender of the author. This field might be limited to only "male" and "female"
        /// but is left as a string in case any other options are possible through the Goodreads API.
        /// </summary>
        public string Gender { get; private set; }

        /// <summary>
        /// The hometown the author grew up in.
        /// </summary>
        public string Hometown { get; private set; }

        /// <summary>
        /// The author's birthdate.
        /// </summary>
        public DateTime? BornOnDate { get; private set; }

        /// <summary>
        /// The date on which the author died.
        /// </summary>
        public DateTime? DiedOnDate { get; private set; }

        /// <summary>
        /// Determines whether this author is also a regular Goodreads user or not.
        /// </summary>
        public bool IsGoodreadsAuthor { get; private set; }

        /// <summary>
        /// If <see cref="IsGoodreadsAuthor"/> is true, this property is set to the author's Goodreads user Id.
        /// </summary>
        public int? GoodreadsUserId { get; private set; }

        internal string DebuggerDisplay
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Author: Id: {0}, Name: {1}",
                    Id,
                    Name);
            }
        }

        public override void Parse(XElement element)
        {
            Id = element.ElementAsLong("id");
            Name = element.ElementAsString("name");
            Link = element.ElementAsString("link");
            FollowersCount = element.ElementAsInt("author_followers_count");
            LargeImageUrl = element.ElementAsString("large_image_url");
            ImageUrl = element.ElementAsString("image_url");
            SmallImageUrl = element.ElementAsString("small_image_url");
            About = element.ElementAsString("about");
            Influences = element.ElementAsString("influences");
            WorksCount = element.ElementAsInt("works_count");
            Gender = element.ElementAsString("gender");
            Hometown = element.ElementAsString("hometown");
            BornOnDate = element.ElementAsDate("born_at");
            DiedOnDate = element.ElementAsDate("died_at");

            IsGoodreadsAuthor = element.ElementAsBool("goodreads_author");
            if (IsGoodreadsAuthor)
            {
                var goodreadsUser = element.Element("user");
                if (goodreadsUser != null)
                {
                    GoodreadsUserId = goodreadsUser.ElementAsInt("id");
                }
            }
        }
    }
}
