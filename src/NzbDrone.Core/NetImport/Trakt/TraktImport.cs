using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.Trakt
{
    public class TraktImport : HttpNetImportBase<TraktSettings>
    {
        public override string Name => "Trakt List";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        private readonly IHttpClient _httpClient;
        public IConfigService _configService;
        public IProvideMovieIdService movieIdService;

        public TraktImport(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, IProvideMovieIdService movieIdService, Logger logger)
            : base(httpClient, configService, parsingService, movieIdService, logger)
        {
            _configService = configService;
            _httpClient = httpClient;
        }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new TraktRequestGenerator() { Settings = Settings, _configService=_configService, HttpClient = _httpClient, };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new TraktParser(Settings);
        }
    }
}
