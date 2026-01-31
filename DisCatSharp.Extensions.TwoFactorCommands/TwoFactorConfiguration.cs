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

using TwoFactorAuthNet;

namespace DisCatSharp.Extensions.TwoFactorCommands;

/// <summary>
///     Represents a configuration for <see cref="TwoFactorExtension" />.
/// </summary>
public sealed class TwoFactorConfiguration
{
	/// <summary>
	///     Creates a new instance of <see cref="TwoFactorConfiguration" />.
	/// </summary>
	[ActivatorUtilitiesConstructor]
	public TwoFactorConfiguration()
	{ }

	/// <summary>
	///     Initializes a new instance of the <see cref="TwoFactorConfiguration" /> class.
	/// </summary>
	/// <param name="provider">The service provider.</param>
	[ActivatorUtilitiesConstructor]
	public TwoFactorConfiguration(IServiceProvider provider)
	{
		this.ServiceProvider = provider;
	}

	/// <summary>
	///     Creates a new instance of <see cref="TwoFactorConfiguration" />, copying the properties of another configuration.
	/// </summary>
	/// <param name="other">Configuration the properties of which are to be copied.</param>
	public TwoFactorConfiguration(TwoFactorConfiguration other)
	{
		this.Digits = other.Digits;
		this.Period = other.Period;
		this.Algorithm = other.Algorithm;
		this.Issuer = other.Issuer;
		this.ResponseConfiguration = other.ResponseConfiguration;
	}

	/// <summary>
	///     <para>Sets the service provider for this TwoFactor instance.</para>
	///     <para>
	///         Objects in this provider are used when instantiating command modules. This allows passing data around without
	///         resorting to static members.
	///     </para>
	///     <para>Defaults to an empty service provider.</para>
	/// </summary>
	public IServiceProvider ServiceProvider { internal get; set; } = new ServiceCollection().BuildServiceProvider(true);

	/// <summary>
	///     Sets the length of digits for the 2fa code. Defaults to <c>6</c>.
	/// </summary>
	public int Digits { internal get; set; } = 6;

	/// <summary>
	///     Sets the period in seconds how long a code is valid. Defaults to <c>30</c> seconds..
	/// </summary>
	public int Period { internal get; set; } = 30;

	/// <summary>
	///     Sets the algorithm. Defaults to <see cref="TwoFactorAuthNet.Algorithm.SHA1" />.
	/// </summary>
	public Algorithm Algorithm { internal get; set; } = Algorithm.SHA1;

	/// <summary>
	///     <para>Sets the issuer.</para>
	///     <para>Defaults to <c>aitsys.dev</c> to show the DisCatSharp logo.</para>
	/// </summary>
	public string Issuer { internal get; set; } = "aitsys.dev";

	/// <summary>
	///     Sets the database path. Defaults to <c>./twofactor.sqlite</c>.
	/// </summary>
	public string DatabasePath { internal get; set; } = "./twofactor.sqlite";

	/// <summary>
	///     Sets how long user have to enter the 2fa code. Defaults to <c>30</c> seconds.
	/// </summary>
	public int TwoFactorTimeout { internal get; set; } = 30;

	/// <summary>
	///     Sets overrides for default messages facing the user.
	/// </summary>
	public TwoFactorResponseConfiguration ResponseConfiguration { internal get; set; } = new();
}

/// <summary>
///     Represents a message configuration for <see cref="TwoFactorExtension" />.
/// </summary>
public sealed class TwoFactorResponseConfiguration
{
	/// <summary>
	///     Creates a new instance of <see cref="TwoFactorResponseConfiguration" />.
	/// </summary>
	[ActivatorUtilitiesConstructor]
	public TwoFactorResponseConfiguration()
	{ }

	/// <summary>
	///     <para>
	///         Whether to show a response after entering a two factor code. If set to false, you'll need to respond to the
	///         modal interaction yourself.
	///     </para>
	///     <para>Defaults to: true</para>
	/// </summary>
	public bool ShowResponse { internal get; set; } = true;

	/// <summary>
	///     <para>Sets the message when an correct two factor auth code was entered.</para>
	///     <para>Defaults to: Code valid!</para>
	/// </summary>
	public string AuthenticationSuccessMessage { internal get; set; } = "Code valid!";

	/// <summary>
	///     <para>Sets the message when an incorrect two factor auth code was entered.</para>
	///     <para>Defaults to: Code invalid..</para>
	/// </summary>
	public string AuthenticationFailureMessage { internal get; set; } = "Code invalid..";

	/// <summary>
	///		<para>Sets the message when an user successfully enrolled into two factor auth.</para>
	///		<para>Defaults to: You successfully enrolled in two factor.</para>
	/// </summary>
	public string AuthenticationEnrolledMessage { internal get; set; } = "You successfully enrolled in two factor.";

	/// <summary>
	///		<para>Sets the message when an user successfully unenrolled from two factor auth.</para>
	///		<para>Defaults to: You successfully unenrolled from two factor.</para>
	/// </summary>
	public string AuthenticationUnenrolledMessage { internal get; set; } = "You successfully unenrolled from two factor.";

	/// <summary>
	///     <para>Sets the message when an user is not yet enrolled into two factor auth.</para>
	///     <para>Defaults to: You are not enrolled in two factor.</para>
	/// </summary>
	public string AuthenticationNotEnrolledMessage { internal get; set; } = "You are not enrolled in two factor.";

	/// <summary>
	///     <para>Sets the modal title for two factor auth requests.</para>
	///     <para>Defaults to: Enter 2FA Code</para>
	/// </summary>
	public string AuthenticationModalRequestTitle { internal get; set; } = "Enter 2FA Code";

	/// <summary>
	///     <para>Sets the account prefix used within the authenticator upon registration.</para>
	///     <para>Defaults to: DisCatSharp Auth</para>
	/// </summary>
	public string AuthenticatorAccountPrefix { internal get; set; } = "DisCatSharp Auth";
}
