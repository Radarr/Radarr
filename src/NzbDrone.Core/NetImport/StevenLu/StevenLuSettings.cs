﻿using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.StevenLu
{
    public class StevenLuSettingsValidator : AbstractValidator<StevenLuSettings>
    {
        public StevenLuSettingsValidator()
        {
            RuleFor(c => c.Link).ValidRootUrl();
        }
    }

    public class StevenLuSettings : IProviderConfig
    {
        private static readonly StevenLuSettingsValidator Validator = new StevenLuSettingsValidator();

        public StevenLuSettings()
        {
            Link = "https://s3.amazonaws.com/popular-movies/movies.json";
        }

        [FieldDefinition(0, Label = "URL", HelpText = "Don't change this unless you know what you are doing.")]
        public string Link { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
