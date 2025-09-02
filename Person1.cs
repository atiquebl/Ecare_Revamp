using Microsoft.AspNetCore.Identity;

namespace ECare_Revamp.Models;

public class Person1
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? NormalizedEmail { get; set; } // Fixed CS0114 by adding override keyword  
    public string? TwoFactorEnabled { get; set; }
    public string? SecurityStamp { get; set; }
    public string? PhoneNumberConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PasswordHash { get; set; }
    public string? NormalizedUserName { get; set; }
    public string? LockoutEnabled { get; set; }
    public string? LockoutEnd { get; set; }
    public string? ConcurrencyStamp { get; set; }
    public string? EmailConfirmed { get; set; }
    public string? AccessFailedCount { get; set; }
    public decimal? ObjectType { get; set; }
    public string? WinId { get; set; }
    public DateTime? DateTime { get; set; }
}
