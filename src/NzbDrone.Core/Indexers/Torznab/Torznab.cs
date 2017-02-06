﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

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
                       EnableSearch = false,
                       Name = name,
                       Implementation = GetType().Name,
                       Settings = settings,
                       Protocol = DownloadProtocol.Usenet,
                       SupportsRss = SupportsRss,
                       SupportsSearch = SupportsSearch
                   };
        }

        private TorznabSettings GetSettings(string url, params int[] categories)
        {
            var settings = new TorznabSettings { Url = url };

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

        protected virtual ValidationFailure TestCapabilities()
        {
            try
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                if (capabilities.SupportedSearchParameters != null && capabilities.SupportedSearchParameters.Contains("q"))
                {
                    return null;
                }

                if (capabilities.SupportedMovieSearchParameters != null &&
                    new[] { "q", "imdbid" }.Any(v => capabilities.SupportedMovieSearchParameters.Contains(v)))
                {
                    return null;
                }

                return new ValidationFailure(string.Empty, "Indexer does not support required search parameters");
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to indexer: " + ex.Message);

                return new ValidationFailure(string.Empty, "Unable to connect to indexer, check the log for more details");
            }
        }

    }
}
