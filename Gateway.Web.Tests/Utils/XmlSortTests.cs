using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bagl.Cib.MIT.Logging.Mock;
using Gateway.Web.Helpers;
using NUnit.Framework;

namespace Gateway.Web.Tests.Utils
{
    [TestFixture]
    public class XmlSortTests
    {
        [Test]
        public void Can_sort_xml()
        {
            var input = XElement.Parse(GetEmbeddedContent("UnsortedPayload.xml"));
            var expectation = GetEmbeddedContent("SortedPayload.xml");

            input = XmlSort.TrySortPayload(new MockLogger(), input);

            var result = input.ToString(SaveOptions.None);
            Assert.AreEqual(expectation, result);
        }

        [Test]
        public void Can_sort_empty_xml()
        {
            var input = new XElement("base");
            
            input = XmlSort.TrySortPayload(new MockLogger(), input);

            var result = input.ToString(SaveOptions.None);
            Assert.AreEqual("<base />", result);
        }

        [Test]
        public void Can_sort_null_xml()
        {
            XElement input = null;

            input = XmlSort.TrySortPayload(new MockLogger(), input);

            Assert.IsNull(input);
        }

        private string GetEmbeddedContent(string content)
        {
            var assembly = this.GetType().Assembly;
            using (var stream = assembly.GetManifestResourceStream(this.GetType(), content))
            {
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }
    }
}
