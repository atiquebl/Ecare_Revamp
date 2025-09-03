using ECare_Revamp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECare_Revamp.Areas.Identity.Pages.Account
{
    [Authorize]
    [IgnoreAntiforgeryToken]
    public class HeartbeatModel : PageModel
    {
        private readonly UserManager<ECare_User> _userManager;
        public HeartbeatModel(UserManager<ECare_User> userManager) => _userManager = userManager;

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.LastActivity = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }
            return new OkResult();
        }
    }
}
