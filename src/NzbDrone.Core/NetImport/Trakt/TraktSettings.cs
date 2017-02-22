using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.Trakt
{

    public class TraktSettingsValidator : AbstractValidator<TraktSettings>
    {
        public TraktSettingsValidator()
        {
            RuleFor(c => c.Link).ValidRootUrl();
        }
    }

    public class TraktSettings : NetImportBaseSettings
    {

        private static readonly TraktSettingsValidator Validator = new TraktSettingsValidator();
        public TraktSettings()
        {
            Link = "https://api.trakt.tv";
            Username = "";
            Listname = "";
            Rating = "0-100";
            Ceritification = "NR,G,PG,PG-13,R,NC-17";
            Genres = "";
            Years = "2011-2017";
        }

        [FieldDefinition(0, Label = "Trakt API URL", HelpText = "Link to to Trakt API URL, do not change unless you know what you are doing.")]
        public new string Link { get; set; }

        [FieldDefinition(1, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(TraktListType), HelpText = "Trakt list type")]
        public int ListType { get; set; }

        [FieldDefinition(2, Label = "Username", HelpText = "Required for User List (Ignores Filtering Options)")]
        public string Username { get; set; }

        [FieldDefinition(3, Label = "List Name", HelpText = "Required for Custom List (Ignores Filtering Options)")]
        public string Listname { get; set; }

        [FieldDefinition(4, Label = "Rating", HelpText = "Filter movies by rating range (0-100)")]
        public string Rating { get; set; }

        [FieldDefinition(5, Label = "Ceritification", HelpText = "Filter movies by a ceritification (NR,G,PG,PG-13,R,NC-17), (Comma Separated)")]
        public string Ceritification { get; set; }

        [FieldDefinition(6, Label = "Genres", HelpText = "Filter movies by Trakt Genre Slug (Comma Separated)")]
        public string Genres { get; set; }

        [FieldDefinition(7, Label = "Years", HelpText = "Filter movies by year or year range")]
        public string Years { get; set; }


        public new NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }



}
