using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Goodreads
{
    public class GoodreadsBookshelfNotificationSettingsValidator : GoodreadsSettingsBaseValidator<GoodreadsBookshelfNotificationSettings>
    {
        public GoodreadsBookshelfNotificationSettingsValidator()
        : base()
        {
            RuleFor(c => c.RemoveIds).NotEmpty().When(c => !c.AddIds.Any());
            RuleFor(c => c.AddIds).NotEmpty().When(c => !c.RemoveIds.Any());
        }
    }

    public class GoodreadsBookshelfNotificationSettings : GoodreadsSettingsBase<GoodreadsBookshelfNotificationSettings>
    {
        private static readonly GoodreadsBookshelfNotificationSettingsValidator Validator = new GoodreadsBookshelfNotificationSettingsValidator();

        public GoodreadsBookshelfNotificationSettings()
        {
            RemoveIds = new string[] { };
            AddIds = new string[] { };
        }

        [FieldDefinition(1, Label = "Remove from Bookshelves", Type = FieldType.Bookshelf)]
        public IEnumerable<string> RemoveIds { get; set; }

        [FieldDefinition(1, Label = "Add to Bookshelves", Type = FieldType.Bookshelf)]
        public IEnumerable<string> AddIds { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
