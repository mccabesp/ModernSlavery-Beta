using System;
using System.ComponentModel.DataAnnotations;

namespace ModernSlavery.WebUI.Registration.Models
{
    [Serializable]
    public class FastTrackViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Organisation reference")]
        public string OrganisationReference { get; set; }


        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Security code")]
        public string SecurityCode { get; set; }
    }
}
