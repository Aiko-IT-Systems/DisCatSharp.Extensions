---
uid: extensions_oauth2_web_usage
title: OAuth2 Web Usage
description: Using OAuth2 Web
---

# OAuth2 Web Usage

## Setup

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

## Starting and stopping the oauth server

After u done this you can start the oauth server at any time by calling [Start](xref:DisCatSharp.Extensions.OAuth2Web.OAuth2WebExtension.Start*) on your [OAuth2WebExtension](xref:DisCatSharp.Extensions.OAuth2Web.OAuth2WebExtension) instance:

```cs
client.GetOAuth2WebExtension().Start();
```

This method is a void method and won't return anything.

To stop the oauth server call [StopAsync](xref:DisCatSharp.Extensions.OAuth2Web.OAuth2WebExtension.StopAsync*) on your [OAuth2WebExtension](xref:DisCatSharp.Extensions.OAuth2Web.OAuth2WebExtension) instance:

```cs
await client.GetOAuth2WebExtension().StopAsync();
```

## Requesting user information

```cs
[SlashCommand("start_oauth2", "Starts OAuth2")]
public static async Task StartOAuth2Async(InteractionContext ctx)
{
	await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
		new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Please wait.."));

	var web = ctx.Client.GetOAuth2Web();

	var uri = web.OAuth2Client.GenerateOAuth2Url(
		"identify connections",
		web.OAuth2Client.GenerateSecureState(ctx.User.Id));

	web.SubmitPendingOAuth2Url(uri);

	await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{uri.AbsoluteUri}"));

	var res = await web.WaitForAccessTokenAsync(ctx.User, uri, TimeSpan.FromMinutes(1));
	if (!res.TimedOut)
	{
		var testData = await web.OAuth2Client.GetCurrentUserConnectionsAsync(res.Result.DiscordAccessToken);
		await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
			$"{testData.Count} total connections. First connection username: {testData.First().Name}"));
		await web.RevokeAccessTokenAsync(ctx.User);
	}
	else
		await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Timed out :("));
}
```
