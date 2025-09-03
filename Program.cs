using DevExpress.XtraCharts.Native;
using ECare_Revamp.Areas.Identity.Pages.Account;
using ECare_Revamp.Areas.Identity.Pages.Account.ADProvider;
using ECare_Revamp.Data;
using ECare_Revamp.Data.Seed;
using ECare_Revamp.Models;
using ECare_Revamp.Pages.Helper_Page;
using ECare_Revamp.Script;
using ECare_Revamp.Services.UserService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

#region Log Region
// Configure Serilog
var logDir = builder.Configuration["LoggingOptions:LogDirectory"] ?? "logs";
var activityLog = Path.Combine(logDir, "activity-.log");
var errorLog = Path.Combine(logDir, "errors-.log");
var debugLog = Path.Combine(logDir, "debug-.log");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()  // Capture everything, we'll filter below

    // Suppress framework & EF logs from activity log entirely
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Debug) // for debug log only

    // --- Activity Log: Only logs explicitly marked with LogType="Activity" ---
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.WithProperty<string>("LogType", v => v == "Activity"))
        .WriteTo.File(
            path: activityLog,
            rollingInterval: RollingInterval.Day,
            fileSizeLimitBytes: 5 * 1024 * 1024,
            rollOnFileSizeLimit: true,
            retainedFileCountLimit: 30,
            shared: true,
            outputTemplate: "{Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ} | {Message}{NewLine}"
        ))

    // --- Error Log: Everything else that's Error or higher (stack traces, EF, etc.) ---
    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(Matching.WithProperty<string>("LogType", v => v == "Activity"))
        .MinimumLevel.Error()
        .WriteTo.File(
            path: errorLog,
            rollingInterval: RollingInterval.Day,
            fileSizeLimitBytes: 10 * 1024 * 1024,
            rollOnFileSizeLimit: true,
            retainedFileCountLimit: 30,
            shared: true,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        ))
    // --- Debug Log: EF Core SQL and low-level diagnostics ---
    .WriteTo.Logger(lc => lc
        .MinimumLevel.Debug()
        .WriteTo.File(
            path: debugLog,
            rollingInterval: RollingInterval.Day,
            fileSizeLimitBytes: 10 * 1024 * 1024,
            rollOnFileSizeLimit: true,
            retainedFileCountLimit: 7,
            shared: true,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
        ))

   
    .CreateLogger();
// Replace default logging with Serilog
builder.Host.UseSerilog();
#endregion

#region TNS ADMIN
var tnsAdmin = OracleHelper.ResolveTnsAdminPathFromOciDll();
if (!string.IsNullOrEmpty(tnsAdmin))
{
    Environment.SetEnvironmentVariable("TNS_ADMIN", tnsAdmin);
}
#endregion

#region Database Injection

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("Ecare") ?? throw new InvalidOperationException("Connection string 'Ecare' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseOracle(connectionString));

builder.Services.AddTransient<OracleConnection>(sp =>
{
    string? _PWRQRYConnectionString = builder.Configuration.GetConnectionString("PWRQRY") ?? throw new InvalidOperationException("Connection string 'PWRQRY' not found.");

    if (string.IsNullOrEmpty(_PWRQRYConnectionString))
    {
        throw new InvalidOperationException("The connection string 'PWRQRY' is not configured.");
    }
    return new OracleConnection(_PWRQRYConnectionString);
});
builder.Services.AddTransient<OracleConnection>(sp =>
{
    string? _PREPRODConnectionString = builder.Configuration.GetConnectionString("PRE-PROD") ?? throw new InvalidOperationException("Connection string 'PRE-PROD' not found.");

    if (string.IsNullOrEmpty(_PREPRODConnectionString))
    {
        throw new InvalidOperationException("The connection string 'PRE-PROD' is not configured.");
    }
    return new OracleConnection(_PREPRODConnectionString);
});
builder.Services.AddTransient<OracleConnection>(sp =>
{
    string? _EcarePREPRODConnectionString = builder.Configuration.GetConnectionString("Ecare-PRE-PROD") ?? throw new InvalidOperationException("Connection string 'Ecare-PRE-PROD' not found.");

    if (string.IsNullOrEmpty(_EcarePREPRODConnectionString))
    {
        throw new InvalidOperationException("The connection string 'Ecare-PRE-PROD' is not configured.");
    }
    return new OracleConnection(_EcarePREPRODConnectionString);
});
builder.Services.AddTransient<OracleConnection>(sp =>
{
    string? _SMSConnectionString = builder.Configuration.GetConnectionString("SMS") ?? throw new InvalidOperationException("Connection string 'SMS' not found.");

    if (string.IsNullOrEmpty(_SMSConnectionString))
    {
        throw new InvalidOperationException("The connection string 'SMS' is not configured.");
    }
    return new OracleConnection(_SMSConnectionString);
});
builder.Services.AddTransient<OracleConnection>(sp =>
{
    string? _RDXConnectionString = builder.Configuration.GetConnectionString("RDX") ?? throw new InvalidOperationException("Connection string 'RDX' not found.");

    if (string.IsNullOrEmpty(_RDXConnectionString))
    {
        throw new InvalidOperationException("The connection string 'RDX' is not configured.");
    }
    return new OracleConnection(_RDXConnectionString);
});
builder.Services.AddTransient<OracleConnection>(sp =>
{
    string? _GIFFTConnectionString = builder.Configuration.GetConnectionString("GIFFT") ?? throw new InvalidOperationException("Connection string 'GIFFT' not found.");

    if (string.IsNullOrEmpty(_GIFFTConnectionString))
    {
        throw new InvalidOperationException("The connection string 'GIFFT' is not configured.");
    }
    return new OracleConnection(_GIFFTConnectionString);
});
builder.Services.AddTransient<OracleConnection>(sp =>
{
    string? _BFTNDATAPULLConnectionString = builder.Configuration.GetConnectionString("BFTN_DATA_PULL") ?? throw new InvalidOperationException("Connection string 'BFTN_DATA_PULL' not found.");

    if (string.IsNullOrEmpty(_BFTNDATAPULLConnectionString))
    {
        throw new InvalidOperationException("The connection string 'BFTN_DATA_PULL' is not configured.");
    }
    return new OracleConnection(_BFTNDATAPULLConnectionString);
});
builder.Services.AddTransient<OracleConnection>(sp =>
{
    string? _UBSBOConnectionString = builder.Configuration.GetConnectionString("UBSBO") ?? throw new InvalidOperationException("Connection string 'UBSBO' not found.");

    if (string.IsNullOrEmpty(_UBSBOConnectionString))
    {
        throw new InvalidOperationException("The connection string 'UBSBO' is not configured.");
    }
    return new OracleConnection(_UBSBOConnectionString);
});
builder.Services.AddTransient<OracleConnection>(sp =>
{
    string? _UBSBOConnectionString = builder.Configuration.GetConnectionString("DataSource") ?? throw new InvalidOperationException("Connection string 'DataSource' not found.");

    if (string.IsNullOrEmpty(_UBSBOConnectionString))
    {
        throw new InvalidOperationException("The connection string 'DataSource' is not configured.");
    }
    return new OracleConnection(_UBSBOConnectionString);
});
builder.Services.AddTransient<OracleConnection>(sp =>
{
    string? _DebitCardSourceonnectionString = builder.Configuration.GetConnectionString("DEBIT-CARD-DataSource") ?? throw new InvalidOperationException("Connection string 'DEBIT-CARD-DataSource' not found.");

    if (string.IsNullOrEmpty(_DebitCardSourceonnectionString))
    {
        throw new InvalidOperationException("The connection string 'DEBIT-CARD-DataSource' is not configured.");
    }
    return new OracleConnection(_DebitCardSourceonnectionString);
});
builder.Services.AddTransient<SqlConnection>(sp =>
{
    string? _ONLINEPREPAIDConnectionString = builder.Configuration.GetConnectionString("ONLINEPREPAID") ?? throw new InvalidOperationException("Connection string 'ONLINEPREPAID' not found.");

    if (string.IsNullOrEmpty(_ONLINEPREPAIDConnectionString))
    {
        throw new InvalidOperationException("The connection string 'ONLINEPREPAID' is not configured.");
    }
    return new SqlConnection(_ONLINEPREPAIDConnectionString);
   
});
builder.Services.AddTransient<OracleConnection>(sp =>
{
    string? _UBS_DB_LinkConnectionString = builder.Configuration.GetConnectionString("UBS-DB_Link") ?? throw new InvalidOperationException("Connection string 'UBS-DB_Link' not found.");

    if (string.IsNullOrEmpty(_UBS_DB_LinkConnectionString))
    {
        throw new InvalidOperationException("The connection string 'UBS-DB_Link' is not configured.");
    }
    return new OracleConnection(_UBS_DB_LinkConnectionString);

});
builder.Services.AddTransient<OracleConnection>(sp =>
{
    string? _UBS_DB_LinkConnectionString = builder.Configuration.GetConnectionString("PASSPORT-UPDATE") ?? throw new InvalidOperationException("Connection string 'PASSPORT-UPDATE' not found.");

    if (string.IsNullOrEmpty(_UBS_DB_LinkConnectionString))
    {
        throw new InvalidOperationException("The connection string 'PASSPORT-UPDATE' is not configured.");
    }
    return new OracleConnection(_UBS_DB_LinkConnectionString);

});
builder.Services.AddTransient<OracleConnection>(sp =>
{
    string? _ATMConnectionString = builder.Configuration.GetConnectionString("ATM") ?? throw new InvalidOperationException("Connection string 'ATM' not found.");

    if (string.IsNullOrEmpty(_ATMConnectionString))
    {
        throw new InvalidOperationException("The connection string 'ATM' is not configured.");
    }
    return new OracleConnection(_ATMConnectionString);

});

#endregion

# region Database Developer Page Exception Filter
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ECare_User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;

}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.Configure<SecurityStampValidatorOptions>(options => { options.ValidationInterval = TimeSpan.Zero; });
builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
});
#endregion

#region Dependency Injection
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IADLoginProvider, ADLoginProvider>();
builder.Services.AddScoped<IADProvider, ADProvider>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ECare_Revamp.Models.Main>();
builder.Services.AddSingleton<GlobalActivityLogger>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<Encryption_Process>();
builder.Services.AddSingleton<Globals_IP>();
builder.Services.AddHostedService<UserActivityMonitor>();
#endregion

#region Service Configurations

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Name = "__Ecare-cc";
    options.CookieManager = new ChunkingCookieManager();
    var dataProtectionKey = builder.Configuration["DataProtection:Key"] ?? throw new InvalidOperationException("DataProtection key not found in configuration.");
    var protector = DataProtectionProvider.Create(dataProtectionKey);
    options.TicketDataFormat = new SecureDataFormat<AuthenticationTicket>(
            new TicketSerializer(),
            protector.CreateProtector("Authentication.CookieAuthentication")
        );
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.AccessDeniedPath = "/Status/AccessDenied";
    options.LoginPath = "/Identity/Account/Login";

    options.Events = new CookieAuthenticationEvents
    {
        OnValidatePrincipal = async context =>
        {
            var httpContext = context.HttpContext;
            var userManager = httpContext.RequestServices.GetRequiredService<UserManager<ECare_User>>();
            var signInManager = httpContext.RequestServices.GetRequiredService<SignInManager<ECare_User>>();

            if (context.Principal == null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync();
                return;
            }

            var user = await userManager.GetUserAsync(context.Principal);

            if (user == null || !user.IsActive)
            {
                context.RejectPrincipal();
                await httpContext.SignOutAsync();
                return;
            }

            // Optional: refresh user activity timestamp
            user.LastActivity = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
        }
    };
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = int.MaxValue;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.EventsType = typeof(CustomAuthenticationEvents);
    });

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
    //serverOptions.Limits.MaxRequestBodySize = 1073741824; // 1 GB in bytes
    serverOptions.Limits.MaxRequestBodySize = null; // 1 GB in bytes
});

builder.Services.AddScoped<CustomAuthenticationEvents>();

#endregion

var configuration = builder.Configuration;
var app = builder.Build();

#region Data Seed

using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var userManager = services.GetRequiredService<UserManager<ECare_User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await Seeder.SeedRolesAsync(userManager, roleManager);
        await Seeder.SeedAdminAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error Seeding DB.");
        throw;
    }
}

#endregion

#region App Details
app.UseMiddleware<ActivityTrackingMiddleware>();
app.UseMiddleware<SecureHeadersMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
//app.UseMiddleware<ActivityLoggingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePages(
    async context =>
    {
        if (context.HttpContext.Response.StatusCode == 403)
        {
            await Task.Run(() => context.HttpContext.Response.Redirect("/Status/AccessDenied"));
        }
    }
);
#endregion

app.UseHttpsRedirection();

# region Restrict access to IP URL entirely
var publicBaseUrl = configuration["AppSettings:PublicBaseUrl"]?.TrimEnd('/');

// Extract hostname from config (e.g., "ecarebk.ebl-bd.com")
var publicHost = new Uri(publicBaseUrl!).Host;

var localIps = Dns.GetHostAddresses(Dns.GetHostName())
                  .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                  .Select(ip => ip.ToString())
                  .ToHashSet();

app.Use(async (context, next) =>
{
    var request = context.Request;
    var host = context.Request.Host.Host;

    bool isLocalIpRequest = localIps.Contains(host) ||
                            (IPAddress.TryParse(host, out var ip) && localIps.Contains(ip.ToString()));

    bool isWrongHost = !string.Equals(host, publicHost, StringComparison.OrdinalIgnoreCase);
    bool isNotHttps = !request.IsHttps;

    if (isLocalIpRequest || isWrongHost || isNotHttps)
    {
        var secureUrl = $"{publicBaseUrl}{request.Path}{request.QueryString}";
        context.Response.Redirect(secureUrl, permanent: true);
        return;
    }
    // If the request is valid, proceed to the next middleware
    await next();
});

#endregion

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
