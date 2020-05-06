using System.IO;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Organizer
{
    public class NamingConfig : ModelBase
    {
        public static NamingConfig Default => new NamingConfig
        {
            RenameTracks = false,
            ReplaceIllegalCharacters = true,
            StandardTrackFormat = "{Album Title}" + Path.DirectorySeparatorChar + "{Artist Name} - {Album Title}",
            ArtistFolderFormat = "{Artist Name}",
        };

        public bool RenameTracks { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public string StandardTrackFormat { get; set; }
        public string ArtistFolderFormat { get; set; }
    }
}
