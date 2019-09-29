using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.CustomFormats;

namespace NzbDrone.Core.Qualities
{
    public static class QualityFinder
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(QualityFinder));

        public static Quality FindBySourceAndResolution(Source source, Resolution resolution, Modifier modifer)
        {
            var matchingQuality = Quality.All.SingleOrDefault(q => q.Source == source && q.Resolution == resolution && q.Modifier == modifer);

            if (matchingQuality != null)
            {
                return matchingQuality;
            }

            var matchingModifier = Quality.All.Where(q => q.Modifier == modifer);

            var matchingResolution = matchingModifier.Where(q => q.Resolution == resolution)
                                            .OrderBy(q => q.Source)
                                            .ToList();

            var nearestQuality = Quality.Unknown;

            foreach (var quality in matchingResolution)
            {
                if (quality.Source >= source)
                {
                    nearestQuality = quality;
                    break;
                }
            }

            Logger.Warn("Unable to find exact quality for {0},  {1}, and {2}. Using {3} as fallback", source, resolution, modifer, nearestQuality);

            return nearestQuality;
        }
    }
}
