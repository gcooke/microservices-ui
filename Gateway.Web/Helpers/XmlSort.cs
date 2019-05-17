using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Bagl.Cib.MIT.Logging;

namespace Gateway.Web.Helpers
{
    public static class XmlSort
    {
        public static XElement TrySortPayload(ILogger logger, XElement source)
        {
            try
            {
                // Only sort from level 2 down
                foreach (var level1 in source.Elements())
                    foreach (var level2 in level1.Elements())
                    {
                        XmlSort.Sort(level2);
                    }

                return source;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to sort xml");
                return source;
            }
        }

        public static void Sort(XElement source, bool bSortAttributes = true)
        {
            //Make sure there is a valid source
            if (source == null) throw new ArgumentNullException("source");

            //Sort attributes if needed
            if (bSortAttributes)
            {
                List<XAttribute> sortedAttributes = source.Attributes().OrderBy(a => a.ToString()).ToList();
                sortedAttributes.ForEach(a => a.Remove());
                sortedAttributes.ForEach(a => source.Add(a));
            }

            //Sort the children IF any exist
            List<XElement> sortedChildren = source.Elements()
                .OrderBy(GetElementSortName).ToList();
            if (source.HasElements)
            {
                source.RemoveNodes();
                sortedChildren.ForEach(c => Sort(c));
                sortedChildren.ForEach(c => source.Add(c));
            }
        }

        private static string GetElementSortName(XElement element)
        {
            // The sortable name is [ElementName].[FirstChildName].[Attributes].[FirstChildAttributes]
            var elementName = GetElementName(element);
            var attributes = GetElementAttributeString(element);
            var firstChild = element.Elements().FirstOrDefault();
            var firstChildName = GetElementName(firstChild);
            var firstChildAttributes = GetElementAttributeString(firstChild);

            return $"{elementName}.{firstChildName}.{attributes}.{firstChildAttributes}";
        }

        private static string GetElementName(XElement element)
        {
            if (element == null) return string.Empty;
            return element.Attributes("Name").Any()
                ? element.Attributes("Name").First().Value
                : element.Name.LocalName;
        }

        private static string GetElementAttributeString(XElement element)
        {
            if (element == null) return string.Empty;
            return string.Join(",", element.Attributes());
        }
    }
}