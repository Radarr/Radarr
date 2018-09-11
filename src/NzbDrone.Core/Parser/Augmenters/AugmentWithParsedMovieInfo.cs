using System;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Augmenters
{
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
                if (otherInfo.Languages != null)
                {
                    movieInfo.Languages = movieInfo.Languages.Union(otherInfo.Languages).Distinct().ToList();
                }

                if ((otherInfo.Edition?.Length ?? 0) > (movieInfo.Edition?.Length ?? 0))
                {
                    movieInfo.Edition = otherInfo.Edition;
                }

                if (otherInfo.Quality != null)
                {
                    movieInfo.Quality.CustomFormats = movieInfo.Quality.CustomFormats.Union(otherInfo.Quality.CustomFormats)
                        .Distinct().ToList();
                }

                if (otherInfo.ReleaseGroup.IsNotNullOrWhiteSpace() && movieInfo.ReleaseGroup.IsNullOrWhiteSpace())
                {
                    movieInfo.ReleaseGroup = otherInfo.ReleaseGroup;
                }
            }

            return movieInfo;
        }
    }
}
