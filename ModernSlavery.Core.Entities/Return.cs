﻿using System;
using System.Collections.Generic;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Entities
{
    public partial class Return
    {
        public Return()
        {
            Organisations = new HashSet<Organisation>();
            StatementStatuses = new HashSet<ReturnStatus>();
        }

        public long ReturnId { get; set; }
        public long OrganisationId { get; set; }
        public DateTime AccountingDate { get; set; }
        public decimal DiffMeanHourlyPayPercent { get; set; }
        public decimal DiffMedianHourlyPercent { get; set; }
        public decimal? DiffMeanBonusPercent { get; set; }
        public decimal? DiffMedianBonusPercent { get; set; }
        public decimal MaleMedianBonusPayPercent { get; set; }
        public decimal FemaleMedianBonusPayPercent { get; set; }
        public decimal MaleLowerPayBand { get; set; }
        public decimal FemaleLowerPayBand { get; set; }
        public decimal MaleMiddlePayBand { get; set; }
        public decimal FemaleMiddlePayBand { get; set; }
        public decimal MaleUpperPayBand { get; set; }
        public decimal FemaleUpperPayBand { get; set; }
        public decimal MaleUpperQuartilePayBand { get; set; }
        public decimal FemaleUpperQuartilePayBand { get; set; }
        public string CompanyLinkToGPGInfo { get; set; }
        public StatementStatuses Status { get; set; }
        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        public string StatusDetails { get; set; }
        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public DateTime Modified { get; set; } = VirtualDateTime.Now;
        public string JobTitle { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int MinEmployees { get; set; }
        public int MaxEmployees { get; set; }
        public bool IsLateSubmission { get; set; }
        public string LateReason { get; set; }
        public string Modifications { get; set; }
        public bool EHRCResponse { get; set; }

        public virtual Organisation Organisation { get; set; }
        public virtual ICollection<Organisation> Organisations { get; set; }
        public virtual ICollection<ReturnStatus> StatementStatuses { get; set; }
    }
}