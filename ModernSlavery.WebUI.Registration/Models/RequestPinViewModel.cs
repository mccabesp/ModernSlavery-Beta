using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Registration.Models
{
    [Serializable]
    public class RequestPinViewModel
    {
        public long OrganisationId { get; set; }
        public string UserFullName { get; set; }
        public string UserJobTitle { get; set; }
        public string OrganisationName { get; set; }
        public string Address { get; set; }
    }
}
