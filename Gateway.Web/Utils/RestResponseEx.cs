using System.Xml.Linq;
using Bagl.Cib.MSF.ClientAPI.Gateway;

namespace Gateway.Web.Utils
{
    public static class RestResponseEx
    {
        public static XElement ToXmlDocument(this RestResponse response)
        {
            return (response.Successfull) ? response.Content.GetPayloadAsXElement() : null;
        } 
    }
}