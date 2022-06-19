using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace NzbDrone.Http.Authentication
{
    public class ApiKeyRequirement : AuthorizationHandler<ApiKeyRequirement>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
        {
            var apiKeyClaim = context.User.FindFirst(c => c.Type == "ApiKey");

            if (apiKeyClaim != null)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
