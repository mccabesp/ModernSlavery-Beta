// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Identity.Models
{
    public class LogoutInputModel
    {
        [Text]
        public string LogoutId { get; set; }
    }
}