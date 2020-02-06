using System;
using System.Threading.Tasks;
using AutoMapper;
using ModernSlavery.BusinessLogic.Account.Abstractions;
using ModernSlavery.BusinessLogic.Account.Models;
using ModernSlavery.Database;
using ModernSlavery.WebUI.Areas.Account.Abstractions;
using ModernSlavery.WebUI.Areas.Account.ViewModels;

namespace ModernSlavery.WebUI.Areas.Account.ViewServices
{

    public class ChangeDetailsViewService : IChangeDetailsViewService
    {

        public ChangeDetailsViewService(IUserRepository userRepo)
        {
            UserRepository = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        private IUserRepository UserRepository { get; }

        public async Task<bool> ChangeDetailsAsync(ChangeDetailsViewModel newDetails, User currentUser)
        {
            // map to business domain model
            var mappedDetails = Mapper.Map<UpdateDetailsModel>(newDetails);

            // execute update details
            return await UserRepository.UpdateDetailsAsync(currentUser, mappedDetails);
        }

    }

}
