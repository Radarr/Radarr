using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.TMDb
{
    public abstract class TMDbNetImportBase<TSettings> : HttpNetImportBase<TSettings>
        where TSettings : TMDbSettingsBase<TSettings>, new()
    {
        public override NetImportType ListType => NetImportType.TMDB;

        public readonly ISearchForNewMovie _skyhookProxy;
        public readonly IHttpRequestBuilderFactory _requestBuilder;

        protected TMDbNetImportBase(IRadarrCloudRequestBuilder requestBuilder,
                                    IHttpClient httpClient,
                                    INetImportStatusService netImportStatusService,
                                    IConfigService configService,
                                    IParsingService parsingService,
                                    ISearchForNewMovie skyhookProxy,
                                    Logger logger)
            : base(httpClient, netImportStatusService, configService, parsingService, logger)
        {
            _skyhookProxy = skyhookProxy;
            _requestBuilder = requestBuilder.TMDB;
        }
    }
}
