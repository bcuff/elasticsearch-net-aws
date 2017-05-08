using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using NUnit.Framework;
using Moq;

namespace ElasticSearch.Net.Aws.Tests
{
    [TestFixture]
    public class AwsHttpConnectionTests
    {
        [Test]
        public void Requests_Should_Be_Signed()
        {
            // arrange
            var data = new byte[0];
            var signerMock = new Mock<ISigner>();
            var target = new AwsHttpConnection(signerMock.Object);
            var request = new RequestData(
                HttpMethod.POST,
                "some-path",
                new PostData<object>(data),
                Mock.Of<IConnectionConfigurationValues>(m =>
                    m.QueryStringParameters == new NameValueCollection() &&
                    m.ConnectionPool == Mock.Of<IConnectionPool>() &&
                    m.RequestTimeout == TimeSpan.FromSeconds(5)
                ),
                default(IRequestParameters),
                Mock.Of<IMemoryStreamFactory>()
            );
            signerMock.Setup(m => m.SignRequest(It.IsAny<IRequest>(), It.IsAny<byte[]>())).Verifiable();

            // act
            var result = target.Request<dynamic>(request);

            // assert
            signerMock.Verify(m => m.SignRequest(It.IsAny<IRequest>(), It.IsAny<byte[]>()));
        }
    }
}
