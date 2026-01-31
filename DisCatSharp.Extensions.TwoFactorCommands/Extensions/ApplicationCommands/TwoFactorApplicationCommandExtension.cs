// This file is part of the DisCatSharp project.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Extensions.TwoFactorCommands.Entities;
using DisCatSharp.Extensions.TwoFactorCommands.Enums;
using DisCatSharp.Interactivity.Extensions;

using TwoFactorAuthNet;

namespace DisCatSharp.Extensions.TwoFactorCommands.ApplicationCommands;

public static class TwoFactorApplicationCommandExtension
{
	/// <summary>
	///     <para>Enrolls the user via modal input into two factor.</para>
	///     <para>This uses DisCatSharp.Interactivity.</para>
	///     <para>To be used as first action for application commands.</para>
	/// </summary>
	/// <param name="ctx">The base context.</param>
	/// <returns>A <see cref="TwoFactorResponse" />.</returns>
	public static async Task<TwoFactorResponse> EnrollTwoFactorAsync(this BaseContext ctx)
	{
		var ext = ctx.Client.GetTwoFactor() ?? throw new InvalidOperationException("Two factor extension is not registered on this client.");

		var secret = ext.TwoFactorClient.CreateSecret(160, CryptoSecureRequirement.RequireSecure);
		var label = $"{ext.Configuration.ResponseConfiguration.AuthenticatorAccountPrefix}: {HttpUtility.UrlEncode(ctx.User.Username)}";
		var otpauth = $"otpauth://totp/{label}?secret={secret}&issuer={ext.TwoFactorClient.Issuer}";
		var qrBlockText = otpauth.ToBlockText();

		DiscordInteractionModalBuilder builder = new("Register Two Factor");
		builder.AddTextDisplayComponent(new DiscordTextDisplayComponent($"## Scan this QR code with your authenticator app:\n{qrBlockText.BlockCode()}\n## Can't scan it? Enter this secret key instead:\n{secret.BlockCode()}"));
		builder.AddLabelComponent(new("Enter Code", "Enter the code given by your authenticator app.", new DiscordTextInputComponent(TextComponentStyle.Small, customId: "code", minLength: ext.Configuration.Digits, maxLength: ext.Configuration.Digits)));
		await ctx.CreateModalResponseAsync(builder);

		var response = new TwoFactorResponse
		{
			Client = ctx.Client
		};

		var inter = await ctx.Client.GetInteractivity().WaitForModalAsync(builder.CustomId, TimeSpan.FromSeconds(ext.Configuration.TwoFactorTimeout));
		if (inter.TimedOut)
		{
			response.Result = TwoFactorResult.TimedOut;

			return response;
		}

		response.ComponentInteraction = inter.Result;

		if (ext.Configuration.ResponseConfiguration.ShowResponse)
			await inter.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent("Checking..")], accentColor: DiscordColor.Blurple)));
		var codeResult = inter.Result.Interaction.Data.ModalComponents
			.OfType<DiscordLabelComponent>()
			.Select(component => component.Component as DiscordTextInputComponent)
			.FirstOrDefault(component => component?.CustomId == "code")
			?.Value;
		if (string.IsNullOrWhiteSpace(codeResult) || codeResult.Any(dig => !char.IsDigit(dig)))
		{
			if (ext.Configuration.ResponseConfiguration.ShowResponse)
				await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent(ext.Configuration.ResponseConfiguration.AuthenticationFailureMessage)], accentColor: DiscordColor.Red)));
			response.Result = TwoFactorResult.InvalidCode;
			return response;
		}

		var res = ext.TwoFactorClient.VerifyCode(secret, codeResult);
		if (res)
		{
			ext.EnrollUser(ctx.User.Id, secret);
			if (ext.Configuration.ResponseConfiguration.ShowResponse)
				await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent(ext.Configuration.ResponseConfiguration.AuthenticationEnrolledMessage)], accentColor: DiscordColor.Green)));

			response.Result = TwoFactorResult.Enrolled;

			return response;
		}

		if (ext.Configuration.ResponseConfiguration.ShowResponse)
			await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent(ext.Configuration.ResponseConfiguration.AuthenticationFailureMessage)], accentColor: DiscordColor.Red)));

		response.Result = TwoFactorResult.InvalidCode;

		return response;
	}

	/// <summary>
	///     <para>Unenrolls the user from two factor authentication via modal verification.</para>
	///     <para>This uses DisCatSharp.Interactivity.</para>
	///     <para>To be used as first action for application commands.</para>
	/// </summary>
	/// <param name="ctx">The base context.</param>
	/// <returns>A <see cref="TwoFactorResponse" />.</returns>
	public static async Task<TwoFactorResponse> UnenrollTwoFactorAsync(this BaseContext ctx)
	{
		var ext = ctx.Client.GetTwoFactor() ?? throw new InvalidOperationException("Two factor extension is not registered on this client.");

		if (!ext.IsEnrolled(ctx.User.Id))
		{
			if (ext.Configuration.ResponseConfiguration.ShowResponse)
				await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent(ext.Configuration.ResponseConfiguration.AuthenticationNotEnrolledMessage)], accentColor: DiscordColor.Orange)));

			return new()
			{
				Client = ctx.Client,
				Result = TwoFactorResult.NotEnrolled
			};
		}

		DiscordInteractionModalBuilder builder = new("Remove Two Factor");
		builder.AddTextDisplayComponent(new DiscordTextDisplayComponent("## To confirm removal, please enter your two factor authentication code below:"));
		builder.AddLabelComponent(new("Two Factor Code", "The code displayed in your authenticator app.", new DiscordTextInputComponent(TextComponentStyle.Small, customId: "code", minLength: ext.Configuration.Digits, maxLength: ext.Configuration.Digits)));
		await ctx.CreateModalResponseAsync(builder);

		var response = new TwoFactorResponse
		{
			Client = ctx.Client
		};

		var inter = await ctx.Client.GetInteractivity().WaitForModalAsync(builder.CustomId, TimeSpan.FromSeconds(ext.Configuration.TwoFactorTimeout));
		if (inter.TimedOut)
		{
			response.Result = TwoFactorResult.TimedOut;

			return response;
		}

		response.ComponentInteraction = inter.Result;

		if (ext.Configuration.ResponseConfiguration.ShowResponse)
			await inter.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent("Checking..")], accentColor: DiscordColor.Blurple)));
		var codeResult = inter.Result.Interaction.Data.ModalComponents
			.OfType<DiscordLabelComponent>()
			.Select(component => component.Component as DiscordTextInputComponent)
			.FirstOrDefault(component => component?.CustomId == "code")
			?.Value;
		if (string.IsNullOrWhiteSpace(codeResult) || codeResult.Any(dig => !char.IsDigit(dig)))
		{
			if (ext.Configuration.ResponseConfiguration.ShowResponse)
				await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent(ext.Configuration.ResponseConfiguration.AuthenticationFailureMessage)], accentColor: DiscordColor.Red)));
			response.Result = TwoFactorResult.InvalidCode;
			return response;
		}

		var res = ext.IsValidCode(ctx.User.Id, codeResult);
		if (res)
		{
			ext.DisenrollUser(ctx.User.Id);
			if (ext.Configuration.ResponseConfiguration.ShowResponse)
				await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent("Two factor authentication has been removed from your account.")], accentColor: DiscordColor.Green)));

			response.Result = TwoFactorResult.Unenrolled;

			return response;
		}

		if (ext.Configuration.ResponseConfiguration.ShowResponse)
			await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent(ext.Configuration.ResponseConfiguration.AuthenticationFailureMessage)], accentColor: DiscordColor.Red)));

		response.Result = TwoFactorResult.InvalidCode;

		return response;
	}

	/// <summary>
	///     <para>Asks the user via modal input for the two factor code.</para>
	///     <para>This uses DisCatSharp.Interactivity.</para>
	///     <para>To be used as first action for application commands.</para>
	/// </summary>
	/// <param name="ctx">The base context.</param>
	/// <returns>A <see cref="TwoFactorResponse" />.</returns>
	public static async Task<TwoFactorResponse> RequestTwoFactorAsync(this BaseContext ctx)
	{
		var ext = ctx.Client.GetTwoFactor() ?? throw new InvalidOperationException("Two factor extension is not registered on this client.");

		if (!ext.IsEnrolled(ctx.User.Id))
		{
			if (ext.Configuration.ResponseConfiguration.ShowResponse)
				await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent(ext.Configuration.ResponseConfiguration.AuthenticationNotEnrolledMessage)], accentColor: DiscordColor.Orange)));

			return new()
			{
				Client = ctx.Client,
				Result = TwoFactorResult.NotEnrolled
			};
		}

		DiscordInteractionModalBuilder builder = new(ext.Configuration.ResponseConfiguration.AuthenticationModalRequestTitle);
		builder.AddTextDisplayComponent(new DiscordTextDisplayComponent("Please enter your two factor authentication code below:"));
		builder.AddLabelComponent(new("Two Factor Code", "The code displayed in your authenticator app.", new DiscordTextInputComponent(TextComponentStyle.Small, customId: "code", minLength: ext.Configuration.Digits, maxLength: ext.Configuration.Digits)));
		await ctx.CreateModalResponseAsync(builder);

		var response = new TwoFactorResponse
		{
			Client = ctx.Client
		};

		var inter = await ctx.Client.GetInteractivity().WaitForModalAsync(builder.CustomId, TimeSpan.FromSeconds(ext.Configuration.TwoFactorTimeout));
		if (inter.TimedOut)
		{
			response.Result = TwoFactorResult.TimedOut;

			return response;
		}

		response.ComponentInteraction = inter.Result;

		if (ext.Configuration.ResponseConfiguration.ShowResponse)
			await inter.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent("Checking..")], accentColor: DiscordColor.Blurple)));
		var codeResult = (inter.Result.Interaction.Data.ModalComponents.OfType<DiscordLabelComponent>().First().Component as DiscordTextInputComponent)!.Value!;
		if (codeResult.Any(dig => !char.IsDigit(dig)))
		{
			if (ext.Configuration.ResponseConfiguration.ShowResponse)
				await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent(ext.Configuration.ResponseConfiguration.AuthenticationFailureMessage)], accentColor: DiscordColor.Red)));
			response.Result = TwoFactorResult.InvalidCode;
			return response;
		}

		var res = ext.IsValidCode(ctx.User.Id, codeResult);
		if (res)
		{
			if (ext.Configuration.ResponseConfiguration.ShowResponse)
				await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent(ext.Configuration.ResponseConfiguration.AuthenticationSuccessMessage)], accentColor: DiscordColor.Green)));

			response.Result = TwoFactorResult.ValidCode;

			return response;
		}

		if (ext.Configuration.ResponseConfiguration.ShowResponse)
			await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent(ext.Configuration.ResponseConfiguration.AuthenticationFailureMessage)], accentColor: DiscordColor.Red)));

		response.Result = TwoFactorResult.InvalidCode;

		return response;
	}

	/// <summary>
	///     <para>Asks the user via modal input for the two factor code.</para>
	///     <para>This uses DisCatSharp.Interactivity.</para>
	///     <para>To be used as first action for button.</para>
	/// </summary>
	/// <param name="evt">The interaction context.</param>
	/// <param name="client">The discord client to use.</param>
	/// <returns>A <see cref="TwoFactorResponse" />.</returns>
	public static async Task<TwoFactorResponse> RequestTwoFactorAsync(this ComponentInteractionCreateEventArgs evt, DiscordClient client)
	{
		var ext = client.GetTwoFactor() ?? throw new InvalidOperationException("Two factor extension is not registered on this client.");

		if (!ext.IsEnrolled(evt.User.Id))
		{
			if (ext.Configuration.ResponseConfiguration.ShowResponse)
				await evt.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent(ext.Configuration.ResponseConfiguration.AuthenticationNotEnrolledMessage)], accentColor: DiscordColor.Orange)));

			return new()
			{
				Client = client,
				Result = TwoFactorResult.NotEnrolled
			};
		}

		DiscordInteractionModalBuilder builder = new(ext.Configuration.ResponseConfiguration.AuthenticationModalRequestTitle);
		builder.AddTextDisplayComponent(new DiscordTextDisplayComponent("Please enter your two factor authentication code below:"));
		builder.AddLabelComponent(new("Two Factor Code", "The code displayed in your authenticator app.", new DiscordTextInputComponent(TextComponentStyle.Small, customId: "code", minLength: ext.Configuration.Digits, maxLength: ext.Configuration.Digits)));
		await evt.Interaction.CreateInteractionModalResponseAsync(builder);

		var response = new TwoFactorResponse
		{
			Client = client
		};

		var inter = await client.GetInteractivity().WaitForModalAsync(builder.CustomId, TimeSpan.FromSeconds(ext.Configuration.TwoFactorTimeout));
		if (inter.TimedOut)
		{
			response.Result = TwoFactorResult.TimedOut;

			return response;
		}

		response.ComponentInteraction = inter.Result;

		if (ext.Configuration.ResponseConfiguration.ShowResponse)
			await inter.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent("Checking..")], accentColor: DiscordColor.Blurple)));
		var codeResult = (inter.Result.Interaction.Data.ModalComponents.OfType<DiscordLabelComponent>().First().Component as DiscordTextInputComponent)!.Value!;
		if (codeResult.Any(dig => !char.IsDigit(dig)))
		{
			if (ext.Configuration.ResponseConfiguration.ShowResponse)
				await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent(ext.Configuration.ResponseConfiguration.AuthenticationFailureMessage)], accentColor: DiscordColor.Red)));
			response.Result = TwoFactorResult.InvalidCode;
			return response;
		}

		var res = ext.IsValidCode(evt.User.Id, codeResult);
		if (res)
		{
			if (ext.Configuration.ResponseConfiguration.ShowResponse)
				await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent(ext.Configuration.ResponseConfiguration.AuthenticationSuccessMessage)], accentColor: DiscordColor.Green)));

			response.Result = TwoFactorResult.ValidCode;

			return response;
		}

		if (ext.Configuration.ResponseConfiguration.ShowResponse)
			await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithV2Components().AddComponents(new DiscordContainerComponent([new DiscordTextDisplayComponent("Two Factor"), new DiscordTextDisplayComponent(ext.Configuration.ResponseConfiguration.AuthenticationFailureMessage)], accentColor: DiscordColor.Red)));

		response.Result = TwoFactorResult.InvalidCode;

		return response;
	}
}
