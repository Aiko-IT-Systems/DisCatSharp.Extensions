// This file is part of the DisCatSharp project, based off DSharpPlus.
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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using ConcurrentCollections;

using DisCatSharp.Extensions.OAuth2Web.EventArgs;
using DisCatSharp.Extensions.OAuth2Web.EventHandling.Requests;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Extensions.OAuth2Web.EventHandling;

/// <summary>
/// A <see cref="AuthorizationCodeEventWaiter"/>.
/// </summary>
internal class AuthorizationCodeEventWaiter : IDisposable
{
	private readonly DiscordOAuth2Client _client;
	private readonly OAuth2WebExtension _extension;
	private readonly ConcurrentHashSet<AuthorizationCodeMatchRequest> _matchRequests = new();
	private readonly ConcurrentHashSet<AuthorizationCodeCollectRequest> _collectRequests = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="AuthorizationCodeEventWaiter"/> class.
	/// </summary>
	/// <param name="extension">The OAuth2 web extension.</param>
	/// <param name="client">The client.</param>
	public AuthorizationCodeEventWaiter(OAuth2WebExtension extension, DiscordOAuth2Client client)
	{
		this._extension = extension;
		this._client = client;
		this._extension.AuthorizationCodeExchanged += this.HandleAsync;
	}

	/// <summary>
	/// Waits for a specified <see cref="AuthorizationCodeMatchRequest"/>'s predicate to be fulfilled.
	/// </summary>
	/// <param name="request">The request to wait for.</param>
	/// <returns>The returned args, or null if it timed out.</returns>
	public async Task<AuthorizationCodeExchangeEventArgs?> WaitForMatchAsync(AuthorizationCodeMatchRequest request)
	{
		this._matchRequests.Add(request);

		try
		{
			return await request.Tcs.Task.ConfigureAwait(false);
		}
		catch (Exception e)
		{
			this._client.Logger.LogError(e, "An exception was thrown while waiting for authorization code exchange");
			return null;
		}
		finally
		{
			this._matchRequests.TryRemove(request);
		}
	}

	/// <summary>
	/// Collects requests and returns the result when the <see cref="AuthorizationCodeMatchRequest"/>'s cancellation token is canceled.
	/// </summary>
	/// <param name="request">The request to wait on.</param>
	/// <returns>The result from request's predicate over the period of time leading up to the token's cancellation.</returns>
	public async Task<IReadOnlyList<AuthorizationCodeExchangeEventArgs>> CollectMatchesAsync(AuthorizationCodeCollectRequest request)
	{
		this._collectRequests.Add(request);
		try
		{
			await request.Tcs.Task.ConfigureAwait(false);
		}
		catch (Exception e)
		{
			this._client.Logger.LogError(e, "There was an error while collecting authorization code exchange event args");
		}
		finally
		{
			this._collectRequests.TryRemove(request);
		}

		return request.Collected.ToArray();
	}

	/// <summary>
	/// Handles the waiter.
	/// </summary>
	/// <param name="_">The client.</param>
	/// <param name="args">The args.</param>
	private Task HandleAsync(DiscordOAuth2Client _, AuthorizationCodeExchangeEventArgs args)
	{
		foreach (var mreq in this._matchRequests.Where(mreq => HttpUtility.UrlDecode(mreq.State) == args.ReceivedState && mreq.IsMatch(args)))
			mreq.Tcs.TrySetResult(args);

		foreach (var creq in this._collectRequests.Where(creq => HttpUtility.UrlDecode(creq.State) == args.ReceivedState && creq.IsMatch(args)))
			creq.Collected.Add(args);

		return Task.CompletedTask;
	}

	/// <summary>
	/// Disposes the waiter.
	/// </summary>
	public void Dispose()
	{
		this._matchRequests.Clear();
		this._collectRequests.Clear();
		this._extension.AuthorizationCodeExchanged -= this.HandleAsync;
	}
}
