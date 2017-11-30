using System;
using System.IO;
using System.Net.Http;
using Amazon.Runtime;

namespace Elasticsearch.Net.Aws
{
    /// <summary>
    /// Encapsulates an IConnection that works with AWS's Elasticsearch service.
    /// </summary>
    public class AwsHttpConnection : HttpConnection
    {
        private readonly ImmutableCredentials _credentials;
        private readonly string _region;

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with credentials from the Instance Profile service
        /// </summary>
        /// <param name="region">AWS region</param>
        public AwsHttpConnection(string region)
            : this(region, FallbackCredentialsFactory.GetCredentials().GetCredentials())
        {
        }


        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with credentials from the Instance Profile service
        /// </summary>
        /// <param name="region">AWS region</param>
        /// <param name="credentials">The credentials provider.</param>
        public AwsHttpConnection(string region, ImmutableCredentials credentials)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
            if (string.IsNullOrWhiteSpace(region)) throw new ArgumentException("region is invalid", nameof(region));
            if (credentials == null) throw new ArgumentNullException(nameof(credentials));

            _region = region.ToLowerInvariant();
            _credentials = credentials;
        }

#if NET45
        protected override System.Net.HttpWebRequest CreateHttpWebRequest(RequestData requestData)
        {
            var request = base.CreateHttpWebRequest(requestData);
            SignRequest(new HttpWebRequestAdapter(request), requestData);
            return request;
        }
#else
        protected override HttpRequestMessage CreateHttpRequestMessage(RequestData requestData)
        {
            var request = base.CreateHttpRequestMessage(requestData);
            SignRequest(new HttpRequestMessageAdapter(request), requestData);
            return request;
        }
#endif
        private void SignRequest(IRequest request, RequestData requestData)
        {
            byte[] data = null;
            if (requestData.PostData != null)
            {
                data = requestData.PostData.WrittenBytes;
                if (data == null)
                {
                    using (var ms = new MemoryStream())
                    {
                        requestData.PostData.Write(ms, requestData.ConnectionSettings);
                        data = ms.ToArray();
                    }
                }
            }
            if (_credentials == null)
            {
                throw new Exception("Unable to retrieve credentials required to sign the request.");
            }
            SignV4Util.SignRequest(request, data, _credentials, _region, "es");
        }
    }
}
