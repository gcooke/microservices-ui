using System;
using System.Text;
using System.Xml.Linq;
using Bagl.Cib.MIT.Cube.Impl;
using Bagl.Cib.MSF.Contracts.Compression;
using Bagl.Cib.MSF.Contracts.Converters;
using Bagl.Cib.MSF.Contracts.Model;
using Gateway.Web.Utils;

namespace Gateway.Web.Database
{
    public class PayloadData
    {
        private readonly Payload _model;
        private string _extension;

        public PayloadData(Payload payload)
        {
            _model = payload;

            var payloadTypeValue = (PayloadType)Enum.Parse((typeof(PayloadType)), _model.PayloadType);
            switch (payloadTypeValue)
            {
                case Bagl.Cib.MSF.Contracts.Model.PayloadType.XElement:
                    _extension = "xml";
                    break;
                case Bagl.Cib.MSF.Contracts.Model.PayloadType.JObject:
                    _extension = "txt";
                    break;
                case Bagl.Cib.MSF.Contracts.Model.PayloadType.String:
                    _extension = "txt";
                    break;
                case Bagl.Cib.MSF.Contracts.Model.PayloadType.Binary:
                    _extension = "dat";
                    break;
                case Bagl.Cib.MSF.Contracts.Model.PayloadType.Cube:
                    _extension = "dat";
                    break;
                case Bagl.Cib.MSF.Contracts.Model.PayloadType.MultiPart:
                    _extension = "xml";
                    break;
                case Bagl.Cib.MSF.Contracts.Model.PayloadType.Unknown:
                default:
                    _extension = "txt";
                    break;
            }
        }

        public string Direction { get { return _model.Direction; } }
        public string CompressionType { get { return _model.CompressionType; } }
        public string PayloadType { get { return _model.PayloadType; } }
        public byte[] Data { get { return _model.Data; } }
        public long? DataLengthBytes { get { return _model.DataLengthBytes; } }

        public byte[] GetBytes()
        {
            var data = LegacyCompession.DecodeObject(_model.Data, _model.PayloadType);

            // Fix for xml that is stored as string without line breaks. 
            if (_model.PayloadType == "String" && data.Length > 0 && data.StartsWith("<"))
            {
                try
                {
                    var element = XElement.Parse(data);
                    data = element.ToString(SaveOptions.None);
                    _extension = "xml";
                }
                catch (Exception ex)
                {
                    // Do nothing
                }
            }
            return Encoding.UTF8.GetBytes(data);
        }

        public string GetExtension()
        {
            return _extension;
        }

        public byte[] GetCubeBytes()
        {
            var converter = DefaultPayloadConverters.GetDefault<Cube>();

            var cube = converter.ConvertFromPayload(_model.Data);

            return cube.ToBytes();
        }
    }
}