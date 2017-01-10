using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Elasticsearch.Net.Aws
{
    public static class CredentialProviderExtensions
    {
#if NET45
        public static void Sign(this ICredentialsProvider credentialsProvider, System.Net.HttpWebRequest request, byte[] body)
            => credentialsProvider.Sign(new HttpWebRequestAdapter(request), body);
#else
        public static void Sign(this ICredentialsProvider credentialsProvider, System.Net.Http.HttpRequestMessage request, byte[] body)
            => credentialsProvider.Sign(new HttpRequestMessageAdapter(request), body);
#endif

        internal static void Sign(this ICredentialsProvider credentialsProvider, IRequest request, byte[] body)
        {
            var credentials = credentialsProvider.GetCredentials();
            if (credentials == null)
            {
                throw new Exception("Unable to retrieve credentials.");
            }
            var regionService = ExtractRegionAndService(request.RequestUri);
            SignV4Util.SignRequest(request, body, credentials, regionService.Item1, regionService.Item2);
        }

        static readonly Regex _regionRegex = new Regex(@"\.([\w-]+)\.\w+\.amazonaws\.com$", RegexOptions.Compiled);

        private static Tuple<string, string> ExtractRegionAndService(Uri url)
        {
            var match = _regionRegex.Match(url.Host);
            if (match.Success)
            {
                return new Tuple<string, string>(match.Groups[1].Value, match.Groups[2].Value);
            }
            throw new ArgumentException("Invalid url host. Expected something like ...us-east-1.es.amazonaws.com");
        }
    }
}
