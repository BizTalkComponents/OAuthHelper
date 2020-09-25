using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace BizTalkComponents.CustomComponents.OAuthTokenHelper
{

    internal class TokenDictionary : MarshalByRefObject
    {
        private ConcurrentDictionary<string, TokenInfo> tokenDic = new ConcurrentDictionary<string, TokenInfo>();
        private System.Timers.Timer localTimer = new System.Timers.Timer(60000);

        public TokenDictionary()
        {
            localTimer.Elapsed += LocalTimer_Elapsed;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        private void LocalTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var keys = tokenDic.Keys.ToList();
            foreach (string key in keys)
            {
                TokenInfo ti = tokenDic[key];
                if (!ti.IsValid & !ti.IsWaiting)
                {
                    tokenDic.TryRemove(key, out ti);
                }
            }
            if (tokenDic.Count == 0)
            {
                localTimer.Enabled = false;
                WriteLogMessage("No more valid token available in the dictionary");
            }
        }

        public TokenInfo GetOrCreateTokenInfo(string key)
        {
            var ti = tokenDic.GetOrAdd(key, k => new TokenInfo());
            localTimer.Enabled = true;
            return ti;
        }

        internal static TokenInfo GetNewToken(string oAuth2Url, string tenant_Id, string client_Id, string client_Secret, string resource, string grantType)
        {
            TokenDictionary.WriteLogMessage("Calling url:'" + oAuth2Url + "' to get a new token");
            HttpClient client = new HttpClient();
            StringContent content = new StringContent(string.Format("tenant_id={0}"
                + "&client_id={1}"
                + "&client_secret={2}"
                + "&grant_type={3}"
                + "&resource={4}",
                tenant_Id,
                client_Id,
                client_Secret,
                grantType,
                resource));
            content.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var response = client.PostAsync(oAuth2Url, content).Result;
            response.EnsureSuccessStatusCode();
            string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            TokenInfo tokenInfo = JsonConvert.DeserializeObject<TokenInfo>(json);
            return tokenInfo;
        }

        public string GetToken(string oAuth2Url, string tenant_Id, string client_Id, string client_Secret, string resource, string grantType, bool forceNewToken = false)
        {
            string key = string.Format("url={0}"
                + "&tenant_id={1}"
                + "&client_id={2}"
                + "&client_secret={3}"
                + "&grant_type={4}"
                + "&resource={5}",
                oAuth2Url,
                tenant_Id,
                client_Id,
                client_Secret,
                grantType,
                resource);
            TokenInfo ti = GetOrCreateTokenInfo(key);
            SpinWait.SpinUntil(() => !ti.IsWaiting);
            if (!ti.IsValid | forceNewToken)
            {
                ti.IsWaiting = true;
                try
                {
                    var newToken = GetNewToken(oAuth2Url, tenant_Id, client_Id, client_Secret, resource, grantType);
                    ti.UpdateFrom(newToken);
                }
                finally
                {
                    ti.IsWaiting = false;
                }
            }
            else
            {
                WriteLogMessage("Get token from dictionary");
            }
            return ti.Token;
        }

        internal static void WriteLogMessage(string message, System.Diagnostics.EventLogEntryType evType = System.Diagnostics.EventLogEntryType.Information, [CallerMemberName] string procName = "")
        {
            System.Diagnostics.EventLog.WriteEntry("TokenHelper", string.Format("{0}\n{1}", procName, message), evType);
        }

    }
}
