using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.Simkl
{
    public class SimklMovieIdsResource
    {
        public int Simkl { get; set; }
        public string Imdb { get; set; }
        public string Tmdb { get; set; }
        public string Mal { get; set; }
    }

    public class SimklMoviePropsResource
    {
        public string Title { get; set; }
        public int? Year { get; set; }
        public SimklMovieIdsResource Ids { get; set; }
    }

    public class SimklMovieResource
    {
        public SimklMoviePropsResource Movie { get; set; }

        [JsonProperty("anime_type")]
        public SimklAnimeType AnimeType { get; set; }
    }

    public class SimklShowResource
    {
        public SimklMoviePropsResource Show { get; set; }

        [JsonProperty("anime_type")]
        public SimklAnimeType AnimeType { get; set; }
    }

    public class SimklResponse
    {
        public List<SimklMovieResource> Movies { get; set; }
        public List<SimklShowResource> Anime { get; set; }
    }

    public class RefreshRequestResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class UserSettingsResponse
    {
        public SimklUserResource User { get; set; }
        public SimklUserAccountResource Account { get; set; }
    }

    public class SimklUserResource
    {
        public string Name { get; set; }
    }

    public class SimklUserAccountResource
    {
        public int Id { get; set; }
    }

    public class SimklSyncActivityResource
    {
        [JsonProperty("movies")]
        public SimklMoviesSyncActivityResource Movies { get; set; }

        [JsonProperty("anime")]
        public SimklMoviesSyncActivityResource Anime { get; set; }
    }

    public class SimklMoviesSyncActivityResource
    {
        public DateTime All { get; set; }
    }

    public enum SimklAnimeType
    {
        Unknown,
        Tv,
        Movie,
        Ova,
        Ona,
        Special,

        [EnumMember(Value = "music video")]
        MusicVideo,
    }
}
