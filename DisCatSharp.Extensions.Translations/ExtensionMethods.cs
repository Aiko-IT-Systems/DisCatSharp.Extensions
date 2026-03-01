// This file is part of the DisCatSharp project.
//
// Copyright (c) 2021-2025 AITSYS
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

using DisCatSharp.Entities;

namespace DisCatSharp.Extensions.Translations;

/// <summary>
///     Defines various extensions specific to TranslationsExtension.
/// </summary>
public static class ExtensionMethods
{
	/// <summary>
	///     Enables TranslationsExtension module on this <see cref="DiscordClient" />.
	/// </summary>
	/// <param name="client">Client to enable TranslationsExtension for.</param>
	/// <param name="cfg">Lavalink configuration to use.</param>
	/// <returns>Created <see cref="TranslationsExtension" />.</returns>
	public static TranslationsExtension UseTranslations(this DiscordClient client, TranslationsConfiguration? cfg = null)
	{
		if (client.GetExtension<TranslationsExtension>() != null)
			throw new InvalidOperationException("TranslationsExtension is already enabled for that client.");

		cfg ??= new();

		var smc = new TranslationsExtension(cfg);
		client.AddExtension(smc);
		return smc;
	}

	/// <summary>
	///     Enables TranslationsExtension module on all shards in this <see cref="DiscordShardedClient" />.
	/// </summary>
	/// <param name="client">Client to enable TranslationsExtension for.</param>
	/// <param name="cfg">Lavalink configuration to use.</param>
	/// <returns>A dictionary of created <see cref="TranslationsExtension" />, indexed by shard id.</returns>
	public static async Task<IReadOnlyDictionary<int, TranslationsExtension>> UseTranslationsAsync(this DiscordShardedClient client, TranslationsConfiguration? cfg = null)
	{
		var modules = new Dictionary<int, TranslationsExtension>();
		await client.InitializeShardsAsync().ConfigureAwait(false);

		foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
		{
			var smc = shard.GetExtension<TranslationsExtension>();
			smc ??= shard.UseTranslations(cfg);
			modules[shard.ShardId] = smc;
		}

		return new ReadOnlyDictionary<int, TranslationsExtension>(modules);
	}

	/// <summary>
	///     Gets the active TranslationsExtension module for this client.
	/// </summary>
	/// <param name="client">Client to get TranslationsExtension module from.</param>
	/// <returns>The module, or null if not activated.</returns>
	public static TranslationsExtension? GetTranslationsExtension(this DiscordClient client)
		=> client.GetExtension<TranslationsExtension>();

	/// <summary>
	///     Gets the active TranslationsExtension modules for all shards in this client.
	/// </summary>
	/// <param name="client">Client to get TranslationsExtension instances from.</param>
	/// <returns>A dictionary of the modules, indexed by shard id.</returns>
	public static async Task<IReadOnlyDictionary<int, TranslationsExtension?>> GetTranslationsExtension(this DiscordShardedClient client)
	{
		await client.InitializeShardsAsync().ConfigureAwait(false);
		var extensions = client.ShardClients.Select(xkvp => xkvp.Value).ToDictionary(shard => shard.ShardId, shard => shard.GetExtension<TranslationsExtension>());

		return new ReadOnlyDictionary<int, TranslationsExtension?>(extensions);
	}
	
	/// <summary>
	/// Translates a key using the locale from the <see cref="DiscordInteraction"/>.
	/// </summary>
	/// <param name="interaction">The interaction.</param>
	/// <param name="key">The translation key.</param>
	/// <param name="placeholders">The placeholders to replace in the translation.</param>
	/// <param name="forceGuildLocale">Whether to force the guild locale.</param>
	/// <returns>The translated string.</returns>
    public static string T(this DiscordInteraction interaction, string key, object? placeholders = null, bool forceGuildLocale = false)
    {
        var locale = "en-US";
        if (forceGuildLocale && !string.IsNullOrWhiteSpace(interaction.GuildLocale))
            locale = interaction.GuildLocale;
        else if (!string.IsNullOrWhiteSpace(interaction.Locale))
            locale = interaction.Locale;
		else if (!string.IsNullOrWhiteSpace(interaction.GuildLocale))
			locale = interaction.GuildLocale;
        return (interaction.Discord as DiscordClient)!.GetTranslationsExtension()!.TranslationEngine.TLocale(locale, key, placeholders);
    }
}
