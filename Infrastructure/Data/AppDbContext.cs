using Domain.Common;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Data
{
    public partial class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IUnitOfWork
    {
        private readonly TimeProvider _timeProvider;
        private IDbContextTransaction? _currentTransaction;

        public OutboxState? OutboxState { get; }

        private static readonly System.Text.Json.JsonSerializerOptions DomainEventJsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };

        public AppDbContext(DbContextOptions<AppDbContext> options, OutboxState? outboxState = null, TimeProvider? timeProvider = null) : base(options)
        {
            OutboxState = outboxState;
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<Tenant> Tenants => Set<Tenant>();

        public DbSet<AlertCampaign> AlertCampaigns => Set<AlertCampaign>();
        public DbSet<CampaignApproval> CampaignApprovals => Set<CampaignApproval>();
        public DbSet<CampaignDispatch> CampaignDispatches => Set<CampaignDispatch>();
        public DbSet<MissingPerson> MissingPersons => Set<MissingPerson>();
        public DbSet<TenantSecurityPolicy> TenantSecurityPolicies => Set<TenantSecurityPolicy>();
        public DbSet<Gunaso> Gunasos => Set<Gunaso>();
        public DbSet<DevelopmentPlan> DevelopmentPlans => Set<DevelopmentPlan>();

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null) return;
            _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync(cancellationToken);
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync(cancellationToken);
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void ClearTracker()
        {
            ChangeTracker.Clear();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await SaveChangesAndDispatchEventsInternalAsync(cancellationToken);
        }

        private async Task<int> SaveChangesAndDispatchEventsInternalAsync(CancellationToken cancellationToken)
        {
            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

            var modifiedOrDeletedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted);

            foreach (var entry in modifiedOrDeletedEntries)
            {
                var isDefaultProp = entry.Entity.GetType().GetProperty("IsDefault");
                if (isDefaultProp != null)
                {
                    var isDefault = isDefaultProp.GetValue(entry.Entity) as bool?;

                    if (isDefault == true)
                    {
                        throw new DomainException("Data protection enforcement: Cannot edit or delete a default system entity.");
                    }
                }
            }

            var entries = ChangeTracker.Entries<Entity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added) { entry.Property(e => e.CreatedAt).CurrentValue = utcNow; }
                else if (entry.State == EntityState.Modified) { entry.Property(e => e.UpdatedAt).CurrentValue = utcNow; }
            }

            var entitiesWithEvents = ChangeTracker.Entries<IHasDomainEvents>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            var domainEvents = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();

            foreach (var domainEvent in domainEvents)
            {
                OutboxMessages.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    OccurredOn = domainEvent.OccurredOn,
                    Type = domainEvent.EventType,
                    Content = System.Text.Json.JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), DomainEventJsonOptions)
                });
            }

            int result = await base.SaveChangesAsync(cancellationToken);

            foreach (var entity in entitiesWithEvents) entity.ClearDomainEvents();

            // Removed ChangeTracker.Clear() to prevent Identity framework tracking exceptions 
            // when performing chained operations like User Creation + Role Assignment

            return result;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasPostgresExtension("ltree");
            builder.HasPostgresExtension("vector");

            ConfigureFluentApi(builder);
            SeedData(builder);
        }
    }
}