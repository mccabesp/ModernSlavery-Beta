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
using ModernSlavery.WebUI.Shared.Classes.UrlHelper;
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
        public async Task<IActionResult> OrganisationType(bool? isFastTrack, SectorTypes sectorType)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load organisations from session
            var organisationViewModel = UnstashModel<OrganisationViewModel>();
            if (organisationViewModel == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            if (VirtualUser.UserOrganisations.Any())
                organisationViewModel.BackAction = Url.ActionArea("ManageOrganisations", "Submission", "Submission");

            ModelState.Clear();

            if (!isFastTrack.HasValue)
            {
                // either it is fast track, or sectortype mut be set
                AddModelError(3034, nameof(isFastTrack));
                return View("OrganisationType", organisationViewModel);
            }
            else
            {
                organisationViewModel.IsFastTrack = isFastTrack.Value;
                if (isFastTrack.Value)
                {
                    StashModel(organisationViewModel);
                    return RedirectToAction(nameof(FastTrack));
                }
                else if (sectorType == SectorTypes.Unknown)
                {
                    // either it is fast track, or sectortype mut be set
                    AddModelError(3005, nameof(sectorType));
                    return View("OrganisationType", organisationViewModel);
                }
            }

            CompaniesHouseFailures = 0;
            organisationViewModel.SectorType = sectorType;
            StashModel(organisationViewModel);
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

            model.IsManualRegistration = true;
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

            var searchViewModel = new OrganisationSearchViewModel
            {
                SearchText = model.SearchText,
                Organisations = model.Organisations,
                SectorType=model.SectorType,
                LastPrivateSearchRemoteTotal=model.LastPrivateSearchRemoteTotal
            };
            return View("OrganisationSearch", searchViewModel);
        }

        /// <summary>
        ///     Search organisation submit
        /// </summary>
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("organisation-search")]
        public async Task<IActionResult> OrganisationSearch(OrganisationSearchViewModel searchViewModel)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<OrganisationSearchViewModel>();
                return View("OrganisationSearch", searchViewModel);
            }

            //Make sure we can load organisations from session
            var organisationViewModel = UnstashModel<OrganisationViewModel>();
            if (organisationViewModel == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));
            organisationViewModel.SearchText = searchViewModel.SearchText.TrimI();
            organisationViewModel.IsManualRegistration = true;
            organisationViewModel.BackAction = "OrganisationSearch";
            organisationViewModel.SelectedOrganisationIndex = -1;
            organisationViewModel.OrganisationName = null;
            organisationViewModel.CompanyNumber = null;
            organisationViewModel.Address1 = null;
            organisationViewModel.Address2 = null;
            organisationViewModel.Address3 = null;
            organisationViewModel.Country = null;
            organisationViewModel.Postcode = null;
            organisationViewModel.PoBox = null;

            switch (organisationViewModel.SectorType)
            {
                case SectorTypes.Private:
                    try
                    {
                        organisationViewModel.Organisations = await _registrationService.PrivateSectorRepository.SearchAsync(organisationViewModel.SearchText, 1, SharedBusinessLogic.SharedOptions.OrganisationPageSize);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, ex.Message);

                        CompaniesHouseFailures++;
                        if (CompaniesHouseFailures < 3)
                        {
                            organisationViewModel.Organisations?.Results?.Clear();
                            StashModel(organisationViewModel);

                            searchViewModel.Organisations = organisationViewModel.Organisations;
                            searchViewModel.SectorType = organisationViewModel.SectorType;
                            searchViewModel.LastPrivateSearchRemoteTotal = organisationViewModel.LastPrivateSearchRemoteTotal;

                            ModelState.AddModelError(1141);
                            return View(searchViewModel);
                        }

                        await _registrationService.SharedBusinessLogic.SendEmailService.SendMsuMessageAsync("GPG - COMPANIES HOUSE ERROR", $"Cant search using Companies House API for query '{organisationViewModel.SearchText}' page:'1' due to following error:\n\n{ex.GetDetailsText()}");
                        return View("CustomError", WebService.ErrorViewModelFactory.Create(1140));
                    }

                    break;
                case SectorTypes.Public:
                    organisationViewModel.Organisations = await _registrationService.PublicSectorRepository.SearchAsync(organisationViewModel.SearchText, 1, SharedBusinessLogic.SharedOptions.OrganisationPageSize);

                    break;

                default:
                    throw new NotImplementedException();
            }

            ModelState.Clear();
            organisationViewModel.LastPrivateSearchRemoteTotal = LastPrivateSearchRemoteTotal;
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

            StashModel(organisationViewModel);

            searchViewModel.Organisations = organisationViewModel.Organisations;
            searchViewModel.SectorType = organisationViewModel.SectorType;
            searchViewModel.LastPrivateSearchRemoteTotal = organisationViewModel.LastPrivateSearchRemoteTotal;

            //Search again if no results
            if (organisationViewModel.Organisations.Results.Count < 1) return View("OrganisationSearch", searchViewModel);

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
            model.IsManualRegistration = false;
            model.AddressReturnAction = null;
            model.ConfirmReturnAction = null;
            model.IsManualAuthorised = false;
            model.IsManualAddress = false;
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

            var searchViewModel = new OrganisationSearchViewModel
            {
                SearchText = model.SearchText,
                Organisations = model.Organisations,
                SectorType = model.SectorType,
                LastPrivateSearchRemoteTotal = model.LastPrivateSearchRemoteTotal
            };

            return View("ChooseOrganisation", searchViewModel);
        }


        /// <summary>
        ///     Choose organisation with paging or search
        /// </summary>
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("choose-organisation")]
        public async Task<IActionResult> ChooseOrganisation(OrganisationSearchViewModel searchViewModel, [IgnoreText] string command)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load organisations from session
            var organisationViewModel = UnstashModel<OrganisationViewModel>();
            if (organisationViewModel == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            //Restore the lost settings
            searchViewModel.Organisations = organisationViewModel.Organisations;
            searchViewModel.SectorType = organisationViewModel.SectorType;
            searchViewModel.LastPrivateSearchRemoteTotal = organisationViewModel.LastPrivateSearchRemoteTotal;

            var nextPage = organisationViewModel.Organisations.CurrentPage;

            organisationViewModel.SelectedOrganisationIndex = -1;
            organisationViewModel.OrganisationName = null;
            organisationViewModel.CompanyNumber = null;
            organisationViewModel.Address1 = null;
            organisationViewModel.Address2 = null;
            organisationViewModel.Address3 = null;
            organisationViewModel.Country = null;
            organisationViewModel.Postcode = null;
            organisationViewModel.PoBox = null;
            organisationViewModel.IsManualAuthorised = false;
            organisationViewModel.IsManualAddress = false;

            var doSearch = false;
            ModelState.Include("SearchText");
            if (command == "search")
            {
                organisationViewModel.SearchText = searchViewModel.SearchText.TrimI();

                if (!ModelState.IsValid)
                {
                    this.SetModelCustomErrors<OrganisationSearchViewModel>();
                    return View("ChooseOrganisation", searchViewModel);
                }

                nextPage = 1;
                doSearch = true;
            }
            else if (command == "pageNext")
            {
                if (nextPage >= organisationViewModel.Organisations.ActualPageCount) throw new Exception("Cannot go past last page");

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
                if (page < 1 || page > organisationViewModel.Organisations.ActualPageCount) throw new Exception("Invalid page selected");

                if (page != nextPage)
                {
                    nextPage = page;
                    doSearch = true;
                }
            }

            if (doSearch)
            {
                switch (organisationViewModel.SectorType)
                {
                    case SectorTypes.Private:
                        try
                        {
                            organisationViewModel.Organisations = await _registrationService.PrivateSectorRepository.SearchAsync(organisationViewModel.SearchText, nextPage, SharedBusinessLogic.SharedOptions.OrganisationPageSize);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, ex.Message);

                            CompaniesHouseFailures++;
                            if (CompaniesHouseFailures < 3)
                            {
                                organisationViewModel.Organisations?.Results?.Clear();
                                StashModel(organisationViewModel);
                                ModelState.AddModelError(1141);
                                searchViewModel.Organisations = organisationViewModel.Organisations;
                                searchViewModel.SectorType = organisationViewModel.SectorType;
                                searchViewModel.LastPrivateSearchRemoteTotal = organisationViewModel.LastPrivateSearchRemoteTotal;
                                return View("ChooseOrganisation", searchViewModel);
                            }

                            await _registrationService.SharedBusinessLogic.SendEmailService.SendMsuMessageAsync("GPG - COMPANIES HOUSE ERROR", $"Cant search using Companies House API for query '{organisationViewModel.SearchText}' page:'1' due to following error:\n\n{ex.GetDetailsText()}");
                            return View("CustomError", WebService.ErrorViewModelFactory.Create(1140));
                        }

                        organisationViewModel.LastPrivateSearchRemoteTotal = LastPrivateSearchRemoteTotal;
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
                        organisationViewModel.Organisations = await _registrationService.PublicSectorRepository.SearchAsync(organisationViewModel.SearchText, nextPage, SharedBusinessLogic.SharedOptions.OrganisationPageSize);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                ModelState.Clear();
                StashModel(organisationViewModel);

                //Go back if no results
                if (organisationViewModel.Organisations.Results.Count < 1) return RedirectToAction("OrganisationSearch");

                //Otherwise show results
                //Restore the lost settings
                searchViewModel.Organisations = organisationViewModel.Organisations;
                searchViewModel.SectorType = organisationViewModel.SectorType;
                searchViewModel.LastPrivateSearchRemoteTotal = organisationViewModel.LastPrivateSearchRemoteTotal;
                return View("ChooseOrganisation", searchViewModel);
            }

            if (command.StartsWithI("organisation_"))
            {
                var organisationIndex = command.AfterFirst("organisation_").ToInt32();
                var organisation = organisationViewModel.Organisations.Results[organisationIndex];

                //Ensure organisations from companies house have a sector
                if (organisation.SectorType == SectorTypes.Unknown) organisation.SectorType = organisationViewModel.SectorType.Value;

                //Make sure user is fully registered for one private org before registering another 
                if (searchViewModel.SectorType == SectorTypes.Private
                    && VirtualUser.UserOrganisations.Any()
                    && !VirtualUser.UserOrganisations.Any(uo => uo.PINConfirmedDate != null))
                {
                    AddModelError(3022);
                    this.SetModelCustomErrors<OrganisationSearchViewModel>();
                    searchViewModel.Organisations = organisationViewModel.Organisations;
                    searchViewModel.SectorType = organisationViewModel.SectorType;
                    searchViewModel.LastPrivateSearchRemoteTotal = organisationViewModel.LastPrivateSearchRemoteTotal;
                    return View("ChooseOrganisation", searchViewModel);
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
                    if (org.SectorType != organisationViewModel.SectorType)
                        return View("CustomError",
                            WebService.ErrorViewModelFactory.Create(organisationViewModel.SectorType == SectorTypes.Private
                                ? 1146
                                : 1147));

                    //Ensure user is not already registered for this organisation
                    var userOrg = SharedBusinessLogic.DataRepository.GetAll<UserOrganisation>()
                        .FirstOrDefault(
                            uo => uo.OrganisationId == org.OrganisationId && uo.UserId == VirtualUser.UserId);
                    if (userOrg != null)
                    {
                        AddModelError(userOrg.PINConfirmedDate == null ? 3021 : 3020);
                        this.SetModelCustomErrors<OrganisationSearchViewModel>();
                        searchViewModel.Organisations = organisationViewModel.Organisations;
                        searchViewModel.SectorType = organisationViewModel.SectorType;
                        searchViewModel.LastPrivateSearchRemoteTotal = organisationViewModel.LastPrivateSearchRemoteTotal;
                        return View("ChooseOrganisation", searchViewModel);
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

                organisationViewModel.SelectedOrganisationIndex = organisationIndex;


                //Make sure the organisation has an address
                if (organisation.SectorType == SectorTypes.Public)
                {
                    organisationViewModel.IsManualRegistration = false;
                        organisationViewModel.IsManualAddress = true;
                        organisationViewModel.AddressReturnAction = nameof(ChooseOrganisation);
                        StashModel(organisationViewModel);
                        return RedirectToAction("AddAddress");
                }
                else if (organisation.SectorType == SectorTypes.Private && !organisation.HasAnyAddress())
                {
                    organisationViewModel.AddressReturnAction = nameof(ChooseOrganisation);
                    organisationViewModel.IsManualRegistration = false;
                    organisationViewModel.IsManualAddress = true;
                    StashModel(organisationViewModel);
                    return RedirectToAction("AddAddress");
                }

                organisationViewModel.IsManualRegistration = false;
                organisationViewModel.IsManualAddress = false;
                organisationViewModel.AddressReturnAction = null;
            }

            ModelState.Clear();

            //If we havend selected one the reshow same view
            if (organisationViewModel.SelectedOrganisationIndex < 0) 
            {
                searchViewModel.Organisations = organisationViewModel.Organisations;
                searchViewModel.SectorType = organisationViewModel.SectorType;
                searchViewModel.LastPrivateSearchRemoteTotal = organisationViewModel.LastPrivateSearchRemoteTotal;
                return View("ChooseOrganisation", searchViewModel); 
            }

            organisationViewModel.ConfirmReturnAction = nameof(ChooseOrganisation);
            StashModel(organisationViewModel);
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

            model.IsManualRegistration = true;
            model.IsManualAuthorised = false;
            model.IsManualAddress = false;
            StashModel(model);

            var addOrganisationViewModel = new AddOrganisationViewModel
            {
                OrganisationName = model.OrganisationName,
                CompanyNumber=model.CompanyNumber,
                CharityNumber=model.CharityNumber,
                MutualNumber=model.MutualNumber,
                NoReference=model.NoReference,
                OtherName=model.OtherName,
                OtherValue=model.OtherValue,
            };
            return View("AddOrganisation", addOrganisationViewModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("add-organisation")]
        public async Task<IActionResult> AddOrganisation(AddOrganisationViewModel addOrganisationViewModel)
        {
            //Ensure user has completed the registration process

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Exclude the address details
            var excludes = new HashSet<string>();

            if (addOrganisationViewModel.NoReference)
            {
                excludes.AddRange(
                    nameof(addOrganisationViewModel.CompanyNumber),
                    nameof(addOrganisationViewModel.CharityNumber),
                    nameof(addOrganisationViewModel.MutualNumber),
                    nameof(addOrganisationViewModel.OtherName),
                    nameof(addOrganisationViewModel.OtherValue));
                if (addOrganisationViewModel.ContainsReference)
                    ModelState.AddModelError("", "You must clear all your reference fields");
            }
            else if (addOrganisationViewModel.ContainsReference)
            {
                if (string.IsNullOrWhiteSpace(addOrganisationViewModel.CompanyNumber)) excludes.Add(nameof(addOrganisationViewModel.CompanyNumber));

                if (string.IsNullOrWhiteSpace(addOrganisationViewModel.CharityNumber)) excludes.Add(nameof(addOrganisationViewModel.CharityNumber));

                if (string.IsNullOrWhiteSpace(addOrganisationViewModel.MutualNumber)) excludes.Add(nameof(addOrganisationViewModel.MutualNumber));

                if (string.IsNullOrWhiteSpace(addOrganisationViewModel.OtherName))
                {
                    if (addOrganisationViewModel.OtherName.ReplaceI(" ").EqualsI("CompanyNumber", "CompanyNo", "CompanyReference", "CompanyRef"))
                        ModelState.AddModelError(nameof(addOrganisationViewModel.OtherName),"Cannot user Company Number as an Other reference");
                    else if (addOrganisationViewModel.OtherName.ReplaceI(" ").EqualsI("CharityNumber", "CharityNo", "CharityReference", "CharityRef"))
                        ModelState.AddModelError(nameof(addOrganisationViewModel.OtherName),"Cannot user Charity Number as an Other reference");
                    else if (addOrganisationViewModel.OtherName.ReplaceI(" ").EqualsI("MutualNumber","MutualNo","MutualReference","MutualRef","MutualPartnsershipNumber","MutualPartnsershipNo","MutualPartnsershipReference","MutualPartnsershipRef"))
                        ModelState.AddModelError(nameof(addOrganisationViewModel.OtherName),"Cannot user Mutual Partnership Number as an Other reference");

                    if (string.IsNullOrWhiteSpace(addOrganisationViewModel.OtherValue))
                    {
                        excludes.Add(nameof(addOrganisationViewModel.OtherName));
                        excludes.Add(nameof(addOrganisationViewModel.OtherValue));
                    }
                }
            }

            //Check model is valid
            ModelState.Exclude(excludes.ToArray());
            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<AddOrganisationViewModel>();
                return View("AddOrganisation", addOrganisationViewModel);
            }

            //Make sure we can load organisations from session
            var organisationViewModel = UnstashModel<OrganisationViewModel>();
            if (organisationViewModel == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            organisationViewModel.OrganisationName = addOrganisationViewModel.OrganisationName;
            organisationViewModel.CompanyNumber = addOrganisationViewModel.CompanyNumber;
            organisationViewModel.CharityNumber = addOrganisationViewModel.CharityNumber;
            organisationViewModel.MutualNumber = addOrganisationViewModel.MutualNumber;
            organisationViewModel.NoReference = addOrganisationViewModel.NoReference;
            organisationViewModel.OtherName = addOrganisationViewModel.OtherName;
            organisationViewModel.OtherValue = addOrganisationViewModel.OtherValue;

            //Check the company doesnt already exist
            IEnumerable<long> results;
            var orgIds = new HashSet<long>();
            var orgIdrefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!organisationViewModel.NoReference)
            {
                if (!string.IsNullOrWhiteSpace(organisationViewModel.CompanyNumber))
                {
                    results = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                        .Where(o => o.CompanyNumber == organisationViewModel.CompanyNumber)
                        .Select(o => o.OrganisationId);
                    if (results.Any())
                    {
                        orgIdrefs.Add(nameof(organisationViewModel.CompanyNumber));
                        orgIds.AddRange(results);
                    }
                }

                if (!string.IsNullOrWhiteSpace(organisationViewModel.CharityNumber))
                {
                    results = SharedBusinessLogic.DataRepository.GetAll<OrganisationReference>()
                        .Where(
                            r => r.ReferenceName.ToLower() == nameof(OrganisationViewModel.CharityNumber).ToLower()
                                 && r.ReferenceValue.ToLower() == organisationViewModel.CharityNumber.ToLower())
                        .Select(r => r.OrganisationId);
                    if (results.Any())
                    {
                        orgIdrefs.Add(nameof(organisationViewModel.CharityNumber));
                        orgIds.AddRange(results);
                    }
                }

                if (!string.IsNullOrWhiteSpace(organisationViewModel.MutualNumber))
                {
                    results = SharedBusinessLogic.DataRepository.GetAll<OrganisationReference>()
                        .Where(
                            r => r.ReferenceName.ToLower() == nameof(OrganisationViewModel.MutualNumber).ToLower()
                                 && r.ReferenceValue.ToLower() == organisationViewModel.MutualNumber.ToLower())
                        .Select(r => r.OrganisationId);
                    if (results.Any())
                    {
                        orgIdrefs.Add(nameof(organisationViewModel.MutualNumber));
                        orgIds.AddRange(results);
                    }
                }

                if (!string.IsNullOrWhiteSpace(organisationViewModel.OtherName) && !string.IsNullOrWhiteSpace(organisationViewModel.OtherValue))
                {
                    if (organisationViewModel.IsDUNS)
                    {
                        results = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                            .Where(r => r.DUNSNumber.ToLower() == organisationViewModel.OtherValue.ToLower())
                            .Select(r => r.OrganisationId);
                        if (results.Any())
                        {
                            orgIdrefs.Add(nameof(organisationViewModel.OtherName));
                            orgIds.AddRange(results);
                        }
                    }

                    results = SharedBusinessLogic.DataRepository.GetAll<OrganisationReference>()
                        .Where(
                            r => r.ReferenceName.ToLower() == organisationViewModel.OtherName.ToLower()
                                 && r.ReferenceValue.ToLower() == organisationViewModel.OtherValue.ToLower())
                        .Select(r => r.OrganisationId);
                    if (results.Any())
                    {
                        orgIdrefs.Add(nameof(organisationViewModel.OtherName));
                        orgIds.AddRange(results);
                    }
                }
            }

            organisationViewModel.MatchedReferenceCount = orgIds.Count;

            //Only show orgs matching names when none matching references
            if (organisationViewModel.MatchedReferenceCount == 0)
            {
                var orgName = organisationViewModel.OrganisationName.ToLower().ReplaceI("limited", "").ReplaceI("ltd", "");
                results = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .Where(o => o.OrganisationName.Contains(orgName))
                    .Select(o => o.OrganisationId);
                if (results.Any()) orgIds.AddRange(results);


                results = _registrationService.OrganisationBusinessLogic.SearchOrganisations(organisationViewModel.OrganisationName, 49)
                    .Select(o => o.OrganisationId);
                if (results.Any()) orgIds.AddRange(results);
            }

            organisationViewModel.IsManualRegistration = true;
            organisationViewModel.ManualOrganisationIndex = -1;
            organisationViewModel.NameSource = VirtualUser.EmailAddress;

            if (!orgIds.Any())
            {
                organisationViewModel.AddressReturnAction = nameof(AddOrganisation);
                StashModel(organisationViewModel);
                return RedirectToAction("AddAddress");
            }

            var organisations =
                await SharedBusinessLogic.DataRepository.ToListAscendingAsync<Organisation, string>(
                    o => o.OrganisationName,
                    o => orgIds.Contains(o.OrganisationId));

            organisationViewModel.ManualOrganisations = _registrationService.OrganisationBusinessLogic.CreateOrganisationRecords(organisations,false).ToList();

            //Ensure exact match shown at top
            if (organisationViewModel.ManualOrganisations != null && organisationViewModel.ManualOrganisations.Count > 1)
            {
                var index = organisationViewModel.ManualOrganisations.FindIndex(e => e.OrganisationName.EqualsI(organisationViewModel.OrganisationName));
                if (index > 0)
                {
                    organisationViewModel.ManualOrganisations.Insert(0, organisationViewModel.ManualOrganisations[index]);
                    organisationViewModel.ManualOrganisations.RemoveAt(index + 1);
                }
            }

            if (organisationViewModel.MatchedReferenceCount == 1)
            {
                organisationViewModel.ManualOrganisationIndex = 0;
                StashModel(organisationViewModel);
                return await SelectOrganisation(VirtualUser, organisationViewModel, organisationViewModel.ManualOrganisationIndex, nameof(AddOrganisation), addOrganisationViewModel);
            }

            StashModel(organisationViewModel);
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

            model.IsManualAuthorised = false;
            model.IsManualRegistration = true;
            model.IsManualAddress = false;
            StashModel(model);
            return View("SelectOrganisation", model);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("select-organisation")]
        public async Task<IActionResult> SelectOrganisation([IgnoreText]string command)
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

                model.IsManualRegistration = true;
                model.ManualOrganisationIndex = -1;
                model.AddressReturnAction = nameof(SelectOrganisation);
                StashModel(model);
                return RedirectToAction("AddAddress");
            }

            var organisationIndex = command.AfterFirst("organisation_").ToInt32();

            return await SelectOrganisation(VirtualUser, model, organisationIndex, nameof(SelectOrganisation), model);
        }

        [NonAction]
        protected async Task<IActionResult> SelectOrganisation(User VirtualUser,
            OrganisationViewModel model,
            int organisationIndex,
            string returnAction, object originalModel)
        {
            if (organisationIndex < 0) return new HttpBadRequestResult($"Invalid organisation index {organisationIndex}");

            model.ManualOrganisationIndex = organisationIndex;
            model.IsManualAuthorised = false;

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
                    return View(returnAction, originalModel);
                }


            var userOrg = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo =>
                uo.OrganisationId == org.OrganisationId && uo.UserId == VirtualUser.UserId);
            if (userOrg != null)
            {
                AddModelError(userOrg.PINConfirmedDate == null ? 3021 : 3020);
                return View(returnAction, originalModel);
            }

            //If the organisation already exists in DB then use its address and not that from CoHo
            //if (org.LatestAddress != null) organisation.ActiveAddressId = org.LatestAddress.AddressId;

            //Make sure the organisation has an address
            if (organisation.SectorType == SectorTypes.Public)
            {
                if (!organisation.HasAnyAddress()) model.IsManualAddress = true;
            }
            else if (organisation.SectorType == SectorTypes.Private && !organisation.HasAnyAddress())
            {
                model.IsManualAddress = true;
            }

            model.IsManualRegistration = false;
            if (model.IsManualAddress)
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
            if (!model.IsManualRegistration) organisation = model.GetManualOrganisation() ?? model.GetSelectedOrganisation();

            #region Get the sic codes if there isnt any

            if (organisation != null)
            {
                if (!model.IsManualRegistration && string.IsNullOrWhiteSpace(organisation.SicCodeIds))
                {
                    organisation.SicSource = "CoHo";
                    if (SharedBusinessLogic.TestOptions.LoadTesting)
                    {
                        var sic = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<SicCode>(s => s.SicSectionId != "X");
                        organisation.SicCodeIds = sic?.SicCodeId.ToString();
                    }
                    else if (organisation.OrganisationId > 0)
                    {
                        var org = await SharedBusinessLogic.DataRepository.GetAsync<Organisation>(organisation.OrganisationId);
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

                            await _registrationService.SharedBusinessLogic.SendEmailService.SendMsuMessageAsync("GPG - COMPANIES HOUSE ERROR", $"Cant get SIC Codes from Companies House API for company {organisation.OrganisationName} No:{organisation.CompanyNumber} due to following error:\n\n{ex.Message}");
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
                if (!model.IsManualAddress)
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
                    else if (!string.IsNullOrWhiteSpace(organisation.PostCode))
                        model.IsUkAddress = await _postcodeChecker.CheckPostcodeAsync(organisation.PostCode)
                            ? true
                            : (bool?)null;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(model.Postcode))
                        model.IsUkAddress = await _postcodeChecker.CheckPostcodeAsync(model.Postcode)
                            ? true
                            : (bool?)null;
                }


                model.SicCodeIds = organisation.SicCodeIds;
                model.SicSource = organisation.SicSource;
            }

            #endregion
            StashModel(model);

            return View(nameof(ConfirmOrganisation), model);
        }

        /// <summary>
        ///     On confirmation save the organisation
        /// </summary>
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("confirm-organisation")]
        public async Task<IActionResult> ConfirmOrganisation(ConfirmOrganisationViewModel confirmOrganisationViewModel, [IgnoreText]string command = null)
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

            var organisationViewModel = UnstashModel<OrganisationViewModel>();
            if (organisationViewModel == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            if(!organisationViewModel.IsUkAddress.HasValue && !organisationViewModel.IsManualRegistration){
                organisationViewModel.IsUkAddress = confirmOrganisationViewModel.IsUkAddress;
            }

            if (!command.EqualsI("confirm"))
            {
                organisationViewModel.AddressReturnAction = nameof(ConfirmOrganisation);
                organisationViewModel.IsWrongAddress = true;
                organisationViewModel.IsManualRegistration = false;
                organisationViewModel.AddressSource = null;
                organisationViewModel.Address1 = null;
                organisationViewModel.Address2 = null;
                organisationViewModel.Address3 = null;
                organisationViewModel.City = null;
                organisationViewModel.County = null;
                organisationViewModel.Country = null;
                organisationViewModel.PoBox = null;
                organisationViewModel.Postcode = null;
                organisationViewModel.IsUkAddress = null;
                organisationViewModel.SectorType = organisationViewModel.SectorType;
                StashModel(organisationViewModel);
                return RedirectToAction("AddAddress");
            }

            #endregion

            //Save the registration
            UserOrganisation userOrg;
            try
            {
                userOrg = await SaveRegistrationAsync(VirtualUser, organisationViewModel);
            }
            catch (Exception ex)
            {
                //This line is to help diagnose object reference not found exception raised at this point 
                Logger.LogError(ex, Core.Extensions.Json.SerializeObject(organisationViewModel));
                throw;
            }

            //Save the organisation identifier
            TempData["NewOrganisationIdentifier"] = SharedBusinessLogic.Obfuscator.Obfuscate(userOrg.OrganisationId);
            TempData["NewUserIdentifier"] = SharedBusinessLogic.Obfuscator.Obfuscate(userOrg.UserId);

            PendingFasttrackCodes = null;

            //Save the model state
            StashModel(organisationViewModel);

            //Select the organisation
            ReportingOrganisationId = userOrg.OrganisationId;

            //Remove any previous searches from the cache
            _registrationService.PrivateSectorRepository.ClearSearch();

            var authorised = false;
            var hasAddress = false;
            OrganisationRecord organisation = null;
            if (!organisationViewModel.IsManualRegistration)
            {
                organisation = organisationViewModel.GetManualOrganisation();
                if (organisation != null)
                {
                    authorised = organisationViewModel.IsManualAuthorised;
                    hasAddress = organisation.HasAnyAddress();
                }
                else
                {
                    organisation = organisationViewModel.GetSelectedOrganisation();
                    authorised = organisationViewModel.IsFastTrackAuthorised;
                    if (organisation != null) hasAddress = organisation.HasAnyAddress();
                }
            }

            var sector = organisation == null ? organisationViewModel.SectorType : organisation.SectorType;

            //If manual registration then show confirm receipt
            if (organisationViewModel.IsManualRegistration ||
                organisationViewModel.IsManualAddress && (sector == SectorTypes.Private || !authorised || hasAddress))
            {
                var reviewCode = Encryption.Encrypt($"{userOrg.UserId}:{userOrg.OrganisationId}:{VirtualDateTime.Now.ToSmallDateTime()}", Encryption.Encodings.Base62);

                return RedirectToAction("RequestReceived");
            }

            //If public sector or fasttracked then we are complete
            if (sector == SectorTypes.Public || organisationViewModel.IsFastTrackAuthorised)
            {
                //Log the registration
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
                        OrganisationName = userOrg.Organisation.OrganisationName,
                        AccountingDate = _registrationService.SharedBusinessLogic.ReportingDeadlineHelper.GetReportingStartDate(sector.Value)
                    });

                //BUG: the return keyword was missing here so no redirection would occur
                return RedirectToAction("ServiceActivated");
            }

            //If private sector then send the pin
            if (organisationViewModel.IsUkAddress.HasValue && organisationViewModel.IsUkAddress.Value)
            {
                var id = SharedBusinessLogic.Obfuscator.Obfuscate(userOrg.OrganisationId);
                return RedirectToAction("PINSent", new { id });
            }

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
            if (!model.IsManualRegistration)
            {
                organisationRecord = model.GetManualOrganisation();

                if (organisationRecord != null)
                {
                    authorised = model.IsManualAuthorised;
                    hasAddress = organisationRecord.HasAnyAddress();
                }
                else
                {
                    organisationRecord = model.GetSelectedOrganisation();
                    authorised = model.IsFastTrackAuthorised;
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
                    authorised && !model.IsManualRegistration
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
            if (model.IsManualRegistration)
            {
                newName = model.OrganisationName;
                newNameSource = model.NameSource;
            }
            else if (organisationRecord != null)
            {
                newName = organisationRecord.OrganisationName;
                newNameSource = organisationRecord.NameSource;
            }

            newName=newName?.Trim().Left(100);

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

            if (model.IsManualRegistration)
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

            if (model.IsManualRegistration || model.IsManualAddress)
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
                address.Trim();
                address.IsUkAddress = newAddressModel.IsUkAddress;
                address.Source = newAddressSource;
                address.SetStatus(AddressStatuses.Pending,OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId);
                _registrationService.OrganisationBusinessLogic.DataRepository.Insert(address);
            }

            //This line is to help diagnose object reference not found exception raised at this point 
            if (address == null)
                Logger.LogDebug("Address should not be null", Core.Extensions.Json.SerializeObject(model));

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
                Logger.LogWarning("Address should not be null", Core.Extensions.Json.SerializeObject(model));

            userOrg.Address = address;
            userOrg.PIN = null;
            userOrg.PINHash = null;
            userOrg.PINSentDate = null;

            #endregion

            #region Save the contact details

            var sendRequest = false;
            if (model.IsManualRegistration
                || model.IsManualAddress && (org.SectorType == SectorTypes.Private || !authorised || hasAddress)
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

            if (authorised && !model.IsManualRegistration && (!model.IsManualAddress || !hasAddress))
            {
                //Set the user org as confirmed
                userOrg.Method = RegistrationMethods.Fasttrack;
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
            await SharedBusinessLogic.DataRepository.ExecuteTransactionAsync(
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
                        Logger.LogError(ex, Core.Extensions.Json.SerializeObject(model));
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
                if (model.IsManualRegistration)
                    await SendMsuRegistrationRequestAsync(
                        userOrg,
                        $"{model.ContactFirstName} {VirtualUser.ContactLastName} ({VirtualUser.JobTitle})",
                        org.OrganisationName,
                        address.GetAddressString());
                else
                    await SendMsuRegistrationRequestAsync(
                        userOrg,
                        $"{VirtualUser.Fullname} ({VirtualUser.JobTitle})",
                        org.OrganisationName,
                        address.GetAddressString());
            }

            return userOrg;
        }

        //Send the registration request
        protected async Task SendMsuRegistrationRequestAsync(UserOrganisation userOrg,
            string contactName,
            string reportingOrg,
            string reportingAddress)
        {
            //Send a verification link to the email address
            var reviewCode = userOrg.GetReviewCode();
            var reviewUrl = Url.ActionArea("ReviewRequest", "Admin", "Admin", new { code = reviewCode }, protocol: "https");

            await _registrationService.SharedBusinessLogic.SendEmailService.SendMsuRegistrationRequestAsync(reviewUrl, contactName, reportingOrg, reportingAddress);
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

            return View("RequestReceived");
        }

        #endregion
    }
}