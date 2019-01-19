using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Music
{
    public class SecondaryAlbumType : IEmbeddedDocument, IEquatable<SecondaryAlbumType>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public SecondaryAlbumType()
        {
        }

        private SecondaryAlbumType(int id, string name)
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

        public bool Equals(SecondaryAlbumType other)
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
            return ReferenceEquals(this, obj) || Equals(obj as SecondaryAlbumType);
        }

        public static bool operator ==(SecondaryAlbumType left, SecondaryAlbumType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SecondaryAlbumType left, SecondaryAlbumType right)
        {
            return !Equals(left, right);
        }

        public static SecondaryAlbumType Studio => new SecondaryAlbumType(0, "Studio");
        public static SecondaryAlbumType Compilation => new SecondaryAlbumType(1, "Compilation");
        public static SecondaryAlbumType Soundtrack => new SecondaryAlbumType(2, "Soundtrack");
        public static SecondaryAlbumType Spokenword => new SecondaryAlbumType(3, "Spokenword");
        public static SecondaryAlbumType Interview => new SecondaryAlbumType(4, "Interview");
        public static SecondaryAlbumType Audiobook => new SecondaryAlbumType(5, "Audiobook");
        public static SecondaryAlbumType Live => new SecondaryAlbumType(6, "Live");
        public static SecondaryAlbumType Remix => new SecondaryAlbumType(7, "Remix");
        public static SecondaryAlbumType DJMix => new SecondaryAlbumType(8, "DJ-mix");
        public static SecondaryAlbumType Mixtape => new SecondaryAlbumType(9, "Mixtape/Street");
        public static SecondaryAlbumType Demo => new SecondaryAlbumType(10, "Demo");


        public static readonly List<SecondaryAlbumType> All = new List<SecondaryAlbumType>
        {
            Studio,
            Compilation,
            Soundtrack,
            Spokenword,
            Interview,
            Live,
            Remix,
            DJMix,
            Mixtape,
            Demo
        };


        public static SecondaryAlbumType FindById(int id)
        {
            if (id == 0)
            {
                return Studio;
            }

            SecondaryAlbumType albumType = All.FirstOrDefault(v => v.Id == id);

            if (albumType == null)
            {
                throw new ArgumentException(@"ID does not match a known album type", nameof(id));
            }

            return albumType;
        }

        public static explicit operator SecondaryAlbumType(int id)
        {
            return FindById(id);
        }

        public static explicit operator int(SecondaryAlbumType albumType)
        {
            return albumType.Id;
        }

        public static explicit operator SecondaryAlbumType(string type)
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
