using ECare_Revamp.Models;
using Novell.Directory.Ldap;

namespace ECare_Revamp.Areas.Identity.Pages.Account.ADProvider
{
    public class ADProvider : IADProvider
    {
        public ADProvider() { }


        /// <summary>
        /// Searches User in AD based on Employee ID or EBLUsername
        /// </summary>
        /// <param name="EmployeeID"></param>
        /// <param name="EmailName"></param>
        public List<ADObjDTO> SearchForUsers(int EmployeeID = 0, string EmailName = "")
        {
            List<ADObjDTO> adObjDTOList = new();


            #region Searching

            using (LdapConnection connection = new())
            {
                connection.Connect("192.168.5.224", 389);

                connection.Bind("EBL\\ppc", "Ebl12345");

                LdapSearchResults lsc;

                if (EmployeeID is not 0)
                {
                    lsc = (LdapSearchResults)connection.Search(
                "DC=ebl,DC=bd",
                LdapConnection.SCOPE_SUB,
                $"(&(objectClass=user)(postalcode={EmployeeID}))", new string[] { "name", "mail", "givenName", "sn", "userPrincipalName", "distinguishedName", "postalcode" },
                false);
                }
                else
                {
                    if (!EmailName.Contains("@ebl-bd.com"))
                    {
                        EmailName = $"{EmailName}@ebl-bd.com";
                    }
                    lsc = (LdapSearchResults)connection.Search(
                "DC=ebl,DC=bd",
                LdapConnection.SCOPE_SUB,
                $"(&(objectClass=user)(mail={EmailName}))", new string[] { "name", "mail", "givenName", "sn", "userPrincipalName", "distinguishedName", "postalcode" },
                false);
                }

                while (lsc.HasMore())
                {
                    LdapEntry nextEntry = new();
                    try
                    {
                        nextEntry = lsc.Next();
                        nextEntry.getAttributeSet();
                        string distinguishedName = nextEntry.getAttribute("distinguishedName").StringValue;
                        if (distinguishedName.Contains("OU=Disabled-Users", StringComparison.OrdinalIgnoreCase))
                        {
                            adObjDTOList.Clear();
                            break; // Exit loop immediately
                        }
                        string firstName = nextEntry.getAttribute("givenName")?.StringValue ?? string.Empty;
                        string lastName = nextEntry.getAttribute("sn")?.StringValue ?? string.Empty;
                        string fullName = nextEntry.getAttribute("name")?.StringValue ?? string.Empty;

                        if (string.IsNullOrEmpty(lastName))
                            {
                            if (!string.IsNullOrEmpty(fullName))
                            {
                                var parts = fullName.Trim().Split(' ');
                                if (parts.Length == 1)
                                {
                                    firstName = parts[0];
                                }
                                else
                                {
                                    firstName = string.Join(" ", parts.Take(parts.Length - 1));
                                    lastName = parts.Last();
                                }
                            }
                        }
                        adObjDTOList.Add(
                               new ADObjDTO
                               {
                                   DepartmentName = nextEntry.getAttribute("name")?.StringValue ?? string.Empty,
                                   Email = nextEntry.getAttribute("mail")?.StringValue ?? string.Empty,
                                   GivenName = firstName,
                                   Surname = string.IsNullOrEmpty(nextEntry.getAttribute("sn")?.StringValue)? lastName
                                            : nextEntry.getAttribute("sn")?.StringValue ?? string.Empty,
                                   PrincipalName = nextEntry.getAttribute("userPrincipalName")?.StringValue ?? string.Empty,
                                   DistinguishedName = nextEntry.getAttribute("distinguishedName")?.StringValue ?? string.Empty,
                                   EmployeeID = nextEntry.getAttribute("postalcode")?.StringValue ?? string.Empty
                               });
                    }
                    catch
                    {
                        continue;
                    }
                };
                connection.Disconnect();
            }

            #endregion

            return adObjDTOList;
        }
    }
}
