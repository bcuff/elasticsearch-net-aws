using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

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
                    if (role == null) return null;
                    var json = GetContents(new Uri(Server + RolesPath + role));
                    var credentials = JsonConvert.DeserializeObject<InstanceProfileCredentials>(json);

                    if (credentials.Code != SuccessCode)
                        return null;

                    credentials.LastObtained = DateTime.UtcNow;

                    _cachedCredentials = credentials;
                }
            }

            return GetCachedCredentials();
        }

        private static InstanceProfileCredentials GetCachedCredentials()
        {
            var cached = _cachedCredentials;
            if (cached != null)
            {
                var now = DateTime.UtcNow;
                // if we still have at least 10 minutes left
                if (cached.Expiration > now.AddMinutes(10))
                {
                    return cached;
                }
                // if we got the credentials less than 1 minute ago
                if ((now - cached.LastObtained) < TimeSpan.FromMinutes(1))
                {
                    return cached;
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

            return null;
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
                using (var client = new HttpClient())
                using (var reader = new StreamReader(client.GetStreamAsync(uri).Result))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Unable to reach credentials server", ex);
            }
        }

    }
}
