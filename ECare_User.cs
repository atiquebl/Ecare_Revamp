using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ECare_Revamp.Models;

public class ECare_User : IdentityUser
{
    public int EmployeeId { get; set; } 

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string EblUsername { get; set; } = string.Empty;
    public int? PCI_Role { get; set; } = 0;

    public int? LoginCount { get; set; } = 0;

    public bool IsActive { get; set; }
    public DateTime? LastActivity { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
   
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "dd MMMM, yyyy")]
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public string? UpdatedBy { get; set; } = string.Empty;

    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "dd MMMM, yyyy")]
    public DateTime? UpdationDate { get; set; }
}
