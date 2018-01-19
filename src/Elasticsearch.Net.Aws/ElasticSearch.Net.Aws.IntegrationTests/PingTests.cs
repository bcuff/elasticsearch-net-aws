using System;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public class PingTests
    {
        public class TestDoc
        {
            public string Id { get; set; }
            public string Something { get; set; }
        }
        private class Response : IElasticsearchResponse
        {
            public bool TryGetServerErrorReason(out string reason)
            {
                reason = "error";
                return true;
            }

            public IApiCallDetails ApiCall { get; set; }
        }

        private static string Region => TestConfig.AwsSettings.Region;
        private static ICredentialsProvider Credentials => new StaticCredentialsProvider(TestConfig.AwsSettings);


        [Test]
        public async Task Indexing_documents_should_work()
        {
            var httpConnection = new AwsHttpConnection(Region, Credentials);
            var pool = new SingleNodeConnectionPool(new Uri(TestConfig.Endpoint));
            var config = new Nest.ConnectionSettings(pool, httpConnection);
            var client = new Nest.ElasticClient(config);
            var indexName = "integration-test";
            var existsResponse = await client.IndexExistsAsync(indexName);
            if (existsResponse.Exists)
            {
                await client.DeleteIndexAsync(indexName);
            }
            var createIndexResponse = await client.CreateIndexAsync(indexName);
            Assert.IsTrue(createIndexResponse.Acknowledged);

            var doc = new TestDoc { Id = "123", Something = "unit-test" };
            var indexResponse = await client.IndexAsync(doc, i => i.Id(doc.Id).Index(indexName));
            Assert.AreEqual(Nest.Result.Created, indexResponse.Result);
            var queryResponse = await client.SearchAsync<TestDoc>(s => s.Index(indexName).Query(q => q.Ids(id => id.Values(doc.Id))));
            Assert.AreEqual(queryResponse.Documents.Count, 1);
            Assert.AreEqual(queryResponse.Documents.First().Id, doc.Id);
            Assert.AreEqual(queryResponse.Documents.First().Something, doc.Something);
            var deleteIndexResonse = await client.DeleteIndexAsync(indexName);
            Assert.IsTrue(deleteIndexResonse.Acknowledged);
        }
        [Test]
        public void Ping_should_work()
        {
            var httpConnection = new AwsHttpConnection(Region, Credentials);
            var pool = new SingleNodeConnectionPool(new Uri(TestConfig.Endpoint));
            var config = new ConnectionConfiguration(pool, httpConnection);
            var client = new ElasticLowLevelClient(config);
            var response = client.Ping<Response>();
            Assert.AreEqual(200, response.ApiCall.HttpStatusCode.GetValueOrDefault(-1));

        }

        [Test]
        public void NestPing_should_work()
        {
            var httpConnection = new AwsHttpConnection(Region, Credentials);
            var pool = new SingleNodeConnectionPool(new Uri(TestConfig.Endpoint));
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
            var pool = new SingleNodeConnectionPool(new Uri(TestConfig.Endpoint));
            var config = new ConnectionConfiguration(pool, httpConnection);
            var client = new ElasticLowLevelClient(config);
            var response = client.Get<Response>(randomString, string.Join(",", Enumerable.Repeat(randomString, 2)), randomString);
            Assert.AreEqual(404, response.ApiCall.HttpStatusCode.GetValueOrDefault(-1));
        }

        [Test]
        public void Asterisk_encoded_url_should_work()
        {
            var httpConnection = new AwsHttpConnection(Region, Credentials);
            var pool = new SingleNodeConnectionPool(new Uri(TestConfig.Endpoint));
            var config = new ConnectionConfiguration(pool, httpConnection);
            var client = new ElasticLowLevelClient(config);
            var response = client.Get<Response>("index*", "type", "id");
            Assert.AreEqual(404, response.ApiCall.HttpStatusCode.GetValueOrDefault(-1));
        }
    }

}
