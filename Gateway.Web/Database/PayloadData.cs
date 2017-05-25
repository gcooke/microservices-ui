using System.Text;
using Bagl.Cib.MSF.Contracts.Compression;

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
            if (_model.Direction == "Request")
            {
                switch (_model.PayloadType)
                {
                    case "Binary":
                        var bytes = _model.CompressionType == "GZIP" ? _model.Data.DecompressByGZip() : _model.Data;
                        return bytes;
                    default:
                        var x = Encoding.UTF8.GetString(_model.Data);
                        var str = _model.CompressionType == "GZIP" ? x.DecompressByGZip() : x;
                        return Encoding.Unicode.GetBytes(str);
                }
            }
            else
            {
                var bytes = _model.CompressionType == "GZIP" ? _model.Data.DecompressByGZip() : _model.Data;
                switch (_model.PayloadType)
                {
                    case "Binary":
                        return bytes;
                    default:
                        return bytes;
                }
            }
        }
    }
}