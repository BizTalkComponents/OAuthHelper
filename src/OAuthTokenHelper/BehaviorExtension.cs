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

        private string oAuth_Url;
        private string tenant_Id;
        private string client_Id;
        private string client_Secret;
        private string resource;
        private string grant_Type;
        private bool cacheToken;

        public OAuthTokenSecurityBehavior(string oAuthUrl, string tenantId, string clientId, string clientSecret, string resource, string grantType, bool cacheToken)
        {
            this.oAuth_Url = oAuthUrl;
            this.tenant_Id = tenantId;
            this.client_Id = clientId;
            this.client_Secret = clientSecret;
            this.resource = resource;
            this.grant_Type = grantType;
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
            
            string token =  cacheToken ? AppDomainHelper.TokenDictionary.GetToken(oAuth_Url, tenant_Id, client_Id, client_Secret, resource, grant_Type)
                : TokenDictionary.GetNewToken(oAuth_Url, tenant_Id, client_Id, client_Secret, resource, grant_Type).Token;
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
            return new OAuthTokenSecurityBehavior(OAuthUrl, TenantId, ClientId, ClientSecret, Resource, GrantType, CacheToken);
        }

        [ConfigurationProperty("OAuthUrl", IsRequired = true)]
        public string OAuthUrl
        {
            get { return (string)this["OAuthUrl"]; }
            set { this["OAuthUrl"] = value; }
        }

        [ConfigurationProperty("TenantId", IsRequired = true)]
        public string TenantId
        {
            get { return (string)this["TenantId"]; }
            set { this["TenantId"] = value; }
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

        [ConfigurationProperty("Resource", IsRequired = true)]
        public string Resource
        {
            get { return (string)this["Resource"]; }
            set { this["Resource"] = value; }
        }

        [ConfigurationProperty("GrantType", IsRequired = true)]
        public string GrantType
        {
            get { return (string)this["GrantType"]; }
            set { this["GrantType"] = value; }
        }
        [ConfigurationProperty("CacheToken", IsRequired = false, DefaultValue = false)]
        public bool CacheToken
        {
            get { return (bool)this["CacheToken"]; }
            set { this["CacheToken"] = value; }
        }
    }

}
