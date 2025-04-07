using System;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.RadarrList2.IMDbList
{
    public class IMDbListRequestGenerator : RadarrList2RequestGeneratorBase
    {
        public IMDbListSettings Settings { get; set; }

        protected override HttpRequest GetHttpRequest()
        {
            // Use IMDb list Export for user lists to bypass RadarrAPI caching
            if (Settings.ListId.StartsWith("ls", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("IMDb lists of the form 'ls12345678' are no longer supported. Feel free to remove this list after you review your Clean Library Level.");
            }

            return RequestBuilder.Create()
                .SetSegment("route", $"list/imdb/{Settings.ListId}")
                .Accept(HttpAccept.Json)
                .Build();
        }
    }
}
