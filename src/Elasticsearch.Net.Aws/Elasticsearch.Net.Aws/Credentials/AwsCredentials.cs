using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    /// <summary>
    /// Encapsulates AWS credentials needed for request signing.
    /// </summary>
    public class AwsCredentials
    {
        /// <summary>
        /// Gets or sets the AWS access key. Required.
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the AWS secret key. e.g. wJalrXUtnFEMI/K7MDENG+bPxRfiCYEXAMPLEKEY
        ///  Required.
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// Gets or sets the security token.
        /// </summary>
        public string Token { get; set; }
    }
}
