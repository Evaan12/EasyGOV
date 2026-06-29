using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class OutboxPurgeBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxPurgeBackgroundService> _logger;
        private readonly TimeProvider _timeProvider;

        public OutboxPurgeBackgroundService(IServiceProvider serviceProvider, ILogger<OutboxPurgeBackgroundService> logger, TimeProvider timeProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _timeProvider = timeProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var retentionDate = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-7);
                    int batchSize = 2000;
                    int totalDeleted = 0;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var messageIds = await dbContext.OutboxMessages
                            .Where(m => m.ProcessedOn != null && m.ProcessedOn < retentionDate)
                            .OrderBy(m => m.Id) 
                            .Select(m => m.Id)
                            .Take(batchSize)
                            .ToListAsync(stoppingToken);

                        if (!messageIds.Any()) break;

                        int deletedInBatch = await dbContext.OutboxMessages
                            .Where(m => messageIds.Contains(m.Id))
                            .ExecuteDeleteAsync(stoppingToken);

                        totalDeleted += deletedInBatch;

                        await Task.Delay(100, stoppingToken);
                    }

                    if (totalDeleted > 0)
                    {
                        _logger.LogInformation("Purged {Count} old outbox messages in batches.", totalDeleted);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to purge outbox messages.");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}