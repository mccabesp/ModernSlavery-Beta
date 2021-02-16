﻿using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Account.Models
{
    [Serializable]
    public class ResetViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string EmailAddress { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Confirm your email address")]
        [Compare("EmailAddress", ErrorMessage = "The email address and confirmation do not match.")]
        public string ConfirmEmailAddress { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [Password]
        public string Password { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [BindNever]
        public string Resetcode { get; set; }
    }
}