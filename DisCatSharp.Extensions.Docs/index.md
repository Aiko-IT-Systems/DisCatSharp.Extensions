---
uid: home
title: DisCatSharp Extensions
author: DisCatSharp Team
hasDiscordComponents: true
_disableAffix: true
_disableBreadcrumb: true
_disableNextArticle: true
---

<section class="catpunk-hero">
	<div>
		<div class="catpunk-eyebrow">Official DisCatSharp Extensions</div>
		<h1 class="catpunk-hero-title"><span class="catpunk-gradient-text">More tricks. Same sharp claws.</span></h1>
		<p class="catpunk-hero-copy">
			Focused packages for bots that need a little more than the core library: reusable music commands,
			two-factor authentication flows, and more.
		</p>
	</div>
	<div class="catpunk-discord-preview">
		<discord-messages>
			<discord-header guild="DisCatSharp" channel="extensions" icon="https://i.imgur.com/sHdXUPx.png"></discord-header>
			<discord-message profile="user">Can my bot grow without stuffing everything into one project?</discord-message>
			<discord-message profile="dcs" highlight>
				<discord-reply slot="reply" profile="user" mentions>Can my bot grow without stuffing...</discord-reply>
				Yep. Pick the official <discord-bold>Extensions</discord-bold> you need and keep the rest of your stack pleasantly small.
				<discord-reactions slot="reactions">
					<discord-reaction interactive="true" name="catjam" emoji="https://cdn.discordapp.com/emojis/1059823127271575612.gif" count="23"></discord-reaction>
				</discord-reactions>
			</discord-message>
		</discord-messages>
	</div>
</section>

<section class="catpunk-card-grid" aria-label="DisCatSharp extension packages">
	<a class="catpunk-link-card" href="/articles/extensions/simple_music_commands/intro.html">
		<strong>Simple Music Commands</strong>
		<span>Drop-in Lavalink music commands for common playback workflows.</span>
	</a>
	<a class="catpunk-link-card" href="/articles/extensions/twofactor_commands/intro.html">
		<strong>Two-Factor Commands</strong>
		<span>Add enrollment and verification flows to sensitive bot commands.</span>
	</a>
</section>

<section class="catpunk-panel">
	<h2>Install only what your bot needs</h2>
	<p>Each extension is published as its own NuGet package and builds on DisCatSharp.</p>
	<div class="catpunk-terminal">
		<div class="catpunk-terminal-bar"><span class="catpunk-dot" aria-hidden="true"></span></div>
		<pre><code class="lang-powershell">dotnet add package DisCatSharp.Extensions.SimpleMusicCommands --prerelease
dotnet add package DisCatSharp.Extensions.TwoFactorCommands --prerelease</code></pre>
	</div>
</section>

<section class="catpunk-panel">
	<div class="catpunk-eyebrow">One searchable ecosystem</div>
	<h2>Core and Extensions, together.</h2>
	<p>
		The public DisCatSharp MCP searches both the main library and official Extensions. Agents can resolve overload-safe API
		symbols, fetch complete documentation records, and read approved source ranges without juggling separate servers.
	</p>
	<div class="catpunk-terminal">
		<div class="catpunk-terminal-bar"><span class="catpunk-dot" aria-hidden="true"></span></div>
		<pre><code>https://docs.dcs.aitsys.dev/mcp</code></pre>
	</div>
	<div class="catpunk-actions">
		<a class="catpunk-button" href="https://docs.dcs.aitsys.dev/articles/misc/agent_setup">Install the skill</a>
		<a class="catpunk-button secondary" href="https://docs.dcs.aitsys.dev/articles/misc/agent_setup#connect-the-documentation-mcp">Connect MCP</a>
	</div>
</section>

<section class="catpunk-link-grid" aria-label="Extension documentation links">
	<a class="catpunk-link-card" href="/articles/">
		<strong>Guides</strong>
		<span>Setup, usage, migrations, nightlies, and troubleshooting.</span>
	</a>
	<a class="catpunk-link-card" href="/api/">
		<strong>API Reference</strong>
		<span>Generated packages, types, members, and overloads.</span>
	</a>
	<a class="catpunk-link-card" href="https://docs.dcs.aitsys.dev/">
		<strong>Main documentation</strong>
		<span>Return to the core library, modules, and current changelogs.</span>
	</a>
	<a class="catpunk-link-card" href="https://github.com/Aiko-IT-Systems/DisCatSharp.Extensions">
		<strong>GitHub</strong>
		<span>Source, issues, package projects, and contributions.</span>
	</a>
</section>
