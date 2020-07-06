using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Language
{
    public interface IAugmentLanguage
    {
        AugmentLanguageResult AugmentLanguage(LocalMovie localMovie);
    }
}
