using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Organizer
{
    public class NamingConfig : ModelBase
    {
        public static NamingConfig Default => new NamingConfig
        {
            RenameMovies = false,
            ReplaceIllegalCharacters = true,
            ColonReplacementFormat = 0,
            MultiEpisodeStyle = 0,
            MovieFolderFormat = "{Movie Title:EN} ({Release Year})",
            StandardMovieFormat = "{Movie Title:EN} ({Release Year}) {Quality Full}",
        };

        public bool RenameMovies { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public ColonReplacementFormat ColonReplacementFormat { get; set; }
        public int MultiEpisodeStyle { get; set; }
        public string StandardMovieFormat { get; set; }
        public string MovieFolderFormat { get; set; }
    }

    public enum ColonReplacementFormat
    {
        Delete = 0,
        Dash = 1,
        SpaceDash = 2,
        SpaceDashSpace = 3
    }

    public static class ColonReplacementFormatMethods
    {
        public static string GetFormatString(this ColonReplacementFormat format)
        {
            switch (format)
            {
                case ColonReplacementFormat.Delete:
                    return "";
                case ColonReplacementFormat.Dash:
                    return "-";
                case ColonReplacementFormat.SpaceDash:
                    return " -";
                case ColonReplacementFormat.SpaceDashSpace:
                    return " - ";
                default:
                    return "";
            }
        }
    }
}
