using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Moq;
using NUnit.Framework;
using System;

namespace ElasticSearch.Net.Aws.Tests
{
    [TestFixture]
    public class AwsHttpConnectionTests
    {
        [Test]
        public void Requests_Should_Be_Signed()
        {
            // arrange
            var uri = new Uri("https://some-host:9200/");
            var signerMock = new Mock<ISigner>();
            var client = new ElasticLowLevelClient(
                new ConnectionConfiguration(
                    new SingleNodeConnectionPool(uri),
                    new AwsHttpConnection(signerMock.Object)
                )
            );
            signerMock.Setup(m => m.SignRequest(It.IsAny<IRequest>(), It.IsAny<byte[]>())).Verifiable();

            // act
            var result = client.Bulk<dynamic>(new PostData<string>(new byte[0] { }));

            // assert
            signerMock.Verify(m => m
                .SignRequest(
                    It.Is<IRequest>(v => v.RequestUri.ToString() == $"{uri}_bulk"), 
                    It.IsAny<byte[]>()
                ),
                Times.Once()
            );
        }
    }
}
