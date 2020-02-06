using System;
using ModernSlavery.Core.Abstractions;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Core.Interfaces
{
    public interface IEmailTemplateRepository
    {

        void Add<TTemplate>(string templateId, string filePath) where TTemplate : AEmailTemplate;

        EmailTemplateInfo GetByTemplateId(string templateId);

        EmailTemplateInfo GetByType(Type templateType);

    }

}
