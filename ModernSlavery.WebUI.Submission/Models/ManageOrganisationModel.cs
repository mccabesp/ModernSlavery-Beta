using System;
using System.Collections.Generic;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.BusinessDomain.Submission.Models;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.WebUI.Submission.Models
{

    [Serializable]
    public class ManageOrganisationModel
    {

        public List<ReportInfoModel> ReportInfoModels;
        public UserOrganisation CurrentUserOrg { get; set; }

        public List<UserOrganisation> AssociatedUserOrgs { get; set; }

        public string EncCurrentOrgId { get; set; }

        public Core.Entities.Organisation Organisation => CurrentUserOrg.Organisation;

    }

}
