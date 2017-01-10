using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    public interface ICredentialsProvider
    {
        AwsCredentials GetCredentials();
    }
}
