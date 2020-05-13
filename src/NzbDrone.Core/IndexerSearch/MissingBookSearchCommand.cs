using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.IndexerSearch
{
    public class MissingBookSearchCommand : Command
    {
        public int? AuthorId { get; set; }

        public override bool SendUpdatesToClient => true;

        public MissingBookSearchCommand()
        {
        }

        public MissingBookSearchCommand(int authorId)
        {
            AuthorId = authorId;
        }
    }
}
