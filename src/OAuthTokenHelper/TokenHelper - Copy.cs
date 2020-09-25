using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BizTalkComponents.CustomComponents.OAuthTokenHelper
{
    [Serializable]
    public class TokenHelper
    {
        private TokenInfo GetNewToken(string oAuth2Url, string tenant_Id, string client_Id, string client_Secret, string resource, string grantType = "client_credentials")
        {
            //TokenDictionary.WriteLogMessage("Calling url:'"+oAuth2Url+"' to get a new token");
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

        public string GetToken(string oAuth2Url, string tenant_Id, string client_Id, string client_Secret, string resource, string grantType = "client_credentials")
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
            TokenInfo ti = TokenDictionary.TheInstance.GetOrCreateTokenInfo(key);
            while (ti.IsWaiting)
                Thread.Sleep(100);
            if (!ti.IsValid)
            {
                ti.IsWaiting = true;
                try
                {
                    var newToken = GetNewToken(oAuth2Url, tenant_Id, client_Id, client_Secret, resource, grantType);
                    var ti1 = AppDomainHelper.LazyWrapper.Value.UpdateToken(newToken, key);
                    //ti.UpdateFrom(newToken);
                }
                finally
                {
                    ti.IsWaiting = false;
                }
            }
            else
            {
                TokenDictionary.WriteLogMessage("Get token from dictionary");
            }
            return ti.Token;
        }
        ~TokenHelper()
        {
            TokenDictionary.WriteLine("TokenHelper is destroyed");
        }

    }
}
