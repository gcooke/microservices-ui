using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Gateway.Web.Database;
using Gateway.Web.Utils;

namespace Gateway.Web.Models.Controller
{
    public class RequestModel : BaseControllerModel
    {
        public RequestModel(string name) : base(name)
        {
            Items = new List<PayloadModel>();
        }

        public List<PayloadModel> Items { get; private set; }
    }

    public class PayloadModel
    {
        public PayloadModel(Payload payload)
        {
            Direction = payload.Direction;
            SetData(payload.Data, payload.CompressionType);
            FormatData();
        }

        public string Data { get; set; }

        public string Direction { get; set; }

        private void SetData(string data, string compression)
        {
            try
            {
                if (compression == "gzip")
                    Data = Compression.UnCompressGZip(data);
                else
                    Data = data;
            }
            catch (Exception ex)
            {
                Data = string.Format("Could not decompress data: {0}", ex.Message);
                throw;
            }
        }

        private void FormatData()
        {
            if (string.IsNullOrEmpty(Data)) return;
            if (!Data.StartsWith("<")) return;

            try
            {
                Data = XElement.Parse(Data).ToString(SaveOptions.None);
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}