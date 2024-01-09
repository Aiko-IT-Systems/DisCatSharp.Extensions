---
uid: extensions_oauth2_web_usage_sharded
title: OAuth2 Web Usage (Sharded)
description: Using OAuth2 Web (Sharded)
---

# OAuth2 Web Usage (Sharded)

## Setup

Enable the extension by calling [UseOAuth2WebAsync](xref:DisCatSharp.Extensions.OAuth2Web.ExtensionMethods.UseOAuth2WebAsync*) on your [DiscordShardedClient](xref:DisCatSharp.DiscordShardedClient) instance:

```cs
using DisCatSharp.Extensions.OAuth2Web;

// ...

await shardedClient.UseOAuth2WebAsync(new OAuth2WebConfiguration
{
    ClientId = 1234567890, // Your application's client ID
    ClientSecret = "your_client_secret", // Your application's client secret
    RedirectUri = "http(s)://(sub).domain.tld/oauth/" // Your application's redirect URI
	// ... other options
});
```

## Starting and stopping the oauth server

After u done this you can start the oauth server at any time by calling [Start](xref:DisCatSharp.Extensions.OAuth2Web.OAuth2WebExtension.Start*) on your [OAuth2WebExtension](xref:DisCatSharp.Extensions.OAuth2Web.OAuth2WebExtension) instances:

```cs
var oauth2Extensions = await shardedClient.GetOAuth2WebAsync();
oauth2Extensions.Start();
```

This method is a void method and won't return anything.

To stop the oauth server call [StopAsync](xref:DisCatSharp.Extensions.OAuth2Web.OAuth2WebExtension.StopAsync*) on your [OAuth2WebExtension](xref:DisCatSharp.Extensions.OAuth2Web.OAuth2WebExtension) instances:

```cs
var oauth2Extensions = await shardedClient.GetOAuth2WebAsync();
await oauth2Extensions.StopAsync();
```
