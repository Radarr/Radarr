﻿using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Series
{
    public class SeriesResource : RestResource
    {
        public SeriesResource()
        {
            Monitored = true;
        }

        //Todo: Sorters should be done completely on the client
        //Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        //Todo: We should get the entire Profile instead of ID and Name separately

        //View Only
        public string Title { get; set; }
        //public List<AlternativeTitleResource> AlternateTitles { get; set; }
        public string SortTitle { get; set; }

        public int SeasonCount
        {
            get
            {
                if (Seasons == null) return 0;

                return Seasons.Where(s => s.SeasonNumber > 0).Count();
            }
        }

        public int? TotalEpisodeCount { get; set; }
        public int? EpisodeCount { get; set; }
        public int? EpisodeFileCount { get; set; }
        public long? SizeOnDisk { get; set; }
        public SeriesStatusType Status { get; set; }
        public string Overview { get; set; }
        public DateTime? NextAiring { get; set; }
        public DateTime? PreviousAiring { get; set; }
        public string Network { get; set; }
        public string AirTime { get; set; }
        public List<MediaCover> Images { get; set; }

        public string RemotePoster { get; set; }
        public List<SeasonResource> Seasons { get; set; }
        public int Year { get; set; }

        //View & Edit
        public string Path { get; set; }
        public int ProfileId { get; set; }

        //Editing Only
        public bool SeasonFolder { get; set; }
        public bool Monitored { get; set; }

        public bool UseSceneNumbering { get; set; }
        public int Runtime { get; set; }
        public int TvdbId { get; set; }
        public int TvRageId { get; set; }
        public int TvMazeId { get; set; }
        public DateTime? FirstAired { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public SeriesTypes SeriesType { get; set; }
        public string CleanTitle { get; set; }
        public string ImdbId { get; set; }
        public string TitleSlug { get; set; }
        public string RootFolderPath { get; set; }
        public string Certification { get; set; }
        public List<string> Genres { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public AddSeriesOptions AddOptions { get; set; }
        public Ratings Ratings { get; set; }

        //TODO: Add series statistics as a property of the series (instead of individual properties)

        //Used to support legacy consumers
        public int QualityProfileId
        {
            get
            {
                return ProfileId;
            }
            set
            {
                if (value > 0 && ProfileId == 0)
                {
                    ProfileId = value;
                }
            }
        }
    }

    public static class SeriesResourceMapper
    {
        public static SeriesResource ToResource(this Core.Tv.Series model)
        {
            if (model == null) return null;

            return new SeriesResource
            {
                Id = model.Id,

                Title = model.Title,
                //AlternateTitles
                SortTitle = model.SortTitle,

                //TotalEpisodeCount
                //EpisodeCount
                //EpisodeFileCount
                //SizeOnDisk
                Status = model.Status,
                Overview = model.Overview,
                //NextAiring
                //PreviousAiring
                Network = model.Network,
                AirTime = model.AirTime,
                Images = model.Images,
                
                Seasons = model.Seasons.ToResource(),
                Year = model.Year,
                
                Path = model.Path,
                ProfileId = model.ProfileId,
                
                SeasonFolder = model.SeasonFolder,
                Monitored = model.Monitored,

                UseSceneNumbering = model.UseSceneNumbering,
                Runtime = model.Runtime,
                TvdbId = model.TvdbId,
                TvRageId = model.TvRageId,
                TvMazeId = model.TvMazeId,
                FirstAired = model.FirstAired,
                LastInfoSync = model.LastInfoSync,
                SeriesType = model.SeriesType,
                CleanTitle = model.CleanTitle,
                ImdbId = model.ImdbId,
                TitleSlug = model.TitleSlug,
                RootFolderPath = model.RootFolderPath,
                Certification = model.Certification,
                Genres = model.Genres,
                Tags = model.Tags,
                Added = model.Added,
                AddOptions = model.AddOptions,
                Ratings = model.Ratings
            };
        }

        public static Core.Tv.Series ToModel(this SeriesResource resource)
        {
            if (resource == null) return null;

            return new Core.Tv.Series
            {
                Id = resource.Id,

                Title = resource.Title,
                //AlternateTitles
                SortTitle = resource.SortTitle,

                //TotalEpisodeCount
                //EpisodeCount
                //EpisodeFileCount
                //SizeOnDisk
                Status = resource.Status,
                Overview = resource.Overview,
                //NextAiring
                //PreviousAiring
                Network = resource.Network,
                AirTime = resource.AirTime,
                Images = resource.Images,

                Seasons = resource.Seasons.ToModel(),
                Year = resource.Year,

                Path = resource.Path,
                ProfileId = resource.ProfileId,

                SeasonFolder = resource.SeasonFolder,
                Monitored = resource.Monitored,

                UseSceneNumbering = resource.UseSceneNumbering,
                Runtime = resource.Runtime,
                TvdbId = resource.TvdbId,
                TvRageId = resource.TvRageId,
                TvMazeId = resource.TvMazeId,
                FirstAired = resource.FirstAired,
                LastInfoSync = resource.LastInfoSync,
                SeriesType = resource.SeriesType,
                CleanTitle = resource.CleanTitle,
                ImdbId = resource.ImdbId,
                TitleSlug = resource.TitleSlug,
                RootFolderPath = resource.RootFolderPath,
                Certification = resource.Certification,
                Genres = resource.Genres,
                Tags = resource.Tags,
                Added = resource.Added,
                AddOptions = resource.AddOptions,
                Ratings = resource.Ratings
            };
        }

        public static Core.Tv.Series ToModel(this SeriesResource resource, Core.Tv.Series series)
        {
            series.TvdbId = resource.TvdbId;

            series.Seasons = resource.Seasons.ToModel();
            series.Path = resource.Path;
            series.ProfileId = resource.ProfileId;

            series.SeasonFolder = resource.SeasonFolder;
            series.Monitored = resource.Monitored;

            series.SeriesType = resource.SeriesType;
            series.RootFolderPath = resource.RootFolderPath;
            series.Tags = resource.Tags;
            series.AddOptions = resource.AddOptions;

            return series;
        }

        public static List<SeriesResource> ToResource(this IEnumerable<Core.Tv.Series> series)
        {
            return series.Select(ToResource).ToList();
        }
    }
}
