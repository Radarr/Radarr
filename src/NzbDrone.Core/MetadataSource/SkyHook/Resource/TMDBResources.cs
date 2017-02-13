﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{

    public class FindRoot
    {
        public MovieResult[] movie_results { get; set; }
    }
    public class MovieSearchRoot
    {
        public int page { get; set; }
        public MovieResult[] results { get; set; }
        public int total_results { get; set; }
        public int total_pages { get; set; }
    }

    public class MovieResult
    {
        public string poster_path { get; set; }
        public bool adult { get; set; }
        public string overview { get; set; }
        public string release_date { get; set; }
        public int?[] genre_ids { get; set; }
        public int id { get; set; }
        public string original_title { get; set; }
        public string original_language { get; set; }
        public string title { get; set; }
        public string backdrop_path { get; set; }
        public float popularity { get; set; }
        public int vote_count { get; set; }
        public bool video { get; set; }
        public float vote_average { get; set; }
    }


    public class MovieResourceRoot
    {
        public bool adult { get; set; }
        public string backdrop_path { get; set; }
        public Belongs_To_Collection belongs_to_collection { get; set; }
        public int? status_code { get; set; }
        public string status_message { get; set; }
        public int budget { get; set; }
        public Genre[] genres { get; set; }
        public string homepage { get; set; }
        public int id { get; set; }
        public string imdb_id { get; set; }
        public string original_language { get; set; }
        public string original_title { get; set; }
        public string overview { get; set; }
        public float popularity { get; set; }
        public string poster_path { get; set; }
        public Production_Companies[] production_companies { get; set; }
        public Production_Countries[] production_countries { get; set; }
        public string release_date { get; set; }
        public long revenue { get; set; }
        public int runtime { get; set; }
        public Spoken_Languages[] spoken_languages { get; set; }
        public string status { get; set; }
        public string tagline { get; set; }
        public string title { get; set; }
        public bool video { get; set; }
        public float vote_average { get; set; }
        public int vote_count { get; set; }
        public AlternativeTitles alternative_titles { get; set; }
        public ReleaseDatesResource release_dates { get; set; }
        public VideosResource videos { get; set; }
    }

    public class ReleaseDatesResource
    {
        public List<ReleaseDates> results { get; set; }
    }

    public class ReleaseDate
    {
        public string certification { get; set; }
        public string iso_639_1 { get; set; }
        public string note { get; set; }
        public string release_date { get; set; }
        public int type { get; set; }
    }

    public class ReleaseDates
    {
        public string iso_3166_1 { get; set; }
        public List<ReleaseDate> release_dates { get; set; }
    }

    public class Belongs_To_Collection
    {
        public int id { get; set; }
        public string name { get; set; }
        public string poster_path { get; set; }
        public string backdrop_path { get; set; }
    }

    public class Genre
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Production_Companies
    {
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Production_Countries
    {
        public string iso_3166_1 { get; set; }
        public string name { get; set; }
    }

    public class Spoken_Languages
    {
        public string iso_639_1 { get; set; }
        public string name { get; set; }
    }

    public class AlternativeTitles
    {
        public List<Title> titles { get; set; }
    }

    public class Title
    {
        public string iso_3166_1 { get; set; }
        public string title { get; set; }
    }

    public class VideosResource
    {
        public List<Video> results { get; set; }
    }

    public class Video
    {
        public string id { get; set; }
        public string iso_639_1 { get; set; }
        public string iso_3166_1 { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public string site { get; set; }
        public string size { get; set; }
        public string type { get; set; }
    }

    public class ListResponseRoot
    {
        public string created_by { get; set; }
        public string description { get; set; }
        public int favorite_count { get; set; }
        public string id { get; set; }
        public Item[] items { get; set; }
        public int item_count { get; set; }
        public string iso_639_1 { get; set; }
        public string name { get; set; }
        public object poster_path { get; set; }
    }

    public class Item
    {
        public string poster_path { get; set; }
        public bool adult { get; set; }
        public string overview { get; set; }
        public string release_date { get; set; }
        public string original_title { get; set; }
        public int[] genre_ids { get; set; }
        public int id { get; set; }
        public string media_type { get; set; }
        public string original_language { get; set; }
        public string title { get; set; }
        public string backdrop_path { get; set; }
        public float popularity { get; set; }
        public int vote_count { get; set; }
        public bool video { get; set; }
        public float vote_average { get; set; }
        public string first_air_date { get; set; }
        public string[] origin_country { get; set; }
        public string name { get; set; }
        public string original_name { get; set; }
    }

}
