using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Entities;
using ModernSlavery.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.Entities.Enums;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminUnconfirmedPinsController : BaseController
    {

        private readonly IDataRepository dataRepository;
        private readonly IOrganisationBusinessLogic organisationBusinessLogic;
        protected readonly INotificationService NotificationService;

        public AdminUnconfirmedPinsController(
            IOrganisationBusinessLogic organisationBusinessLogic,
            ILogger<AdminUnconfirmedPinsController> logger, IWebService webService, ICommonBusinessLogic commonBusinessLogic) : base(logger, webService, commonBusinessLogic)
        {
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

            if (userOrganisation.PINSentDate.Value.AddDays(CommonBusinessLogic.GlobalOptions.PinInPostExpiryDays) < VirtualDateTime.Now)
            {
                string newPin = organisationBusinessLogic.GeneratePINCode(false);
                userOrganisation.PIN = newPin;
            }

            userOrganisation.PINSentDate = VirtualDateTime.Now;
            await dataRepository.SaveChangesAsync();

            NotificationService.SendPinEmail(
                userOrganisation.User.EmailAddress,
                userOrganisation.PIN,
                userOrganisation.Organisation.OrganisationName);

            return View("../Admin/SendPinConfirmation", userOrganisation);
        }

    }

}
