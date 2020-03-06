using System;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Infrastructure.Message
{
    public interface IEmailTemplateRepository
    {

        void Add<TTemplate>(string templateId, string filePath) where TTemplate : AEmailTemplate;

        EmailTemplateInfo GetByTemplateId(string templateId);

        EmailTemplateInfo GetByType(Type templateType);

    }

}
