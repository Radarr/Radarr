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
        public RadarrError APIErrors;

        public RadarrAPIException(RadarrError apiError) : base(HumanReadable(apiError))
        {
            APIErrors = apiError;
        }

        private static string HumanReadable(RadarrError apiErrors)
        {
            var firstError = apiErrors.Errors.First();
            var details = string.Join("\n", apiErrors.Errors.Select(error =>
            {
                return $"{error.Title} ({error.Status}, RayId: {error.RayId}), Details: {error.Detail}";
            }));
           return $"Error while calling api: {firstError.Title}\nFull error(s): {details}";
        }
    }

    public class TitleInfo
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("aka_title")]
        public string AkaTitle { get; set; }

        [JsonProperty("aka_clean_title")]
        public string AkaCleanTitle { get; set; }
    }

    public class YearInfo
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("aka_year")]
        public int AkaYear { get; set; }
    }

    public class Title
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("tmdbid")]
        public int Tmdbid { get; set; }

        [JsonProperty("votes")]
        public int Votes { get; set; }

        [JsonProperty("vote_count")]
        public int VoteCount { get; set; }

        [JsonProperty("locked")]
        public bool Locked { get; set; }

        [JsonProperty("info_type")]
        public string InfoType { get; set; }

        [JsonProperty("info_id")]
        public int InfoId { get; set; }

        [JsonProperty("info")]
        public TitleInfo Info { get; set; }
    }

    public class Year
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("tmdbid")]
        public int Tmdbid { get; set; }

        [JsonProperty("votes")]
        public int Votes { get; set; }

        [JsonProperty("vote_count")]
        public int VoteCount { get; set; }

        [JsonProperty("locked")]
        public bool Locked { get; set; }

        [JsonProperty("info_type")]
        public string InfoType { get; set; }

        [JsonProperty("info_id")]
        public int InfoId { get; set; }

        [JsonProperty("info")]
        public YearInfo Info { get; set; }
    }

    public class Mappings
    {

        [JsonProperty("titles")]
        public IList<Title> Titles { get; set; }

        [JsonProperty("years")]
        public IList<Year> Years { get; set; }
    }

    public class Mapping
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("imdb_id")]
        public string ImdbId { get; set; }

        [JsonProperty("mappings")]
        public Mappings Mappings { get; set; }
    }

    public class AddTitleMapping
    {

        [JsonProperty("tmdbid")]
        public string Tmdbid { get; set; }

        [JsonProperty("info_type")]
        public string InfoType { get; set; }

        [JsonProperty("info_id")]
        public int InfoId { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("info")]
        public TitleInfo Info { get; set; }

        [JsonProperty("votes")]
        public int Votes { get; set; }

        [JsonProperty("vote_count")]
        public int VoteCount { get; set; }

        [JsonProperty("locked")]
        public bool Locked { get; set; }
    }

    public class AddYearMapping
    {

        [JsonProperty("tmdbid")]
        public string Tmdbid { get; set; }

        [JsonProperty("info_type")]
        public string InfoType { get; set; }

        [JsonProperty("info_id")]
        public int InfoId { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("info")]
        public YearInfo Info { get; set; }

        [JsonProperty("votes")]
        public int Votes { get; set; }

        [JsonProperty("vote_count")]
        public int VoteCount { get; set; }

        [JsonProperty("locked")]
        public bool Locked { get; set; }
    }

}
