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
    public class AddOrganisationViewModel : BaseViewModel
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string OrganisationName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [CompanyNumber]
        public string CompanyNumber { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string CharityNumber { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string MutualNumber { get; set; }

        public bool NoReference { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string OtherName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string OtherValue { get; set; }

        public bool ContainsReference=> !string.IsNullOrWhiteSpace(CompanyNumber)
                    || !string.IsNullOrWhiteSpace(CharityNumber)
                    || !string.IsNullOrWhiteSpace(MutualNumber)
                    || !string.IsNullOrWhiteSpace(OtherName)
                    || !string.IsNullOrWhiteSpace(OtherValue);
 
    }
}