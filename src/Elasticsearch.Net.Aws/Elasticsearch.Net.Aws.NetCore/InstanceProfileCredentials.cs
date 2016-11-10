using System;

namespace Elasticsearch.Net.Aws
{
    internal class InstanceProfileCredentials
    {
        public string Code { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Type { get; set; }
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
