using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net.Aws;

namespace IntegrationTests
{
    public static class TestConfig
    {
        public static string Endpoint { get; set; }
        public static AwsSettings AwsSettings { get; set; }
    }
}
