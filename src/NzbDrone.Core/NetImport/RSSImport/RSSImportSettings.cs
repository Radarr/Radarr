using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.RSSImport
{

    public class RSSImportSettings : NetImportBaseSettings
    {
        //private const string helpLink = "https://imdb.com";

        public RSSImportSettings()
        {
            Link = "http://rss.yoursite.com";
        }

        [FieldDefinition(0, Label = "RSS Link", HelpText = "Link to the rss feed of movies.")]
        public new string Link { get; set; }
    }
}
