using System;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.TMDb
{
    public abstract class TMDbImportListBase<TSettings> : HttpImportListBase<TSettings>
        where TSettings : TMDbSettingsBase<TSettings>, new()
    {
        public override ImportListType ListType => ImportListType.TMDB;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(12);

        public readonly ISearchForNewMovie _skyhookProxy;
        public readonly IHttpRequestBuilderFactory _requestBuilder;

        protected TMDbImportListBase(IRadarrCloudRequestBuilder requestBuilder,
                                    IHttpClient httpClient,
                                    IImportListStatusService importListStatusService,
                                    IConfigService configService,
                                    IParsingService parsingService,
                                    ISearchForNewMovie skyhookProxy,
                                    Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
            _skyhookProxy = skyhookProxy;
            _requestBuilder = requestBuilder.TMDB;
        }
    }
}
