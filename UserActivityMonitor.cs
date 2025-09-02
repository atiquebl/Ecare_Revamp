using Microsoft.AspNetCore.Identity;

namespace ECare_Revamp.Models
{
    public class UserActivityMonitor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public UserActivityMonitor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ECare_User>>();
                    var users = userManager.Users.ToList();

                    foreach (var user in users)
                    {
                        if (user.LastActivity.HasValue &&
                            user.LastActivity.Value < DateTime.UtcNow.AddMinutes(-10)) // 10 min timeout
                        {
                            user.IsActive = false;
                            user.LastActivity = null;
                            await userManager.UpdateAsync(user);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); // check every 2 mins
            }
        }
    }

}
