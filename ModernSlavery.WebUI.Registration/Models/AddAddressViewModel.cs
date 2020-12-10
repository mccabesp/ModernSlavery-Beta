using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder;
using ModernSlavery.WebUI.Shared.Models;

namespace ModernSlavery.WebUI.Registration.Models
{
    [Serializable]
    public class AddAddressViewModel
    {
        public string AddressReturnAction { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string Address1 { get; set; }

        [MaxLength(100)] public string Address2 { get; set; }

        public string Address3 { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(20, MinimumLength = 3)]
        public string Postcode { get; set; }
    }
}