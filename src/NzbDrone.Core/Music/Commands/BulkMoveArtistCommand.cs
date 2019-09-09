using System;
using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Music.Commands
{
    public class BulkMoveArtistCommand : Command
    {
        public List<BulkMoveArtist> Artist { get; set; }
        public string DestinationRootFolder { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;
    }

    public class BulkMoveArtist : IEquatable<BulkMoveArtist>
    {
        public int ArtistId { get; set; }
        public string SourcePath { get; set; }

        public bool Equals(BulkMoveArtist other)
        {
            if (other == null)
            {
                return false;
            }

            return ArtistId.Equals(other.ArtistId);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return ArtistId.Equals(((BulkMoveArtist)obj).ArtistId);
        }

        public override int GetHashCode()
        {
            return ArtistId.GetHashCode();
        }
    }
}
