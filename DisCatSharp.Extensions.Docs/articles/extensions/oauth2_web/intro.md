---
uid: extensions_oauth2_web_intro
title: OAuth2 Web Introduction
description: Introduction to OAuth2 Web
author: DisCatSharp Team
---

# Introduction to OAuth2 Web

> [!IMPORTANT]
> `DisCatSharp.Extensions.OAuth2Web` is deprecated.
> The replacement package is `DisCatSharp.Hosting.AspNetCore` in the main DisCatSharp repository.
>
> `DiscordOAuth2Client` is still supported. The deprecated piece is the old extension-hosted web layer.

You might encounter a situation where you need more than the normal api information about users.

For example, you might want to know the user's email address or their guilds. This is where OAuth2 Web comes in.

You can simply request the information through the oauth2 flow.

## Status

OAuth2 Web is now a **legacy package**.
Use it only when maintaining existing applications that still depend on it.

For new work, move to:

- `DiscordOAuth2Client` for the OAuth protocol flow
- `DisCatSharp.Hosting.AspNetCore` for callback hosting, interactions, webhook events, proxy helpers, and validation helpers

See [OAuth2 Web Migration](xref:extensions_oauth2_web_migration).

## Legacy installation

Simply install the NuGet package `DisCatSharp.Extensions.OAuth2Web` into your project.

![NuGet Package Search](/images/oauth2web_nuget.png)

## Requirements

See [OAuth2 Web Requirements](xref:extensions_oauth2_web_requirements).

## Usage

See [OAuth2 Web Usage](xref:extensions_oauth2_web_usage).
