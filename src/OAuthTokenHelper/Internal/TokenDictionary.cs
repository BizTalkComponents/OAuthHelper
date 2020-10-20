using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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


        internal static TokenInfo GetNewToken(string oAuth2Url, string client_Id, string client_Secret, string claims)
        {
            return GetNewToken(oAuth2Url, GetContentsAsString(client_Id, client_Secret, claims));
        }

        private static TokenInfo GetNewToken(string oAuth2Url, string content)
        {
            HttpClient client = new HttpClient();
            StringContent scontent = new StringContent(content);
            scontent.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            TokenDictionary.WriteLogMessage("Calling url:'" + oAuth2Url + "' to get a new token");
            TokenInfo tokenInfo = null;
            try
            {
                var response = client.PostAsync(oAuth2Url, scontent).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                tokenInfo = JsonConvert.DeserializeObject<TokenInfo>(json);
            }
            catch (Exception ex)
            {
                WriteLogMessage(ex.Message, EventLogEntryType.Error);
                throw ex;
            }
            return tokenInfo;
        }

        private static string GetContentsAsString(string client_Id, string client_Secret, string claims)
        {
            List<string> contents = new List<string>();
            contents.Add($"client_id={client_Id}");
            contents.Add($"client_secret={client_Secret}");
            contents.Add($"grant_type=client_credentials");
            if (!string.IsNullOrEmpty(claims))
                contents.Add(claims);
            return string.Join("&", contents);
        }

        public string GetToken(string oAuth2Url, string client_Id, string client_Secret, string claims, bool forceNewToken = false)
        {            
            string key,content = key = GetContentsAsString(client_Id, client_Secret, claims); 
            TokenInfo ti = GetOrCreateTokenInfo(key);
            SpinWait.SpinUntil(() => !ti.IsWaiting);
            if (!ti.IsValid | forceNewToken)
            {
                ti.IsWaiting = true;
                try
                {
                    var newToken = GetNewToken(oAuth2Url, key);
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
