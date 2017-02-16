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

namespace NzbDrone.Core.NetImport.Trakt
{
    public class TraktImport : HttpNetImportBase<TraktSettings>
    {
        public override string Name => "Trakt";
        public override bool Enabled => false;
        public override bool EnableAuto => false;
        public IConfigService _configService;

        public TraktImport(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        { _configService = configService; }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new TraktRequestGenerator() { Settings = Settings, _configService = _configService };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new TraktParser(Settings);
        }
    }
}
