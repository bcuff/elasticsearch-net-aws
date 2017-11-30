using System;
using Elasticsearch.Net.Aws;
using Nest;
using Elasticsearch.Net;

namespace Nest5Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var httpConnection = new AwsHttpConnection("us-east-1");
            var pool = new SingleNodeConnectionPool(new Uri("https://someendpoint.us-east-1.es.amazonaws.com"));
            var config = new ConnectionSettings(pool, httpConnection);
            var client = new ElasticClient(config);
            client.Ping();
            Console.WriteLine("pinged successfully");
        }
    }
}
