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
        public string GetToken(string oAuth2Url, string tenant_Id, string client_Id, string client_Secret, string resource, string grantType = "client_credentials", bool forceNewToken = false)
        {
            return AppDomainHelper.TokenDictionary.GetToken(oAuth2Url, tenant_Id, client_Id, client_Secret, resource, grantType, forceNewToken);
        }
    }
}
