using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Models;

namespace ModernSlavery.WebUI.Registration.Controllers
{
    public partial class RegistrationController : BaseController
    {
        #region AddAddress

        [Authorize]
        [HttpGet("add-address")]
        public async Task<IActionResult> AddAddress()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Get the model from the stash
            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            //Pre-populate address from selected employer
            var employer = model.ManualRegistration ? null : model.GetManualEmployer() ?? model.GetSelectedEmployer();

            if (employer != null)
            {
                var list = employer.GetAddressList();
                model.Address1 = list.Count > 0 ? list[0] : null;
                model.Address2 = list.Count > 1 ? list[1] : null;
                model.Address3 = list.Count > 2 ? list[2] : null;
                model.City = null;
                model.County = null;
                model.Country = null;
                model.Postcode = employer.PostCode;
            }

            return View(nameof(AddAddress), model);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("add-address")]
        public async Task<IActionResult> AddAddress(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.Employers = m.Employers;
            model.ManualEmployers = m.ManualEmployers;

            //Exclude the contact details
            var excludes = new HashSet<string>();
            excludes.AddRange(
                nameof(model.ContactFirstName),
                nameof(model.ContactLastName),
                nameof(model.ContactJobTitle),
                nameof(model.ContactEmailAddress),
                nameof(model.ContactPhoneNumber));

            //Exclude the organisation details
            excludes.AddRange(
                nameof(model.OrganisationName),
                nameof(model.CompanyNumber),
                nameof(model.CharityNumber),
                nameof(model.MutualNumber),
                nameof(model.OtherName),
                nameof(model.OtherValue));

            //Exclude the search
            excludes.AddRange(nameof(model.SearchText));

            //Exclude the SIC Codes
            excludes.Add(nameof(model.SicCodeIds));

            //Exclude the SIC Codes
            excludes.Add(nameof(model.DUNSNumber));

            //Check model is valid
            ModelState.Exclude(excludes.ToArray());
            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<OrganisationViewModel>();
                return View(nameof(AddAddress), model);
            }

            var sector = model.SectorType;
            var authorised = false;
            EmployerRecord employer = null;
            if (!model.ManualRegistration)
            {
                employer = model.GetManualEmployer();

                if (employer != null)
                {
                    authorised = model.ManualAuthorised;
                }
                else
                {
                    employer = model.GetSelectedEmployer();
                    authorised = model.SelectedAuthorised;
                }
            }

            //Set the address source to the user or original source if unchanged
            if (employer != null && model.GetAddressModel().Equals(employer.GetAddressModel()))
                model.AddressSource = employer.AddressSource;
            else
                model.AddressSource = VirtualUser.EmailAddress;

            if (model.WrongAddress) model.ManualAddress = true;

            //When doing manual address only and user is already authorised redirect to confirm page
            if (model.ManualAddress && sector == SectorTypes.Public && authorised && !employer.HasAnyAddress())
            {
                //We don't need contact info if there is no address only when there is an address
                model.ConfirmReturnAction = nameof(AddAddress);
                StashModel(model);
                return RedirectToAction(nameof(ConfirmOrganisation));
            }

            //When manual registration
            StashModel(model);
            return RedirectToAction("AddContact");
        }

        #endregion

        #region AddContact

        [Authorize]
        [HttpGet("add-contact")]
        public async Task<IActionResult> AddContact()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Get the model from the stash
            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            //Pre-load contact details
            if (string.IsNullOrWhiteSpace(model.ContactFirstName))
                model.ContactFirstName = string.IsNullOrWhiteSpace(VirtualUser.ContactFirstName)
                    ? VirtualUser.Firstname
                    : VirtualUser.ContactFirstName;

            if (string.IsNullOrWhiteSpace(model.ContactLastName))
                model.ContactLastName = string.IsNullOrWhiteSpace(VirtualUser.ContactLastName)
                    ? VirtualUser.Lastname
                    : VirtualUser.ContactLastName;

            if (string.IsNullOrWhiteSpace(model.ContactJobTitle))
                model.ContactJobTitle = string.IsNullOrWhiteSpace(VirtualUser.ContactJobTitle)
                    ? VirtualUser.JobTitle
                    : VirtualUser.ContactJobTitle;

            if (string.IsNullOrWhiteSpace(model.ContactEmailAddress))
                model.ContactEmailAddress = string.IsNullOrWhiteSpace(VirtualUser.ContactEmailAddress)
                    ? VirtualUser.EmailAddress
                    : VirtualUser.ContactEmailAddress;

            if (string.IsNullOrWhiteSpace(model.ContactPhoneNumber))
                model.ContactPhoneNumber = VirtualUser.ContactPhoneNumber;

            return View("AddContact", model);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("add-contact")]
        public async Task<IActionResult> AddContact(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.Employers = m.Employers;
            model.ManualEmployers = m.ManualEmployers;

            //Exclude the organisation details
            var excludes = new HashSet<string>();
            excludes.AddRange(
                nameof(model.OrganisationName),
                nameof(model.CompanyNumber),
                nameof(model.CharityNumber),
                nameof(model.MutualNumber),
                nameof(model.OtherName),
                nameof(model.OtherValue));

            //Exclude the address details
            excludes.AddRange(
                nameof(model.Address1),
                nameof(model.Address2),
                nameof(model.Address3),
                nameof(model.City),
                nameof(model.County),
                nameof(model.Country),
                nameof(model.Postcode),
                nameof(model.PoBox));

            //Exclude the search
            excludes.AddRange(nameof(model.SearchText));

            //Exclude the SIC Codes
            excludes.Add(nameof(model.SicCodeIds));

            //Exclude the SIC Codes
            excludes.Add(nameof(model.DUNSNumber));

            //Check model is valid
            ModelState.Exclude(excludes.ToArray());
            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<OrganisationViewModel>();
                return View("AddContact", model);
            }

            //Whenever doing a manual address change redirect to confirm page
            if (model.ManualAddress)
            {
                if (string.IsNullOrWhiteSpace(model.ConfirmReturnAction))
                    model.ConfirmReturnAction = nameof(AddContact);

                StashModel(model);
                return RedirectToAction(nameof(ConfirmOrganisation));
            }

            model.ConfirmReturnAction = nameof(AddContact);
            StashModel(model);
            return RedirectToAction(nameof(ConfirmOrganisation));
        }

        #endregion
    }
}