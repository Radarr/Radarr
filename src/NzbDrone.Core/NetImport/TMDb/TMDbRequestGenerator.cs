using NzbDrone.Common.Http;
using System.Collections.Generic;

namespace NzbDrone.Core.NetImport.TMDb
{
    public class TMDbRequestGenerator : INetImportRequestGenerator
    {
        public TMDbSettings Settings { get; set; }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var searchType = "";

            switch (Settings.ListType)
            {
                case (int)TMDbListType.Theaters:
                    searchType = "/3/movie/now_playing";
                    break;
                case (int)TMDbListType.Popular:
                    searchType = "/3/movie/popular";
                    break;
                case (int)TMDbListType.Top:
                    searchType = "/3/movie/top_rated";
                    break;
                case (int)TMDbListType.Upcoming:
                    searchType = "/3/movie/upcoming";
                    break;
            }

            var pageableRequests = new NetImportPageableRequestChain();
            pageableRequests.Add(GetPagedRequests(searchType));
            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetPagedRequests(string searchType)
        {
            var baseUrl = $"{Settings.Link.Trim()}{searchType}?api_key=1a7373301961d03f97f853a876dd1212";
            for (var page = 1; page < 100; page++)
            {
                yield return new NetImportRequest($"{baseUrl}&page={page}", HttpAccept.Json);
            }
        }
    }
}
