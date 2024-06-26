﻿using System.Timers;
using BotRpc.Domain.Entities;
using BotRps.Application.Common.Interfaces;
using BotRps.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace BotRps.Infrastructure.Services;

public class ResetBalanceService : IResetBalanceService, IHostedService
{
    private readonly Timer _timer;
    private readonly ResetBalanceOptions _options;
    private readonly IRepository _repository;
    private readonly ILogger<ResetBalanceService> _logger;

    public ResetBalanceService(IRepository repository,
        IOptions<ResetBalanceOptions> options, ILogger<ResetBalanceService> logger)
    {
        _repository = repository;
        _options = options.Value;
        _timer = new Timer(TimeSpan.FromMinutes(1));
        _timer.Elapsed += TimerOnElapsed;
        _logger = logger;
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        if (e.SignalTime.Hour == _options.ResetTime.Hour && e.SignalTime.Minute == _options.ResetTime.Minute)
        {
            using var transaction = _repository.BeginTransaction<User>();
            var usersWithLowBalance = transaction.Set.Where(user => user.Balance < 10).ToList();
            foreach (var user in usersWithLowBalance)
            {
                user.Balance = _options.ResetBalance;
            }

            transaction.Commit();

            _logger.LogInformation("Balance reset at {Time} for {UsersCount} users", e.SignalTime,
                usersWithLowBalance.Count);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer.Start();
        _logger.LogInformation("Reset balance service started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Stop();
        _logger.LogInformation("Reset balance service stopped");
        return Task.CompletedTask;
    }
}