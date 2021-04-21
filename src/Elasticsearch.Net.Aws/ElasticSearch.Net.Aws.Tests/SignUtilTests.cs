using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Elasticsearch.Net.Aws;
using NUnit.Framework;
using Elasticsearch.Net;
using System.IO;
#if NETFRAMEWORK
using System.Net;
#endif

namespace Tests
{
#if NETCOREAPP
    [TestFixture(true)]
    [TestFixture(false)]
#else
    [TestFixture]
#endif
    public class SignUtilTests
    {
        bool _useByteArrayContent;
        IRequest _sampleRequest;
        byte[] _sampleBody;

#if NETCOREAPP
        public SignUtilTests(bool useByteArrayContent)
        {
            _useByteArrayContent = useByteArrayContent;
        }
#endif

        [SetUp]
        public void SetUp()
        {
            var encoding = new UTF8Encoding(false);
            _sampleBody = encoding.GetBytes("Action=ListUsers&Version=2010-05-08");
#if NETCOREAPP
            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://iam.amazonaws.com/");
            if (_useByteArrayContent)
            {
                request.Content = new ByteArrayContent(_sampleBody);
            }
            else
            {
                request.Content = new StreamContent(new MemoryStream(_sampleBody));
            }
            request.Content.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");
            request.Headers.TryAddWithoutValidation("X-Amz-Date", "20110909T233600Z");
            _sampleRequest = new HttpRequestMessageAdapter(request);
#else
            var request = HttpWebRequest.CreateHttp("https://iam.amazonaws.com/");
            request.Method = "POST";
            //request.Content = new System.Net.Http.ByteArrayContent(_sampleBody);
            request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
            request.Headers["X-Amz-Date"] = "20110909T233600Z";
            var data = new RequestData(
                Elasticsearch.Net.HttpMethod.POST,
                "/",
                PostData.Bytes(_sampleBody),
                new ConnectionConfiguration(),
                new CreateRequestParameters(),
                new MemoryStreamFactory());
            _sampleRequest = new HttpWebRequestAdapter(request, data);
#endif
        }

        [Test]
        public async Task GetCanonicalRequest_should_match_sample()
        {
            await _sampleRequest.PrepareForSigningAsync();
            var canonicalRequest = SignV4Util.GetCanonicalRequest(_sampleRequest);
            Trace.WriteLine("=== Actual ===");
            Trace.Write(canonicalRequest);

            const string expected = "POST\n/\n\ncontent-type:application/x-www-form-urlencoded; charset=utf-8\nhost:iam.amazonaws.com\nx-amz-date:20110909T233600Z\n\ncontent-type;host;x-amz-date\nb6359072c78d70ebee1e81adcbab4f01bf2c23245fa365ef83fe8f1f955085e2";
            Trace.WriteLine("=== Expected ===");
            Trace.Write(expected);

            Assert.AreEqual(expected, canonicalRequest);
        }

        [Test]
        public async Task GetStringToSign_should_match_sample()
        {
            await _sampleRequest.PrepareForSigningAsync();
            var stringToSign = SignV4Util.GetStringToSign(_sampleRequest, "us-east-1", "iam");
            Trace.WriteLine("=== Actual ===");
            Trace.Write(stringToSign);

            const string expected = "AWS4-HMAC-SHA256\n20110909T233600Z\n20110909/us-east-1/iam/aws4_request\n3511de7e95d28ecd39e9513b642aee07e54f4941150d8df8bf94b328ef7e55e2";
            Trace.WriteLine("=== Expected ===");
            Trace.Write(expected);

            Assert.AreEqual(expected, stringToSign);
        }

        [Test]
        public void GetSigningKey_should_match_sample()
        {
            // sample comes from - http://docs.aws.amazon.com/general/latest/gr/signature-v4-examples.html
            var secretKey = "wJalrXUtnFEMI/K7MDENG+bPxRfiCYEXAMPLEKEY";
            var date = "20120215";
            var region = "us-east-1";
            var service = "iam";
            var expectedSigningKey = "f4780e2d9f65fa895f9c67b32ce1baf0b0d8a43505a000a1a9e090d414db404d";
            var actualSigningKeyBytes = SignV4Util.GetSigningKey(secretKey, date, region, service);
            var actualSigningKey = BitConverter.ToString(actualSigningKeyBytes).Replace("-", "").ToLowerInvariant();
            Trace.WriteLine("Expected: " + expectedSigningKey);
            Trace.WriteLine("Actual:   " + actualSigningKey);
            Assert.AreEqual(expectedSigningKey, actualSigningKey);
        }

        [Test]
        public async Task SignRequest_should_apply_signature_to_request()
        {
            var creds = new SessionAWSCredentials("ExampleKey", "wJalrXUtnFEMI/K7MDENG+bPxRfiCYEXAMPLEKEY", "token1")
                .GetCredentials();
            await SignV4Util.SignRequestAsync(_sampleRequest, creds, "us-east-1", "iam");

            var amzDate = _sampleRequest.Headers.XAmzDate;
            Assert.False(String.IsNullOrEmpty(amzDate));
            Trace.WriteLine("X-Amz-Date: " + amzDate);

            var auth = _sampleRequest.Headers.Authorization;
            Assert.False(String.IsNullOrEmpty(auth));
            Trace.WriteLine("Authorize: " + auth);

            var token = _sampleRequest.Headers.XAmzSecurityToken;
            Assert.False(String.IsNullOrEmpty(token));
            Trace.WriteLine("Token: " + token);
        }

        [Test]
        public async Task SignRequest_should_apply_signature_to_request_right_culture()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("th");

            var creds = new SessionAWSCredentials("ExampleKey", "wJalrXUtnFEMI/K7MDENG+bPxRfiCYEXAMPLEKEY", "token1")
                .GetCredentials();
            await SignV4Util.SignRequestAsync(_sampleRequest, creds, "us-east-1", "iam");

            var amzDateValue = _sampleRequest.Headers.XAmzDate;
            Assert.False(String.IsNullOrEmpty(amzDateValue));
            var amzDates = amzDateValue.Split(',');
            if (amzDates.Length != 2)
            {
                Assert.Inconclusive("Thai culture not working so this test won't work on this platform.");
                return;
            }
            Assert.True(amzDates[1].StartsWith(DateTime.UtcNow.Year.ToString()));
            Trace.WriteLine("X-Amz-Date: " + amzDateValue);

            var auth = _sampleRequest.Headers.Authorization;
            Assert.False(String.IsNullOrEmpty(auth));
            Trace.WriteLine("Authorize: " + auth);

            var token = _sampleRequest.Headers.XAmzSecurityToken;
            Assert.False(String.IsNullOrEmpty(token));
            Trace.WriteLine("Token: " + token);
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
