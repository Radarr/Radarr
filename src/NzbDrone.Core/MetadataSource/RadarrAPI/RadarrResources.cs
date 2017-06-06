using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
namespace NzbDrone.Core.MetadataSource.RadarrAPI
{
    public class Error
    {
        [JsonProperty("id")]
        public string RayId { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }
    }

    public class RadarrError
    {
        [JsonProperty("errors")]
        public IList<Error> Errors { get; set; }
    }

    public class RadarrAPIException : Exception
    {
        RadarrError APIErrors;

        public RadarrAPIException(RadarrError apiError) : base(HumanReadable(apiError))
        {
            
        }

        private static string HumanReadable(RadarrError APIErrors)
        {
            var firstError = APIErrors.Errors.First();
            var details = string.Join("\n", APIErrors.Errors.Select(error =>
            {
                return $"{error.Title} ({error.Status}, RayId: {error.RayId}), Details: {error.Detail}";
            }));
           return $"Error while calling api: {firstError.Title}\nFull error(s): {details}";
        }
    }
}
