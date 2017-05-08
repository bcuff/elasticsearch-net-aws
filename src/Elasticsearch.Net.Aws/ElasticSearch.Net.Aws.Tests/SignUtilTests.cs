using System;
using System.Diagnostics;
using System.Text;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using NUnit.Framework;
using Moq;

namespace Tests
{
    [TestFixture]
    public class SignUtilTests
    {
        IRequest _sampleRequest;
        byte[] _sampleBody;

        [SetUp]
        public void SetUp()
        {
            var encoding = new UTF8Encoding(false);
            _sampleBody = encoding.GetBytes("Action=ListUsers&Version=2010-05-08");
#if NETCOREAPP1_1
            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://iam.amazonaws.com/");
            request.Content = new System.Net.Http.ByteArrayContent(_sampleBody);
            request.Content.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");
            _sampleRequest = new HttpRequestMessageAdapter(request);
#elif NET451
            var request = System.Net.WebRequest.CreateHttp("https://iam.amazonaws.com/");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
            _sampleRequest = new HttpWebRequestAdapter(request);
#endif
        }

        [TestCase("us-east-1", "iam", "2017-01-01", "ExampleKey", "wJalrXUtnFEMI/K7MDENG+bPxRfiCYEXAMPLEKEY", "token1", "9471c6bd5c29dcf200bfe3800ce56d2256fb9a20fd4ee0a46d1a75997d96f494")]
        [TestCase("eu-west-1", "execute-api", "2017-05-08T16:35:25", "Some-other-key", "oL/fZjFyYDbNk8HLDtezQutb6Bvj81d/SjJdOjsO", "token2", "f93d91cff77bac6e97dbec26504e8c2b8bc218d61ee8191341aa1d7851c840c1")]
        public void SignRequest_should_apply_signature_to_request(string region, string service, string dateTimeStr, string accessKey, string secretKey, string token, string expectedSignature)
        {
            var dateTime = DateTime.Parse(dateTimeStr);
            var target = new AwsV4Signer(
                region, 
                service, 
                Mock.Of<ICredentialsProvider>(m => m.GetCredentials() == new AwsCredentials { AccessKey = accessKey, SecretKey = secretKey, Token = token }),
                Mock.Of<IDateTimeProvider>(m => m.Now() == dateTime)
            );

            target.SignRequest(_sampleRequest, _sampleBody);

            Assert.AreEqual(dateTime.ToString("yyyyMMddTHHmmssZ"), _sampleRequest.Headers.XAmzDate);
            Trace.WriteLine("X-Amz-Date: " + _sampleRequest.Headers.XAmzDate);

            Assert.AreEqual(
                $"AWS4-HMAC-SHA256 Credential={accessKey}/{dateTime:yyyyMMdd}/{region}/{service}/aws4_request, SignedHeaders=content-type;host;x-amz-date, Signature={expectedSignature}", 
                _sampleRequest.Headers.Authorization
            );
            Trace.WriteLine("Authorize: " + _sampleRequest.Headers.Authorization);

            Assert.AreEqual(token, _sampleRequest.Headers.XAmzSecurityToken);
            Trace.WriteLine("Token: " + _sampleRequest.Headers.XAmzSecurityToken);
        }

        [Test]
        public void GetCanonicalQueryString_should_match_sample()
        {
            const string before = ""
                + "X-Amz-Date=20110909T233600Z"
                + "&X-Amz-SignedHeaders=content-type%3Bhost%3Bx-amz-date"
                + "&Action=ListUsers"
                + "&Version=2010-05-08"
                + "&X-Amz-Algorithm=AWS4-HMAC-SHA256"
                + "&X-Amz-Credential=AKIDEXAMPLE%2F20110909%2Fus-east-1%2Fiam%2Faws4_request";

            var canonicalQueryString = new Uri("http://foo.com?" + before).GetCanonicalQueryString();
            const string expected = "Action=ListUsers&Version=2010-05-08&X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIDEXAMPLE%2F20110909%2Fus-east-1%2Fiam%2Faws4_request&X-Amz-Date=20110909T233600Z&X-Amz-SignedHeaders=content-type%3Bhost%3Bx-amz-date";

            Trace.WriteLine("Actual:   " + canonicalQueryString);
            Trace.WriteLine("Expected: " + expected);

            Assert.AreEqual(expected, canonicalQueryString);
        }
    }
}
