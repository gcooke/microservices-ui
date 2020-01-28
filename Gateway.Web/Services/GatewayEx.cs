using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Bagl.Cib.MSF.ClientAPI.Model;

namespace Gateway.Web.Services
{
    public static class GatewayEx
    {
        public static async Task<GatewayResponse<XElement>> GetSync(this Bagl.Cib.MSF.ClientAPI.Gateway.IGateway gateway, string controller, string query, string version)
        {
            var get = new Get(controller) {Query = query};

            if (!string.IsNullOrWhiteSpace(version))
                get.Version = version;

            get.AddHeader("IsAsync", "false");
            return await gateway.Invoke<XElement>(get);
        }

        public static async Task<GatewayResponse<string>> PutSync(this Bagl.Cib.MSF.ClientAPI.Gateway.IGateway gateway, string controller, string query, string version, string content)
        {
            var put = new Put(controller) {Query = query};
            if (!string.IsNullOrWhiteSpace(version))
                put.Version = version;
            put.SetBody(content);

            put.AddHeader("IsAsync", "false");
            return await gateway.Invoke<string>(put);
        }

        public static async Task<GatewayResponse<string>> DeleteSync(this Bagl.Cib.MSF.ClientAPI.Gateway.IGateway gateway, string controller, string query, string version)
        {
            var delete = new Delete(controller) { Query = query };
            if (!string.IsNullOrWhiteSpace(version))
                delete.Version = version;

            delete.AddHeader("IsAsync", "false");
            return await gateway.Invoke<string>(delete);
        }
    }
}