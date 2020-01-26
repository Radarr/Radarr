using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.NetImport.TMDb.Popular
{
    public class TMDbPopularSettingsValidator : TMDbSettingsBaseValidator<TMDbPopularSettings>
    {
        public TMDbPopularSettingsValidator()
        : base()
        {
            RuleFor(c => c.TMDbListType).NotNull();

            RuleFor(c => c.FilterCriteria).SetValidator(_ => new TMDbFilterSettingsValidator());
        }
    }

    public class TMDbPopularSettings : TMDbSettingsBase<TMDbPopularSettings>
    {
        protected override AbstractValidator<TMDbPopularSettings> Validator => new TMDbPopularSettingsValidator();

        public TMDbPopularSettings()
        {
            TMDbListType = (int)TMDbPopularListType.Popular;
        }

        [FieldDefinition(1, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(TMDbPopularListType), HelpText = "Type of list your seeking to import from")]
        public int TMDbListType { get; set; }

        [FieldDefinition(2)]
        public TMDbFilterSettings FilterCriteria { get; set; } = new TMDbFilterSettings();
    }
}
