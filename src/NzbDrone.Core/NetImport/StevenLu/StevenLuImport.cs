using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.StevenLu
{
    public class StevenLuImport : HttpNetImportBase<StevenLuSettings>
    {
        public override string Name => "StevenLu";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public StevenLuImport(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, IProvideMovieIdService movieIdService, Logger logger)
            : base(httpClient, configService, parsingService, movieIdService, logger)
        { }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new StevenLuRequestGenerator() { Settings = Settings };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new StevenLuParser(Settings, _movieIdService);
        }
    }
}
