using AutoMapper;
using ModernSlavery.Core.Models;
using ModernSlavery.Entities;
using ModernSlavery.WebUI.Areas.Account.ViewModels;

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
