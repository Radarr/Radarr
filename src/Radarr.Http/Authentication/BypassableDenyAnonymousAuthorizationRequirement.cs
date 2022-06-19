using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Radarr.Http.Authentication
{
    public class BypassableDenyAnonymousAuthorizationRequirement : DenyAnonymousAuthorizationRequirement
    {
    }
}
