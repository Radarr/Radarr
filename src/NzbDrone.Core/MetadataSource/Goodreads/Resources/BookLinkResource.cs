using System.Diagnostics;
using System.Xml.Linq;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    /// <summary>
    /// This class models a book link as defined by the Goodreads API.
    /// This is usually a link to a third-party site to purchase the book.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class BookLinkResource : GoodreadsResource
    {
        public override string ElementName => "book_link";

        /// <summary>
        /// The Id of this book link.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// The name of this book link provider.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The link to this book on the provider's site.
        /// Be sure to append book_id as a query parameter
        /// to actually be redirected to the correct page.
        /// </summary>
        public string Link { get; private set; }

        public override void Parse(XElement element)
        {
            Id = element.ElementAsLong("id");
            Name = element.ElementAsString("name");
            Link = element.ElementAsString("link");
        }

        /// <summary>
        /// Goodreads returns incomplete book links for some reason.
        /// The link results in an error unless you append a book_id query parameter.
        /// This method fixes up these book links with the given book id.
        /// </summary>
        /// <param name="bookId">The book id to append to the book link.</param>
        internal void FixBookLink(long bookId)
        {
            if (!string.IsNullOrWhiteSpace(Link))
            {
                if (!Link.Contains("book_id"))
                {
                    Link += (Link.Contains("?") ? "&" : "?") + "book_id=" + bookId;
                }
            }
        }
    }
}
