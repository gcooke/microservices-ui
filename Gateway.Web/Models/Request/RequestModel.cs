using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Gateway.Web.Database;
using Gateway.Web.Models.Controller;
using Gateway.Web.Utils;

namespace Gateway.Web.Models.Request
{
    public class RequestModel
    {
        public RequestModel()
        {
            Items = new List<PayloadModel>();
        }

        public Guid CorrelationId { get; set; }
        public Guid ParentCorrelationId { get; set; }
        public string User { get; set; }
        public string IpAddress { get; set; }
        public string Controller { get; set; }
        public string Version { get; set; }
        public string Resource { get; set; }
        public string RequestType { get; set; }
        public int Priority { get; set; }
        public bool IsAsync { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public int QueueTimeMs { get; set; }
        public int TimeTakenMs { get; set; }
        public int ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public DateTime UpdateTime { get; set; }

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

            try
            {
                if (Data.StartsWith("<"))
                    Data = XElement.Parse(Data).ToString(SaveOptions.None);
            }
            catch (Exception)
            {
                return;
            }

            try
            {
                    Data = Data.Replace("&gt;", ">");
                    Data = Data.Replace("&lt;", "<");
                    Data = Data.Replace("\\r\\n", Environment.NewLine);
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}