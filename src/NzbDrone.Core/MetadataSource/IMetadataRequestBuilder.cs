using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.MetadataSource
{

    public interface IMetadataRequestBuilder
    {
        IHttpRequestBuilderFactory GetRequestBuilder();
    }
    public class MetadataRequestBuilder : IMetadataRequestBuilder
    {
        private readonly IConfigService _configService;

        private readonly ILidarrCloudRequestBuilder _defaultRequestFactory;

        public MetadataRequestBuilder(IConfigService configService, ILidarrCloudRequestBuilder defaultRequestBuilder)
        {
            _configService = configService;
            _defaultRequestFactory = defaultRequestBuilder;
        }

        public IHttpRequestBuilderFactory GetRequestBuilder()
        {
            if (_configService.MetadataSource.IsNotNullOrWhiteSpace())
            {
                return new HttpRequestBuilder(_configService.MetadataSource.TrimEnd("/") + "/{route}").KeepAlive().CreateFactory();
            }
            else
            {
                return _defaultRequestFactory.Search;
            }
        }
    }
}
