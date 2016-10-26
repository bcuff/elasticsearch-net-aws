using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

#if DOTNETCORE
using System.Net.Http;
#endif

namespace Elasticsearch.Net.Aws
{
    internal static class InstanceProfileService
    {
        private static readonly string[] AliasSeparators = { "<br/>" };
        private const string Server = "http://169.254.169.254";
        private const string RolesPath = "/latest/meta-data/iam/security-credentials/";
        private const string SuccessCode = "Success";
        private static InstanceProfileCredentials _cachedCredentials;
        private static readonly object CacheLock = new object();

        internal static InstanceProfileCredentials GetCredentials()
        {
            var cachedCredentials = GetCachedCredentials();
            if (cachedCredentials != null)
                return cachedCredentials;

            lock (CacheLock)
            {
                if (GetCachedCredentials() == null)
                {
                    var role = GetFirstRole();
                    var json = GetContents(new Uri(Server + RolesPath + role));
                    var credentials = JsonConvert.DeserializeObject<InstanceProfileCredentials>(json);

                    if (credentials.Code != SuccessCode)
                        return null;

                    _cachedCredentials = credentials;

                }
            }

            return GetCachedCredentials();
        }

        private static InstanceProfileCredentials GetCachedCredentials()
        {
            if (_cachedCredentials != null)
            {
                if (_cachedCredentials.Expiration > DateTime.UtcNow.AddMinutes(15))
                {
                    return _cachedCredentials;
                }
            }
            return null;
        }

        private static string GetFirstRole()
        {
            var roles = GetAvailableRoles();
            foreach (var role in roles)
            {
                return role;
            }

            // no roles found
            throw new InvalidOperationException("No roles found");
        }

        private static IEnumerable<string> GetAvailableRoles()
        {
            var allAliases = GetContents(new Uri(Server + RolesPath));
            if (string.IsNullOrEmpty(allAliases))
                yield break;

            var parts = allAliases.Split(AliasSeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trim = part.Trim();
                if (!string.IsNullOrEmpty(trim))
                    yield return trim;
            }
        }

        private static string GetContents(Uri uri)
        {
            try
            {
#if DOTNETCORE
                using (var client = new HttpClient())
                using (var response = client.GetAsync(uri).Result)
                {
                    return response.Content.ReadAsStringAsync().Result;
               }
#else
                using (var response = WebRequest.CreateHttp(uri).GetResponse())
                using (var body = response.GetResponseStream())
                using (var reader = new StreamReader(body))
                {
                    return reader.ReadToEnd();
                }
#endif
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get instance profile credentials", ex);
            }
        }

    }
}
