using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.TMDb.Popular
{
    public class TMDbPopularSettingsValidator : TMDbSettingsBaseValidator<TMDbPopularSettings>
    {
        public TMDbPopularSettingsValidator()
        {
            RuleFor(c => c.TMDbListType).NotNull();

            RuleFor(c => c.FilterCriteria).SetValidator(_ => new TMDbFilterSettingsValidator());
        }
    }

    public class TMDbPopularSettings : TMDbSettingsBase<TMDbPopularSettings>
    {
        private static readonly TMDbPopularSettingsValidator Validator = new ();

        public TMDbPopularSettings()
        {
            TMDbListType = (int)TMDbPopularListType.Popular;
        }

        [FieldDefinition(1, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(TMDbPopularListType), HelpText = "Type of list your seeking to import from")]
        public int TMDbListType { get; set; }

        [FieldDefinition(2)]
        public TMDbFilterSettings FilterCriteria { get; set; } = new ();

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
