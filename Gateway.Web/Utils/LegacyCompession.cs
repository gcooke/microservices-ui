using System;
using System.Text;
using Bagl.Cib.MSF.Contracts.Compression;
using Bagl.Cib.MSF.Contracts.Converters;
using Bagl.Cib.MSF.Contracts.Model;

namespace Gateway.Web.Utils
{
    public static class LegacyCompession
    {
        public static string CompressByGZip(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            byte[] buffer = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(buffer.CompressByGZip());
        }

        public static string DecompressByGZip(string compressedText)
        {
            if (string.IsNullOrEmpty(compressedText))
                return "";

            try
            {
                var gzBuffer = Convert.FromBase64String(compressedText);
                return Encoding.UTF8.GetString(gzBuffer.DecompressByGZip());
            }
            catch
            {
                return compressedText;
            }
        }

        public static string DecodeLegacyObject(Byte[] encodedBytes, string type)
        {
            string result;
            try
            {
                //New format
                var payloadTypeValue = (PayloadType) Enum.Parse((typeof(PayloadType)), type);
                var converter = DefaultPayloadConverters.GetDefault(payloadTypeValue);

                result = converter.ConvertForDisplay(encodedBytes);
            }
            catch
            {
                try
                {
                    //old format
                    var x = Encoding.UTF8.GetString(encodedBytes);
                    result = DecompressByGZip(x);
                }
                finally
                {
                }
            }

            return result;
        }
    }
}