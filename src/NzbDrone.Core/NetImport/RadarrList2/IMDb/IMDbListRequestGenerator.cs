using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport.RadarrList2.IMDbList
{
    public class IMDbListRequestGenerator : RadarrList2RequestGeneratorBase
    {
        public IMDbListSettings Settings { get; set; }

        protected override HttpRequest GetHttpRequest()
        {
            return RequestBuilder.Create()
                .SetSegment("route", $"list/imdb/{Settings.ListId}")
                .Build();
        }
    }
}
