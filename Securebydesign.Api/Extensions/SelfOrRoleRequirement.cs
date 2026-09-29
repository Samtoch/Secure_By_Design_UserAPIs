using Microsoft.AspNetCore.Authorization;
using Securebydesign.Application.DTOs.Auth;

namespace Securebydesign.Api.Extensions
{
    /// <summary>
    /// Passes if the caller is acting on their own account,
    /// or holds one of the listed roles.
    /// </summary>
    public sealed class SelfOrRoleRequirement(params string[] roles) : IAuthorizationRequirement
    {
        public IReadOnlyList<string> Roles { get; } = roles;
    }

    /// <summary>
    /// Resource-based check: the resource is the ID of the user being accessed.
    /// Closes the IDOR gap that role checks alone leave open.
    /// </summary>
    public sealed class SelfOrRoleHandler : AuthorizationHandler<SelfOrRoleRequirement, Guid>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            SelfOrRoleRequirement requirement,
            Guid targetUserId)
        {
            // Staff roles may act on any account
            if (requirement.Roles.Any(context.User.IsInRole))
            {
                context.Succeed(requirement);
            }
            // Everyone else only on their own account ("sub" = user ID in the JWT)
            else if (Guid.TryParse(context.User.FindFirst(AppClaimTypes.UserId)?.Value, out Guid callerId)
                     && callerId == targetUserId)
            {
                context.Succeed(requirement);
            }

            // Not calling Succeed means the check fails (deny by default)
            return Task.CompletedTask;
        }
    }
}
