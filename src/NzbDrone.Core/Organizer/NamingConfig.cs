using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Organizer
{
    public class NamingConfig : ModelBase
    {
        public static NamingConfig Default => new NamingConfig
        {
            RenameEpisodes = false,
            RenameTracks = false,
            ReplaceIllegalCharacters = true,
            MultiEpisodeStyle = 0,
            StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}",
            StandardTrackFormat = "{Artist Name} - {track:00} - {Album Title} - {Track Title}",
            DailyEpisodeFormat = "{Series Title} - {Air-Date} - {Episode Title} {Quality Full}",
            AnimeEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}",
            SeriesFolderFormat = "{Series Title}",
            SeasonFolderFormat = "Season {season}",
            ArtistFolderFormat = "{Artist Name}",
            AlbumFolderFormat = "{Album Title} ({Release Year})"
        };

        public bool RenameEpisodes { get; set; }
        public bool RenameTracks { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public int MultiEpisodeStyle { get; set; }
        public string StandardEpisodeFormat { get; set; }
        public string StandardTrackFormat { get; set; }
        public string DailyEpisodeFormat { get; set; }
        public string AnimeEpisodeFormat { get; set; }
        public string SeriesFolderFormat { get; set; }
        public string SeasonFolderFormat { get; set; }
        public string ArtistFolderFormat { get; set; }
        public string AlbumFolderFormat { get; set; }
    }
}
