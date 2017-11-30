using System;
using System.IO;
using System.Linq;
using Amazon;
using Amazon.Runtime;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public class PingTests
    {
        private static string Region => TestConfig.Region;
        private static ImmutableCredentials Credentials => TestConfig.Credentials;

        [Test]
        public void Ping_should_work()
        {
            var httpConnection = new AwsHttpConnection(Region, Credentials);
            var pool = new SingleNodeConnectionPool(TestConfig.Endpoint);
            var config = new ConnectionConfiguration(pool, httpConnection);
            var client = new ElasticLowLevelClient(config);
            var response = client.Ping<object>();
            Assert.AreEqual(200, response.HttpStatusCode.GetValueOrDefault(-1));

        }

        [Test]
        public void NestPing_should_work()
        {
            var httpConnection = new AwsHttpConnection(Region, Credentials);
            var pool = new SingleNodeConnectionPool(TestConfig.Endpoint);
            var config = new Nest.ConnectionSettings(pool, httpConnection);
            var client = new Nest.ElasticClient(config);
            var response = client.Ping();
            Assert.AreEqual(true, response.IsValid);
        }

        [Test]
        public void Random_encoded_url_should_work()
        {
            var randomString = Guid.NewGuid().ToString("N");
            var httpConnection = new AwsHttpConnection(Region, Credentials);
            var pool = new SingleNodeConnectionPool(TestConfig.Endpoint);
            var config = new ConnectionConfiguration(pool, httpConnection);
            var client = new ElasticLowLevelClient(config);
            var response = client.Get<Stream>(randomString, string.Join(",", Enumerable.Repeat(randomString, 2)), randomString);
            Assert.AreEqual(404, response.HttpStatusCode.GetValueOrDefault(-1));
        }

        [Test]
        public void Asterisk_encoded_url_should_work()
        {
            var httpConnection = new AwsHttpConnection(Region, Credentials);
            var pool = new SingleNodeConnectionPool(TestConfig.Endpoint);
            var config = new ConnectionConfiguration(pool, httpConnection);
            var client = new ElasticLowLevelClient(config);
            var response = client.Get<Stream>("index*", "type", "id");
            Assert.AreEqual(404, response.HttpStatusCode.GetValueOrDefault(-1));
        }
    }

}
