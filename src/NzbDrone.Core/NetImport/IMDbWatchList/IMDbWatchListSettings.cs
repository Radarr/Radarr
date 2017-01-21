using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.IMDbWatchList
{

    public class IMDbWatchListSettings : NetImportBaseSettings
    {
        //private const string helpLink = "https://imdb.com";

        public IMDbWatchListSettings()
        {
            Link = "http://rss.imdb.com/list/";
            ProfileId = 1;
        }

        [FieldDefinition(0, Label = "RSS Link", HelpText = "Link to the rss feed of movies.")]
        public new string Link { get; set; }
    }
}
