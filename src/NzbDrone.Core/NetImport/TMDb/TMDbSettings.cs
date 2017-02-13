using System.Collections.Generic;
using System.Globalization;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.TMDb
{

    public class TMDbSettingsValidator : AbstractValidator<TMDbSettings>
    {
        public TMDbSettingsValidator()
        {
            RuleFor(c => c.Link).ValidRootUrl();
            RuleFor(c => double.Parse(c.MinVoteAverage)).ExclusiveBetween(0, 10);
        }
    }

    public class TMDbSettings : NetImportBaseSettings
    {
        private static readonly TMDbSettingsValidator Validator = new TMDbSettingsValidator();

        public TMDbSettings()
        {
            Link = "https://api.themoviedb.org";
            MinVoteAverage = "5.5";
            // Language = (int) TMDbLanguageCodes.en;
        }

        [FieldDefinition(0, Label = "TMDb API URL", HelpText = "Link to to TMDb API URL, do not change unless you know what you are doing.")]
        public new string Link { get; set; }

        [FieldDefinition(1, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(TMDbListType), HelpText = "Type of list your seeking to import from")]
        public int ListType { get; set; }

        //[FieldDefinition(2, Label = "Language", Type = FieldType.Select, SelectOptions = typeof(TMDbLanguageCodes), HelpText = "Filter movies by Language")]
        //public int Language { get; set; }

        [FieldDefinition(2, Label = "Minimum Vote Average", HelpText = "Filter movies by rating (0.0-10.0)")]
        public string MinVoteAverage { get; set; }

        [FieldDefinition(3, Label = "Public List ID", HelpText = "Required for List")]
        public string ListId { get; set; }

        public new NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

    



}
