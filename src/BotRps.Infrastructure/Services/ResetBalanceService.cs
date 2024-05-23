using System.Timers;
using BotRps.Application.Common.Interfaces;
using BotRps.Infrastructure.Options;
using BotRps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace BotRps.Infrastructure.Services;

public class ResetBalanceService : IResetBalanceService, IHostedService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly Timer _timer;
    private readonly ResetBalanceOptions _options;

    public ResetBalanceService(IDbContextFactory<DatabaseContext> dbContextFactory,
        IOptions<ResetBalanceOptions> options)
    {
        _dbContextFactory = dbContextFactory;
        _options = options.Value;
        _timer = new Timer(TimeSpan.FromMinutes(1));
        _timer.Elapsed += TimerOnElapsed;
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        if (e.SignalTime.Hour == _options.ResetTime.Hour && e.SignalTime.Minute == _options.ResetTime.Minute)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var usersWithLowBalance = dbContext.Users.Where(user => user.Balance < 10).ToList();
            foreach (var user in usersWithLowBalance)
            {
                user.Balance = _options.ResetBalance;
            }

            dbContext.SaveChanges();
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _timer.Start();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Stop();
    }
}