using System.IO;
using Equ;
using NzbDrone.Common.Extensions;
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
        Author = 0,
        Book = 1
    }

    public class MediaCover : MemberwiseEquatable<MediaCover>, IEmbeddedDocument
    {
        private string _url;
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
                if (Extension.IsNullOrWhiteSpace())
                {
                    Extension = Path.GetExtension(value);
                }
            }
        }

        public MediaCoverTypes CoverType { get; set; }
        public string Extension { get; private set; }

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
