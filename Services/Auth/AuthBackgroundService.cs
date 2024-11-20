namespace sample_auth_aspnet.Services.Auth;
public class AuthBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<AuthBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("AuthBackgroundService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var currentTime = DateTime.UtcNow;

            if (currentTime.Hour == 0 && currentTime.Minute == 10)
            {
                logger.LogInformation("Service ran at 12:10 AM.");

                using var scope = serviceProvider.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                await authService.RemoveRevokedTokenAsync();
            }

            await Task.Delay(1000, stoppingToken);
        }

        logger.LogInformation("AuthBackgroundService is stopping.");
    }
}
