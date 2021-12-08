using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Torznab
{
    public class Torznab : HttpIndexerBase<TorznabSettings>
    {
        private readonly INewznabCapabilitiesProvider _capabilitiesProvider;

        public override string Name => "Torznab";

        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override int PageSize => _capabilitiesProvider.GetCapabilities(Settings).DefaultPageSize;

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new NewznabRequestGenerator(_capabilitiesProvider)
            {
                PageSize = PageSize,
                Settings = Settings
            };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new TorznabRssParser();
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                yield return GetDefinition("Jackett", GetSettings("http://localhost:9117/api/v2.0/indexers/YOURINDEXER/results/torznab/"));
                yield return GetDefinition("HD4Free.xyz", GetSettings("http://hd4free.xyz"));
            }
        }

        public Torznab(INewznabCapabilitiesProvider capabilitiesProvider, IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
            _capabilitiesProvider = capabilitiesProvider;
        }

        private IndexerDefinition GetDefinition(string name, TorznabSettings settings)
        {
            return new IndexerDefinition
            {
                EnableRss = false,
                EnableAutomaticSearch = false,
                EnableInteractiveSearch = false,
                Name = name,
                Implementation = GetType().Name,
                Settings = settings,
                Protocol = DownloadProtocol.Usenet,
                SupportsRss = SupportsRss,
                SupportsSearch = SupportsSearch
            };
        }

        private TorznabSettings GetSettings(string url, string apiPath = null, int[] categories = null)
        {
            var settings = new TorznabSettings { BaseUrl = url };

            if (categories != null)
            {
                settings.Categories = categories;
            }

            if (apiPath.IsNotNullOrWhiteSpace())
            {
                settings.ApiPath = apiPath;
            }

            return settings;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            base.Test(failures);
            if (failures.HasErrors())
            {
                return;
            }

            failures.AddIfNotNull(JackettAll());
            failures.AddIfNotNull(TestCapabilities());
        }

        protected static List<int> CategoryIds(List<NewznabCategory> categories)
        {
            var l = categories.Select(c => c.Id).ToList();

            foreach (var category in categories)
            {
                if (category.Subcategories != null)
                {
                    l.AddRange(CategoryIds(category.Subcategories));
                }
            }

            return l;
        }

        protected virtual ValidationFailure TestCapabilities()
        {
            try
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                var notSupported = Settings.Categories.Except(CategoryIds(capabilities.Categories));

                if (notSupported.Any())
                {
                    _logger.Warn($"{Definition.Name} does not support the following categories: {string.Join(", ", notSupported)}. You should probably remove them.");
                    if (notSupported.Count() == Settings.Categories.Count())
                    {
                        return new ValidationFailure(string.Empty, $"This indexer does not support any of the selected categories! (You may need to turn on advanced settings to see them)");
                    }
                }

                if (capabilities.SupportedSearchParameters != null && capabilities.SupportedSearchParameters.Contains("q"))
                {
                    return null;
                }

                if (capabilities.SupportedMovieSearchParameters != null &&
                    new[] { "q", "imdbid" }.Any(v => capabilities.SupportedMovieSearchParameters.Contains(v)) &&
                    new[] { "imdbtitle", "imdbyear" }.All(v => capabilities.SupportedMovieSearchParameters.Contains(v)))
                {
                    return null;
                }

                return new ValidationFailure(string.Empty, "This indexer does not support searching for movies :(. Tell your indexer staff to enable this or force add the indexer by disabling search, adding the indexer and then enabling it again.");
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to indexer: " + ex.Message);

                return new ValidationFailure(string.Empty, "Unable to connect to indexer, check the log for more details");
            }
        }

        protected virtual ValidationFailure JackettAll()
        {
            if (Settings.ApiPath.Contains("/torznab/all", StringComparison.InvariantCultureIgnoreCase) ||
                Settings.ApiPath.Contains("/api/v2.0/indexers/all/results/torznab", StringComparison.InvariantCultureIgnoreCase) ||
                Settings.BaseUrl.Contains("/torznab/all", StringComparison.InvariantCultureIgnoreCase) ||
                Settings.BaseUrl.Contains("/api/v2.0/indexers/all/results/torznab", StringComparison.InvariantCultureIgnoreCase))
            {
                return new NzbDroneValidationFailure("ApiPath", "Jackett's all endpoint is not supported, please add indexers individually")
                {
                    IsWarning = true,
                    DetailedDescription = "Jackett's all endpoint is not supported, please add indexers individually"
                };
            }

            return null;
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "newznabCategories")
            {
                List<NewznabCategory> categories = null;
                try
                {
                    categories = _capabilitiesProvider.GetCapabilities(Settings).Categories;
                }
                catch
                {
                    // Use default categories
                }

                return new
                {
                    options = NewznabCategoryFieldOptionsConverter.GetFieldSelectOptions(categories)
                };
            }

            return base.RequestAction(action, query);
        }
    }
}
