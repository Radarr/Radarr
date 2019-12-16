using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Music
{
    public class AddAlbumOptions : IEmbeddedDocument
    {
        public AddAlbumOptions()
        {
            // default in case not set in db
            AddType = AlbumAddType.Automatic;
        }

        public AlbumAddType AddType { get; set; }
        public bool SearchForNewAlbum { get; set;  }
    }

    public enum AlbumAddType
    {
        Automatic,
        Manual
    }
}
