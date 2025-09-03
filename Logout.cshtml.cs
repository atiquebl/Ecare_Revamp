// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using ECare_Revamp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECare_Revamp.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly UserManager<ECare_User> _userManager;
        private readonly SignInManager<ECare_User> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly GlobalActivityLogger _dblogger;
        public bool IsActive { get; set; }

        public LogoutModel(UserManager<ECare_User> userManager, SignInManager<ECare_User> signInManager,
            ILogger<LogoutModel> logger, GlobalActivityLogger dblogger)
        {
            _userManager = userManager; // Fix: Initialize _userManager
            _signInManager = signInManager;
            _logger = logger;
            _dblogger = dblogger;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            var userName = User.Identity.Name; // Fix: Retrieve the current user's username
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                user.IsActive = false; // Mark user as inactive
                user.LastActivity = null;   
                await _userManager.UpdateAsync(user); // Save to DB
            }

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            await _dblogger.LogAsync(userName, "LOGOUT", "USER SUCCESSFULLY LOGGED OUT");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage();
            }
        }
    }
}
