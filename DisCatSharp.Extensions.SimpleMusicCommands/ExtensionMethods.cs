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

using DisCatSharp.Lavalink;

namespace DisCatSharp.Extensions.SimpleMusicCommands;

/// <summary>
///     Defines various extensions specific to SimpleMusicCommandsExtension.
/// </summary>
public static class ExtensionMethods
{
	/// <summary>
	///     Enables SimpleMusicCommandsExtension module on this <see cref="DiscordClient" />.
	/// </summary>
	/// <param name="client">Client to enable SimpleMusicCommandsExtension for.</param>
	/// <param name="cfg">Lavalink configuration to use.</param>
	/// <returns>Created <see cref="SimpleMusicCommandsExtension" />.</returns>
	public static SimpleMusicCommandsExtension UseSimpleMusicCommands(this DiscordClient client, LavalinkConfiguration? cfg = null)
	{
		if (client.GetExtension<SimpleMusicCommandsExtension>() != null)
			throw new InvalidOperationException("SimpleMusicCommandsExtension is already enabled for that client.");

		cfg ??= new();

		var smc = new SimpleMusicCommandsExtension(cfg);
		client.UseLavalink();
		client.AddExtension(smc);
		return smc;
	}

	/// <summary>
	///     Enables SimpleMusicCommandsExtension module on all shards in this <see cref="DiscordShardedClient" />.
	/// </summary>
	/// <param name="client">Client to enable SimpleMusicCommandsExtension for.</param>
	/// <param name="cfg">Lavalink configuration to use.</param>
	/// <returns>A dictionary of created <see cref="SimpleMusicCommandsExtension" />, indexed by shard id.</returns>
	public static async Task<IReadOnlyDictionary<int, SimpleMusicCommandsExtension>> UseSimpleMusicCommandsAsync(this DiscordShardedClient client, LavalinkConfiguration? cfg = null)
	{
		var modules = new Dictionary<int, SimpleMusicCommandsExtension>();
		await client.InitializeShardsAsync().ConfigureAwait(false);

		foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
		{
			var smc = shard.GetExtension<SimpleMusicCommandsExtension>();
			smc ??= shard.UseSimpleMusicCommands(cfg);

			modules[shard.ShardId] = smc;
		}

		return new ReadOnlyDictionary<int, SimpleMusicCommandsExtension>(modules);
	}

	/// <summary>
	///     Gets the active SimpleMusicCommandsExtension module for this client.
	/// </summary>
	/// <param name="client">Client to get SimpleMusicCommandsExtension module from.</param>
	/// <returns>The module, or null if not activated.</returns>
	public static SimpleMusicCommandsExtension? GetSimpleMusicCommandsExtension(this DiscordClient client)
		=> client.GetExtension<SimpleMusicCommandsExtension>();

	/// <summary>
	///     Gets the active SimpleMusicCommandsExtension modules for all shards in this client.
	/// </summary>
	/// <param name="client">Client to get SimpleMusicCommandsExtension instances from.</param>
	/// <returns>A dictionary of the modules, indexed by shard id.</returns>
	public static async Task<IReadOnlyDictionary<int, SimpleMusicCommandsExtension?>> GetSimpleMusicCommandsExtension(this DiscordShardedClient client)
	{
		await client.InitializeShardsAsync().ConfigureAwait(false);
		var extensions = client.ShardClients.Select(xkvp => xkvp.Value).ToDictionary(shard => shard.ShardId, shard => shard.GetExtension<SimpleMusicCommandsExtension>());

		return new ReadOnlyDictionary<int, SimpleMusicCommandsExtension?>(extensions);
	}
}
