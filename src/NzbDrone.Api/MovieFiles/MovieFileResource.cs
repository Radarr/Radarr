using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.REST;
using NzbDrone.Api.Movies;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Api.MovieFiles
{
    public class MovieFileResource : RestResource
    {
        public MovieFileResource()
        {
            
        }

        //Todo: Sorters should be done completely on the client
        //Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        //Todo: We should get the entire Profile instead of ID and Name separately

        public int MovieId { get; set; }
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string SceneName { get; set; }
        public string ReleaseGroup { get; set; }
        public QualityModel Quality { get; set; }
        public MovieResource Movie { get; set; }
        public string Edition { get; set; }
        public Core.MediaFiles.MediaInfo.MediaInfoModel MediaInfo { get; set; }

        //TODO: Add series statistics as a property of the series (instead of individual properties)
    }

    public static class MovieFileResourceMapper
    {
        public static MovieFileResource ToResource(this MovieFile model)
        {
            if (model == null) return null;

            MovieResource movie = null;

            /*if (model.Movie != null)
            {
                //model.Movie.LazyLoad();
                if (model.Movie.Value != null)
                {
                    //movie = model.Movie.Value.ToResource();
                }
            }*/

            return new MovieFileResource
            {
                Id = model.Id,
                RelativePath = model.RelativePath,
                Path = model.Path,
                Size = model.Size,
                DateAdded = model.DateAdded,
                SceneName = model.SceneName,
                ReleaseGroup = model.ReleaseGroup,
                Quality = model.Quality,
                Movie = movie,
                MediaInfo = model.MediaInfo,
                Edition = model.Edition
            };
        }

        public static MovieFile ToModel(this MovieFileResource resource)
        {
            if (resource == null) return null;

            return new MovieFile
            {
                
            };
        }

        public static List<MovieFileResource> ToResource(this IEnumerable<MovieFile> movies)
        {
            return movies.Select(ToResource).ToList();
        }
    }
}
