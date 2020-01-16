﻿using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.NetImport.TMDb.List
{
    public class TMDbListSettingsValidator : TMDbSettingsBaseValidator<TMDbListSettings>
    {
        public TMDbListSettingsValidator()
        : base()
        {
            RuleFor(c => c.ListId).NotEmpty();
        }
    }

    public class TMDbListSettings : TMDbSettingsBase<TMDbListSettings>
    {
        protected override AbstractValidator<TMDbListSettings> Validator => new TMDbListSettingsValidator();

        public TMDbListSettings()
        {
            ListId = "";
        }

        [FieldDefinition(1, Label = "ListId", Type = FieldType.Textbox, HelpText = "TMDb Id of List to Follow")]
        public string ListId { get; set; }
    }
}
