using System;
using System.IO;
using System.Linq;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Utils;
using NUnit.Framework;

namespace Gateway.Web.Tests.Models.Controllers
{
    [TestFixture]
    public class GatewayInfoTests
    {
        private T GetPayload<T>(string resourceName = "Gateway.Web.Tests.Resources.GatewayInfoPayload.xml")
        {
            var assembly = GetType().Assembly;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd().Deserialize<T>();
            }
        }

        [Test]
        public void Can_load_payload()
        {
            Assert.DoesNotThrow(() => GetPayload<GatewayInfo>());
        }

        [Test]
        public void Does_model_constructs_4_nodes()
        {
            var gatewayInfo = GetPayload<GatewayInfo>();
            Assert.AreEqual(gatewayInfo.GatewayNodes.Length, 4);
        }

        [Test]
        public void Does_model_constructs_server_model()
        {
            var gatewayInfo = GetPayload<GatewayInfo>();
            var serverModel = new ServersModel(gatewayInfo);
            Assert.AreEqual(serverModel.Servers.Count(), 4);
        }

        [Test]
        public void Does_model_constructs_server_instance()
        {
            var gatewayInfo = GetPayload<GatewayInfo>();
            var serverModel = new ServersModel(gatewayInfo);
            var server = serverModel.Servers
                        .SingleOrDefault(node => node.Node.Equals("JHBPSM020000757", StringComparison.InvariantCultureIgnoreCase));

            Assert.IsNotNull(server);
        }

        [Test]
        public void Does_model_constructs_4_workers_for_757_node()
        {
            var gatewayInfo = GetPayload<GatewayInfo>();
            var serverModel = new ServersModel(gatewayInfo);
            var server = serverModel.Servers
                        .Single(node => node.Node.Equals("JHBPSM020000757", StringComparison.InvariantCultureIgnoreCase));

            Assert.AreEqual(5, server.Workers);
        }
    }
}