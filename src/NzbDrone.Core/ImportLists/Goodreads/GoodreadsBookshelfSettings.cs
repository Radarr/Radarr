using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Goodreads
{
    public class GoodreadsBookshelfSettingsValidator : GoodreadsSettingsBaseValidator<GoodreadsBookshelfSettings>
    {
        public GoodreadsBookshelfSettingsValidator()
        : base()
        {
            RuleFor(c => c.PlaylistIds).NotEmpty();
        }
    }

    public class GoodreadsBookshelfSettings : GoodreadsSettingsBase<GoodreadsBookshelfSettings>
    {
        public GoodreadsBookshelfSettings()
        {
            PlaylistIds = new string[] { };
        }

        [FieldDefinition(1, Label = "Bookshelves", Type = FieldType.Playlist)]
        public IEnumerable<string> PlaylistIds { get; set; }

        protected override AbstractValidator<GoodreadsBookshelfSettings> Validator => new GoodreadsBookshelfSettingsValidator();
    }
}
