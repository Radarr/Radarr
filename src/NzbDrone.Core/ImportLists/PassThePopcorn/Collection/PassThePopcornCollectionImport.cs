using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.PassThePopcorn.Collection;

public class PassThePopcornCollectionImport : HttpImportListBase<PassThePopcornCollectionSettings>
{
    public override string Name => "PassThePopcorn Collection";
    public override ImportListType ListType => ImportListType.Other;
    public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(12);
    public override int PageSize => 60;
    public override TimeSpan RateLimit => TimeSpan.FromSeconds(10);

    public PassThePopcornCollectionImport(IHttpClient httpClient, IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
        : base(httpClient, importListStatusService, configService, parsingService, logger)
    {
    }

    public override IImportListRequestGenerator GetRequestGenerator()
    {
        return new PassThePopcornCollectionRequestGenerator(Settings);
    }

    public override IParseImportListResponse GetParser()
    {
        return new PassThePopcornCollectionParser();
    }
}
