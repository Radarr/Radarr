using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies.Credits;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Credits
{
    public class CreditResource : RestResource
    {
        public CreditResource()
        {
        }

        public string PersonName { get; set; }
        public string CreditTmdbId { get; set; }
        public int PersonTmdbId { get; set; }
        public int MovieMetadataId { get; set; }
        public List<MediaCover> Images { get; set; }
        public string Department { get; set; }
        public string Job { get; set; }
        public string Character { get; set; }
        public int Order { get; set; }
        public CreditType Type { get; set; }
    }

    public static class CreditResourceMapper
    {
        public static CreditResource ToResource(this Credit model)
        {
            if (model == null)
            {
                return null;
            }

            return new CreditResource
            {
                Id = model.Id,
                MovieMetadataId = model.MovieMetadataId,
                CreditTmdbId = model.CreditTmdbId,
                PersonTmdbId = model.PersonTmdbId,
                PersonName = model.Name,
                Order = model.Order,
                Character = model.Character,
                Department = model.Department,
                Images = model.Images,
                Job = model.Job,
                Type = model.Type
            };
        }

        public static List<CreditResource> ToResource(this IEnumerable<Credit> credits)
        {
            return credits.Select(ToResource).ToList();
        }

        public static Credit ToModel(this CreditResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Credit
            {
                Id = resource.Id,
                MovieMetadataId = resource.MovieMetadataId,
                Name = resource.PersonName,
                Order = resource.Order,
                Character = resource.Character,
                Department = resource.Department,
                Job = resource.Job,
                Type = resource.Type,
                Images = resource.Images,
                CreditTmdbId = resource.CreditTmdbId,
                PersonTmdbId = resource.PersonTmdbId
            };
        }
    }
}
