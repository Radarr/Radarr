using System;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser.Augmenters
{
    public class AugmentWithReleaseInfo : IAugmentParsedMovieInfo

    {
        public Type HelperType
        {
            get
            {
                return typeof(ReleaseInfo);
            }
        }

        public ParsedMovieInfo AugmentMovieInfo(ParsedMovieInfo movieInfo, object helper)
        {
            var releaseInfo = helper as ReleaseInfo;

            if (releaseInfo != null)
            {
                // First, let's augment the language!
                var languageTitle = movieInfo.SimpleReleaseTitle;
                if (movieInfo.MovieTitle.IsNotNullOrWhiteSpace())
                {
                    if (languageTitle.ToLower().Contains("multi") && releaseInfo?.IndexerSettings?.MultiLanguages?.Any() == true)
                    {
                        foreach (var i in releaseInfo.IndexerSettings.MultiLanguages)
                        {
                            var language = (Language) i;
                            if (!movieInfo.Languages.Contains(language))
                                movieInfo.Languages.Add(language);
                        }
                    }

                }

                //Next, let's add other useful info to the extra info dict
                if (!movieInfo.ExtraInfo.ContainsKey("Size"))
                {
                    movieInfo.ExtraInfo["Size"] = releaseInfo.Size;
                }
                movieInfo.ExtraInfo["IndexerFlags"] = releaseInfo.IndexerFlags;

            }

            return movieInfo;
        }
    }
}
