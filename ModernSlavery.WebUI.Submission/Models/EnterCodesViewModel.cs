using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Submission.Models
{
    [Serializable]
    public class EnterCodesViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Enter your organisation reference")]
        [SecurityCode]
        public string OrganisationReference { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Enter your security code")]
        [SecurityCode]
        public string SecurityToken { get; set; }
    }
}