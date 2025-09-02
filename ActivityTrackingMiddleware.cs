using Microsoft.AspNetCore.Identity;

namespace ECare_Revamp.Models
{
    public class ActivityTrackingMiddleware
    {
        private readonly RequestDelegate _next;

        public ActivityTrackingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ECare_User> userManager)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null)
                {
                    user.LastActivity = DateTime.UtcNow;
                    user.IsActive = true;
                    await userManager.UpdateAsync(user);
                }
            }

            await _next(context);
        }
    }

}
