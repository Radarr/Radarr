using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Core.NetImport.RadarrList
{
    public class RadarrListException : Exception
    {
        public RadarrErrors APIErrors;

        public RadarrListException(RadarrErrors apiError)
            : base(HumanReadable(apiError))
        {
            APIErrors = apiError;
        }

        private static string HumanReadable(RadarrErrors apiErrors)
        {
            var firstError = apiErrors.Errors.First();
            var details = string.Join("\n", apiErrors.Errors.Select(error =>
            {
                return $"{error.Title} ({error.Status}, RayId: {error.RayId}), Details: {error.Detail}";
            }));
            return $"Error while calling api: {firstError.Title}\nFull error(s): {details}";
        }
    }

    public class RadarrError
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

    public class RadarrErrors
    {
        [JsonProperty("errors")]
        public IList<RadarrError> Errors { get; set; }
    }
}
