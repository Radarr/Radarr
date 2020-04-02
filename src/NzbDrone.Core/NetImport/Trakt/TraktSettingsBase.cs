using System;
using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.Trakt
{
    public class TraktSettingsBaseValidator<TSettings> : AbstractValidator<TSettings>
    where TSettings : TraktSettingsBase<TSettings>
    {
        public TraktSettingsBaseValidator()
        {
            RuleFor(c => c.Link).ValidRootUrl();
            RuleFor(c => c.AccessToken).NotEmpty();
            RuleFor(c => c.RefreshToken).NotEmpty();
            RuleFor(c => c.Expires).NotEmpty();

            // Loose validation @TODO
            RuleFor(c => c.Rating)
                .Matches(@"^\d+\-\d+$", RegexOptions.IgnoreCase)
                .When(c => c.Rating.IsNotNullOrWhiteSpace())
                .WithMessage("Not a valid rating");

            // Any valid certification
            RuleFor(c => c.Certification)
                .Matches(@"^\bNR\b|\bG\b|\bPG\b|\bPG\-13\b|\bR\b|\bNC\-17\b$", RegexOptions.IgnoreCase)
                .When(c => c.Certification.IsNotNullOrWhiteSpace())
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

    public class TraktSettingsBase<TSettings> : IProviderConfig
        where TSettings : TraktSettingsBase<TSettings>
    {
        protected virtual AbstractValidator<TSettings> Validator => new TraktSettingsBaseValidator<TSettings>();

        public TraktSettingsBase()
        {
            Link = "https://api.trakt.tv";
            SignIn = "startOAuth";
            Rating = "0-100";
            Certification = "NR,G,PG,PG-13,R,NC-17";
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

        [FieldDefinition(0, Label = "Auth User", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AuthUser { get; set; }

        [FieldDefinition(0, Label = "Trakt API URL", HelpText = "Link to to Trakt API URL, do not change unless you know what you are doing.")]
        public string Link { get; set; }

        [FieldDefinition(1, Label = "Rating", HelpText = "Filter movies by rating range (0-100)")]
        public string Rating { get; set; }

        [FieldDefinition(2, Label = "Certification", HelpText = "Filter movies by a certification (NR,G,PG,PG-13,R,NC-17), (Comma Separated)")]
        public string Certification { get; set; }

        [FieldDefinition(3, Label = "Genres", HelpText = "Filter movies by Trakt Genre Slug (Comma Separated)")]
        public string Genres { get; set; }

        [FieldDefinition(4, Label = "Years", HelpText = "Filter movies by year or year range")]
        public string Years { get; set; }

        [FieldDefinition(5, Label = "Limit", HelpText = "Limit the number of movies to get")]
        public int Limit { get; set; }

        [FieldDefinition(6, Label = "Additional Parameters", HelpText = "Additional Trakt API parameters", Advanced = true)]
        public string TraktAdditionalParameters { get; set; }

        [FieldDefinition(99, Label = "Authenticate with Trakt", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
