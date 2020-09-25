using System;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.RadarrList2.IMDbList
{
    public class IMDbListRequestGenerator : RadarrList2RequestGeneratorBase
    {
        public IMDbListSettings Settings { get; set; }

        protected override HttpRequest GetHttpRequest()
        {
            //Use IMDb list Export for user lists to bypass RadarrAPI caching
            if (Settings.ListId.StartsWith("ls", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpRequest($"https://www.imdb.com/list/{Settings.ListId}/export", new HttpAccept("*/*"));
            }

            return RequestBuilder.Create()
                .SetSegment("route", $"list/imdb/{Settings.ListId}")
                .Accept(HttpAccept.Json)
                .Build();
        }
    }
}
