using System;
using System.Collections;
using System.Net.Http;
using Nancy;
using Newtonsoft.Json.Linq;
using Radarr.Http.Extensions;

namespace Radarr.Api.V3.Jackett
{
  public class StatisticsModule : RadarrV3Module
  {
    public StatisticsModule()
      : base("jackett")
    {
        Get("/", x => GetIndexersAsync());
    }

    private async global::System.Threading.Tasks.Task<object> GetIndexersAsync()
    {
      var jackettApi = Request.GetNullableStringQueryParameter("api");
      var jackettPath = Request.GetNullableStringQueryParameter("path");

      if (jackettApi == null || jackettPath == null)
      {
        return new ArrayList();
      }

      const string url = "/api/v2.0/indexers?configured=true&apiKey=";

      try
      {
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(jackettPath + url + jackettApi))
            using (HttpContent content = response.Content)
            {
                string indexerRequest = await content.ReadAsStringAsync();

                return new
                {
                    ConfiguredIndexers = JArray.Parse(indexerRequest)
                };
            }
      }
      catch (Exception)
      {
        return new ArrayList();
      }
    }
  }
}
