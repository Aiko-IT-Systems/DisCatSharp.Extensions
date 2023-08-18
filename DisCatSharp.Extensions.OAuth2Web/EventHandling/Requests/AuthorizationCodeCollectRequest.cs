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
using System.Collections.Concurrent;
using System.Threading;

using DisCatSharp.Extensions.OAuth2Web.EventArgs;

namespace DisCatSharp.Extensions.OAuth2Web.EventHandling.Requests;

/// <summary>
/// Represents a authorization code exchange event that is being waited for.
/// </summary>
internal sealed class AuthorizationCodeCollectRequest : AuthorizationCodeMatchRequest
{
	/// <summary>
	/// Gets the collected.
	/// </summary>
	public ConcurrentBag<AuthorizationCodeExchangeEventArgs> Collected { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="AuthorizationCodeCollectRequest"/> class.
	/// </summary>
	/// <param name="state">The state to wait for.</param>
	/// <param name="predicate">The predicate.</param>
	/// <param name="cancellation">The cancellation token.</param>
	public AuthorizationCodeCollectRequest(string state, Func<AuthorizationCodeExchangeEventArgs, bool> predicate, CancellationToken cancellation)
		: base(state, predicate, cancellation)
	{ }
}
