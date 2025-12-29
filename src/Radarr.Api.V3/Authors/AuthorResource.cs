using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Authors;
using Radarr.Api.V3.MediaItems;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Authors
{
    public class AuthorResource : RestResource, IMediaResource
    {
        public AuthorResource()
        {
            Monitored = true;
        }

        public string Name { get; set; }
        public string SortName { get; set; }
        public string Description { get; set; }
        public string ForeignAuthorId { get; set; }
        public bool Monitored { get; set; }
        public string Path { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public DateTime Added { get; set; }
        public HashSet<int> Tags { get; set; }
    }

    public static class AuthorResourceMapper
    {
        public static AuthorResource ToResource(this Author model)
        {
            if (model == null)
            {
                return null;
            }

            return new AuthorResource
            {
                Id = model.Id,
                Name = model.Name,
                SortName = model.SortName,
                Description = model.Description,
                ForeignAuthorId = model.ForeignAuthorId,
                Monitored = model.Monitored,
                Path = model.Path,
                RootFolderPath = model.RootFolderPath,
                QualityProfileId = model.QualityProfileId,
                Added = model.Added,
                Tags = model.Tags
            };
        }

        public static Author ToModel(this AuthorResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Author
            {
                Id = resource.Id,
                Name = resource.Name,
                SortName = resource.SortName,
                Description = resource.Description,
                ForeignAuthorId = resource.ForeignAuthorId,
                Monitored = resource.Monitored,
                Path = resource.Path,
                RootFolderPath = resource.RootFolderPath,
                QualityProfileId = resource.QualityProfileId,
                Tags = resource.Tags ?? new HashSet<int>()
            };
        }

        public static Author ToModel(this AuthorResource resource, Author author)
        {
            var updatedAuthor = resource.ToModel();

            author.Name = updatedAuthor.Name;
            author.SortName = updatedAuthor.SortName;
            author.Description = updatedAuthor.Description;
            author.ForeignAuthorId = updatedAuthor.ForeignAuthorId;
            author.Monitored = updatedAuthor.Monitored;
            author.Path = updatedAuthor.Path;
            author.RootFolderPath = updatedAuthor.RootFolderPath;
            author.QualityProfileId = updatedAuthor.QualityProfileId;
            author.Tags = updatedAuthor.Tags;

            return author;
        }

        public static List<AuthorResource> ToResource(this IEnumerable<Author> authors)
        {
            return authors.Select(ToResource).ToList();
        }

        public static List<Author> ToModel(this IEnumerable<AuthorResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
