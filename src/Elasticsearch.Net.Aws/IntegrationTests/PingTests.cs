using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Elasticsearch.Net.Connection;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public class PingTests
    {
        [Test]
        public void Ping_should_work()
        {
            var settings = new ConnectionConfiguration(new Uri(TestConfig.Endpoint));
            var client = new ElasticsearchClient(settings, new AwsHttpConnection(settings, TestConfig.AwsSettings));
            var response = client.Ping();
            Assert.AreEqual(200, response.HttpStatusCode.GetValueOrDefault(-1));
        }
    }
}
