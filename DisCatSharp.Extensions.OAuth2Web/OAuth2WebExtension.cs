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
using System.Collections.Concurrent;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.Entities.OAuth2;
using DisCatSharp.Extensions.OAuth2Web.EventArgs;
using DisCatSharp.Extensions.OAuth2Web.EventHandling;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.Extensions.OAuth2Web;

/// <summary>
/// Represents a <see cref="OAuth2WebExtension"/>.
/// </summary>
public sealed class OAuth2WebExtension : BaseExtension
{
	/// <summary>
	/// Gets the OAuth2 Web configuration.
	/// </summary>
	internal OAuth2WebConfiguration Configuration { get; }

	/// <summary>
	/// Gets the oauth2 client.
	/// </summary>
	public DiscordOAuth2Client OAuth2Client { get; private set; }

	/// <summary>
	/// Gets the web application.
	/// </summary>
	private WebApplication WEB_APP { get; }

	/// <summary>
	/// Gets the service provider this OAuth2 Web module was configured with.
	/// </summary>
	public IServiceProvider ServiceProvider
		=> this.Configuration.ServiceProvider;

	/// <summary>
	/// Gets the authorization code event waiter.
	/// </summary>
	private readonly AuthorizationCodeEventWaiter _authorizationCodeWaiter;

	/// <summary>
	/// Triggered when an authorizaton code was received.
	/// </summary>
	public event AsyncEventHandler<DiscordOAuth2Client, AuthorizationCodeReceiveEventArgs> AuthorizationCodeReceived
	{
		add => this._authorizationCodeReceived.Register(value);
		remove => this._authorizationCodeReceived.Unregister(value);
	}

	/// <summary>
	/// Fired when an authorizaton code was received.
	/// </summary>
	private readonly AsyncEvent<DiscordOAuth2Client, AuthorizationCodeReceiveEventArgs> _authorizationCodeReceived;

	/// <summary>
	/// Triggered when an authorizaton code was exchanged.
	/// </summary>
	public event AsyncEventHandler<DiscordOAuth2Client, AuthorizationCodeExchangeEventArgs> AuthorizationCodeExchanged
	{
		add => this._authorizationCodeExchanged.Register(value);
		remove => this._authorizationCodeExchanged.Unregister(value);
	}

	/// <summary>
	/// Fired when an authorizaton code was exchanged.
	/// </summary>
	private readonly AsyncEvent<DiscordOAuth2Client, AuthorizationCodeExchangeEventArgs> _authorizationCodeExchanged;

	/// <summary>
	/// Triggered when an access token was refreshed.
	/// </summary>
	public event AsyncEventHandler<DiscordOAuth2Client, AccessTokenRefreshEventArgs> AccessTokenRefreshed
	{
		add => this._accessTokenRefreshed.Register(value);
		remove => this._accessTokenRefreshed.Unregister(value);
	}

	/// <summary>
	/// Fired when an access token was refreshed.
	/// </summary>
	private readonly AsyncEvent<DiscordOAuth2Client, AccessTokenRefreshEventArgs> _accessTokenRefreshed;

	/// <summary>
	/// Triggered when an access token was revoked.
	/// </summary>
	public event AsyncEventHandler<DiscordOAuth2Client, AccessTokenRevokeEventArgs> AccessTokenRevoked
	{
		add => this._accessTokenRevoked.Register(value);
		remove => this._accessTokenRevoked.Unregister(value);
	}

	/// <summary>
	/// Fired when an access token was revoked.
	/// </summary>
	private readonly AsyncEvent<DiscordOAuth2Client, AccessTokenRevokeEventArgs> _accessTokenRevoked;

	/// <summary>
	/// All pending OAuth2 request urls.
	/// </summary>
	internal ConcurrentBag<string> OAuth2RequestUrls { get; } = new();

	/// <summary>
	/// Gets all access tokens mapped to user id.
	/// </summary>
	internal ConcurrentDictionary<ulong, DiscordAccessToken> UserIdAccessTokenMapper { get; } = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="OAuth2WebExtension"/> class.
	/// </summary>
	/// <param name="configuration">The config.</param>
	internal OAuth2WebExtension(OAuth2WebConfiguration configuration)
	{
		this.Configuration = configuration;

		this.OAuth2Client = new(this.Configuration.ClientId, this.Configuration.ClientSecret, this.Configuration.RedirectUri);

		this._authorizationCodeReceived = new("OAUTH2_AUTH_CODE_RECEIVED", TimeSpan.Zero, this.OAuth2Client.EventErrorHandler);
		this._authorizationCodeExchanged = new("OAUTH2_AUTH_CODE_EXCHANGED", TimeSpan.Zero, this.OAuth2Client.EventErrorHandler);
		this._accessTokenRefreshed = new("OAUTH2_ACCESS_TOKEN_REFRESHED", TimeSpan.Zero, this.OAuth2Client.EventErrorHandler);
		this._accessTokenRevoked = new("OAUTH2_ACCESS_TOKEN_REVOKED", TimeSpan.Zero, this.OAuth2Client.EventErrorHandler);
		this._authorizationCodeWaiter = new(this, this.OAuth2Client);

		var builder = WebApplication.CreateBuilder();
		;
		builder.Services.AddRouting();
		builder.Services.AddAuthorization();

		builder.WebHost.UseUrls(this.Configuration.ListenAll
			? $"http://*:{this.Configuration.Port}"
			: $"http://127.0.0.1:{this.Configuration.Port}");

		this.WEB_APP = builder.Build();

		this.WEB_APP.UseRouting();

		this.WEB_APP.UseAuthorization();

		this.WEB_APP.MapGet("/oauth", this.HandleOAuth2Async);

		this.AuthorizationCodeExchanged += this.OnAuthorizationCodeExchangedAsync;
		this.AccessTokenRefreshed += this.OnAccessTokenRefreshedAsync;
		this.AccessTokenRevoked += this.OnAccessTokenRevokedAsync;
	}

	/// <summary>
	/// Gets an access token for <paramref name="user"/>.
	/// </summary>
	/// <param name="user">The user to get the token from.</param>
	/// <param name="token">The found access token.</param>
	/// <returns>Whether an access token was found.</returns>
	public bool TryGetAccessToken(DiscordUser user, out DiscordAccessToken? token)
		=> this.TryGetAccessToken(user.Id, out token);

	/// <summary>
	/// Gets an access token for <paramref name="userId"/>.
	/// </summary>
	/// <param name="userId">The user id to get the token from.</param>
	/// <param name="token">The found access token.</param>
	/// <returns>Whether an access token was found.</returns>
	public bool TryGetAccessToken(ulong userId, out DiscordAccessToken? token)
		=> this.UserIdAccessTokenMapper.TryGetValue(userId, out token);

	/// <summary>
	/// Refreshes an access token for <paramref name="user"/>.
	/// <para>Fires an <see cref="AccessTokenRefreshed"/> event.</para>
	/// </summary>
	/// <param name="user">The user to revoke their token from.</param>
	public Task<bool> RefreshAccessTokenAsync(DiscordUser user)
		=> this.RefreshAccessTokenAsync(user.Id);

	/// <summary>
	/// Revokes an access token for <paramref name="user"/>.
	/// <para>Fires an <see cref="AccessTokenRevoked"/> event.</para>
	/// </summary>
	/// <param name="user">The user to revoke their token from.</param>
	public Task<bool> RevokeAccessTokenAsync(DiscordUser user)
		=> this.RevokeAccessTokenAsync(user.Id);

	/// <summary>
	/// Refreshes an access token for <paramref name="userId"/>.
	/// <para>Fires an <see cref="AccessTokenRefreshed"/> event.</para>
	/// </summary>
	/// <param name="userId">The user id to revoke their token from.</param>
	public async Task<bool> RefreshAccessTokenAsync(ulong userId)
	{
		if (!this.UserIdAccessTokenMapper.TryGetValue(userId, out var token))
			return false;

		await this.RefreshAccessTokenAsync(token, userId);
		return true;
	}

	/// <summary>
	/// Revokes an access token for <paramref name="userId"/>.
	/// <para>Fires an <see cref="AccessTokenRevoked"/> event.</para>
	/// </summary>
	/// <param name="userId">The user id to revoke their token from.</param>
	public async Task<bool> RevokeAccessTokenAsync(ulong userId)
	{
		if (!this.UserIdAccessTokenMapper.TryGetValue(userId, out var token))
			return false;

		await this.RevokeAccessTokenAsync(token, userId);
		return true;
	}

	/// <summary>
	/// Refreshes all access tokens.
	/// <para>Fires an <see cref="AccessTokenRefreshed"/> event for every refreshed token.</para>
	/// </summary>
	public async Task RefreshAllAccessTokensAsync()
	{
		foreach (var mappedToken in this.UserIdAccessTokenMapper)
			await this.RefreshAccessTokenAsync(mappedToken.Value, mappedToken.Key);
	}

	/// <summary>
	/// Revokes all access tokens.
	/// <para>Fires an <see cref="AccessTokenRevoked"/> event for every refreshed token.</para>
	/// </summary>
	public async Task RevokeAllAccessTokensAsync()
	{
		foreach (var mappedToken in this.UserIdAccessTokenMapper)
			await this.RevokeAccessTokenAsync(mappedToken.Value, mappedToken.Key);
	}

	/// <summary>
	/// Handles access token revokes.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event args.</param>
	private Task OnAccessTokenRevokedAsync(DiscordOAuth2Client sender, AccessTokenRevokeEventArgs e)
	{
		this.UserIdAccessTokenMapper.TryRemove(e.UserId, out _);
		e.Handled = false;
		return Task.CompletedTask;
	}

	/// <summary>
	/// Handles access token refreshes.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event args.</param>
	private Task OnAccessTokenRefreshedAsync(DiscordOAuth2Client sender, AccessTokenRefreshEventArgs e)
	{
		_ = this.UserIdAccessTokenMapper.AddOrUpdate(e.UserId, e.RefreshedDiscordAccessToken, (id, old) =>
		{
			old.AccessToken = e.RefreshedDiscordAccessToken.AccessToken;
			old.ExpiresIn = e.RefreshedDiscordAccessToken.ExpiresIn;
			old.RefreshToken = e.RefreshedDiscordAccessToken.RefreshToken;
			old.Scope = e.RefreshedDiscordAccessToken.Scope;
			old.TokenType = e.RefreshedDiscordAccessToken.TokenType;
			return old;
		});
		e.Handled = false;
		return Task.CompletedTask;
	}

	/// <summary>
	/// Handles authorization code exchanges.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event args.</param>
	private Task OnAuthorizationCodeExchangedAsync(DiscordOAuth2Client sender, AuthorizationCodeExchangeEventArgs e)
	{
		_ = this.UserIdAccessTokenMapper.AddOrUpdate(e.UserId, e.DiscordAccessToken, (id, old) =>
		{
			old.AccessToken = e.DiscordAccessToken.AccessToken;
			old.ExpiresIn = e.DiscordAccessToken.ExpiresIn;
			old.RefreshToken = e.DiscordAccessToken.RefreshToken;
			old.Scope = e.DiscordAccessToken.Scope;
			old.TokenType = e.DiscordAccessToken.TokenType;
			return old;
		});
		e.Handled = false;
		return Task.CompletedTask;
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
	}

	/// <summary>
	/// Starts the web server.
	/// </summary>
	public void StartAsync()
		=> Task.Run(() => this.WEB_APP.RunAsync());

	/// <summary>
	/// Adds a url to pending urls.
	/// </summary>
	/// <param name="uri">The url to add.</param>
	public void SubmitPendingOAuth2Url(Uri uri)
		=> this.OAuth2RequestUrls.Add(uri.AbsoluteUri);

	/// <summary>
	/// Waits for an access token.
	/// </summary>
	/// <param name="user">The user to wait for.</param>
	/// <param name="uri">The oauth url generated from <see cref="DiscordOAuth2Client.GenerateOAuth2Url"/> to wait for.</param>
	/// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
	public async Task<OAuth2Result<AuthorizationCodeExchangeEventArgs>> WaitForAccessTokenAsync(DiscordUser user, Uri uri, CancellationToken token)
	{
		var state = this.OAuth2Client.GetStateFromUri(uri);

		var res = await this._authorizationCodeWaiter
			.WaitForMatchAsync(new(state,
				x => x.UserId == user.Id, token)).ConfigureAwait(false);

		return new(res is null, res!);
	}

	/// <summary>
	/// Waits for an access token.
	/// <para>Make sure to submit <paramref name="uri"/> to <see cref="SubmitPendingOAuth2Url"/> before calling.</para>
	/// </summary>
	/// <param name="user">The user to wait for.</param>
	/// <param name="uri">The oauth url generated from <see cref="DiscordOAuth2Client.GenerateOAuth2Url"/> to wait for.</param>
	/// <param name="timeoutOverride">Override the timeout period of one minute.</param>
	public Task<OAuth2Result<AuthorizationCodeExchangeEventArgs>> WaitForAccessTokenAsync(DiscordUser user, Uri uri, TimeSpan? timeoutOverride = null)
		=> this.WaitForAccessTokenAsync(user, uri, GetCancellationToken(timeoutOverride));

	/// <summary>
	/// Refreshes an access token.
	/// </summary>
	/// <param name="accessToken">The access token to refresh.</param>
	/// <param name="userId">The user id the access token belongs to.</param>
	internal async Task RefreshAccessTokenAsync(DiscordAccessToken accessToken, ulong userId)
	{
		var freshToken = await this.OAuth2Client.RefreshAccessTokenAsync(accessToken);
		_ = Task.Run(() => this._accessTokenRefreshed.InvokeAsync(this.OAuth2Client, new(this.ServiceProvider)
		{
			RefreshedDiscordAccessToken = freshToken, UserId = userId
		}));
	}

	/// <summary>
	/// Revokes an access token.
	/// </summary>
	/// <param name="accessToken">The access token to revoke.</param>
	/// <param name="userId">The user id the access token belongs to.</param>
	internal async Task RevokeAccessTokenAsync(DiscordAccessToken accessToken, ulong userId)
	{
		await this.OAuth2Client.RevokeByAccessTokenAsync(accessToken);
		await this.OAuth2Client.RevokeByRefreshTokenAsync(accessToken);
		_ = Task.Run(() => this._accessTokenRevoked.InvokeAsync(this.OAuth2Client, new(this.ServiceProvider)
		{
			RevokedDiscordAccessToken = accessToken, UserId = userId
		}));
	}

	/// <summary>
	/// Handles the OAuth2 authorization code exchange.
	/// </summary>
	/// <param name="context">The http context.</param>
	private async Task HandleOAuth2Async(HttpContext context)
	{
		try
		{
			Uri requestUrl = new(context.Request.GetDisplayUrl());

			if (!this.OAuth2RequestUrls.Any(u =>
				    this.OAuth2Client.ValidateState(new(u), requestUrl, this.Configuration.SecureStates)))
			{
				context.Response.StatusCode = 500;
				context.Response.ContentType = "application/json";
				await context.Response.WriteAsync(
					"{ \"handled\": false, \"error\": true, \"message\": \"Invalid state\" }");
				return;
			}

			var code = this.OAuth2Client.GetCodeFromUri(requestUrl);
			var state = this.OAuth2Client.GetStateFromUri(requestUrl);

			await this._authorizationCodeReceived.InvokeAsync(this.OAuth2Client,
				new(this.ServiceProvider) { ReceivedCode = code, ReceivedState = state });

			var accessToken = await this.OAuth2Client.ExchangeAccessTokenAsync(code);
			var info = await this.OAuth2Client.GetCurrentAuthorizationInformationAsync(accessToken);
			if (this.Configuration.SecureStates)
			{
				var stateUserId = ulong.Parse(this.OAuth2Client.ReadSecureState(state).Split("::")[1]);
				if (stateUserId != info.User?.Id)
					throw new SecurityException("State mismatch");
			}

			_ = Task.Run(() => this._authorizationCodeExchanged.InvokeAsync(this.OAuth2Client,
				new(this.ServiceProvider)
				{
					ExchangedCode = code,
					ReceivedState = state,
					DiscordAccessToken = accessToken,
					UserId = info.User!.Id
				}));

			context.Response.StatusCode = 200;
			context.Response.ContentType = "application/json";
			await context.Response.WriteAsync("{ \"handled\": true, \"error\": false }");
		}
		catch (SecurityException ex)
		{
			_ = Task.Run(() => this.OAuth2Client._oauth2ClientErrored.InvokeAsync(this.OAuth2Client,
				new(this.ServiceProvider) { EventName = "HandleOAuth2Async", Exception = ex }));
			context.Response.StatusCode = 401;
			context.Response.ContentType = "application/json";
			await context.Response.WriteAsync("{ \"handled\": false, \"error\": true, \"message\": \"Security Exception\" }");
		}
		catch (Exception ex)
		{
			_ = Task.Run(() => this.OAuth2Client._oauth2ClientErrored.InvokeAsync(this.OAuth2Client,
				new(this.ServiceProvider) { EventName = "HandleOAuth2Async", Exception = ex }));
			context.Response.StatusCode = 500;
			context.Response.ContentType = "application/json";
			await context.Response.WriteAsync("{ \"handled\": false, \"error\": true, \"message\": \"Something went wrong\" }");
		}
	}

	/// <summary>
	/// Gets the cancellation token.
	/// </summary>
	/// <param name="timeout">The timeout.</param>
	private static CancellationToken GetCancellationToken(TimeSpan? timeout = null)
		=> new CancellationTokenSource(timeout ?? DiscordOAuth2Client.EventExecutionLimit).Token;

	/// <summary>
	/// Stops the web server.
	/// </summary>
	public Task StopAsync()
		=> this.WEB_APP.StopAsync();
}
