﻿using System;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Core.Models
{
    [Serializable]
    public class UnfinishedOrgsModel
    {
        public string DUNSNumber { get; set; }
        public string OrganisationReference { get; set; }
        public string SecurityToken { get; set; }
        public DateTime? SecurityTokenCreated { get; set; }

        public SectorTypes SectorType { get; set; }

        public string CompanyNumber { get; set; }
        public string OrganisationName { get; set; }

        public string Title { get; set; }
        public string JobTitle { get; set; }
        public string Company { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
    }
}