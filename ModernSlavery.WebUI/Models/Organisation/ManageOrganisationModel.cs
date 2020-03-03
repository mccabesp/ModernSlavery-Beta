using System;
using System.Collections.Generic;
using ModernSlavery.BusinessLogic.Models.Organisation;
using ModernSlavery.Entities;

namespace ModernSlavery.WebUI.Models.Organisation
{

    [Serializable]
    public class ManageOrganisationModel
    {

        public List<ReportInfoModel> ReportInfoModels;
        public UserOrganisation CurrentUserOrg { get; set; }

        public List<UserOrganisation> AssociatedUserOrgs { get; set; }

        public string EncCurrentOrgId { get; set; }

        public Entities.Organisation Organisation => CurrentUserOrg.Organisation;

    }

}
