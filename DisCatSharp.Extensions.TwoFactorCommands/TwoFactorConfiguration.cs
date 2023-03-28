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
/// Represents a configuration for <see cref="TwoFactorConfiguration"/>.
/// </summary>
public sealed class TwoFactorConfiguration
{
	/// <summary>
	/// <para>Sets the service provider for this TwoFactor instance.</para>
	/// <para>Objects in this provider are used when instantiating command modules. This allows passing data around without resorting to static members.</para>
	/// <para>Defaults to an empty service provider.</para>
	/// </summary>
	public IServiceProvider ServiceProvider { internal get; set; }

	/// <summary>
	/// Gets the length of digits for the 2fa code. Defaults to 6.
	/// </summary>
	public int Digits { internal get; set; } = 6;

	/// <summary>
	/// Gets the period in seconds how long a code is valid. Defaults to 30.
	/// </summary>
	public int Period { internal get; set; } = 30;

	/// <summary>
	/// Gets the algorithm. Defaults to <see cref="Algorithm.SHA1"/>.
	/// </summary>
	public Algorithm Algorithm { internal get; set; } = Algorithm.SHA1;

	/// <summary>
	/// <para>Gets the issuer.</para>
	/// <para>Defaults to aitsys.dev to show the DisCatSharp logo.</para>
	/// </summary>
	public string Issuer { internal get; set; } = "aitsys.dev";

	/// <summary>
	/// Creates a new instance of <see cref="TwoFactorConfiguration"/>.
	/// </summary>
	[ActivatorUtilitiesConstructor]
	public TwoFactorConfiguration()
	{

	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TwoFactorConfiguration"/> class.
	/// </summary>
	/// <param name="provider">The service provider.</param>
	[ActivatorUtilitiesConstructor]
	public TwoFactorConfiguration(IServiceProvider provider)
	{
		this.ServiceProvider = provider;
	}

	/// <summary>
	/// Creates a new instance of <see cref="TwoFactorConfiguration"/>, copying the properties of another configuration.
	/// </summary>
	/// <param name="other">Configuration the properties of which are to be copied.</param>
	public TwoFactorConfiguration(TwoFactorConfiguration other)
	{
		this.Digits = other.Digits;
		this.Period = other.Period;
		this.Algorithm = other.Algorithm;
		this.Issuer = other.Issuer;
	}
}
