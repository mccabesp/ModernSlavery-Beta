using AutoMapper;
using ModernSlavery.WebUI.Models;
using ModernSlavery.WebUI.Models.Search;

namespace ModernSlavery.WebUI.Classes.Mappers
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
                .ForMember(dest => dest.SearchType, opt => opt.MapFrom(src => src.t));
        }

    }

}
