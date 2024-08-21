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
using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DisCatSharp.Extensions.OAuth2Web;

/// <summary>
///     Represents a configuration for <see cref="OAuth2WebExtension" />.
/// </summary>
public sealed class OAuth2WebConfiguration
{
	/// <summary>
	///     Creates a new instance of <see cref="OAuth2WebConfiguration" />.
	/// </summary>
	public OAuth2WebConfiguration()
	{ }

	/// <summary>
	///     Initializes a new instance of the <see cref="OAuth2WebConfiguration" /> class.
	/// </summary>
	/// <param name="provider">The service provider.</param>
	[ActivatorUtilitiesConstructor]
	public OAuth2WebConfiguration(IServiceProvider provider)
	{
		this.ServiceProvider = provider;
	}

	/// <summary>
	///     Creates a new instance of <see cref="OAuth2WebConfiguration" />, copying the properties of another configuration.
	/// </summary>
	/// <param name="other">Configuration the properties of which are to be copied.</param>
	public OAuth2WebConfiguration(OAuth2WebConfiguration other)
	{
		this.ClientId = other.ClientId;
		this.ClientSecret = other.ClientSecret;
		this.RedirectUri = other.RedirectUri;
		this.ServiceProvider = other.ServiceProvider;
		this.ListenAll = other.ListenAll;
		this.StartPort = other.StartPort;
		this.SecureStates = other.SecureStates;
		this.ProxyTargetIpOrHost = other.ProxyTargetIpOrHost;
		this.UseHtmlOutput = other.UseHtmlOutput;
		this.HtmlOutputSuccess = other.HtmlOutputSuccess;
		this.HtmlOutputInvalidState = other.HtmlOutputInvalidState;
		this.HtmlOutputSecurityException = other.HtmlOutputSecurityException;
		this.HtmlOutputException = other.HtmlOutputException;
		this.Proxy = other.Proxy;
		this.LogTimestampFormat = other.LogTimestampFormat;
		this.MinimumLogLevel = other.MinimumLogLevel;
		this.LoggerFactory = other.LoggerFactory;
	}

	/// <summary>
	///     <para>Sets the service provider for this OAuth2Web instance.</para>
	///     <para>
	///         Objects in this provider are used when instantiating command modules. This allows passing data around without
	///         resorting to static members.
	///     </para>
	///     <para>Defaults to an empty service provider.</para>
	/// </summary>
	public IServiceProvider ServiceProvider { internal get; set; } = new ServiceCollection().BuildServiceProvider(true);

	/// <summary>
	///     Sets the client id for the OAuth2 client.
	/// </summary>
	public ulong ClientId { internal get; init; }

	/// <summary>
	///     Sets the client secret for the OAuth2 client.
	/// </summary>
	public string ClientSecret { internal get; init; }

	/// <summary>
	///     Sets the redirect uri.
	///     <para>Must be <c>http(s)://(sub).domain.tld/oauth/</c>.</para>
	///     <para>Must end on <c>/</c>.</para>
	///     <para>
	///         If used in sharding, the redirect uri internally appends <see cref="DisCatSharp.DiscordClient.ShardId" />
	///     </para>
	///     <example>
	///         Example: Sharded redirect uri:
	///         <para>
	///             Shard 1 will have the uri
	///             <c>http://<see cref="ProxyTargetIpOrHost">Host</see>:<see cref="StartPort">StartPort</see>/oauth/s0/</c>.
	///         </para>
	///         <para>
	///             Shard 2 will have the uri
	///             <c>http://<see cref="ProxyTargetIpOrHost">Host</see>:<see cref="StartPort">StartPort+1</see>/oauth/s1/</c>.
	///         </para>
	///     </example>
	/// </summary>
	public string RedirectUri { internal get; init; }

	/// <summary>
	///     Whether <see cref="DiscordOAuth2Client.GenerateSecureState" /> and
	///     <see cref="DiscordOAuth2Client.ReadSecureState" /> will be used.
	///     <para>Defaults to <see langword="false" />.</para>
	/// </summary>
	public bool SecureStates { internal get; init; } = false;

	/// <summary>
	///     Sets whether to listen on <c>*:<see cref="StartPort">StartPort</see></c> instead of
	///     <c>127.0.0.1:<see cref="StartPort">StartPort</see></c>.
	///     <para>Defaults to <see langword="false" />.</para>
	/// </summary>
	public bool ListenAll { internal get; init; } = false;

	/// <summary>
	///     Sets the ip address the proxy will target.
	///     <para>Will be <c>127.0.0.1</c> if <see cref="ListenAll" /> is <see langword="false" />.</para>
	/// </summary>
	public string ProxyTargetIpOrHost { internal get; init; }

	/// <summary>
	///     Sets the port to listen on.
	///     <para>Defaults to <c>42069</c>.</para>
	///     <para>If used in sharding, the port will automatically be increased by <c>1</c> per shard.</para>
	///     <example>
	///         Example: Sharding with 2 shards.
	///         <para>Shard 0 will listen on <c>42069</c>.</para>
	///         <para>Shard 1 will listen on <c>42070</c></para>
	///     </example>
	/// </summary>
	public int StartPort { internal get; init; } = 42069;

	/// <summary>
	///     Sets whether to use html output instead of json output for the OAuth2 flow.
	/// </summary>
	public bool UseHtmlOutput { internal get; init; } = false;

	/// <summary>
	///     This will show when the OAuth2 flow was successfully completed, instead of json output.
	/// </summary>
	public string? HtmlOutputSuccess { internal get; init; } = null;

	/// <summary>
	///     This will show when the state parameter is invalid during the OAuth2 flow, instead of json output.
	/// </summary>
	public string? HtmlOutputInvalidState { internal get; init; } = null;

	/// <summary>
	///     This will show when <see cref="SecureStates" /> is active and the state has a mismatch during the OAuth2 flow,
	///     instead of json output.
	/// </summary>
	public string? HtmlOutputSecurityException { internal get; init; } = null;

	/// <summary>
	///     This will show when any exception occured during the OAuth2 flow, instead of json output.
	/// </summary>
	public string? HtmlOutputException { internal get; init; } = null;

	/// <summary>
	///     <para>Sets the minimum logging level for messages.</para>
	///     <para>Defaults to <see cref="LogLevel.Information" />.</para>
	/// </summary>
	public LogLevel MinimumLogLevel { internal get; set; } = LogLevel.Information;

	/// <summary>
	///     <para>Allows you to overwrite the time format used by the internal debug logger.</para>
	///     <para>
	///         Only applicable when <see cref="LoggerFactory" /> is set left at default value. Defaults to ISO 8601-like
	///         format.
	///     </para>
	/// </summary>
	public string LogTimestampFormat { internal get; set; } = "yyyy-MM-dd HH:mm:ss zzz";

	/// <summary>
	///     <para>Sets the proxy to use for HTTP connections to Discord.</para>
	///     <para>Defaults to <see langword="null" />.</para>
	/// </summary>
	public IWebProxy? Proxy { internal get; set; } = null;

	/// <summary>
	///     <para>Sets the logger implementation to use.</para>
	///     <para>To create your own logger, implement the <see cref="Microsoft.Extensions.Logging.ILoggerFactory" /> instance.</para>
	///     <para>Defaults to built-in implementation.</para>
	/// </summary>
	public ILoggerFactory? LoggerFactory { internal get; set; } = null;
}
