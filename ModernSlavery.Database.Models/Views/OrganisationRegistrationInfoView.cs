﻿using System;
using System.Collections.Generic;

namespace ModernSlavery.Entities
{
    public partial class OrganisationRegistrationInfoView
    {
        public long OrganisationId { get; set; }
        public string UserInfo { get; set; }
        public string ContactInfo { get; set; }
        public string RegistrationMethod { get; set; }
        public DateTime? PinsentDate { get; set; }
        public DateTime? PinconfirmedDate { get; set; }
        public DateTime? ConfirmAttemptDate { get; set; }
        public int ConfirmAttempts { get; set; }
    }
}
