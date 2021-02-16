using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Registration.Models;
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

            //Pre-populate address from selected organisation
            var organisation = model.IsManualRegistration ? null : model.GetManualOrganisation() ?? model.GetSelectedOrganisation();

            if (organisation != null)
            {
                var list = organisation.GetAddressList();
                model.Address1 = organisation.Address1;
                model.Address2 = organisation.Address2;
                model.Address3 = organisation.Address3;
                model.City = organisation.City;
                model.County = organisation.County;
                model.Country = organisation.Country;
                model.Postcode = organisation.PostCode;
            }

            var addAddressViewModel = new AddAddressViewModel
            {
                AddressReturnAction = model.AddressReturnAction,
                Address1 = model.Address1,
                Address2 = model.Address2,
                City = model.City,
                County = model.County,
                Country = model.Country,
                Postcode = model.Postcode
            }; 
            return View(nameof(AddAddress), addAddressViewModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("add-address")]
        public async Task<IActionResult> AddAddress(AddAddressViewModel addAddressViewModel)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<AddAddressViewModel>();
                return View(nameof(AddAddress), addAddressViewModel);
            }

            //Make sure we can load organisations from session
            var organisationViewModel = UnstashModel<OrganisationViewModel>();
            if (organisationViewModel == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            organisationViewModel.Address1 = addAddressViewModel.Address1;
            organisationViewModel.Address2 = addAddressViewModel.Address2;
            organisationViewModel.Address3 = null;
            organisationViewModel.City = addAddressViewModel.City;
            organisationViewModel.County = addAddressViewModel.County;
            organisationViewModel.Country = addAddressViewModel.Country;
            organisationViewModel.PoBox = null;
            organisationViewModel.Postcode = addAddressViewModel.Postcode;

            var authorised = false;
            OrganisationRecord organisationRecord = null;
            if (!organisationViewModel.IsManualRegistration)
            {
                organisationRecord = organisationViewModel.GetManualOrganisation();

                if (organisationRecord != null)
                {
                    authorised = organisationViewModel.IsManualAuthorised;
                }
                else
                {
                    organisationRecord = organisationViewModel.GetSelectedOrganisation();
                    authorised = organisationViewModel.IsFastTrackAuthorised;
                }
            }

            //Set the address source to the user or original source if unchanged
            if (organisationRecord != null && organisationViewModel.GetAddressModel().Equals(organisationRecord))
                organisationViewModel.AddressSource = organisationRecord.AddressSource;
            else
                organisationViewModel.AddressSource = VirtualUser.EmailAddress;

            if (organisationViewModel.IsWrongAddress) organisationViewModel.IsManualAddress = true;

            //When doing manual address only and user is already authorised redirect to confirm page
            if (organisationViewModel.IsManualAddress && organisationViewModel.SectorType == SectorTypes.Public && authorised && !organisationRecord.HasAnyAddress())
            {
                //We don't need contact info if there is no address only when there is an address
                organisationViewModel.ConfirmReturnAction = nameof(AddAddress);
                StashModel(organisationViewModel);
                return RedirectToAction(nameof(ConfirmOrganisation));
            }

            //When manual registration
            StashModel(organisationViewModel);
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

            var addContactViewModel = new AddContactViewModel
            {

                ContactFirstName = model.ContactFirstName,
                ContactLastName = model.ContactLastName,
                ContactJobTitle = model.ContactJobTitle, 
                ContactEmailAddress = model.ContactEmailAddress,
                ContactPhoneNumber= model.ContactPhoneNumber
            };

            return View("AddContact", addContactViewModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("add-contact")]
        public async Task<IActionResult> AddContact(AddContactViewModel addContactViewModel)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<AddContactViewModel>();
                return View("AddContact", addContactViewModel);
            }

            //Make sure we can load organisation from session
            var organisationViewModel = UnstashModel<OrganisationViewModel>();
            if (organisationViewModel == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            organisationViewModel.ContactFirstName=addContactViewModel.ContactFirstName;
            organisationViewModel.ContactLastName = addContactViewModel.ContactLastName;
            organisationViewModel.ContactJobTitle = addContactViewModel.ContactJobTitle;
            organisationViewModel.ContactEmailAddress = addContactViewModel.ContactEmailAddress;
            organisationViewModel.ContactPhoneNumber = addContactViewModel.ContactPhoneNumber;

            //Whenever doing a manual address change redirect to confirm page
            if (organisationViewModel.IsManualAddress)
            {
                if (string.IsNullOrWhiteSpace(organisationViewModel.ConfirmReturnAction))
                    organisationViewModel.ConfirmReturnAction = nameof(AddContact);

                StashModel(organisationViewModel);
                return RedirectToAction(nameof(ConfirmOrganisation));
            }

            organisationViewModel.ConfirmReturnAction = nameof(AddContact);
            StashModel(organisationViewModel);
            return RedirectToAction(nameof(ConfirmOrganisation));
        }

        #endregion
    }
}