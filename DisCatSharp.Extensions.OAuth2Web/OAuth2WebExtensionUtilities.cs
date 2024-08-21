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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Extensions.OAuth2Web;

/// <summary>
///     Provides extension methods for OAuth2Web.
/// </summary>
public static class OAuth2WebExtensionUtilities
{
	/// <summary>
	///     Generates the proxy configuration for apache2 hosts for use with a <see cref="DiscordShardedClient" />.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="cancellationToken">The optional cancellation token.</param>
	/// <returns>A file will be created in the executing directory with the name <c>dcs_oauth2web_sharded_proxy.conf</c>.</returns>
	public static async Task GenerateApache2ProxyFileAsync(this DiscordShardedClient client, CancellationToken? cancellationToken = null)
	{
		var extensions = await client.GetOAuth2WebAsync();
		ArgumentNullException.ThrowIfNull(extensions);

		var configurations = extensions.Values.Select(x => x.Configuration).ToList();
		List<string> proxyStrings = ["\tProxyRequests Off", "\tProxyPreserveHost On"];

		foreach (var configuration in configurations)
		{
			Uri redirectUri = new(configuration.RedirectUri);
			var targetHost = configuration.ListenAll ? configuration.ProxyTargetIpOrHost : "127.0.0.1";
			var path = redirectUri.AbsolutePath;

			proxyStrings.Add($"\tProxyPass {path} http://{targetHost}:{configuration.StartPort}{path}");
			proxyStrings.Add($"\tProxyPassReverse {path} http://{targetHost}:{configuration.StartPort}{path}");
		}

		await File.WriteAllLinesAsync("dcs_oauth2web_sharded_proxy.conf", proxyStrings, Encoding.UTF8, cancellationToken ?? CancellationToken.None);
	}

	/// <summary>
	///     Generates the proxy configuration for apache2 hosts for use with a single <see cref="DiscordClient" />.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="cancellationToken">The optional cancellation token.</param>
	/// <returns>A file will be created in the executing directory with the name <c>dcs_oauth2web_proxy.conf</c>.</returns>
	public static async Task GenerateApache2ProxyFileAsync(this DiscordClient client, CancellationToken? cancellationToken = null)
	{
		var extensions = client.GetOAuth2Web();
		ArgumentNullException.ThrowIfNull(extensions);

		var configuration = extensions.Configuration;
		List<string> proxyStrings = [];

		Uri redirectUri = new(configuration.RedirectUri);
		var targetHost = configuration.ListenAll ? configuration.ProxyTargetIpOrHost : "127.0.0.1";
		var path = redirectUri.AbsolutePath;

		proxyStrings.Add("\tProxyRequests Off");
		proxyStrings.Add("\tProxyPreserveHost On");
		proxyStrings.Add($"\tProxyPass {path} http://{targetHost}:{configuration.StartPort}{path}");
		proxyStrings.Add($"\tProxyPassReverse {path} http://{targetHost}:{configuration.StartPort}{path}");

		await File.WriteAllLinesAsync("dcs_oauth2web_proxy.conf", proxyStrings, Encoding.UTF8, cancellationToken ?? CancellationToken.None);
	}
}
