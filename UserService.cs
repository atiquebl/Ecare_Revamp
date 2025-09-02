using ECare_Revamp.Data;
using ECare_Revamp.Models;
using ECare_Revamp.Services.UserService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECare_Revamp.Services.UserService
{
    public class UserService : IUserService
    {
        ILogger<UserService> _logger;
        ApplicationDbContext _identityContext;
        public UserService(ILogger<UserService> logger, ApplicationDbContext identityContext)
        {
            _identityContext = identityContext;
            _logger = logger;
        }

        public async Task<ECare_User?> GetUserByEblNameAsync(string eblName)
        {
            try
            {
                var user = await _identityContext.People.AsNoTracking().FirstOrDefaultAsync(f => f.EblUsername.ToLower().Equals(eblName.ToLower()));
                if (user is not null)
                    _identityContext.Entry(user).State = EntityState.Detached;

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"User with ebl name {eblName} cannot be found. Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<List<ECare_User>?> GetAllApplicationUser()
        {
            try
            {
                return await _identityContext.People.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to fetch the user list. Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<List<ECare_User>?> GetAllLockedOutUser()
        {
            try
            {
                return await _identityContext.People.Where(w => w.LockoutEnabled == true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to fetch the lockedout user list. Exception: {ex.Message}");
                return null;
            }
        }

        public async Task DetachEntity<T>(T entity) where T : class
        {
            var entry = _identityContext.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                return;
            }
            entry.State = EntityState.Detached;
            await Task.CompletedTask; // Placeholder for async signature }
        }
    }
}
