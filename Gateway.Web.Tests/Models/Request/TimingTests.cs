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
            Assert.DoesNotThrow(() => GetLargePayload());
            Assert.DoesNotThrow(() => GetSimplePayload());
            Assert.DoesNotThrow(() => GetEmptyPayload());
        }

        [Test]
        public void Test_payloads_load_correctly()
        {
            var small = GetSmallPayload();
            var large = GetLargePayload();
            var simple = GetSimplePayload();
            var empty = GetEmptyPayload();

            Assert.AreEqual(4, small.Root.ChildRequests.Count);
            Assert.AreEqual(144, large.Root.ChildRequests.Count);
            Assert.AreEqual(1, simple.Items.Count);
            Assert.AreEqual(1, empty.Items.Count);
        }

        [Test]
        public void Totals_are_calculated_on_small_payload_on_construction()
        {
            var small = GetSmallPayload();

            Assert.AreEqual(38584m, small.TotalTimeMs);
        }

        [Test]
        public void Totals_are_calculated_on_large_payload_on_construction()
        {
            var large = GetLargePayload();

            Assert.AreEqual(472877m, large.TotalTimeMs);
        }

        [Test]
        public void Totals_are_calculated_on_simple_payload_on_construction()
        {
            var simple = GetSimplePayload();

            Assert.AreEqual(38584m, simple.TotalTimeMs);
        }

        [Test]
        public void Summary_total_is_calculated_on_small_payload()
        {
            var small = GetSmallPayload();

            var tradeStoreSummary = small.ControllerSummaries.First(f => f.Controller == "tradestore");
            Assert.AreEqual(22776m, tradeStoreSummary.TotalTimeMs);
        }

        [Test]
        public void Summary_average_is_calculated_on_small_payload()
        {
            var small = GetSmallPayload();

            var summary = small.ControllerSummaries.First(f => f.Controller == "tradestore");
            Assert.AreEqual(7592d, summary.AverageTimeMs);
        }

        [Test]
        public void Summary_average_with_no_size_is_calculated_on_small_payload()
        {
            var small = GetSmallPayload();

            var summary = small.ControllerSummaries.First(f => f.Controller == "riskbatch");
            Assert.AreEqual(38582, summary.AverageTimeMs);
        }

        [Test]
        public void Wallclock_is_difference_between_start_and_end_times_on_small_payload()
        {
            var small = GetSmallPayload();

            Assert.AreEqual("38s 584ms", small.WallClock);
        }

        [Test]
        public void Total_time_is_sequential_time_lapsed_on_small_payload()
        {
            var small = GetSmallPayload();

            Assert.AreEqual("54s 80ms", small.TotalTime);
        }

        [Test]
        public void Child_request_queue_time_is_valid_percentage_total_time_taken()
        {
            var small = GetSmallPayload();
            Assert.AreEqual(16m, small.Root.ChildRequests[0].QueueTime);
        }

        [Test]
        public void Child_request_processing_time_is_valid_percentage_total_time_taken()
        {
            var small = GetSmallPayload();
            Assert.AreEqual(5m, small.Root.ChildRequests[0].ProcessingTime);
        }

        [Test]
        public void Child_request_start_time_ms_is_difference_between_parent_start_and_child_start()
        {
            var small = GetSmallPayload();

            Assert.AreEqual(11324, small.Root.ChildRequests[0].StartTimeMs);
        }

        private Timings GetSmallPayload()
        {
            return GetPayload("Gateway.Web.Tests.Resources.SmallRequestPayload.xml");
        }

        private Timings GetSimplePayload()
        {
            return GetPayload("Gateway.Web.Tests.Resources.SimpleRequestPayload.xml");
        }

        private Timings GetLargePayload()
        {
            return GetPayload("Gateway.Web.Tests.Resources.LargeRequestPayload.xml");
        }

        private Timings GetEmptyPayload()
        {
            return GetPayload("Gateway.Web.Tests.Resources.EmptyRequestPayload.xml");
        }

        private Timings GetPayload(string resourceName)
        {
            var assembly = this.GetType().Assembly;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var payload = reader.ReadToEnd().DeserializeUsingDataContract<RequestPayload>();
                return new Timings(payload);
            }
        }
    }
}