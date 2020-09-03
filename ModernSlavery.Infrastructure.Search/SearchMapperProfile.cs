using AutoMapper;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Infrastructure.Search
{
    public class SearchMapperProfile : Profile
    {
        public SearchMapperProfile()
        {
            CreateMap<OrganisationSearchModel,AzureOrganisationSearchModel>();
        }
    }
}