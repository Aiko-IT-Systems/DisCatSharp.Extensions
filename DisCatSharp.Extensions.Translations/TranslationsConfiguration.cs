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

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.Extensions.Translations;

/// <summary>
///     Represents a configuration for <see cref="TranslationsExtension" />.
/// </summary>
public class TranslationsConfiguration
{
	/// <summary>
	///     Creates a new instance of <see cref="TranslationsConfiguration" />.
	/// </summary>
	[ActivatorUtilitiesConstructor]
	public TranslationsConfiguration()
	{ }

	/// <summary>
	///     Initializes a new instance of the <see cref="TranslationsConfiguration" /> class.
	/// </summary>
	/// <param name="provider">The service provider.</param>
	[ActivatorUtilitiesConstructor]
	public TranslationsConfiguration(IServiceProvider provider)
	{
		this.ServiceProvider = provider;
	}

	/// <summary>
	///     Creates a new instance of <see cref="TranslationsConfiguration" />, copying the properties of another configuration.
	/// </summary>
	/// <param name="other">Configuration the properties of which are to be copied.</param>
	public TranslationsConfiguration(TranslationsConfiguration other)
	{
		this.TranslationsFolder = other.TranslationsFolder;
		this.TranslationsFileName = other.TranslationsFileName;
		this.ServiceProvider = other.ServiceProvider;
		this.DefaultLocale = other.DefaultLocale;
	}

	/// <summary>
	///     <para>Sets the service provider for this Translations instance.</para>
	///     <para>
	///         Objects in this provider are used when instantiating command modules. This allows passing data around without
	///         resorting to static members.
	///     </para>
	///     <para>Defaults to an empty service provider.</para>
	/// </summary>
	public IServiceProvider ServiceProvider { internal get; set; } = new ServiceCollection().BuildServiceProvider(true);

	/// <summary>
	///     Sets the folder where translation files are stored. Defaults to <c>data/translations</c>.
	/// </summary>
	public string TranslationsFolder { internal get; set; } = "data/translations";

	/// <summary>
	///     Sets the name of the translations file. Defaults to <c>strings.json</c>.
	/// </summary>
	public string TranslationsFileName { internal get; set; } = "strings.json";

	/// <summary>
	/// Gets or sets the default locale used for culture-specific operations.
	/// </summary>
	public string DefaultLocale { internal get; set; } = "en-US";
}
