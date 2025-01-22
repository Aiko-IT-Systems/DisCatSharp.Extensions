using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;

namespace DisCatSharp.Extensions.SimpleMusicCommands.Commands;

[SlashCommandGroup("music", "Music commands", allowedContexts: [InteractionContextType.Guild], integrationTypes: [ApplicationCommandIntegrationTypes.GuildInstall])]
public sealed class MusicCommands : ApplicationCommandsModule
{
	[SlashCommandGroup("control", "Music control commands")]
	public sealed class ControlCommands : ApplicationCommandsModule
	{
		[SlashCommand("connect", "Connects to a channel")]
		public async Task ConnectAsync(InteractionContext ctx, [Option("channel", "Channel to connect to"), ChannelTypes(ChannelType.Voice, ChannelType.Stage)] DiscordChannel voiceChannel)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"Trying to connect to {voiceChannel.Mention}"));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var session = ctx.Client.GetLavalink().GetIdealSession(voiceChannel.RtcRegion);
				ArgumentNullException.ThrowIfNull(session);
				await session.ConnectAsync(voiceChannel);
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Connected to {voiceChannel.Mention}."));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Please make sure you're calling this from within the guild you want to connect to."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}

		[SlashCommand("switch_to", "Switches to another channel")]
		public async Task SwitchToChannelAsync(InteractionContext ctx, [Option("channel", "Channel to switch to"), ChannelTypes(ChannelType.Voice, ChannelType.Stage)] DiscordChannel voiceChannel)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"Trying to switch to {voiceChannel.Mention}"));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				var prev = player.Channel;
				await player.SwitchChannelAsync(voiceChannel);
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Switched successfully from {prev.Mention} to {voiceChannel.Mention}."));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Please make sure you're calling this from within the guild you want to connect to, and that the bot is already connected to another channel."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}

		[SlashCommand("disconnect", "Disconnects the player")]
		public async Task DisconnectAsync(InteractionContext ctx)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Disconnecting.."));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				await player.DisconnectAsync();
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Success!"));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Make sure you're connected to a channel."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}

		[SlashCommand("pause", "Pauses the playback")]
		public async Task PauseAsync(InteractionContext ctx)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Pausing.."));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				await player.PauseAsync();
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Success!"));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Make sure you're connected to a channel."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}

		[SlashCommand("resume", "Resumes the playback")]
		public async Task ResumeAsync(InteractionContext ctx)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Resuming.."));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				await player.ResumeAsync();
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Success!"));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Make sure you're connected to a channel."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}

		[SlashCommand("skip", "Skips the current song")]
		public async Task SkipAsync(InteractionContext ctx)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Skipping song.."));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				await player.SkipAsync();
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Success!"));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Make sure you're connected to a channel."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}

		[SlashCommand("stop", "Stops the playback and clears the queue")]
		public async Task StopAsync(InteractionContext ctx)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Stopping.."));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				player.ClearQueue();
				await player.StopAsync();
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Success!"));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Make sure you're connected to a channel."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}

		[SlashCommand("now_playing", "Gets the now playing information")]
		public async Task NowPlayingAsync(InteractionContext ctx)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Getting info.."));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				ArgumentNullException.ThrowIfNull(player.CurrentTrack);
				var builder = new DiscordEmbedBuilder
				{
					Title = "Now Playing",
					Color = DiscordColor.Goldenrod,
					Thumbnail = player.CurrentTrack.Info.ArtworkUrl is not null
						? new()
						{
							Url = player.CurrentTrack.Info.ArtworkUrl.AbsoluteUri
						}
						: null!,
					Description = $"[{player.CurrentTrack.Info.Title}]({player.CurrentTrack.Info.Uri}) by {player.CurrentTrack.Info.Author}"
				};
				builder.AddField(new("In queue", player.Queue.Count.ToString()));
				builder.AddField(new("Next song", player.TryPeekQueue(out var track) ? $"[{track.Info.Title}]({track.Info.Uri}) by {track.Info.Author}" : "None"));
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder.Build()));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Make sure you're connected to a channel and are playing a song currently."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}
	}

	[SlashCommandGroup("option", "Music option commands")]
	public sealed class OptionCommands : ApplicationCommandsModule
	{
		[SlashCommand("set_volume", "Sets the volume")]
		public async Task SetVolumeAsync(InteractionContext ctx, [Option("volume", "The volume in % to set the player to"), MinimumValue(0), MaximumValue(150)] int volume)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Setting volume.."));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				await player.SetVolumeAsync(volume);
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Success! Set volume to {volume}%"));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Make sure you're connected to a channel."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}

		[SlashCommand("seek", "Seeks the current track to given position")]
		public async Task SeekAsync(InteractionContext ctx, [Option("position", "The total seconds to seek to")] long seconds)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Seeking.."));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				var ts = TimeSpan.FromSeconds(seconds);
				await player.SeekAsync(ts);
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Success! Seeked to {ts}"));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Make sure you're connected to a channel and are currently playing a song."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}
	}

	[SlashCommandGroup("queue", "Music queue commands")]
	public sealed class QueueCommands : ApplicationCommandsModule
	{
		[SlashCommand("play", "Starts the queue playback")]
		public async Task PlayQueueAsync(InteractionContext ctx)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Trying to play.."));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				player.PlayQueue();
				player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Playing {player!.CurrentTrack!.Info.Title.Bold} from {player.CurrentTrack.Info.Author.Italic}"));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Make sure you're connected to a channel. And you added songs to the queue."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}

		[SlashCommand("add_playlist", "Adds a playlist to the queue (youtube.com/playlist?list=XYZ)")]
		public async Task QueuePlaylistAsync(InteractionContext ctx, [Option("url", "The url to play")] string url)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Adding playlist.."));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				var loadResult = await player.LoadTracksAsync(LavalinkSearchType.Plain, url);
				LavalinkPlaylist playlist;
				switch (loadResult.LoadType)
				{
					case LavalinkLoadResultType.Error:
						throw new(loadResult.GetResultAs<LavalinkException>().Message);
					case LavalinkLoadResultType.Empty:
					case LavalinkLoadResultType.Track:
					case LavalinkLoadResultType.Search:
						await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Not a playlist."));
						return;
					case LavalinkLoadResultType.Playlist:
						playlist = loadResult.GetResultAs<LavalinkPlaylist>();
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(loadResult.LoadType));
				}

				player.AddToQueue(playlist);
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added {playlist.Tracks.Count.ToString().Bold()} songs from {playlist.Info.Name.Bold()} to the queue."));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Make sure you're connected to a channel."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}

		[SlashCommand("add_song", "Adds a song to the queue")]
		public async Task QueueSongAsync(InteractionContext ctx, [Option("url", "The url to play")] string url)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Adding song.."));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				var loadResult = await player.LoadTracksAsync(LavalinkSearchType.Plain, url);
				LavalinkTrack track;
				switch (loadResult.LoadType)
				{
					case LavalinkLoadResultType.Empty:
						await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No tracks found."));
						return;
					case LavalinkLoadResultType.Error:
						throw new(loadResult.GetResultAs<LavalinkException>().Message);
					case LavalinkLoadResultType.Track:
						track = loadResult.GetResultAs<LavalinkTrack>();
						break;
					case LavalinkLoadResultType.Search:
						track = loadResult.GetResultAs<List<LavalinkTrack>>().First();
						break;
					case LavalinkLoadResultType.Playlist:
						track = loadResult.GetResultAs<LavalinkPlaylist>().Tracks.First();
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(loadResult.LoadType));
				}

				player.AddToQueue(track);
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added {track.Info.Title.Bold()} by {track.Info.Author.Italic()} to the queue."));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Make sure you're connected to a channel."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}

		[SlashCommand("shuffle", "Shuffles the queue")]
		public async Task ShuffleQueueAsync(InteractionContext ctx)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Shuffling queue.."));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				player.ShuffleQueue();
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Success!"));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Make sure you're connected to a channel and have songs in the queue."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}

		[SlashCommand("clear", "Clears the queue")]
		public async Task ClearQueueAsync(InteractionContext ctx)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Clearing queue.."));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				player.ClearQueue();
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Success!"));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Make sure you're connected to a channel and have songs in the queue."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}

		[SlashCommand("reverse", "Reverses the queue")]
		public async Task ReverseQueueAsync(InteractionContext ctx)
		{
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Reversing queue.."));
			try
			{
				ArgumentNullException.ThrowIfNull(ctx.Guild);
				var player = ctx.Client.GetLavalink().GetGuildPlayer(ctx.Guild);
				ArgumentNullException.ThrowIfNull(player);
				player.ReverseQueue();
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Success!"));
			}
			catch (ArgumentNullException)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Make sure you're connected to a channel and have songs in the queue."));
			}
			catch (Exception ex)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(ex.Message ?? "No message"));
			}
		}
	}
}
