using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Augmenters
{
    public class AugmentWithAdditionalFormats : IAugmentParsedMovieInfo

    {
        public Type HelperType
        {
            get
            {
                return typeof(CustomFormat);
            }
        }

        public ParsedMovieInfo AugmentMovieInfo(ParsedMovieInfo movieInfo, object helper)
        {
            if (helper is CustomFormat format)
            {
                if (movieInfo.ExtraInfo.GetValueOrDefault("AdditionalFormats") is List<CustomFormat> existing)
                {
                    existing.Add(format);
                    movieInfo.ExtraInfo["AdditionalFormats"] = existing;
                }
                else
                {
                    movieInfo.ExtraInfo["AdditionalFormats"] = new List<CustomFormat>{format};
                }
            }

            return movieInfo;
        }
    }
}
