using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Gateway.Web.Services;
using Gateway.Web.Utils;
using NUnit.Framework;

namespace Gateway.Web.Tests.Models.Controllers
{
    [TestFixture]
    public class QueueModelTest
    {
        private IEnumerable<ControllerQueueInformation> GetPayload()
        {
            var document = LoadDocument();
            //TODO: Ideally  GetCurrentQueues(IEnumerable<ServerResponse>) method in GatewayService class should be called. The GatewayService is not fully tested due to it's design. The class should be refactor.
            foreach (var info in document.Descendants("Controller"))
            {
                var result = info.Deserialize<ControllerQueueInformation>();
                yield return result;
            }
        }

        private XDocument LoadDocument(
            string resourceName = "Gateway.Web.Tests.Resources.QueueModelPayload.xml")
        {
            var assembly = GetType().Assembly;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return XDocument.Load(reader);
            }
        }

        [Test]
        public void Can_load_payload()
        {
            Assert.DoesNotThrow(() => { GetPayload(); });
        }

        [Test]
        public void Does_model_constructs_1_controller()
        {
            var payload = GetPayload();
            Assert.AreEqual(1, payload.Count());
        }

        [Test]
        public void Does_model_constructs_1_queue()
        {
            var count = GetPayload().SelectMany(c => c.Versions).SelectMany(c => c.Queues).Count();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void Does_model_constructs_1_version()
        {
            var count = GetPayload().SelectMany(c => c.Versions).Count();
            Assert.AreEqual(1, count);
        }
    }
}