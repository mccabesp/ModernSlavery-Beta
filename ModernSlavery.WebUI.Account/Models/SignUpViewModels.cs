﻿using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Resources;

namespace ModernSlavery.WebUI.Account.Models
{
    [Serializable]
    [DefaultResource(typeof(AccountResources))]
    public class SignUpViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "First name")]
        [Text]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Last name")]
        [Text] 
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Job title")]
        [StringLength(50, ErrorMessage = "Your job title cannot be longer than {1} characters.")]
        [Text] 
        public string JobTitle { get; set; }

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

        [Display(Name = nameof(SendUpdates))] public bool SendUpdates { get; set; }

        [Display(Name = nameof(AllowContact))] public bool AllowContact { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}