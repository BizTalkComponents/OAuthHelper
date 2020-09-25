
# OAuth Helper
OAuth helper is a WCF endpoint behavior extensions allows to get the security token by calling an OAuth2 server with a set of properties, and adds the Authorization header to the request, it can also cache the token, so the token can be used for the following token as long as it is valid.

## Extension's Properties
| Property | Type| Description |
|--|--|--|
| OAuthUrl | String (Required) | The OAuth2.0 server's URL |
| Client Id | String (Required) |  self explained |
| Client Secret | String (Required) |  self explained |
| Tenant Id | String (Required) |  self explained |
| Grant Type | String (Required) |  self explained |
| Resource | String (Required) |  self explained |
| CacheToken | Boolean |  If True, the token will be cached |

## Installation
Download and install the latest release.
This extension is designed to be used with BizTalk, but it can also be used with other applications.
There are two ways to add this extension to the configuration, it can be added to the global machine.config file, or it can be added to the host instance only.

###  Adding to machine.config
the extension must be added to boath .Net framework versions (32 bit and 64 bit).


`%windir%\Microsoft.Net\Framework\<.NET Version>\config\machine.config`

`%windir%\Microsoft.Net\Framework64\<.NET Version>\config\machine.config`


locate behaviorExtensions section and add the following line

    <add name="OAuthHelper" type="BizTalkComponents.CustomComponents.OAuthTokenHelper.OAuthTokenSecurityBehaviorElement, BizTalkComponents.CustomComponents.OAuthTokenHelper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=f8600b34c8012b7b" />
### Adding to BizTalk Host Instance
In BizTalk Administrator Console, go to Platform Settings --> Adapters  and select the WCF adapter you would like to add the extension to, then select the send handler you want to make the extension available with, right click on it and then select properties, Click Properties button in Adapter Handler Properties, then select WCF extensions, copy the code below to a new Xml file then click on Import.

	<configuration>
	  <system.serviceModel>
	    <extensions>
	      <behaviorExtensions>
	            <add name="OAuthHelper" type="BizTalkComponents.CustomComponents.OAuthTokenHelper.OAuthTokenSecurityBehaviorElement, BizTalkComponents.CustomComponents.OAuthTokenHelper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=f8600b34c8012b7b" />
	      </behaviorExtensions>
	    </extensions>
	  </system.serviceModel>
	</configuration>