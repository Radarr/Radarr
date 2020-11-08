using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityResult
    {
        public string Name { get; set; }
        public Source Source { get; set; }
        public Confidence SourceConfidence { get; set; }
        public int Resolution { get; set; }
        public Confidence ResolutionConfidence { get; set; }
        public Modifier Modifier { get; set; }
        public Confidence ModifierConfidence { get; set; }
        public Revision Revision { get; set; }

        public AugmentQualityResult(Source source,
                                    Confidence sourceConfidence,
                                    int resolution,
                                    Confidence resolutionConfidence,
                                    Modifier modifier,
                                    Confidence modifierConfidence,
                                    Revision revision)
        {
            Source = source;
            SourceConfidence = sourceConfidence;
            Resolution = resolution;
            ResolutionConfidence = resolutionConfidence;
            Modifier = modifier;
            ModifierConfidence = modifierConfidence;
            Revision = revision;
        }

        public static AugmentQualityResult SourceOnly(Source source, Confidence sourceConfidence)
        {
            return new AugmentQualityResult(source, sourceConfidence, 0, Confidence.Default, Modifier.NONE, Confidence.Default, null);
        }

        public static AugmentQualityResult ResolutionOnly(int resolution, Confidence resolutionConfidence)
        {
            return new AugmentQualityResult(Source.UNKNOWN, Confidence.Default, resolution, resolutionConfidence, Modifier.NONE, Confidence.Default, null);
        }

        public static AugmentQualityResult ModifierOnly(Modifier modifier, Confidence modifierConfidence)
        {
            return new AugmentQualityResult(Source.UNKNOWN, Confidence.Default, 0, Confidence.Default, modifier, modifierConfidence, null);
        }
    }
}
