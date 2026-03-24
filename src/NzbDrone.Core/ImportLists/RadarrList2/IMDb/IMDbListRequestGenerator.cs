using System;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.RadarrList2.IMDbList
{
    public class IMDbListRequestGenerator : RadarrList2RequestGeneratorBase
    {
        public IMDbListSettings Settings { get; set; }

        protected override HttpRequest GetHttpRequest()
        {
            Logger.Info("IMDb List {0}: Importing movies", Settings.ListId);

            // Use IMDb list Export for user lists to bypass RadarrAPI caching
            if (Settings.ListId.StartsWith("ls", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("IMDb lists of the form 'ls12345678' are no longer supported. Feel free to remove this list after you review your Clean Library Level.");
            }

            var request = RequestBuilder.Create()
                .SetSegment("route", $"list/imdb/{Settings.ListId}")
                .Accept(HttpAccept.Json)
                .Build();

            Logger.Trace("IMDb List {0}: Request URL: {1}", Settings.ListId, request.Url);

            return request;
        }
    }
}
