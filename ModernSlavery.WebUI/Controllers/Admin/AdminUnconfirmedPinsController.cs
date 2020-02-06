using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic;
using ModernSlavery.Core;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Database;
using ModernSlavery.Extensions;
using ModernSlavery.WebUI.Classes;
using ModernSlavery.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ModernSlavery.WebUI.Controllers.Admin
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminUnconfirmedPinsController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly IOrganisationBusinessLogic organisationBusinessLogic;

        public AdminUnconfirmedPinsController(
            IDataRepository dataRepository,
            IOrganisationBusinessLogic organisationBusinessLogic
        )
        {
            this.dataRepository = dataRepository;
            this.organisationBusinessLogic = organisationBusinessLogic;
        }

        [HttpGet("unconfirmed-pins")]
        public async Task<IActionResult> UnconfirmedPins()
        {
            List<UserOrganisation> model = await dataRepository.GetAll<UserOrganisation>()
                .Where(uo => uo.Method == RegistrationMethods.PinInPost)
                .Where(uo => uo.PINConfirmedDate == null)
                .Where(uo => uo.PIN != null)
                .OrderByDescending(uo => uo.PINConfirmedDate.Value)
                .ToListAsync();
            return View("../Admin/UnconfirmedPins", model);
        }

        [HttpGet("send-pin")]
        public async Task<IActionResult> SendPinWarning(long userId, long organisationId)
        {
            UserOrganisation userOrganisation = await dataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.UserId == userId && uo.OrganisationId == organisationId);

            return View("../Admin/SendPinWarning", userOrganisation);
        }

        [HttpPost("send-pin")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPin(long userId, long organisationId)
        {
            UserOrganisation userOrganisation = await dataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.UserId == userId && uo.OrganisationId == organisationId);

            if (userOrganisation.PINSentDate.Value.AddDays(Global.PinInPostExpiryDays) < VirtualDateTime.Now)
            {
                string newPin = organisationBusinessLogic.GeneratePINCode(false);
                userOrganisation.PIN = newPin;
            }

            userOrganisation.PINSentDate = VirtualDateTime.Now;
            await dataRepository.SaveChangesAsync();

            EmailSendingService.SendPinEmail(
                userOrganisation.User.EmailAddress,
                userOrganisation.PIN,
                userOrganisation.Organisation.OrganisationName);

            return View("../Admin/SendPinConfirmation", userOrganisation);
        }

    }

}
