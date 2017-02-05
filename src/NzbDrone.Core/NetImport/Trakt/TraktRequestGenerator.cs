using NzbDrone.Common.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace NzbDrone.Core.NetImport.Trakt
{
    public class TraktRequestGenerator : INetImportRequestGenerator
    {
        public TraktSettings Settings { get; set; }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();

            pageableRequests.Add(GetMovies(null));

            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetMovies(string searchParameters)
        {
            var link = Settings.Link.Trim();

            switch (Settings.ListType)
            {
                case (int)TraktListType.UserCustomList:
                    link = link + $"/users/{Settings.Username.Trim()}/lists/{Settings.Listname.Trim()}/items/movies";
                    break;
                case (int)TraktListType.UserWatchList:
                    link = link + $"/users/{Settings.Username.Trim()}/watchlist/movies";
                    break;
                case (int)TraktListType.UserWatchedList:
                    link = link + $"/users/{Settings.Username.Trim()}/watched/movies";
                    break;
                case (int)TraktListType.TrendingMovies:
                    link = link + "/movies/trending";
                    break;
                case (int)TraktListType.PopularMovies:
                    link = link + "/movies/popular";
                    break;
                case (int)TraktListType.AnticipatedMovies:
                    link = link + "/movies/anticipated";
                    break;
                case (int)TraktListType.BoxOfficeMovies:
                    link = link + "/movies/boxoffice";
                    break;
                case (int)TraktListType.TopWatchedByWeek:
                    link = link + "/movies/watched/weekly";
                    break;
                case (int)TraktListType.TopWatchedByMonth:
                    link = link + "/movies/watched/monthly";
                    break;
                case (int)TraktListType.TopWatchedByYear:
                    link = link + "/movies/watched/yearly";
                    break;
                case (int)TraktListType.TopWatchedByAllTime:
                    link = link + "/movies/watched/all";
                    break;
            }

            /*
            if (Settings.Refreshtoken ! = null) //if a refreshToken exists
            {
                TimeSpan span = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0,DateTimeKind.Utc));
                double unixTime = span.TotalSeconds;
                bool tokenExpired = false; //TokenCreatedAt + TokenExpiresIn < unixTime()
                if (tokenExpired)
                {
                    var url = Settings.Link.Trim();
                    url = url + "/oauth/token";

                    string postData = "refresh_token=" + Settings.Refreshtoken.Trim();
                    postData += "&client_id=" + Settings.Authtoken.Trim();
                    postData += "&client_secret=b16be19076b515553bb830141e08729c1d987fe686b3c3bc0316ba4382c2b810";
                    postData += "&redirect_uri=urn:ietf:wg:oauth:2.0:oob";
                    postData += "&grant_type=refresh_token";

                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                    HttpWebRequest rquest = (HttpWebRequest)WebRequest.Create(url);
                    rquest.Method = "POST";
                    rquest.Headers.ContentType = "application/json";
                    using (var st = rquest.GetRequestStream())
                        st.Write(byteArray, 0, byteArray.Length);
                    var rsponse = (HttpWebResponse)rquest.GetResponse();
                    var rsponseString = new StreamReader(rsponse.GetResponseStream()).ReadToEnd();
                    rsponse.Close();
                    dynamic j1 = JObject.Parse(rsponseString);

                    //these need to be updated and need to but so far dont know how
                    //this shoud only be done if the post request was successful.
                    Settings.Authtoken = j1.access_token;
                    Settings.Refreshtoken = j1.refresh_token;
                    TokenCreatedAt = j1.created_at; //convert to double
                    TokenExpiresIn = j1.expires_in; //convert to double
                }
            }
            */

            var request = new NetImportRequest($"{link}", HttpAccept.Json);
            request.HttpRequest.Headers.Add("trakt-api-version", "2");
            request.HttpRequest.Headers.Add("trakt-api-key", "657bb899dcb81ec8ee838ff09f6e013ff7c740bf0ccfa54dd41e791b9a70b2f0");
            if (Settings.Authtoken != null)
            {
                request.HttpRequest.Headers.Add("Authorization", "Bearer " + Settings.Authtoken.Trim());
            }

                yield return request;
        }
    }
}
