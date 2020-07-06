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
using ModernSlavery.WebUI.Shared.Options;

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
            model.Employers = new PagedResult<EmployerRecord>();
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

            //Make sure we can load employers from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.Employers = m.Employers;

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
                this.CleanModelErrors<OrganisationViewModel>();
                return View("OrganisationType", model);
            }

            CompaniesHouseFailures = 0;

            StashModel(model);
            return RedirectToAction("OrganisationSearch");
        }

        #endregion

        #region Organisation Search

        /// Search employer
        [Authorize]
        [HttpGet("organisation-search")]
        public async Task<IActionResult> OrganisationSearch()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
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
        ///     Search employer submit
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
                this.CleanModelErrors<OrganisationViewModel>();
                return View("OrganisationSearch", model);
            }

            //Make sure we can load employers from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.Employers = m.Employers;
            model.ManualRegistration = true;
            model.BackAction = "OrganisationSearch";
            model.SelectedEmployerIndex = -1;
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
                        model.Employers = await _registrationService.PrivateSectorRepository.SearchAsync(
                            model.SearchText,
                            1,
                            SharedBusinessLogic.SharedOptions.EmployerPageSize,
                            VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, ex.Message);

                        CompaniesHouseFailures++;
                        if (CompaniesHouseFailures < 3)
                        {
                            model.Employers?.Results?.Clear();
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
                    model.Employers = await _registrationService.PublicSectorRepository.SearchAsync(
                        model.SearchText,
                        1,
                        SharedBusinessLogic.SharedOptions.EmployerPageSize,
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
            if (model.Employers.Results.Count < 1) return View("OrganisationSearch", model);

            //Go to step 5 with results
            if (Request.Query["fail"].ToBoolean())
                return RedirectToAction("ChooseOrganisation", "Registration", new { fail = true });

            return RedirectToAction("ChooseOrganisation");
        }

        #endregion

        #region Choose Organisation

        /// <summary>
        ///     Choose employer view results
        /// </summary>
        [Authorize]
        [HttpGet("choose-organisation")]
        public async Task<IActionResult> ChooseOrganisation()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load employers from session
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
        ///     Choose employer with paging or search
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

            //Make sure we can load employers from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.Employers = m.Employers;

            var nextPage = m.Employers.CurrentPage;

            model.SelectedEmployerIndex = -1;
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
                    this.CleanModelErrors<OrganisationViewModel>();
                    return View("ChooseOrganisation", model);
                }

                nextPage = 1;
                doSearch = true;
            }
            else if (command == "pageNext")
            {
                if (nextPage >= model.Employers.PageCount) throw new Exception("Cannot go past last page");

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
                if (page < 1 || page > model.Employers.PageCount) throw new Exception("Invalid page selected");

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
                            model.Employers = await _registrationService.PrivateSectorRepository.SearchAsync(
                                model.SearchText,
                                nextPage,
                                SharedBusinessLogic.SharedOptions.EmployerPageSize,
                                VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, ex.Message);

                            CompaniesHouseFailures++;
                            if (CompaniesHouseFailures < 3)
                            {
                                model.Employers?.Results?.Clear();
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
                        model.Employers = await _registrationService.PublicSectorRepository.SearchAsync(
                            model.SearchText,
                            nextPage,
                            SharedBusinessLogic.SharedOptions.EmployerPageSize,
                            VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));
                        break;

                    default:
                        throw new NotImplementedException();
                }

                ModelState.Clear();
                StashModel(model);

                //Go back if no results
                if (model.Employers.Results.Count < 1) return RedirectToAction("OrganisationSearch");

                //Otherwise show results
                return View("ChooseOrganisation", model);
            }

            if (command.StartsWithI("employer_"))
            {
                var employerIndex = command.AfterFirst("employer_").ToInt32();
                var employer = model.Employers.Results[employerIndex];

                //Ensure employers from companies house have a sector
                if (employer.SectorType == SectorTypes.Unknown) employer.SectorType = model.SectorType.Value;

                //Make sure user is fully registered for one private org before registering another 
                if (model.SectorType == SectorTypes.Private
                    && VirtualUser.UserOrganisations.Any()
                    && !VirtualUser.UserOrganisations.Any(uo => uo.PINConfirmedDate != null))
                {
                    AddModelError(3022);
                    this.CleanModelErrors<OrganisationViewModel>();
                    return View("ChooseOrganisation", model);
                }

                //Get the organisation from the database
                var org = employer.OrganisationId > 0
                    ? SharedBusinessLogic.DataRepository.Get<Organisation>(employer.OrganisationId)
                    : null;
                if (org == null && !string.IsNullOrWhiteSpace(employer.CompanyNumber))
                    org = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                        .FirstOrDefault(o => o.CompanyNumber != null && o.CompanyNumber == employer.CompanyNumber);

                if (org == null && !string.IsNullOrWhiteSpace(employer.EmployerReference))
                    org = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                        .FirstOrDefault(o => o.EmployerReference == employer.EmployerReference);

                if (org != null)
                {
                    //Make sure the found organisation is active or pending
                    if (org.Status != OrganisationStatuses.Active && org.Status != OrganisationStatuses.Pending)
                    {
                        Logger.LogWarning(
                            $"Attempt to register a {org.Status} organisation",
                            $"Organisation: '{org.OrganisationName}' Reference: '{org.EmployerReference}' User: '{VirtualUser.EmailAddress}'");
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
                        this.CleanModelErrors<OrganisationViewModel>();
                        return View("ChooseOrganisation", model);
                    }

                    //Ensure there isnt another pending registeration for this organisation
                    //userOrg = SharedBusinessLogic.DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.OrganisationId == org.OrganisationId && uo.UserId != VirtualUser.UserId && uo.PINSentDate!=null && uo.PINConfirmedDate==null);
                    //if (userOrg != null)
                    //{
                    //    var remainingTime = userOrg.PINSentDate.Value.AddDays(SharedBusinessLogic.SharedOptions.PinInPostExpiryDays) - VirtualDateTime.Now;
                    //    return View("CustomError", WebService.ErrorViewModelFactory.Create(1148, new { remainingTime = remainingTime.ToFriendly(maxParts: 2) }));
                    //}

                    employer.OrganisationId = org.OrganisationId;
                }

                model.SelectedEmployerIndex = employerIndex;


                //Make sure the organisation has an address
                if (employer.SectorType == SectorTypes.Public)
                {
                    //Get the email domains from the D&B file
                    if (string.IsNullOrWhiteSpace(employer.EmailDomains))
                    {
                        var allDnBOrgs = await _registrationService.OrganisationBusinessLogic.DnBOrgsRepository
                            .GetAllDnBOrgsAsync();
                        var dnbOrg = allDnBOrgs?.FirstOrDefault(o => o.EmployerReference == employer.EmployerReference);
                        if (dnbOrg != null) employer.EmailDomains = dnbOrg.EmailDomains;
                    }

                    model.ManualRegistration = false;
                    model.SelectedAuthorised = employer.IsAuthorised(VirtualUser.EmailAddress);
                    if (!model.SelectedAuthorised || !employer.HasAnyAddress())
                    {
                        model.ManualAddress = true;
                        model.AddressReturnAction = nameof(ChooseOrganisation);
                        StashModel(model);
                        return RedirectToAction("AddAddress");
                    }
                }
                else if (employer.SectorType == SectorTypes.Private && !employer.HasAnyAddress())
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
            if (model.SelectedEmployerIndex < 0) return View("ChooseOrganisation", model);

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

            //Make sure we can load employers from session
            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.Employers = m.Employers;

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
                this.CleanModelErrors<OrganisationViewModel>();
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
            model.ManualEmployerIndex = -1;
            model.NameSource = VirtualUser.EmailAddress;

            if (!orgIds.Any())
            {
                model.AddressReturnAction = nameof(AddOrganisation);
                StashModel(model);
                return RedirectToAction("AddAddress");
            }

            var employers =
                await SharedBusinessLogic.DataRepository.ToListAscendingAsync<Organisation, string>(
                    o => o.OrganisationName,
                    o => orgIds.Contains(o.OrganisationId));

            model.ManualEmployers = employers.Select(o => _registrationService.OrganisationBusinessLogic.CreateEmployerRecord(o)).ToList();

            //Ensure exact match shown at top
            if (model.ManualEmployers != null && model.ManualEmployers.Count > 1)
            {
                var index = model.ManualEmployers.FindIndex(e => e.OrganisationName.EqualsI(model.OrganisationName));
                if (index > 0)
                {
                    model.ManualEmployers.Insert(0, model.ManualEmployers[index]);
                    model.ManualEmployers.RemoveAt(index + 1);
                }
            }

            if (model.MatchedReferenceCount == 1)
            {
                model.ManualEmployerIndex = 0;
                StashModel(model);
                return await SelectOrganisation(VirtualUser, model, model.ManualEmployerIndex, nameof(AddOrganisation));
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

            //Make sure we can load employers from session
            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            if (command.EqualsI("Continue"))
            {
                //Ensure they select one of the matched references
                if (model.MatchedReferenceCount > 0) throw new ArgumentException(nameof(model.MatchedReferenceCount));

                model.ManualRegistration = true;
                model.ManualEmployerIndex = -1;
                model.AddressReturnAction = nameof(SelectOrganisation);
                StashModel(model);
                return RedirectToAction("AddAddress");
            }

            var employerIndex = command.AfterFirst("employer_").ToInt32();

            return await SelectOrganisation(VirtualUser, model, employerIndex, nameof(SelectOrganisation));
        }

        public async Task<IActionResult> SelectOrganisation(User VirtualUser,
            OrganisationViewModel model,
            int employerIndex,
            string returnAction)
        {
            if (employerIndex < 0) return new HttpBadRequestResult($"Invalid employer index {employerIndex}");

            model.ManualEmployerIndex = employerIndex;
            model.ManualAuthorised = false;

            var employer = model.GetManualEmployer();

            var org = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                .FirstOrDefault(o => o.OrganisationId == employer.OrganisationId);

            //Make sure the found organisation is active or pending
            if (org.Status != OrganisationStatuses.Active && org.Status != OrganisationStatuses.Pending)
            {
                Logger.LogWarning(
                    $"Attempt to register a {org.Status} organisation",
                    $"Organisation: '{org.OrganisationName}' Reference: '{org.EmployerReference}' User: '{VirtualUser.EmailAddress}'");
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1149));
            }

            if (org.SectorType == SectorTypes.Private)
                //Make sure they are fully registered for one before requesting another
                if (VirtualUser.UserOrganisations.Any() &&
                    !VirtualUser.UserOrganisations.Any(uo => uo.PINConfirmedDate != null))
                {
                    AddModelError(3022);
                    this.CleanModelErrors<OrganisationViewModel>();
                    return View(returnAction, model);
                }


            var userOrg = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo =>
                uo.OrganisationId == org.OrganisationId && uo.UserId == VirtualUser.UserId);
            if (userOrg != null)
            {
                AddModelError(userOrg.PINConfirmedDate == null ? 3021 : 3020);
                this.CleanModelErrors<OrganisationViewModel>();
                return View(returnAction, model);
            }

            //If the organisation already exists in DB then use its address and not that from CoHo
            //if (org.LatestAddress != null) employer.ActiveAddressId = org.LatestAddress.AddressId;

            //Make sure the organisation has an address
            if (employer.SectorType == SectorTypes.Public)
            {
                //Get the email domains from the D&B file
                if (string.IsNullOrWhiteSpace(employer.EmailDomains))
                {
                    var allDnBOrgs = await _registrationService.OrganisationBusinessLogic.DnBOrgsRepository
                        .GetAllDnBOrgsAsync();
                    var dnbOrg = allDnBOrgs?.FirstOrDefault(o => o.EmployerReference == employer.EmployerReference);
                    if (dnbOrg != null) employer.EmailDomains = dnbOrg.EmailDomains;
                }

                model.ManualAuthorised = employer.IsAuthorised(VirtualUser.EmailAddress);
                if (!model.ManualAuthorised || !employer.HasAnyAddress()) model.ManualAddress = true;
            }
            else if (employer.SectorType == SectorTypes.Private && !employer.HasAnyAddress())
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

            //Make sure we can load employers from session
            var model = UnstashModel<OrganisationViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            //Get the sic codes from companies house
            EmployerRecord employer = null;
            if (!model.ManualRegistration) employer = model.GetManualEmployer() ?? model.GetSelectedEmployer();

            #region Get the sic codes if there isnt any

            if (employer != null)
            {
                if (!model.ManualRegistration && string.IsNullOrWhiteSpace(employer.SicCodeIds))
                {
                    employer.SicSource = "CoHo";
                    if (VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
                    {
                        var sic = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<SicCode>(s =>
                            s.SicSectionId != "X");
                        employer.SicCodeIds = sic?.SicCodeId.ToString();
                    }
                    else
                    {
                        try
                        {
                            if (employer.SectorType == SectorTypes.Public)
                                employer.SicCodeIds =
                                    await _registrationService.PublicSectorRepository.GetSicCodesAsync(
                                        employer.CompanyNumber);
                            else
                                employer.SicCodeIds =
                                    await _registrationService.PrivateSectorRepository.GetSicCodesAsync(
                                        employer.CompanyNumber);

                            employer.SicSource = "CoHo";
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
                                $"Cant get SIC Codes from Companies House API for company {employer.OrganisationName} No:{employer.CompanyNumber} due to following error:\n\n{ex.Message}",
                                VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));
                            return View("CustomError", WebService.ErrorViewModelFactory.Create(1140));
                        }

                        CompaniesHouseFailures = 0;
                    }
                }

                model.SicCodeIds = employer.SicCodeIds;
                model.SicSource = employer.SicSource;
            }

            if (!string.IsNullOrWhiteSpace(model.SicCodeIds))
            {
                var codes = model.GetSicCodeIds();
                if (codes.Count > 0) model.SicCodes = codes.ToList();
            }

            if (employer != null && employer.SectorType == SectorTypes.Public ||
                employer == null && model.SectorType == SectorTypes.Public)
            {
                if (model.SicCodes == null) model.SicCodes = new List<int>();

                if (!model.SicCodes.Any(s => s == 1)) model.SicCodes.Insert(0, 1);
            }

            #endregion

            StashModel(model);

            #region Populate the view model

            model = model.GetClone();
            if (employer != null)
            {
                model.DUNSNumber = employer.DUNSNumber;
                model.DateOfCessation = employer.DateOfCessation;
                model.NameSource = employer.NameSource;
                ViewBag.LastOrg = model.OrganisationName;
                model.OrganisationName = employer.OrganisationName;
                model.SectorType = employer.SectorType;
                model.CompanyNumber = employer.CompanyNumber;
                model.CharityNumber = employer.References.ContainsKey(nameof(model.CharityNumber))
                    ? employer.References[nameof(model.CharityNumber)]
                    : null;
                model.MutualNumber = employer.References.ContainsKey(nameof(model.MutualNumber))
                    ? employer.References[nameof(model.MutualNumber)]
                    : null;
                model.OtherName = !string.IsNullOrWhiteSpace(model.OtherName) &&
                                  employer.References.ContainsKey(model.OtherName)
                    ? model.OtherName
                    : null;
                model.OtherValue = !string.IsNullOrWhiteSpace(model.OtherName) &&
                                   employer.References.ContainsKey(model.OtherName)
                    ? employer.References[model.OtherName]
                    : null;
                if (!model.ManualAddress)
                {
                    model.AddressSource = employer.AddressSource;
                    model.Address1 = employer.Address1;
                    model.Address2 = employer.Address2;
                    model.Address3 = employer.Address3;
                    model.City = employer.City;
                    model.County = employer.County;
                    model.Country = employer.Country;
                    model.Postcode = employer.PostCode;
                    model.PoBox = employer.PoBox;
                    if (employer.IsUkAddress.HasValue)
                        model.IsUkAddress = employer.IsUkAddress;
                    else
                        model.IsUkAddress = await _registrationService.PostcodeChecker.IsValidPostcode(employer.PostCode)
                            ? true
                            : (bool?)null;
                }

                model.SicCodeIds = employer.SicCodeIds;
                model.SicSource = employer.SicSource;
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

            #region Load the employers from session

            var m = UnstashModel<OrganisationViewModel>();
            if (m == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            model.Employers = m.Employers;
            model.ManualEmployers = m.ManualEmployers;
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
            EmployerRecord employer = null;
            if (!model.ManualRegistration)
            {
                employer = model.GetManualEmployer();
                if (employer != null)
                {
                    authorised = model.ManualAuthorised;
                    hasAddress = employer.HasAnyAddress();
                }
                else
                {
                    employer = model.GetSelectedEmployer();
                    authorised = model.SelectedAuthorised;
                    if (employer != null) hasAddress = employer.HasAnyAddress();
                }
            }

            var sector = employer == null ? model.SectorType : employer.SectorType;

            //If manual registration then show confirm receipt
            if (model.ManualRegistration ||
                model.ManualAddress && (sector == SectorTypes.Private || !authorised || hasAddress))
            {
                var reviewCode = Encryption.EncryptQuerystring(
                    userOrg.UserId + ":" + userOrg.OrganisationId + ":" + VirtualDateTime.Now.ToSmallDateTime());

                if (VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
                    TempData["TestUrl"] = Url.ActionArea("ReviewRequest", "Admin","Admin",new {code = reviewCode});

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

                if (model.IsFastTrackAuthorised)
                    //Send notification email to existing users
                    _registrationService.SharedBusinessLogic.NotificationService.SendUserAddedEmailToExistingUsers(
                        userOrg.Organisation, userOrg.User);

                StashModel(
                    new CompleteViewModel
                    {
                        OrganisationId = userOrg.OrganisationId,
                        AccountingDate = _registrationService.SharedBusinessLogic.GetAccountingStartDate(sector.Value)
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
            EmployerRecord employer = null;
            var now = VirtualDateTime.Now;
            if (!model.ManualRegistration)
            {
                employer = model.GetManualEmployer();

                if (employer != null)
                {
                    authorised = model.ManualAuthorised;
                    hasAddress = employer.HasAnyAddress();
                }
                else
                {
                    employer = model.GetSelectedEmployer();
                    authorised = model.SelectedAuthorised;
                    if (employer != null) hasAddress = employer.HasAnyAddress();
                }
            }

            var org = employer == null || employer.OrganisationId == 0
                ? null
                : SharedBusinessLogic.DataRepository.Get<Organisation>(employer.OrganisationId);

            #region Create a new organisation

            var badSicCodes = new SortedSet<int>();
            if (org == null)
            {
                org = new Organisation();
                org.SectorType = employer == null ? model.SectorType.Value : employer.SectorType;
                org.CompanyNumber = employer == null ? model.CompanyNumber : employer.CompanyNumber;
                org.DateOfCessation = employer == null ? model.DateOfCessation : employer.DateOfCessation;
                org.Created = now;
                org.Modified = now;
                org.Status = OrganisationStatuses.New;

                //Create a presumed in-scope for current year
                var newScope = new OrganisationScope
                {
                    Organisation = org,
                    ScopeStatus = ScopeStatuses.PresumedInScope,
                    ScopeStatusDate = now,
                    Status = ScopeRowStatuses.Active,
                    StatusDetails = "Generated by the system",
                    SnapshotDate = _registrationService.SharedBusinessLogic.GetAccountingStartDate(org.SectorType)
                };
                SharedBusinessLogic.DataRepository.Insert(newScope);
                org.OrganisationScopes.Add(newScope);

                //Create a presumed out-of-scope for previous year
                var oldScope = new OrganisationScope
                {
                    Organisation = org,
                    ScopeStatus = ScopeStatuses.PresumedOutOfScope,
                    ScopeStatusDate = now,
                    Status = ScopeRowStatuses.Active,
                    StatusDetails = "Generated by the system",
                    SnapshotDate = newScope.SnapshotDate.AddYears(-1)
                };
                SharedBusinessLogic.DataRepository.Insert(oldScope);
                org.OrganisationScopes.Add(oldScope);

                if (employer == null)
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
                        SharedBusinessLogic.DataRepository.Insert(reference);
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
                        SharedBusinessLogic.DataRepository.Insert(reference);
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
                        SharedBusinessLogic.DataRepository.Insert(reference);
                        org.OrganisationReferences.Add(reference);
                    }
                }

                org.SetStatus(
                    authorised && !model.ManualRegistration
                        ? OrganisationStatuses.Active
                        : OrganisationStatuses.Pending,
                    OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId);
                SharedBusinessLogic.DataRepository.Insert(org);
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
            else if (employer != null)
            {
                newName = employer.OrganisationName;
                newNameSource = employer.NameSource;
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
                    SharedBusinessLogic.DataRepository.Insert(orgName);
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
            else if (employer != null)
            {
                newSicCodeIds = employer.GetSicCodes();
                newSicSource = employer.SicSource;
            }

            if (org.SectorType == SectorTypes.Public) newSicCodeIds.Add(1);

            //Remove invalid SicCodes
            if (newSicCodeIds.Any())
            {
                //TODO we should cache these SIC codes
                var allSicCodes = SharedBusinessLogic.DataRepository.GetAll<SicCode>().Select(s => s.SicCodeId)
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
                            SharedBusinessLogic.DataRepository.Insert(sicCode);
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
            else if (employer != null)
            {
                newAddressModel = employer.GetAddressModel();
                newAddressModel.IsUkAddress = model.IsUkAddress;
                newAddressSource = employer.AddressSource;
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
                SharedBusinessLogic.DataRepository.Insert(address);
            }

            //This line is to help diagnose object reference not found exception raised at this point 
            if (address == null)
                Logger.LogDebug("Address should not be null", Core.Extensions.Json.SerializeObjectDisposed(model));

            #endregion

            #region add the user org

            userOrg = org.OrganisationId == 0
                ? null
                : await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo =>
                    uo.OrganisationId == org.OrganisationId && uo.UserId == VirtualUser.UserId);

            if (userOrg == null)
            {
                userOrg = new UserOrganisation { User = VirtualUser, Organisation = org, Created = now };
                SharedBusinessLogic.DataRepository.Insert(userOrg);
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
            var tempUserOrg = userOrg; // Need to use a temporary UserOrg inside a lambda expression for out parameters
            await SharedBusinessLogic.DataRepository.BeginTransactionAsync(
                async () =>
                {
                    try
                    {
                        await SharedBusinessLogic.DataRepository.SaveChangesAsync();

                        _registrationService.ScopeBusinessLogic.FillMissingScopes(tempUserOrg.Organisation);

                        if (tempUserOrg.PINConfirmedDate != null)
                            tempUserOrg.Organisation.LatestRegistration = tempUserOrg;

                        await SharedBusinessLogic.DataRepository.SaveChangesAsync();

                        if (tempUserOrg.Address.Status == AddressStatuses.Active)
                            tempUserOrg.Organisation.LatestAddress = tempUserOrg.Address;

                        await SharedBusinessLogic.DataRepository.SaveChangesAsync();

                        //Ensure the organisation has an employer reference
                        if (string.IsNullOrWhiteSpace(tempUserOrg.Organisation.EmployerReference))
                            await _registrationService.OrganisationBusinessLogic.SetUniqueEmployerReferenceAsync(
                                tempUserOrg.Organisation);

                        SharedBusinessLogic.DataRepository.CommitTransaction();
                        saved = true;
                    }
                    catch (Exception ex)
                    {
                        SharedBusinessLogic.DataRepository.RollbackTransaction();
                        sendRequest = false;
                        Logger.LogWarning(ex, Core.Extensions.Json.SerializeObjectDisposed(model));
                        throw;
                    }
                });
            userOrg = tempUserOrg; // Need to return temporary UserOrg inside a lambda expression back to out parameters

            #endregion

            #region Update search indexes, log bad SIC codes and send registration request

            //Add or remove this organisation to/from the search index
            if (saved) await _registrationService.SearchBusinessLogic.UpdateSearchIndexAsync(userOrg.Organisation);

            //Log the bad sic codes here to ensure organisation identifiers have been created when saved
            if (badSicCodes.Count > 0)
            {
                //Create the logging tasks
                var badSicLoggingtasks = new List<Task>();
                badSicCodes.ForEach(
                    code => badSicLoggingtasks.Add(
                        _registrationService.BadSicLog.WriteAsync(
                            new BadSicLogModel
                            {
                                OrganisationId = org.OrganisationId,
                                OrganisationName = org.OrganisationName,
                                SicCode = code,
                                Source = "CoHo"
                            })));

                //Wait for all the logging tasks to complete
                await Task.WhenAll(badSicLoggingtasks);
            }

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
            var reviewUrl = Url.ActionArea("ReviewRequest", "Admin", "Admin", new {code = reviewCode});

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