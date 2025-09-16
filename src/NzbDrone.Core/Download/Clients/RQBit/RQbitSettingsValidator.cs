using FluentValidation;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.RQBit;

public class RQbitSettingsValidator : AbstractValidator<RQbitSettings>
{
    public RQbitSettingsValidator()
    {
        RuleFor(c => c.Host).ValidHost();
        RuleFor(c => c.Port).InclusiveBetween(1, 65535);

        RuleFor(c => c.UrlBase).ValidUrlBase();
    }
}
