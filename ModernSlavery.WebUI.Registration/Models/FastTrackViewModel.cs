﻿using System;
using System.ComponentModel.DataAnnotations;

namespace ModernSlavery.WebUI.Registration.Models
{
    [Serializable]
    public class FastTrackViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "You must enter an employer reference")]
        [Display(Name = "Employer reference")]
        public string EmployerReference { get; set; }


        [Required(AllowEmptyStrings = false, ErrorMessage = "You must enter a security code")]
        [Display(Name = "Security code")]
        public string SecurityCode { get; set; }
    }
}