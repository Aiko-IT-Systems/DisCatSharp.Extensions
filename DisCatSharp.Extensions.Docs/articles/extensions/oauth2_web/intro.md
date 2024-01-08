---
uid: extensions_oauth2_web_intro
title: OAuth2 Web Introduction
---

# Introduction to OAuth2 Web

## Using OAuth2 Web

### Installation

Install the NuGet package `DisCatSharp.Extensions.OAuth2Web` into your project.

Enable the extension by calling [UseOAuth2Web](xref:DisCatSharp.Extensions.OAuth2Web.ExtensionMethods.UseOAuth2Web*) on your [DiscordClient](xref:DisCatSharp.DiscordClient) instance:

```cs
using DisCatSharp.Extensions.OAuth2Web;

// ...

client.UseOAuth2Web(new OAuth2WebConfiguration
{
    ClientId = 1234567890, // Your application's client ID
    ClientSecret = "your_client_secret", // Your application's client secret
    RedirectUri = "http(s)://(sub).domain.tld/oauth/" // Your application's redirect URI
	// ... other options
});
```
