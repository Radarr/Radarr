using System;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser.Augmenters
{
    public class AugmentWithReleaseInfo : IAugmentParsedMovieInfo
    {
        private readonly Lazy<IIndexerFactory> _indexerFactory;

        public AugmentWithReleaseInfo(Lazy<IIndexerFactory> indexerFactory)
        {
            _indexerFactory = indexerFactory;
        }

        public Type HelperType
        {
            get
            {
                return typeof(ReleaseInfo);
            }
        }

        public ParsedMovieInfo AugmentMovieInfo(ParsedMovieInfo movieInfo, object helper)
        {
            if (helper is ReleaseInfo releaseInfo)
            {
                IIndexerSettings indexerSettings = null;
                try
                {
                    indexerSettings = _indexerFactory.Value.Get(releaseInfo.IndexerId)?.Settings as IIndexerSettings;
                }
                catch (Exception)
                {
                    //_logger.Debug("Indexer with id {0} does not exist, skipping minimum seeder checks.", subject.Release.IndexerId);
                }                // First, let's augment the language!

                var languageTitle = movieInfo.SimpleReleaseTitle;
                if (movieInfo.PrimaryMovieTitle.IsNotNullOrWhiteSpace())
                {
                    if (languageTitle.ToLower().Contains("multi") && indexerSettings?.MultiLanguages?.Any() == true)
                    {
                        foreach (var i in indexerSettings.MultiLanguages)
                        {
                            var language = (Language)i;
                            if (!movieInfo.Languages.Contains(language))
                            {
                                movieInfo.Languages.Add(language);
                            }
                        }
                    }
                }

                //Next, let's add other useful info to the extra info dict
                if (!movieInfo.ExtraInfo.ContainsKey("Size"))
                {
                    movieInfo.ExtraInfo["Size"] = releaseInfo.Size;
                }

                movieInfo.ExtraInfo["IndexerFlags"] = releaseInfo.IndexerFlags;
            }

            return movieInfo;
        }
    }
}
