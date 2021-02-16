﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.ComponentModel.DataAnnotations;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Identity.Models
{
    public class LoginInputModel
    {
        [EmailAddress]
        [MaxLength(100)]
        [Required] public string Username { get; set; }

        [MaxLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [Password]
        [Required] public string Password { get; set; }

        public bool RememberLogin { get; set; }
        
        [IgnoreText]
        public string ReturnUrl { get; set; }
    }
}