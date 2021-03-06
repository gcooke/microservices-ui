﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

        private static bool TryGzipUncompressFromBase64(string compressedText, Encoding finalEncoding, out string result)
        {
            result = string.Empty;
            if (string.IsNullOrEmpty(compressedText))
                return true;

            try
            {
                var gzBuffer = Convert.FromBase64String(compressedText);
                var bytes = gzBuffer.DecompressByGZip();
                result = finalEncoding.GetString(bytes);
                return true;
            }
            catch (Exception ex)
            {
                result = compressedText;
                return false;
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

        public static string DecodeObject(Byte[] encodedBytes, string type)
        {
            if (encodedBytes == null || encodedBytes.Length == 0) return null;

            if (IsGzipCompressed(encodedBytes))
            {
                var payloadTypeValue = (PayloadType)Enum.Parse((typeof(PayloadType)), type);
                var converter = DefaultPayloadConverters.GetDefault(payloadTypeValue);
                return converter.ConvertForDisplay(encodedBytes);
            }

            var text = Encoding.UTF8.GetString(encodedBytes);
            string result;
            if (TryGzipUncompressFromBase64(text, Encoding.Unicode, out result))
                return result;
            if (TryGzipUncompressFromBase64(text, Encoding.UTF8, out result))
                return result;

            return text;
        }

        private static bool IsGzipCompressed(byte[] data)
        {
            var magicNumberBytes = data.Skip(4).Take(4).ToArray();
            if (magicNumberBytes.Length <= 0) return false;
            var magicNumber = BitConverter.ToInt32(magicNumberBytes, 0);
            magicNumber &= 0x00FFFFFF;
            return magicNumber == 0x00088B1F;
        }
    }
}