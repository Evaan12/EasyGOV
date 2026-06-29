using Domain.Common;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using System;

namespace Domain.Entities
{
    public class DevelopmentPlan : Entity, IAggregateRoot
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public decimal Budget { get; private set; }
        public PlanStatus Status { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        
        public Guid TenantId { get; private set; }
        public string TenantLtreePath { get; private set; }

        private DevelopmentPlan() { }

        public DevelopmentPlan(Guid id, string title, string description, decimal budget, DateTime startDate, DateTime endDate, Guid tenantId, string tenantLtreePath, Guid createdBy)
            : base(id, createdBy)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required.");
            if (startDate >= endDate) throw new DomainException("Start date must be before end date.");
            if (budget < 0) throw new DomainException("Budget cannot be negative.");

            Title = title;
            Description = description;
            Budget = budget;
            StartDate = startDate;
            EndDate = endDate;
            TenantId = tenantId;
            TenantLtreePath = tenantLtreePath;
            Status = PlanStatus.Draft;
        }

        public void Publish(Guid updatedBy)
        {
            if (Status != PlanStatus.Draft) throw new DomainException("Only draft plans can be published.");
            Status = PlanStatus.Published;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new DevelopmentPlanPublishedEvent(Id, TenantId));
        }

        public void Complete(Guid updatedBy)
        {
            if (Status != PlanStatus.Published) throw new DomainException("Only published plans can be marked as completed.");
            Status = PlanStatus.Completed;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel(Guid updatedBy)
        {
            if (Status == PlanStatus.Completed) throw new DomainException("Cannot cancel a completed plan.");
            Status = PlanStatus.Cancelled;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}