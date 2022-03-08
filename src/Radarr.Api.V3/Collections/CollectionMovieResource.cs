using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Collections;

namespace Radarr.Api.V3.Collections
{
    public class CollectionMovieResource
    {
        public int TmdbId { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public string Overview { get; set; }
        public int Runtime { get; set; }
        public List<MediaCover> Images { get; set; }
        public int Year { get; set; }
        public Ratings Ratings { get; set; }
        public List<string> Genres { get; set; }
        public string Folder { get; set; }
    }

    public static class CollectionMovieResourceMapper
    {
        public static CollectionMovieResource ToResource(this MovieMetadata model)
        {
            if (model == null)
            {
                return null;
            }

            return new CollectionMovieResource
            {
                TmdbId = model.TmdbId,
                Title = model.Title,
                Overview = model.Overview,
                SortTitle = model.SortTitle,
                Images = model.Images,
                ImdbId = model.ImdbId,
                Ratings = model.Ratings,
                Runtime = model.Runtime,
                CleanTitle = model.CleanTitle,
                Genres = model.Genres,
                Year = model.Year
            };
        }
    }
}
