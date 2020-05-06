using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class MissingAlbumSearchCommand : Command
    {
        public int? AuthorId { get; set; }

        public override bool SendUpdatesToClient => true;

        public MissingAlbumSearchCommand()
        {
        }

        public MissingAlbumSearchCommand(int authorId)
        {
            AuthorId = authorId;
        }
    }
}
