using System;
using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.Trakt
{
    public class TraktSettingsValidator : AbstractValidator<TraktSettings>
    {
        public TraktSettingsValidator()
        {
            RuleFor(c => c.Link).ValidRootUrl();
            RuleFor(c => c.AccessToken).NotEmpty();
            RuleFor(c => c.RefreshToken).NotEmpty();
            RuleFor(c => c.Expires).NotEmpty();

            // List name required for UserCustomList
            RuleFor(c => c.Listname)
                .Matches(@"^[A-Za-z0-9\-_]+$", RegexOptions.IgnoreCase)
                .When(c => c.ListType == (int)TraktListType.UserCustomList)
                .WithMessage("List name is required when using Custom Trakt Lists");

            // Username required for UserWatchedList/UserWatchList
            RuleFor(c => c.Username)
                .Matches(@"^[A-Za-z0-9\-_]+$", RegexOptions.IgnoreCase)
                .When(c => c.ListType == (int)TraktListType.UserWatchedList || c.ListType == (int)TraktListType.UserWatchList)
                .WithMessage("Username is required when using User Trakt Lists");

            // Loose validation @TODO
            RuleFor(c => c.Rating)
                .Matches(@"^\d+\-\d+$", RegexOptions.IgnoreCase)
                .When(c => c.Rating.IsNotNullOrWhiteSpace())
                .WithMessage("Not a valid rating");

            // Any valid certification
            RuleFor(c => c.Ceritification)
                .Matches(@"^\bNR\b|\bG\b|\bPG\b|\bPG\-13\b|\bR\b|\bNC\-17\b$", RegexOptions.IgnoreCase)
                .When(c => c.Ceritification.IsNotNullOrWhiteSpace())
                .WithMessage("Not a valid cerification");

            // Loose validation @TODO
            RuleFor(c => c.Years)
                .Matches(@"^\d+(\-\d+)?$", RegexOptions.IgnoreCase)
                .When(c => c.Years.IsNotNullOrWhiteSpace())
                .WithMessage("Not a valid year or range of years");

            // Limit not smaller than 1 and not larger than 100
            RuleFor(c => c.Limit)
                .GreaterThan(0)

            //    .InclusiveBetween(1, 500)
                .WithMessage("Must be integer greater than 0");
        }
    }

    public class TraktSettings : IProviderConfig
    {
        private static readonly TraktSettingsValidator Validator = new TraktSettingsValidator();

        public TraktSettings()
        {
            Link = "https://api.trakt.tv";
            SignIn = "startOAuth";
            ListType = (int)TraktListType.Popular;
            Username = "";
            Listname = "";
            Rating = "0-100";
            Ceritification = "NR,G,PG,PG-13,R,NC-17";
            Genres = "";
            Years = "";
            Limit = 100;
        }

        public string OAuthUrl => "http://radarr.aeonlucid.com/v1/trakt/redirect";
        public string RenewUri => "http://radarr.aeonlucid.com/v1/trakt/refresh";
        public string ClientId => "964f67b126ade0112c4ae1f0aea3a8fb03190f71117bd83af6a0560a99bc52e6";
        public virtual string Scope => "";

        [FieldDefinition(0, Label = "Access Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(0, Label = "Refresh Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string RefreshToken { get; set; }

        [FieldDefinition(0, Label = "Expires", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public DateTime Expires { get; set; }

        [FieldDefinition(0, Label = "Trakt API URL", HelpText = "Link to to Trakt API URL, do not change unless you know what you are doing.")]
        public string Link { get; set; }

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

        [FieldDefinition(8, Label = "Limit", HelpText = "Limit the number of movies to get")]
        public int Limit { get; set; }

        [FieldDefinition(9, Label = "Additional Parameters", HelpText = "Additional Trakt API parameters", Advanced = true)]
        public string TraktAdditionalParameters { get; set; }

        [FieldDefinition(99, Label = "Authenticate with Trakt", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
