using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class CutoffUnmetAlbumSearchCommand : Command
    {
        public int? AuthorId { get; set; }

        public override bool SendUpdatesToClient => true;

        public CutoffUnmetAlbumSearchCommand()
        {
        }

        public CutoffUnmetAlbumSearchCommand(int authorId)
        {
            AuthorId = authorId;
        }
    }
}
