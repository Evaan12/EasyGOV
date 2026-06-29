using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;
using System;

namespace Domain.Entities
{
    public class DocumentTemplate : Entity, IAggregateRoot
    {
        public TemplateType Type { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        
        public string FormSchemaJson { get; private set; }
        public string HtmlContent { get; private set; } 
        
        public TenantType TenantType { get; private set; }
        public Guid TenantId { get; private set; }
        public Guid? OverridesTemplateId { get; private set; }
        public Guid? LinkedTemplateId { get; private set; }

        private DocumentTemplate() { }

        public DocumentTemplate(Guid id, TemplateType type, string name, string description, string formSchemaJson, string htmlContent, TenantType tenantType, Guid tenantId, Guid? overridesTemplateId, Guid? linkedTemplateId, Guid createdBy)
            : base(id, createdBy)
        {
            Type = type;
            Name = name;
            Description = description;
            FormSchemaJson = formSchemaJson;
            HtmlContent = htmlContent;
            TenantType = tenantType;
            TenantId = tenantId;
            OverridesTemplateId = overridesTemplateId;
            LinkedTemplateId = linkedTemplateId;
        }
    }
}