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

using DatabaseWrapper.Core;
using DatabaseWrapper.Sqlite;

using ExpressionTree;

using TwoFactorAuthNet;

namespace DisCatSharp.Extensions.TwoFactorCommands;

public sealed class TwoFactorExtension : BaseExtension
{
	/// <summary>
	/// Gets the two factor configuration.
	/// </summary>
	internal TwoFactorConfiguration Configuration { get; private set; }

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
		=> this.Configuration.ServiceProvider;

	/// <summary>
	/// Initializes a new instance of the <see cref="TwoFactorExtension"/> class.
	/// </summary>
	/// <param name="configuration">The config.</param>
	internal TwoFactorExtension(TwoFactorConfiguration configuration = null)
	{
		configuration ??= new TwoFactorConfiguration();
		this.Configuration = configuration;
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

		this.DatabaseClient = new(this.Configuration.DatabasePath);
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

	private void RemoveSecret(ulong user)
		=> this.DatabaseClient.Delete(this._tableName, new Expr(this._userField, OperatorEnum.Equals, user.ToString()));

	internal bool IsValidCode(ulong user, string code)
		=> this.HasData(user) && this.TwoFactorClient.VerifyCode(this.GetSecretFor(user), code);

	internal bool IsEnrolled(ulong user)
		=> this.HasData(user);

	internal void EnrollUser(ulong user, string secret)
		=> this.AddSecretFor(user, secret);

	internal void UnenrollUser(ulong user)
		=> this.RemoveSecret(user);
}
