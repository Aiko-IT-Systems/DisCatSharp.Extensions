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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using DatabaseWrapper.Core;
using DatabaseWrapper.Sqlite;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity.Extensions;

using ExpressionTree;

using TwoFactorAuthNet;

namespace DisCatSharp.Extensions.TwoFactorCommands;

public sealed class TwoFactorExtension : BaseExtension
{
	/// <summary>
	/// Gets the two factor configuration.
	/// </summary>
	private readonly TwoFactorConfiguration _config;

	/// <summary>
	/// Gets the database client.
	/// </summary>
	internal DatabaseClient DatabaseClient { get; private set; }

	/// <summary>
	/// Gets the tfa client.
	/// </summary>
	internal TwoFactorAuth TwoFactorClient { get; private set; }

	/// <summary>
	/// Gets the table name.
	/// </summary>
	private readonly string _tableName = "twofactor_mapper";

	/// <summary>
	/// Gets the user field name.
	/// </summary>
	private readonly string _userField = "user";

	/// <summary>
	/// Gets the secret field name.
	/// </summary>
	private readonly string _secretField = "secret";

	/// <summary>
	/// Gets the service provider this TwoFactor module was configured with.
	/// </summary>
	public IServiceProvider Services
		=> this._config.ServiceProvider;

	/// <summary>
	/// Initializes a new instance of the <see cref="TwoFactorExtension"/> class.
	/// </summary>
	/// <param name="configuration">The config.</param>
	internal TwoFactorExtension(TwoFactorConfiguration configuration = null)
	{
		configuration ??= new TwoFactorConfiguration();
		this._config = configuration;
		this.TwoFactorClient = new(configuration.Issuer, configuration.Digits, configuration.Period, configuration.Algorithm);
	}

	/// <summary>
	/// DO NOT USE THIS MANUALLY.
	/// </summary>
	/// <param name="client">DO NOT USE THIS MANUALLY.</param>
	/// <exception cref="InvalidOperationException"/>
	protected internal override void Setup(DiscordClient client)
	{
		if (this.Client != null)
			throw new InvalidOperationException("What did I tell you?");

		this.Client = client;

		this.DatabaseClient = new(this._config.DatabasePath);
		if (!this.DatabaseClient.TableExists(this._tableName))
		{
			var columns = new List<Column>
			{
				new Column(this._userField, true, DataTypeEnum.Varchar, 128, null, false),
				new Column(this._secretField, false, DataTypeEnum.Varchar, 512, null, false),
			};
			this.DatabaseClient.CreateTable(this._tableName, columns);
		}
	}

	private bool HasData(ulong user)
		=> this.DatabaseClient.Exists(this._tableName, new Expr(this._userField, OperatorEnum.Equals, user.ToString()));

	private string GetSecretFor(ulong user)
		=> this.DatabaseClient.Select(this._tableName, null, 1, null, new Expr(this._userField, OperatorEnum.Equals, user.ToString())).Rows[0].ItemArray[1].ToString();

	private void AddSecretFor(ulong user, string secret)
	{
		Dictionary<string, object> data = new()
		{
			{ this._userField, user.ToString() },
			{ this._secretField, secret }
		};
		this.DatabaseClient.Insert(this._tableName, data);
	}

	internal bool IsValidCode(ulong user, string code)
		=> this.HasData(user) && this.TwoFactorClient.VerifyCode(this.GetSecretFor(user), code);

	public (string, Stream) RegisterTwoFactor(DiscordUser user)
	{
		var secret = this.TwoFactorClient.CreateSecret(160, CryptoSecureRequirement.RequireSecure);
		var label = $"DisCatSharp Auth: {HttpUtility.UrlEncode(user.UsernameWithDiscriminator)}";
		var text = $"otpauth://totp/{label}?secret={secret}&issuer={this.TwoFactorClient.Issuer}";
		var image = this.TwoFactorClient.QrCodeProvider.GetQrCodeImage(text, 512);
		MemoryStream ms = new(image)
		{
			Position = 0
		};
		return (secret, ms);
	}

	public async Task<bool> AskForTwoFactorAsync(BaseContext ctx)
	{
		DiscordInteractionModalBuilder builder = new("Enter 2FA Code");
		builder.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "code", "Code", "123456", this._config.Digits, this._config.Digits));
		await ctx.CreateModalResponseAsync(builder);

		var inter = await ctx.Client.GetInteractivity().WaitForModalAsync(builder.CustomId, TimeSpan.FromSeconds(this._config.TwoFactorTimeout));
		if (inter.TimedOut)
			return false;

		await inter.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Checking.."));
		var res = this.IsValidCode(ctx.User.Id, inter.Result.Interaction.Data.Components.First().Value);
		if (res)
		{
			await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Code valid!"));
			return true;
		}

		await inter.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Code invalid.."));
		return false;
	}

	// TODO: Implement
	internal static async Task<bool> AskForTwoFactorAsync(CommandContext ctx)
		=> await Task.FromResult(false);
}
