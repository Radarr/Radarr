using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Parser.Model
{
    public class RemoteAlbum
    {
        public ReleaseInfo Release { get; set; }
        public ParsedAlbumInfo ParsedAlbumInfo { get; set; }
        public Artist Artist { get; set; }
        public List<Album> Albums { get; set; }
        public bool DownloadAllowed { get; set; }
        public TorrentSeedConfiguration SeedConfiguration { get; set; }
        public int PreferredWordScore { get; set; }

        public RemoteAlbum()
        {
            Albums = new List<Album>();
        }

        public bool IsRecentAlbum()
        {
            return Albums.Any(e => e.ReleaseDate >= DateTime.UtcNow.Date.AddDays(-14));
        }

        public override string ToString()
        {
            return Release.Title;
        }
    }
}
