namespace FinanceTracker.Api.Services;

public class SchedulerService : BackgroundService
{
    private readonly ILogger<SchedulerService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public SchedulerService(ILogger<SchedulerService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scheduler Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var targetTime = new TimeSpan(23, 0, 0);

            var nextRun = now.Date.Add(targetTime);
            if (now.TimeOfDay > targetTime)
            {
                nextRun = nextRun.AddDays(1);
            }

            var delay = nextRun - now;
            _logger.LogInformation("Next daily recap scheduled at {NextRun} (in {Delay})", nextRun, delay);

            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                await SendDailyRecapAsync();
            }
        }
    }

    private async Task SendDailyRecapAsync()
    {
        _logger.LogInformation("Sending daily recap...");

        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
        var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();

        var users = await userService.GetAllUsersAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var user in users)
        {
            try
            {
                var recap = await transactionService.GetRecapAsync(user.Id, today, today);
                var income = recap.Item1;
                var expense = recap.Item2;
                var balance = recap.Item3;

                var message = $"📅 *Rekap Harian {today:dd/MM/yyyy}*\n\n" +
                             $"📈 Pemasukan: Rp{income:N0}\n" +
                             $"📉 Pengeluaran: Rp{expense:N0}\n" +
                             $"━━━━━━━━━━━━━━\n" +
                             $"💵 *Saldo Hari Ini: Rp{balance:N0}*";

                await telegramService.SendMessageAsync(user.TelegramId, message);
                _logger.LogInformation("Daily recap sent to user {TelegramId}", user.TelegramId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send daily recap to user {TelegramId}", user.TelegramId);
            }
        }
    }
}