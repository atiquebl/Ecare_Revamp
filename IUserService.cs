using ECare_Revamp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECare_Revamp.Services.UserService
{
    public interface IUserService
    {
        Task<ECare_User?> GetUserByEblNameAsync(string eblName);
        Task<List<ECare_User>?> GetAllApplicationUser();
        Task<List<ECare_User>?> GetAllLockedOutUser();
        Task DetachEntity<T>(T entity) where T : class;
    }
}
