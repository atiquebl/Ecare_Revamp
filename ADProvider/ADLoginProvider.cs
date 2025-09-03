using Novell.Directory.Ldap;
using Serilog;
//using Serilog;

namespace ECare_Revamp.Areas.Identity.Pages.Account.ADProvider
{
    public class ADLoginProvider : IADLoginProvider
    {
        public ADLoginProvider() { }

        public bool AuthenticateUserFromAD(string userName, string password)
        {
            using (LdapConnection connection = new())
            {
                try
                {
                    connection.Connect("192.168.5.224", 389);

                    if (userName.Contains("EBL\\"))
                    {

                        connection.Bind(userName, password);
                    }
                    else
                    {
                        connection.Bind($"EBL\\{userName}", password);
                    }

                    connection.Disconnect();

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error($"Login credential verification failed for username = {userName}", ex);
                    connection.Disconnect();
                    return false;
                }
            }
        }
    }
}
