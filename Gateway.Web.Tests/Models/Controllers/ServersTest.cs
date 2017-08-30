using System.IO;
using System.Linq;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Utils;
using NUnit.Framework;

namespace Gateway.Web.Tests.Models.Controllers
{
    [TestFixture]
    public class ServersTest
    {
        private T GetPayload<T>(string resourceName = "Gateway.Web.Tests.Resources.RichServersModelPayload.xml")
        {
            var assembly = GetType().Assembly;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd().Deserialize<T>();
            }
        }

        [Test]
        public void Can_load_model()
        {
            Assert.DoesNotThrow(() =>
            {
                var gatewayInfos = GetPayload<GatewayInfo>();
                var serversModel = new ServersModel(gatewayInfos);
            });

            Assert.DoesNotThrow(() =>
            {
                var gatewayInfos = GetPayload<GatewayInfo>("Gateway.Web.Tests.Resources.ServersModelPayload.xml");
                var serversModel = new ServersModel(gatewayInfos);
                //ToList() required due to lazy loading.
                var servers = serversModel.Servers.ToList();
            });
        }

        [Test]
        public void Can_load_payload()
        {
            Assert.DoesNotThrow(() => GetPayload<GatewayInfo>());
        }

        [Test]
        public void Does_model_constructs_three_server_items()
        {
            var gatewayInfos = GetPayload<GatewayInfo>();
            var serversModel = new ServersModel(gatewayInfos);

            Assert.AreEqual(3, serversModel.Servers.Count());
        }

        [Test]
        public void Does_model_counts_the_number_of_queues_correctly()
        {
            var gatewayInfos = GetPayload<GatewayInfo>();
            var serversModel = new ServersModel(gatewayInfos);
            var server = serversModel.Servers.FirstOrDefault(svr => svr.Node == "jhbpsm020000757");
            Assert.IsNotNull(server);

            Assert.AreEqual(5, server.Queues);
        }

        [Test]
        public void Does_model_counts_the_number_of_workers_correctly()
        {
            var gatewayInfos = GetPayload<GatewayInfo>();
            var serversModel = new ServersModel(gatewayInfos);
            var server = serversModel.Servers.FirstOrDefault(svr => svr.Node == "jhbpsm020000757");
            Assert.IsNotNull(server);

            Assert.AreEqual(11, server.Workers);
        }

        [Test]
        public void Does_model_set_server_status_to_passing()
        {
            var gatewayInfos = GetPayload<GatewayInfo>();
            var serversModel = new ServersModel(gatewayInfos);
            var server = serversModel.Servers.FirstOrDefault(svr => svr.Node == "jhbpsm020000757");
            Assert.IsNotNull(server);
            Assert.AreEqual("success", server.Status);
        }
    }
}