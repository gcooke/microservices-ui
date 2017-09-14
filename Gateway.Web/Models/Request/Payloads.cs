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
    public class Payloads
    {
        public Payloads()
        {
            Items = new List<PayloadModel>();
        }

        public Guid CorrelationId { get; set; }
        public string Controller { get; set; }
        public string Version { get; set; }

        public List<PayloadModel> Items { get; private set; }
    }

    public class PayloadModel
    {
        public PayloadModel(string direction)
        {
            Id = 0;
            Data = "None";
            Direction = direction;
        }

        public PayloadModel(spGetPayloads_Result payload)
        {
            Id = payload.Id;
            Direction = payload.Direction;
            SetData(payload.Data, payload.DataLengthBytes, payload.CompressionType, payload.PayloadType);
            FormatData();
        }

        public long Id { get; set; }

        public string Data { get; set; }

        public string Direction { get; set; }

        public bool IsLarge { get; set; }

        private void SetData(byte[] data, long? lengthInBytes, string compressionType, string payloadType)
        {
            try
            {
                if (lengthInBytes >= 50000)
                {
                    Data = string.Format("Payload size {0:N2} kilobytes. Download below.", lengthInBytes / 10000m);
                    IsLarge = true;
                    return;
                }

                if (Direction == "Request")
                {
                    switch (payloadType)
                    {
                        case "Binary":
                            var bytes = compressionType == "GZIP" ? data.DecompressByGZip() : data;
                            Data = Convert.ToBase64String(bytes);
                            break;
                        default:
                            var x = Encoding.UTF8.GetString(data);
                            var str = compressionType == "GZIP" ? x.DecompressByGZip() : x;
                            Data = str;
                            break;
                    }
                }
                else
                {
                    var bytes = compressionType == "GZIP" ? data.DecompressByGZip() : data;
                    switch (payloadType)
                    {
                        case "Binary":
                            Data = Convert.ToBase64String(bytes);
                            break;
                        default:
                            Data = Encoding.UTF8.GetString(bytes);
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Data = string.Format("Could not decompress payload data: {0}", ex.Message);
            }
        }

        private void FormatData()
        {
            if (string.IsNullOrEmpty(Data)) return;

            try
            {
                if (Data.StartsWith("<"))
                    Data = XElement.Parse(Data).ToString(SaveOptions.None);

                if (Data.StartsWith("{"))
                    Data = JObject.Parse(Data).ToString(Formatting.Indented);
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