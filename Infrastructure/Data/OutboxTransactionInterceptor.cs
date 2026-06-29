using Application.Common.Caching;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    // Simplified interceptor: PostgreSQL DB Triggers now handle the `LISTEN/NOTIFY` channels
    public class OutboxTransactionInterceptor : DbTransactionInterceptor
    {
        public override async Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is AppDbContext dbContext && dbContext.OutboxState != null)
            {
                dbContext.OutboxState.HasPendingMessages = false;
            }
            await base.TransactionCommittedAsync(transaction, eventData, cancellationToken);
        }

        public override Task TransactionRolledBackAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is AppDbContext dbContext && dbContext.OutboxState != null)
            {
                dbContext.OutboxState.HasPendingMessages = false;
            }
            return base.TransactionRolledBackAsync(transaction, eventData, cancellationToken);
        }
    }
}