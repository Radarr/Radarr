using System;
using System.Collections.Generic;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using Radarr.Http.REST;

namespace Radarr.Api.V3.ImportLists
{
    public class ImportListMoviesResource : RestResource
    {
        public ImportListMoviesResource()
        {
            Lists = new HashSet<int>();
        }

        public string Title { get; set; }
        public string SortTitle { get; set; }
        public MovieStatusType Status { get; set; }
        public string Overview { get; set; }
        public DateTime? InCinemas { get; set; }
        public DateTime? PhysicalRelease { get; set; }
        public DateTime? DigitalRelease { get; set; }
        public List<MediaCover> Images { get; set; }
        public string Website { get; set; }
        public string RemotePoster { get; set; }
        public int Year { get; set; }
        public string YouTubeTrailerId { get; set; }
        public string Studio { get; set; }

        public int Runtime { get; set; }
        public string ImdbId { get; set; }
        public int TmdbId { get; set; }
        public string Folder { get; set; }
        public string Certification { get; set; }
        public List<string> Genres { get; set; }
        public Ratings Ratings { get; set; }
        public MovieCollection Collection { get; set; }
        public bool IsExcluded { get; set; }
        public bool IsExisting { get; set; }

        public bool IsRecommendation { get; set; }
        public HashSet<int> Lists { get; set; }
    }

    public static class DiscoverMoviesResourceMapper
    {
        public static ImportListMoviesResource ToResource(this Movie model)
        {
            if (model == null)
            {
                return null;
            }

            return new ImportListMoviesResource
            {
                TmdbId = model.TmdbId,
                Title = model.Title,
                SortTitle = model.MovieMetadata.Value.SortTitle,
                InCinemas = model.MovieMetadata.Value.InCinemas,
                PhysicalRelease = model.MovieMetadata.Value.PhysicalRelease,
                DigitalRelease = model.MovieMetadata.Value.DigitalRelease,

                Status = model.MovieMetadata.Value.Status,
                Overview = model.MovieMetadata.Value.Overview,

                Images = model.MovieMetadata.Value.Images,

                Year = model.Year,

                Runtime = model.MovieMetadata.Value.Runtime,
                ImdbId = model.ImdbId,
                Certification = model.MovieMetadata.Value.Certification,
                Website = model.MovieMetadata.Value.Website,
                Genres = model.MovieMetadata.Value.Genres,
                Ratings = model.MovieMetadata.Value.Ratings,
                YouTubeTrailerId = model.MovieMetadata.Value.YouTubeTrailerId,
                Studio = model.MovieMetadata.Value.Studio,
                Collection = model.MovieMetadata.Value.Collection
            };
        }

        public static ImportListMoviesResource ToResource(this ImportListMovie model)
        {
            if (model == null)
            {
                return null;
            }

            return new ImportListMoviesResource
            {
                TmdbId = model.TmdbId,
                Title = model.Title,
                SortTitle = model.MovieMetadata.Value.SortTitle,
                InCinemas = model.MovieMetadata.Value.InCinemas,
                PhysicalRelease = model.MovieMetadata.Value.PhysicalRelease,
                DigitalRelease = model.MovieMetadata.Value.DigitalRelease,

                Status = model.MovieMetadata.Value.Status,
                Overview = model.MovieMetadata.Value.Overview,

                Images = model.MovieMetadata.Value.Images,

                Year = model.Year,

                Runtime = model.MovieMetadata.Value.Runtime,
                ImdbId = model.ImdbId,
                Certification = model.MovieMetadata.Value.Certification,
                Website = model.MovieMetadata.Value.Website,
                Genres = model.MovieMetadata.Value.Genres,
                Ratings = model.MovieMetadata.Value.Ratings,
                YouTubeTrailerId = model.MovieMetadata.Value.YouTubeTrailerId,
                Studio = model.MovieMetadata.Value.Studio,
                Collection = model.MovieMetadata.Value.Collection,
                Lists = new HashSet<int> { model.ListId }
            };
        }
    }
}
