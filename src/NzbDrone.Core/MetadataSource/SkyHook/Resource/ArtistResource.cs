using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class StorePlatformDataResource
    {
        public StorePlatformDataResource() { }
        public ArtistInfoResource Artist { get; set; }
        //public Lockup lockup { get; set; }
    }

    public class ArtistInfoResource
    {
        public ArtistInfoResource() { }
        public Dictionary<int, ArtistInfoResource> Results { get; set; }

        public bool HasArtistBio { get; set; }

        public string url { get; set; }
        public string shortUrl { get; set; }
       
        public List<string> artistContemporaries { get; set; }
        public List<string> genreNames { get; set; }
        public bool hasSocialPosts { get; set; }
        public string artistBio { get; set; }
        public bool isGroup { get; set; }
        public string id { get; set; }
        public string bornOrFormed { get; set; }
        public string name { get; set; }
        public string latestAlbumContentId { get; set; }
        public string nameRaw { get; set; }

        //public string kind { get; set; }
        //public List<Gallery> gallery { get; set; }
        //public List<Genre> genres { get; set; }
        public List<object> artistInfluencers { get; set; }
        public List<object> artistFollowers { get; set; }
        //public string umcArtistImageUrl { get; set; }
    }

    public class AlbumResource
    { 
        public AlbumResource()
        {

        }

        public string ArtistName { get; set; }
        public int ArtistId { get; set; }
        public string CollectionName { get; set; }
        public int CollectionId { get; set; }
        public string PrimaryGenreName { get; set; }
        public string ArtworkUrl100 { get; set; }
        public string Country { get; set; }
        public string CollectionExplicitness { get; set; }
        public int TrackCount { get; set; }
        public string Copyright { get; set; }
        public DateTime ReleaseDate { get; set; }

    }

    public class ArtistResource
    {
        public ArtistResource()
        {

        }

        public int ResultCount { get; set; }
        public List<AlbumResource> Results { get; set; }
        //public string ArtistName { get; set; }
        //public List<AlbumResource> Albums { get; set; }
        public StorePlatformDataResource StorePlatformData { get; set; }
    }
}
