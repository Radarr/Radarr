using System;
using System.Collections.Generic;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport.ImportExclusions;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Movies
{
    public class DiscoverMoviesResource : RestResource
    {
        //View Only
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string TitleSlug { get; set; }
        public MovieStatusType Status { get; set; }
        public string Overview { get; set; }
        public DateTime? InCinemas { get; set; }
        public DateTime? PhysicalRelease { get; set; }
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
    }

    public static class DiscoverMoviesResourceMapper
    {
        public static DiscoverMoviesResource ToResource(this Movie model, IMovieService movieService, IImportExclusionsService importExclusionService)
        {
            if (model == null)
            {
                return null;
            }

            return new DiscoverMoviesResource
            {
                TmdbId = model.TmdbId,
                Title = model.Title,
                SortTitle = model.SortTitle,
                TitleSlug = model.TitleSlug,
                InCinemas = model.InCinemas,
                PhysicalRelease = model.PhysicalRelease,

                Status = model.Status,
                Overview = model.Overview,

                Images = model.Images,

                Year = model.Year,

                Runtime = model.Runtime,
                ImdbId = model.ImdbId,
                Certification = model.Certification,
                Website = model.Website,
                Genres = model.Genres,
                Ratings = model.Ratings,
                YouTubeTrailerId = model.YouTubeTrailerId,
                Studio = model.Studio,
                Collection = model.Collection,
                IsExcluded = importExclusionService.IsMovieExcluded(model.TmdbId),
                IsExisting = movieService.FindByTmdbId(model.TmdbId) != null,
            };
        }
    }
}
