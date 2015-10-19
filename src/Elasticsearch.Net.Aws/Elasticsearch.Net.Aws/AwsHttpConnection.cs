using System;
using System.Linq;
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
        readonly string _region;

        /// <summary>
        /// Initializes a new instance of the AwsHttpConnection class.
        /// </summary>
        /// <param name="settings">The NEST/Elasticsearch.Net settings.</param>
        /// <param name="awsSettings">AWS specific settings required for signing requests.</param>
        public AwsHttpConnection(IConnectionConfigurationValues settings, AwsSettings awsSettings) : base(settings)
        {
            if (awsSettings == null) throw new ArgumentNullException("awsSettings");
            if (string.IsNullOrWhiteSpace(awsSettings.SecretKey)) throw new ArgumentException("awsSettings.SecretKey is invalid.", "awsSettings");
            if (string.IsNullOrWhiteSpace(awsSettings.Region)) throw new ArgumentException("awsSettings.Region is invalid.", "awsSettings");
            _accessKey = awsSettings.AccessKey;
            _secretKey = awsSettings.SecretKey;
            _region = awsSettings.Region.ToLowerInvariant();
        }

        protected override HttpWebRequest CreateHttpWebRequest(Uri uri, string method, byte[] data, IRequestConfiguration requestSpecificConfig)
        {
            var request = base.CreateHttpWebRequest(uri, method, data, requestSpecificConfig);
            SignV4Util.SignRequest(request, data, _accessKey, _secretKey, _region, "es");
            return request;
        }
    }
}
