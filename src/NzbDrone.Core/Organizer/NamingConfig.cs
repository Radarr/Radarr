using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Organizer
{
    public class NamingConfig : ModelBase
    {
        public static NamingConfig Default => new NamingConfig
        {
            RenameEpisodes = false,
            ReplaceIllegalCharacters = true,
            MultiEpisodeStyle = 0,
            MovieFolderFormat = "{Movie Title} ({Release Year})",
            StandardMovieFormat = "{Movie Title} ({Release Year}) {Quality Full}",
        };

        public bool RenameEpisodes { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public int MultiEpisodeStyle { get; set; }
        public string StandardMovieFormat { get; set; }
        public string MovieFolderFormat { get; set; }
    }
}
