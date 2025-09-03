namespace ECare_Revamp.Areas.Identity.Pages.Account.ADProvider
{
    public interface IADLoginProvider
    {
        bool AuthenticateUserFromAD(string userName, string password);
    }
}
