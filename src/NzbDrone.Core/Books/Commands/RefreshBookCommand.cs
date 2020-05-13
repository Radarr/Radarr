using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Books.Commands
{
    public class RefreshBookCommand : Command
    {
        public int? BookId { get; set; }

        public RefreshBookCommand()
        {
        }

        public RefreshBookCommand(int? bookId)
        {
            BookId = bookId;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => !BookId.HasValue;
    }
}
