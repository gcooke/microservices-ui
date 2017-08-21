using System.IO;
using System.Linq;
using Gateway.Web.Models.Controllers;
using Gateway.Web.Utils;
using NUnit.Framework;

namespace Gateway.Web.Tests.Models.Controllers
{
    [TestFixture]
    public class CatalogueTest
    {
        private T GetPayload<T>(string resourceName = "Gateway.Web.Tests.Resources.CataloguePayload.xml")
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
            Assert.DoesNotThrow(() => GetPayload<Catalogue>());
        }

        [Test]
        public void Does_model_constructs_three_controllers()
        {
            var catalogue = GetPayload<Catalogue>();

            Assert.AreEqual(19, catalogue.Controllers.Count());
        } 
    }
}