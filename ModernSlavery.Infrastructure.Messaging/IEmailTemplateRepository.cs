using System;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Infrastructure.Messaging
{
    public interface IEmailTemplateRepository
    {
        void Add<TTemplate>(string templateId, string filePath) where TTemplate : EmailTemplate;

        EmailTemplateInfo GetByTemplateId(string templateId);

        EmailTemplateInfo GetByType(Type templateType);
    }
}