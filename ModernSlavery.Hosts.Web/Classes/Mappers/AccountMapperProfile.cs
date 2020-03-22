using AutoMapper;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Areas.Account.ViewModels;
using ModernSlavery.WebUI.Areas.Account.ViewModels.ChangeDetails;
using ModernSlavery.WebUI.Areas.Account.ViewModels.ManageAccount;

namespace ModernSlavery.WebUI.Classes.Mappers
{

    public class AccountMapperProfile : Profile
    {
        public AccountMapperProfile()
        {
            CreateMap<User, ManageAccountViewModel>();
            CreateMap<User, ChangeDetailsViewModel>();
            CreateMap<User, UpdateDetailsModel>();
            CreateMap<ChangeDetailsViewModel, UpdateDetailsModel>();
        }

    }

}
