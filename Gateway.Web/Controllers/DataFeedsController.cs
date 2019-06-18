using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Authorization;
using Gateway.Web.Database;
using Gateway.Web.Models.DataFeeds;
using Gateway.Web.Services;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Gateway.Web.Controllers
{
    [RoleBasedAuthorize(Roles = "Security.View")]
    public class DataFeedsController : BaseController
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;
        private readonly ISystemInformation _information;
        private readonly IDataFeedService _dataFeedService;

        public DataFeedsController(
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

        public ActionResult Dashboard(DateTime? businessDate = null)
        {
            var model = new DataFeedDashboard();
            if (!businessDate.HasValue)
                businessDate = DateTime.Now;

            model.BusinessDate = businessDate.Value;
            var datafeeds = _dataFeedService.FetchDataFeedSummary(businessDate.Value);

            model.DataFeeds.AddRange(datafeeds.OrderBy(c => c.Name));
            return View(model);
        }
    }
}