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
        private static string Region => TestConfig.AwsSettings.Region;
        private static ICredentialsProvider Credentials => new StaticCredentialsProvider(TestConfig.AwsSettings);

        string _indexName;
        ElasticLowLevelClient _client;

        [SetUp]
        public void Setup()
        {
            var httpConnection = new AwsHttpConnection(Region, Credentials);
            var pool = new SingleNodeConnectionPool(new Uri(TestConfig.Endpoint));
            var config = new ConnectionConfiguration(pool, httpConnection);
            _client = new ElasticLowLevelClient(config);
            _indexName = $"unittest_{Guid.NewGuid().ToString("n")}";
        }

        [TearDown]
        public void TearDown()
        {
            _client.IndicesDelete<VoidResponse>(_indexName);
        }

        [Test]
        public void SimplePost_should_work()
        {
            var id = Guid.NewGuid().ToString("n");
            var response = _client.Create<VoidResponse>(_indexName, "test", id, PostData.Serializable(new { message = "Hello, World!" }));
            Assert.AreEqual(true, response.Success);
            var getResponse = _client.Get<StringResponse>(_indexName, "test", id);
            Assert.AreEqual(true, getResponse.Success);
            StringAssert.Contains("Hello, World!", getResponse.Body);
        }
    }
}
