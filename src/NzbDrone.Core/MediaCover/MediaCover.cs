using System.IO;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MediaCover
{

    public enum MediaCoverTypes
    {
        Unknown = 0,
        Poster = 1,
        Banner = 2,
        Fanart = 3,
        Screenshot = 4,
        Headshot = 5,
        Cover = 6,
        Disc = 7,
        Logo = 8
    }

    public enum MediaCoverEntity
    {
        Artist = 0,
        Album = 1
    }

    public class MediaCover : IEmbeddedDocument
    {
        public MediaCoverTypes CoverType { get; set; }
        public string Url { get; set; }
        public string Extension => Path.GetExtension(Url);

        public MediaCover()
        {
        }

        public MediaCover(MediaCoverTypes coverType, string url)
        {
            CoverType = coverType;
            Url = url;
        }
    }
}
