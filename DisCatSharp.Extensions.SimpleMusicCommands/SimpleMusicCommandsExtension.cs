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
using System.Reflection;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands;
using DisCatSharp.Extensions.SimpleMusicCommands.Commands;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;

namespace DisCatSharp.Extensions.SimpleMusicCommands;

/// <summary>
///     Represents a <see cref="SimpleMusicCommandsExtension" />.
/// </summary>
public sealed class SimpleMusicCommandsExtension : BaseExtension
{
	/// <summary>
	///     Initializes a new instance of the <see cref="SimpleMusicCommandsExtension" /> class.
	/// </summary>
	/// <param name="configuration">The config.</param>
	internal SimpleMusicCommandsExtension(LavalinkConfiguration? configuration = null)
	{
		configuration ??= new();
		configuration.EnableBuiltInQueueSystem = true;
		configuration.QueueEntryFactory = () => new DefaultQueueEntry();
		this.Configuration = configuration;
	}

	/// <summary>
	///     Gets the lavalink configuration.
	/// </summary>
	internal LavalinkConfiguration Configuration { get; }

	/// <summary>
	///     Gets the discord client.
	/// </summary>
	internal DiscordClient Client { get; private set; }

	/// <summary>
	///     DO NOT USE THIS MANUALLY.
	/// </summary>
	/// <param name="client">DO NOT USE THIS MANUALLY.</param>
	/// <exception cref="InvalidOperationException" />
	protected internal override void Setup(DiscordClient client)
	{
		if (this.Client != null)
			throw new InvalidOperationException("What did I tell you?");

		this.Client = client;

		var a = typeof(SimpleMusicCommandsExtension).GetTypeInfo().Assembly;

		var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		if (iv != null)
			this.VersionString = iv.InformationalVersion;
		else
		{
			var v = a.GetName().Version;
			var vs = v.ToString(3);

			if (v.Revision > 0)
				this.VersionString = $"{vs}, CI build {v.Revision}";
		}

		this.HasVersionCheckSupport = true;

		this.RepositoryOwner = "Aiko-IT-Systems";

		this.Repository = "DisCatSharp.Extensions";

		this.PackageId = "DisCatSharp.Extensions.SimpleMusicCommandsExtension";
	}

	/// <summary>
	///     Registers the music commands.
	/// </summary>
	/// <param name="guildIds">
	///     The optional list of guilds to register the commands on. If not supplied, will register as
	///     global commands.
	/// </param>
	public void RegisterMusicCommands(List<ulong>? guildIds = null)
	{
		if (guildIds is null)
			this.Client.GetApplicationCommands().RegisterGlobalCommands<MusicCommands>();
		else
			foreach (var guildId in guildIds)
				this.Client.GetApplicationCommands().RegisterGuildCommands<MusicCommands>(guildId);
	}

	/// <summary>
	///     Connects to lavalink.
	/// </summary>
	public async Task ConnectAsync()
	{
		var ex = this.Client.GetLavalink();
		await ex.ConnectAsync(this.Configuration);
	}
}
