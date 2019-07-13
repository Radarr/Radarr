using System;
using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Movies.Commands
{
    public class BulkMoveMovieCommand : Command
    {
        public List<BulkMoveMovie> Movies { get; set; }
        public string DestinationRootFolder { get; set; }

        public override bool SendUpdatesToClient => true;
        public override bool RequiresDiskAccess => true;
    }

    public class BulkMoveMovie : IEquatable<BulkMoveMovie>
    {
        public int MovieId { get; set; }
        public string SourcePath { get; set; }

        public bool Equals(BulkMoveMovie other)
        {
            if (other == null)
            {
                return false;
            }

            return MovieId.Equals(other.MovieId);
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

            return MovieId.Equals(((BulkMoveMovie)obj).MovieId);
        }

        public override int GetHashCode()
        {
            return MovieId.GetHashCode();
        }
    }
}