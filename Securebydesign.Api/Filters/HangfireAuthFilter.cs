using Hangfire.Dashboard;

namespace Securebydesign.Api.Filters
{
    public class HangfireAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Allow only authenticated admin users
            return httpContext.User.Identity?.IsAuthenticated == true
                && httpContext.User.IsInRole("admin");
        }
    }
}