using System;
using Newtonsoft.Json;

namespace BizTalkComponents.CustomComponents.OAuthTokenHelper
{

    //[DataContract(Namespace = "", Name = "root")]
    [Serializable]
    public class TokenInfo
    {
        internal bool IsWaiting;
        //[DataMember(Name = "token_type", Order = 0)]
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        //[DataMember(Name = "expires_in", Order = 1)]
        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }

        //[DataMember(Name = "ext_expires_in", Order = 2)]
        [JsonProperty(PropertyName = "ext_expires_in")]
        public int ExtExpiresIn { get; set; }

        //[DataMember(Name = "expires_on", Order = 3)]
        [JsonProperty(PropertyName = "expires_on")]
        private long expires_on;
        public DateTime ExpiresOn { get { return (new DateTime(1970, 1, 1)).AddSeconds(expires_on).ToLocalTime(); } }

        //[DataMember(Name = "not_before", Order = 4)]
        [JsonProperty(PropertyName = "not_before")]
        private long not_before;

        public DateTime NotBefore { get { return (new DateTime(1970, 1, 1)).AddSeconds(not_before).ToLocalTime(); } }

        //[DataMember(Name = "resource", Order = 5)]
        [JsonProperty(PropertyName = "resource")]
        public string Resource { get; set; }

        //[DataMember(Name = "access_token", Order = 6)]
        [JsonProperty(PropertyName = "access_token")]
        public string Token { get; set; }

        public bool IsValid
        {
            get { return (ExpiresOn > DateTime.Now.AddMinutes(5)); }
        }

        public DateTime Created { get; private set; }

        internal void UpdateFrom(TokenInfo ti)
        {
            TokenType = ti.TokenType;
            Token = ti.Token;
            ExpiresIn = ti.ExpiresIn;
            expires_on = ti.expires_on;
            ExtExpiresIn = ti.ExtExpiresIn;
            not_before = ti.not_before;
            Resource = ti.Resource;
            Created = ti.Created;
        }
    }
}
