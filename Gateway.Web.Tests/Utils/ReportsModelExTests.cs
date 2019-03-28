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

            Assert.AreEqual(2, output.Columns);
            Assert.AreEqual("Add-In", output.ColumnDefinitions[0].Name);
            Assert.AreEqual("Version", output.ColumnDefinitions[1].Name);
            Assert.AreEqual(2, output.Rows);
            Assert.AreEqual("PnRLib", output[0, 0]);
            Assert.AreEqual("1.1.1", output[0, 1]);
            Assert.AreEqual("PnRLib", output[1, 0]);
            Assert.AreEqual("1.2.2", output[1, 1]);
        }

        private List<SelectListItem> GetAddInVersionReportModel()
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Value = "PnRLib|1.1.1" });
            result.Add(new SelectListItem() { Value = "PnRLib|1.2.2" });
            return result;
        }
    }
}
