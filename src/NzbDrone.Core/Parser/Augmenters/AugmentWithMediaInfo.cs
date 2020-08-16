using System;
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
                    !(quality.Quality.Resolution == (int)Resolution.R480p || quality.Quality.Resolution == (int)Resolution.R576p))
                {
                    var width = mediaInfo.Width;
                    var existing = quality.Quality.Resolution;

                    if (width > 854)
                    {
                        quality.Quality.Resolution = (int)Resolution.R720p;
                    }

                    if (width > 1280)
                    {
                        quality.Quality.Resolution = (int)Resolution.R1080p;
                    }

                    if (width > 1920)
                    {
                        quality.Quality.Resolution = (int)Resolution.R2160p;
                    }

                    if (existing != quality.Quality.Resolution)
                    {
                        //_logger.Debug("Overwriting resolution info {0} with info from media info {1}", existing, quality.Resolution);
                        quality.ResolutionDetectionSource = QualityDetectionSource.MediaInfo;
                        movieInfo.Quality = quality;
                    }
                }
            }

            return movieInfo;
        }
    }
}
