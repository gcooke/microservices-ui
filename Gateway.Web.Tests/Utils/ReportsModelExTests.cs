using System.Collections.Generic;
using System.Web.Mvc;
using Gateway.Web.Utils;
using NUnit.Framework;

namespace Gateway.Web.Tests.Utils
{
    [TestFixture]
    public class ReportsModelExTests
    {
        [Test]
        public void Can_deserialize_report_model()
        {
            Assert.DoesNotThrow(() => GetAddInVersionReportModel());
        }

        [Test]
        public void Can_summarize_version_report()
        {
            var input = GetAddInVersionReportModel();

            var output = input.ConvertToReportTable();

            Assert.AreEqual(2, output.Columns.Count);
            Assert.AreEqual("Add-In", output.Columns[0]);
            Assert.AreEqual("Version", output.Columns[1]);
            Assert.AreEqual(2, output.Rows.Count);
            Assert.AreEqual("PnRLib", output.Rows[0].Values[0]);
            Assert.AreEqual("1.1.1", output.Rows[0].Values[1]);
            Assert.AreEqual("PnRLib", output.Rows[1].Values[0]);
            Assert.AreEqual("1.2.2", output.Rows[1].Values[1]);
        }

        private List<SelectListItem> GetAddInVersionReportModel()
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Value = "PnRLib|1.1.1"});
            result.Add(new SelectListItem() { Value = "PnRLib|1.2.2"});
            return result;
        }
    }
}
