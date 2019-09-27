using System.Web.Mvc;
using Bagl.Cib.MIT.Logging;
using Gateway.Web.Services.Batches.BatchCosts;

namespace Gateway.Web.Controllers
{
    public class BatchCostsController : BaseController
    {
        private readonly IBatchCostsService _batchCostsService;

        public BatchCostsController(
            ILoggingService loggingService,
            IBatchCostsService batchCostsService)
            : base(loggingService)
        {
            _batchCostsService = batchCostsService;
        }

        [HttpGet]
        public ActionResult GetBatchCosts()
        {
            var costsCube = _batchCostsService.GetBatchCosts();

            var costsMonthlyItems = _batchCostsService.GetBatchMonthlyCosts(costsCube);

            return View("Index", costsMonthlyItems);
        }
    }
}