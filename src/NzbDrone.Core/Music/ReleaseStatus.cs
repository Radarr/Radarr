using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Music
{
    public class ReleaseStatus : IEmbeddedDocument, IEquatable<ReleaseStatus>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ReleaseStatus()
        {
        }

        private ReleaseStatus(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(ReleaseStatus other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return ReferenceEquals(this, obj) || Equals(obj as ReleaseStatus);
        }

        public static bool operator ==(ReleaseStatus left, ReleaseStatus right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ReleaseStatus left, ReleaseStatus right)
        {
            return !Equals(left, right);
        }

        public static ReleaseStatus Official => new ReleaseStatus(0, "Official");
        public static ReleaseStatus Promotion => new ReleaseStatus(1, "Promotion");
        public static ReleaseStatus Bootleg => new ReleaseStatus(2, "Bootleg");
        public static ReleaseStatus Pseudo => new ReleaseStatus(3, "Pseudo");


        public static readonly List<ReleaseStatus> All = new List<ReleaseStatus>
        {
            Official,
            Promotion,
            Bootleg,
            Pseudo
        };


        public static ReleaseStatus FindById(int id)
        {
            if (id == 0)
            {
                return Official;
            }

            ReleaseStatus albumType = All.FirstOrDefault(v => v.Id == id);

            if (albumType == null)
            {
                throw new ArgumentException(@"ID does not match a known album type", nameof(id));
            }

            return albumType;
        }

        public static explicit operator ReleaseStatus(int id)
        {
            return FindById(id);
        }

        public static explicit operator int(ReleaseStatus albumType)
        {
            return albumType.Id;
        }

        public static explicit operator ReleaseStatus(string type)
        {
            var releaseStatus = All.FirstOrDefault(v => v.Name.Equals(type, StringComparison.InvariantCultureIgnoreCase));

            if (releaseStatus == null)
            {
                throw new ArgumentException(@"Status does not match a known release status", nameof(type));
            }

            return releaseStatus;
        }
    }
}
