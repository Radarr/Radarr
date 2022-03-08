using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Collections;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Collections
{
    public class CollectionResource : RestResource
    {
        public CollectionResource()
        {
            Movies = new List<CollectionMovieResource>();
        }

        public string Title { get; set; }
        public string SortTitle { get; set; }
        public int TmdbId { get; set; }
        public List<MediaCover> Images { get; set; }
        public string Overview { get; set; }
        public bool Monitored { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public bool SearchOnAdd { get; set; }
        public MovieStatusType MinimumAvailability { get; set; }
        public List<CollectionMovieResource> Movies { get; set; }
    }

    public static class CollectionResourceMapper
    {
        public static CollectionResource ToResource(this MovieCollection model)
        {
            if (model == null)
            {
                return null;
            }

            return new CollectionResource
            {
                Id = model.Id,
                TmdbId = model.TmdbId,
                Title = model.Title,
                Overview = model.Overview,
                SortTitle = model.SortTitle,
                Monitored = model.Monitored,
                Images = model.Images,
                QualityProfileId = model.QualityProfileId,
                RootFolderPath = model.RootFolderPath,
                MinimumAvailability = model.MinimumAvailability,
                SearchOnAdd = model.SearchOnAdd
            };
        }

        public static List<CollectionResource> ToResource(this IEnumerable<MovieCollection> collections)
        {
            return collections.Select(ToResource).ToList();
        }

        public static MovieCollection ToModel(this CollectionResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new MovieCollection
            {
                Id = resource.Id,
                Title = resource.Title,
                TmdbId = resource.TmdbId,
                SortTitle = resource.SortTitle,
                Overview = resource.Overview,
                Monitored = resource.Monitored,
                QualityProfileId = resource.QualityProfileId,
                RootFolderPath = resource.RootFolderPath,
                SearchOnAdd = resource.SearchOnAdd,
                MinimumAvailability = resource.MinimumAvailability
            };
        }

        public static MovieCollection ToModel(this CollectionResource resource, MovieCollection collection)
        {
            var updatedmovie = resource.ToModel();

            collection.ApplyChanges(updatedmovie);

            return collection;
        }
    }
}
