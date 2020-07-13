using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.RadarrList2.StevenLu
{
    public class StevenLu2SettingsValidator : AbstractValidator<StevenLu2Settings>
    {
        public StevenLu2SettingsValidator()
        {
            RuleFor(c => c.MinScore).GreaterThanOrEqualTo(x => 5).LessThanOrEqualTo(x => 8);
        }
    }

    public enum StevenLuSource
    {
        Standard,
        Imdb,
        Metacritic,
        RottenTomatoes
    }

    public class StevenLu2Settings : IProviderConfig
    {
        private static readonly StevenLu2SettingsValidator Validator = new StevenLu2SettingsValidator();

        public StevenLu2Settings()
        {
            MinScore = 5;
        }

        [FieldDefinition(1, Label = "Rating source", Type = FieldType.Select, SelectOptions = typeof(StevenLuSource), HelpText = "StevenLu ratings source")]
        public int Source { get; set; }

        [FieldDefinition(1, Label = "Minimum Score", Type = FieldType.Number, HelpText = "Only applies if 'Rating source' is not 'Standard'")]
        public int MinScore { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
