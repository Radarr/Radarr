using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Filmweb
{
    public abstract class FilmwebImportBase<TSettings> : HttpImportListBase<TSettings>
        where TSettings : FilmwebSettingsBase<TSettings>, new()
    {
        public override ImportListType ListType => ImportListType.Filmweb;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(12);

        protected FilmwebImportBase(IHttpClient httpClient,
                                   IImportListStatusService importListStatusService,
                                   IConfigService configService,
                                   IParsingService parsingService,
                                   Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override IParseImportListResponse GetParser()
        {
            return new FilmwebParser(_httpClient, Settings.Limit);
        }
    }
}
