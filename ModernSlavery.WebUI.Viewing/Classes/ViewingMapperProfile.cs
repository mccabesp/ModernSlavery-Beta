using AutoMapper;
using ModernSlavery.WebUI.Viewing.Models;

namespace ModernSlavery.WebUI.Viewing.Classes
{
    public class ViewingMapperProfile : Profile
    {
        public ViewingMapperProfile()
        {
            CreateMap<SearchResultsQuery, OrganisationSearchParameters>()
                .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => src.search))
                .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.p))
                .ForMember(dest => dest.FilterSectorTypeIds, opt => opt.MapFrom(src => src.s))
                .ForMember(dest => dest.FilterTurnoverRanges, opt => opt.MapFrom(src => src.tr))
                .ForMember(dest => dest.FilterReportedYears, opt => opt.MapFrom(src => src.y))
                .ForMember(dest => dest.FilterCodeIds, opt => opt.Ignore())
                .ForMember(dest => dest.SearchFields, opt => opt.Ignore())
                .ForMember(dest => dest.SearchMode, opt => opt.Ignore())
                .ForMember(dest => dest.PageSize, opt => opt.Ignore());
        }
    }
}