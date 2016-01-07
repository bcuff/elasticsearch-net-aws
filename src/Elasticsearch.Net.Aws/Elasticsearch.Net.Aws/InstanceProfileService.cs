using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace Elasticsearch.Net.Aws
{
    public static class InstanceProfileService
    {
        private static readonly string[] AliasSeparators = { "<br/>" };
        private const string Server = "http://169.254.169.254";
        private const string RolesPath = "/latest/meta-data/iam/security-credentials/";
        private const string SuccessCode = "Success";
        private static readonly JavaScriptSerializer CredentialSerializer = new JavaScriptSerializer();
        private static InstanceProfileCredentials _cachedCredentials;
        private static readonly object CacheLock = new object();

        public static InstanceProfileCredentials GetCredentials()
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
                    var credentials = CredentialSerializer.Deserialize<InstanceProfileCredentials>(json);

                    if (credentials.Code != SuccessCode)
                        return null;

                    _cachedCredentials = cachedCredentials = credentials;

                }
            }

            return cachedCredentials;
        }

        private static InstanceProfileCredentials GetCachedCredentials()
        {
            if (_cachedCredentials != null)
            {
                if (_cachedCredentials.Expiration > DateTime.Now.ToUniversalTime().AddMinutes(15))
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
                var request = WebRequest.Create(uri) as HttpWebRequest;
                var asyncResult = request.BeginGetResponse(null, null);
                using (var response = request.EndGetResponse(asyncResult) as HttpWebResponse)
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException)
            {
                throw new Exception("Unable to reach credentials server");
            }
        }

    }
}
