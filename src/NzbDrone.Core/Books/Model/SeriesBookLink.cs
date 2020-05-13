using Equ;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Books
{
    public class SeriesBookLink : Entity<SeriesBookLink>
    {
        public string Position { get; set; }
        public int SeriesId { get; set; }
        public int BookId { get; set; }
        public bool IsPrimary { get; set; }

        [MemberwiseEqualityIgnore]
        public LazyLoaded<Series> Series { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<Book> Book { get; set; }

        public override void UseMetadataFrom(SeriesBookLink other)
        {
            Position = other.Position;
            IsPrimary = other.IsPrimary;
        }

        public override void UseDbFieldsFrom(SeriesBookLink other)
        {
            Id = other.Id;
            SeriesId = other.SeriesId;
            BookId = other.BookId;
        }
    }
}
