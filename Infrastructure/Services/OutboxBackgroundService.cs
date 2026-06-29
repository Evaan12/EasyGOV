using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Common;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.Services
{
    public class OutboxBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxBackgroundService> _logger;
        private readonly string _workerId;

        private static readonly System.Text.Json.JsonSerializerOptions DomainEventJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };

        private static readonly Dictionary<string, Type> _eventTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IDomainEvent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToDictionary(t => t.Name, t => t);

        public OutboxBackgroundService(IServiceProvider serviceProvider, ILogger<OutboxBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _workerId = Guid.NewGuid().ToString();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
                    var connString = configuration.GetConnectionString("DefaultConnection");

                    using var connection = new NpgsqlConnection(connString);
                    await connection.OpenAsync(stoppingToken);

                    connection.Notification += (o, e) =>
                    {
                        _logger.LogDebug("Received NOTIFY outbox_messages event. Instructing processor...");
                    };

                    using var cmd = new NpgsqlCommand("LISTEN outbox_messages;", connection);
                    await cmd.ExecuteNonQueryAsync(stoppingToken);

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        await ProcessOutboxMessagesAsync(stoppingToken);

                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                        cts.CancelAfter(TimeSpan.FromMinutes(1));

                        try
                        {
                            await connection.WaitAsync(cts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            // Expected heartbeat timeout
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PostgreSQL LISTEN connection disrupted. Backing off for 5s before reconnecting...");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var timeProvider = scope.ServiceProvider.GetService<TimeProvider>() ?? TimeProvider.System;

            var utcNow = timeProvider.GetUtcNow().UtcDateTime;
            var lockExpirationTime = utcNow.AddMinutes(-5);

            try
            {
                // Highly concurrent UPDATE ... RETURNING query utilizing FOR UPDATE SKIP LOCKED
                var sql = @"
                    UPDATE ""OutboxMessages""
                    SET ""LockedOn"" = @utcNow, ""LockedBy"" = @workerId
                    WHERE ""Id"" IN (
                        SELECT ""Id"" FROM ""OutboxMessages""
                        WHERE ""ProcessedOn"" IS NULL 
                          AND ""RetryCount"" < 3 
                          AND (""LockedOn"" IS NULL OR ""LockedOn"" < @lockExpiration)
                        ORDER BY ""OccurredOn""
                        LIMIT 20
                        FOR UPDATE SKIP LOCKED
                    )
                    RETURNING *;";

                var messages = await dbContext.OutboxMessages.FromSqlRaw(
                    sql,
                    new Npgsql.NpgsqlParameter("@utcNow", utcNow),
                    new Npgsql.NpgsqlParameter("@workerId", _workerId),
                    new Npgsql.NpgsqlParameter("@lockExpiration", lockExpirationTime)
                ).ToListAsync(stoppingToken);

                if (!messages.Any()) return;

                var processedIds = new List<Guid>();
                var failedMessages = new List<(Guid Id, string Error)>();

                foreach (var message in messages)
                {
                    if (!_eventTypes.TryGetValue(message.Type, out var eventType))
                    {
                        failedMessages.Add((message.Id, $"Event type '{message.Type}' is not registered."));
                        continue;
                    }

                    try
                    {
                        var domainEvent = System.Text.Json.JsonSerializer.Deserialize(message.Content, eventType, DomainEventJsonOptions);
                        if (domainEvent != null)
                        {
                            var handlerType = typeof(Application.Interfaces.IDomainEventHandler<>).MakeGenericType(eventType);
                            var handlers = scope.ServiceProvider.GetServices(handlerType);

                            foreach (dynamic handler in handlers)
                            {
                                await handler.HandleAsync((dynamic)domainEvent, stoppingToken);
                            }
                        }
                        processedIds.Add(message.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
                        failedMessages.Add((message.Id, ex.Message));
                    }
                }

                if (processedIds.Any())
                {
                    await dbContext.OutboxMessages
                        .Where(m => processedIds.Contains(m.Id))
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.ProcessedOn, timeProvider.GetUtcNow().UtcDateTime)
                            .SetProperty(p => p.LockedOn, (DateTime?)null)
                            .SetProperty(p => p.LockedBy, (string?)null), stoppingToken);
                }

                foreach (var failed in failedMessages)
                {
                    await dbContext.OutboxMessages
                        .Where(m => m.Id == failed.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.Error, failed.Error)
                            .SetProperty(p => p.RetryCount, p => p.RetryCount + 1)
                            .SetProperty(p => p.LockedOn, (DateTime?)null)
                            .SetProperty(p => p.LockedBy, (string?)null), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure processing Outbox transaction.");
            }
        }
    }
}