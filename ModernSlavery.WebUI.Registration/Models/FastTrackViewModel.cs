using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Registration.Models
{
    [Serializable]
    public class FastTrackViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Organisation reference")]
        [SecurityCode]
        public string OrganisationReference { get; set; }


        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Security code")]
        [SecurityCode]
        public string SecurityCode { get; set; }
    }
}
