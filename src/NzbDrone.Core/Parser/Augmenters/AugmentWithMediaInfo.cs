using System;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Augmenters
{
    public class AugmentWithMediaInfo : IAugmentParsedMovieInfo

    {
        public Type HelperType
        {
            get
            {
                return typeof(MediaInfoModel);
            }
        }

        public ParsedMovieInfo AugmentMovieInfo(ParsedMovieInfo movieInfo, object helper)
        {
            if (helper is MediaInfoModel mediaInfo)
            {
                var quality = movieInfo.Quality;
                if (!(quality.Modifier == Modifier.BRDISK || quality.Modifier == Modifier.REMUX) &&
                    (quality.Source == Source.BLURAY || quality.Source == Source.TV ||
                     quality.Source == Source.WEBDL) &&
                    !(quality.Resolution == Resolution.R480P || quality.Resolution == Resolution.R576P))
                {
                    var width = mediaInfo.Width;
                    var existing = quality.Resolution;

                    if (width > 854)
                    {
                        quality.Resolution = Resolution.R720P;
                    }

                    if (width > 1280)
                    {
                        quality.Resolution = Resolution.R1080P;
                    }

                    if (width > 1920)
                    {
                        quality.Resolution = Resolution.R2160P;
                    }

                    if (existing != quality.Resolution)
                    {
                        //_logger.Debug("Overwriting resolution info {0} with info from media info {1}", existing, quality.Resolution);
                        quality.QualitySource = QualitySource.MediaInfo;
                        movieInfo.Quality = quality;
                    }
                }

            }

            return movieInfo;
        }
    }
}
