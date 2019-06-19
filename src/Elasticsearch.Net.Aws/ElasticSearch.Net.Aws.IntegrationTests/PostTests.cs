using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;

namespace IntegrationTests
{
    [TestFixture]
    public class PostTests
    {
        private static string Region => TestConfig.Region;

        string _indexName;
        ElasticLowLevelClient _client;

        [SetUp]
        public void Setup()
        {
            var httpConnection = new AwsHttpConnection(Region);
            var pool = new SingleNodeConnectionPool(new Uri(TestConfig.Endpoint));
            var config = new ConnectionConfiguration(pool, httpConnection);
            config.DisableDirectStreaming();
            _client = new ElasticLowLevelClient(config);
            _indexName = $"unittest_{Guid.NewGuid().ToString("n")}";
        }

        [TearDown]
        public void TearDown()
        {
            _client.Indices.Delete<VoidResponse>(_indexName);
        }

        [Test]
        public void SimplePost_should_work()
        {
            var id = Guid.NewGuid().ToString("n");
            var response = _client.Create<VoidResponse>(
                _indexName,
                id,
                PostData.Serializable(new { message = "Hello, World!" }));
            Assert.AreEqual(true, response.Success, response.DebugInformation);
            var getResponse = _client.Get<StringResponse>(_indexName, id);
            Assert.AreEqual(true, getResponse.Success, getResponse.DebugInformation);
            StringAssert.Contains("Hello, World!", getResponse.Body);
        }
    }
}