using System;
using System.Net;
using Elasticsearch.Net.Connection;
using Elasticsearch.Net.Connection.Configuration;

namespace Elasticsearch.Net.Aws
{
    /// <summary>
    /// Encapsulates an IConnection that works with AWS's Elasticsearch service.
    /// </summary>
    public class AwsHttpConnection : HttpConnection
    {
        readonly string _accessKey;
        readonly string _secretKey;
        readonly string _token;
        readonly string _region;

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with the specified AccessKey, SecretKey and Token.
        /// </summary>
        /// <param name="settings">The NEST/Elasticsearch.Net settings.</param>
        /// <param name="awsSettings">AWS specific settings required for signing requests.</param>
        public AwsHttpConnection(IConnectionConfigurationValues settings, AwsSettings awsSettings) : base(settings)
        {
            if (awsSettings == null) throw new ArgumentNullException("awsSettings");
            if (string.IsNullOrWhiteSpace(awsSettings.AccessKey)) throw new ArgumentException("awsSettings.AccessKey is invalid.", "awsSettings");
            if (string.IsNullOrWhiteSpace(awsSettings.SecretKey)) throw new ArgumentException("awsSettings.SecretKey is invalid.", "awsSettings");
            if (string.IsNullOrWhiteSpace(awsSettings.Region)) throw new ArgumentException("awsSettings.Region is invalid.", "awsSettings");
            _accessKey = awsSettings.AccessKey;
            _secretKey = awsSettings.SecretKey;
            _token = awsSettings.Token;
            _region = awsSettings.Region.ToLowerInvariant();
        }

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class with info from the Instance Profile service
        /// </summary>
        /// <param name="settings">The NEST/Elasticsearch.Net settings.</param>
        public AwsHttpConnection(IConnectionConfigurationValues settings, string region) : base(settings)
        {
            var credentials = InstanceProfileService.GetCredentials();
            if (credentials == null) throw new Exception("Unable to retrieve session credentials from instance profile service");

            _accessKey = credentials.AccessKeyId;
            _secretKey = credentials.SecretAccessKey;
            _token = credentials.Token;
            _region = region.ToLowerInvariant();
        }

        protected override HttpWebRequest CreateHttpWebRequest(Uri uri, string method, byte[] data, IRequestConfiguration requestSpecificConfig)
        {
            var request = base.CreateHttpWebRequest(uri, method, data, requestSpecificConfig);
            SignV4Util.SignRequest(request, data, _accessKey, _secretKey, _token, _region, "es");
            return request;
        }
    }
}
