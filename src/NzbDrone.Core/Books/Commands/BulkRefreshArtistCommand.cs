using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Music.Commands
{
    public class BulkRefreshArtistCommand : Command
    {
        public BulkRefreshArtistCommand()
        {
        }

        public BulkRefreshArtistCommand(List<int> authorIds, bool areNewArtists = false)
        {
            AuthorIds = authorIds;
            AreNewArtists = areNewArtists;
        }

        public List<int> AuthorIds { get; set; }
        public bool AreNewArtists { get; set; }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => false;
    }
}
