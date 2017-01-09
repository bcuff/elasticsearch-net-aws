using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    public class EnvironmentVariableCredentialsProvider : ICredentialsProvider
    {
        static readonly CredentialCache _cache = new CredentialCache(GetCredentialsInternal, TimeSpan.FromSeconds(10));

        private static AwsCredentials GetCredentialsInternal()
        {
            var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
                ?? Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
            if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey)) return null;
            return new AwsCredentials
            {
                AccessKey = accessKey,
                SecretKey = secretKey,
                Token = Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN"),
            };
        }

        public AwsCredentials GetCredentials() => _cache.Value;
    }
}
