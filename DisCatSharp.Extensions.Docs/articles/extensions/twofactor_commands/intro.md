---
uid: extensions_twofactor_commands_intro
title: TwoFactor Commands Introduction
---

# Introduction to TwoFactor Commands


## Using TwoFactor Commands

### Installation

Install the NuGet package `DisCatSharp.Extensions.TwoFactorCommands` into your project. Currently only available as prerelease.

You'll also need `DiscordSharp.Interactivity`.

Enable the extension by calling [UseTwoFactor](xref:DisCatSharp.Extensions.TwoFactorCommands.ExtensionMethods#DisCatSharp_Extensions_TwoFactorCommands_ExtensionMethods_UseTwoFactor_DiscordClient_DisCatSharp_Extensions_TwoFactorCommands_TwoFactorConfiguration_) on your [DiscordClient](xref:DisCatSharp.DiscordClient) instance:

```cs
using DisCatSharp.Extensions.TwoFactorCommands;

// ...

client.UseTwoFactor();
```

### Basic Operations

#### Enrolling a user in two factor

To enroll a user in two factor, call [EnrollTwoFactor(DiscordUser.Id)](xref:DisCatSharp.Extensions.TwoFactorCommands.TwoFactorExtensionUtilities#DisCatSharp_Extensions_TwoFactorCommands_TwoFactorExtensionUtilities_EnrollTwoFactor_DiscordClient_DiscordUser_) on your [DiscordClient](xref:DisCatSharp.DiscordClient) instance.

```cs
using DisCatSharp.Extensions.TwoFactorCommands;

// ...
[SlashCommand("enroll", "Enroll in two factor")]
public static async Task EnrollTwoFactor(InteractionContext ctx)
{
    // ...
	var (Secret, QrCode) = ctx.Client.EnrollTwoFactor(ctx.User.Id);

    // Either send the QR code to the user, or the secret.
    // QrCode is a MemoryStream you can use with DiscordWebhookBuilder.AddFile as example.
}
```

Example way to ask a user to register their two factor:

![Example](/images/two_factor_enrollment_message_example.png)

#### Disenrolling a user in two factor

To disenroll a user from two factor, call [DisenrollTwoFactor(DiscordUser.Id)](xref:DisCatSharp.Extensions.TwoFactorCommands.TwoFactorExtensionUtilities#DisCatSharp_Extensions_TwoFactorCommands_TwoFactorExtensionUtilities_DisenrollTwoFactor_DiscordClient_System_UInt64_) on your [DiscordClient](xref:DisCatSharp.DiscordClient) instance.

```cs
using DisCatSharp.Extensions.TwoFactorCommands;

// ...
[SlashCommand("disenroll", "Disenroll from two factor"), ApplicationCommandRequireEnrolledTwoFactor]
public static async Task DisenrollTwoFactor(InteractionContext ctx)
{
    // ...
	ctx.Client.DisenrollTwoFactor(ctx.User.Id);
}
```

#### Check if a user is enrolled in two factor

To check the enrollment of a user, use the function [CheckTwoFactorEnrollmentFor(DiscordUser.Id)](xref:DisCatSharp.Extensions.TwoFactorCommands.TwoFactorExtensionUtilities#DisCatSharp_Extensions_TwoFactorCommands_TwoFactorExtensionUtilities_CheckTwoFactorEnrollmentFor_DiscordClient_System_UInt64_) on your [DiscordClient](xref:DisCatSharp.DiscordClient) instance.


```cs
using DisCatSharp.Extensions.TwoFactorCommands;

// ...

ctx.Client.CheckTwoFactorEnrollmentFor(ctx.User.Id);

// ...
```

### Using TwoFactor with Application Commands

To force a command to require two factor, use the [ApplicationCommandRequireEnrolledTwoFactorAttribute](xref:DisCatSharp.ApplicationCommands.Attributes.ApplicationCommandRequireEnrolledTwoFactorAttribute) attribute.

```cs
[SlashCommand("some_tfa_command", "This command can only be used with tfa"), ApplicationCommandRequireEnrolledTwoFactor]
```

To ask a user to submit their two factor code, use the function [RequestTwoFactorAsync()](xref:DisCatSharp.Extensions.TwoFactorCommands.ApplicationCommands.TwoFactorApplicationCommandExtension#DisCatSharp_Extensions_TwoFactorCommands_ApplicationCommands_TwoFactorApplicationCommandExtension_RequestTwoFactorAsync_BaseContext_) on your [BaseContext](xref:DisCatSharp.ApplicationCommands.Context.BaseContext).

It returns a [TwoFactorResponse](xref:DisCatSharp.Extensions.TwoFactorCommands.Properties.TwoFactorResponse).

```cs
var tfa_result = ctx.RequestTwoFactorAsync();

if (tfa_result != TwoFactorResponse.Result.ValidCode)
{
	// Handle incorrect code
    return;
}

// Do your stuff
```

### Using TwoFactor with Buttons

Using two factor authentication on buttons is pretty similar to slash commands but it'll need a DiscordClient to attach to.

Run [RequestTwoFactorAsync()](xref:DisCatSharp.Extensions.TwoFactorCommands.ApplicationCommands.TwoFactorApplicationCommandExtension#DisCatSharp_Extensions_TwoFactorCommands_ApplicationCommands_TwoFactorApplicationCommandExtension_RequestTwoFactorAsync_BaseContext_) on your [ComponentInteractionCreateEventArgs](xref:DisCatSharp.EventArgs.ComponentInteractionCreateEventArgs) to ask the user for the two factor auth code.

Same deal as for slash commands, it'll return a [TwoFactorResponse](xref:DisCatSharp.Extensions.TwoFactorCommands.Properties.TwoFactorResponse).

```cs
async Task SomeButtonInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e)
{
    var tfa_result = await e.RequestTwoFactorAsync(sender);

    if (tfa_result != TwoFactorResponse.Result.ValidCode)
    {
        // Handle incorrect code
        return;
    }

    // Do your stuff
}
```

The user will be asked to submit their two factor code like this:

![Example](/images/two_factor_request_example.png)