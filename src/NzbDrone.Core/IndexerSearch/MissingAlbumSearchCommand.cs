using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class MissingAlbumSearchCommand : Command
    {
        public int? ArtistId { get; set; }

        public override bool SendUpdatesToClient => true;

        public MissingAlbumSearchCommand()
        {
        }

        public MissingAlbumSearchCommand(int artistId)
        {
            ArtistId = artistId;
        }
    }
}
