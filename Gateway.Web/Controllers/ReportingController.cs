using System;
using System.Globalization;
using System.Web.Mvc;
using System.Xml.Linq;
using Gateway.Web.Database;
using Gateway.Web.Models.Reporting;
using Gateway.Web.Services;
using Controller = System.Web.Mvc.Controller;

namespace Gateway.Web.Controllers
{
    public class ReportingController : Controller
    {
        private readonly IGatewayDatabaseService _dataService;
        private readonly IGatewayService _gateway;

        public ReportingController(IGatewayDatabaseService dataService, IGatewayService gateway)
        {
            _dataService = dataService;
            _gateway = gateway;
        }

        public ActionResult Pfe(string date)
        {
            var model = new PfeModel();
            //var report = string.Format("pfe?site=GHANA&partysds=10051752&valDate={0}&dataset=JhbEoDNC", date);
            var report = string.Format("getxvarisks2?valDate={0}", date);
            var elements = _gateway.GetReport(report);
            foreach (var element in elements)
            {
                var item = new XvaResult();
                item.Site = element.Element("Site").Value;
                item.Counterparty = element.Element("Counterparty").Value;
                foreach (var point in elements.Descendants("Point"))
                {
                    var pfePoint = new PfePoint();
                    pfePoint.Date = point.Attribute("date").Value;
                    pfePoint.Value = point.Value;
                    item.Items.Add(pfePoint);
                }
                model.Items.Add(item);
            }

            return View(model);
        }
    }
}