using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using Bagl.Cib.MIT.Logging;

namespace Gateway.Web.Services
{
    public class BasicRestService : IBasicRestService
    {
        private readonly TimeSpan _defaultRequestTimeout;
        private readonly int _port = 7010;
        private readonly ILogger _logger;

        public BasicRestService(ILoggingService loggingService)
        {
            _defaultRequestTimeout = TimeSpan.FromSeconds(10);
            _logger = loggingService.GetLogger(this);
        }

        public XDocument Get(string server, string query, CancellationToken cancelToken)
        {
            var uri = string.Format("https://{0}:{1}/{2}", server, _port, query);

            using (var client = new HttpClient(new HttpClientHandler
            {
                UseDefaultCredentials = true,
                AllowAutoRedirect = true
            }))
            {
                client.DefaultRequestHeaders.Add("Accept", "application/xml");

                client.Timeout = _defaultRequestTimeout;
                var response = client.GetAsync(uri, cancelToken);
                response.Wait(_defaultRequestTimeout);

                if (response.Result.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                var responseContent = response.Result.Content.ReadAsStringAsync();
                responseContent.Wait(cancelToken);

                return XDocument.Parse(responseContent.Result);
            }
        }

        public HttpResponseMessage Delete(string server, string query, CancellationToken cancelToken)
        {
            string baseUrl = string.Format("https://{0}:{1}", server, _port);

            var url = string.Format("{0}/{1}", baseUrl, query);

            using (var client = new HttpClient(new HttpClientHandler
            {
                UseDefaultCredentials = true,
                AllowAutoRedirect = true
            }))
            {
                client.Timeout = _defaultRequestTimeout;
                var response = client.DeleteAsync(url, cancelToken);
                response.Wait(_defaultRequestTimeout);
                return response.Result;
            }
        }

        public HttpResponseMessage Post(string server, string query, string payload, CancellationToken cancelToken)
        {
            string baseUrl = string.Format("https://{0}:{1}", server, _port);

            var url = string.Format("{0}/{1}", baseUrl, query);

            using (var client = new HttpClient(new HttpClientHandler
            {
                UseDefaultCredentials = true,
                AllowAutoRedirect = true
            }))
            {
                client.Timeout = _defaultRequestTimeout;
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = client.PostAsync(url, content, cancelToken);
                response.Wait(_defaultRequestTimeout);
                return response.Result;
            }
        }

        public HttpResponseMessage Put(string server, string query, string payload, CancellationToken cancelToken)
        {
            string baseUrl = string.Format("https://{0}:{1}", server, _port);

            var url = string.Format("{0}/{1}", baseUrl, query);

            using (var client = new HttpClient(new HttpClientHandler
            {
                UseDefaultCredentials = true,
                AllowAutoRedirect = true
            }))
            {
                client.Timeout = _defaultRequestTimeout;
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = client.PutAsync(url, content, cancelToken);
                response.Wait(_defaultRequestTimeout);
                return response.Result;
            }
        }
    }

    public interface IBasicRestService
    {
        XDocument Get(string server, string query, CancellationToken cancelToken);
        HttpResponseMessage Delete(string server, string query, CancellationToken cancelToken);
        HttpResponseMessage Post(string server, string query, string payload, CancellationToken cancelToken);
        HttpResponseMessage Put(string server, string query, string payload, CancellationToken cancelToken);
    }
}