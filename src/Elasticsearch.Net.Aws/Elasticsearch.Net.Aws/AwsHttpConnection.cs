using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
#if NETSTANDARD
using Amazon.Extensions.NETCore.Setup;
#endif

namespace Elasticsearch.Net.Aws
{
    /// <summary>
    /// Encapsulates an IConnection that works with AWS's Elasticsearch service.
    /// </summary>
    public class AwsHttpConnection : HttpConnection
    {
        private readonly AWSCredentials _credentials;
        private readonly RegionEndpoint _region;

        static AWSCredentials GetCredentials()
            => FallbackCredentialsFactory.GetCredentials()
            ?? throw new Exception("Unable to obtain AWS Credentials.");

        static RegionEndpoint GetRegion()
            => FallbackRegionFactory.GetRegionEndpoint()
            ?? throw new Exception("Unable to determine the correct AWS region. Please try providing it explicitly.");

#if NETSTANDARD
        static AWSCredentials GetCredentialsFromOptions(AWSOptions options)
        {
            var ret = options.Credentials;
            if (ret != null) return ret;
            if (!string.IsNullOrEmpty(options.Profile))
            {
                var chain = new CredentialProfileStoreChain(options.ProfilesLocation);
                if (chain.TryGetAWSCredentials(options.Profile, out ret)) return ret;
            }
            return GetCredentials();
        }

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection using AWSOptions.
        /// </summary>
        /// <param name="options">The AWS options.</param>
        public AwsHttpConnection(AWSOptions options) : this(
            GetCredentialsFromOptions(options),
            options.Region ?? GetRegion())
        {
        }
#endif

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
            : this(GetCredentials(), RegionEndpoint.GetBySystemName(region))
        {
        }

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with credentials from the Instance Profile service
        /// </summary>
        public AwsHttpConnection() : this(
            GetCredentials(),
            GetRegion())
        {
        }

#if NETSTANDARD
        protected override HttpMessageHandler CreateHttpClientHandler(RequestData requestData)
        {
            var innerHandler = base.CreateHttpClientHandler(requestData);
            return new SigningHttpMessageHandler(_credentials, _region, innerHandler);
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
                SignRequest(new HttpWebRequestAdapter(ret, requestData));
            }
            return ret;

        }

        protected override HttpWebRequest CreateHttpWebRequest(RequestData requestData)
            => CreateWebRequestInternal(requestData, base.CreateHttpWebRequest);

        protected override HttpWebRequest CreateWebRequest(RequestData requestData)
            => CreateWebRequestInternal(requestData, base.CreateWebRequest);

        private void SignRequest(IRequest request)
        {
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
            SignV4Util.SignRequestAsync(request, credentials, _region.SystemName, "es").Wait();
        }
#endif
    }
}
