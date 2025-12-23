using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.Books;
using Radarr.Api.V3.MediaItems;
using Radarr.Http;

namespace Radarr.Api.V3.Books
{
    [V3ApiController("book/editor")]
    public class BookEditorController : BaseMediaEditorController<Book, BookResource, BookEditorResource>
    {
        private readonly IBookService _bookService;
        private readonly BookEditorValidator _bookEditorValidator;

        public BookEditorController(IBookService bookService, BookEditorValidator bookEditorValidator)
        {
            _bookService = bookService;
            _bookEditorValidator = bookEditorValidator;
        }

        protected override List<Book> GetItemsByIds(List<int> ids) => _bookService.GetBooks(ids);
        protected override List<Book> UpdateItems(List<Book> items) => _bookService.UpdateBooks(items);
        protected override void DeleteItems(List<int> ids, bool deleteFiles) => _bookService.DeleteBooks(ids, deleteFiles);
        protected override ValidationResult ValidateItem(Book item) => _bookEditorValidator.Validate(item);
        protected override BookResource ToResource(Book model) => model.ToResource();

        protected override bool GetMonitored(Book item) => item.Monitored;
        protected override void SetMonitored(Book item, bool monitored) => item.Monitored = monitored;
        protected override int GetQualityProfileId(Book item) => item.QualityProfileId;
        protected override void SetQualityProfileId(Book item, int qualityProfileId) => item.QualityProfileId = qualityProfileId;
        protected override string GetRootFolderPath(Book item) => item.RootFolderPath;
        protected override void SetRootFolderPath(Book item, string rootFolderPath) => item.RootFolderPath = rootFolderPath;
        protected override HashSet<int> GetTags(Book item) => item.Tags;
        protected override void SetTags(Book item, HashSet<int> tags) => item.Tags = tags;
    }
}
