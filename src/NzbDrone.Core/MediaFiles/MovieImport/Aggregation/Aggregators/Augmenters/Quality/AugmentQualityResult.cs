using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityResult
    {
        public Source Source { get; set; }
        public Confidence SourceConfidence { get; set; }
        public Resolution Resolution { get; set; }
        public Confidence ResolutionConfidence { get; set; }
        public Revision Revision { get; set; }

        public AugmentQualityResult(Source source,
                                    Confidence sourceConfidence,
                                    Resolution resolution,
                                    Confidence resolutionConfidence,
                                    Revision revision)
        {
            Source = source;
            SourceConfidence = sourceConfidence;
            Resolution = resolution;
            ResolutionConfidence = resolutionConfidence;
            Revision = revision;
        }

        public static AugmentQualityResult SourceOnly(Source source, Confidence sourceConfidence)
        {
            return new AugmentQualityResult(source, sourceConfidence, 0, Confidence.Default, null);
        }

        public static AugmentQualityResult ResolutionOnly(Resolution resolution, Confidence resolutionConfidence)
        {
            return new AugmentQualityResult(Source.UNKNOWN, Confidence.Default, resolution, resolutionConfidence, null);
        }
    }
}
