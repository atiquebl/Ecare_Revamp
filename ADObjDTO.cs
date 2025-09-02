using System.ComponentModel;

namespace ECare_Revamp.Models
{
    public class ADObjDTO
    {
        [DisplayName("Division")]
        public string DepartmentName { get; set; } = string.Empty;

        [DisplayName("Email")]
        public string Email { get; set; } = string.Empty;

        [DisplayName("First Name")]
        public string GivenName { get; set; } = string.Empty;

        [DisplayName("Last Name")]
        public string Surname { get; set; } = string.Empty;

        [DisplayName("Role")]
        public string Role { get; set; } = string.Empty;

        public string PrincipalName { get; set; } = string.Empty;

        public string DistinguishedName { get; set; } = string.Empty;

        public string EmployeeID { get; set; } = string.Empty;
    }
}
