using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace BizTalkComponents.CustomComponents.OAuthTokenHelper
{
    public class OAuthTokenSecurityBehavior : IClientMessageInspector, IEndpointBehavior
    {

        private string OAuth2TokenEP;
        private string client_Id;
        private string client_Secret;
        private bool cacheToken;
        private string claims;

        public OAuthTokenSecurityBehavior(string oAuth2TokenEP, string clientId, string clientSecret, string claims, bool cacheToken)
        {
            this.claims = claims;
            this.OAuth2TokenEP = oAuth2TokenEP;
            this.client_Id = clientId;
            this.client_Secret = clientSecret;
            this.cacheToken = cacheToken;
        }

        #region IClientMessageInspector

        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            // do nothing
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel)
        {
            HttpRequestMessageProperty httpRequest = null;
            if (request.Properties.ContainsKey(HttpRequestMessageProperty.Name))
            {
                httpRequest = request.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            }

            if (httpRequest == null)
            {
                httpRequest = new HttpRequestMessageProperty()
                {
                    Method = "GET",
                    SuppressEntityBody = true
                };
                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequest);
            }
            WebHeaderCollection headers = httpRequest.Headers;

            string token = cacheToken ? AppDomainHelper.TokenDictionary.GetToken(OAuth2TokenEP, client_Id, client_Secret, claims)
                : TokenDictionary.GetNewToken(OAuth2TokenEP, client_Id, client_Secret, claims).Token;
            //Remove the authorization header if already exists.
            headers.Remove(HttpRequestHeader.Authorization);
            headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
            return null;
        }

        #endregion IClientMessageInspector

        #region IEndpointBehavior

        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {

        }

        #endregion IEndpointBehavior
    }

    public class OAuthTokenSecurityBehaviorElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(OAuthTokenSecurityBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new OAuthTokenSecurityBehavior(OAuth2TokenEP, ClientId, ClientSecret, Claims, CacheToken);
        }

        [ConfigurationProperty("OAuth 2.0 Token Endpoint", IsRequired = true)]
        public string OAuth2TokenEP
        {
            get { return (string)this["OAuth2TokenEP"]; }
            set { this["OAuth2TokenEP"] = value; }
        }


        [ConfigurationProperty("ClientId", IsRequired = true)]
        public string ClientId
        {
            get { return (string)this["ClientId"]; }
            set { this["ClientId"] = value; }
        }

        [ConfigurationProperty("ClientSecret", IsRequired = true)]
        public string ClientSecret
        {
            get { return (string)this["ClientSecret"]; }
            set { this["ClientSecret"] = value; }
        }

        [ConfigurationProperty("Claims", IsRequired = false,DefaultValue ="")]
        [RegexStringValidator(@"(\w+?=.+?(&|$))+")]
        public string Claims
        {
            get { return (string)this["Claims"]; }
            set { this["Claims"] = value; }
        }

        [ConfigurationProperty("CacheToken", IsRequired = false, DefaultValue = false)]
        public bool CacheToken
        {
            get { return (bool)this["CacheToken"]; }
            set { this["CacheToken"] = value; }
        }
    }

}
