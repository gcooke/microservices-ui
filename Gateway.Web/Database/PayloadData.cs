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

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(LegacyCompession.DecodeLegacyObject(_model.Data, _model.PayloadType));
        }

        public byte[] GetCubeBytes()
        {
            var converter = DefaultPayloadConverters.GetDefault<Cube>();

            var cube = converter.ConvertFromPayload(_model.Data);

            return cube.ToBytes();
        }
    }
}