using System;
using System.Net;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Elasticsearch.Net.Aws
{
    /// <summary>
    /// Implements Aws V4 signature
    /// </summary>
    public class AwsV4Signer : ISigner
    {
        private readonly string _region;
        private readonly string _service;
        private readonly ICredentialsProvider _credentialsProvider;
        private readonly char[] _datePartSplitChars = { 'T' };
        private readonly byte[] _emptyBytes = new byte[0];
        private readonly UTF8Encoding _encoding = new UTF8Encoding(false);

        public AwsV4Signer(string region, string service, ICredentialsProvider credentialsProvider)
        {
            _region = !String.IsNullOrWhiteSpace(region) ? region.ToLowerInvariant() : throw new ArgumentNullException(nameof(region));
            _service = !String.IsNullOrWhiteSpace(service) ? service : throw new ArgumentNullException(nameof(service));
            _credentialsProvider = credentialsProvider ?? throw new ArgumentNullException(nameof(credentialsProvider));
        }

        public void SignRequest(IRequest request, byte[] body)
        {
            var date = DateTime.UtcNow;
            var dateStamp = date.ToString("yyyyMMdd");
            var amzDate = date.ToString("yyyyMMddTHHmmssZ");
            var credentials = _credentialsProvider.GetCredentials();
            request.Headers.XAmzDate = amzDate;

            var signingKey = GetSigningKey(credentials.SecretKey, dateStamp, _region, _service);
            var stringToSign = GetStringToSign(request, body, _region, _service);
            Debug.Write("========== String to Sign ==========\r\n{0}\r\n========== String to Sign ==========\r\n", stringToSign);
            var signature = signingKey.GetHmacSha256Hash(stringToSign, _encoding).ToLowercaseHex();
            var auth = string.Format(
                "AWS4-HMAC-SHA256 Credential={0}/{1}, SignedHeaders={2}, Signature={3}",
                credentials.AccessKey,
                GetCredentialScope(dateStamp, _region, _service),
                GetSignedHeaders(request),
                signature);

            request.Headers.Authorization = auth;
            if (!String.IsNullOrWhiteSpace(credentials.Token))
                request.Headers.XAmzSecurityToken = credentials.Token;
        }

        private byte[] GetSigningKey(string secretKey, string dateStamp, string region, string service)
        {
            return _encoding.GetBytes("AWS4" + secretKey)
                .GetHmacSha256Hash(dateStamp, _encoding)
                .GetHmacSha256Hash(region, _encoding)
                .GetHmacSha256Hash(service, _encoding)
                .GetHmacSha256Hash("aws4_request", _encoding);
        }

        private string GetStringToSign(IRequest request, byte[] data, string region, string service)
        {
            var canonicalRequest = GetCanonicalRequest(request, data);
            Debug.Write("========== Canonical Request ==========\r\n{0}\r\n========== Canonical Request ==========\r\n", canonicalRequest);
            var awsDate = request.Headers.XAmzDate;
            Debug.Assert(Regex.IsMatch(awsDate, @"\d{8}T\d{6}Z"));
            var datePart = awsDate.Split(_datePartSplitChars, 2)[0];
            return string.Join("\n",
                "AWS4-HMAC-SHA256",
                awsDate,
                GetCredentialScope(datePart, region, service),
                GetHash(canonicalRequest).ToLowercaseHex()
            );
        }

        private string GetCredentialScope(string date, string region, string service)
        {
            return string.Format("{0}/{1}/{2}/aws4_request", date, region, service);
        }

        private string GetCanonicalRequest(IRequest request, byte[] data)
        {
            var canonicalHeaders = request.GetCanonicalHeaders();
            var result = new StringBuilder();
            result.Append(request.Method);
            result.Append('\n');
            result.Append(GetPath(request.RequestUri));
            result.Append('\n');
            result.Append(request.RequestUri.GetCanonicalQueryString());
            result.Append('\n');
            WriteCanonicalHeaders(canonicalHeaders, result);
            result.Append('\n');
            WriteSignedHeaders(canonicalHeaders, result);
            result.Append('\n');
            WriteRequestPayloadHash(data, result);
            return result.ToString();
        }

        private string GetPath(Uri uri)
        {
            var path = uri.AbsolutePath;
            if (path.Length == 0) return "/";

            IEnumerable<string> segments = path
                .Split('/')
                .Select(segment =>
                {
                    string escaped = WebUtility.UrlEncode(segment);
                    escaped = escaped.Replace("*", "%2A");
                    return escaped;
                }
                );
            return string.Join("/", segments);
        }

        private void WriteCanonicalHeaders(Dictionary<string, string> canonicalHeaders, StringBuilder output)
        {
            var q = from pair in canonicalHeaders
                    orderby pair.Key ascending
                    select string.Format("{0}:{1}\n", pair.Key, pair.Value);
            foreach (var line in q)
            {
                output.Append(line);
            }
        }

        private string GetSignedHeaders(IRequest request)
        {
            var canonicalHeaders = request.GetCanonicalHeaders();
            var result = new StringBuilder();
            WriteSignedHeaders(canonicalHeaders, result);
            return result.ToString();
        }

        private void WriteSignedHeaders(Dictionary<string, string> canonicalHeaders, StringBuilder output)
        {
            bool started = false;
            foreach (var pair in canonicalHeaders.OrderBy(v => v.Key))
            {
                if (started) output.Append(';');
                output.Append(pair.Key.ToLowerInvariant());
                started = true;
            }
        }

        private void WriteRequestPayloadHash(byte[] data, StringBuilder output)
        {
            data = data ?? _emptyBytes;
            var hash = data.GetHash();
            foreach (var b in hash)
            {
                output.AppendFormat("{0:x2}", b);
            }
        }

        private byte[] GetHash(string data)
        {
            return _encoding.GetBytes(data).GetHash();
        }
    }

    public static class AwsV4SignerHelpers
    {
        public static byte[] GetHmacSha256Hash(this byte[] key, string data, Encoding encoding)
        {
            using (var kha = new HMACSHA256())
            {
                kha.Key = key;
                return kha.ComputeHash(encoding.GetBytes(data));
            }
        }

        public static Dictionary<string, string> GetCanonicalHeaders(this IRequest request)
        {
            var q = from string key in request.Headers.Keys
                    let headerName = key.ToLowerInvariant()
                    let headerValues = string.Join(",",
                        request.Headers
                        .GetValues(key) ?? Enumerable.Empty<string>()
                        .Select(v => v.Trimall())
                    )
                    select new { headerName, headerValues };
            var result = q.ToDictionary(v => v.headerName, v => v.headerValues);
            result["host"] = request.RequestUri.Host.ToLowerInvariant();
            return result;
        }

        public static string GetCanonicalQueryString(this Uri uri)
        {
            if (string.IsNullOrWhiteSpace(uri.Query)) return string.Empty;
            var queryParams = ParseQueryString(uri.Query);
            var q = from string key in queryParams
                    orderby key
                    from value in queryParams.GetValues(key)
                    select new { key, value };

            var output = new StringBuilder();
            foreach (var param in q)
            {
                if (output.Length > 0) output.Append('&');
                output.WriteEncoded(param.key);
                output.Append('=');
                output.WriteEncoded(param.value);
            }
            return output.ToString();
        }

        public static void WriteEncoded(this StringBuilder output, string value)
        {
            for (var i = 0; i < value.Length; ++i)
            {
                if (value[i].RequiresEncoding())
                {
                    output.Append(Uri.EscapeDataString(value[i].ToString()));
                }
                else
                {
                    output.Append(value[i]);
                }
            }
        }

        public static bool RequiresEncoding(this char value)
        {
            if ('A' <= value && value <= 'Z') return false;
            if ('a' <= value && value <= 'z') return false;
            if ('0' <= value && value <= '9') return false;
            switch (value)
            {
                case '-':
                case '_':
                case '.':
                case '~':
                    return false;
            }
            return true;
        }

        public static string ToLowercaseHex(this byte[] data)
        {
            var result = new StringBuilder();
            foreach (var b in data)
            {
                result.AppendFormat("{0:x2}", b);
            }
            return result.ToString();
        }

        public static byte[] GetHash(this byte[] data)
        {
            using (var algo = SHA256.Create())
            {
                return algo.ComputeHash(data);
            }
        }

#if NET45
        private static NameValueCollection ParseQueryString(string query) =>
           System.Web.HttpUtility.ParseQueryString(query);
#else
        private static NameValueCollection ParseQueryString(string query) =>
            Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(query)
                .Aggregate(new NameValueCollection(), (col, kv) =>
                {
                    kv.Value.ToList().ForEach(v => col.Add(kv.Key, v));
                    return col;
                });
#endif
    }
}
