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
        public int AuthorId { get; set; }
        public string SourcePath { get; set; }

        public bool Equals(BulkMoveArtist other)
        {
            if (other == null)
            {
                return false;
            }

            return AuthorId.Equals(other.AuthorId);
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

            return AuthorId.Equals(((BulkMoveArtist)obj).AuthorId);
        }

        public override int GetHashCode()
        {
            return AuthorId.GetHashCode();
        }
    }
}
