using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.Music
{
    public interface IProvideMusicInfo
    {
        ArtistMetadata GetArtistById(string musicBrainzId);
        ArtistMetadata GetArtistByName(string name);
        List<ArtistMetadata> SearchArtists(string query);

        AlbumMetadata GetAlbumById(string musicBrainzId);
        List<AlbumMetadata> GetAlbumsByArtist(string artistMusicBrainzId);
        List<AlbumMetadata> SearchAlbums(string query);

        TrackMetadata GetTrackById(string musicBrainzId);
        List<TrackMetadata> GetTracksByAlbum(string albumMusicBrainzId);
    }

    public class ArtistMetadata
    {
        public string MusicBrainzId { get; set; }
        public string Name { get; set; }
        public string SortName { get; set; }
        public string Disambiguation { get; set; }
        public string Type { get; set; }
        public string Country { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Ended { get; set; }
        public List<string> Genres { get; set; }
        public List<string> Tags { get; set; }
        public string Overview { get; set; }
        public List<ArtistLink> Links { get; set; }
        public List<AlbumMetadata> Albums { get; set; }
    }

    public class ArtistLink
    {
        public string Type { get; set; }
        public string Url { get; set; }
    }

    public class AlbumMetadata
    {
        public string MusicBrainzId { get; set; }
        public string Title { get; set; }
        public string ArtistMusicBrainzId { get; set; }
        public string ArtistName { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string ReleaseType { get; set; }
        public string Status { get; set; }
        public string Country { get; set; }
        public string Label { get; set; }
        public string CatalogNumber { get; set; }
        public string Barcode { get; set; }
        public int? TrackCount { get; set; }
        public int? DiscCount { get; set; }
        public List<string> Genres { get; set; }
        public string CoverUrl { get; set; }
        public List<TrackMetadata> Tracks { get; set; }
    }

    public class TrackMetadata
    {
        public string MusicBrainzId { get; set; }
        public string Title { get; set; }
        public string AlbumMusicBrainzId { get; set; }
        public string ArtistMusicBrainzId { get; set; }
        public string ArtistName { get; set; }
        public int TrackNumber { get; set; }
        public int DiscNumber { get; set; }
        public int? DurationMs { get; set; }
    }
}
