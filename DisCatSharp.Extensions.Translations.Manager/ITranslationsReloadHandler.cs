// This file is part of the DisCatSharp project.
//
// Copyright (c) 2021-2025 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to do so.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Extensions.Translations.Manager;

/// <summary>
/// Provides a hook to refresh translations after the backing store changes.
/// </summary>
internal interface ITranslationsReloadHandler
{
	Task ReloadAsync(CancellationToken cancellationToken = default);
}

internal sealed class NoOpTranslationsReloadHandler : ITranslationsReloadHandler
{
	public Task ReloadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class DiscordTranslationsReloadHandler : ITranslationsReloadHandler
{
	private readonly DiscordClient? _client;
	private readonly DiscordShardedClient? _shardedClient;

	public DiscordTranslationsReloadHandler(DiscordClient client)
	{
		this._client = client;
	}

	public DiscordTranslationsReloadHandler(DiscordShardedClient client)
	{
		this._shardedClient = client;
	}

	public async Task ReloadAsync(CancellationToken cancellationToken = default)
	{
		if (this._client is not null)
		{
			var extension = this._client.GetTranslationsExtension();
			extension?.TranslationEngine.Reload();
		}

		if (this._shardedClient is not null)
		{
			var extensions = await this._shardedClient.GetTranslationsExtension().ConfigureAwait(false);
			foreach (var kvp in extensions)
				kvp.Value?.TranslationEngine.Reload();
		}
	}
}
