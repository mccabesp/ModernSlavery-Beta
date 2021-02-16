using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoleNames.SuperAdmin)]
    [Route("admin")]
    public class AdminUnconfirmedPinsController : BaseController
    {
        private readonly IAdminService _adminService;
        public AdminUnconfirmedPinsController(
            IAdminService adminService,
            ILogger<AdminUnconfirmedPinsController> logger, IWebService webService,
            ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _adminService = adminService;
        }

        [HttpGet("unconfirmed-pins")]
        public async Task<IActionResult> UnconfirmedPins()
        {
            var model = await _adminService.SharedBusinessLogic.DataRepository.GetAll<UserOrganisation>()
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
            var userOrganisation = await _adminService.SharedBusinessLogic.DataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.UserId == userId && uo.OrganisationId == organisationId);

            return View("../Admin/SendPinWarning", userOrganisation);
        }

        [HttpPost("send-pin")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPin(long userId, long organisationId)
        {
            var userOrganisation = await _adminService.SharedBusinessLogic.DataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.UserId == userId && uo.OrganisationId == organisationId);

            if (userOrganisation.PINSentDate.Value.AddDays(SharedBusinessLogic.SharedOptions.PinInPostExpiryDays) <
                VirtualDateTime.Now)
            {
                var newPin = _adminService.OrganisationBusinessLogic.GeneratePINCode();
                userOrganisation.PIN = newPin;
                userOrganisation.PINHash = Crypto.GetSHA512Checksum(newPin);
            }

            userOrganisation.PINSentDate = VirtualDateTime.Now;
            await _adminService.SharedBusinessLogic.DataRepository.SaveChangesAsync();

            _adminService.SharedBusinessLogic.NotificationService.SendPinEmail(
                userOrganisation.User.EmailAddress,
                userOrganisation.PIN,
                userOrganisation.Organisation.OrganisationName);

            return View("../Admin/SendPinConfirmation", userOrganisation);
        }
    }
}