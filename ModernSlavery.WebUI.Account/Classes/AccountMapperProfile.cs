using AutoMapper;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Account.Models;

namespace ModernSlavery.WebUI.Account.Classes
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