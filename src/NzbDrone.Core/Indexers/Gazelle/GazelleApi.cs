using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.Gazelle
{
    public class GazelleArtist
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Aliasid { get; set; }
    }

    public class GazelleTorrent
    {
        public int TorrentId { get; set; }
        public int EditionId { get; set; }
        public List<GazelleArtist> Artists { get; set; }
        public bool Remastered { get; set; }
        public string RemasterYear { get; set; }
        public string RemasterTitle { get; set; }
        public string Media { get; set; }
        public string Encoding { get; set; }
        public string Format { get; set; }
        public bool HasLog { get; set; }
        public int LogScore { get; set; }
        public bool HasQueue { get; set; }
        public bool Scene { get; set; }
        public bool VanityHouse { get; set; }
        public int FileCount { get; set; }
        public DateTime Time { get; set; }
        public string Size { get; set; }
        public string Snatches { get; set; }
        public string Seeders { get; set; }
        public string Leechers { get; set; }
        public bool IsFreeLeech { get; set; }
        public bool IsNeutralLeech { get; set; }
        public bool IsPersonalFreeLeech { get; set; }
        public bool CanUseToken { get; set; }
    }

    public class GazelleRelease
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string Artist { get; set; }
        public string GroupYear { get; set; }
        public string Cover { get; set; }
        public List<string> Tags { get; set; }
        public string ReleaseType { get; set; }
        public int TotalLeechers { get; set; }
        public int TotalSeeders { get; set; }
        public int TotalSnatched { get; set; }
        public long MaxSize { get; set; }
        public string GroupTime { get; set; }
        public List<GazelleTorrent> Torrents { get; set; }
    }

    public class GazelleResponse
    {
        public string Status { get; set; }
        public GazelleBrowseResponse Response { get; set; }
    }

    public class GazelleBrowseResponse
    {
        public List<GazelleRelease> Results { get; set; }
        public string CurrentPage { get; set; }
        public string Pages { get; set; }
    }

    public class GazelleAuthResponse
    {
        public string Status { get; set; }
        public GazelleIndexResponse Response { get; set; }

    }

    public class GazelleIndexResponse
    {
        public string Username { get; set; }
        public string Id { get; set; }
        public string Authkey { get; set; }
        public string Passkey { get; set; }

    }

}
