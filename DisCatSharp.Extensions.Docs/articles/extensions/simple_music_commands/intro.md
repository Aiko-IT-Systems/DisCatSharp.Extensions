---
uid: extensions_simple_music_commands_intro
title: Simple Music Commands Introduction
description: Introduction to Simple Music Commands
author: DisCatSharp Team
---

# Simple Music Commands

## What is this?

This extensions turns your DisCatSharp bot into a music bot, without the trouble of writing each command.

All you need to do is to follow the setup for Lavalink mentioned in https://docs.dcs.aitsys.dev/articles/modules/audio/lavalink_v4/docker.

## Quick run-down

```cs
DiscordClient client;

// Initiate the application commands module first

// Initiate the simple music commands module
client.UseSimpleMusicCommands(new LavalinkConfiguration());

// Register the music commands
var extension = await Client.GetSimpleMusicCommandsExtension();
extension.RegisterMusicCommands();

// Connect the discord client first!
await client.ConnectAsync();

// Connect the extension
await extension.ConnectAsync();
```

## And that does?

It creates and handles application commands for your bot, all by itself 💕

![Commands 1](/images/simple_music_commands_commandblock_1.png) ![Commands 2](/images/simple_music_commands_commandblock_2.png)
