---
uid: extensions_oauth2_web_requirements
title: OAuth2 Web Requirements
description: Requirements for OAuth2 Web
---

# OAuth2 Web Requirements

To use OAuth2 Web, you will need a few things:

- A web server which can act as proxy for the OAuth2 flow. This can be a simple web server like nginx or apache, or a more complex one like ASP.NET Core.
- A domain name for your web server (optional)
- A valid SSL certificate for your domain (optional)
- A Discord application with a redirect URI set to your web server's domain or IP address
- Your application's client ID and client secret
- Free ports on your host (Especially if you are using OAuth2Web with shards)

## Web Server

To make things easier, we've added a method in the extension to generate proxy configurations for apache.

To use them, follow the first steps of the [usage article](xref:extensions_oauth2_web_usage) and then call [GenerateApacheProxyConfigAsync](xref:DisCatSharp.Extensions.OAuth2Web.OAuth2WebExtensionUtilities.GenerateApache2ProxyFileAsync*) on your [DiscordClient](xref:DisCatSharp.DiscordClient) instance:

```cs
await client.GenerateApache2ProxyFileAsync();
```

This will generate a file called `dcs_oauth2web_proxy.conf` in the current working directory.

The content of these files contains only the proxy configuration. You can use that to add it to your full apache configuration.

An overload for the [DiscordShardedClient](xref:DisCatSharp.DiscordShardedClient) is also available:

```cs
await shardedClient.GenerateApache2ProxyFileAsync();
```

The generated file will be called `dcs_oauth2web_proxy_sharded.conf` and contains the proxy configurations for all shards.

### Example generated configuration

```apache
    ProxyRequests Off
    ProxyPreserveHost On
    ProxyPass /oauth/ http://127.0.0.1:42069/oauth/
    ProxyPassReverse /oauth/ http://127.0.0.1:42069/oauth/
```

### Example generated configuration (sharded)

```apache
    ProxyRequests Off
    ProxyPreserveHost On
    ProxyPass /oauth/s0/ http://127.0.0.1:42069/oauth/s0/
    ProxyPassReverse /oauth/s0/ http://127.0.0.1:42069/oauth/s0/
    ProxyPass /oauth/s1/ http://127.0.0.1:42070/oauth/s1/
    ProxyPassReverse /oauth/s1/ http://127.0.0.1:42070/oauth/s1/
```

In case you want to manually configure it yourself, follow the official instructions of your web server about how to set up a reverse proxy.

In any case it should behave the same way as the example above.

As you probably noticed, we don't use a SSL proxy here. We might support that in the future, but for now, the connection between your web server (proxy) and your bot (backend) cannot be served over HTTPS.

## Redirect URI(s)

To use OAuth2 Web, you will need to set up a redirect URI for your Discord application.

We've added multiple methods to make the setup a bit easier.

For the [DiscordClient](xref:DisCatSharp.DiscordClient) you only need to set up one redirect URI in the format of `http(s)://(sub).domain.tld/oauth/`.

For the [DiscordShardedClient](xref:DisCatSharp.DiscordShardedClient) you can call [GetRequiredRedirectUris](xref:DisCatSharp.Extensions.OAuth2Web.ExtensionMethods.GetRequiredRedirectUris*) on your extension list to get a list of all required redirect URIs:

```cs
var oauth2web = await shardedClient.GetOAuth2WebAsync();
var redirectUris = oauth2web.GetRequiredRedirectUris();
Console.WriteLine(string.Join("\n", redirectUris));
```

To check on startup whether all redirect uris are set you can use these methods:

```cs
// For non-sharded clients
bool hasRequiredUri = client.GetOAuth2Web().HasRequiredRedirectUriSet();

// For sharded clients
var oauth2web = await shardedClient.GetOAuth2WebAsync();
bool hasRequiredUris = oauth2web.HasAllRequiredRedirectUrisSet();
```
