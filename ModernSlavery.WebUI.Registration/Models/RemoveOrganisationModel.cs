using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Registration.Models
{
    [Serializable]
    public class RemoveOrganisationModel
    {
        [Obfuscated]public string EncOrganisationId { get; set; }
        [Obfuscated] public string EncUserId { get; set; }

        [BindNever]public string OrganisationName { get; set; }
        [BindNever] public string OrganisationAddress { get; set; }
        [BindNever] public string UserName { get; set; }
    }
}