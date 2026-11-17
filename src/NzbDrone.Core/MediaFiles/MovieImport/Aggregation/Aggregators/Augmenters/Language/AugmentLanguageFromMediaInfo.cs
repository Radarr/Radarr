using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Language
{
    public class AugmentLanguageFromMediaInfo : IAugmentLanguage
    {
        private readonly Logger _logger;

        public int Order => 4;
        public string Name => "MediaInfo";

        public AugmentLanguageFromMediaInfo(Logger logger)
        {
            _logger = logger;
        }

        public AugmentLanguageResult AugmentLanguage(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (localMovie.MediaInfo == null)
            {
                return null;
            }

            var audioLanguages = localMovie.MediaInfo.AudioStreams?
                .Where(s =>
                {
                    if (s.Title.IsNotNullOrWhiteSpace() && s.Title.Contains("commentary", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.Trace("Ignoring audio language {Language} with commentary: '{Title}'", s.Language, s.Title);
                        return false;
                    }

                    return true;
                })
                .Select(s => s.Language)
                .Distinct()
                .ToList() ?? [];

            var languages = new List<Languages.Language>();

            foreach (var audioLanguage in audioLanguages)
            {
                var language = IsoLanguages.Find(audioLanguage)?.Language;
                languages.AddIfNotNull(language);
            }

            if (languages.Count == 0)
            {
                return null;
            }

            return new AugmentLanguageResult(languages, Confidence.MediaInfo);
        }
    }
}
