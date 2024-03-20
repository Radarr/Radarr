using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.PassThePopcorn;

public class PassThePopcornCollectionSettingsValidator : AbstractValidator<PassThePopcornCollectionSettings>
{
    public PassThePopcornCollectionSettingsValidator()
    {
        RuleFor(c => c.BaseUrl).ValidRootUrl();
        RuleFor(c => c.APIUser).NotEmpty();
        RuleFor(c => c.APIKey).NotEmpty();
        RuleFor(c => c.Id).GreaterThan(0);
    }
}

public class PassThePopcornCollectionSettings : IProviderConfig
{
    private static readonly PassThePopcornCollectionSettingsValidator Validator = new PassThePopcornCollectionSettingsValidator();

    public PassThePopcornCollectionSettings()
    {
        BaseUrl = "https://passthepopcorn.me";
    }

    [FieldDefinition(0, Label = "URL", Advanced = true, HelpText = "Don't change this unless you know what you're doing, since your API key will be sent to this host.")]
    public string BaseUrl { get; set; }

    [FieldDefinition(1, Label = "APIUser", Privacy = PrivacyLevel.UserName)]
    public string APIUser { get; set; }

    [FieldDefinition(2, Label = "APIKey", Privacy = PrivacyLevel.Password)]
    public string APIKey { get; set; }

    [FieldDefinition(3, Label = "Collection ID", HelpText = "The \"id\" parameter in the collection URL.")]
    public int Id { get; set; }

    public NzbDroneValidationResult Validate()
    {
        return new NzbDroneValidationResult(Validator.Validate(this));
    }
}
