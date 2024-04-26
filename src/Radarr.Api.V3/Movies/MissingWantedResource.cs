using System.Collections.Generic;
using NzbDrone.Core.Movies;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Movies
{
    public class MissingWantedResource : RestResource
    {
        public int Total {  get; set; }
        public List<MovieResource> Movies { get; set; }
    }

    public static class MissingResourceMapper
    {
        public static MissingWantedResource ToResource(this List<Movie> model)
        {
            if (model == null)
            {
                return null;
            }

            return new MissingWantedResource
            {
                Total = model.Count,
                Movies = model.ToResource(0)
            };
        }
    }
}
