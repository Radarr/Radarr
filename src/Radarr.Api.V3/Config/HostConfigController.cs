using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Network;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Update;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;
using Radarr.Http.Validation;

namespace Radarr.Api.V3.Config
{
    [V3ApiController("config/host")]
    public class HostConfigController : RestController<HostConfigResource>
    {
        private const string PrivateValue = "********";

        private readonly IConfigFileProvider _configFileProvider;
        private readonly IConfigService _configService;
        private readonly IUserService _userService;

        public HostConfigController(IConfigFileProvider configFileProvider,
                                    IConfigService configService,
                                    IUserService userService,
                                    IDiskProvider diskProvider,
                                    OidcAuthorityValidator oidcAuthorityValidator)
        {
            _configFileProvider = configFileProvider;
            _configService = configService;
            _userService = userService;

            SharedValidator.RuleFor(c => c.BindAddress)
                           .ValidIpAddress()
                           .When(c => c.BindAddress != "*" && c.BindAddress != "localhost");

            SharedValidator.RuleFor(c => c.Port).ValidPort();

            SharedValidator.RuleFor(c => c.AllowedHosts).NotNull();

            SharedValidator.RuleFor(c => c.AllowedHosts)
                           .Must(h => AllowedHostsParser.Parse(h).Any())
                           .When(c => c.AuthenticationRequired != AuthenticationRequiredType.Enabled)
                           .WithMessage("Allowed Hosts is required when 'Authentication Required' is not 'Enabled'");

            SharedValidator.RuleFor(c => c.AllowedHosts)
                           .ValidHosts()
                           .When(c => c.AllowedHosts.IsNotNullOrWhiteSpace());

            SharedValidator.RuleFor(c => c.UrlBase).ValidUrlBase();
            SharedValidator.RuleFor(c => c.TrustedNetworks).ValidIpNetworks();
            SharedValidator.RuleFor(c => c.InstanceName).ContainsRadarr().When(c => c.InstanceName.IsNotNullOrWhiteSpace());

            SharedValidator.RuleFor(c => c.Username).NotEmpty().When(c => c.AuthenticationMethod == AuthenticationType.Forms);
            SharedValidator.RuleFor(c => c.Password).NotEmpty().When(c => c.AuthenticationMethod == AuthenticationType.Forms);

            SharedValidator.RuleFor(c => c.AuthenticationMethod)
#pragma warning disable CS0618 // Type or member is obsolete
                .NotEqual(AuthenticationType.Basic)
#pragma warning restore CS0618 // Type or member is obsolete
                .WithMessage("'Basic' is no longer supported, switch to 'Forms' instead.");

            SharedValidator.RuleFor(c => c.PasswordConfirmation)
                .Must((resource, p) => IsMatchingPassword(resource)).WithMessage("Must match Password");

            SharedValidator.RuleFor(c => c.OidcAuthority)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(c => c is not null && c.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                .WithMessage("OIDC Authority must start with 'https://'")
                .When(c => c.AuthenticationMethod == AuthenticationType.Oidc)
                .WithName("OIDC Authority");

            SharedValidator.RuleFor(c => c.OidcAuthority)
                .SetValidator(oidcAuthorityValidator)
                .When(c => c.AuthenticationMethod == AuthenticationType.Oidc &&
                           c.OidcAuthority != _configFileProvider.OidcAuthority)
                .WithName("OIDC Authority");

            SharedValidator.RuleFor(c => c.OidcClientId).NotEmpty()
                .When(c => c.AuthenticationMethod == AuthenticationType.Oidc)
                .WithName("OIDC Client ID");

            SharedValidator.RuleFor(c => c.OidcClientSecret).NotEmpty()
                .When(c => c.AuthenticationMethod == AuthenticationType.Oidc)
                .WithName("OIDC Client Secret");

            SharedValidator.RuleFor(c => c.OidcUserIdentifier).NotEmpty()
                .When(c => c.AuthenticationMethod == AuthenticationType.Oidc)
                .WithName("OIDC User");

            SharedValidator.RuleFor(c => c.OidcScopes)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(c => AuthenticationConfigurationExtensions.GetOidcScopes(c).Contains("openid"))
                .WithMessage("OIDC Scopes must include 'openid'")
                .When(c => c.AuthenticationMethod == AuthenticationType.Oidc)
                .WithName("OIDC Scopes");

            SharedValidator.RuleFor(c => c.SslPort).ValidPort().When(c => c.EnableSsl);
            SharedValidator.RuleFor(c => c.SslPort).NotEqual(c => c.Port).When(c => c.EnableSsl);

            SharedValidator.RuleFor(c => c.SslCertPath)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .SetValidator(new FileExistsValidator(diskProvider))
                .IsValidCertificate()
                .When(c => c.EnableSsl);

            SharedValidator.RuleFor(c => c.SslKeyPath)
                .NotEmpty()
                .IsValidPath()
                .SetValidator(new FileExistsValidator(diskProvider))
                .When(c => c.SslKeyPath.IsNotNullOrWhiteSpace());

            SharedValidator.RuleFor(c => c.LogSizeLimit).InclusiveBetween(1, 10);

            SharedValidator.RuleFor(c => c.Branch).NotEmpty().WithMessage("Branch name is required, 'master' is the default");
            SharedValidator.RuleFor(c => c.UpdateScriptPath).IsValidPath().When(c => c.UpdateMechanism == UpdateMechanism.Script);

            SharedValidator.RuleFor(c => c.BackupFolder).IsValidPath().When(c => Path.IsPathRooted(c.BackupFolder));
            SharedValidator.RuleFor(c => c.BackupInterval).InclusiveBetween(1, 7);
            SharedValidator.RuleFor(c => c.BackupRetention).InclusiveBetween(1, 90);
        }

        private bool IsMatchingPassword(HostConfigResource resource)
        {
            var user = _userService.FindUser();

            if (user != null && user.Password == resource.Password)
            {
                return true;
            }

            if (resource.Password == resource.PasswordConfirmation)
            {
                return true;
            }

            return false;
        }

        protected override HostConfigResource GetResourceById(int id)
        {
            return GetHostConfig();
        }

        [HttpGet]
        [Produces("application/json")]
        public HostConfigResource GetHostConfig()
        {
            var oidcClientSecret = _configFileProvider.OidcClientSecret;
            var resource = _configFileProvider.ToResource(_configService);
            var user = _userService.FindUser();

            resource.Id = 1;
            resource.Username = user?.Username ?? string.Empty;
            resource.Password = user?.Password ?? string.Empty;
            resource.PasswordConfirmation = string.Empty;

            // Prevent the OIDC client secret from being exposed
            resource.OidcClientSecret = oidcClientSecret.IsNullOrWhiteSpace() ? string.Empty : PrivateValue;

            return resource;
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public Results<Accepted<HostConfigResource>, NotFound> SaveHostConfig([FromBody] HostConfigResource resource)
        {
            resource.TrustedNetworks = IPNetworkParser.NormalizeList(resource.TrustedNetworks);

            var dictionary = resource.GetType()
                                     .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

            // Don't persist the secret OIDC client secret placeholder
            var oidcClientSecret = _configFileProvider.OidcClientSecret;

            if (resource.OidcClientSecret == PrivateValue)
            {
                dictionary["OidcClientSecret"] = oidcClientSecret;
            }

            _configFileProvider.SaveConfigDictionary(dictionary);
            _configService.SaveConfigDictionary(dictionary);

            if (resource.Username.IsNotNullOrWhiteSpace() && resource.Password.IsNotNullOrWhiteSpace())
            {
                _userService.Upsert(resource.Username, resource.Password);
            }

            return Accepted(resource.Id);
        }
    }
}
