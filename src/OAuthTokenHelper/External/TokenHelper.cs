using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BizTalkComponents.CustomComponents.OAuthTokenHelper
{
    [Serializable]
    public static class TokenHelper
    {
        public static string GetToken(string oAuth2Url, string client_Id, string client_Secret, string claims, bool forceNewToken = false)
        {
            return AppDomainHelper.TokenDictionary.GetToken(oAuth2Url, client_Id, client_Secret, claims, forceNewToken);
        }
    }
}
