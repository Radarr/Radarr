using System;
using System.Collections.Generic;
using System.Linq;
using Equ;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Books
{
    public class Edition : Entity<Edition>
    {
        public Edition()
        {
            Overview = string.Empty;
            Images = new List<MediaCover.MediaCover>();
            Links = new List<Links>();
            Ratings = new Ratings();
        }

        // These correspond to columns in the Books table
        // These are metadata entries
        public int BookId { get; set; }
        public string ForeignEditionId { get; set; }
        public string TitleSlug { get; set; }
        public string Isbn13 { get; set; }
        public string Asin { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }
        public string Overview { get; set; }
        public string Format { get; set; }
        public bool IsEbook { get; set; }
        public string Disambiguation { get; set; }
        public string Publisher { get; set; }
        public int PageCount { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public List<Links> Links { get; set; }
        public Ratings Ratings { get; set; }

        // These are Readarr generated/config
        public bool Monitored { get; set; }
        public bool ManualAdd { get; set; }

        // These are dynamically queried from other tables
        [MemberwiseEqualityIgnore]
        public LazyLoaded<Book> Book { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<List<BookFile>> BookFiles { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ForeignEditionId, Title.NullSafe());
        }

        public override void UseMetadataFrom(Edition other)
        {
            ForeignEditionId = other.ForeignEditionId;
            TitleSlug = other.TitleSlug;
            Isbn13 = other.Isbn13;
            Asin = other.Asin;
            Title = other.Title;
            Language = other.Language;
            Overview = other.Overview.IsNullOrWhiteSpace() ? Overview : other.Overview;
            Format = other.Format;
            IsEbook = other.IsEbook;
            Disambiguation = other.Disambiguation;
            Publisher = other.Publisher;
            PageCount = other.PageCount;
            ReleaseDate = other.ReleaseDate;
            Images = other.Images.Any() ? other.Images : Images;
            Links = other.Links;
            Ratings = other.Ratings;
        }

        public override void UseDbFieldsFrom(Edition other)
        {
            Id = other.Id;
            BookId = other.BookId;
            Book = other.Book;
            Monitored = other.Monitored;
            ManualAdd = other.ManualAdd;
        }

        public override void ApplyChanges(Edition other)
        {
            ForeignEditionId = other.ForeignEditionId;
            Monitored = other.Monitored;
        }
    }
}
