---
uid: extensions_twofactor_commands_intro
title: TwoFactor Commands Introduction
---

# Introduction to TwoFactor Commands


## Using TwoFactor Commands

### Installation

Install the NuGet package `DisCatSharp.Extensions.TwoFactorCommands` into your project. Currently only available as prerelease.

Enable the extension by calling [UseTwoFactor](xref:DisCatSharp.Extensions.TwoFactorCommands.ExtensionMethods#DisCatSharp_Extensions_TwoFactorCommands_ExtensionMethods_UseTwoFactor_DiscordClient_DisCatSharp_Extensions_TwoFactorCommands_TwoFactorConfiguration_) on your `DiscordClient` instance:

```cs
using DisCatSharp.Extensions.TwoFactorCommands;

// ...

client.UseTwoFactor();
```

### Basic Operations



### Using TwoFactor with Application Commands
