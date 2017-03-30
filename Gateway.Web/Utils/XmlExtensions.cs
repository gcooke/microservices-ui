using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace Gateway.Web.Utils
{
    public static class XmlExtensions
    {
        public static string FetchControllerInfo(string server, string name)
        {
            var url1 = "http://" + server + ":7010/health/services/" + name;
            return FetchXml(url1);
        }

        public static string FetchQueueInfo(string server, string name)
        {
            var url1 = "http://" + server + ":7010/health/queues/" + name;
            return FetchXml(url1);
        }

        private static string FetchXml(string url)
        {
            string xml = null;
            var client = new WebClient();
            try
            {
                client.Headers.Add("accept", "application/xml");
                client.UseDefaultCredentials = true;
                xml = client.DownloadString(url);
                xml = XmlExtensions.FormatXml(xml);
            }
            catch (WebException ex)
            {
                xml = "Could not fetch data: " + ex.Message;
            }
            return xml;
        }

        public static string FormatXml(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return xml;

            var xd = new XmlDocument();
            xd.LoadXml(xml);
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            XmlTextWriter xtw = null;
            try
            {
                xtw = new XmlTextWriter(sw);
                xtw.Formatting = Formatting.Indented;
                xd.WriteTo(xtw);
            }
            finally
            {
                if (xtw != null)
                    xtw.Close();
            }
            return sb.ToString();
        }
    }
}