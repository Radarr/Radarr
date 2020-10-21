using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Simkl.User
{
    public class SimklUserSettingsValidator : SimklSettingsBaseValidator<SimklUserSettings>
    {
        public SimklUserSettingsValidator()
        : base()
        {
            RuleFor(c => c.SimklListType).NotNull();
            RuleFor(c => c.AuthUser).NotEmpty();
        }
    }

    public class SimklUserSettings : SimklSettingsBase<SimklUserSettings>
    {
        protected override AbstractValidator<SimklUserSettings> Validator => new SimklUserSettingsValidator();

        public SimklUserSettings()
        {
            SimklListType = (int)SimklUserListType.UserWatchList;
        }

        [FieldDefinition(1, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(SimklUserListType), HelpText = "Type of list you're seeking to import from")]
        public int SimklListType { get; set; }
    }
}
