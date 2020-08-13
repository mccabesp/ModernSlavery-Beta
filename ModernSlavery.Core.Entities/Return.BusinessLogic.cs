using System;
using System.ComponentModel.DataAnnotations.Schema;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Entities
{
    [Serializable]
    public partial class Return
    {
        [NotMapped]
        public string ResponsiblePerson
        {
            get
            {
                if (string.IsNullOrWhiteSpace(LastName)) return null;

                return $"{FirstName} {LastName} ({JobTitle})";
            }
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;

            var target = (Return) obj;
            return ReturnId == target.ReturnId;
        }

        public override int GetHashCode()
        {
            return ReturnId.GetHashCode();
        }

        public bool IsSubmitted()
        {
            return Status == Entities.StatementStatuses.Submitted;
        }

        public bool HasBonusesPaid()
        {
            return FemaleMedianBonusPayPercent != default(long)
                   || MaleMedianBonusPayPercent != default(long)
                   || DiffMeanBonusPercent != default(long)
                   || DiffMedianBonusPercent != default(long);
        }

        #region Methods

        public ScopeStatuses GetScopeStatus()
        {
            return Organisation.GetActiveScopeStatus(AccountingDate);
        }



        public void SetStatus(StatementStatuses status, long byUserId, string details = null)
        {
            if (status == Status && details == StatusDetails) return;

            StatementStatuses.Add(
                new ReturnStatus
                {
                    ReturnId = ReturnId,
                    Status = status,
                    StatusDate = VirtualDateTime.Now,
                    StatusDetails = details,
                    ByUserId = byUserId
                });
            Status = status;
            StatusDate = VirtualDateTime.Now;
            StatusDetails = details;
        }

        public bool Equals(Return model)
        {
            if (AccountingDate != model.AccountingDate) return false;

            if (CompanyLinkToGPGInfo != model.CompanyLinkToGPGInfo) return false;

            if (DiffMeanBonusPercent != model.DiffMeanBonusPercent) return false;

            if (DiffMeanHourlyPayPercent != model.DiffMeanHourlyPayPercent) return false;

            if (DiffMedianBonusPercent != model.DiffMedianBonusPercent) return false;

            if (DiffMedianHourlyPercent != model.DiffMedianBonusPercent) return false;

            if (FemaleLowerPayBand != model.FemaleLowerPayBand) return false;

            if (FemaleMedianBonusPayPercent != model.FemaleMedianBonusPayPercent) return false;

            if (FemaleMiddlePayBand != model.FemaleMiddlePayBand) return false;

            if (FemaleUpperPayBand != model.FemaleUpperPayBand) return false;

            if (FemaleUpperQuartilePayBand != model.FemaleUpperQuartilePayBand) return false;

            if (FirstName != model.FirstName) return false;

            if (LastName != model.LastName) return false;

            if (JobTitle != model.JobTitle) return false;

            if (MaleLowerPayBand != model.MaleLowerPayBand) return false;

            if (MaleMedianBonusPayPercent != model.MaleMedianBonusPayPercent) return false;

            if (MaleUpperQuartilePayBand != model.MaleUpperQuartilePayBand) return false;

            if (MaleMiddlePayBand != model.MaleMiddlePayBand) return false;

            if (MaleUpperPayBand != model.MaleUpperPayBand) return false;

            if (OrganisationId != model.OrganisationId) return false;

            if (MinEmployees != model.MinEmployees) return false;

            if (MaxEmployees != model.MaxEmployees) return false;

            return true;
        }


        public string GetReportingPeriod()
        {
            return $"{AccountingDate.ToString("yyyy")}/{AccountingDate.AddYears(1).ToString("yy")}";
        }

        #endregion
    }
}