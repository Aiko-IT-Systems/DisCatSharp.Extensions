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

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Extensions.TwoFactorCommands.Enums;
using DisCatSharp.Extensions.TwoFactorCommands.Properties;
using DisCatSharp.Interactivity.Extensions;

namespace DisCatSharp.Extensions.TwoFactorCommands.ApplicationCommands;

public static class TwoFactorApplicationCommandExtension
{
	/// <summary>
	/// <para>Asks the user via modal input for the two factor code.</para>
	/// <para>This uses DisCatSharp.Interactivity.</para>
	/// <para>To be used as first action for application commands.</para>
	/// </summary>
	/// <param name="ctx">The base context.</param>
	/// <returns>A <see cref="TwoFactorResult"/>.</returns>
	public static async Task<TwoFactorResponse> RequestTwoFactorAsync(this BaseContext ctx)
	{
		var ext = ctx.Client.GetTwoFactor();
		/*
		if (!ext.IsEnrolled(ctx.User.Id))
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("You are not enrolled in two factor."));
			return TwoFactorResponse.NotEnrolled;
		}
		*/

		DiscordInteractionModalBuilder builder = new(ext.Configuration.ResponseConfiguration.AuthenticationModalRequestTitle);
		builder.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "code", "Code", "123456", ext.Configuration.Digits, ext.Configuration.Digits));
		await ctx.CreateModalResponseAsync(builder);

		var response = new TwoFactorResponse();

		var inter = await ctx.Client.GetInteractivity().WaitForModalAsync(builder.CustomId, TimeSpan.FromSeconds(ext.Configuration.TwoFactorTimeout));
		if (inter.TimedOut)
		{
			response.Result = TwoFactorResult.TimedOut;
			return response;
		}

		response.Interaction = inter.Result.Interaction;

		if (ext.Configuration.ResponseConfiguration.ShowResponse)
			await inter.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Checking.."));
		var res = ext.IsValidCode(ctx.User.Id, inter.Result.Interaction.Data.Components.First().Value);
		if (res)
		{
			if (ext.Configuration.ResponseConfiguration.ShowResponse)
				await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(ext.Configuration.ResponseConfiguration.AuthenticationSuccessMessage));

			response.Result = TwoFactorResult.ValidCode;
			return response;
		}

		if (ext.Configuration.ResponseConfiguration.ShowResponse)
			await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(ext.Configuration.ResponseConfiguration.AuthenticationFailureMessage));

		response.Result = TwoFactorResult.InvalidCode;
		return response;
	}

	/// <summary>
	/// <para>Asks the user via modal input for the two factor code.</para>
	/// <para>This uses DisCatSharp.Interactivity.</para>
	/// <para>To be used as first action for button.</para>
	/// </summary>
	/// <param name="ctx">The interaction context.</param>
	/// <param name="client">The discord client.</param>
	/// <returns>A <see cref="TwoFactorResult"/>.</returns>
	public static async Task<TwoFactorResponse> RequestTwoFactorAsync(this ComponentInteractionCreateEventArgs ctx, DiscordClient client)
	{
		var ext = client.GetTwoFactor();

		DiscordInteractionModalBuilder builder = new(ext.Configuration.ResponseConfiguration.AuthenticationModalRequestTitle);
		builder.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "code", "Code", "123456", ext.Configuration.Digits, ext.Configuration.Digits));
		await ctx.Interaction.CreateInteractionModalResponseAsync(builder);

		var response = new TwoFactorResponse();

		var inter = await client.GetInteractivity().WaitForModalAsync(builder.CustomId, TimeSpan.FromSeconds(ext.Configuration.TwoFactorTimeout));
		if (inter.TimedOut)
		{
			response.Result = TwoFactorResult.TimedOut;
			return response;
		}

		response.Interaction = inter.Result.Interaction;

		if (ext.Configuration.ResponseConfiguration.ShowResponse)
			await inter.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Checking.."));
		var res = ext.IsValidCode(ctx.User.Id, inter.Result.Interaction.Data.Components.First().Value);
		if (res)
		{
			if (ext.Configuration.ResponseConfiguration.ShowResponse)
				await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(ext.Configuration.ResponseConfiguration.AuthenticationSuccessMessage));

			response.Result = TwoFactorResult.ValidCode;
			return response;
		}

		if (ext.Configuration.ResponseConfiguration.ShowResponse)
			await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(ext.Configuration.ResponseConfiguration.AuthenticationFailureMessage));

		response.Result = TwoFactorResult.InvalidCode;
		return response;
	}
}
