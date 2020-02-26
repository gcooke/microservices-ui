using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Xml.Linq;
using Bagl.Cib.MIT.Cube;
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

        public bool IsCube { get; set; }

        public bool IsXvaResult { get; set; }

        public bool ContainsCubeResult { get; set; }

        public bool ContainsXmlResult { get; set; }

        public string Data { get; set; }

        public string Direction { get; set; }

        public bool IsLarge { get; set; }

        public void SetData(byte[] data, long? lengthInBytes, string compressionType, string payloadType)
        {
            try
            {
                if (payloadType == "Cube")
                    IsCube = true;

                if (payloadType.ToLower() == "xelement" || payloadType.ToLower() == "string")
                    ContainsXmlResult = true;

                if (lengthInBytes >= 50000)
                {
                    Data = string.Format("Payload size {0:N2} kilobytes. Download below.", lengthInBytes / 10000m);
                    IsLarge = true;
                    return;
                }

                if (IsCube)
                {
                    Data = CubeBuilder.FromBytes(data).ToCsv().Replace(Environment.NewLine, "<br/>");
                }
                else
                {
                    Data = LegacyCompession.DecodeObject(data, payloadType);
                }

                IsXvaResult = IsXvaScenarioResult(Data, payloadType);

                if (Data?.Contains("<CubeResult") == true)
                    ContainsCubeResult = true;
            }
            catch (Exception ex)
            {
                Data = string.Format("Could not decompress payload data: {0}", ex.Message);
            }
        }

        private bool IsXvaScenarioResult(string data, string payloadType)
        {
            try
            {
                if (!payloadType.Equals("String", StringComparison.InvariantCultureIgnoreCase))
                    return false;

                var xml = XElement.Load(new StringReader(data));
                var scenarioResult = xml.Name.ToString().Equals("ScenarioResult", StringComparison.InvariantCultureIgnoreCase);

                return scenarioResult;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void FormatData()
        {
            if (string.IsNullOrEmpty(Data)) return;

            try
            {
                if (Data.StartsWith("<"))
                {
                    var element = XElement.Parse(Data);
                    Data = element.ToString(SaveOptions.None);

                    // Try parse QA cube result (decode the cube)
                    var cubeResult = element.Descendants("CubeResult").FirstOrDefault();
                    if (cubeResult != null)
                    {
                        var data = cubeResult.Attributes("data").First();
                        var encoded = Convert.FromBase64String(data.Value);
                        var cube = CubeBuilder.FromBytes(encoded);
                        data.Value = string.Empty;

                        // Move result to element
                        cubeResult.Value = cube.ToCsv();
                        Data = element.ToString(SaveOptions.None);
                    }
                }

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