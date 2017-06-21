using System.IO;
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
        }

        [Test]
        public void Test_payloads_load_correctly()
        {
            var small = GetSmallPayload();
            var large = GetLargePayload();

            Assert.AreEqual(2, small.Root.ChildRequests.Count);
            Assert.AreEqual(144, large.Root.ChildRequests.Count);
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
