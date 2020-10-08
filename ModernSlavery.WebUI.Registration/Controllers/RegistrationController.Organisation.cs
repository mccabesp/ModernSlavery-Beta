using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.WebUI.Registration.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Models;
using Newtonsoft.Json;

namespace ModernSlavery.WebUI.Registration.Controllers
{
    public partial class RegistrationController : BaseController
    {
        #region Organisation Type

        [Authorize]
        [HttpGet("organisation-type")]
        public async Task<IActionResult> OrganisationTypeAsync()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var model = new OrganisationViewModel();
            model.Organisations = new PagedResult<OrganisationRecord>();
            StashModel(model);
            if (VirtualUser.UserOrganisations.Any())
                model.BackAction = Url.ActionArea("ManageOrganisations", "Submission", "Submission");

            return View("OrganisationType", model);
        }

        /// <summary>
        ///     Get the sector type
        /// </summary>
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("organisation-type")]
        public async Task<IActionResult> OrganisationType(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load organisations from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.Organisations = m.Organisations;

            ModelState.Clear();

            if (model.RegistrationType.Equals("fasttrack"))
                return RedirectToAction(nameof(FastTrack));
            else if (model.RegistrationType.Equals("private"))
                model.SectorType = SectorTypes.Private;
            else if (model.RegistrationType.Equals("public"))
                model.SectorType = SectorTypes.Public;
            else
            {
                // either it is fast track, or sectortype mut be set
                AddModelError(3005, "RegistrationType");
                this.SetModelCustomErrors<OrganisationViewModel>();
                return View("OrganisationType", model);
            }

            CompaniesHouseFailures = 0;

            StashModel(model);
            return RedirectToAction("OrganisationSearch");
        }

        #endregion

        #region Organisation Search

        /// Search organisation
        [Authorize]
        [HttpGet("organisation-search")]
        public async Task<IActionResult> OrganisationSearch()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load organisations from session
            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.ManualRegistration = true;
            model.BackAction = "OrganisationSearch";
            model.OrganisationName = null;
            model.CompanyNumber = null;
            model.Address1 = null;
            model.Address2 = null;
            model.Address3 = null;
            model.Country = null;
            model.Postcode = null;
            model.PoBox = null;

            StashModel(model);

            return View("OrganisationSearch", model);
        }

        /// <summary>
        ///     Search organisation submit
        /// </summary>
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("organisation-search")]
        public async Task<IActionResult> OrganisationSearch(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            ModelState.Include("SearchText");
            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<OrganisationViewModel>();
                return View("OrganisationSearch", model);
            }

            //Make sure we can load organisations from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.Organisations = m.Organisations;
            model.ManualRegistration = true;
            model.BackAction = "OrganisationSearch";
            model.SelectedOrganisationIndex = -1;
            model.OrganisationName = null;
            model.CompanyNumber = null;
            model.Address1 = null;
            model.Address2 = null;
            model.Address3 = null;
            model.Country = null;
            model.Postcode = null;
            model.PoBox = null;

            model.SearchText = model.SearchText.TrimI();

            switch (model.SectorType)
            {
                case SectorTypes.Private:
                    try
                    {
                        model.Organisations = await _registrationService.PrivateSectorRepository.SearchAsync(
                            model.SearchText,
                            1,
                            SharedBusinessLogic.SharedOptions.OrganisationPageSize,
                            VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, ex.Message);

                        CompaniesHouseFailures++;
                        if (CompaniesHouseFailures < 3)
                        {
                            model.Organisations?.Results?.Clear();
                            StashModel(model);
                            ModelState.AddModelError(1141);
                            return View(model);
                        }

                        await _registrationService.SharedBusinessLogic.SendEmailService.SendGeoMessageAsync(
                            "GPG - COMPANIES HOUSE ERROR",
                            $"Cant search using Companies House API for query '{model.SearchText}' page:'1' due to following error:\n\n{ex.GetDetailsText()}",
                            VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));
                        return View("CustomError", WebService.ErrorViewModelFactory.Create(1140));
                    }

                    break;
                case SectorTypes.Public:
                    model.Organisations = await _registrationService.PublicSectorRepository.SearchAsync(
                        model.SearchText,
                        1,
                        SharedBusinessLogic.SharedOptions.OrganisationPageSize,
                        VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));

                    break;

                default:
                    throw new NotImplementedException();
            }

            ModelState.Clear();
            model.LastPrivateSearchRemoteTotal = LastPrivateSearchRemoteTotal;
            if (LastPrivateSearchRemoteTotal == -1)
            {
                CompaniesHouseFailures++;
                if (CompaniesHouseFailures >= 3)
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1140));
            }
            else
            {
                CompaniesHouseFailures = 0;
            }

            StashModel(model);

            //Search again if no results
            if (model.Organisations.Results.Count < 1) return View("OrganisationSearch", model);

            //Go to step 5 with results
            if (Request.Query["fail"].ToBoolean())
                return RedirectToAction("ChooseOrganisation", "Registration", new { fail = true });

            return RedirectToAction("ChooseOrganisation");
        }

        #endregion

        #region Choose Organisation

        /// <summary>
        ///     Choose organisation view results
        /// </summary>
        [Authorize]
        [HttpGet("choose-organisation")]
        public async Task<IActionResult> ChooseOrganisation()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load organisations from session
            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.NoReference = false;
            model.ManualRegistration = false;
            model.AddressReturnAction = null;
            model.ConfirmReturnAction = null;
            model.ManualAuthorised = false;
            model.ManualAddress = false;
            model.BackAction = "ChooseOrganisation";
            model.OrganisationName = null;
            model.CompanyNumber = null;
            model.AddressSource = null;
            model.Address1 = null;
            model.Address2 = null;
            model.Address3 = null;
            model.City = null;
            model.County = null;
            model.Country = null;
            model.PoBox = null;
            model.Postcode = null;
            model.ContactEmailAddress = null;
            model.ContactFirstName = null;
            model.ContactLastName = null;
            model.ContactJobTitle = null;
            model.ContactPhoneNumber = null;
            model.SicCodeIds = null;

            StashModel(model);

            return View("ChooseOrganisation", model);
        }


        /// <summary>
        ///     Choose organisation with paging or search
        /// </summary>
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("choose-organisation")]
        public async Task<IActionResult> ChooseOrganisation(OrganisationViewModel model, string command)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load organisations from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.Organisations = m.Organisations;

            var nextPage = m.Organisations.CurrentPage;

            model.SelectedOrganisationIndex = -1;
            model.OrganisationName = null;
            model.CompanyNumber = null;
            model.Address1 = null;
            model.Address2 = null;
            model.Address3 = null;
            model.Country = null;
            model.Postcode = null;
            model.PoBox = null;
            model.ManualAuthorised = false;
            model.ManualAddress = false;

            var doSearch = false;
            ModelState.Include("SearchText");
            if (command == "search")
            {
                model.SearchText = model.SearchText.TrimI();

                if (!ModelState.IsValid)
                {
                    this.SetModelCustomErrors<OrganisationViewModel>();
                    return View("ChooseOrganisation", model);
                }

                nextPage = 1;
                doSearch = true;
            }
            else if (command == "pageNext")
            {
                if (nextPage >= model.Organisations.PageCount) throw new Exception("Cannot go past last page");

                nextPage++;
                doSearch = true;
            }
            else if (command == "pagePrev")
            {
                if (nextPage <= 1) throw new Exception("Cannot go before previous page");

                nextPage--;
                doSearch = true;
            }
            else if (command.StartsWithI("page_"))
            {
                var page = command.AfterFirst("page_").ToInt32();
                if (page < 1 || page > model.Organisations.PageCount) throw new Exception("Invalid page selected");

                if (page != nextPage)
                {
                    nextPage = page;
                    doSearch = true;
                }
            }

            if (doSearch)
            {
                switch (model.SectorType)
                {
                    case SectorTypes.Private:
                        try
                        {
                            model.Organisations = await _registrationService.PrivateSectorRepository.SearchAsync(
                                model.SearchText,
                                nextPage,
                                SharedBusinessLogic.SharedOptions.OrganisationPageSize,
                                VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, ex.Message);

                            CompaniesHouseFailures++;
                            if (CompaniesHouseFailures < 3)
                            {
                                model.Organisations?.Results?.Clear();
                                StashModel(model);
                                ModelState.AddModelError(1141);
                                return View(model);
                            }

                            await _registrationService.SharedBusinessLogic.SendEmailService.SendGeoMessageAsync(
                                "GPG - COMPANIES HOUSE ERROR",
                                $"Cant search using Companies House API for query '{model.SearchText}' page:'1' due to following error:\n\n{ex.GetDetailsText()}",
                                VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));
                            return View("CustomError", WebService.ErrorViewModelFactory.Create(1140));
                        }

                        model.LastPrivateSearchRemoteTotal = LastPrivateSearchRemoteTotal;
                        if (LastPrivateSearchRemoteTotal == -1)
                        {
                            CompaniesHouseFailures++;
                            if (CompaniesHouseFailures >= 3)
                                return View("CustomError", WebService.ErrorViewModelFactory.Create(1140));
                        }
                        else
                        {
                            CompaniesHouseFailures = 0;
                        }

                        break;

                    case SectorTypes.Public:
                        model.Organisations = await _registrationService.PublicSectorRepository.SearchAsync(
                            model.SearchText,
                            nextPage,
                            SharedBusinessLogic.SharedOptions.OrganisationPageSize,
                            VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));
                        break;

                    default:
                        throw new NotImplementedException();
                }

                ModelState.Clear();
                StashModel(model);

                //Go back if no results
                if (model.Organisations.Results.Count < 1) return RedirectToAction("OrganisationSearch");

                //Otherwise show results
                return View("ChooseOrganisation", model);
            }

            if (command.StartsWithI("organisation_"))
            {
                var organisationIndex = command.AfterFirst("organisation_").ToInt32();
                var organisation = model.Organisations.Results[organisationIndex];

                //Ensure organisations from companies house have a sector
                if (organisation.SectorType == SectorTypes.Unknown) organisation.SectorType = model.SectorType.Value;

                //Make sure user is fully registered for one private org before registering another 
                if (model.SectorType == SectorTypes.Private
                    && VirtualUser.UserOrganisations.Any()
                    && !VirtualUser.UserOrganisations.Any(uo => uo.PINConfirmedDate != null))
                {
                    AddModelError(3022);
                    this.SetModelCustomErrors<OrganisationViewModel>();
                    return View("ChooseOrganisation", model);
                }

                //Get the organisation from the database
                var org = organisation.OrganisationId > 0
                    ? SharedBusinessLogic.DataRepository.Get<Organisation>(organisation.OrganisationId)
                    : null;
                if (org == null && !string.IsNullOrWhiteSpace(organisation.CompanyNumber))
                    org = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                        .FirstOrDefault(o => o.CompanyNumber != null && o.CompanyNumber == organisation.CompanyNumber);

                if (org == null && !string.IsNullOrWhiteSpace(organisation.OrganisationReference))
                    org = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                        .FirstOrDefault(o => o.OrganisationReference == organisation.OrganisationReference);

                if (org != null)
                {
                    //Make sure the found organisation is active or pending
                    if (org.Status != OrganisationStatuses.Active && org.Status != OrganisationStatuses.Pending)
                    {
                        Logger.LogWarning(
                            $"Attempt to register a {org.Status} organisation",
                            $"Organisation: '{org.OrganisationName}' Reference: '{org.OrganisationReference}' User: '{VirtualUser.EmailAddress}'");
                        return View("CustomError", WebService.ErrorViewModelFactory.Create(1149));
                    }

                    //Make sure the found organisation is of the correct sector type
                    if (org.SectorType != model.SectorType)
                        return View("CustomError",
                            WebService.ErrorViewModelFactory.Create(model.SectorType == SectorTypes.Private
                                ? 1146
                                : 1147));

                    //Ensure user is not already registered for this organisation
                    var userOrg = SharedBusinessLogic.DataRepository.GetAll<UserOrganisation>()
                        .FirstOrDefault(
                            uo => uo.OrganisationId == org.OrganisationId && uo.UserId == VirtualUser.UserId);
                    if (userOrg != null)
                    {
                        AddModelError(userOrg.PINConfirmedDate == null ? 3021 : 3020);
                        this.SetModelCustomErrors<OrganisationViewModel>();
                        return View("ChooseOrganisation", model);
                    }

                    //Ensure there isnt another pending registeration for this organisation
                    //userOrg = SharedBusinessLogic.DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.OrganisationId == org.OrganisationId && uo.UserId != VirtualUser.UserId && uo.PINSentDate!=null && uo.PINConfirmedDate==null);
                    //if (userOrg != null)
                    //{
                    //    var remainingTime = userOrg.PINSentDate.Value.AddDays(SharedBusinessLogic.SharedOptions.PinInPostExpiryDays) - VirtualDateTime.Now;
                    //    return View("CustomError", WebService.ErrorViewModelFactory.Create(1148, new { remainingTime = remainingTime.ToFriendly(maxParts: 2) }));
                    //}

                    organisation.OrganisationId = org.OrganisationId;
                }

                model.SelectedOrganisationIndex = organisationIndex;


                //Make sure the organisation has an address
                if (organisation.SectorType == SectorTypes.Public)
                {
                    model.ManualRegistration = false;
                    model.SelectedAuthorised = organisation.IsAuthorised(VirtualUser.EmailAddress);
                    if (!model.SelectedAuthorised || !organisation.HasAnyAddress())
                    {
                        model.ManualAddress = true;
                        model.AddressReturnAction = nameof(ChooseOrganisation);
                        StashModel(model);
                        return RedirectToAction("AddAddress");
                    }
                }
                else if (organisation.SectorType == SectorTypes.Private && !organisation.HasAnyAddress())
                {
                    model.AddressReturnAction = nameof(ChooseOrganisation);
                    model.ManualRegistration = false;
                    model.ManualAddress = true;
                    StashModel(model);
                    return RedirectToAction("AddAddress");
                }

                model.ManualRegistration = false;
                model.ManualAddress = false;
                model.AddressReturnAction = null;
            }

            ModelState.Clear();

            //If we havend selected one the reshow same view
            if (model.SelectedOrganisationIndex < 0) return View("ChooseOrganisation", model);

            model.ConfirmReturnAction = nameof(ChooseOrganisation);
            StashModel(model);
            //If private sector add organisation address
            return RedirectToAction(nameof(ConfirmOrganisation));
        }

        #endregion

        #region Add Organisation

        [Authorize]
        [HttpGet("add-organisation")]
        public async Task<IActionResult> AddOrganisation()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Get the model from the stash
            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            //Prepopulate name if it empty
            if (string.IsNullOrWhiteSpace(model.OrganisationName)
                && !string.IsNullOrWhiteSpace(model.SearchText)
                && (model.SearchText.Length != 8 || !model.SearchText.ContainsNumber()))
                model.OrganisationName = model.SearchText;

            model.ManualRegistration = true;
            model.ManualAuthorised = false;
            model.ManualAddress = false;
            StashModel(model);

            return View("AddOrganisation", model);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("add-organisation")]
        public async Task<IActionResult> AddOrganisation(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load organisations from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.Organisations = m.Organisations;

            //Exclude the address details
            var excludes = new HashSet<string>();
            excludes.AddRange(
                nameof(model.Address1),
                nameof(model.Address2),
                nameof(model.Address3),
                nameof(model.City),
                nameof(model.County),
                nameof(model.Country),
                nameof(model.Postcode),
                nameof(model.PoBox));

            //Exclude the contact details
            excludes.AddRange(
                nameof(model.ContactFirstName),
                nameof(model.ContactLastName),
                nameof(model.ContactJobTitle),
                nameof(model.ContactEmailAddress),
                nameof(model.ContactPhoneNumber));

            //Exclude the SIC Codes
            excludes.Add(nameof(model.SicCodeIds));

            //Exclude the SIC Codes
            excludes.Add(nameof(model.DUNSNumber));

            if (model.NoReference)
            {
                excludes.AddRange(
                    nameof(model.CompanyNumber),
                    nameof(model.CharityNumber),
                    nameof(model.MutualNumber),
                    nameof(model.OtherName),
                    nameof(model.OtherValue));
                if (!string.IsNullOrWhiteSpace(model.CompanyNumber)
                    || !string.IsNullOrWhiteSpace(model.CharityNumber)
                    || !string.IsNullOrWhiteSpace(model.MutualNumber)
                    || !string.IsNullOrWhiteSpace(model.OtherName)
                    || !string.IsNullOrWhiteSpace(model.OtherValue))
                    ModelState.AddModelError("", "You must clear all your reference fields");
            }
            else if (!string.IsNullOrWhiteSpace(model.CompanyNumber)
                     || !string.IsNullOrWhiteSpace(model.CharityNumber)
                     || !string.IsNullOrWhiteSpace(model.MutualNumber)
                     || !string.IsNullOrWhiteSpace(model.OtherName)
                     || !string.IsNullOrWhiteSpace(model.OtherValue))
            {
                if (string.IsNullOrWhiteSpace(model.CompanyNumber)) excludes.Add(nameof(model.CompanyNumber));

                if (string.IsNullOrWhiteSpace(model.CharityNumber)) excludes.Add(nameof(model.CharityNumber));

                if (string.IsNullOrWhiteSpace(model.MutualNumber)) excludes.Add(nameof(model.MutualNumber));

                if (string.IsNullOrWhiteSpace(model.OtherName))
                {
                    if (model.OtherName.ReplaceI(" ")
                        .EqualsI("CompanyNumber", "CompanyNo", "CompanyReference", "CompanyRef"))
                        ModelState.AddModelError(nameof(model.OtherName),
                            "Cannot user Company Number as an Other reference");
                    else if (model.OtherName.ReplaceI(" ")
                        .EqualsI("CharityNumber", "CharityNo", "CharityReference", "CharityRef"))
                        ModelState.AddModelError(nameof(model.OtherName),
                            "Cannot user Charity Number as an Other reference");
                    else if (model.OtherName.ReplaceI(" ")
                        .EqualsI(
                            "MutualNumber",
                            "MutualNo",
                            "MutualReference",
                            "MutualRef",
                            "MutualPartnsershipNumber",
                            "MutualPartnsershipNo",
                            "MutualPartnsershipReference",
                            "MutualPartnsershipRef"))
                        ModelState.AddModelError(nameof(model.OtherName),
                            "Cannot user Mutual Partnership Number as an Other reference");

                    if (string.IsNullOrWhiteSpace(model.OtherValue))
                    {
                        excludes.Add(nameof(model.OtherName));
                        excludes.Add(nameof(model.OtherValue));
                    }
                }
            }

            //Check model is valid
            ModelState.Exclude(excludes.ToArray());
            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<OrganisationViewModel>();
                return View("AddOrganisation", model);
            }

            //Check the company doesnt already exist
            IEnumerable<long> results;
            var orgIds = new HashSet<long>();
            var orgIdrefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!model.NoReference)
            {
                if (!string.IsNullOrWhiteSpace(model.CompanyNumber))
                {
                    results = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                        .Where(o => o.CompanyNumber == model.CompanyNumber)
                        .Select(o => o.OrganisationId);
                    if (results.Any())
                    {
                        orgIdrefs.Add(nameof(model.CompanyNumber));
                        orgIds.AddRange(results);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.CharityNumber))
                {
                    results = SharedBusinessLogic.DataRepository.GetAll<OrganisationReference>()
                        .Where(
                            r => r.ReferenceName.ToLower() == nameof(OrganisationViewModel.CharityNumber).ToLower()
                                 && r.ReferenceValue.ToLower() == model.CharityNumber.ToLower())
                        .Select(r => r.OrganisationId);
                    if (results.Any())
                    {
                        orgIdrefs.Add(nameof(model.CharityNumber));
                        orgIds.AddRange(results);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.MutualNumber))
                {
                    results = SharedBusinessLogic.DataRepository.GetAll<OrganisationReference>()
                        .Where(
                            r => r.ReferenceName.ToLower() == nameof(OrganisationViewModel.MutualNumber).ToLower()
                                 && r.ReferenceValue.ToLower() == model.MutualNumber.ToLower())
                        .Select(r => r.OrganisationId);
                    if (results.Any())
                    {
                        orgIdrefs.Add(nameof(model.MutualNumber));
                        orgIds.AddRange(results);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.OtherName) && !string.IsNullOrWhiteSpace(model.OtherValue))
                {
                    if (model.IsDUNS)
                    {
                        results = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                            .Where(r => r.DUNSNumber.ToLower() == model.OtherValue.ToLower())
                            .Select(r => r.OrganisationId);
                        if (results.Any())
                        {
                            orgIdrefs.Add(nameof(model.OtherName));
                            orgIds.AddRange(results);
                        }
                    }

                    results = SharedBusinessLogic.DataRepository.GetAll<OrganisationReference>()
                        .Where(
                            r => r.ReferenceName.ToLower() == model.OtherName.ToLower()
                                 && r.ReferenceValue.ToLower() == model.OtherValue.ToLower())
                        .Select(r => r.OrganisationId);
                    if (results.Any())
                    {
                        orgIdrefs.Add(nameof(model.OtherName));
                        orgIds.AddRange(results);
                    }
                }
            }

            model.MatchedReferenceCount = orgIds.Count;

            //Only show orgs matching names when none matching references
            if (model.MatchedReferenceCount == 0)
            {
                var orgName = model.OrganisationName.ToLower().ReplaceI("limited", "").ReplaceI("ltd", "");
                results = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .Where(o => o.OrganisationName.Contains(orgName))
                    .Select(o => o.OrganisationId);
                if (results.Any()) orgIds.AddRange(results);


                results = _registrationService.OrganisationBusinessLogic.SearchOrganisations(model.OrganisationName, 49)
                    .Select(o => o.OrganisationId);
                if (results.Any()) orgIds.AddRange(results);
            }

            model.ManualRegistration = true;
            model.ManualOrganisationIndex = -1;
            model.NameSource = VirtualUser.EmailAddress;

            if (!orgIds.Any())
            {
                model.AddressReturnAction = nameof(AddOrganisation);
                StashModel(model);
                return RedirectToAction("AddAddress");
            }

            var organisations =
                await SharedBusinessLogic.DataRepository.ToListAscendingAsync<Organisation, string>(
                    o => o.OrganisationName,
                    o => orgIds.Contains(o.OrganisationId));

            model.ManualOrganisations = organisations.Select(o => _registrationService.OrganisationBusinessLogic.CreateOrganisationRecord(o)).ToList();

            //Ensure exact match shown at top
            if (model.ManualOrganisations != null && model.ManualOrganisations.Count > 1)
            {
                var index = model.ManualOrganisations.FindIndex(e => e.OrganisationName.EqualsI(model.OrganisationName));
                if (index > 0)
                {
                    model.ManualOrganisations.Insert(0, model.ManualOrganisations[index]);
                    model.ManualOrganisations.RemoveAt(index + 1);
                }
            }

            if (model.MatchedReferenceCount == 1)
            {
                model.ManualOrganisationIndex = 0;
                StashModel(model);
                return await SelectOrganisation(VirtualUser, model, model.ManualOrganisationIndex, nameof(AddOrganisation));
            }

            StashModel(model);
            return RedirectToAction("SelectOrganisation");
        }

        #endregion

        #region Select Organisation

        [Authorize]
        [HttpGet("select-organisation")]
        public async Task<IActionResult> SelectOrganisation()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Get the model from the stash
            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.ManualAuthorised = false;
            model.ManualRegistration = true;
            model.ManualAddress = false;
            StashModel(model);
            return View("SelectOrganisation", model);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("select-organisation")]
        public async Task<IActionResult> SelectOrganisation(string command)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load organisations from session
            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            if (command.EqualsI("Continue"))
            {
                //Ensure they select one of the matched references
                if (model.MatchedReferenceCount > 0) throw new ArgumentException(nameof(model.MatchedReferenceCount));

                model.ManualRegistration = true;
                model.ManualOrganisationIndex = -1;
                model.AddressReturnAction = nameof(SelectOrganisation);
                StashModel(model);
                return RedirectToAction("AddAddress");
            }

            var organisationIndex = command.AfterFirst("organisation_").ToInt32();

            return await SelectOrganisation(VirtualUser, model, organisationIndex, nameof(SelectOrganisation));
        }

        [NonAction]
        protected async Task<IActionResult> SelectOrganisation(User VirtualUser,
            OrganisationViewModel model,
            int organisationIndex,
            string returnAction)
        {
            if (organisationIndex < 0) return new HttpBadRequestResult($"Invalid organisation index {organisationIndex}");

            model.ManualOrganisationIndex = organisationIndex;
            model.ManualAuthorised = false;

            var organisation = model.GetManualOrganisation();

            var org = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                .FirstOrDefault(o => o.OrganisationId == organisation.OrganisationId);

            //Make sure the found organisation is active or pending
            if (org.Status != OrganisationStatuses.Active && org.Status != OrganisationStatuses.Pending)
            {
                Logger.LogWarning(
                    $"Attempt to register a {org.Status} organisation",
                    $"Organisation: '{org.OrganisationName}' Reference: '{org.OrganisationReference}' User: '{VirtualUser.EmailAddress}'");
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1149));
            }

            if (org.SectorType == SectorTypes.Private)
                //Make sure they are fully registered for one before requesting another
                if (VirtualUser.UserOrganisations.Any() &&
                    !VirtualUser.UserOrganisations.Any(uo => uo.PINConfirmedDate != null))
                {
                    AddModelError(3022);
                    this.SetModelCustomErrors<OrganisationViewModel>();
                    return View(returnAction, model);
                }


            var userOrg = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo =>
                uo.OrganisationId == org.OrganisationId && uo.UserId == VirtualUser.UserId);
            if (userOrg != null)
            {
                AddModelError(userOrg.PINConfirmedDate == null ? 3021 : 3020);
                this.SetModelCustomErrors<OrganisationViewModel>();
                return View(returnAction, model);
            }

            //If the organisation already exists in DB then use its address and not that from CoHo
            //if (org.LatestAddress != null) organisation.ActiveAddressId = org.LatestAddress.AddressId;

            //Make sure the organisation has an address
            if (organisation.SectorType == SectorTypes.Public)
            {
                model.ManualAuthorised = organisation.IsAuthorised(VirtualUser.EmailAddress);
                if (!model.ManualAuthorised || !organisation.HasAnyAddress()) model.ManualAddress = true;
            }
            else if (organisation.SectorType == SectorTypes.Private && !organisation.HasAnyAddress())
            {
                model.ManualAddress = true;
            }

            model.ManualRegistration = false;
            if (model.ManualAddress)
            {
                model.AddressReturnAction = returnAction;
                StashModel(model);
                return RedirectToAction("AddAddress");
            }

            model.ConfirmReturnAction = returnAction;
            model.AddressSource = null;
            model.Address1 = null;
            model.Address2 = null;
            model.Address3 = null;
            model.City = null;
            model.County = null;
            model.Country = null;
            model.PoBox = null;
            model.Postcode = null;
            model.ContactEmailAddress = null;
            model.ContactFirstName = null;
            model.ContactLastName = null;
            model.ContactJobTitle = null;
            model.ContactPhoneNumber = null;
            model.SicCodeIds = null;

            StashModel(model);
            //If private sector add organisation address
            return RedirectToAction(nameof(ConfirmOrganisation));
        }

        #endregion

        #region Confirm Organisation

        /// <summary>
        ///     Show user the confirm organisation view
        /// </summary>
        [Authorize]
        [HttpGet("confirm-organisation")]
        public async Task<IActionResult> ConfirmOrganisation()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load organisations from session
            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            //Get the sic codes from companies house
            OrganisationRecord organisation = null;
            if (!model.ManualRegistration) organisation = model.GetManualOrganisation() ?? model.GetSelectedOrganisation();

            #region Get the sic codes if there isnt any

            if (organisation != null)
            {
                if (!model.ManualRegistration && string.IsNullOrWhiteSpace(organisation.SicCodeIds))
                {
                    organisation.SicSource = "CoHo";
                    if (VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
                    {
                        var sic = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<SicCode>(s =>
                            s.SicSectionId != "X");
                        organisation.SicCodeIds = sic?.SicCodeId.ToString();
                    }
                    else if (organisation.OrganisationId > 0)
                    {
                        var org= await SharedBusinessLogic.DataRepository.GetAsync<Organisation>(organisation.OrganisationId);
                        if (org != null)
                        {
                            var sicCodes = org.GetLatestSicCodes();
                            if (sicCodes.Any())
                            {
                                organisation.SicCodeIds = sicCodes.OrderBy(s => s.SicCodeId).Select(s => s.SicCodeId).ToDelimitedString();
                                organisation.SicSource = sicCodes.First().Source;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            if (organisation.SectorType == SectorTypes.Public)
                                organisation.SicCodeIds =
                                    await _registrationService.PublicSectorRepository.GetSicCodesAsync(
                                        organisation.CompanyNumber);
                            else
                                organisation.SicCodeIds =
                                    await _registrationService.PrivateSectorRepository.GetSicCodesAsync(
                                        organisation.CompanyNumber);

                            organisation.SicSource = "CoHo";
                        }
                        catch (Exception ex)
                        {
                            CompaniesHouseFailures++;
                            if (CompaniesHouseFailures < 3)
                            {
                                StashModel(model);
                                ModelState.AddModelError(1142);
                                return View(model);
                            }

                            await _registrationService.SharedBusinessLogic.SendEmailService.SendGeoMessageAsync(
                                "GPG - COMPANIES HOUSE ERROR",
                                $"Cant get SIC Codes from Companies House API for company {organisation.OrganisationName} No:{organisation.CompanyNumber} due to following error:\n\n{ex.Message}",
                                VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));
                            return View("CustomError", WebService.ErrorViewModelFactory.Create(1140));
                        }

                        CompaniesHouseFailures = 0;
                    }
                }

                model.SicCodeIds = organisation.SicCodeIds;
                model.SicSource = organisation.SicSource;
            }

            if (!string.IsNullOrWhiteSpace(model.SicCodeIds))
            {
                var codes = model.GetSicCodeIds();
                if (codes.Count > 0) model.SicCodes = codes.ToList();
            }

            if (organisation != null && organisation.SectorType == SectorTypes.Public ||
                organisation == null && model.SectorType == SectorTypes.Public)
            {
                if (model.SicCodes == null) model.SicCodes = new List<int>();

                if (!model.SicCodes.Any(s => s == 1)) model.SicCodes.Insert(0, 1);
            }

            #endregion

            StashModel(model);

            #region Populate the view model

            model = model.GetClone();
            if (organisation != null)
            {
                model.DUNSNumber = organisation.DUNSNumber;
                model.DateOfCessation = organisation.DateOfCessation;
                model.NameSource = organisation.NameSource;
                ViewBag.LastOrg = model.OrganisationName;
                model.OrganisationName = organisation.OrganisationName;
                model.SectorType = organisation.SectorType;
                model.CompanyNumber = organisation.CompanyNumber;
                model.CharityNumber = organisation.References.ContainsKey(nameof(model.CharityNumber))
                    ? organisation.References[nameof(model.CharityNumber)]
                    : null;
                model.MutualNumber = organisation.References.ContainsKey(nameof(model.MutualNumber))
                    ? organisation.References[nameof(model.MutualNumber)]
                    : null;
                model.OtherName = !string.IsNullOrWhiteSpace(model.OtherName) &&
                                  organisation.References.ContainsKey(model.OtherName)
                    ? model.OtherName
                    : null;
                model.OtherValue = !string.IsNullOrWhiteSpace(model.OtherName) &&
                                   organisation.References.ContainsKey(model.OtherName)
                    ? organisation.References[model.OtherName]
                    : null;
                if (!model.ManualAddress)
                {
                    model.AddressSource = organisation.AddressSource;
                    model.Address1 = organisation.Address1;
                    model.Address2 = organisation.Address2;
                    model.Address3 = organisation.Address3;
                    model.City = organisation.City;
                    model.County = organisation.County;
                    model.Country = organisation.Country;
                    model.Postcode = organisation.PostCode;
                    model.PoBox = organisation.PoBox;
                    if (organisation.IsUkAddress.HasValue)
                        model.IsUkAddress = organisation.IsUkAddress;
                    else
                        model.IsUkAddress = await _postcodeChecker.IsValidPostcode(organisation.PostCode)
                            ? true
                            : (bool?)null;
                }

                model.SicCodeIds = organisation.SicCodeIds;
                model.SicSource = organisation.SicSource;
            }

            #endregion

            return View(nameof(ConfirmOrganisation), model);
        }

        /// <summary>
        ///     On confirmation save the organisation
        /// </summary>
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("confirm-organisation")]
        public async Task<IActionResult> ConfirmOrganisation(OrganisationViewModel model, string command = null)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Cancel quick fasttrack
            if (command.EqualsI("CancelFasttrack"))
            {
                PendingFasttrackCodes = null;
                ClearStash();
                if (CurrentUser.UserOrganisations.Any())
                    return RedirectToActionArea("ManageOrganisations", "Submission", "Submission");

                return RedirectToAction(nameof(OrganisationType));
            }

            #region Load the organisations from session

            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.Organisations = m.Organisations;
            model.ManualOrganisations = m.ManualOrganisations;
            if (!command.EqualsI("confirm"))
            {
                m.AddressReturnAction = nameof(ConfirmOrganisation);
                m.WrongAddress = true;
                m.ManualRegistration = false;
                m.AddressSource = null;
                m.Address1 = null;
                m.Address2 = null;
                m.Address3 = null;
                m.City = null;
                m.County = null;
                m.Country = null;
                m.PoBox = null;
                m.Postcode = null;
                m.IsUkAddress = null;
                m.SectorType = model.SectorType;
                StashModel(m);
                return RedirectToAction("AddAddress");
            }

            #endregion

            //Save the registration
            UserOrganisation userOrg;
            try
            {
                userOrg = await SaveRegistrationAsync(VirtualUser, model);
            }
            catch (Exception ex)
            {
                //This line is to help diagnose object reference not found exception raised at this point 
                Logger.LogWarning(ex, Core.Extensions.Json.SerializeObjectDisposed(m));
                throw;
            }

            PendingFasttrackCodes = null;

            //Save the model state
            StashModel(model);

            //Select the organisation
            ReportingOrganisationId = userOrg.OrganisationId;

            //Remove any previous searches from the cache
            _registrationService.PrivateSectorRepository.ClearSearch();

            var authorised = false;
            var hasAddress = false;
            OrganisationRecord organisation = null;
            if (!model.ManualRegistration)
            {
                organisation = model.GetManualOrganisation();
                if (organisation != null)
                {
                    authorised = model.ManualAuthorised;
                    hasAddress = organisation.HasAnyAddress();
                }
                else
                {
                    organisation = model.GetSelectedOrganisation();
                    authorised = model.SelectedAuthorised;
                    if (organisation != null) hasAddress = organisation.HasAnyAddress();
                }
            }

            var sector = organisation == null ? model.SectorType : organisation.SectorType;

            //If manual registration then show confirm receipt
            if (model.ManualRegistration ||
                model.ManualAddress && (sector == SectorTypes.Private || !authorised || hasAddress))
            {
                var reviewCode = Encryption.EncryptQuerystring(
                    userOrg.UserId + ":" + userOrg.OrganisationId + ":" + VirtualDateTime.Now.ToSmallDateTime());

                if (VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
                    TempData["TestUrl"] = Url.ActionArea("ReviewRequest", "Admin", "Admin", new { code = reviewCode });

                return RedirectToAction("RequestReceived");
            }

            //If public sector or fasttracked then we are complete
            if (sector == SectorTypes.Public || model.IsFastTrackAuthorised)
            {
                //Log the registration
                if (!userOrg.User.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
                    await _registrationService.RegistrationLog.WriteAsync(
                        new RegisterLogModel
                        {
                            StatusDate = VirtualDateTime.Now,
                            Status = "Public sector email confirmed",
                            ActionBy = VirtualUser.EmailAddress,
                            Details = "",
                            Sector = userOrg.Organisation.SectorType,
                            Organisation = userOrg.Organisation.OrganisationName,
                            CompanyNo = userOrg.Organisation.CompanyNumber,
                            Address = userOrg?.Address.GetAddressString(),
                            SicCodes = userOrg.Organisation.GetLatestSicCodeIdsString(),
                            UserFirstname = userOrg.User.Firstname,
                            UserLastname = userOrg.User.Lastname,
                            UserJobtitle = userOrg.User.JobTitle,
                            UserEmail = userOrg.User.EmailAddress,
                            ContactFirstName = userOrg.User.ContactFirstName,
                            ContactLastName = userOrg.User.ContactLastName,
                            ContactJobTitle = userOrg.User.ContactJobTitle,
                            ContactOrganisation = userOrg.User.ContactOrganisation,
                            ContactPhoneNumber = userOrg.User.ContactPhoneNumber
                        });

                StashModel(
                    new CompleteViewModel
                    {
                        OrganisationId = userOrg.OrganisationId,
                        AccountingDate = _registrationService.SharedBusinessLogic.ReportingDeadlineHelper.GetReportingStartDate(sector.Value)
                    });

                //BUG: the return keyword was missing here so no redirection would occur
                return RedirectToAction("ServiceActivated");
            }


            //If private sector then send the pin
            if (model.IsUkAddress.HasValue && model.IsUkAddress.Value) return RedirectToAction("PINSent");

            return RedirectToAction("RequestReceived");
        }

        #endregion

        /// <summary>
        ///     Save the current users registration
        /// </summary>
        /// <param name="VirtualUser"></param>
        /// <param name="model"></param>
        private async Task<UserOrganisation> SaveRegistrationAsync(User VirtualUser, OrganisationViewModel model)
        {
            UserOrganisation userOrg = null;
            var authorised = false;
            var hasAddress = false;
            OrganisationRecord organisationRecord = null;
            var now = VirtualDateTime.Now;
            if (!model.ManualRegistration)
            {
                organisationRecord = model.GetManualOrganisation();

                if (organisationRecord != null)
                {
                    authorised = model.ManualAuthorised;
                    hasAddress = organisationRecord.HasAnyAddress();
                }
                else
                {
                    organisationRecord = model.GetSelectedOrganisation();
                    authorised = model.SelectedAuthorised;
                    if (organisationRecord != null) hasAddress = organisationRecord.HasAnyAddress();
                }
            }

            var org = organisationRecord == null || organisationRecord.OrganisationId == 0
                ? null
                : _registrationService.OrganisationBusinessLogic.DataRepository.Get<Organisation>(organisationRecord.OrganisationId);

            #region Create a new organisation

            var badSicCodes = new SortedSet<int>();
            if (org == null)
            {
                org = new Organisation();
                org.SectorType = organisationRecord == null ? model.SectorType.Value : organisationRecord.SectorType;
                org.CompanyNumber = organisationRecord == null ? model.CompanyNumber : organisationRecord.CompanyNumber;
                org.DateOfCessation = organisationRecord == null ? model.DateOfCessation : organisationRecord.DateOfCessation;
                org.Created = now;
                org.Modified = now;
                org.Status = OrganisationStatuses.New;

                if (organisationRecord == null)
                {
                    OrganisationReference reference;
                    //Add the charity number
                    if (!string.IsNullOrWhiteSpace(model.CharityNumber))
                    {
                        reference = new OrganisationReference
                        {
                            ReferenceName = nameof(model.CharityNumber),
                            ReferenceValue = model.CharityNumber,
                            Organisation = org
                        };
                        _registrationService.OrganisationBusinessLogic.DataRepository.Insert(reference);
                        org.OrganisationReferences.Add(reference);
                    }

                    //Add the mutual number
                    if (!string.IsNullOrWhiteSpace(model.MutualNumber))
                    {
                        reference = new OrganisationReference
                        {
                            ReferenceName = nameof(model.MutualNumber),
                            ReferenceValue = model.MutualNumber,
                            Organisation = org
                        };
                        _registrationService.OrganisationBusinessLogic.DataRepository.Insert(reference);
                        org.OrganisationReferences.Add(reference);
                    }

                    //Add the other reference 
                    if (!string.IsNullOrWhiteSpace(model.OtherName) && !string.IsNullOrWhiteSpace(model.OtherValue))
                    {
                        reference = new OrganisationReference
                        {
                            ReferenceName = model.OtherName,
                            ReferenceValue = model.OtherValue,
                            Organisation = org
                        };
                        _registrationService.OrganisationBusinessLogic.DataRepository.Insert(reference);
                        org.OrganisationReferences.Add(reference);
                    }
                }

                org.SetStatus(
                    authorised && !model.ManualRegistration
                        ? OrganisationStatuses.Active
                        : OrganisationStatuses.Pending,
                    OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId);

                _registrationService.OrganisationBusinessLogic.DataRepository.Insert(org);
            }

            #endregion

            #region Set Organisation name

            var oldName = org.OrganisationName;
            string newName = null;
            string newNameSource = null;
            if (model.ManualRegistration)
            {
                newName = model.OrganisationName;
                newNameSource = model.NameSource;
            }
            else if (organisationRecord != null)
            {
                newName = organisationRecord.OrganisationName;
                newNameSource = organisationRecord.NameSource;
            }

            if (string.IsNullOrWhiteSpace(newName))
                throw new Exception("Cannot save a registration with no organisation name");

            //Update the new organisation name
            if (string.IsNullOrWhiteSpace(oldName) || !newName.Equals(oldName))
            {
                var oldOrgName = org.GetLatestName();

                //Set the latest name if there isnt a name already or new name is from CoHo
                if (oldOrgName == null
                    || oldOrgName?.Name != newName
                    && _registrationService.SharedBusinessLogic.SourceComparer.IsCoHo(newNameSource)
                    && _registrationService.SharedBusinessLogic.SourceComparer.CanReplace(newNameSource,
                        oldOrgName.Source))
                {
                    org.OrganisationName = newName;

                    var orgName = new OrganisationName { Name = newName, Source = newNameSource };
                    _registrationService.OrganisationBusinessLogic.DataRepository.Insert(orgName);
                    org.OrganisationNames.Add(orgName);
                }
            }

            #endregion

            #region Set Organisation SIC codes

            var newSicCodeIds = new SortedSet<int>();
            string newSicSource = null;

            if (model.ManualRegistration)
            {
                newSicCodeIds = model.GetSicCodeIds();
                newSicSource = model.SicSource;
            }
            else if (organisationRecord != null)
            {
                newSicCodeIds = organisationRecord.GetSicCodes();
                newSicSource = organisationRecord.SicSource;
            }

            if (org.SectorType == SectorTypes.Public) newSicCodeIds.Add(1);

            //Remove invalid SicCodes
            if (newSicCodeIds.Any())
            {
                //TODO we should cache these SIC codes
                var allSicCodes = _registrationService.OrganisationBusinessLogic.DataRepository.GetAll<SicCode>().Select(s => s.SicCodeId)
                    .ToSortedSet();
                badSicCodes = newSicCodeIds.Except(allSicCodes).ToSortedSet();
                newSicCodeIds = newSicCodeIds.Except(badSicCodes).ToSortedSet();

                //Update the new and retire the old SIC codes
                if (newSicCodeIds.Count > 0)
                {
                    var oldSicCodes = org.GetLatestSicCodes();
                    var oldSicCodeIds = org.GetLatestSicCodeIds();
                    //Set the sic codes if there arent any sic codes already or new sic codes are from CoHo
                    if (!oldSicCodes.Any()
                        || !newSicCodeIds.SequenceEqual(oldSicCodeIds)
                        && _registrationService.SharedBusinessLogic.SourceComparer.IsCoHo(newSicSource)
                        && _registrationService.SharedBusinessLogic.SourceComparer.CanReplace(newSicSource,
                            oldSicCodes.Select(s => s.Source)))
                    {
                        //Retire the old SicCodes
                        foreach (var oldSicCode in oldSicCodes) oldSicCode.Retired = now;

                        foreach (var newSicCodeId in newSicCodeIds)
                        {
                            var sicCode = new OrganisationSicCode
                            { Organisation = org, SicCodeId = newSicCodeId, Source = newSicSource };
                            _registrationService.OrganisationBusinessLogic.DataRepository.Insert(sicCode);
                            org.OrganisationSicCodes.Add(sicCode);
                        }
                    }
                }
            }

            #endregion

            #region Set the organisation address

            var oldAddress = org.GetLatestAddress();
            var oldAddressModel = AddressModel.Create(oldAddress);

            AddressModel newAddressModel = null;
            var oldAddressSource = oldAddress?.Source;
            string newAddressSource = null;

            if (model.ManualRegistration || model.ManualAddress)
            {
                newAddressModel = model.GetAddressModel();
                newAddressSource = model.AddressSource;
            }
            else if (organisationRecord != null)
            {
                newAddressModel = organisationRecord;
                newAddressModel.IsUkAddress = model.IsUkAddress;
                newAddressSource = organisationRecord.AddressSource;
            }

            if (newAddressModel == null || newAddressModel.IsEmpty())
                throw new Exception("Cannot save a registration with no address");

            if (oldAddressModel == null || !oldAddressModel.Equals(newAddressModel))
            {
                var pendingAddress = newAddressModel.FindAddress(org, AddressStatuses.Pending);
                if (pendingAddress != null)
                {
                    oldAddress = pendingAddress;
                    oldAddressModel = AddressModel.Create(pendingAddress);
                    oldAddressSource = pendingAddress.Source;
                }
            }


            //Use the old address for this registration
            var address = oldAddress;

            //If the new address is different...
            if (oldAddressModel == null || oldAddressModel.IsEmpty() || !newAddressModel.Equals(oldAddressModel))
            {
                //Retire the old address
                oldAddress?.SetStatus(AddressStatuses.Retired,
                    OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId);

                //Create address received from user
                address = new OrganisationAddress();
                address.Organisation = org;
                address.CreatedByUserId = VirtualUser.UserId;
                address.Address1 = newAddressModel.Address1;
                address.Address2 = newAddressModel.Address2;
                address.Address3 = newAddressModel.Address3;
                address.TownCity = newAddressModel.City;
                address.County = newAddressModel.County;
                address.Country = newAddressModel.Country;
                address.PostCode = newAddressModel.PostCode;
                address.PoBox = newAddressModel.PoBox;
                address.IsUkAddress = newAddressModel.IsUkAddress;
                address.Source = newAddressSource;
                address.SetStatus(AddressStatuses.Pending,
                    OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId);
                _registrationService.OrganisationBusinessLogic.DataRepository.Insert(address);
            }

            //This line is to help diagnose object reference not found exception raised at this point 
            if (address == null)
                Logger.LogDebug("Address should not be null", Core.Extensions.Json.SerializeObjectDisposed(model));

            #endregion

            #region add the user org

            userOrg = org.OrganisationId == 0
                ? null
                : await _registrationService.OrganisationBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo =>
                    uo.OrganisationId == org.OrganisationId && uo.UserId == VirtualUser.UserId);

            if (userOrg == null)
            {
                userOrg = new UserOrganisation { User = VirtualUser, Organisation = org, Created = now };
                _registrationService.OrganisationBusinessLogic.DataRepository.Insert(userOrg);
            }

            //This line is to help diagnose object reference not found exception raised at this point 
            if (address == null)
                Logger.LogWarning("Address should not be null", Core.Extensions.Json.SerializeObjectDisposed(model));

            userOrg.Address = address;
            userOrg.PIN = null;
            userOrg.PINHash = null;
            userOrg.PINSentDate = null;

            #endregion

            #region Save the contact details

            var sendRequest = false;
            if (model.ManualRegistration
                || model.ManualAddress && (org.SectorType == SectorTypes.Private || !authorised || hasAddress)
                || !model.IsUkAddress.HasValue
                || !model.IsUkAddress.Value)
            {
                VirtualUser.ContactFirstName = model.ContactFirstName;
                VirtualUser.ContactLastName = model.ContactLastName;
                VirtualUser.ContactJobTitle = model.ContactJobTitle;
                VirtualUser.ContactEmailAddress = model.ContactEmailAddress;
                VirtualUser.ContactPhoneNumber = model.ContactPhoneNumber;
                userOrg.Method = RegistrationMethods.Manual;

                //Send request to GEO
                sendRequest = true;
            }

            #endregion

            #region Activate organisation and address if the user is authorised

            if (authorised && !model.ManualRegistration && (!model.ManualAddress || !hasAddress))
            {
                //Set the user org as confirmed
                userOrg.Method = model.IsFastTrackAuthorised
                    ? RegistrationMethods.Fasttrack
                    : RegistrationMethods.EmailDomain;
                userOrg.ConfirmAttempts = 0;
                userOrg.PINConfirmedDate = now;

                //Set the pending organisation to active
                userOrg.Organisation.SetStatus(
                    OrganisationStatuses.Active,
                    OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId,
                    userOrg.Method == RegistrationMethods.Fasttrack ? "Fasttrack" : "Email Domain");

                //Retire the old address 
                if (userOrg.Organisation.LatestAddress != null &&
                    userOrg.Organisation.LatestAddress.AddressId != userOrg.Address.AddressId)
                    userOrg.Organisation.LatestAddress.SetStatus(
                        AddressStatuses.Retired,
                        OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId,
                        "Replaced by PIN in post");

                //Activate the address the pin was sent to
                userOrg.Address.SetStatus(
                    AddressStatuses.Active,
                    OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId,
                    userOrg.Method == RegistrationMethods.Fasttrack ? "Fasttrack" : "Email Domain");
            }

            #endregion

            #region Save the changes to the database

            var saved = false;
            await _registrationService.OrganisationBusinessLogic.DataRepository.BeginTransactionAsync(
                async () =>
                {
                    try
                    {
                        await _registrationService.OrganisationBusinessLogic.SaveOrganisationAsync(org);

                        _registrationService.OrganisationBusinessLogic.DataRepository.CommitTransaction();
                        saved = true;
                    }
                    catch (Exception ex)
                    {
                        _registrationService.OrganisationBusinessLogic.DataRepository.RollbackTransaction();
                        sendRequest = false;
                        Logger.LogError(ex, JsonConvert.SerializeObject(model));
                        throw;
                    }
                });

            #endregion

            #region Update search indexes, log bad SIC codes and send registration request

            //Add or remove this organisation to/from the search index
            if (saved && !_registrationService.SearchBusinessLogic.SearchOptions.Disabled) await _registrationService.SearchBusinessLogic.RefreshSearchDocumentsAsync(userOrg.Organisation);

            //Log the bad sic codes here to ensure organisation identifiers have been created when saved
            if (badSicCodes.Count > 0) await _registrationService.OrganisationBusinessLogic.LogBadSicCodesAsync(org, badSicCodes);

            //Send request to GEO
            if (sendRequest)
            {
                if (model.ManualRegistration)
                    await SendGEORegistrationRequestAsync(
                        userOrg,
                        $"{model.ContactFirstName} {VirtualUser.ContactLastName} ({VirtualUser.JobTitle})",
                        org.OrganisationName,
                        address.GetAddressString(),
                        VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));
                else
                    await SendGEORegistrationRequestAsync(
                        userOrg,
                        $"{VirtualUser.Fullname} ({VirtualUser.JobTitle})",
                        org.OrganisationName,
                        address.GetAddressString(),
                        VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));
            }

            return userOrg;
        }

        //Send the registration request
        protected async Task SendGEORegistrationRequestAsync(UserOrganisation userOrg,
            string contactName,
            string reportingOrg,
            string reportingAddress,
            bool test = false)
        {
            //Send a verification link to the email address
            var reviewCode = userOrg.GetReviewCode();
            var reviewUrl = Url.ActionArea("ReviewRequest", "Admin", "Admin", new { code = reviewCode }, protocol: "https");

            //If the email address is a test email then simulate sending
            if (userOrg.User.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix)) return;

            await _registrationService.SharedBusinessLogic.SendEmailService.SendGEORegistrationRequestAsync(reviewUrl,
                contactName, reportingOrg, reportingAddress, test);
        }


        [Authorize]
        [HttpGet("request-received")]
        public async Task<IActionResult> RequestReceived()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Clear the stash
            ClearStash();

            if (VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix) &&
                TempData.ContainsKey("TestUrl")) ViewBag.TestUrl = TempData["TestUrl"];

            return View("RequestReceived");
        }

        #endregion
    }
}