using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Headphones
{
    public class HeadphonesSettingsValidator : AbstractValidator<HeadphonesSettings>
    {
        public HeadphonesSettingsValidator()
        {
            Custom(newznab =>
            {
                if (newznab.Categories.Empty())
                {
                    return new ValidationFailure("", "'Categories' must be provided");
                }

                return null;
            });

            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
        }
    }

    public class HeadphonesSettings : IIndexerSettings
    {
        private static readonly HeadphonesSettingsValidator Validator = new HeadphonesSettingsValidator();

        public HeadphonesSettings()
        {
            ApiPath = "/api";
            BaseUrl = "https://indexer.codeshy.com";
            ApiKey = "964d601959918a578a670984bdee9357";
            Categories = new[] { 3000, 3010, 3020, 3030, 3040 };
        }

        public string BaseUrl { get; set; }

        public string ApiPath { get; set; }

        public string ApiKey { get; set; }

        [FieldDefinition(0, Label = "Categories", HelpText = "Comma Separated list, leave blank to disable standard/daily shows", Advanced = true)]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(1, Label = "Username")]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "Password", Type = FieldType.Password)]
        public string Password { get; set; }

        [FieldDefinition(3, Type = FieldType.Number, Label = "Early Download Limit", Unit = "days", HelpText = "Time before release date Lidarr will download from this indexer, empty is no limit", Advanced = true)]
        public int? EarlyReleaseLimit { get; set; }

        public virtual NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
