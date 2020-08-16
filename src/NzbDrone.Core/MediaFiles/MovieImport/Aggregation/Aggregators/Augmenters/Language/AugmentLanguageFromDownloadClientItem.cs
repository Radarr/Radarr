using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Language
{
    public class AugmentLanguageFromDownloadClientItem : IAugmentLanguage
    {
        public int Order => 3;
        public string Name => "DownloadClientItem";

        public AugmentLanguageResult AugmentLanguage(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var languages = localMovie.DownloadClientMovieInfo?.Languages;

            if (languages == null)
            {
                return null;
            }

            return new AugmentLanguageResult(languages, Confidence.DownloadClientItem);
        }
    }
}
