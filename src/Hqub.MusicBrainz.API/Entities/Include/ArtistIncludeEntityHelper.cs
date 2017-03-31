using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hqub.MusicBrainz.API.Entities.Include
{
    public static class ArtistIncludeEntityHelper
    {
        public const string Recordings = "recordings";
        public const string Releases = "releases";
        public const string ReleaseGroups = "release-groups";
        public const string Works = "works";
        public const string Tags = "tags";
        public const string Ratings = "ratings";

        // Relations
//        public const string ArtistRelation = "artist-rels";
//        public const string LabelRelation = "label-rels";
//        public const string RecordingRelation = "recording-rels";
//        public const string ReleaseRelation = "release-rels";
//        public const string ReleaseGroupRelation = "release-group-rels";
        public const string UrlRelation = "url-rels";
//        public const string WorkRelation = "work-rels";


        public static bool Check(string incEntity)
        {
            switch (incEntity)
            {
                case Recordings:
                case Releases:
                case ReleaseGroups:
                case Works:
                case Tags:
                case Ratings:
                    return true;
                default:
                    return false;
            }
        }
    }
}
