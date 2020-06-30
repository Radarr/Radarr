using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles.BookImport.Manual;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using Readarr.Api.V1.Author;
using Readarr.Api.V1.Books;
using Readarr.Http.REST;

namespace Readarr.Api.V1.ManualImport
{
    public class ManualImportResource : RestResource
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public AuthorResource Author { get; set; }
        public BookResource Book { get; set; }
        public int EditionId { get; set; }
        public QualityModel Quality { get; set; }
        public int QualityWeight { get; set; }
        public string DownloadId { get; set; }
        public IEnumerable<Rejection> Rejections { get; set; }
        public ParsedTrackInfo AudioTags { get; set; }
        public bool AdditionalFile { get; set; }
        public bool ReplaceExistingFiles { get; set; }
        public bool DisableReleaseSwitching { get; set; }
    }

    public static class ManualImportResourceMapper
    {
        public static ManualImportResource ToResource(this ManualImportItem model)
        {
            if (model == null)
            {
                return null;
            }

            return new ManualImportResource
            {
                Id = model.Id,
                Path = model.Path,
                Name = model.Name,
                Size = model.Size,
                Author = model.Author.ToResource(),
                Book = model.Book.ToResource(),
                EditionId = model.Edition?.Id ?? 0,
                Quality = model.Quality,

                //QualityWeight
                DownloadId = model.DownloadId,
                Rejections = model.Rejections,
                AudioTags = model.Tags,
                AdditionalFile = model.AdditionalFile,
                ReplaceExistingFiles = model.ReplaceExistingFiles,
                DisableReleaseSwitching = model.DisableReleaseSwitching
            };
        }

        public static List<ManualImportResource> ToResource(this IEnumerable<ManualImportItem> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
