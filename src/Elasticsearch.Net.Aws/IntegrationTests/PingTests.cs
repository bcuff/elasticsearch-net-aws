using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public class PingTests
    {
        [Test]
        public void Ping_should_work()
        {
            var httpConnection = new AwsHttpConnection(TestConfig.AwsSettings);
            var pool = new SingleNodeConnectionPool(new Uri(TestConfig.Endpoint));
            var config = new ConnectionConfiguration(pool, httpConnection);
            var client = new ElasticLowLevelClient(config);
            var response = client.Ping<object>();
            Assert.AreEqual(200, response.HttpStatusCode.GetValueOrDefault(-1));

        }

        [Test]
        public void NestPing_should_work()
        {
            var httpConnection = new AwsHttpConnection(TestConfig.AwsSettings);
            var pool = new SingleNodeConnectionPool(new Uri(TestConfig.Endpoint));
            var config = new Nest.ConnectionSettings(pool, httpConnection);
            var client = new Nest.ElasticClient(config);
            var response = client.Ping();
            Assert.AreEqual(true, response.IsValid);
        }
    }

}
