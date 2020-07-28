using System;
using System.Collections.Generic;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.WebUI.Submission.Models
{
    [Serializable]
    public class ManageOrganisationModel
    {
        public string CurrentUserIdentifier;
        public string CurrentUserFullName;
        public string OrganisationName;
        public string LatestAddress;

        public IAsyncEnumerable<StatementInfoModel> StatementInfoModels;

        public List<UserOrganisation> AssociatedUserOrgs { get; set; }

        public string OrganisationIdentifier { get; set; }
    }
}