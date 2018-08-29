using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Gateway.Web.Database;
using Gateway.Web.Models.Schedule;
using Gateway.Web.Models.Schedule.Input;

namespace Gateway.Web.Services.Schedule.Interfaces
{
    public interface IScheduleService<T> where T : BaseScheduleModel
    {
        IList<ModelErrorCollection> ScheduleBatches(T model);
    }
}
