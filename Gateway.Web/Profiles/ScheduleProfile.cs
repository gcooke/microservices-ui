using AutoMapper;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gateway.Web.Profiles
{
    public class ScheduleProfile : Profile
    {
        public ScheduleProfile()
        {
            CreateMap<spGetHistoryTimingForSchedule_Result, HistoryTimingForScheduleViewModel > ().ReverseMap();
        }

    }
}