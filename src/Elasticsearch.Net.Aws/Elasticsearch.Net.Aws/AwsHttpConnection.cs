using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Amazon;
using Amazon.Runtime;

namespace Elasticsearch.Net.Aws
{
    /// <summary>
    /// Encapsulates an IConnection that works with AWS's Elasticsearch service.
    /// </summary>
    public class AwsHttpConnection : HttpConnection
    {
        private readonly AWSCredentials _credentials;
        private readonly RegionEndpoint _region;

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with the specified AccessKey, SecretKey and Token.
        /// </summary>
        /// <param name="credentials">The AWS credentials.</param>
        /// <param name="region">The AWS region to connect to.</param>
        public AwsHttpConnection(AWSCredentials credentials, RegionEndpoint region)
        {
            _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            _region = region ?? throw new ArgumentNullException(nameof(region));
        }

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with credentials from the Instance Profile service
        /// </summary>
        /// <param name="region">AWS region</param>
        public AwsHttpConnection(string region)
            : this(FallbackCredentialsFactory.GetCredentials(), RegionEndpoint.GetBySystemName(region))
        {
        }

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with credentials from the Instance Profile service
        /// </summary>
        public AwsHttpConnection() : this(
            FallbackCredentialsFactory.GetCredentials(),
            FallbackRegionFactory.GetRegionEndpoint() ?? throw new Exception("Unable to determine the correct AWS region. Please try providing it explicitly."))
        {
        }

#if NETSTANDARD
        protected override HttpRequestMessage CreateHttpRequestMessage(RequestData requestData)
        {
            var request = base.CreateHttpRequestMessage(requestData);
            SignRequest(new HttpRequestMessageAdapter(request), requestData);
            return request;
        }
#else
        [ThreadStatic]
        static int _createHttpRequestDepth;

        HttpWebRequest CreateWebRequestInternal(RequestData requestData, Func<RequestData, HttpWebRequest> creator)
        {
            _createHttpRequestDepth++;
            HttpWebRequest ret;
            try
            {
                ret = creator(requestData);
            }
            finally
            {
                _createHttpRequestDepth--;
            }
            if (_createHttpRequestDepth == 0)
            {
                SignRequest(new HttpWebRequestAdapter(ret), requestData);
            }
            return ret;

        }

        protected override HttpWebRequest CreateHttpWebRequest(RequestData requestData)
            => CreateWebRequestInternal(requestData, base.CreateHttpWebRequest);

        protected override HttpWebRequest CreateWebRequest(RequestData requestData)
            => CreateWebRequestInternal(requestData, base.CreateWebRequest);
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
            ImmutableCredentials credentials;
            try
            {
                credentials = _credentials.GetCredentials();
            }
            catch (Exception e)
            {
                throw new Exception("Unable to retrieve credentials required to sign the request.", e);
            }

            if (credentials == null)
            {
                throw new Exception("Unable to retrieve credentials required to sign the request.");
            }
            SignV4Util.SignRequest(request, data, credentials, _region.SystemName, "es");
        }
    }
}
