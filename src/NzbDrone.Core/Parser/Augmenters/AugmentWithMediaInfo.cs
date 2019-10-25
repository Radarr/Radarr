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
                if (!(quality.Quality.Modifier == Modifier.BRDISK || quality.Quality.Modifier == Modifier.REMUX) &&
                    (quality.Quality.Source == Source.BLURAY || quality.Quality.Source == Source.TV ||
                     quality.Quality.Source == Source.WEBDL) &&
                    !(quality.Quality.Resolution == Resolution.R480P || quality.Quality.Resolution == Resolution.R576P))
                {
                    var width = mediaInfo.Width;
                    var existing = quality.Quality.Resolution;

                    if (width > 854)
                    {
                        quality.Quality.Resolution = Resolution.R720P;
                    }

                    if (width > 1280)
                    {
                        quality.Quality.Resolution = Resolution.R1080P;
                    }

                    if (width > 1920)
                    {
                        quality.Quality.Resolution = Resolution.R2160P;
                    }

                    if (existing != quality.Quality.Resolution)
                    {
                        //_logger.Debug("Overwriting resolution info {0} with info from media info {1}", existing, quality.Resolution);
                        quality.QualityDetectionSource = QualityDetectionSource.MediaInfo;
                        movieInfo.Quality = quality;
                    }
                }

            }

            return movieInfo;
        }
    }
}
