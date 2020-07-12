using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Goodreads
{
    public class GoodreadsBookshelfImportListSettingsValidator : GoodreadsSettingsBaseValidator<GoodreadsBookshelfImportListSettings>
    {
        public GoodreadsBookshelfImportListSettingsValidator()
        : base()
        {
            RuleFor(c => c.BookshelfIds).NotEmpty();
        }
    }

    public class GoodreadsBookshelfImportListSettings : GoodreadsSettingsBase<GoodreadsBookshelfImportListSettings>
    {
        public GoodreadsBookshelfImportListSettings()
        {
            BookshelfIds = new string[] { };
        }

        [FieldDefinition(1, Label = "Bookshelves", Type = FieldType.Bookshelf)]
        public IEnumerable<string> BookshelfIds { get; set; }

        protected override AbstractValidator<GoodreadsBookshelfImportListSettings> Validator => new GoodreadsBookshelfImportListSettingsValidator();
    }
}
