using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Movies.Commands
{
    public class RefreshCollectionsCommand : Command
    {
        public List<int> CollectionIds { get; set; }

        public RefreshCollectionsCommand()
        {
            CollectionIds = new List<int>();
        }

        public RefreshCollectionsCommand(List<int> collectionIds)
        {
            CollectionIds = collectionIds;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => CollectionIds.Empty();

        public override bool IsLongRunning => true;

        public override string CompletionMessage => "Completed";
    }
}
