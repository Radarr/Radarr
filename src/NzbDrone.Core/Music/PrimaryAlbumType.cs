using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Music
{
    public class PrimaryAlbumType : IEmbeddedDocument, IEquatable<PrimaryAlbumType>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public PrimaryAlbumType()
        {
        }

        private PrimaryAlbumType(int id, string name)
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

        public bool Equals(PrimaryAlbumType other)
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
            return ReferenceEquals(this, obj) || Equals(obj as PrimaryAlbumType);
        }

        public static bool operator ==(PrimaryAlbumType left, PrimaryAlbumType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PrimaryAlbumType left, PrimaryAlbumType right)
        {
            return !Equals(left, right);
        }

        public static PrimaryAlbumType Album => new PrimaryAlbumType(0, "Album");
        public static PrimaryAlbumType EP => new PrimaryAlbumType(1, "EP");
        public static PrimaryAlbumType Single => new PrimaryAlbumType(2, "Single");
        public static PrimaryAlbumType Broadcast => new PrimaryAlbumType(3, "Broadcast");
        public static PrimaryAlbumType Other => new PrimaryAlbumType(4, "Other");


        public static readonly List<PrimaryAlbumType> All = new List<PrimaryAlbumType>
        {
            Album,
            EP,
            Single,
            Broadcast,
            Other
        };


        public static PrimaryAlbumType FindById(int id)
        {
            if (id == 0)
            {
                return Album;
            }

            PrimaryAlbumType albumType = All.FirstOrDefault(v => v.Id == id);

            if (albumType == null)
            {
                throw new ArgumentException(@"ID does not match a known album type", nameof(id));
            }

            return albumType;
        }

        public static explicit operator PrimaryAlbumType(int id)
        {
            return FindById(id);
        }

        public static explicit operator int(PrimaryAlbumType albumType)
        {
            return albumType.Id;
        }

        public static explicit operator PrimaryAlbumType(string type)
        {
            var albumType = All.FirstOrDefault(v => v.Name.Equals(type, StringComparison.InvariantCultureIgnoreCase));

            if (albumType == null)
            {
                throw new ArgumentException(@"Type does not match a known album type", nameof(type));
            }

            return albumType;
        }
    }
}
