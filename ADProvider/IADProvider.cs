using ECare_Revamp.Models;

namespace ECare_Revamp.Areas.Identity.Pages.Account.ADProvider
{
    public interface IADProvider
    {
        public List<ADObjDTO> SearchForUsers(int EmployeeID = 0, string EmailName = "");
    }
}
