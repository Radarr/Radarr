using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NLog;
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.BookImport.Manual;
using NzbDrone.Core.Qualities;
using Readarr.Http;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.ManualImport
{
    public class ManualImportModule : ReadarrRestModule<ManualImportResource>
    {
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IEditionService _editionService;
        private readonly IManualImportService _manualImportService;
        private readonly Logger _logger;

        public ManualImportModule(IManualImportService manualImportService,
                                  IAuthorService authorService,
                                  IEditionService editionService,
                                  IBookService bookService,
                                  Logger logger)
        {
            _authorService = authorService;
            _bookService = bookService;
            _editionService = editionService;
            _manualImportService = manualImportService;
            _logger = logger;

            GetResourceAll = GetMediaFiles;

            Put("/", options =>
                {
                    var resource = Request.Body.FromJson<List<ManualImportResource>>();
                    return ResponseWithCode(UpdateImportItems(resource), HttpStatusCode.Accepted);
                });
        }

        private List<ManualImportResource> GetMediaFiles()
        {
            var folder = (string)Request.Query.folder;
            var downloadId = (string)Request.Query.downloadId;
            NzbDrone.Core.Books.Author author = null;

            var authorIdQuery = Request.GetNullableIntegerQueryParameter("authorId", null);

            if (authorIdQuery.HasValue && authorIdQuery.Value > 0)
            {
                author = _authorService.GetAuthor(Convert.ToInt32(authorIdQuery.Value));
            }

            var filter = Request.GetBooleanQueryParameter("filterExistingFiles", true) ? FilterFilesType.Matched : FilterFilesType.None;
            var replaceExistingFiles = Request.GetBooleanQueryParameter("replaceExistingFiles", true);

            return _manualImportService.GetMediaFiles(folder, downloadId, author, filter, replaceExistingFiles).ToResource().Select(AddQualityWeight).ToList();
        }

        private ManualImportResource AddQualityWeight(ManualImportResource item)
        {
            if (item.Quality != null)
            {
                item.QualityWeight = Quality.DefaultQualityDefinitions.Single(q => q.Quality == item.Quality.Quality).Weight;
                item.QualityWeight += item.Quality.Revision.Real * 10;
                item.QualityWeight += item.Quality.Revision.Version;
            }

            return item;
        }

        private List<ManualImportResource> UpdateImportItems(List<ManualImportResource> resources)
        {
            var items = new List<ManualImportItem>();
            foreach (var resource in resources)
            {
                items.Add(new ManualImportItem
                {
                    Id = resource.Id,
                    Path = resource.Path,
                    Name = resource.Name,
                    Size = resource.Size,
                    Author = resource.Author == null ? null : _authorService.GetAuthor(resource.Author.Id),
                    Book = resource.Book == null ? null : _bookService.GetBook(resource.Book.Id),
                    Edition = resource.EditionId == 0 ? null : _editionService.GetEdition(resource.EditionId),
                    Quality = resource.Quality,
                    DownloadId = resource.DownloadId,
                    AdditionalFile = resource.AdditionalFile,
                    ReplaceExistingFiles = resource.ReplaceExistingFiles,
                    DisableReleaseSwitching = resource.DisableReleaseSwitching
                });
            }

            return _manualImportService.UpdateItems(items).Select(x => x.ToResource()).ToList();
        }
    }
}
