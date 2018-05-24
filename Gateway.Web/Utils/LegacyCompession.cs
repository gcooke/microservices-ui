using System;
using System.IO;
using System.IO.Compression;
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

        public static string GetUncompressedString(this byte[] compressed)
        {
            var gzBuffer = compressed;
            using (var ms = new MemoryStream())
            {
                var msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                var buffer = new byte[msgLength];
                ms.Position = 0;
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
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