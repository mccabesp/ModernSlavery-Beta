using System;
using System.ComponentModel.DataAnnotations;

namespace ModernSlavery.WebUI.Submission.Models
{
    [Serializable]
    public class EnterCodesViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Enter your organisation reference")]
        public string OrganisationReference { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Enter your security code")]
        public string SecurityToken { get; set; }
    }
}