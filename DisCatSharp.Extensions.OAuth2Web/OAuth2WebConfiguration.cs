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

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.Extensions.OAuth2Web;

/// <summary>
/// Represents a configuration for <see cref="OAuth2WebExtension"/>.
/// </summary>
public class OAuth2WebConfiguration
{
	/// <summary>
	/// <para>Sets the service provider for this OAuth2Web instance.</para>
	/// <para>Objects in this provider are used when instantiating command modules. This allows passing data around without resorting to static members.</para>
	/// <para>Defaults to an empty service provider.</para>
	/// </summary>
	public IServiceProvider ServiceProvider { internal get; set; }

	/// <summary>
	/// Sets the client id for the OAuth2 client.
	/// </summary>
	public ulong ClientId { internal get; init; }

	/// <summary>
	/// Sets the client secret for the OAuth2 client.
	/// </summary>
	public string ClientSecret { internal get; init; }

	/// <summary>
	/// Sets the redirect uri.
	/// </summary>
	public string RedirectUri { internal get; init; }

	/// <summary>
	/// Whether <see cref="DiscordOAuth2Client.GenerateSecureState"/> and <see cref="DiscordOAuth2Client.ReadSecureState"/> will be used.
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool SecureStates { internal get; init; }

	/// <summary>
	/// Sets whether to listen on <c>*:<see cref="Port">Port</see></c> instead of <c>127.0.0.1:<see cref="Port">Port</see></c>.
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool ListenAll { internal get; init; }

	/// <summary>
	/// Sets the port to listen on.
	/// <para>Defaults to <c>42069</c>.</para>
	/// </summary>
	public int Port { internal get; set; } = 42069;

	/// <summary>
	/// Creates a new instance of <see cref="OAuth2WebConfiguration"/>.
	/// </summary>
	[ActivatorUtilitiesConstructor]
	public OAuth2WebConfiguration()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="OAuth2WebConfiguration"/> class.
	/// </summary>
	/// <param name="provider">The service provider.</param>
	[ActivatorUtilitiesConstructor]
	public OAuth2WebConfiguration(IServiceProvider provider)
	{
		this.ServiceProvider = provider;
	}

	/// <summary>
	/// Creates a new instance of <see cref="OAuth2WebConfiguration"/>, copying the properties of another configuration.
	/// </summary>
	/// <param name="other">Configuration the properties of which are to be copied.</param>
	public OAuth2WebConfiguration(OAuth2WebConfiguration other)
	{
		this.ClientId = other.ClientId;
		this.ClientSecret = other.ClientSecret;
		this.RedirectUri = other.RedirectUri;
		this.ServiceProvider = other.ServiceProvider;
		this.ListenAll = other.ListenAll;
		this.Port = other.Port;
		this.SecureStates = other.SecureStates;
	}
}
