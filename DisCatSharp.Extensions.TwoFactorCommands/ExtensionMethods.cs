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
using System.Reflection;
using System.Threading.Tasks;

using DisCatSharp.CommandsNext.Builders;
using DisCatSharp.CommandsNext.Converters;
using DisCatSharp.Enums;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DisCatSharp.Extensions.TwoFactorCommands;

/// <summary>
/// Defines various extensions specific to TwoFactor.
/// </summary>
public static class ExtensionMethods
{
	/// <summary>
	/// Enables TwoFactor module on this <see cref="DiscordClient"/>.
	/// </summary>
	/// <param name="client">Client to enable TwoFactor for.</param>
	/// <param name="cfg">TwoFactor configuration to use.</param>
	/// <returns>Created <see cref="TwoFactorExtension"/>.</returns>
	public static TwoFactorExtension UseTwoFactor(this DiscordClient client, TwoFactorConfiguration cfg)
	{
		if (client.GetExtension<TwoFactorExtension>() != null)
			throw new InvalidOperationException("CommandsNext is already enabled for that client.");

		cfg.ServiceProvider ??= client.ServiceProvider ?? new ServiceCollection().BuildServiceProvider(true);

		var cnext = new TwoFactorExtension(cfg);
		client.AddExtension(cnext);
		return cnext;
	}

	/// <summary>
	/// Enables TwoFactor module on all shards in this <see cref="DiscordShardedClient"/>.
	/// </summary>
	/// <param name="client">Client to enable TwoFactor for.</param>
	/// <param name="cfg">TwoFactor configuration to use.</param>
	/// <returns>A dictionary of created <see cref="TwoFactorExtension"/>, indexed by shard id.</returns>
	public static async Task<IReadOnlyDictionary<int, TwoFactorExtension>> UseTwoFactorAsync(this DiscordShardedClient client, TwoFactorConfiguration cfg)
	{
		var modules = new Dictionary<int, TwoFactorExtension>();
		await client.InitializeShardsAsync().ConfigureAwait(false);

		foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
		{
			var cnext = shard.GetExtension<TwoFactorExtension>();
			cnext ??= shard.UseTwoFactor(cfg);

			modules[shard.ShardId] = cnext;
		}

		return new ReadOnlyDictionary<int, TwoFactorExtension>(modules);
	}

	/// <summary>
	/// Gets the active TwoFactor module for this client.
	/// </summary>
	/// <param name="client">Client to get TwoFactor module from.</param>
	/// <returns>The module, or null if not activated.</returns>
	public static TwoFactorExtension GetTwoFactor(this DiscordClient client)
		=> client.GetExtension<TwoFactorExtension>();


	/// <summary>
	/// Gets the active TwoFactor modules for all shards in this client.
	/// </summary>
	/// <param name="client">Client to get TwoFactor instances from.</param>
	/// <returns>A dictionary of the modules, indexed by shard id.</returns>
	public static async Task<IReadOnlyDictionary<int, TwoFactorExtension>> GetTwoFactorAsync(this DiscordShardedClient client)
	{
		await client.InitializeShardsAsync().ConfigureAwait(false);
		var extensions = new Dictionary<int, TwoFactorExtension>();

		foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
		{
			extensions.Add(shard.ShardId, shard.GetExtension<TwoFactorExtension>());
		}

		return new ReadOnlyDictionary<int, TwoFactorExtension>(extensions);
	}
}
