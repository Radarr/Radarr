using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Books
{
    public class AddBookOptions : IEmbeddedDocument
    {
        public AddBookOptions()
        {
            // default in case not set in db
            AddType = BookAddType.Automatic;
        }

        public BookAddType AddType { get; set; }
        public bool SearchForNewBook { get; set; }
    }

    public enum BookAddType
    {
        Automatic,
        Manual
    }
}
