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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

namespace DisCatSharp.Extensions.OAuth2Web;

/// <summary>
///     Defines various extensions specific to OAuth2Web.
/// </summary>
public static class ExtensionMethods
{
	/// <summary>
	///     Enables OAuth2Web module on this <see cref="DiscordClient" />.
	/// </summary>
	/// <param name="client">Client to enable OAuth2Web for.</param>
	/// <param name="config">OAuth2Web configuration to use.</param>
	/// <returns>Created <see cref="OAuth2WebExtension" />.</returns>
	public static OAuth2WebExtension UseOAuth2Web(this DiscordClient client, OAuth2WebConfiguration config)
	{
		if (client.GetExtension<OAuth2WebExtension>() != null)
			throw new InvalidOperationException("OAuth2Web is already enabled for that client.");

		config.ServiceProvider ??= client.ServiceProvider ?? new ServiceCollection().BuildServiceProvider(true);

		var oa2W = new OAuth2WebExtension(config); // , client);
		client.AddExtension(oa2W);
		return oa2W;
	}

	/// <summary>
	///     Enables OAuth2Web module on all shards in this <see cref="DiscordShardedClient" />.
	/// </summary>
	/// <param name="client">Client to enable OAuth2Web for.</param>
	/// <param name="config">OAuth2Web configuration to use.</param>
	/// <returns>A dictionary of created <see cref="OAuth2WebExtension" />, indexed by shard id.</returns>
	public static async Task<IReadOnlyDictionary<int, OAuth2WebExtension>> UseOAuth2WebAsync(this DiscordShardedClient client, OAuth2WebConfiguration config)
	{
		var modules = new Dictionary<int, OAuth2WebExtension>();
		await client.InitializeShardsAsync().ConfigureAwait(false);

		var currentPort = config.StartPort;
		var baseRedirectUri = config.RedirectUri;
		foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
		{
			var oa2W = shard.GetExtension<OAuth2WebExtension>();
			oa2W ??= shard.UseOAuth2Web(new(config)
			{
				StartPort = currentPort,
				RedirectUri = $"{baseRedirectUri}s{shard.ShardId}/"
			});
			currentPort++;

			modules[shard.ShardId] = oa2W;
		}

		return new ReadOnlyDictionary<int, OAuth2WebExtension>(modules);
	}

	/// <summary>
	///     Gets the active OAuth2Web module for this client.
	/// </summary>
	/// <param name="client">Client to get OAuth2Web module from.</param>
	/// <returns>The module, or null if not activated.</returns>
	public static OAuth2WebExtension? GetOAuth2Web(this DiscordClient client)
		=> client.GetExtension<OAuth2WebExtension>();

	/// <summary>
	///     Gets the active OAuth2Web modules for all shards in this client.
	/// </summary>
	/// <param name="client">Client to get OAuth2Web instances from.</param>
	/// <returns>A dictionary of the modules, indexed by shard id.</returns>
	public static async Task<IReadOnlyDictionary<int, OAuth2WebExtension?>> GetOAuth2WebAsync(this DiscordShardedClient client)
	{
		await client.InitializeShardsAsync().ConfigureAwait(false);
		var extensions = client.ShardClients.Select(xkvp => xkvp.Value).ToDictionary(shard => shard.ShardId, shard => shard.GetExtension<OAuth2WebExtension>());

		return new ReadOnlyDictionary<int, OAuth2WebExtension?>(extensions);
	}

	/// <summary>
	///     Starts the oauth2 web server for all shards.
	/// </summary>
	/// <param name="extensions">The extension dictionary.</param>
	public static void Start(this IReadOnlyDictionary<int, OAuth2WebExtension> extensions)
	{
		foreach (var extension in extensions.Values)
			extension.Start();
	}

	/// <summary>
	///     Stops the oauth2 web server for all shards.
	/// </summary>
	/// <param name="extensions">The extension dictionary.</param>
	public static async Task StopAsync(this IReadOnlyDictionary<int, OAuth2WebExtension> extensions)
	{
		foreach (var extension in extensions.Values)
			await extension.StopAsync();
	}

	/// <summary>
	///     <para>Checks if all redirect uris are set for the application in the developer portal.</para>
	///     <para>Use this function after you've executed <see cref="DiscordShardedClient.StartAsync" />.</para>
	/// </summary>
	/// <param name="extensions">The extensions.</param>
	/// <param name="client">The <see cref="DiscordShardedClient" />.</param>
	/// <returns>Whether all required redirect uris are set.</returns>
	public static bool HasAllRequiredRedirectUrisSet(this IReadOnlyDictionary<int, OAuth2WebExtension> extensions, DiscordShardedClient client)
		=> extensions.Values.Select(extension => extension.Configuration.RedirectUri).All(client.CurrentApplication.RedirectUris.Contains);

	/// <summary>
	///     <para>Checks if the redirect uri is set for the application in the developer portal.</para>
	///     <para>Use this function after you've executed <see cref="DiscordClient.ConnectAsync" />.</para>
	/// </summary>
	/// <param name="extensions">The extensions.</param>
	/// <param name="client">The <see cref="DiscordClient" />.</param>
	/// <returns>Whether the required redirect uris is set.</returns>
	public static bool HasRequiredRedirectUriSet(this OAuth2WebExtension extensions, DiscordClient client)
		=> client.CurrentApplication.RedirectUris.Contains(extensions.Configuration.RedirectUri);

	/// <summary>
	///     Gets the required redirect uris for the developer portal.
	/// </summary>
	/// <param name="extensions">The extensions.</param>
	/// <returns>The required redirect uris.</returns>
	public static IReadOnlyList<string> GetRequiredRedirectUris(this IReadOnlyDictionary<int, OAuth2WebExtension> extensions)
		=> extensions.Values.Select(extension => extension.Configuration.RedirectUri).ToList().AsReadOnly();

	/// <summary>
	///     <para>Checks if all redirect uris are set for the application in the developer portal.</para>
	///     <para>Use this function after you've executed <see cref="DiscordClient.ConnectAsync" />.</para>
	/// </summary>
	/// <param name="extension">The extension.</param>
	/// <returns>Whether the required redirect uri is set.</returns>
	public static bool HasRequiredRedirectUriSet(this OAuth2WebExtension extension)
		=> extension.Client.CurrentApplication.RedirectUris.Contains(extension.Configuration.RedirectUri);
}
