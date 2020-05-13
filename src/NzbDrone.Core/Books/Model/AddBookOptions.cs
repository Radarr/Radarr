using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Books
{
    public class AddBookOptions : IEmbeddedDocument
    {
        public AddBookOptions()
        {
            // default in case not set in db
            AddType = AlbumAddType.Automatic;
        }

        public AlbumAddType AddType { get; set; }
        public bool SearchForNewAlbum { get; set; }
    }

    public enum AlbumAddType
    {
        Automatic,
        Manual
    }
}
