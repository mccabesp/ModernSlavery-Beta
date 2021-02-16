using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Classes.UrlHelper;
using ModernSlavery.WebUI.Shared.Models;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    public partial class AdminController
    {
        private ActionResult UnwrapRegistrationRequest(ReviewOrganisationViewModel model, out UserOrganisation userOrg,
            bool ignoreDUNS)
        {
            userOrg = null;

            long userId = 0;
            long orgId = 0;
            try
            {
                var code = Encryption.Decrypt(model.ReviewCode, Encryption.Encodings.Base62);
                code = HttpUtility.UrlDecode(code);
                var args = code.SplitI(':');
                if (args.Length != 3) throw new ArgumentException("Too few parameters in registration review code");

                userId = args[0].ToLong();
                if (userId == 0) throw new ArgumentException("Invalid user id in registration review code");

                orgId = args[1].ToLong();
                if (orgId == 0) throw new ArgumentException("Invalid organisation id in registration review code");
            }
            catch
            {
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1114));
            }

            //Get the user oganisation
            userOrg = SharedBusinessLogic.DataRepository.GetAll<UserOrganisation>()
                .FirstOrDefault(uo => uo.UserId == userId && uo.OrganisationId == orgId);

            if (userOrg == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1115));

            //Check this registrations hasnt already completed
            if (userOrg.PINConfirmedDate != null)
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1145));

            switch (userOrg.Organisation.Status)
            {
                case OrganisationStatuses.Active:
                case OrganisationStatuses.Pending:
                    break;
                default:
                    throw new ArgumentException(
                        $"Invalid organisation status {userOrg.Organisation.Status} user {userId} and organisation {orgId} for reviewing registration request");
            }

            if (userOrg.Address == null)
                throw new Exception(
                    $"Cannot find address for user {userId} and organisation {orgId} for reviewing registration request");

            //Load view model
            model.ContactFirstName = userOrg.User.ContactFirstName;
            model.ContactLastName = userOrg.User.ContactLastName;
            if (string.IsNullOrWhiteSpace(model.ContactFirstName) && string.IsNullOrWhiteSpace(model.ContactFirstName))
            {
                model.ContactFirstName = userOrg.User.Firstname;
                model.ContactLastName = userOrg.User.Lastname;
            }

            model.ContactJobTitle = userOrg.User.ContactJobTitle.Coalesce(userOrg.User.JobTitle);
            model.ContactEmailAddress = userOrg.User.ContactEmailAddress.Coalesce(userOrg.User.EmailAddress);
            model.EmailAddress = userOrg.User.EmailAddress;
            model.ContactPhoneNumber = userOrg.User.ContactPhoneNumber;

            model.OrganisationName = userOrg.Organisation.OrganisationName;
            model.CompanyNumber = userOrg.Organisation.CompanyNumber;
            model.SectorType = userOrg.Organisation.SectorType;
            model.SicCodes = userOrg.Organisation.GetLatestSicCodeIds().ToList();

            // pre populate the DUNSNumber text box
            if (!ignoreDUNS) model.DUNSNumber = userOrg.Organisation.DUNSNumber;

            ViewBag.NoDUNS = string.IsNullOrWhiteSpace(userOrg.Organisation.DUNSNumber);

            model.Address1 = userOrg.Address.Address1;
            model.Address2 = userOrg.Address.Address2;
            model.Address3 = userOrg.Address.Address3;
            model.Country = userOrg.Address.Country;
            model.Postcode = userOrg.Address.PostCode;
            model.PoBox = userOrg.Address.PoBox;

            model.RegisteredAddress = userOrg.Address.Status == AddressStatuses.Pending
                ? userOrg.Organisation.LatestAddress?.GetAddressString()
                : null;

            model.CharityNumber = userOrg.Organisation.OrganisationReferences
                .Where(o => o.ReferenceName.ToLower() == nameof(ReviewOrganisationViewModel.CharityNumber).ToLower())
                .Select(or => or.ReferenceValue)
                .FirstOrDefault();

            model.MutualNumber = userOrg.Organisation.OrganisationReferences
                .Where(o => o.ReferenceName.ToLower() == nameof(ReviewOrganisationViewModel.MutualNumber).ToLower())
                .Select(or => or.ReferenceValue)
                .FirstOrDefault();

            model.OtherName = userOrg.Organisation.OrganisationReferences.ToList()
                .Where(
                    o => o.ReferenceName.ToLower() != nameof(ReviewOrganisationViewModel.CharityNumber).ToLower()
                         && o.ReferenceName.ToLower() != nameof(ReviewOrganisationViewModel.MutualNumber).ToLower())
                .Select(or => or.ReferenceName)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(model.OtherName))
                model.OtherValue = userOrg.Organisation.OrganisationReferences
                    .Where(o => o.ReferenceName == model.OtherName)
                    .Select(or => or.ReferenceValue)
                    .FirstOrDefault();

            return null;
        }

        #region ReviewRequest

        [HttpGet("review-request/{code}")]
        [IPAddressFilter]
        public async Task<IActionResult> ReviewRequest([IgnoreText] string code)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var model = new ReviewOrganisationViewModel();

            if (string.IsNullOrWhiteSpace(code))
            {
                //Load the organisation from session
                model = UnstashModel<ReviewOrganisationViewModel>();
                if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1114));
            }

            model.ReviewCode = code;

            //Unwrap code
            UserOrganisation userOrg;
            var result = UnwrapRegistrationRequest(model, out userOrg, false);
            if (result != null) return result;

            //Tell reviewer if this org has already been approved
            
            if (userOrg.Organisation.UserOrganisations.Any())
            {
                var firstRegistered = userOrg.Organisation.UserOrganisations.OrderByDescending(uo => uo.PINConfirmedDate).FirstOrDefault(uo => uo.PINConfirmedDate != null);

                if (firstRegistered != null)
                    AddModelError(
                        3017,
                        parameters: new
                        {
                            approvedUser = firstRegistered.User.EmailAddress,
                            approvedDate = firstRegistered.PINConfirmedDate.Value.ToShortDateString(),
                            approvedAddress = firstRegistered.Address?.GetAddressString()
                        });

                //Tell reviewer how many other open regitrations for same organisation
                var requestCount = await SharedBusinessLogic.DataRepository.GetAll<UserOrganisation>()
                    .CountAsync(
                        uo => uo.UserId != userOrg.UserId
                              && uo.OrganisationId == userOrg.OrganisationId
                              && uo.Organisation.Status == OrganisationStatuses.Pending);
                if (requestCount > 0) AddModelError(3018, parameters: new {requestCount});
            }

            //Get any conflicting or similar organisations
            IEnumerable<long> results;
            var orgIds = new HashSet<long>();

            if (!string.IsNullOrWhiteSpace(model.CompanyNumber))
            {
                results = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .Where(
                        o => o.OrganisationId != userOrg.OrganisationId
                             && o.SectorType == SectorTypes.Private
                             && o.CompanyNumber == model.CompanyNumber)
                    .Select(o => o.OrganisationId);
                if (results.Any()) orgIds.AddRange(results);
            }

            if (!string.IsNullOrWhiteSpace(model.CharityNumber))
            {
                results = SharedBusinessLogic.DataRepository.GetAll<OrganisationReference>()
                    .Where(
                        r => r.OrganisationId != userOrg.OrganisationId
                             && r.ReferenceName.ToLower() == "charity number"
                             && r.ReferenceValue.ToLower() == model.CharityNumber.ToLower())
                    .Select(r => r.OrganisationId);
                if (results.Any()) orgIds.AddRange(results);
            }

            if (!string.IsNullOrWhiteSpace(model.MutualNumber))
            {
                results = SharedBusinessLogic.DataRepository.GetAll<OrganisationReference>()
                    .Where(
                        r => r.OrganisationId != userOrg.OrganisationId
                             && r.ReferenceName.ToLower() == "mutual number"
                             && r.ReferenceValue.ToLower() == model.MutualNumber.ToLower())
                    .Select(r => r.OrganisationId);
                if (results.Any()) orgIds.AddRange(results);
            }

            if (!string.IsNullOrWhiteSpace(model.OtherName) && !string.IsNullOrWhiteSpace(model.OtherValue))
            {
                if (model.IsDUNS)
                {
                    results = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                        .Where(r => r.OrganisationId != userOrg.OrganisationId &&
                                    r.DUNSNumber.ToLower() == model.OtherValue.ToLower())
                        .Select(r => r.OrganisationId);
                    if (results.Any()) orgIds.AddRange(results);
                }

                results = SharedBusinessLogic.DataRepository.GetAll<OrganisationReference>()
                    .Where(
                        r => r.OrganisationId != userOrg.OrganisationId
                             && r.ReferenceName.ToLower() == model.OtherName.ToLower()
                             && r.ReferenceValue.ToLower() == model.OtherValue.ToLower())
                    .Select(r => r.OrganisationId);
                if (results.Any()) orgIds.AddRange(results);
            }

            model.MatchedReferenceCount = orgIds.Count;

            //Only show orgs matching names when none matching references
            if (model.MatchedReferenceCount == 0)
            {
                var orgName = model.OrganisationName.ToLower().ReplaceI("limited", "").ReplaceI("ltd", "");
                results = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .Where(o => o.OrganisationId != userOrg.OrganisationId &&
                                o.OrganisationName.ToLower().Contains(orgName))
                    .Select(o => o.OrganisationId);
                if (results.Any()) orgIds.AddRange(results);

                results = _adminService.OrganisationBusinessLogic.SearchOrganisations(
                        model.OrganisationName,
                        50 - results.Count())
                    .Select(o => o.OrganisationId);
                if (results.Any()) orgIds.AddRange(results);
            }

            if (orgIds.Any())
            {
                //Add the registrations
                var orgs = await SharedBusinessLogic.DataRepository.GetAll<Organisation>().Where(o => orgIds.Contains(o.OrganisationId)).ToListAsync();
                model.ManualOrganisations = _adminService.OrganisationBusinessLogic.CreateOrganisationRecords(orgs, true).ToList();
            }

            //Ensure exact match shown at top
            if (model.ManualOrganisations != null)
            {
                if (model.ManualOrganisations.Count > 1)
                {
                    var index = model.ManualOrganisations.FindIndex(e =>
                        e.OrganisationName.EqualsI(model.OrganisationName));
                    if (index > 0)
                    {
                        model.ManualOrganisations.Insert(0, model.ManualOrganisations[index]);
                        model.ManualOrganisations.RemoveAt(index + 1);
                    }
                }

                //Sort he organisations
                model.ManualOrganisations = model.ManualOrganisations.OrderBy(o => o.OrganisationName).ToList();
            }

            StashModel(model);
            return View("ReviewRequest", model);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [IPAddressFilter]
        [HttpPost("review-request/{code}")]
        public async Task<IActionResult> ReviewRequestPost([IgnoreText]string command)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load organisations from session
            var model = UnstashModel<ReviewOrganisationViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            //Unwrap code
            UserOrganisation userOrg;
            var result = UnwrapRegistrationRequest(model, out userOrg, true);
            if (result != null) return result;

            //Check model is valid
            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<ReviewOrganisationViewModel>();
                return View(nameof(ReviewRequest), model);
            }

            if (command.EqualsI("decline"))
            {
                result = RedirectToAction("ConfirmCancellation");
            }
            else if (command.EqualsI("approve"))
            {
                //Check for DUNS number conflicts
                Organisation conflictOrg = null;
                if (!string.IsNullOrWhiteSpace(model.DUNSNumber))
                {
                    conflictOrg = await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                        .FirstOrDefaultAsync(
                            o => userOrg.OrganisationId != o.OrganisationId &&
                                 o.DUNSNumber.ToLower() == model.DUNSNumber.ToLower());
                    if (conflictOrg != null) ModelState.AddModelError(3030, nameof(model.DUNSNumber));
                }

                //Check for company number conflicts
                if (!string.IsNullOrWhiteSpace(model.CompanyNumber))
                {
                    conflictOrg = await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                        .FirstOrDefaultAsync(
                            o => userOrg.OrganisationId != o.OrganisationId &&
                                 o.CompanyNumber.ToLower() == model.CompanyNumber.ToLower());
                    if (conflictOrg != null)
                        ModelState.AddModelError(
                            3031,
                            nameof(model.CompanyNumber),
                            new {organisationName = conflictOrg.OrganisationName, referenceName = "Company number"});
                }

                //Check for charity number conflicts
                if (!string.IsNullOrWhiteSpace(model.CharityNumber))
                {
                    var orgRef = await SharedBusinessLogic.DataRepository.GetAll<OrganisationReference>()
                        .FirstOrDefaultAsync(
                            o => userOrg.OrganisationId != o.OrganisationId
                                 && o.ReferenceName.ToLower() == nameof(model.CharityNumber).ToLower()
                                 && o.ReferenceValue.ToLower() == model.CharityNumber.ToLower());
                    conflictOrg = orgRef?.Organisation;
                    if (conflictOrg != null)
                        ModelState.AddModelError(
                            3031,
                            nameof(model.CharityNumber),
                            new {organisationName = conflictOrg.OrganisationName, referenceName = "Charity number"});
                }

                //Check for mutual number conflicts
                if (!string.IsNullOrWhiteSpace(model.MutualNumber))
                {
                    var orgRef = await SharedBusinessLogic.DataRepository.GetAll<OrganisationReference>()
                        .FirstOrDefaultAsync(
                            o => userOrg.OrganisationId != o.OrganisationId
                                 && o.ReferenceName.ToLower() == nameof(model.MutualNumber).ToLower()
                                 && o.ReferenceValue.ToLower() == model.MutualNumber.ToLower());
                    conflictOrg = orgRef?.Organisation;
                    if (conflictOrg != null)
                        ModelState.AddModelError(
                            3031,
                            nameof(model.MutualNumber),
                            new
                            {
                                organisationName = conflictOrg.OrganisationName,
                                referenceName = "Mutual partnership number"
                            });
                }

                //Check for other reference conflicts
                if (!string.IsNullOrWhiteSpace(model.OtherValue))
                {
                    var orgRef = await SharedBusinessLogic.DataRepository.GetAll<OrganisationReference>()
                        .FirstOrDefaultAsync(
                            o => userOrg.OrganisationId != o.OrganisationId
                                 && o.ReferenceName.ToLower() == model.OtherName.ToLower()
                                 && o.ReferenceValue.ToLower() == model.OtherValue.ToLower());
                    conflictOrg = orgRef?.Organisation;
                    if (conflictOrg != null)
                        ModelState.AddModelError(
                            3031,
                            nameof(model.OtherValue),
                            new {organisationName = conflictOrg.OrganisationName, referenceName = model.OtherValue});
                }

                if (!ModelState.IsValid)
                {
                    this.SetModelCustomErrors<ReviewOrganisationViewModel>();
                    return View("ReviewRequest", model);
                }

                //Activate the org user
                userOrg.PINConfirmedDate = VirtualDateTime.Now;

                //Activate the organisation
                userOrg.Organisation.SetStatus(
                    OrganisationStatuses.Active,
                    OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId,
                    "Manually registered");

                // Save the DUNS Number
                if (string.IsNullOrWhiteSpace(userOrg.Organisation.DUNSNumber) &&
                    !string.IsNullOrWhiteSpace(model.DUNSNumber)) userOrg.Organisation.DUNSNumber = model.DUNSNumber;

                //Delete the DUNS reference
                if (model.IsDUNS && userOrg.Organisation.DUNSNumber == model.OtherValue)
                {
                    var dunsRef =
                        userOrg.Organisation.OrganisationReferences.FirstOrDefault(r =>
                            r.ReferenceName == nameof(model.OtherName));
                    if (dunsRef != null) userOrg.Organisation.OrganisationReferences.Remove(dunsRef);
                }

                //Set the latest registration
                userOrg.Organisation.LatestRegistration = userOrg;

                // save any sic codes
                if (!string.IsNullOrEmpty(model.SicCodeIds))
                {
                    var newSicCodes = model.SicCodeIds.Split(',').Cast<int>().OrderBy(sc => sc);
                    foreach (var sc in newSicCodes)
                        userOrg.Organisation.OrganisationSicCodes.Add(
                            new OrganisationSicCode
                            {
                                SicCodeId = sc, OrganisationId = userOrg.OrganisationId, Created = VirtualDateTime.Now
                            });
                }

                //Retire the old address 
                if (userOrg.Organisation.LatestAddress != null &&
                    userOrg.Organisation.LatestAddress.AddressId != userOrg.Address.AddressId)
                    userOrg.Organisation.LatestAddress.SetStatus(
                        AddressStatuses.Retired,
                        OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId,
                        "Replaced by Manual registration");

                //Activate the address
                userOrg.Address.SetStatus(
                    AddressStatuses.Active,
                    OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId,
                    "Manually registered");
                userOrg.Organisation.LatestAddress = userOrg.Address;

                //Send the approved email to the applicant
                if (!await SendRegistrationAcceptedAsync(
                    userOrg.Organisation.OrganisationName,
                    userOrg.User.ContactEmailAddress.Coalesce(userOrg.User.EmailAddress)))
                {
                    ModelState.AddModelError(1132);
                    this.SetModelCustomErrors<ReviewOrganisationViewModel>();
                    return View("ReviewRequest", model);
                }

                //Log the approval
                await _adminService.RegistrationLog.WriteAsync(
                    new RegisterLogModel
                    {
                        StatusDate = VirtualDateTime.Now,
                        Status = "Manually registered",
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

                result = RedirectToAction("RequestAccepted");
            }
            else
            {
                return new HttpBadRequestResult($"Invalid command on '{command}'");
            }

            //Save the changes and redirect
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Ensure the organisation has an employer reference
            if (userOrg.PINConfirmedDate.HasValue && string.IsNullOrWhiteSpace(userOrg.Organisation.OrganisationReference))
            {
                await _adminService.OrganisationBusinessLogic.SetUniqueOrganisationReferenceAsync(userOrg.Organisation);
                await SharedBusinessLogic.DataRepository.SaveChangesAsync();
            }

            //Add or remove this organisation to/from the search index
            await _adminService.SearchBusinessLogic.RefreshSearchDocumentsAsync(userOrg.Organisation);

            //Save the model for the redirect
            StashModel(model);

            return result;
        }

        //Send the registration request
        protected async Task<bool> SendRegistrationAcceptedAsync(string organisationName, string emailAddress)
        {
            //Send an acceptance link to the email address
            var returnUrl = Url.ActionArea("ManageOrganisations", "Submission", "Submission", null, "https");
            return await _adminService.SharedBusinessLogic.SendEmailService.SendRegistrationApprovedAsync(organisationName, returnUrl, emailAddress);
        }

        /// <summary>
        ///     ask the reviewer for decline reason and confirmation ///
        /// </summary>
        /// <returns></returns>
        [HttpGet("confirm-cancellation")]
        [IPAddressFilter]
        public async Task<IActionResult> ConfirmCancellation()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load organisations from session
            var model = UnstashModel<ReviewOrganisationViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            return View("ConfirmCancellation", model);
        }

        /// <summary>
        ///     On confirmation save the organisation
        /// </summary>
        /// <returns></returns>
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("confirm-cancellation")]
        [IPAddressFilter]
        public async Task<IActionResult> ConfirmCancellation([IgnoreText] string command)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Load the organisations from session
            var model = UnstashModel<ReviewOrganisationViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            //If cancel button clicked the n return to review page
            if (command.EqualsI("Cancel")) return RedirectToAction("ReviewRequest", new { code = model.ReviewCode });

            //Unwrap code
            UserOrganisation userOrg;
            var result = UnwrapRegistrationRequest(model, out userOrg, true);
            if (result != null) return result;

            //Log the rejection
            await _adminService.RegistrationLog.WriteAsync(
                new RegisterLogModel
                {
                    StatusDate = VirtualDateTime.Now,
                    Status = "Manually Rejected",
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

            //Delete address for this user and organisation
            if (userOrg.Address.Status != AddressStatuses.Active && userOrg.Address.CreatedByUserId == userOrg.UserId)
                SharedBusinessLogic.DataRepository.Delete(userOrg.Address);

            //Delete the org user
            var orgId = userOrg.OrganisationId;
            var emailAddress = userOrg.User.ContactEmailAddress.Coalesce(userOrg.User.EmailAddress);

            //Delete the organisation if it has no statement, is not in D&B, is not in scopes table, and is not registered to another user
            if (userOrg.Organisation != null
                && !userOrg.Organisation.Statements.Any()
                && !userOrg.Organisation.OrganisationAddresses.Any(a => a.CreatedByUserId == -1 || a.Source == "Ext")
                && !userOrg.Organisation.OrganisationScopes.Any()
                && !await SharedBusinessLogic.DataRepository.GetAll<UserOrganisation>()
                    .AnyAsync(uo =>
                        uo.OrganisationId == userOrg.Organisation.OrganisationId && uo.UserId != userOrg.UserId))
            {
                Logger.LogInformation(
                    $"Unused organisation {userOrg.OrganisationId}:'{userOrg.Organisation.OrganisationName}'(DUNS:{userOrg.Organisation.DUNSNumber}) deleted by {(OriginalUser == null ? VirtualUser.EmailAddress : OriginalUser.EmailAddress)} when declining manual registration for {userOrg.User.EmailAddress}");
                SharedBusinessLogic.DataRepository.Delete(userOrg.Organisation);
            }

            var searchRecords = await _adminService.SearchBusinessLogic.ListSearchDocumentsAsync(userOrg.Organisation);
            SharedBusinessLogic.DataRepository.Delete(userOrg);

            //Send the declined email to the applicant
            if (!await SendRegistrationDeclinedAsync(
                userOrg.Organisation.OrganisationName,
                emailAddress,
                string.IsNullOrWhiteSpace(model.CancellationReason)
                    ? "We haven't been able to verify your organisation's identity. So we have declined your application."
                    : model.CancellationReason))
            {
                ModelState.AddModelError(1131);
                this.SetModelCustomErrors<ReviewOrganisationViewModel>();
                return View("ConfirmCancellation", model);
            }

            //Save the changes and redirect
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Remove this organisation from the search index
            await _adminService.SearchBusinessLogic.RemoveSearchDocumentsAsync(searchRecords);

            //Save the model for the redirect
            StashModel(model);

            //If private sector then send the pin
            return RedirectToAction("RequestCancelled");
        }


        //Send the registration request
        protected async Task<bool> SendRegistrationDeclinedAsync(string organisationName, string emailAddress, string reason)
        {
            //Send a verification link to the email address
            return await _adminService.SharedBusinessLogic.SendEmailService.SendRegistrationDeclinedAsync(organisationName, emailAddress,
                reason);
        }

        /// <summary>
        ///     Show review accepted confirmation
        ///     <returns></returns>
        [HttpGet("request-accepted")]
        [IPAddressFilter]
        public async Task<IActionResult> RequestAccepted()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load model from session
            var model = UnstashModel<ReviewOrganisationViewModel>(true);
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            return View("RequestAccepted", model);
        }

        /// <summary>
        ///     Show review cancel confirmation
        ///     <returns></returns>
        [HttpGet("request-cancelled")]
        [IPAddressFilter]
        public async Task<IActionResult> RequestCancelled()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Make sure we can load model from session
            var model = UnstashModel<ReviewOrganisationViewModel>(true);
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1112));

            return View("RequestCancelled", model);
        }

        #endregion
    }
}