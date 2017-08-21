using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.Shared;
using Microsoft.Practices.ObjectBuilder2;

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