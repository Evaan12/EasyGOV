using System;
using Domain.Common;

namespace Domain.Events
{
    public class DocumentForgeryDetectedEvent : IDomainEvent
    {
        public Guid DocumentId { get; }
        public Guid UploaderId { get; }
        public double ConfidenceScore { get; }
        public string EventType => nameof(DocumentForgeryDetectedEvent);
        public DateTime OccurredOn { get; }

        public DocumentForgeryDetectedEvent(Guid documentId, Guid uploaderId, double confidenceScore)
        {
            DocumentId = documentId;
            UploaderId = uploaderId;
            ConfidenceScore = confidenceScore;
            OccurredOn = DateTime.UtcNow;
        }
    }
}