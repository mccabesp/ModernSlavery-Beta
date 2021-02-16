using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Registration.Models
{
    [Serializable]
    public class CompleteViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "You must enter a PIN code")]
        [Display(Name = "Enter pin")]
        [Pin]
        public string PIN { get; set; }

        public bool AllowResend { get; set; }
        [BindNever]
        public string Remaining { get; set; }
        public DateTime AccountingDate { get; set; }

        public long OrganisationId { get; set; }
        
        [BindNever]
        public string OrganisationName { get; set; }
    }
}