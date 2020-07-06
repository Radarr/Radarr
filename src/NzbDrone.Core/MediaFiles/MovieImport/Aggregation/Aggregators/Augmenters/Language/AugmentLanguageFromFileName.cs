using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Language
{
    public class AugmentLanguageFromFileName : IAugmentLanguage
    {
        public AugmentLanguageResult AugmentLanguage(LocalMovie localMovie)
        {
            var languages = localMovie.FileMovieInfo?.Languages;

            if (languages == null)
            {
                return null;
            }

            return new AugmentLanguageResult(languages, Confidence.Filename);
        }
    }
}
