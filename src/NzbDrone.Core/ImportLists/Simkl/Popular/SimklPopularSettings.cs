using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.Simkl.Popular
{
    public class SimklPopularSettingsValidator : SimklSettingsBaseValidator<SimklPopularSettings>
    {
        public SimklPopularSettingsValidator()
        : base()
        {
            RuleFor(c => c.SimklListType).NotNull();
        }
    }

    public class SimklPopularSettings : SimklSettingsBase<SimklPopularSettings>
    {
        protected override AbstractValidator<SimklPopularSettings> Validator => new SimklPopularSettingsValidator();

        public SimklPopularSettings()
        {
            SimklListType = (int)SimklPopularListType.Popular;
        }

        [FieldDefinition(1, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(SimklPopularListType), HelpText = "Type of list you're seeking to import from")]
        public int SimklListType { get; set; }
    }
}
