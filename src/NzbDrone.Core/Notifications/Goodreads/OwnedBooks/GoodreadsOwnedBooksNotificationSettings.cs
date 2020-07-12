using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Goodreads
{
    public enum OwnedBookCondition
    {
        BrandNew = 10,
        LikeNew = 20,
        VeryGood = 30,
        Good = 40,
        Acceptable = 50,
        Poor = 60
    }

    public class GoodreadsOwnedBooksNotificationSettings : GoodreadsSettingsBase<GoodreadsOwnedBooksNotificationSettings>
    {
        private static readonly GoodreadsSettingsBaseValidator<GoodreadsOwnedBooksNotificationSettings> Validator = new GoodreadsSettingsBaseValidator<GoodreadsOwnedBooksNotificationSettings>();

        public GoodreadsOwnedBooksNotificationSettings()
        {
        }

        [FieldDefinition(1, Label = "Condition", Type = FieldType.Select, SelectOptions = typeof(OwnedBookCondition))]
        public int Condition { get; set; } = (int)OwnedBookCondition.BrandNew;

        [FieldDefinition(1, Label = "Condition Description", Type = FieldType.Textbox)]
        public string Description { get; set; }

        [FieldDefinition(1, Label = "Purchase Location", HelpText = "Will be displayed on Goodreads website", Type = FieldType.Textbox)]
        public string Location { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
