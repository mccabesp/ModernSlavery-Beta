using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Registration.Models;
using ModernSlavery.WebUI.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Registration.Presenters
{
    public interface IRegistrationPresenter
    {
        Task<OrganisationViewModel> CreateOrganisationViewModelAsync(FastTrackViewModel enterCodes, User currentUser);
    }

    public class RegistrationPresenter : IRegistrationPresenter
    {
        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;

        public RegistrationPresenter(IOrganisationBusinessLogic organisationBusinessLogic)
        {
            _organisationBusinessLogic = organisationBusinessLogic;
        }

        public virtual async Task<OrganisationViewModel> CreateOrganisationViewModelAsync(
            FastTrackViewModel enterCodes, User currentUser)
        {
            var org = await _organisationBusinessLogic.GetOrganisationByEmployerReferenceAndSecurityCodeAsync(
                enterCodes.EmployerReference,
                enterCodes.SecurityCode);
            if (org == null) return null;

            var model = new OrganisationViewModel();
            // when SecurityToken is expired then model.SecurityCodeExpired should be true
            model.IsSecurityCodeExpired = org.HasSecurityCodeExpired();

            if (model.IsSecurityCodeExpired)
                return model;

            model.IsRegistered = org.UserOrganisations.Any(uo => uo.User == currentUser && uo.PINConfirmedDate != null);

            model.Employers = new PagedResult<OrganisationRecord>();
            model.Employers.Results = new List<OrganisationRecord> { _organisationBusinessLogic.CreateOrganisationRecord(org) };
            model.SelectedEmployerIndex = 0;

            //Mark the organisation as authorised
            model.SelectedAuthorised = true;
            model.IsFastTrackAuthorised = true;

            return model;
        }
    }
}
