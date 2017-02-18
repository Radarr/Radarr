using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.PassThePopcorn;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.NetImport.Kitsu
{
    public class KitsuImport : HttpNetImportBase<KitsuSettings>
    {
        public override string Name => "Kitsu Library";
        public override bool Enabled => true;
        public override bool EnableAuto => false;
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public KitsuImport(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new KitsuRequestGenerator()
            {
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient
            };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new KitsuParser(Settings);
        }
    }
}
