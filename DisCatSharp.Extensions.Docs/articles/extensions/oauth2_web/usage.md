---
uid: extensions_oauth2_web_usage
title: OAuth2 Web Usage
description: Using OAuth2 Web
---

# OAuth2 Web Usage

## Setup

# [Setup](#tab/single)

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

After u done this you can start the oauth server at any time by calling [Start](xref:DisCatSharp.Extensions.OAuth2Web.OAuth2WebExtension.Start*) on your [OAuth2WebExtension](xref:DisCatSharp.Extensions.OAuth2Web.OAuth2WebExtension) instance:

```cs
client.GetOAuth2WebExtension().Start();
```

This method is a void method and won't return anything.

To stop the oauth server call [StopAsync](xref:DisCatSharp.Extensions.OAuth2Web.OAuth2WebExtension.StopAsync*) on your [OAuth2WebExtension](xref:DisCatSharp.Extensions.OAuth2Web.OAuth2WebExtension) instance:

```cs
await client.GetOAuth2WebExtension().StopAsync();
```

# [Setup (sharded)](#tab/sharded)

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
---

## Requesting access tokens

This is an example of how to request an access token from a user and to access user connections.

```cs
[SlashCommand("test_oauth2", "Test OAuth2 Web")]
public static async Task TestOAuth2Async(InteractionContext ctx)
{
    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
        new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Please wait.."));

    // Get the oauth2 web extension
    var oauth2 = ctx.Client.GetOAuth2Web();

    // Generate the oauth2 url with the additional connections scope
    // We assume you set SecureStates to true in the configuration
    var uri = oauth2.OAuth2Client.GenerateOAuth2Url(
        "identify connections",
        oauth2.OAuth2Client.GenerateSecureState(ctx.User.Id));

    // Add the pending oauth2 url to the oauth2 web extension
    oauth2.SubmitPendingOAuth2Url(uri);

    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Please authorize via oauth at: {uri.AbsoluteUri}"));

    // Give them a minute to authorize
    // This method waits for the automatic handling of access token receiving and exchange
    var requestWaiterTask = await oauth2.WaitForAccessTokenAsync(ctx.User, uri, TimeSpan.FromMinutes(1));

    // Use the access token if the request hasn't timed out, otherwise respond with a timeout message
    if (!requestWaiterTask.TimedOut)
    {
        // Get the user connections
        var connections = await oauth2.OAuth2Client.GetCurrentUserConnectionsAsync(requestWaiterTask.Result.DiscordAccessToken);

        // Respond with the connection count and the first connection's username
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            $"{connections.Count} total connections\n\nFirst connection username: {connections.First().Name}"));

        // Revoke the access token, we don't need it anymore for this example
        await oauth2.RevokeAccessTokenAsync(ctx.User);

        // Inform the user that we revoked the access token
        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Revoked access token"));
    }
    else
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Timed out"));
}
```
