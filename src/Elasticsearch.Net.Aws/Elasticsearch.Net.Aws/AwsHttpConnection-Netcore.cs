using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    public class AwsHttpConnection : HttpConnection
    {
        readonly Credentials _credentials;
        readonly string _region;

        public AwsHttpConnection(AwsSettings settings)
        {
            // todo - proper credential chain
            _credentials = new Credentials
            {
                AccessKey = settings.AccessKey,
                SecretKey = settings.SecretKey,
            };
            _region = settings.Region;
        }

        protected override HttpRequestMessage CreateHttpRequestMessage(RequestData requestData)
        {

            var result = base.CreateHttpRequestMessage(requestData);
            var signable = new SignableHttpRequestMessage(result, requestData);
            SignV4Util.SignRequest(signable, _credentials, _region, "es");
            return result;
        }
    }
}
