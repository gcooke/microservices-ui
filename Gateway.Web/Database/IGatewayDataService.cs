using System;
using Gateway.Web.Models;

namespace Gateway.Web.Database
{
    public interface IGatewayDataService
    {
        ControllersModel GetControllers(DateTime start);
        ControllerDetailModel GetControllerInfo(string name);
        ControllerRequestsSummaryModel GetControllerRequestSummary(string name, DateTime start);
    }
}