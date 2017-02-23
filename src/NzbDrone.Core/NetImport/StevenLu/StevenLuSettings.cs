using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.NetImport.StevenLu
{

    public class StevenLuSettings : NetImportBaseSettings
    {
        public StevenLuSettings()
        {
            Link = "https://s3.amazonaws.com/popular-movies/movies.json";
        }

        [FieldDefinition(0, Label = "URL", HelpText = "Don't change this unless you know what you are doing.")]
        public new string Link { get; set; }

    }

}
