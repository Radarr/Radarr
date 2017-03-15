using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;


namespace NzbDrone.Core.NetImport.TMDb
{
    public class TMDbImport : HttpNetImportBase<TMDbSettings>
    {
        public override string Name => "TMDb Lists";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        private readonly IHttpClient _httpClient;
        private readonly IProvideMovieIdService _movieIdService;
        private readonly Logger _logger;

        public TMDbImport(IHttpClient httpClient, 
            IConfigService configService, 
            IParsingService parsingService,
            IProvideMovieIdService movieIdService,
            Logger logger)
            : base(httpClient, configService, parsingService, movieIdService, logger)
        {
            _movieIdService = movieIdService;
            _logger = logger;
            _httpClient = httpClient;
        }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new TMDbRequestGenerator()
            {
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient
            };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new TMDbParser(Settings, _movieIdService);
        }
    }
}