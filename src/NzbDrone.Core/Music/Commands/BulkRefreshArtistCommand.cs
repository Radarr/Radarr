using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Music.Commands
{
    public class BulkRefreshArtistCommand : Command
    {
        public BulkRefreshArtistCommand()
        {
        }

        public BulkRefreshArtistCommand(List<int> artistIds, bool areNewArtists = false)
        {
            ArtistIds = artistIds;
            AreNewArtists = areNewArtists;
        }

        public List<int> ArtistIds { get; set; }
        public bool AreNewArtists { get; set; }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => false;
    }
}
