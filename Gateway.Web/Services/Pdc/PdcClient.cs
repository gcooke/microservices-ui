using Bagl.Cib.MIT.Logging;
using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Gateway.Web.Services.Pdc
{
    public class PdcClient
    {
        private readonly ILogger _logger;
        private TcpClient _tcpClient;

        public PdcClient(ILogger logger)
        {
            _logger = logger;
        }

        public SslStream Connect(string environment, string hostName, int hostPort, SslProtocols sslProtocols)
        {
            LogLine($"Attempting {sslProtocols.ToString()} connection to {hostName}:{hostPort}.");
            _tcpClient = new TcpClient();
            _tcpClient.Connect(hostName, hostPort);

            LogLine($"Connected to host {hostName}:{hostPort}.");
            var sslStream = new SslStream(_tcpClient.GetStream(), true, ValidateServerCertificate, null);

            LogLine($"Stream initialized to host {hostName}:{hostPort}.");
            var certificates = GetCertificates(environment);

            LogLine($"Attempting authentication with {hostName}:{hostPort}.");
            sslStream.AuthenticateAsClient(string.Empty, certificates, sslProtocols, false);

            LogLine($"Authenticated {hostName}:{hostPort}.");
            return sslStream;
        }

        public string Read(SslStream sslStream)
        {
            var bufferSize = 1024;
            // to hold read data
            var buffer = new byte[bufferSize];
            while (Array.IndexOf(buffer, Convert.ToByte('\n'), 0) < 0)
            {
                var readBytes = sslStream.Read(buffer, 0, buffer.Length);
            }

            var message = Encoding.ASCII.GetString(buffer, 0, bufferSize);
            LogLine(message);
            return message;
        }

        private X509CertificateCollection GetCertificates(string envName)
        {
            string certName = $"sigma.auth.service.{envName}";

            LogLine($"Opening local machine X509Store and attempting to retrieve certificate for {envName}.");
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var certificates = store.Certificates.Find(X509FindType.FindBySubjectName, certName, false);
            var message = certificates.Count > 0 ? "Certificate found" : "Certificate was not installed";
            LogLine(message);
            return certificates;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }

        private void LogLine(string line)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            _logger.Info($"{line}");
        }

        public void Close()
        {
            _tcpClient.Close();
            _tcpClient.Dispose();
        }
    }
}