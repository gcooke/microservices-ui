using System;
using System.Text;
using Bagl.Cib.MSF.Contracts.Compression;

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

            var gzBuffer = Convert.FromBase64String(compressedText);
            return Encoding.UTF8.GetString(gzBuffer.DecompressByGZip());
        }

        public static string DecodeLegacyObject(Byte[] encodedBytes)
        {
            var x = Encoding.UTF8.GetString(encodedBytes);
            return DecompressByGZip(x); ;
        }
    }
}