using System;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser.Augmenters
{
    public class AugmentWithOriginalLanguage : IAugmentParsedMovieInfo
    {
        public Type HelperType
        {
            get
            {
                return typeof(Movie);
            }
        }

        public ParsedMovieInfo AugmentMovieInfo(ParsedMovieInfo movieInfo, object helper)
        {
            if (helper is Movie movie && movie?.OriginalLanguage != null && movieInfo != null)
            {
                movieInfo.ExtraInfo["OriginalLanguage"] = movie.OriginalLanguage;
            }

            return movieInfo;
        }
    }
}
