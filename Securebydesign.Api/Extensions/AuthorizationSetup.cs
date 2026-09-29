using Microsoft.AspNetCore.Authorization;
using Securebydesign.Application.DTOs.Auth;
using System.Threading.RateLimiting;

namespace Securebydesign.Api.Extensions
{
    /// <summary>
    /// All access rules for the API, registered once at startup.
    /// Program.cs calls these; controllers only reference policy names.
    /// </summary>
    public static class AuthorizationSetup
    {
        public static IServiceCollection AddSecureByDesignAuthorization(this IServiceCollection services)
        {
            services.AddAuthorizationBuilder()
                // Deny by default: an endpoint without an explicit rule still needs a valid token
                .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build())

                // Role-based policies
                .AddPolicy(Policies.ViewAllUsers, p => p.RequireRole(Roles.Admin, Roles.Support))
                .AddPolicy(Policies.DeleteUsers, p => p.RequireRole(Roles.Admin))
                .AddPolicy(Policies.ManageRoles, p => p.RequireRole(Roles.Admin))

                // Resource-based policies (own account, or listed roles)
                .AddPolicy(Policies.ViewUser, p => p.AddRequirements(new SelfOrRoleRequirement(Roles.Admin, Roles.Support)))
                .AddPolicy(Policies.UpdateUser, p => p.AddRequirements(new SelfOrRoleRequirement(Roles.Admin)));

            services.AddSingleton<IAuthorizationHandler, SelfOrRoleHandler>();
            return services;
        }

        /// <summary>Anonymous signup: 5 requests per minute per client IP.</summary>
        public static IServiceCollection AddSignupRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("signup", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1)
                        }));
            });
            return services;
        }
    }
}
