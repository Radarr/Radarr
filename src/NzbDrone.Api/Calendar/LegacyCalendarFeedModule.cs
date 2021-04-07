using System.Text;
using Nancy;
using Nancy.Responses;

namespace NzbDrone.Api.Calendar
{
    public class LegacyCalendarFeedModule : NzbDroneFeedModule
    {
        public LegacyCalendarFeedModule()
            : base("calendar")
        {
            Get("/NzbDrone.ics", options => GetCalendarFeed());
            Get("/Sonarr.ics", options => GetCalendarFeed());
            Get("/Radarr.ics", options => GetCalendarFeed());
        }

        private object GetCalendarFeed()
        {
            string queryString = ConvertQueryParams(Request.Query);
            var url = string.Format("/feed/v3/calendar/Radarr.ics?{0}", queryString);
            return Response.AsRedirect(url, RedirectResponse.RedirectType.Permanent);
        }

        private string ConvertQueryParams(DynamicDictionary query)
        {
            var sb = new StringBuilder();

            foreach (var key in query)
            {
                var value = query[key];

                sb.AppendFormat("&{0}={1}", key, value);
            }

            return sb.ToString().Trim('&');
        }
    }
}
