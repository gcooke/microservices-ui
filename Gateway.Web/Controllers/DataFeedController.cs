using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Database;
using Gateway.Web.Models.DataFeeds;
using Gateway.Web.Services;
using System;
using System.Web.Mvc;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class DataFeedController : BaseController
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;
        private readonly ISystemInformation _information;
        private readonly IDataFeedService _dataFeedService;

        public DataFeedController(
            IGatewayDatabaseService dataService,
            IGatewayService gateway,
            ILoggingService loggingService,
            ISystemInformation information,
            IDataFeedService dataFeedService)
            : base(loggingService)
        {
            _dataService = dataService;
            _gateway = gateway;
            _information = information;
            _dataFeedService = dataFeedService;
        }

        public ActionResult DashboardDetail(long? id = null, DateTime? businessDate = null)
        {
            DataFeedHeader datafeeds = null;
            var model = new DataFeedDashboardItem();
            if (!businessDate.HasValue)
                businessDate = DateTime.Now;

            model.RunDate = businessDate.Value;
            if (id.HasValue)
            {
                datafeeds = _dataFeedService.FetchDataFeedByHeaderId(id.Value);
                if (datafeeds != null)
                    ViewBag.DataFeedTypeId = datafeeds.DataFeedTypeId;
            }
            else
            {
                if (businessDate.HasValue && ViewBag.DataFeedTypeId != null)
                {
                    datafeeds = _dataFeedService.FetchDataFeedByRunDateAndType(businessDate.Value, (int)ViewBag.DataFeedTypeId);
                }
            }

            model.DataFeedHeader = datafeeds;
            return View(model);
        }
    }
}