using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Messaging.Events;

namespace Radarr.Http.Authentication.Plex
{
    public class PlexServerRequirement : IAuthorizationRequirement
    {
    }

    public class PlexServerHandler : AuthorizationHandler<PlexServerRequirement>, IHandle<ConfigSavedEvent>
    {
        private readonly IConfigService _configService;
        private string _requiredServer;
        private bool _requireOwner;

        public PlexServerHandler(IConfigService configService)
        {
            _configService = configService;
            _requiredServer = configService.PlexAuthServer;
            _requireOwner = configService.PlexRequireOwner;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PlexServerRequirement requirement)
        {
            var serverClaim = context.User.FindFirst(c => c.Type == PlexConstants.ServerOwnedClaim && c.Value == _requiredServer);
            if (serverClaim != null)
            {
                context.Succeed(requirement);
            }

            if (!_requireOwner)
            {
                serverClaim = context.User.FindFirst(c => c.Type == PlexConstants.ServerAccessClaim && c.Value == _requiredServer);
                if (serverClaim != null)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }

        public void Handle(ConfigSavedEvent message)
        {
            _requiredServer = _configService.PlexAuthServer;
            _requireOwner = _configService.PlexRequireOwner;
        }
    }
}
