using Marr.Data;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Profiles.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public class ArtistMetadata : ModelBase
    {
        public ArtistMetadata()
        {
            Images = new List<MediaCover.MediaCover>();
            Genres = new List<string>();
            Members = new List<Member>();
            Links = new List<Links>();
        }

        public string ForeignArtistId { get; set; }
        public string Name { get; set; }
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
            Name = otherArtist.Name;
            Overview = otherArtist.Overview;
            Disambiguation = otherArtist.Disambiguation;
            Type = otherArtist.Type;
            Status = otherArtist.Status;
            Images = otherArtist.Images;
            Links = otherArtist.Links;
            Genres = otherArtist.Genres;
            Ratings = otherArtist.Ratings;
            Members = otherArtist.Members;
        }
    }
}
