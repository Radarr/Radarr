﻿using System;
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

namespace NzbDrone.Core.NetImport.CouchPotato
{
    public class CouchPotatoImport : HttpNetImportBase<CouchPotatoSettings>
    {
        public override string Name => "CouchPotato";
        public override bool Enabled => false;
        public override bool EnableAuto => false;

        public CouchPotatoImport(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        { }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new CouchPotatoRequestGenerator() { Settings = Settings };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new CouchPotatoParser(Settings);
        }
    }
}
