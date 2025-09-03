using ECare_Revamp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ECare_Revamp.Areas.Identity.Pages.Account
{
    public class CustomAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly UserManager<ECare_User> _userManager;
        private readonly SignInManager<ECare_User> _signInManager;

        // Use constructor injection
        public CustomAuthenticationEvents(
            UserManager<ECare_User> userManager,
            SignInManager<ECare_User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            if (context.Principal == null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var user = await _userManager.GetUserAsync(context.Principal);
            if (user == null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            // Check for account lockout
            var isLockedOut = await _userManager.IsLockedOutAsync(user);
            if (isLockedOut)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var httpContext = context.HttpContext;
            var currentIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            var currentUserAgent = httpContext.Request.Headers["User-Agent"].ToString();

            // Store or compare with original session details
            var originalIpAddress = context.Principal.FindFirstValue("IPAddress");
            var originalUserAgent = context.Principal.FindFirstValue("UserAgent");

            // Implement strict validation
            if (string.IsNullOrEmpty(originalIpAddress) || string.IsNullOrEmpty(originalUserAgent) ||
                currentIpAddress != originalIpAddress ||
                currentUserAgent != originalUserAgent)
            {
                context.RejectPrincipal();
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }
        }
    }
}
