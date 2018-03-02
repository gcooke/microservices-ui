﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Xml.Linq;
using Bagl.Cib.MSF.Contracts.Compression;
using Bagl.Cib.MSF.Contracts.Converters;
using Bagl.Cib.MSF.Contracts.Model;
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

                try
                {
                    //New format
                    var payloadTypeValue = (PayloadType)Enum.Parse((typeof(PayloadType)), payloadType);
                    var converter = DefaultPayloadConverters.GetDefault(payloadTypeValue);

                    Data = converter.ConvertForDisplay(data);
                }
                catch
                {
                    try
                    {
                        //old format
                        Data = LegacyCompession.DecodeLegacyObject(data);
                    }
                    finally
                    {
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