using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Music
{
    public class ArtistMetadata : ModelBase, IEquatable<ArtistMetadata>
    {
        public ArtistMetadata()
        {
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            Members = new List<Member>();
            Links = new List<Links>();
            OldForeignArtistIds = new List<string>();
            Aliases = new List<string>();
        }

        public string ForeignArtistId { get; set; }
        public List<string> OldForeignArtistIds { get; set; }
        public string Name { get; set; }
        public List<string> Aliases { get; set; }
        public string Overview { get; set; }
        public string Disambiguation { get; set; }
        public string Type { get; set; }
        public ArtistStatusType Status { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public List<Links> Links { get; set; }
        public List<string> Genres { get; set; }
        public Ratings Ratings { get; set; }
        public List<Member> Members { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ForeignArtistId, Name.NullSafe());
        }

        public void ApplyChanges(ArtistMetadata otherArtist)
        {
            ForeignArtistId = otherArtist.ForeignArtistId;
            OldForeignArtistIds = otherArtist.OldForeignArtistIds;
            Name = otherArtist.Name;
            Aliases = otherArtist.Aliases;
            Overview = otherArtist.Overview.IsNullOrWhiteSpace() ? Overview : otherArtist.Overview;
            Disambiguation = otherArtist.Disambiguation;
            Type = otherArtist.Type;
            Status = otherArtist.Status;
            Images = otherArtist.Images.Any() ? otherArtist.Images : Images;
            Links = otherArtist.Links;
            Genres = otherArtist.Genres;
            Ratings = otherArtist.Ratings;
            Members = otherArtist.Members;
        }

        public bool Equals(ArtistMetadata other)
        {
            if (other == null)
            {
                return false;
            }

            if (Id == other.Id &&
                ForeignArtistId == other.ForeignArtistId &&
                (OldForeignArtistIds?.SequenceEqual(other.OldForeignArtistIds) ?? true) &&
                Name == other.Name &&
                (Aliases?.SequenceEqual(other.Aliases) ?? true) &&
                Overview == other.Overview &&
                Disambiguation == other.Disambiguation &&
                Type == other.Type &&
                Status == other.Status &&
                Images?.ToJson() == other.Images?.ToJson() &&
                Links?.ToJson() == other.Links?.ToJson() &&
                (Genres?.SequenceEqual(other.Genres) ?? true) &&
                Ratings?.ToJson() == other.Ratings?.ToJson() &&
                Members?.ToJson() == other.Members?.ToJson())
            {
                return true;
            }
            
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var other = obj as ArtistMetadata;
            if (other == null)
            {
                return false;
            }
            else
            {
                return Equals(other);
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Id;
                hash = hash * 23 + ForeignArtistId.GetHashCode();
                hash = hash * 23 + OldForeignArtistIds.GetHashCode();
                hash = hash * 23 + Name?.GetHashCode() ?? 0;
                hash = hash * 23 + Aliases?.GetHashCode() ?? 0;
                hash = hash * 23 + Overview?.GetHashCode() ?? 0;
                hash = hash * 23 + Disambiguation?.GetHashCode() ?? 0;
                hash = hash * 23 + Type?.GetHashCode() ?? 0;
                hash = hash * 23 + (int)Status;
                hash = hash * 23 + Images?.GetHashCode() ?? 0;
                hash = hash * 23 + Links?.GetHashCode() ?? 0;
                hash = hash * 23 + Genres?.GetHashCode() ?? 0;
                hash = hash * 23 + Ratings?.GetHashCode() ?? 0;
                hash = hash * 23 + Members?.GetHashCode() ?? 0;
                return hash;
            }
        }
    }
}
