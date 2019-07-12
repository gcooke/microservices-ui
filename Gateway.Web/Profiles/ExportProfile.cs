using Absa.Cib.MIT.TaskScheduling.Models;
using AutoMapper;
using Gateway.Web.Models.Export;

namespace Gateway.Web.Profiles
{
    public class ExportProfile : Profile
    {
        public ExportProfile()
        {
            CreateMap<Models.Export.ArgumentDto.Argument, Argument>().ReverseMap();
            CreateMap<CubeToCsvDestinationInformation, DestinationInfoViewModel>().ReverseMap();
            CreateMap<CubeToCsvSourceInformation, SourceInformationViewModel>().ReverseMap();
            CreateMap<FileExport, ExportUpdateViewModel>()
                .ForMember(x => x.ExportId, opt => opt.MapFrom(o => o.Id))
                .ForMember(x => x.DestinationInformation, opt => opt.Ignore())
                .ForMember(x => x.SourceInformation, opt => opt.Ignore())
                .ForMember(x => x.ExportTypes, opt => opt.Ignore())

                .ReverseMap();
        }
    }
}