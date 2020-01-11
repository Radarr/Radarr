﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class Newznab : HttpIndexerBase<NewznabSettings>
    {
        private readonly INewznabCapabilitiesProvider _capabilitiesProvider;

        public override string Name => "Newznab";

        public override DownloadProtocol Protocol => DownloadProtocol.Usenet;

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
            return new NewznabRssParser(Settings);
        }

        public override IEnumerable<ProviderDefinition> GetDefaultDefinitions()
        {
            yield return GetDefinition("DOGnzb", GetSettings("https://api.dognzb.cr"));
            yield return GetDefinition("DrunkenSlug", GetSettings("https://api.drunkenslug.com"));
            yield return GetDefinition("Nzb-Tortuga", GetSettings("https://www.nzb-tortuga.com"));
            yield return GetDefinition("Nzb.su", GetSettings("https://api.nzb.su"));
            yield return GetDefinition("NZBCat", GetSettings("https://nzb.cat"));
            yield return GetDefinition("NZBFinder.ws", GetSettings("https://nzbfinder.ws", categories: new[] { 2020,2030,2035,2040,2045,2050,2060,2070,2080,2090 }));
            yield return GetDefinition("NZBgeek", GetSettings("https://api.nzbgeek.info"));
            yield return GetDefinition("nzbplanet.net", GetSettings("https://api.nzbplanet.net"));
            yield return GetDefinition("Nzbs.org", GetSettings("http://nzbs.org"));
            yield return GetDefinition("omgwtfnzbs", GetSettings("https://api.omgwtfnzbs.me"));
            yield return GetDefinition("OZnzb.com", GetSettings("https://api.oznzb.com"));
            yield return GetDefinition("PFmonkey", GetSettings("https://www.pfmonkey.com"));
            yield return GetDefinition("SimplyNZBs", GetSettings("https://simplynzbs.com"));
            yield return GetDefinition("Usenet Crawler", GetSettings("https://www.usenet-crawler.com"));
        }

        public Newznab(INewznabCapabilitiesProvider capabilitiesProvider, IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
            _capabilitiesProvider = capabilitiesProvider;
        }

        private IndexerDefinition GetDefinition(string name, NewznabSettings settings)
        {
            return new IndexerDefinition
                   {
                       EnableRss = false,
                       EnableSearch = false,
                       Name = name,
                       Implementation = GetType().Name,
                       Settings = settings,
                       Protocol = DownloadProtocol.Usenet,
                       SupportsRss = SupportsRss,
                       SupportsSearch = SupportsSearch
                   };
        }

        private NewznabSettings GetSettings(string url, params int[] categories)
        {
            var settings = new NewznabSettings { BaseUrl = url };

            if (categories.Any())
            {
                settings.Categories = categories;
            }

            return settings;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            base.Test(failures);

            failures.AddIfNotNull(TestCapabilities());
        }

        protected static List<int> CategoryIds(List<NewznabCategory> categories)
        {
            var l = categories.Select(c => c.Id).ToList();

            foreach (var category in categories)
            {
                if (category.Subcategories != null)
                    l.AddRange(CategoryIds(category.Subcategories));
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
                        return new ValidationFailure(string.Empty, $"This indexer does not support any of the selected categories! (You may need to turn on advanced settings to see them)");
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
    }
}
