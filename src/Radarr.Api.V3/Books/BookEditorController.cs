using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using Radarr.Http;

namespace Radarr.Api.V3.Books
{
    [V3ApiController("book/editor")]
    public class BookEditorController : Controller
    {
        private readonly IBookService _bookService;
        private readonly BookEditorValidator _bookEditorValidator;

        public BookEditorController(IBookService bookService,
                                    BookEditorValidator bookEditorValidator)
        {
            _bookService = bookService;
            _bookEditorValidator = bookEditorValidator;
        }

        [HttpPut]
        public IActionResult SaveAll([FromBody] BookEditorResource resource)
        {
            var booksToUpdate = _bookService.GetBooks(resource.BookIds);

            foreach (var book in booksToUpdate)
            {
                if (resource.Monitored.HasValue)
                {
                    book.Monitored = resource.Monitored.Value;
                }

                if (resource.QualityProfileId.HasValue)
                {
                    book.QualityProfileId = resource.QualityProfileId.Value;
                }

                if (resource.RootFolderPath.IsNotNullOrWhiteSpace())
                {
                    book.RootFolderPath = resource.RootFolderPath;
                }

                if (resource.Tags != null)
                {
                    var newTags = resource.Tags;
                    var applyTags = resource.ApplyTags;

                    switch (applyTags)
                    {
                        case ApplyTags.Add:
                            newTags.ForEach(t => book.Tags.Add(t));
                            break;
                        case ApplyTags.Remove:
                            newTags.ForEach(t => book.Tags.Remove(t));
                            break;
                        case ApplyTags.Replace:
                            book.Tags = new HashSet<int>(newTags);
                            break;
                    }
                }

                var validationResult = _bookEditorValidator.Validate(book);

                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
            }

            var updatedBooks = _bookService.UpdateBooks(booksToUpdate);

            var booksResources = new List<BookResource>(updatedBooks.Count);

            foreach (var book in updatedBooks)
            {
                booksResources.Add(book.ToResource());
            }

            return Ok(booksResources);
        }

        [HttpDelete]
        public object DeleteBooks([FromBody] BookEditorResource resource)
        {
            _bookService.DeleteBooks(resource.BookIds, resource.DeleteFiles);

            return new { };
        }
    }
}
