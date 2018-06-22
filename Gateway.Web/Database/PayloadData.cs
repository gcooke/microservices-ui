using System;
using System.Text;
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

        public PayloadData(Payload payload)
        {
            _model = payload;
        }

        public string Direction { get { return _model.Direction; } }
        public string CompressionType { get { return _model.CompressionType; } }
        public string PayloadType { get { return _model.PayloadType; } }
        public byte[] Data { get { return _model.Data; } }
        public long? DataLengthBytes { get { return _model.DataLengthBytes; }}

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(LegacyCompession.DecodeObject(_model.Data, _model.PayloadType));
        }

        public byte[] GetCubeBytes()
        {
            var converter = DefaultPayloadConverters.GetDefault<Cube>();

            var cube = converter.ConvertFromPayload(_model.Data);

            return cube.ToBytes();
        }
    }
}