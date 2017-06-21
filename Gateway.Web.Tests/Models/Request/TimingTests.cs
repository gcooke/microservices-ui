using System;
using System.IO;
using System.Linq;
using Gateway.Web.Models.Request;
using Gateway.Web.Utils;
using NUnit.Framework;

namespace Gateway.Web.Tests.Models.Request
{
    [TestFixture]
    public class TimingTests
    {
        [Test]
        public void Can_load_payload()
        {
            Assert.DoesNotThrow(() => GetSmallPayload());
            Assert.DoesNotThrow(() => GetSimplePayload());
        }

        [Test]
        public void Test_payloads_load_correctly()
        {
            var small = GetSmallPayload();
            var large = GetLargePayload();
            var simple = GetSimplePayload();

            Assert.AreEqual(2, small.Root.ChildRequests.Count);
            Assert.AreEqual(144, large.Root.ChildRequests.Count);
            Assert.AreEqual(1, simple.Items.Count);
        }

        [Test]
        public void Totals_are_calculated_on_small_payload_on_construction()
        {
            var small = GetSmallPayload();

            Assert.AreEqual(38584, small.TotalTimeMs);
        }

        [Test]
        public void Totals_are_calculated_on_large_payload_on_construction()
        {
            var large = GetLargePayload();

            Assert.AreEqual(472877, large.TotalTimeMs);
        }

        [Test]
        public void Totals_are_calculated_on_simple_payload_on_construction()
        {
            var simple = GetSimplePayload();

            //var start = new DateTime(2017, 06, 20, 18, 00, 06).AddMilliseconds(033);

            //var end = new DateTime(2017, 06, 20, 18, 00, 44).AddMilliseconds(617);

            //var ms = (start - end).TotalMilliseconds;
            Assert.AreEqual(38584, simple.TotalTimeMs);
        }

        [Test]
        public void Summary_total_are_calculated_on_small_payload()
        {
            var small = GetSmallPayload();

            var tradeStoreSummary = small.ControllerSummaries.First(f => f.Controller == "tradestore");
            Assert.AreEqual(15490, tradeStoreSummary.TotalTimeMs);
        }

        [Test]
        public void Summary_average_are_calculated_on_small_payload()
        {
            var small = GetSmallPayload();

            var summary = small.ControllerSummaries.First(f => f.Controller == "tradestore");
            Assert.AreEqual(7745, summary.AverageTimeMs);
        }

        [Test]
        public void Summary_average_with_no_size_are_calculated_on_small_payload()
        {
            var small = GetSmallPayload();

            var summary = small.ControllerSummaries.First(f => f.Controller == "riskbatch");
            Assert.AreEqual(38582, summary.AverageTimeMs);
        }

        [Test]
        public void Wallclock_is_total_time_on_small_payload()
        {
            var small = GetSmallPayload();

            Assert.AreEqual("38 seconds, 584 milliseconds", small.WallClock);
        }

        [Test]
        public void Total_time_is_serial_time_lapsed_on_small_payload()
        {
            var small = GetSmallPayload();

            Assert.AreEqual("54 seconds, 80 milliseconds", small.TotalTime);
        }

        private Timings GetSmallPayload()
        {
            var assembly = this.GetType().Assembly;
            var resourceName = "Gateway.Web.Tests.Resources.SmallRequestPayload.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                var payload = reader.ReadToEnd().DeserializeUsingDataContract<RequestPayload>();
                return new Timings(payload);
            }
        }

        private Timings GetSimplePayload()
        {
            var assembly = this.GetType().Assembly;
            var resourceName = "Gateway.Web.Tests.Resources.SimpleRequestPayload.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                var payload = reader.ReadToEnd().DeserializeUsingDataContract<RequestPayload>();
                return new Timings(payload);
            }
        }

        private Timings GetLargePayload()
        {
            var assembly = this.GetType().Assembly;
            var resourceName = "Gateway.Web.Tests.Resources.LargeRequestPayload.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                var payload = reader.ReadToEnd().DeserializeUsingDataContract<RequestPayload>();
                return new Timings(payload);
            }
        }
    }
}