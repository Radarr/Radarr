using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.PassThePopcorn.Collection;

public class PassThePopcornCollectionSettingsValidator : AbstractValidator<PassThePopcornCollectionSettings>
{
    public PassThePopcornCollectionSettingsValidator()
    {
        RuleFor(c => c.CollectionUrl).NotEmpty().IsValidUrl();

        RuleFor(c => c.CollectionUrl)
            .Matches(@"^https://passthepopcorn\.me/collages\.php\?id=\d+(?:&[\w-]+(=[\w-]*)?)*?$", RegexOptions.IgnoreCase)
            .WithMessage("Invalid Collection URL. Acceptable format: 'https://passthepopcorn.me/collages.php?id=21&order_by=year&order_way=desc'");

        RuleFor(c => c.ApiUser).NotEmpty();
        RuleFor(c => c.ApiKey).NotEmpty();

        RuleFor(c => c.MaxPages).InclusiveBetween(1, 20);
    }
}

public class PassThePopcornCollectionSettings : IProviderConfig
{
    private static readonly PassThePopcornCollectionSettingsValidator Validator = new ();

    public PassThePopcornCollectionSettings()
    {
        CollectionUrl = "https://passthepopcorn.me/collages.php?id=21";
        MaxPages = 5;
    }

    [FieldDefinition(0, Label = "Collection URL", HelpText = "Provide a fully URL to your wanted collection, including filters to your liking.", HelpTextWarning = "By default only the category Feature Film will be imported.", HelpLink = "https://passthepopcorn.me/collages.php")]
    public string CollectionUrl { get; set; }

    [FieldDefinition(1, Label = "API User", HelpText = "These settings are found in your PassThePopcorn security settings (Edit Profile > Security).", Privacy = PrivacyLevel.UserName)]
    public string ApiUser { get; set; }

    [FieldDefinition(2, Label = "API Key", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
    public string ApiKey { get; set; }

    [FieldDefinition(3, Label = "Max Pages", HelpText = "Number of pages to pull from list (Max 20)", Type = FieldType.Number)]
    public int MaxPages { get; set; }

    public NzbDroneValidationResult Validate()
    {
        return new NzbDroneValidationResult(Validator.Validate(this));
    }
}
