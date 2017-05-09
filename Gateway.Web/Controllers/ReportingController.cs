using System;
using System.Globalization;
using System.Linq;
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
        private static string[] _sites = new[] { "" };
        private DateTime RootDate = new DateTime(2000, 01, 01);

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

            foreach (var site in _gateway.GetSites())
            {
                var siteModel = new PfeSite(site);
                var report = string.Format("getxvarisks?site={0}&dataset=JhbEoDNC&valDate={1}", site, date);
                var elements = _gateway.GetReport(report);
                foreach (var element in elements)
                {
                    var item = new XvaResult();
                    item.Site = site;
                    item.Counterparty = element.Element("Counterparty").Value;
                    var riskName = element.Element("RiskName").Value;
                    if (riskName != "PFE Upper") continue;

                    var dateVector = element.Descendants("DateVectorResult").FirstOrDefault().Value;
                    var valueVector = element.Descendants("ValueVectorResult").FirstOrDefault().Value;
                    var dates = dateVector.Split(',');
                    var values = valueVector.Split(',');
                    for (int index = 0; index < dates.Length - 1; index++)
                    {
                        var pfeDateLabel = dates[index];
                        var pfeDate = RootDate.AddDays(int.Parse(pfeDateLabel));
                        pfeDateLabel = pfeDate.ToString("dd MMM yyyy");

                        var pfePoint = item.Get(pfeDate);
                        pfePoint.Date = pfeDateLabel;
                        pfePoint.Value += double.Parse(values[index]);
                    }
                    siteModel.Items.Add(item);
                }
                //if (siteModel.Items.Count > 0)
                model.Items.Add(siteModel);
            }

            return View(model);
        }
    }
}