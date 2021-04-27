using System;
using System.Net;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Core.Classes.ErrorMessages
{
    public class InternalMessages
    {
        public static CustomError SameScopesCannotBeUpdated(ScopeStatuses newScopeStatus, ScopeStatuses oldScopeStatus,DateTime reportingDeadline)
        {
            return new CustomError(
                4006,
                $"Unable to update to {newScopeStatus} as the record for {reportingDeadline} is already showing as {oldScopeStatus}");
        }

        public static CustomError OrganisationRevertOnlyRetiredErrorMessage(string organisationName,
            string organisationReference,
            string status)
        {
            return new CustomError(
                4005,
                $"Only organisations with current status 'Retired' are allowed to be reverted, Organisation '{organisationName}' organisationReference '{organisationReference}' has status '{status}'.");
        }

        public static CustomError SecurityCodeMustExpireInFutureErrorMessage()
        {
            return new CustomError(4004, "Security code must expire in the future");
        }

        public static CustomError SecurityCodeCannotModifyAnAlreadyExpiredSecurityCodeErrorMessage()
        {
            return new CustomError(4004,
                "Cannot modify the security code information of an already expired security code");
        }

        public static CustomError SecurityCodeCreateIsOnlyAllowedToNonRetiredOrgsErrorMessage(string organisationName,
            string organisationReference,
            string status)
        {
            return new CustomError(
                4003,
                $"Generation of security codes cannot be performed for retired organisations. Organisation '{organisationName}' organisationReference '{organisationReference}' has status '{status}'.");
        }

        public static CustomError HttpBadRequestCausedByInvalidOrganisationIdentifier(string organisationIdentifier)
        {
            return new CustomError(HttpStatusCode.BadRequest, $"Bad organisation identifier {organisationIdentifier}");
        }

        public static CustomError HttpNotFoundCausedByOrganisationIdNotInDatabase(string organisationIdentifier)
        {
            return new CustomError(HttpStatusCode.NotFound,
                $"Organisation: Could not find organisation '{organisationIdentifier}'");
        }

        public static CustomError HttpGoneCausedByOrganisationBeingInactive(OrganisationStatuses organisationStatus)
        {
            return new CustomError(HttpStatusCode.Gone,
                $"Organisation: The status of this organisation is '{organisationStatus}'");
        }

        public static CustomError HttpNotFoundCausedByOrganisationReturnNotInDatabase(string organisationIdEncrypted,
            int year)
        {
            return new CustomError(
                HttpStatusCode.NotFound,
                $"Organisation: Could not find Modern Slavery Data for organisation:{organisationIdEncrypted} and year:{year}");
        }

        public static CustomError HttpGoneCausedByReportNotHavingBeenSubmitted(int reportYear, string reportStatus)
        {
            return new CustomError(HttpStatusCode.Gone,
                $"Organisation report '{reportYear}' is showing with status '{reportStatus}'");
        }

        public static CustomError HttpNotFoundCausedByReturnIdNotInDatabase(string returnIdEncrypted)
        {
            return new CustomError(HttpStatusCode.NotFound,
                $"Organisation: Could not find Modern Slavery Data for returnId:'{returnIdEncrypted}'");
        }
    }
}