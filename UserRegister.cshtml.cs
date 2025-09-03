using ECare_Revamp.Areas.Identity.Pages.Account.ADProvider;
using ECare_Revamp.Models;
using ECare_Revamp.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ECare_Revamp.Areas.Identity.Pages.Account
{
    [Authorize(Roles = "admin")]
    //[AllowAnonymous]
    public class UserRegistrationModel : PageModel
    {
        private readonly IADProvider _adProvider;
        private readonly SignInManager<ECare_User> _signInManager;
        private readonly UserManager<ECare_User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<ECare_User> _userStore;
        private readonly IUserEmailStore<ECare_User> _emailStore;
        private readonly IUserService _userService;
        private readonly GlobalActivityLogger _logger;
        private readonly IEmailService _emailService;
        public List<IdentityRole> _roles;
        [TempData]
        public bool? SuccessFlag { get; set; } = null;

        public string? _returnUrl { get; set; }

        public IList<ADObjDTO> ADObjDTOs { get; set; } = default!;
        public string AlertMessage { get; set; } = string.Empty; // Initialized to an empty string to fix CS8618

        [BindProperty(SupportsGet = true)]
        public string? SearchObject { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? EmployeeID { get; set; } = string.Empty;

        [BindProperty]
        public List<ECare_User>? existingUser { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? RoleId { get; set; } = string.Empty;
        public UserRegistrationModel(IADProvider adProvider, UserManager<ECare_User> userManager,
            IUserStore<ECare_User> userStore, GlobalActivityLogger logger,
            SignInManager<ECare_User> signInManager, RoleManager<IdentityRole> roleManager,
            IUserService userService, IEmailService emailService)
        {
            _adProvider = adProvider;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userService = userService;
            _logger = logger;
            _emailService = emailService;
        }
        [NonHandler]
        public async Task<List<IdentityRole>> GetUserRoles()
        {
            return await _roleManager.Roles.AsNoTracking().ToListAsync();
        }

        public void OnGet(string? returnUrl = null, bool? successResult = null)
        {
            ADObjDTOs = new List<ADObjDTO>();
            ModelState.Clear();
            TempData["SuccessFlag"] = successResult;
            _returnUrl = returnUrl;
            //_roles = GetUserRoles().Result; // Fix: Assign the result of the asynchronous method directly  
        }
        public async Task OnPostSearchObject()
        {
            _roles = GetUserRoles().Result;
            if (ModelState.IsValid && !string.IsNullOrEmpty(SearchObject))
            {
                existingUser = await _userService.GetAllApplicationUser();
                if (int.TryParse(SearchObject, out int searchobj))
                {
                    // Search by Employee ID
                    ADObjDTOs = _adProvider.SearchForUsers(EmployeeID: searchobj);
                    if (ADObjDTOs.Count > 0)
                    {
                        TempData["ADObjDTOs"] = JsonConvert.SerializeObject(ADObjDTOs);
                    }
                    else
                    {
                        AlertMessage = $"No user found or disable user with the provided Employee ID.";
                    }
                }
                else
                {
                    // Search by Email Address
                    ADObjDTOs = _adProvider.SearchForUsers(EmailName: SearchObject.ToString());
                    if (ADObjDTOs.Count > 0)
                    {
                        TempData["ADObjDTOs"] = JsonConvert.SerializeObject(ADObjDTOs);
                    }
                    else
                    {
                        AlertMessage = $"No user found or disable user with the provided Employee ID.";
                    }
                }
            }
        }
        public async Task<IActionResult> OnPostCreate()
        {
            var Current_userEmail = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            string returnUrl = Url.Content("~/");
            try
            {
                if (int.TryParse(SearchObject, out int searchobj))
                {
                    ADObjDTOs = _adProvider.SearchForUsers(EmployeeID: searchobj);
                    if (ADObjDTOs.Count > 0)
                    {
                        TempData["ADObjDTOs"] = JsonConvert.SerializeObject(ADObjDTOs);
                    }
                }
                var tempJson = TempData["ADObjDTOs"]?.ToString();
                if (string.IsNullOrWhiteSpace(tempJson))
                {
                    ModelState.AddModelError(string.Empty, "Session expired. Please search again.");
                    return Page();
                }

                List<ADObjDTO> searchResults = JsonConvert.DeserializeObject<List<ADObjDTO>>(TempData.Peek("ADObjDTOs")?.ToString() ?? "") ?? new();
                ADObjDTO targetUser = searchResults.Where(w => w.EmployeeID == EmployeeID).FirstOrDefault() ?? new();
                if (targetUser != null)
                {
                    var user = CreateUser();
                    user.EmployeeId = int.TryParse(Regex.Replace(targetUser.EmployeeID, @"\D", "0"), out int uid) ? uid : throw new InvalidDataException("Employee ID Cannot be invalid.");
                    user.FirstName = targetUser.GivenName ?? string.Empty;
                    user.LastName = targetUser.Surname?? ".";
                    user.LoginCount = 0;
                    user.Email = targetUser.Email;
                    user.NormalizedEmail = targetUser.Email.Normalize();
                    string userName = targetUser.PrincipalName.Contains("@ebl.bd") ? targetUser.PrincipalName.Replace("@ebl.bd", "") : targetUser.PrincipalName.Replace("@ebl-bd.com", "");
                    user.NormalizedUserName = userName.Normalize();
                    user.EblUsername = userName;
                    user.PCI_Role = 0; // Default role
                    user.CreationDate = DateTime.Now;
                    await _userStore.SetUserNameAsync(user, userName, CancellationToken.None);
                    await _emailStore.SetEmailAsync(user, targetUser.Email, CancellationToken.None);
                    user.CreatedBy = (await _userManager.FindByNameAsync(User?.Identity?.Name ?? ""))?.Id ?? "admin";
                    user.IsActive = false;
                    user.LockoutEnabled = false;
                    user.AccessFailedCount = 0;

                    var result = await _userManager.CreateAsync(user, "Ebl1ecare@12345");
                    if (result.Succeeded)
                    {
                        var request = HttpContext.Request;
                        string baseUrl = $"{request.Scheme}://{request.Host}";

                        user.LockoutEnabled = false;
                        await _userManager.UpdateAsync(user);
                        var role = await _roleManager.Roles.AsNoTracking().FirstOrDefaultAsync(f => f.Id.Equals(RoleId));
                        var roleResult = await _userManager.AddToRoleAsync(user, role?.Name?? "admin");
                        if (roleResult.Succeeded)
                        {
                            var adList = TempData.Peek("ADObjDTOs")?.ToString();
                            TempData.Clear();
                            //Log.Information($"{user.EblUsername} is assigned User Role by Default.");
                            TempData["SuccessFlag"] = true;
                            TempData["ADObjDTOs"] = adList;

                            await _logger.LogAsync(User?.Identity?.Name ?? "System", "USER CREATION", $"USER {user.EmployeeId}-{user.FirstName} {user.LastName} CREATED SUCCESSFULLY.");

                            AlertMessage = $"User {user.EmployeeId}-{user.FirstName} {user.LastName} created successfully.";

                            //Send email
                            string subject = "E-Care User Created";
                            string body = $@"<p>Dear <strong>{user.FirstName.ToUpper()} {user.LastName.ToUpper()}</strong>,</p>
                                            <p>We are pleased to inform you that your E-Care user account has been successfully created.
                                            You may now access the E-Care system using your designated credentials. Please find the account details below:</p>
                                            <table style=""border-collapse: collapse; width: auto;"">
                                                <tr>
                                                    <td style=""border: 1px solid #ddd;"">Employee ID</td>
                                                    <td style=""border: 1px solid #ddd;"">:</td>
                                                    <td style=""border: 1px solid #ddd;""><strong>{user.EmployeeId}</strong></td>
                                                </tr>
                                                <tr>
                                                    <td style=""border: 1px solid #ddd;"">Username</td>
                                                    <td style=""border: 1px solid #ddd;"">:</td>
                                                    <td style=""border: 1px solid #ddd;""><strong>{user.EblUsername}</strong></td>
                                                </tr>
                                                <tr>
                                                    <td style=""border: 1px solid #ddd;"">Role</td>
                                                    <td style=""border: 1px solid #ddd;"">:</td>
                                                    <td style=""border: 1px solid #ddd;""><strong>{role?.Name?.ToUpper()}</strong></td>
                                                </tr>
                                                <tr>
                                                    <td style=""border: 1px solid #ddd;"">E-Care URL</td>
                                                    <td style=""border: 1px solid #ddd;"">:</td>
                                                    <td style=""border: 1px solid #ddd;""><strong>{baseUrl}</strong></td>
                                                </tr>
                                            </table> 
                                            <p><strong>Note:</strong>E-Care users are required to log in using their Windows User ID and Password.</p>
                                            <p>Should you notice any discrepancies or require further assistance, please contact the <strong>Payment Systems Team, ICT Division.</strong></p>
                                            <p>Best Regards,</p>
                                            <p>Payment Systems Team</p>";

                            try
                            {
                                await _emailService.SendAsync(user.Email, Current_userEmail ?? string.Empty, subject, body); // <-- Make sure _emailService is injected
                            }
                            catch (Exception emailEx)
                            {
                                ModelState.AddModelError(string.Empty, "User created, but email could not be sent. " + emailEx.Message);
                            }

                            ModelState.AddModelError(string.Empty, $"{user.EblUsername} is assigned User Role by Default.");
                            // Fix for CS0037: Cannot convert null to 'int' because it is a non-nullable value type

                            user.EmployeeId = 0;
                            ModelState.Clear();
                        }
                        else
                        {
                            //Log.Information($"{user.EblUsername} could not be assigned default role.");
                            ModelState.AddModelError(string.Empty, $"{user.EblUsername} could not be assigned default role.");
                        }
                        //return RedirectToPage("AutoRegistration", new { returnUrl = _returnUrl, successResult = TempData["SuccessFlag"] });
                    }
                    else
                    {
                        TempData["SuccessFlag"] = false;
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("User data not found", "The user data was null. Try refresing the page. If the problem persists, contact Payment Systems Team, ICT Division.");
                    TempData["SuccessFlag"] = false;
                }
            }
            catch (Exception ex)
            {
                //Log.Error($"Exception occured while Creating User with AD. Details: {ex.Message}");
                TempData["SuccessFlag"] = false;
                ModelState.AddModelError(string.Empty, "Error while creating User from Active Directory. " + ex.Message);
            }
            return Page();
            //return RedirectToPage("UserRegister", new { returnUrl = _returnUrl, successResult = TempData["SuccessFlag"], ModelState });
        }
        private ECare_User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ECare_User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ECare_User)}'. " +
                    $"Ensure that '{nameof(ECare_User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ECare_User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                //Log.Error("The default UI requires a user store with email support.");
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ECare_User>)_userStore;
        }
    }
}
