using AutoMapper;
using ModernSlavery.WebUI.Viewing.Models;
using ModernSlavery.WebUI.Viewing.Models.Search;

namespace ModernSlavery.WebUI.Viewing.Classes
{

    public class ViewingMapperProfile : Profile
    {
        public ViewingMapperProfile()
        {
            CreateMap<SearchResultsQuery, EmployerSearchParameters>()
                .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => src.search))
                .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.p))
                .ForMember(dest => dest.FilterSicSectionIds, opt => opt.MapFrom(src => src.s))
                .ForMember(dest => dest.FilterEmployerSizes, opt => opt.MapFrom(src => src.es))
                .ForMember(dest => dest.FilterReportedYears, opt => opt.MapFrom(src => src.y))
                .ForMember(dest => dest.FilterReportingStatus, opt => opt.MapFrom(src => src.st))
                .ForMember(dest => dest.FilterCodeIds, opt => opt.Ignore())
                .ForMember(dest => dest.SearchFields, opt => opt.Ignore())
                .ForMember(dest => dest.SearchMode, opt => opt.Ignore())
                .ForMember(dest => dest.PageSize, opt => opt.Ignore())
                .ForMember(dest => dest.SearchType, opt => opt.MapFrom(src => src.t));

        }

    }

}
