using System;
using Bagl.Cib.MIT.IO;
using Gateway.Web.Database;

namespace Gateway.Web.Helpers
{
    public static class PayloadFileRetrieval
    {
        public const string PayloadSeverPathFormat = "\\\\{server}\\DATA\\Audit\\{requestdate}\\{direction}\\{correlationid}";

        public static byte[] GetPayloadFromSever(IFileService fileService, spGetPayloads_Result payload)
        {
            return GetPayloadFromFile(fileService, payload.CorrelationId, payload.Direction, payload.UpdateTime, payload.Server, payload.DataLengthBytes);
        }

        public static byte[] GetPayloadFromFile(IFileService fileService, Guid CorrelationId, string direction, DateTime UpdateTime, string server, long? dataLengthBytes)
        {
            var path = PayloadSeverPathFormat
                .Replace("{server}", server)
                .Replace("{requestdate}", UpdateTime.ToString("yyyyMMdd"))
                .Replace("{direction}", direction)
                .Replace("{correlationid}", $"{CorrelationId}");

            if (!fileService.FileExists(path))
                return null;

            if (dataLengthBytes.HasValue)
            {
                var totalbytes = (int)dataLengthBytes.Value;
                var data = new byte[totalbytes];
                var numBytesRead = 0;

                using (var stream = fileService.OpenRead(path))
                {
                    var readbytes = 0;

                    while ((readbytes = stream.Read(data, numBytesRead, totalbytes - numBytesRead)) > 0)
                    {
                        numBytesRead += readbytes;
                    }
                }

                return data;
            }

            // Endless Read ?
            return null;
        }

        public static byte[] GetPayloadFromSever(IFileService fileService, Payload payload)
        {
            return GetPayloadFromFile(fileService, payload.CorrelationId, payload.Direction, payload.UpdateTime, payload.Server, payload.DataLengthBytes);
        }
    }
}