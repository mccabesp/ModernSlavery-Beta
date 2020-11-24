using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.BusinessDomain.Registration
{
    public class PinInThePostService : IPinInThePostService
    {
        private const int _notifyAddressLineLength = 35;
        private readonly IEventLogger _customLogger;
        private readonly IGovNotifyAPI _govNotifyApi;

        private readonly SharedOptions _sharedOptions;

        public PinInThePostService(
            SharedOptions sharedOptions, 
            IEventLogger customLogger, 
            IGovNotifyAPI govNotifyApi)
        {
            _sharedOptions = sharedOptions;
            _customLogger = customLogger;
            _govNotifyApi = govNotifyApi;
        }

        public async Task<SendLetterResponse> SendPinInThePostAsync(UserOrganisation userOrganisation, string pin, string returnUrl)
        {
            var userFullNameAndJobTitle = $"{userOrganisation.User.Fullname} ({userOrganisation.User.JobTitle})";

            var companyName = userOrganisation.Organisation.OrganisationName;

            var address = GetAddressInFourLineFormat(userOrganisation.Organisation);

            var postCode = userOrganisation.Organisation.OrganisationAddresses.OrderByDescending(a => a.Modified).FirstOrDefault().PostCode;

            var pinExpiryDate = VirtualDateTime.Now.AddDays(_sharedOptions.PinInPostExpiryDays);

            var templateId = _sharedOptions.GovUkNotifyPinInThePostTemplateId;

            var personalisation = new Dictionary<string, dynamic>
            {
                {"address_line_1", userFullNameAndJobTitle},
                {"address_line_2", companyName},
                {"address_line_3", address.Count > 0 ? address[0] : ""},
                {"address_line_4", address.Count > 1 ? address[1] : ""},
                {"address_line_5", address.Count > 2 ? address[2] : ""},
                {"address_line_6", address.Count > 3 ? address[3] : ""},
                {"postcode", postCode},
                {"company", companyName},
                {"pin", pin},
                {"returnurl", returnUrl},
                {"expires", pinExpiryDate.ToString("d MMMM yyyy")}
            };

            return await _govNotifyApi.SendLetterAsync(templateId, personalisation);
        }

        public List<string> GetAddressInFourLineFormat(Organisation organisation)
        {
            var address = GetAddressComponentsWithoutRepeatsOrUnnecessaryComponents(organisation);

            ReduceAddressToAtMostFourLines(address);

            return address;
        }

        public List<string> GetAddressComponentsWithoutRepeatsOrUnnecessaryComponents(Organisation organisation)
        {
            var address = new List<string>();

            var latestAddress = organisation.OrganisationAddresses.OrderByDescending(a => a.Modified).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(latestAddress.PoBox))
            {
                var poBox = latestAddress.PoBox;
                if (!poBox.Contains("PO Box", StringComparison.OrdinalIgnoreCase)) poBox = $"PO Box {poBox}";

                address.Add("PO Box " + poBox);
            }

            if (!string.IsNullOrWhiteSpace(latestAddress.Address1)) address.Add(latestAddress.Address1);

            if (!string.IsNullOrWhiteSpace(latestAddress.Address2)) address.Add(latestAddress.Address2);

            if (!string.IsNullOrWhiteSpace(latestAddress.Address3)) address.Add(latestAddress.Address3);

            if (!string.IsNullOrWhiteSpace(latestAddress.TownCity)) address.Add(latestAddress.TownCity);

            if (!string.IsNullOrWhiteSpace(latestAddress.County)) address.Add(latestAddress.County);

            // Gov.UK Notify can only send post to the UK, so there's no need
            // to have 'UK' or 'United Kingdom' as part of the address
            if (!string.IsNullOrWhiteSpace(latestAddress.Country)
                && latestAddress.Country.ToUpper() != "UNITED KINGDOM"
                && latestAddress.Country.ToUpper() != "UK")
                address.Add(latestAddress.Country);

            return address;
        }

        private void ReduceAddressToAtMostFourLines(List<string> address)
        {
            // The address might be up to 7 lines, to start with,
            // so we might need to remove up to 3 lines
            for (var i = 0; i < 3; i++)
                if (address.Count > 4)
                    ReduceByOneLine(address);

            if (address.Count > 4)
            {
                // If the address is still more than 4 lines long, log an Error and chop off the end
                var originalAddress = address.ToList(); // Take a copy of the list for the log message
                address.RemoveRange(4, address.Count - 1);

                _customLogger.Error(
                    "PITP address is too long and has been reduced to 4 lines to fit on the Gov.UK Notify envelope",
                    new {OriginalAddress = originalAddress, ReducedAddress = address});
            }
        }

        private static void ReduceByOneLine(List<string> address)
        {
            // Loop through all pairs of lines, starting at the end of the address
            for (var currentLineNumber = address.Count - 2; currentLineNumber > 0; currentLineNumber--)
            {
                var currentLine = address[currentLineNumber];
                var nextLine = address[currentLineNumber + 1];

                if (currentLine.Length + nextLine.Length < _notifyAddressLineLength)
                {
                    var concatenatedLines = currentLine + ", " + nextLine;
                    address.RemoveRange(currentLineNumber, 2);
                    address.Insert(currentLineNumber, concatenatedLines);

                    // At this point, we've reduced the address by 1 line, so we can return
                    return;
                }
            }
        }
    }
}