---
uid: extensions_oauth2_web_migration
title: OAuth2 Web Migration
description: Migrating away from OAuth2 Web
author: DisCatSharp Team
---

# OAuth2 Web Migration

`DisCatSharp.Extensions.OAuth2Web` is deprecated.

The replacement package is `DisCatSharp.Hosting.AspNetCore` in the main DisCatSharp repository.

> [!IMPORTANT]
> This migration does **not** deprecate `DiscordOAuth2Client`.
> It deprecates the old extension-hosted web layer.

## Why migrate

The old extension was built around a self-hosted extension model.
The new package moves this into the first-party hosting family and expands the scope beyond OAuth callbacks.

The new package adds:

- existing ASP.NET Core endpoint registration
- self-hosted ingress without the old extension model
- signed HTTP interactions
- signed webhook events
- proxy helpers
- validation helpers

## API mapping

| OAuth2Web | ASP.NET Core ingress |
| --- | --- |
| `UseOAuth2Web(...)` | `AddDisCatSharpAspNetCore(...)` |
| `UseOAuth2WebAsync(...)` | `AddDisCatSharpAspNetCore(...)` or `AddDisCatSharpAspNetCoreSelfHost(...)` |
| `Start()` / `StopAsync()` | `app.MapDisCatSharpIngress()` or self-hosted host lifecycle |
| Apache-only helper methods | `DiscordIngressProxyHelpers` |
| redirect URI helper checks on the extension | `DiscordIngressConfigurationValidator` |

## What to move first

1. move callback hosting to `DisCatSharp.Hosting.AspNetCore`
2. keep using `DiscordOAuth2Client` for URL generation, state, and token exchange concerns
3. update your proxy and portal validation flow to the new helper surface
4. remove the old extension registration from your bot startup

## NuGet deprecation

The package is deprecated in its package metadata and documentation, and package owners should also mark its published NuGet versions deprecated with `DisCatSharp.Hosting.AspNetCore` as the migration target.
