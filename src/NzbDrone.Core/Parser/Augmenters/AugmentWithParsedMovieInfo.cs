using System;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Augmenters
{
    //TODO: Create tests for this augmenter!
    public class AugmentWithParsedMovieInfo : IAugmentParsedMovieInfo
    {
        public Type HelperType
        {
            get
            {
                return typeof(ParsedMovieInfo);
            }
        }

        public ParsedMovieInfo AugmentMovieInfo(ParsedMovieInfo movieInfo, object helper)
        {
            if (helper is ParsedMovieInfo otherInfo)
            {
                // Create union of all languages
                movieInfo.Languages = movieInfo.Languages.Union(otherInfo.Languages).Distinct().ToList();

                if (otherInfo.Edition?.Length > movieInfo.Edition?.Length)
                {
                    movieInfo.Edition = otherInfo.Edition;
                }

                movieInfo.Quality.CustomFormats = movieInfo.Quality.CustomFormats.Union(otherInfo.Quality.CustomFormats)
                    .Distinct().ToList();
            }

            return movieInfo;
        }
    }
}
