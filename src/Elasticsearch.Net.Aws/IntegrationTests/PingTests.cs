using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Elasticsearch.Net.Connection;
using Elasticsearch.Net.ConnectionPool;
using NUnit.Framework;

namespace IntegrationTests
{
    using System.IO;

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

        [Test]
        public void Random_encoded_url_should_work()
        {
            var randomString = Guid.NewGuid().ToString("N");
            var settings = new ConnectionConfiguration(new Uri(TestConfig.Endpoint));
            var httpConnection = new AwsHttpConnection(settings, TestConfig.AwsSettings);
            var client = new ElasticsearchClient(settings, httpConnection);
            var response = client.Get<Stream>(randomString, string.Join(",", Enumerable.Repeat(randomString, 2)), randomString);
            Assert.AreEqual(404, response.HttpStatusCode.GetValueOrDefault(-1));
        }

        [Test]
        public void Asterisk_encoded_url_should_work()
        {
            var httpConnection = new AwsHttpConnection(TestConfig.AwsSettings);
            var pool = new SingleNodeConnectionPool(new Uri(TestConfig.Endpoint));
            var config = new ConnectionConfiguration(pool, httpConnection);
            var client = new ElasticLowLevelClient(config);
            var response = client.Get<Stream>("index*", "type", "id");
            Assert.AreEqual(404, response.HttpStatusCode.GetValueOrDefault(-1));
        }
    }
}
