using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Xml.Linq;
using Bagl.Cib.MSF.Contracts.Compression;
using Gateway.Web.Database;
using Gateway.Web.Models.Controller;
using Gateway.Web.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

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
        public PayloadModel(spGetPayloads_Result payload)
        {
            Direction = payload.Direction;
            SetData(payload.Data, payload.DataLengthBytes, payload.CompressionType, payload.PayloadType);
            FormatData();
        }

        public string Data { get; set; }

        public string Direction { get; set; }

        public bool IsLarge { get; set; }

        private void SetData(byte[] data, long? lengthInBytes, string compressionType, string payloadType)
        {
            try
            {
                if (lengthInBytes >= 50000)
                {
                    Data = string.Format("Payload size {0:N2} kilobytes", data.Length / 10000m);
                    IsLarge = true;
                    return;
                }

                var bytes = compressionType == "GZIP" ? data.DecompressByGZip() : data;
                switch (payloadType)
                {
                    case "XElement":
                        Data = GetXElement(bytes).ToString(SaveOptions.None);
                        break;
                    case "JObject":
                        Data = GetJObject(bytes).ToString(Formatting.Indented);
                        break;
                    case "String":
                        Data = Encoding.Unicode.GetString(bytes);
                        break;
                    case "Binary":
                    default:
                        Data = Convert.ToBase64String(bytes);
                        break;
                }
            }
            catch (Exception ex)
            {
                Data = string.Format("Could not decompress data: {0}", ex.Message);
                throw;
            }
        }

        private static XElement GetXElement(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return XElement.Load(ms);
            }
        }

        private static JObject GetJObject(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BsonReader br = new BsonReader(ms))
                {
                    return JObject.Load(br);
                }
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