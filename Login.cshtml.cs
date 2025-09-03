// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable
using ECare_Revamp.Areas.Identity.Pages.Account.ADProvider;
using ECare_Revamp.Data;
using ECare_Revamp.Models;
using ECare_Revamp.Script;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;

namespace ECare_Revamp.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly UserManager<ECare_User> _userManager;
        private readonly SignInManager<ECare_User> _signInManager;
        private readonly IADLoginProvider _adLoginProvider;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly GlobalActivityLogger _dblogger;
        private readonly IEmailService _emailService;
        private readonly Encryption_Process _encProcee; // Replace with your actual encryption key
        private readonly IConfiguration _configuration;
        public LoginModel(UserManager<ECare_User> userManager, SignInManager<ECare_User> signInManager, ILogger<LoginModel> logger, 
            IADLoginProvider adLoginProvider, ApplicationDbContext context,GlobalActivityLogger dblogger, IEmailService emailService
            ,IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _adLoginProvider = adLoginProvider;
            _logger = logger;
            _context = context;
            _dblogger = dblogger;
            _emailService = emailService;
            _configuration= configuration;

        }



        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            public string Username { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
            public int Loginfailed { get; set; } = 0;
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            //var encryptionProcess = new Encryption_Process(_configuration);
            //var encSt = encryptionProcess.EncrytpString("oracle");

            //var encPass = encryptionProcess.EncrytpString("Ebl2022it");

            //var encPass1 = encryptionProcess.EncrytpString("Oracle12345");

            //var encPass2 = encryptionProcess.EncrytpString("pwrcard");

            //var encPass3 = encryptionProcess.EncrytpString("hpsweb");
            //var encPass4 = encryptionProcess.EncrytpString("Cm$sw2023it@");

            returnUrl ??= Url.Content("~/");
            Microsoft.AspNetCore.Identity.SignInResult result;

            if (ModelState.IsValid)
            {
                if (!Input.Username.ToLower().Equals("admin"))
                {
                    var user = await _userManager.FindByNameAsync(Input.Username);

                    if (user.LockoutEnabled == true)
                    {
                        //Send email
                        string subject = "E-Care User Locked";
                        string body = $@"<p>Dear <strong>{user.FirstName.ToUpper()} {user.LastName.ToUpper()}</strong>,</p>
                                            <p>Your E-Care Role has been <strong>locked due to provide 3 times wronng password.</strong>. Please communicate with Payment Systems Team.</p>
                                            <p>Details are given below:</p>
                                            <table style=""border-collapse: collapse; width: 100%;"">
                                                <tr>
                                                    <td style=""padding: 8px; border: 1px solid #ddd;""><strong>Username</strong></td>
                                                     <td style=""padding: 8px; border: 1px solid #ddd;"">:</td>
                                                    <td style=""padding: 8px; border: 1px solid #ddd;"">{user.UserName}</td>
                                                </tr>
                                                <tr>
                                                    <td style=""padding: 8px; border: 1px solid #ddd;""><strong>Employee ID</strong></td>
                                                    <td style=""padding: 8px; border: 1px solid #ddd;"">:</td>  
                                                    <td style=""padding: 8px; border: 1px solid #ddd;"">{user.EmployeeId}</td>
                                                </tr>
                                            </table>                                               
                                            <p><strong>Note: E-Care users will have to use their Windows User ID and Password for application login.</strong></p>
                                            <p>If have any questions, please communicate with us.</p>
                                            <p>Regards</p>
                                            <p>Payment Systems Team</p>";

                        try
                        {
                            await _emailService.SendAsync(user.Email ?? string.Empty, string.Empty, subject, body); // <-- Make sure _emailService is injected
                        }
                        catch (Exception emailEx)
                        {
                            ModelState.AddModelError(string.Empty, "User logged, but email could not be sent. " + emailEx.Message);
                        }
                        return RedirectToPage("./Lockout");
                    }
                    if (_adLoginProvider.AuthenticateUserFromAD(Input.Username, Input.Password))
                    {
                        // Detach existing tracked user
                        if (user is not null)
                        {
                            if (user.IsActive && user.LastActivity >= DateTime.UtcNow.AddMinutes(-5))
                            {
                                ModelState.AddModelError(string.Empty, "User already logged in from another device or browser.");
                                return Page();
                            }

                            // Allow login by resetting session
                            user.IsActive = true;
                            user.SecurityStamp = Guid.NewGuid().ToString(); // Forces existing session logout
                            user.LastActivity = DateTime.UtcNow;
                            await _userManager.UpdateAsync(user);

                            var props = new AuthenticationProperties { IsPersistent = true };
                            await _signInManager.SignInAsync(user, props, default);
                            result = Microsoft.AspNetCore.Identity.SignInResult.Success;
                            await _dblogger.LogAsync(Input.Username, "LOGIN", "USER SUCCESSFULLY LOGGED IN");
                            //if (user.IsActive == false)
                            //{
                            //    user.IsActive = true;
                            //    user.SecurityStamp = Guid.NewGuid().ToString();
                            //    await _userManager.UpdateAsync(user);

                            //    var properties = new AuthenticationProperties { IsPersistent = true };
                            //    await _signInManager.SignInAsync(user, properties, default);
                            //}
                            //else
                            //{
                            //    if (user.IsActive && user.LastActivity >= DateTime.UtcNow.AddMinutes(-5))
                            //    {
                            //        ModelState.AddModelError(string.Empty, "User already logged in from another device or browser.");
                            //        return Page();
                            //    }
                            //    //ModelState.AddModelError(string.Empty, "User already loged in another browser and system" +
                            //    //    " forcely logout.");
                            //    //user.IsActive = false;
                            //    //user.SecurityStamp = Guid.NewGuid().ToString();
                            //    //await _userManager.UpdateAsync(user);
                            //    //return Page();
                            //}
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "User Access not available.");
                            return Page();
                        }

                        result = Microsoft.AspNetCore.Identity.SignInResult.Success;
                        user.AccessFailedCount = 0;
                        user.LockoutEnd = null;
                        user.LockoutEnabled = false;
                    }
                    else
                    {

                        result = Microsoft.AspNetCore.Identity.SignInResult.Failed;
                    }
                }
                else
                {
                    var user = await _userManager.FindByNameAsync(Input.Username);

                    if (user is not null)
                    {
                        user.SecurityStamp = Guid.NewGuid().ToString();
                        await _userManager.UpdateAsync(user);
                    }
                    result = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, isPersistent: true, lockoutOnFailure: true);
                }

                if (result.Succeeded)
                {

                    //Log.Information($"{Input.Username} user logged in.");
                    var user = await _userManager.FindByNameAsync(Input.Username);

                    if (user != null)
                    {
                        user.LoginCount += 1;
                        user.IsActive= true;
                        await _userManager.UpdateAsync(user);
                    }

                    var claims = new List<Claim>
                    {
                        new Claim("IPAddress", HttpContext.Connection.RemoteIpAddress?.ToString()),
                        new Claim("UserAgent", HttpContext.Request.Headers["User-Agent"].ToString())
                    };

                    var claimsIdentity = (ClaimsIdentity)HttpContext.User.Identity;
                    claimsIdentity.AddClaims(claims);

                    await _signInManager.RefreshSignInAsync(user);

                    return LocalRedirect(returnUrl);
                }
                else
                {
                    Input.Loginfailed += 1;
                    if (Input.Loginfailed > 3)
                    {
                        result = Microsoft.AspNetCore.Identity.SignInResult.LockedOut;
                    }
                }

                if (result.IsLockedOut)
                {
                    return RedirectToPage("./Lockout");
                }
                else if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "Login not allowed. Please confirm your account or contact support.");
                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
