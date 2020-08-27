using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using Newtonsoft.Json;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    public partial class AdminController
    {
        private async Task<long> UpdateOrganisationSearchIndexesAsync(string parameters, string comment, StringWriter writer,
            bool test)
        {
            if (!string.IsNullOrWhiteSpace(parameters)) throw new ArgumentException("ERROR: parameters must be empty");

            var count = await _adminService.OrganisationSearchRepository.GetDocumentCountAsync();
            if (!test)
            {
                await _adminService.ExecuteWebjobQueue.AddMessageAsync(
                    new QueueWrapper($"command=UpdateOrganisationSearch&userEmail={CurrentUser.EmailAddress}&comment={comment}"));
                writer.WriteLine(
                    $"An email will be sent to '{CurrentUser.EmailAddress}' when the background task '{nameof(UpdateOrganisationSearchIndexesAsync)}' has completed");
            }

            return count;
        }

        private async Task<long> UpdateDownloadFilesAsync(string parameters, string comment, StringWriter writer,
            bool test)
        {
            if (!string.IsNullOrWhiteSpace(parameters)) throw new ArgumentException("ERROR: parameters must be empty");

            var count = await _adminService.OrganisationSearchRepository.GetDocumentCountAsync();
            if (!test)
            {
                await _adminService.ExecuteWebjobQueue.AddMessageAsync(
                    new QueueWrapper(
                        $"command=UpdateDownloadFiles&userEmail={CurrentUser.EmailAddress}&comment={comment}"));
                writer.WriteLine(
                    $"An email will be sent to '{CurrentUser.EmailAddress}' when the background task '{nameof(UpdateDownloadFilesAsync)}' has completed");
            }

            return count;
        }

        private async Task<long> FixOrganisationsNamesAsync(string parameters, string comment, StringWriter writer,
            bool test)
        {
            if (!string.IsNullOrWhiteSpace(parameters)) throw new ArgumentException("ERROR: parameters must be empty");

            var count = await SharedBusinessLogic.DataRepository.GetAll<OrganisationName>()
                .Where(o => o.Name.ToLower().Contains(" ltd"))
                .Select(o => o.OrganisationId)
                .Distinct()
                .CountAsync();
            if (!test)
            {
                await _adminService.ExecuteWebjobQueue.AddMessageAsync(
                    new QueueWrapper(
                        $"command=FixOrganisationsNames&userEmail={CurrentUser.EmailAddress}&comment={comment}"));
                writer.WriteLine(
                    $"An email will be sent to '{CurrentUser.EmailAddress}' when the background task '{nameof(FixOrganisationsNamesAsync)}' has completed");
            }

            return count;
        }

        public class BulkResult
        {
            public int Count { get; set; }
            public int TotalRecords { get; set; }
        }

        #region Manual Changes

        [HttpGet("manual-changes")]
        public IActionResult ManualChanges()
        {
            //Throw error if the user is not a super administrator
            if (!IsDatabaseAdministrator)
                return new HttpUnauthorizedResult($"User {CurrentUser?.EmailAddress} is not a database administrator");

            return View(new ManualChangesViewModel());
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("manual-changes")]
        public async Task<IActionResult> ManualChanges(ManualChangesViewModel model)
        {
            //Throw error if the user is not a super administrator
            if (!IsDatabaseAdministrator)
                return new HttpUnauthorizedResult($"User {CurrentUser?.EmailAddress} is not a database administrator");

            model.Results = null;
            ModelState.Clear();
            var test = model.LastTestedCommand != model.Command
                       || model.LastTestedInput != model.Parameters.ReplaceI(Environment.NewLine, ";");
            model.Tested = false;
            BulkResult result = null;
            long count = 0;
            int? total = null;

            model.SuccessMessage = null;
            using (var writer = new StringWriter())
            {
                try
                {
                    switch (model.Command)
                    {
                        case "Please select..":
                            throw new ArgumentException("ERROR: You must first select a command");
                        case "Fix organisation names":
                            count = await FixOrganisationsNamesAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Add organisations latest name":
                            count = await SetOrganisationNameAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Reset organisation to only original name":
                            count = await ResetOrganisationNameAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Convert public to private":
                            count = await ConvertPublicOrganisationsToPrivateAsync(model.Parameters, model.Comment,
                                writer, test);
                            break;
                        case "Convert private to public":
                            count = await ConvertPrivateOrganisationsToPublicAsync(model.Parameters, model.Comment,
                                writer, test);
                            break;
                        case "Retire organisations":
                            count = await RetireOrganisationsAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Unretire organisations":
                            count = await UnRetireOrganisationsAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Delete submissions":
                            count = await DeleteSubmissionsAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Set organisation company number":
                            count = await SetOrganisationCompanyNumberAsync(model.Parameters, model.Comment, writer,
                                test);
                            break;
                        case "Set organisation DUNS number":
                            count = await SetOrganisationDUNSNumberAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Set organisation SIC codes":
                            count = await SetOrganisationSicCodesAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Set organisation addresses":
                            count = await SetOrganisationAddressesAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Set public sector type":
                            count = await SetPublicSectorTypeAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Set organisation as out of scope":
                            count = await SetOrganisationScopeAsync(
                                model.Parameters,
                                model.Comment,
                                writer,
                                ScopeStatuses.OutOfScope,
                                test);
                            break;
                        case "Set organisation as in scope":
                            count = await SetOrganisationScopeAsync(model.Parameters, model.Comment, writer,
                                ScopeStatuses.InScope, test);
                            break;
                        case "Update search indexes":
                            count = await UpdateOrganisationSearchIndexesAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Update GPG download data files":
                            count = await UpdateDownloadFilesAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Fix database errors":
                            count = await FixDatabaseErrorsAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Create security code":
                            count = await SecurityCodeWorkAsync(
                                model.Parameters,
                                model.Comment,
                                writer,
                                test,
                                ManualActions.Create,
                                _adminService.OrganisationBusinessLogic.CreateOrganisationSecurityCodeAsync);
                            break;
                        case "Create security codes for all active and pending orgs":
                            result = await SecurityCodeBulkWorkAsync(
                                model.Parameters,
                                model.Comment,
                                writer,
                                test,
                                ManualActions.Create,
                                _adminService.OrganisationBusinessLogic.CreateOrganisationSecurityCodesInBulkAsync);
                            count = result.Count;
                            total = result.TotalRecords;
                            break;
                        case "Extend security code":
                            count = await SecurityCodeWorkAsync(
                                model.Parameters,
                                model.Comment,
                                writer,
                                test,
                                ManualActions.Extend,
                                _adminService.OrganisationBusinessLogic.ExtendOrganisationSecurityCodeAsync);
                            break;
                        case "Extend security codes for all active and pending orgs":
                            result = await SecurityCodeBulkWorkAsync(
                                model.Parameters,
                                model.Comment,
                                writer,
                                test,
                                ManualActions.Create,
                                _adminService.OrganisationBusinessLogic.ExtendOrganisationSecurityCodesInBulkAsync);
                            count = result.Count;
                            total = result.TotalRecords;
                            break;
                        case "Expire security code":
                            count = await ExpireSecurityCodeAsync(model.Parameters, model.Comment, writer, test,
                                ManualActions.Expire);
                            break;
                        case "Expire security codes for all active and pending orgs":
                            result = await ExpireSecurityCodeBulkWorkAsync(
                                model.Parameters,
                                model.Comment,
                                writer,
                                test,
                                ManualActions.Create);
                            count = result.Count;
                            total = result.TotalRecords;
                            break;
                        default:
                            throw new NotImplementedException(
                                $"ERROR: The command '{model.Command}' has not yet been implemented");
                    }

                    if (test)
                    {
                        model.LastTestedCommand = model.Command;
                        model.LastTestedInput = model.Parameters.ReplaceI(Environment.NewLine, ";");
                        model.Tested = true;
                    }
                    else
                    {
                        model.LastTestedCommand = null;
                        model.LastTestedInput = null;
                        model.Comment = null;
                    }

                    //Add a summary to the output
                    if (!string.IsNullOrWhiteSpace(model.Parameters))
                    {
                        total = total ?? model.Parameters.LineCount();
                        model.SuccessMessage =
                            $"SUCCESSFULLY {(test ? "TESTED" : "EXECUTED")} '{model.Command}': {count} of {total}";
                    }
                    else
                    {
                        model.SuccessMessage =
                            $"SUCCESSFULLY {(test ? "TESTED" : "EXECUTED")} '{model.Command}': {count} items";
                    }
                }
                catch (AggregateException ex)
                {
                    foreach (var iex in ex.InnerExceptions)
                    {
                        var exception = iex;

                        while (exception.InnerException != null) exception = exception.InnerException;

                        ModelState.AddModelError("", exception.Message);
                    }
                }
                catch (Exception ex)
                {
                    var exception = ex;

                    while (exception.InnerException != null) exception = exception.InnerException;

                    ModelState.AddModelError("", exception.Message);
                }

                model.Results = writer.ToString();
            }

            return View(model);
        }

        /// <summary>
        ///     Contains the logic to extract an organisation reference from a manual changes parameter line.
        /// </summary>
        /// <param name="lineToReview">line containing the organisation reference</param>
        /// <param name="organisationReference">the found organisation reference or null if unable to extract</param>
        /// <returns>true if the organisation reference was found, false otherwise</returns>
        private static bool HasOrganisationReference(ref string lineToReview, out string organisationReference)
        {
            organisationReference = lineToReview.BeforeFirst("=")?.ToUpper();

            // if found, remove from line to review
            if (!string.IsNullOrEmpty(organisationReference)) lineToReview = lineToReview.Replace(organisationReference, "");

            return !string.IsNullOrEmpty(organisationReference);
        }

        private static bool HasDateTimeInfo(ref string lineToReview, out DateTime extractedDateTime)
        {
            var parameterDateTimeSection = lineToReview.AfterFirst("=")?.ToUpper();
            extractedDateTime = parameterDateTimeSection.ToDateTime();

            return extractedDateTime != DateTime.MinValue;
        }

        /// <summary>
        ///     Contains the logic to extract an snapshotYear from a manual changes parameter line.
        /// </summary>
        /// <param name="lineToReview">line containing the snapshot year</param>
        /// <param name="snapshotYear">the snapshot year or zero if unable to extract</param>
        private static void GetSnapshotYear(ref string lineToReview, out int snapshotYear)
        {
            var parameterDetailSection = lineToReview.AfterFirst("=")?.ToUpper();

            var containsComma = parameterDetailSection.ContainsI(",");

            var candidateSnapshotYear = parameterDetailSection;

            if (containsComma) candidateSnapshotYear = parameterDetailSection.BeforeFirst(",")?.ToUpper();

            snapshotYear = candidateSnapshotYear.ToInt32();

            // if found, remove from line to review
            if (snapshotYear != default) lineToReview = lineToReview.Replace(snapshotYear.ToString(), "");
        }

        /// <summary>
        ///     Contains the logic to extract a comment from the 'lineToReview' parameter.
        ///     <para>
        ///         if the comment was found this method RETURNS 'true' and the extracted comment is available on the 'out'
        ///         parameter 'changeScopeToComment'.
        ///     </para>
        /// </summary>
        /// <param name="lineToReview">line containing the comment to extract</param>
        /// <param name="comment">the found comment or null if unable to extract</param>
        /// <returns>
        ///     FALSE (unable to find) if the comment was NOT found, or TRUE if it was indeed able to read a comment from the
        ///     given line.
        /// </returns>
        private static bool GetComment(ref string lineToReview, out string comment)
        {
            var parameterDetailSection = lineToReview.AfterFirst("=");

            var containsComma = parameterDetailSection.ContainsI(",");

            comment = parameterDetailSection;

            if (containsComma) comment = parameterDetailSection.AfterFirst(",");

            var found = !string.IsNullOrEmpty(comment);

            // if the comment was found - not empty -, then remove it from line under review
            if (found) lineToReview = lineToReview.Replace(comment, "");

            return found;
        }

        private async Task<long> SetOrganisationScopeAsync(string input,
            string comment,
            StringWriter writer,
            ScopeStatuses scopeStatus,
            bool test)
        {
            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var listOfModifiedOrgs = new HashSet<Organisation>();
            foreach (var line in lines)
            {
                var outcome = line;

                i++;

                if (!HasOrganisationReference(ref outcome, out var organisationRef))
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{line}' expected an organisation reference before the '=' character (i.e. OrganisationReference=SnapshotYear,Comment to add to the scope change for this particular organisation)");
                    continue;
                }

                GetSnapshotYear(ref outcome, out var changeScopeToSnapshotYear);
                var wasCommentFoundInLine = GetComment(ref outcome, out var changeScopeToComment);
                var commentBeginsWithNumber = Regex.IsMatch(changeScopeToComment, "^(\\s*\\d{1}|\\d{1})");

                if (commentBeginsWithNumber && changeScopeToSnapshotYear == default)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{organisationRef}' the in-line comment appears to begin with a number, if having a number as part of the comment is intended please reenter this line with the format '{organisationRef}=SnapshotYear,{changeScopeToComment}', alternatively add a comma after the number.");
                    continue;
                }

                if (!wasCommentFoundInLine)
                {
                    if (string.IsNullOrEmpty(comment))
                    {
                        writer.WriteLine(
                            Color.Red,
                            $"{i}: ERROR: '{organisationRef}' please enter a comment in the comments section of this page");
                        continue;
                    }

                    changeScopeToComment = comment;
                }

                try
                {
                    if (processed.Contains(organisationRef))
                    {
                        writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' duplicate organisation");
                        continue;
                    }

                    processed.Add(organisationRef);

                    var outOfScopeOutcome = await _adminService.OrganisationBusinessLogic.SetAsScopeAsync(
                        organisationRef,
                        changeScopeToSnapshotYear,
                        changeScopeToComment,
                        CurrentUser,
                        scopeStatus,
                        false);

                    if (outOfScopeOutcome.Failed)
                    {
                        writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' {outOfScopeOutcome.ErrorMessage}");
                        continue;
                    }

                    if (outOfScopeOutcome.Result.Organisation != null)
                        listOfModifiedOrgs.Add(outOfScopeOutcome.Result.Organisation);

                    var hasBeenWillBe = test ? "will be" : "has been";
                    writer.WriteLine(
                        $"{i}: {organisationRef}: {hasBeenWillBe} set as '{outOfScopeOutcome.Result.ScopeStatus}' for snapshotYear '{outOfScopeOutcome.Result.SubmissionDeadline.Year}' with comment '{outOfScopeOutcome.Result.Reason}'");
                    if (!test)
                        await _adminService.ManualChangeLog.WriteAsync(
                            new ManualChangeLogModel
                            {
                                MethodName = $"SetOrg{scopeStatus.ToString()}",
                                Action = ManualActions.Update,
                                Source = CurrentUser.EmailAddress,
                                Comment = comment,
                                ReferenceName = nameof(Organisation.OrganisationReference),
                                ReferenceValue = organisationRef,
                                TargetName = nameof(Organisation.OrganisationScopes)
                            });
                }
                catch (Exception ex)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' {ex.Message}");
                    continue;
                }

                count++;
            }

            if (!test && listOfModifiedOrgs.Count > 0)
            {
                await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                //todo: writer.WriteLine(Color.Green, $"INFO: Changes saved to database, attempting to update search index.");

                await _adminService.SearchBusinessLogic.UpdateOrganisationSearchIndexAsync(listOfModifiedOrgs.ToArray());
                //todo: writer.WriteLine(Color.Green, $"INFO: Search index updated successfully.");
            }

            return count;
        }

        private async Task<long> FixDatabaseErrorsAsync(string parameters, string comment, StringWriter writer,
            bool test)
        {
            if (!string.IsNullOrWhiteSpace(parameters)) throw new ArgumentException("ERROR: parameters must be empty");

            var count = 0;

            #region Fix latest registrations

            var orgs = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                .Where(o => o.LatestRegistration == null && o.UserOrganisations.Any(uo => uo.PINConfirmedDate != null));
            var subCount = 0;
            foreach (var org in orgs)
            {
                var latestReg = org.UserOrganisations.OrderByDescending(o => o.PINConfirmedDate)
                    .FirstOrDefault(o => o.PINConfirmedDate != null);
                if (latestReg != null)
                {
                    //DOTNETCORE MERGE - latestReg.LatestOrganisation = org;
                    org.LatestRegistration = latestReg;
                    subCount++;
                    writer.WriteLine(
                        $"{subCount:000}: Organisation '{org.OrganisationReference}:{org.OrganisationName}' missing a latest registration {(test ? "will be" : "was successfully")} fixed");
                }
            }

            if (!test && subCount > 0) await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            if (subCount == 0) writer.WriteLine("No organisations missing a latest registration");

            count += subCount;

            #endregion

            #region Fix latest statements

            orgs = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                .Where(o => o.LatestStatement == null && o.Statements.Any(r => r.Status == StatementStatuses.Submitted));
            subCount = 0;
            foreach (var org in orgs)
            {
                var latestStatement = org.Statements.OrderByDescending(o => o.SubmissionDeadline)
                    .FirstOrDefault(o => o.Status == StatementStatuses.Submitted);
                if (latestStatement != null)
                {
                    latestStatement.Organisation = org;
                    org.LatestStatement = latestStatement;
                    subCount++;
                    writer.WriteLine(
                        $"{subCount:000}: Organisation '{org.OrganisationReference}:{org.OrganisationName}' missing a latest statement {(test ? "will be" : "was successfully")} fixed");
                }
            }

            if (!test && subCount > 0) await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            if (subCount == 0) writer.WriteLine("No organisations missing a latest return");

            count += subCount;

            #endregion

            #region Fix latest scopes

            orgs = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                .Where(o => o.LatestScope == null &&
                            o.OrganisationScopes.Any(s => s.ScopeStatus != ScopeStatuses.Unknown));
            subCount = 0;
            foreach (var org in orgs)
            {
                var latestScope = org.OrganisationScopes.OrderByDescending(o => o.SubmissionDeadline)
                    .FirstOrDefault(o => o.ScopeStatus != ScopeStatuses.Unknown);
                if (latestScope != null)
                {
                    //DOTNETCORE MERGE - latestScope.LatestOrganisation = org;
                    org.LatestScope = latestScope;
                    subCount++;
                    writer.WriteLine(
                        $"{subCount:000}: Organisation '{org.OrganisationReference}:{org.OrganisationName}' missing a latest scope {(test ? "will be" : "was successfully")} fixed");
                }
            }

            if (!test && subCount > 0) await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            if (subCount == 0) writer.WriteLine("No organisations missing a latest scope");

            count += subCount;

            #endregion

            return count;
        }

        private async Task<int> SetOrganisationCompanyNumberAsync(string input, string comment, StringWriter writer,
            bool test)
        {
            var methodName = nameof(SetOrganisationCompanyNumberAsync);

            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var processedCoNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                i++;
                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                var organisationRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(organisationRef)) continue;

                if (processed.Contains(organisationRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' duplicate organisation");
                    continue;
                }

                processed.Add(organisationRef);

                var newValue = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI()?.ToUpper();

                if (string.IsNullOrWhiteSpace(newValue))
                {
                    newValue = null;
                }
                else
                {
                    newValue = newValue.FixCompanyNumber();

                    if (!newValue.IsCompanyNumber())
                    {
                        writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' Invalid company number '{newValue}'");
                        continue;
                    }

                    if (processedCoNos.Contains(newValue))
                    {
                        writer.WriteLine(Color.Red,
                            $"{i}: ERROR: '{organisationRef}' duplicate company number '{newValue}'");
                        continue;
                    }

                    processedCoNos.Add(newValue);
                }

                var org = await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.OrganisationReference.ToUpper() == organisationRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}' Cannot find organisation with this organisation reference");
                    continue;
                }

                var oldValue = org.CompanyNumber;

                if (oldValue == newValue)
                {
                    writer.WriteLine(
                        Color.Orange,
                        $"{i}: WARNING '{organisationRef}' '{org.OrganisationName}' Company Number='{org.CompanyNumber}' already set to '{oldValue}'");
                }
                else if (!string.IsNullOrWhiteSpace(newValue)
                         && await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                             .AnyAsync(o => o.CompanyNumber == newValue && o.OrganisationId != org.OrganisationId))
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR '{organisationRef}' Another organisation exists with this company number");
                    continue;
                }
                else
                {
                    //Output the actual execution result
                    org.CompanyNumber = newValue;
                    writer.WriteLine(
                        $"{i}: {organisationRef}: {org.OrganisationName} Company Number='{org.CompanyNumber}' set to '{newValue}'");
                    if (!test)
                    {
                        await _adminService.ManualChangeLog.WriteAsync(
                            new ManualChangeLogModel(
                                methodName,
                                ManualActions.Update,
                                CurrentUser.EmailAddress,
                                nameof(Organisation.OrganisationReference),
                                organisationRef,
                                nameof(Organisation.CompanyNumber),
                                oldValue,
                                newValue,
                                comment));
                        await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                    }
                }

                count++;
            }

            return count;
        }

        private async Task<int> SetOrganisationSicCodesAsync(string input, string comment, StringWriter writer,
            bool test)
        {
            var methodName = nameof(SetOrganisationSicCodesAsync);

            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                i++;
                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                // ensure we have value BEFORE the = sign
                var organisationRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(organisationRef)) continue;

                if (processed.Contains(organisationRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' duplicate organisation");
                    continue;
                }

                processed.Add(organisationRef);

                // ensure the organisation ref exists
                var org = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .FirstOrDefault(o => o.OrganisationReference.ToLower() == organisationRef.ToLower());
                if (org == null)
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}' Cannot find organisation with this organisation reference");
                    continue;
                }

                // ensure the org is active
                if (org.Status != OrganisationStatuses.Active)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' is not an active organisation so you cannot change its SIC codes");
                    continue;
                }

                // ensure the org does not have a company number
                if (string.IsNullOrEmpty(org.CompanyNumber) == false)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' has a company number so you cannot change this organisation");
                    continue;
                }

                // ensure we have value AFTER the = sign
                var newSicCodes = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI()?.ToUpper();
                if (string.IsNullOrWhiteSpace(newSicCodes))
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' must contain at least one SIC code");
                    continue;
                }

                // ensure all sic codes are integers
                var sicCodes = newSicCodes.Trim(' ').Split(',');
                if (sicCodes.Any(x => int.TryParse(x, out var o) == false))
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' you can only input numeric SIC codes");
                    continue;
                }

                // ensure all sic codes exist in db
                var invalidSicCodes = new List<string>();
                var parsedSicCodes = new List<int>();
                foreach (var sc in sicCodes)
                {
                    var parsedSc = int.Parse(sc);
                    if (SharedBusinessLogic.DataRepository.GetAll<SicCode>().Any(x => x.SicCodeId == parsedSc) == false)
                        invalidSicCodes.Add(sc);
                    else
                        parsedSicCodes.Add(parsedSc);
                }

                if (invalidSicCodes.Count > 0)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' the following SIC codes do not exist in the database: {string.Join(",", invalidSicCodes)}'");
                    continue;
                }

                // get all existing sic codes
                var prevSicCodes = org.GetLatestSicCodeIdsString();

                // remove all existing sic codes
                org.GetLatestSicCodes().ForEach(ent => ent.Retired = VirtualDateTime.Now);

                // set new sic codes
                parsedSicCodes.ForEach(
                    x =>
                    {
                        var sic = new OrganisationSicCode {Organisation = org, SicCodeId = x, Source = "Manual"};
                        SharedBusinessLogic.DataRepository.Insert(sic);
                        org.OrganisationSicCodes.Add(sic);
                    });

                //Output the actual execution result
                var oldValue = string.Join(",", prevSicCodes);
                var newValue = string.Join(",", parsedSicCodes);
                var hasBeenWillBe = test ? "will be" : "has been";
                writer.WriteLine(
                    $"{i}: {organisationRef}:{org.OrganisationName} SIC codes={oldValue} {hasBeenWillBe} set to {newValue}");
                if (!test)
                {
                    await _adminService.ManualChangeLog.WriteAsync(
                        new ManualChangeLogModel(
                            methodName,
                            ManualActions.Update,
                            CurrentUser.EmailAddress,
                            nameof(Organisation.OrganisationReference),
                            organisationRef,
                            nameof(Organisation.OrganisationSicCodes),
                            oldValue,
                            newValue,
                            comment));
                    await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                }

                count++;
            }

            return count;
        }

        private async Task<int> SetOrganisationAddressesAsync(string input, string comment, StringWriter writer,
            bool test)
        {
            var methodName = nameof(SetOrganisationAddressesAsync);

            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                i++;
                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                // ensure we have value BEFORE the = sign
                var organisationRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(organisationRef)) continue;

                if (processed.Contains(organisationRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' duplicate organisation");
                    continue;
                }

                processed.Add(organisationRef);

                // ensure the organisation ref exists
                var org = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .FirstOrDefault(o => o.OrganisationReference.ToLower() == organisationRef.ToLower());
                if (org == null)
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}' Cannot find organisation with this organisation reference");
                    continue;
                }

                // ensure the org is active
                if (org.Status != OrganisationStatuses.Active)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' is not an active organisation so you cannot change its address");
                    continue;
                }

                // ensure the org does not have a company number
                if (string.IsNullOrEmpty(org.CompanyNumber) == false)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' has a company number so you cannot change this organisation");
                    continue;
                }

                // ensure we have value AFTER the = sign
                var addressEntries = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI();
                if (string.IsNullOrWhiteSpace(addressEntries))
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' must contain an address entry");
                    continue;
                }

                // ensure all address fields are present
                // [Address1],[Address2],[Address3],[TownCity],[County],[Country],[PostCode]
                var addressFields = addressEntries.Split(',');
                if (addressFields.Length != 7)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' doesnt have the correct number of address fields. Expected 7 fields but saw {addressFields.Length} fields");
                    continue;
                }

                // extract fields
                var address1 = addressFields[0];
                var address2 = addressFields[1];
                var address3 = addressFields[2];
                var townCity = addressFields[3];
                var county = addressFields[4];
                var country = addressFields[5];
                var postCode = addressFields[6];

                // ensure mandatory fields are set
                var requiredState = new List<string>();

                if (string.IsNullOrWhiteSpace(address1)) requiredState.Add("Address1 is required");

                if (address1.Length > 100) requiredState.Add("Address1 is greater than 100 chars");

                if (string.IsNullOrWhiteSpace(address2) == false && address2.Length > 100)
                    requiredState.Add("Address2 is greater than 100 chars");

                if (string.IsNullOrWhiteSpace(address3) == false && address3.Length > 100)
                    requiredState.Add("Address3 is greater than 100 chars");

                if (string.IsNullOrWhiteSpace(townCity)) requiredState.Add("Town\\City is required");

                if (townCity.Length > 100) requiredState.Add("Town\\City is greater than 100 chars");

                if (string.IsNullOrWhiteSpace(county) == false && county.Length > 100)
                    requiredState.Add("County is greater than 100 chars");

                if (string.IsNullOrWhiteSpace(country) == false && country.Length > 100)
                    requiredState.Add("Country is greater than 100 chars");

                if (string.IsNullOrWhiteSpace(postCode)) requiredState.Add("Postcode is required");

                if (postCode.Length < 3) requiredState.Add("Postcode is less than 3 chars");

                if (postCode.Length > 100) requiredState.Add("Postcode is greater than 100 chars");

                if (requiredState.Count > 0)
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' {string.Join(",", requiredState)}");
                    continue;
                }

                // add new address
                var prevAddress = org.LatestAddress;
                var newAddress = new OrganisationAddress
                {
                    OrganisationId = org.OrganisationId,
                    Address1 = address1,
                    Address2 = address2,
                    Address3 = address3,
                    TownCity = townCity,
                    County = county,
                    Country = country,
                    PostCode = postCode,
                    Source = "Manual"
                };

                newAddress.SetStatus(AddressStatuses.Active, CurrentUser.UserId, $"Inserted by {newAddress.Source}");
                if (prevAddress != null)
                    prevAddress.SetStatus(AddressStatuses.Retired, CurrentUser.UserId,
                        $"Replaced by {newAddress.Source}");

                org.LatestAddress = newAddress;

                SharedBusinessLogic.DataRepository.Insert(newAddress);
                org.OrganisationAddresses.Add(newAddress);

                //Output the actual execution result
                var oldValue = prevAddress == null ? "No previous address" : prevAddress.GetAddressString();
                var newValue = string.Join(",", addressFields);
                var hasBeenWillBe = test ? "will be" : "has been";
                writer.WriteLine(
                    $"{i}: {organisationRef}:{org.OrganisationName} Address={oldValue} {hasBeenWillBe} set to {newValue}");
                if (!test)
                {
                    await _adminService.ManualChangeLog.WriteAsync(
                        new ManualChangeLogModel(
                            methodName,
                            ManualActions.Update,
                            CurrentUser.EmailAddress,
                            nameof(Organisation.OrganisationReference),
                            organisationRef,
                            nameof(Organisation.LatestAddress),
                            oldValue,
                            newValue,
                            comment));
                    await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                }

                count++;
            }

            return count;
        }

        private async Task<int> SetPublicSectorTypeAsync(string input, string comment, StringWriter writer, bool test)
        {
            var methodName = nameof(SetPublicSectorTypeAsync);

            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                i++;
                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                // ensure we have value BEFORE the = sign
                var organisationRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(organisationRef)) continue;

                if (processed.Contains(organisationRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' duplicate organisation");
                    continue;
                }

                processed.Add(organisationRef);

                // ensure the organisation ref exists
                var org = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .FirstOrDefault(o => o.OrganisationReference.ToLower() == organisationRef.ToLower());
                if (org == null)
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}' Cannot find organisation with this organisation reference");
                    continue;
                }

                // ensure the org is active
                if (org.Status != OrganisationStatuses.Active)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' is not an active organisation so you cannot change its public sector type");
                    continue;
                }

                // ensure the org is public sector
                if (org.SectorType != SectorTypes.Public)
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' is not a public sector organisation");
                    continue;
                }

                // ensure we have value AFTER the = sign
                var enteredClassification = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI();
                if (string.IsNullOrWhiteSpace(enteredClassification))
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' must contain a public sector type");
                    continue;
                }

                // ensure only one public sector type can be entered
                if (enteredClassification.Contains(","))
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' you can only assign one public sector type per organisation'");
                    continue;
                }

                // ensure the public sector type is an integer
                if (int.TryParse(enteredClassification, out var parsedPublicSectorTypeId) == false)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' you can only input a numeric public sector type");
                    continue;
                }

                // ensure the public sector type exists
                var newSectorType = SharedBusinessLogic.DataRepository.Get<PublicSectorType>(parsedPublicSectorTypeId);
                if (newSectorType == null)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' public sector type {parsedPublicSectorTypeId} does not exist");
                    continue;
                }

                // ensure the organisation isn't already set to the specified public sector type
                if (org.LatestPublicSectorType?.PublicSectorTypeId == parsedPublicSectorTypeId)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{organisationRef}:{org.OrganisationName}' is already set to {parsedPublicSectorTypeId}:{newSectorType.Description}");
                    continue;
                }

                // retire current public sector type
                var prevClassification = org.LatestPublicSectorType;
                if (prevClassification != null) prevClassification.Retired = VirtualDateTime.Now;

                // create new public sector type mapping to the org
                var newOrgSectorClass = new OrganisationPublicSectorType
                {
                    OrganisationId = org.OrganisationId,
                    PublicSectorTypeId = parsedPublicSectorTypeId,
                    PublicSectorType = newSectorType,
                    Source = "Manual"
                };
                SharedBusinessLogic.DataRepository.Insert(newOrgSectorClass);
                org.LatestPublicSectorType = newOrgSectorClass;

                //Output the actual execution result
                var oldValue = prevClassification == null
                    ? "No previous public sector type"
                    : prevClassification.PublicSectorType.Description;
                var newValue = newSectorType.Description;
                var hasBeenWillBe = test ? "will be" : "has been";
                writer.WriteLine(
                    $"{i}: {organisationRef}:{org.OrganisationName} public sector type={oldValue} {hasBeenWillBe} set to {newValue}");
                if (!test)
                {
                    await _adminService.ManualChangeLog.WriteAsync(
                        new ManualChangeLogModel(
                            methodName,
                            ManualActions.Update,
                            CurrentUser.EmailAddress,
                            nameof(Organisation.OrganisationReference),
                            organisationRef,
                            nameof(Organisation.LatestPublicSectorType),
                            oldValue,
                            newValue,
                            comment));
                    await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                }

                count++;
            }

            return count;
        }

        private async Task<int> SetOrganisationDUNSNumberAsync(string input, string comment, StringWriter writer,
            bool test)
        {
            var methodName = nameof(SetOrganisationDUNSNumberAsync);

            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var processedNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                i++;
                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                var organisationRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(organisationRef)) continue;

                if (processed.Contains(organisationRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' duplicate organisation");
                    continue;
                }

                processed.Add(organisationRef);

                var newValue = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI()?.ToUpper();

                if (string.IsNullOrWhiteSpace(newValue))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' must contain a DUNS number");
                    continue;
                }

                if (!newValue.IsDUNSNumber())
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' Invalid DUNS number '{newValue}'");
                    continue;
                }

                if (processedNumbers.Contains(newValue))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' duplicate DUNS number '{newValue}'");
                    continue;
                }

                processedNumbers.Add(newValue);

                var org = await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.OrganisationReference.ToUpper() == organisationRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}' Cannot find organisation with this organisation reference");
                    continue;
                }

                var oldValue = org.DUNSNumber;

                if (oldValue == newValue)
                {
                    writer.WriteLine(
                        Color.Orange,
                        $"{i}: WARNING '{organisationRef}' '{org.OrganisationName}' DUNS Number='{org.DUNSNumber}' already set to '{newValue}'");
                }
                else if (!string.IsNullOrWhiteSpace(oldValue))
                {
                    writer.WriteLine(
                        Color.Orange,
                        $"{i}: ERROR '{organisationRef}' '{org.OrganisationName}' already holds a different DUNS Number='{org.DUNSNumber}'");
                    continue;
                }
                else if (await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .AnyAsync(o => o.DUNSNumber == newValue && o.OrganisationId != org.OrganisationId))
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR '{organisationRef}' Another organisation exists with this DUNS number");
                    continue;
                }
                else
                {
                    //Output the actual execution result
                    org.DUNSNumber = newValue;
                    writer.WriteLine(
                        $"{i}: {organisationRef}: {org.OrganisationName} DUNS Number='{org.DUNSNumber}' set to '{newValue}'");
                    if (!test)
                    {
                        await _adminService.ManualChangeLog.WriteAsync(
                            new ManualChangeLogModel(
                                methodName,
                                ManualActions.Update,
                                CurrentUser.EmailAddress,
                                nameof(Organisation.OrganisationReference),
                                organisationRef,
                                nameof(Organisation.DUNSNumber),
                                oldValue,
                                newValue,
                                comment));
                        await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                    }
                }

                count++;
            }

            return count;
        }

        private async Task<int> DeleteSubmissionsAsync(string input, string comment, StringWriter writer, bool test)
        {
            var methodName = nameof(DeleteSubmissionsAsync);

            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                i++;
                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                var organisationRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(organisationRef)) continue;

                if (processed.Contains(organisationRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR '{organisationRef}' duplicate organisation");
                    continue;
                }

                processed.Add(organisationRef);

                var org = await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.OrganisationReference.ToUpper() == organisationRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}' Cannot find organisation with this organisation reference");
                    continue;
                }

                var number = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI();
                if (number == null || !string.IsNullOrWhiteSpace(number) && !number.IsNumber())
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' invalid year '{number}'");
                    continue;
                }

                var year = number.ToInt32(_adminService.SharedBusinessLogic.GetReportingDeadline(org.SectorType).Year);
                var statement = org.Statements.OrderByDescending(o => o.StatusDate)
                    .FirstOrDefault(r => r.Status == StatementStatuses.Submitted && r.SubmissionDeadline.Year == year);
                if (statement == null)
                {
                    writer.WriteLine(Color.Orange,
                        $"{i}: WARNING: '{organisationRef}' Cannot find submitted data for year {year}");
                    continue;
                }

                var newValue = StatementStatuses.Deleted;
                var oldValue = statement.Status;

                //Output the actual execution result
                statement.SetStatus(newValue, CurrentUser.UserId, comment);
                if (statement.Organisation.LatestStatement != null &&
                    statement.Organisation.LatestStatement.StatementId == statement.StatementId)
                {
                    //Get the latest return (if any)
                    var latestStatement = statement.Organisation.Statements.OrderByDescending(o => o.SubmissionDeadline)
                        .FirstOrDefault(o => o.Status == StatementStatuses.Submitted);

                    //Set the new latest return or the organisation 
                    statement.Organisation.LatestStatement = latestStatement;
                    if (latestStatement != null) latestStatement.Organisation = org;

                    //Remove the old latest return 
                    statement.Organisation = null;
                }

                writer.WriteLine(
                    $"{i}: {organisationRef}: {org.OrganisationName} Year='{year}' Status='{oldValue}' set to '{newValue}'");
                if (!test)
                {
                    await _adminService.ManualChangeLog.WriteAsync(
                        new ManualChangeLogModel(
                            methodName,
                            ManualActions.Update,
                            CurrentUser.EmailAddress,
                            nameof(Statement.StatementId),
                            statement.StatementId.ToString(),
                            nameof(Statement.Status),
                            oldValue.ToString(),
                            newValue.ToString(),
                            comment,
                            $"Year={year} Organisation='{organisationRef}'"));
                    await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                }

                count++;
            }

            return count;
        }

        private async Task<int> UnRetireOrganisationsAsync(string input, string comment, StringWriter writer, bool test)
        {
            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                i++;
                if (line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' contains '=' character");
                    continue;
                }

                var organisationRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(organisationRef)) continue;

                if (processed.Contains(organisationRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' duplicate organisation");
                    continue;
                }

                processed.Add(organisationRef);

                var manualChangeLogModel = new ManualChangeLogModel
                {
                    MethodName = nameof(UnRetireOrganisationsAsync),
                    Action = ManualActions.Update,
                    Source = CurrentUser.EmailAddress,
                    Comment = comment,
                    ReferenceName = nameof(Organisation.OrganisationReference),
                    ReferenceValue = organisationRef
                };

                var org = await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.OrganisationReference.ToUpper() == organisationRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}' Cannot find organisation with this organisation reference");
                    continue;
                }

                manualChangeLogModel.TargetName = nameof(Organisation.Status);
                manualChangeLogModel.TargetOldValue = org.Status.ToString();

                try
                {
                    var errorMessage =
                        _adminService.OrganisationBusinessLogic.UnRetire(org, CurrentUser.UserId, comment);

                    if (errorMessage != null)
                    {
                        writer.WriteLine(Color.Red, $"{i}: ERROR: {errorMessage}");
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' {ex.Message}");
                    continue;
                }

                manualChangeLogModel.TargetNewValue = org.Status.ToString();

                writer.WriteLine(
                    $"{i}: {organisationRef}: {org.OrganisationName} reverted from status '{manualChangeLogModel.TargetOldValue}' to '{manualChangeLogModel.TargetNewValue}'");
                if (!test)
                {
                    await _adminService.ManualChangeLog.WriteAsync(manualChangeLogModel);
                    await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                    await _adminService.SearchBusinessLogic.UpdateOrganisationSearchIndexAsync(org);
                }

                count++;
            }

            return count;
        }

        private async Task<int> RetireOrganisationsAsync(string input, string comment, StringWriter writer, bool test)
        {
            var methodName = nameof(RetireOrganisationsAsync);

            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                i++;
                if (line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' contains '=' character");
                    continue;
                }

                var organisationRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(organisationRef)) continue;

                if (processed.Contains(organisationRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' duplicate organisation");
                    continue;
                }

                processed.Add(organisationRef);

                var org = await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.OrganisationReference.ToUpper() == organisationRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}' Cannot find organisation with this organisation reference");
                    continue;
                }

                var newValue = OrganisationStatuses.Retired;
                var oldValue = org.Status;

                if (oldValue == newValue)
                {
                    writer.WriteLine(
                        Color.Orange,
                        $"{i}: WARNING: '{organisationRef}' '{org.OrganisationName}' Status='{oldValue}' already set to '{newValue}'");
                }
                else
                {
                    //Output the actual execution result
                    org.SetStatus(newValue, CurrentUser.UserId, comment);
                    writer.WriteLine(
                        $"{i}: {organisationRef}: {org.OrganisationName} Status='{oldValue}' set to '{newValue}'");
                    if (!test)
                    {
                        await _adminService.ManualChangeLog.WriteAsync(
                            new ManualChangeLogModel(
                                methodName,
                                ManualActions.Update,
                                CurrentUser.EmailAddress,
                                nameof(Organisation.OrganisationReference),
                                organisationRef,
                                nameof(Organisation.Status),
                                oldValue.ToString(),
                                newValue.ToString(),
                                comment));
                        await SharedBusinessLogic.DataRepository.SaveChangesAsync();

                        //Add or remove this organisation to/from the search index
                        await _adminService.SearchBusinessLogic.UpdateOrganisationSearchIndexAsync(org);
                    }
                }

                count++;
            }

            return count;
        }

        private async Task<int> ConvertPrivateOrganisationsToPublicAsync(string input, string comment,
            StringWriter writer, bool test)
        {
            var methodName = nameof(ConvertPrivateOrganisationsToPublicAsync);

            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                i++;
                if (line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' contains '=' character");
                    continue;
                }

                var organisationRef = line?.ToUpper();
                if (string.IsNullOrWhiteSpace(organisationRef)) continue;

                if (processed.Contains(organisationRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' duplicate organisation");
                    continue;
                }

                processed.Add(organisationRef);

                var newSector = SectorTypes.Public;

                var org = await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.OrganisationReference.ToUpper() == organisationRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}' Cannot find organisation with this organisation reference");
                    continue;
                }

                var oldSector = org.SectorType;

                var badReturnDates = false;
                foreach (var statement in org.Statements)
                {
                    var oldDate = statement.SubmissionDeadline;
                    var newDate = _adminService.SharedBusinessLogic.GetReportingDeadline(newSector, oldDate.Year);
                    if (oldDate == newDate) continue;

                    badReturnDates = true;
                    break;
                }

                var badScopeDates = false;
                foreach (var scope in org.OrganisationScopes)
                {
                    var oldDate = scope.SubmissionDeadline;
                    var newDate = _adminService.SharedBusinessLogic.GetReportingDeadline(newSector, oldDate.Year);
                    if (oldDate == newDate) continue;

                    badScopeDates = true;
                    break;
                }

                var sicCodes = org.GetLatestSicCodes();

                if (oldSector == newSector && sicCodes.Any(s => s.SicCodeId == 1 && s.Retired == null) &&
                    !badReturnDates && !badScopeDates)
                {
                    writer.WriteLine(
                        Color.Orange,
                        $"{i}: WARNING: '{organisationRef}' '{org.OrganisationName}' sector already set to '{oldSector}'");
                }
                else
                {
                    if (oldSector != newSector)
                    {
                        //Change the sector type
                        org.SectorType = newSector;
                        if (!test)
                            await _adminService.ManualChangeLog.WriteAsync(
                                new ManualChangeLogModel(
                                    methodName,
                                    ManualActions.Update,
                                    CurrentUser.EmailAddress,
                                    nameof(Organisation.OrganisationReference),
                                    organisationRef,
                                    nameof(Organisation.SectorType),
                                    oldSector.ToString(),
                                    newSector.ToString(),
                                    comment));
                    }

                    //Add SIC Code 1
                    if (!sicCodes.Any(sic => sic.SicCodeId == 1 && sic.Retired == null))
                    {
                        org.OrganisationSicCodes.Add(
                            new OrganisationSicCode
                            {
                                OrganisationId = org.OrganisationId,
                                SicCodeId = 1,
                                Source = "Manual",
                                Created = sicCodes.Any() ? sicCodes.First().Created : VirtualDateTime.Now
                            });
                        if (!test)
                            await _adminService.ManualChangeLog.WriteAsync(
                                new ManualChangeLogModel(
                                    methodName,
                                    ManualActions.Create,
                                    CurrentUser.EmailAddress,
                                    nameof(Organisation.OrganisationReference),
                                    organisationRef,
                                    nameof(OrganisationSicCode),
                                    null,
                                    "1",
                                    comment));
                    }

                    //Set accounting Date
                    if (badReturnDates)
                        foreach (var statement in org.Statements)
                        {
                            var oldDate = statement.SubmissionDeadline;
                            var newDate =
                                _adminService.SharedBusinessLogic.GetReportingDeadline(newSector, oldDate.Year);
                            if (oldDate == newDate) continue;

                            statement.SubmissionDeadline = newDate;
                            if (!test)
                                await _adminService.ManualChangeLog.WriteAsync(
                                    new ManualChangeLogModel(
                                        methodName,
                                        ManualActions.Update,
                                        CurrentUser.EmailAddress,
                                        nameof(statement.StatementId),
                                        statement.StatementId.ToString(),
                                        nameof(Statement.SubmissionDeadline),
                                        oldDate.ToString(),
                                        newDate.ToString(),
                                        comment));
                        }

                    //Set snapshot Date
                    if (badScopeDates)
                        foreach (var scope in org.OrganisationScopes)
                        {
                            var oldDate = scope.SubmissionDeadline;
                            var newDate =
                                _adminService.SharedBusinessLogic.GetReportingStartDate(newSector, oldDate.Year);
                            if (oldDate == newDate) continue;

                            scope.SubmissionDeadline = newDate;
                            if (!test)
                                await _adminService.ManualChangeLog.WriteAsync(
                                    new ManualChangeLogModel(
                                        methodName,
                                        ManualActions.Update,
                                        CurrentUser.EmailAddress,
                                        nameof(scope.OrganisationScopeId),
                                        scope.OrganisationScopeId.ToString(),
                                        nameof(scope.SubmissionDeadline),
                                        oldDate.ToString(),
                                        newDate.ToString(),
                                        comment));
                        }

                    writer.WriteLine(
                        $"{i}: {organisationRef}: {org.OrganisationName} sector {oldSector} set to '{newSector}'");
                    if (!test) await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                }

                count++;
            }

            return count;
        }

        private async Task<int> ConvertPublicOrganisationsToPrivateAsync(string input, string comment,
            StringWriter writer, bool test)
        {
            var methodName = nameof(ConvertPublicOrganisationsToPrivateAsync);

            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                i++;
                if (line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' contains '=' character");
                    continue;
                }

                var organisationRef = line?.ToUpper();
                if (string.IsNullOrWhiteSpace(organisationRef)) continue;

                if (processed.Contains(organisationRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: '{organisationRef}' duplicate organisation");
                    continue;
                }

                processed.Add(organisationRef);

                var newSector = SectorTypes.Private;

                var org = await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.OrganisationReference.ToUpper() == organisationRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}' Cannot find organisation with this organisation reference");
                    continue;
                }

                var oldSector = org.SectorType;

                var badReturnDates = false;
                foreach (var statement in org.Statements)
                {
                    var oldDate = statement.SubmissionDeadline;
                    var newDate = _adminService.SharedBusinessLogic.GetReportingDeadline(newSector, oldDate.Year);
                    if (oldDate == newDate) continue;

                    badReturnDates = true;
                    break;
                }

                var badScopeDates = false;
                foreach (var scope in org.OrganisationScopes)
                {
                    var oldDate = scope.SubmissionDeadline;
                    var newDate = _adminService.SharedBusinessLogic.GetReportingStartDate(newSector, oldDate.Year);
                    if (oldDate == newDate) continue;

                    badScopeDates = true;
                    break;
                }

                var sicCodes = org.GetLatestSicCodes();

                if (oldSector == newSector && !sicCodes.Any(s => s.SicCodeId == 1) && !badReturnDates && !badScopeDates)
                {
                    writer.WriteLine(
                        Color.Orange,
                        $"{i}: WARNING: '{organisationRef}' '{org.OrganisationName}' sector already set to '{oldSector}'");
                }
                else
                {
                    //Change the sector type
                    if (oldSector != newSector)
                    {
                        org.SectorType = newSector;
                        if (!test)
                            await _adminService.ManualChangeLog.WriteAsync(
                                new ManualChangeLogModel(
                                    methodName,
                                    ManualActions.Update,
                                    CurrentUser.EmailAddress,
                                    nameof(Organisation.OrganisationReference),
                                    organisationRef,
                                    nameof(Organisation.SectorType),
                                    oldSector.ToString(),
                                    newSector.ToString(),
                                    comment));
                    }

                    //Remove SIC Code 1
                    if (sicCodes.Any(s => s.SicCodeId == 1))
                        foreach (var sic in org.OrganisationSicCodes.ToList())
                        {
                            if (sic.SicCodeId != 1) continue;

                            if (!test)
                                await _adminService.ManualChangeLog.WriteAsync(
                                    new ManualChangeLogModel(
                                        methodName,
                                        ManualActions.Delete,
                                        CurrentUser.EmailAddress,
                                        nameof(Organisation.OrganisationReference),
                                        organisationRef,
                                        nameof(OrganisationSicCode),
                                        JsonConvert.SerializeObject(
                                            new
                                            {
                                                sic.OrganisationSicCodeId,
                                                sic.SicCodeId,
                                                sic.OrganisationId,
                                                sic.Source,
                                                sic.Created,
                                                sic.Retired
                                            }),
                                        null,
                                        comment));

                            SharedBusinessLogic.DataRepository.Delete(sic);
                        }

                    //Set accounting Date
                    if (badReturnDates)
                        foreach (var statement in org.Statements)
                        {
                            var oldDate = statement.SubmissionDeadline;
                            var newDate =
                                _adminService.SharedBusinessLogic.GetReportingDeadline(newSector, oldDate.Year);
                            if (oldDate == newDate) continue;

                            statement.SubmissionDeadline = newDate;
                            if (!test)
                                await _adminService.ManualChangeLog.WriteAsync(
                                    new ManualChangeLogModel(
                                        methodName,
                                        ManualActions.Update,
                                        CurrentUser.EmailAddress,
                                        nameof(statement.StatementId),
                                        statement.StatementId.ToString(),
                                        nameof(Statement.SubmissionDeadline),
                                        oldDate.ToString(),
                                        newDate.ToString(),
                                        comment));

                            if (string.IsNullOrWhiteSpace(statement.ApprovingPerson))
                                writer.WriteLine(
                                    Color.Orange,
                                    $"    WARNING: No personal responsible for '{organisationRef}' for data submited for year '{oldDate.Year}'");
                        }

                    //Set snapshot Date
                    if (badScopeDates)
                        foreach (var scope in org.OrganisationScopes)
                        {
                            var oldDate = scope.SubmissionDeadline;
                            var newDate =
                                _adminService.SharedBusinessLogic.GetReportingStartDate(newSector, oldDate.Year);
                            if (oldDate == newDate) continue;

                            scope.SubmissionDeadline = newDate;
                            if (!test)
                                await _adminService.ManualChangeLog.WriteAsync(
                                    new ManualChangeLogModel(
                                        methodName,
                                        ManualActions.Update,
                                        CurrentUser.EmailAddress,
                                        nameof(scope.OrganisationScopeId),
                                        scope.OrganisationScopeId.ToString(),
                                        nameof(scope.SubmissionDeadline),
                                        oldDate.ToString(),
                                        newDate.ToString(),
                                        comment));
                        }

                    writer.WriteLine(
                        $"{i}: {organisationRef}: {org.OrganisationName} sector {oldSector} set to '{newSector}'");
                    if (!test) await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                }

                count++;
            }

            return count;
        }

        /// <summary>
        ///     Sets the latest company name
        /// </summary>
        private async Task<int> SetOrganisationNameAsync(string input, string comment, StringWriter writer, bool test)
        {
            var methodName = nameof(SetOrganisationNameAsync);

            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                i++;
                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                var organisationRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(organisationRef)) continue;

                if (processed.Contains(organisationRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' duplicate organisation");
                    continue;
                }

                processed.Add(organisationRef);
                var newValue = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI();
                if (string.IsNullOrWhiteSpace(newValue))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' No organisation name specified");
                    continue;
                }

                var org = await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.OrganisationReference.ToUpper() == organisationRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}' Cannot find organisation with this organisation reference");
                    continue;
                }

                var oldValue = org.OrganisationName;
                if (oldValue == newValue)
                {
                    writer.WriteLine(Color.Orange,
                        $"{i}: WARNING: '{organisationRef}' '{org.OrganisationName}' already set to '{newValue}'");
                }
                else if (await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .AnyAsync(o =>
                        o.OrganisationName.ToLower() == newValue.ToLower() && o.OrganisationId != org.OrganisationId))
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}' Another organisation exists with this company name");
                    continue;
                }
                else
                {
                    //Output the actual execution result
                    org.OrganisationName = newValue;
                    if (!test)
                        await _adminService.ManualChangeLog.WriteAsync(
                            new ManualChangeLogModel(
                                methodName,
                                ManualActions.Update,
                                CurrentUser.EmailAddress,
                                nameof(Organisation.OrganisationReference),
                                organisationRef,
                                nameof(Organisation.OrganisationName),
                                oldValue,
                                newValue,
                                comment));

                    org.OrganisationNames.Add(new OrganisationName
                        {Organisation = org, Source = "Manual", Name = newValue});
                    if (!test)
                        await _adminService.ManualChangeLog.WriteAsync(
                            new ManualChangeLogModel(
                                methodName,
                                ManualActions.Create,
                                CurrentUser.EmailAddress,
                                nameof(Organisation.OrganisationReference),
                                organisationRef,
                                nameof(Organisation.OrganisationName),
                                oldValue,
                                newValue,
                                comment));

                    writer.WriteLine($"{i}: {organisationRef}: '{oldValue}' set to '{newValue}'");
                    if (!test) await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                }

                count++;
            }

            return count;
        }

        /// <summary>
        ///     Removes all previous names and sets the first company name
        /// </summary>
        private async Task<int> ResetOrganisationNameAsync(string input, string comment, StringWriter writer, bool test)
        {
            var methodName = nameof(ResetOrganisationNameAsync);

            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                i++;
                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                var organisationRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(organisationRef)) continue;

                if (processed.Contains(organisationRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' duplicate organisation");
                    continue;
                }

                processed.Add(organisationRef);
                var newValue = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI();
                if (string.IsNullOrWhiteSpace(newValue))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' No organisation name specified");
                    continue;
                }

                var org = await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.OrganisationReference.ToUpper() == organisationRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}' Cannot find organisation with this organisation reference");
                    continue;
                }

                var oldValue = org.OrganisationName;
                if (oldValue == newValue && org.OrganisationNames.Count() == 1)
                {
                    writer.WriteLine(Color.Orange,
                        $"{i}: WARNING: '{organisationRef}' '{org.OrganisationName}' already set to '{newValue}'");
                }
                else if (await SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .AnyAsync(o =>
                        o.OrganisationName.ToLower() == newValue.ToLower() && o.OrganisationId != org.OrganisationId))
                {
                    writer.WriteLine(Color.Red,
                        $"{i}: ERROR: '{organisationRef}' Another organisation exists with this company name");
                    continue;
                }
                else
                {
                    if (oldValue != newValue)
                    {
                        //Output the actual execution result
                        org.OrganisationName = newValue;
                        if (!test)
                            await _adminService.ManualChangeLog.WriteAsync(
                                new ManualChangeLogModel(
                                    methodName,
                                    ManualActions.Update,
                                    CurrentUser.EmailAddress,
                                    nameof(Organisation.OrganisationReference),
                                    organisationRef,
                                    nameof(Organisation.OrganisationName),
                                    oldValue,
                                    newValue,
                                    comment));
                    }

                    if (org.OrganisationName.Count() != 1)
                    {
                        foreach (var name in org.OrganisationNames.ToList())
                        {
                            if (!test)
                                await _adminService.ManualChangeLog.WriteAsync(
                                    new ManualChangeLogModel(
                                        methodName,
                                        ManualActions.Delete,
                                        CurrentUser.EmailAddress,
                                        nameof(Organisation.OrganisationReference),
                                        organisationRef,
                                        nameof(OrganisationName),
                                        JsonConvert.SerializeObject(
                                            new
                                            {
                                                name.OrganisationNameId,
                                                name.OrganisationId,
                                                name.Name,
                                                name.Created,
                                                name.Source
                                            }),
                                        null,
                                        comment));

                            SharedBusinessLogic.DataRepository.Delete(name);
                        }

                        org.OrganisationNames.Add(
                            new OrganisationName
                                {Organisation = org, Source = "Manual", Name = newValue, Created = org.Created});
                        if (!test)
                            await _adminService.ManualChangeLog.WriteAsync(
                                new ManualChangeLogModel(
                                    methodName,
                                    ManualActions.Create,
                                    CurrentUser.EmailAddress,
                                    nameof(Organisation.OrganisationReference),
                                    organisationRef,
                                    nameof(Organisation.OrganisationName),
                                    oldValue,
                                    newValue,
                                    comment));
                    }

                    writer.WriteLine($"{i}: {organisationRef}: '{oldValue}' set to '{newValue}'");
                    if (!test) await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                }

                count++;
            }

            return count;
        }

        public delegate Task<CustomResult<Organisation>> SecurityCodeDelegate(string organisationRef,
            DateTime securityCodeExpiryDateTime);

        private async Task<int> SecurityCodeWorkAsync(string input,
            string comment,
            StringWriter writer,
            bool test,
            ManualActions manualAction,
            SecurityCodeDelegate callBackDelegatedMethod)
        {
            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var listOfModifiedOrgs = new HashSet<Organisation>();
            foreach (var line in lines)
            {
                var outcome = line;

                i++;

                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                if (!HasOrganisationReference(ref outcome, out var organisationRef))
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{line}' expected an organisation reference before the '=' character (i.e. OrganisationReference=dd/mm/yyyy to know which organisation to modify)");
                    continue;
                }

                if (!HasDateTimeInfo(ref outcome, out var extractedDateTime))
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{line}' expected a valid (dd/mm/yyyy) date value after the '=' character (i.e. OrganisationReference=dd/mm/yyyy to know when to expire the security codes created for this organisation)");
                    continue;
                }

                if (processed.Contains(organisationRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' duplicate organisation");
                    continue;
                }

                processed.Add(organisationRef);

                try
                {
                    var securityCodeWorksOutcome = await callBackDelegatedMethod(organisationRef, extractedDateTime);

                    if (securityCodeWorksOutcome.Failed)
                    {
                        writer.WriteLine(
                            Color.Red,
                            $"{i}: ERROR: '{securityCodeWorksOutcome.ErrorRelatedObject}' {securityCodeWorksOutcome.ErrorMessage}");
                        continue;
                    }

                    if (securityCodeWorksOutcome.Result != null)
                        listOfModifiedOrgs.Add(securityCodeWorksOutcome.Result);

                    var hasBeenWillBe = test ? "will be" : "has been";
                    var createdOrExtended = manualAction == ManualActions.Extend ? "extended" : "created";
                    var securityCodeHiddenOrShow =
                        test
                            ? new string('*', SharedBusinessLogic.SharedOptions.PinLength)
                            : $"{securityCodeWorksOutcome.Result.SecurityCode}";
                    writer.WriteLine(
                        $"{i}: {securityCodeWorksOutcome.Result}: {hasBeenWillBe} {createdOrExtended} to be '{securityCodeHiddenOrShow}' and will expire on '{securityCodeWorksOutcome.Result.SecurityCodeExpiryDateTime:dd/MMMM/yyyy}'");

                    if (!test)
                    {
                        var serialisedInfo = JsonConvert.SerializeObject(
                            new
                            {
                                securityCodeWorksOutcome.Result.SecurityCode,
                                securityCodeWorksOutcome.Result.SecurityCodeExpiryDateTime,
                                securityCodeWorksOutcome.Result.SecurityCodeCreatedDateTime,
                                CurrentUser.EmailAddress
                            });

                        await _adminService.ManualChangeLog.WriteAsync(
                            new ManualChangeLogModel
                            {
                                MethodName = $"{manualAction.ToString()}SecurityCode",
                                Action = manualAction,
                                Source = CurrentUser.EmailAddress,
                                Comment = comment,
                                ReferenceName = nameof(Organisation.OrganisationReference),
                                ReferenceValue = organisationRef,
                                TargetName = nameof(Organisation.SecurityCode),
                                TargetNewValue = serialisedInfo
                            });
                    }
                }
                catch (Exception ex)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' {ex.Message}");
                    continue;
                }

                count++;
            }

            if (!test && listOfModifiedOrgs.Count > 0)
            {
                await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                writer.WriteLine(Color.Green, "INFO: Changes saved to database");
            }

            return count;
        }

        private async Task<int> ExpireSecurityCodeAsync(string input,
            string comment,
            StringWriter writer,
            bool test,
            ManualActions manualAction)
        {
            var methodName = MethodBase.GetCurrentMethod().Name;

            //Split the input into separate action lines
            var lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0) throw new ArgumentException("ERROR: You must supply 1 or more input parameters");

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var listOfModifiedOrgs = new HashSet<Organisation>();
            foreach (var line in lines)
            {
                var outcome = line;

                i++;

                if (line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' must not contain '=' character");
                    continue;
                }

                if (!HasOrganisationReference(ref outcome, out var organisationRef))
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{line}' expected a valid organisation reference to know which organisation to modify");
                    continue;
                }

                if (processed.Contains(organisationRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' duplicate organisation");
                    continue;
                }

                processed.Add(organisationRef);

                try
                {
                    var securityCodeWorksOutcome =
                        await _adminService.OrganisationBusinessLogic.ExpireOrganisationSecurityCodeAsync(organisationRef);

                    if (securityCodeWorksOutcome.Failed)
                    {
                        writer.WriteLine(
                            Color.Red,
                            $"{i}: ERROR: '{securityCodeWorksOutcome.ErrorRelatedObject}' {securityCodeWorksOutcome.ErrorMessage}");
                        continue;
                    }

                    if (securityCodeWorksOutcome.Result != null)
                        listOfModifiedOrgs.Add(securityCodeWorksOutcome.Result);

                    var hasBeenWillBe = test ? "will be" : "has been";
                    writer.WriteLine($"{i}: {securityCodeWorksOutcome.Result}: {hasBeenWillBe} expired.");

                    if (!test)
                    {
                        var serialisedInfo = JsonConvert.SerializeObject(
                            new
                            {
                                securityCodeWorksOutcome.Result.SecurityCode,
                                securityCodeWorksOutcome.Result.SecurityCodeExpiryDateTime,
                                securityCodeWorksOutcome.Result.SecurityCodeCreatedDateTime,
                                CurrentUser.EmailAddress
                            });

                        if (!test)
                            await _adminService.ManualChangeLog.WriteAsync(
                                new ManualChangeLogModel
                                {
                                    MethodName = methodName,
                                    Action = manualAction,
                                    Source = CurrentUser.EmailAddress,
                                    Comment = comment,
                                    ReferenceName = nameof(Organisation.OrganisationReference),
                                    ReferenceValue = organisationRef,
                                    TargetName = nameof(Organisation.SecurityCode),
                                    TargetNewValue = serialisedInfo
                                });
                    }
                }
                catch (Exception ex)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{organisationRef}' {ex.Message}");
                    continue;
                }

                count++;
            }

            if (!test && listOfModifiedOrgs.Count > 0)
            {
                await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                writer.WriteLine(Color.Green, "INFO: Changes saved to database");
            }

            return count;
        }

        public delegate Task<CustomBulkResult<Organisation>> SecurityCodeBulkDelegate(
            DateTime securityCodeExpiryDateTime);

        private async Task<BulkResult> SecurityCodeBulkWorkAsync(string input,
            string comment,
            StringWriter writer,
            bool test,
            ManualActions manualAction,
            SecurityCodeBulkDelegate callBackBulkDelegatedMethod)
        {
            var result = new BulkResult();
            var methodName = MethodBase.GetCurrentMethod().Name;

            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("ERROR: You must supply the expiry date for the security codes");

            if (input.Contains('='))
            {
                writer.WriteLine(Color.Red, $"ERROR: '{input}' must not contain '=' character");
                return result;
            }

            if (!HasDateTimeInfo(ref input, out var extractedDateTime))
            {
                writer.WriteLine(
                    Color.Red,
                    $"ERROR: '{input}' expected a valid (dd/mm/yyyy) date value to know when to expire the security codes");
                return result;
            }

            try
            {
                var securityCodeBulkWorkOutcome = await callBackBulkDelegatedMethod(extractedDateTime);

                if (securityCodeBulkWorkOutcome.Failed)
                {
                    var groupedErrors = securityCodeBulkWorkOutcome.ConcurrentBagOfErrors
                        .GroupBy(x => new {x.ErrorMessage.Code, x.ErrorMessage.Description})
                        .Select(
                            r => new
                            {
                                Total = r.Count(), // count of similar errors
                                r.Key.Code,
                                r.Key.Description
                            });
                    foreach (var reportedError in groupedErrors)
                        writer.WriteLine(
                            Color.Red,
                            $"{reportedError.Total} ERROR(S) of type '{reportedError.Code}' {reportedError.Description}");
                }

                var numberOfSuccesses = securityCodeBulkWorkOutcome.ConcurrentBagOfSuccesses.Count;
                var numberOfFailures = securityCodeBulkWorkOutcome.ConcurrentBagOfErrors.Count;

                var hasBeenWillBe = test ? "will be" : "have been";
                var hasBeenWillBeSuccessfully = test ? hasBeenWillBe : $"{hasBeenWillBe} successfully";
                var failureMessage = numberOfFailures > 0
                    ? $"A Total of {numberOfFailures} records FAILED and {hasBeenWillBe} ignored. " // The space at the end of this message is required for presentation.
                    : string.Empty;
                var successMessage = numberOfSuccesses > 0
                    ? $"A total of {numberOfSuccesses} security codes {hasBeenWillBeSuccessfully} set to expire on '{extractedDateTime:dd/MMMM/yyyy}'"
                    : string.Empty;

                writer.WriteLine($"{failureMessage}{successMessage}");

                /* Completed List of all individual issues */
                if (securityCodeBulkWorkOutcome.Failed)
                    foreach (var detailedError in securityCodeBulkWorkOutcome.ConcurrentBagOfErrors)
                    {
                        var orgDetails = detailedError.ErrorRelatedObject != null
                            ? detailedError.ErrorRelatedObject.ToString()
                            : "[null]";
                        writer.Write(
                            $"[{orgDetails} {detailedError.ErrorMessage.Code} '{detailedError.ErrorMessage.Description}'] ");
                    }

                if (!test)
                    await _adminService.ManualChangeLog.WriteAsync(
                        new ManualChangeLogModel
                        {
                            MethodName = methodName,
                            Action = manualAction,
                            Source = CurrentUser.EmailAddress,
                            Comment = comment,
                            TargetName = nameof(Organisation.SecurityCode),
                            TargetNewValue = $"{failureMessage}{successMessage}"
                        });

                if (!test && securityCodeBulkWorkOutcome.ConcurrentBagOfSuccesses.Count > 0)
                {
                    SharedBusinessLogic.DataRepository.UpdateChangesInBulk(securityCodeBulkWorkOutcome
                        .ConcurrentBagOfSuccesses);
                    writer.WriteLine(Color.Green, "INFO: Changes saved to database");
                }

                result.Count = numberOfSuccesses;
                result.TotalRecords = numberOfSuccesses + numberOfFailures;
            }
            catch (Exception ex)
            {
                writer.WriteLine(Color.Red, $"ERROR: {ex.Message}");
            }

            return result;
        }

        private async Task<BulkResult> ExpireSecurityCodeBulkWorkAsync(string input,
            string comment,
            StringWriter writer,
            bool test,
            ManualActions manualAction)
        {
            var result = new BulkResult();
            var methodName = MethodBase.GetCurrentMethod().Name;

            if (!string.IsNullOrWhiteSpace(input)) throw new ArgumentException("ERROR: parameters must be empty");

            try
            {
                var securityCodeBulkWorkOutcome =
                    await _adminService.OrganisationBusinessLogic.ExpireOrganisationSecurityCodesInBulkAsync();

                if (securityCodeBulkWorkOutcome.Failed)
                {
                    var groupedErrors = securityCodeBulkWorkOutcome.ConcurrentBagOfErrors
                        .GroupBy(x => new {x.ErrorMessage.Code, x.ErrorMessage.Description})
                        .Select(
                            r => new
                            {
                                Total = r.Count(), // count of similar errors
                                r.Key.Code,
                                r.Key.Description
                            });
                    foreach (var reportedError in groupedErrors)
                        writer.WriteLine(
                            Color.Red,
                            $"{reportedError.Total} ERROR(S) of type '{reportedError.Code}' {reportedError.Description}");
                }

                var numberOfSuccesses = securityCodeBulkWorkOutcome.ConcurrentBagOfSuccesses.Count;
                var numberOfFailures = securityCodeBulkWorkOutcome.ConcurrentBagOfErrors.Count;

                var hasBeenWillBe = test ? "will be" : "have been";
                var hasBeenWillBeSuccessfully = test ? hasBeenWillBe : $"{hasBeenWillBe} successfully";
                var failureMessage = numberOfFailures > 0
                    ? $"A Total of {numberOfFailures} records FAILED and {hasBeenWillBe} ignored. "
                    : string.Empty;
                var successMessage = numberOfSuccesses > 0
                    ? $"A total of {numberOfSuccesses} security codes {hasBeenWillBeSuccessfully} expired."
                    : string.Empty;

                writer.WriteLine($"{failureMessage}{successMessage}");

                /* Completed List of all individual issues */
                if (securityCodeBulkWorkOutcome.Failed)
                    foreach (var detailedError in securityCodeBulkWorkOutcome.ConcurrentBagOfErrors)
                    {
                        var orgDetails = detailedError.ErrorRelatedObject != null
                            ? detailedError.ErrorRelatedObject.ToString()
                            : "[null]";
                        writer.Write(
                            $"[{orgDetails} {detailedError.ErrorMessage.Code} '{detailedError.ErrorMessage.Description}'] ");
                    }

                if (!test)
                    await _adminService.ManualChangeLog.WriteAsync(
                        new ManualChangeLogModel
                        {
                            MethodName = methodName,
                            Action = manualAction,
                            Source = CurrentUser.EmailAddress,
                            Comment = comment,
                            TargetName = nameof(Organisation.SecurityCode),
                            TargetNewValue = $"{failureMessage}{successMessage}"
                        });

                if (!test && securityCodeBulkWorkOutcome.ConcurrentBagOfSuccesses.Count > 0)
                {
                    SharedBusinessLogic.DataRepository.UpdateChangesInBulk(securityCodeBulkWorkOutcome
                        .ConcurrentBagOfSuccesses);
                    writer.WriteLine(Color.Green, "INFO: Changes saved to database");
                }

                result.TotalRecords = numberOfSuccesses + numberOfFailures;
                result.Count = numberOfSuccesses;
            }
            catch (Exception ex)
            {
                writer.WriteLine(Color.Red, $"ERROR: {ex.Message}");
            }

            return result;
        }

        #endregion
    }
}