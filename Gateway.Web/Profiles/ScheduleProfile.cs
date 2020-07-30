using AutoMapper;
using Gateway.Web.Database;
using Gateway.Web.Models.Controller;
using Gateway.Web.Models.Schedule.Output;

namespace Gateway.Web.Profiles
{
    public class ScheduleProfile : Profile
    {
        public ScheduleProfile()
        {
            CreateMap<spGetHistoryTimingForSchedule_Result, HistoryTimingForScheduleViewModel>().ReverseMap();
            CreateMap<spGetDeepDive_Result, DeepDiveDto>().ReverseMap();
            CreateMap<spGetPayloads_Result, spGetPayloadResponsesByController_Result>().ReverseMap();
        }
    }
}