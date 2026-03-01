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
using System.IO;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.Extensions.Translations.Manager;

/// <summary>
///     Represents a configuration for <see cref="TranslationsManagerConfiguration" />.
/// </summary>
public sealed class TranslationsManagerConfiguration : TranslationsConfiguration
{
	/// <summary>
	///     Creates a new instance of <see cref="TranslationsManagerConfiguration" />.
	/// </summary>
	[ActivatorUtilitiesConstructor]
	public TranslationsManagerConfiguration()
	{ }

	/// <summary>
	///     Initializes a new instance of the <see cref="TranslationsManagerConfiguration" /> class.
	/// </summary>
	/// <param name="provider">The service provider.</param>
	[ActivatorUtilitiesConstructor]
	public TranslationsManagerConfiguration(IServiceProvider provider)
	{
		this.ServiceProvider = provider;
	}

	/// <summary>
	///     Creates a new instance of <see cref="TranslationsManagerConfiguration" />, copying the properties of another configuration.
	/// </summary>
	/// <param name="other">Configuration the properties of which are to be copied.</param>
	public TranslationsManagerConfiguration(TranslationsManagerConfiguration other)
	{
		this.TranslationsFolder = other.TranslationsFolder;
		this.TranslationsFileName = other.TranslationsFileName;
		this.ServiceProvider = other.ServiceProvider;
		this.DefaultLocale = other.DefaultLocale;
		this.ServerPort = other.ServerPort;
		this.RepoRoot = other.RepoRoot;
		this.SourceDirectory = other.SourceDirectory;
		this.AuditOutputPath = other.AuditOutputPath;
		this.ReportCacheDuration = other.ReportCacheDuration;
		this.WriteAuditToDisk = other.WriteAuditToDisk;
	}

	/// <summary>
	///    Sets the port the translation server will run on. Defaults to <c>5000</c>.
	/// </summary>
	public int ServerPort { get; set; } = 5000;

	/// <summary>
	/// Sets the repository root used for usage scanning and opening files from the UI.
	/// Defaults to the current working directory.
	/// </summary>
	public string RepoRoot { get; set; } = Directory.GetCurrentDirectory();

	/// <summary>
	/// Sets the source directory scanned for translation usages. Defaults to "src" under <see cref="RepoRoot"/>.
	/// </summary>
	public string SourceDirectory { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "src");

	/// <summary>
	/// Optional path where the generated audit report is written. Defaults to translations folder.
	/// </summary>
	public string AuditOutputPath { get; set; } = Path.Combine("data", "translation_audit.json");

	/// <summary>
	/// Duration to cache audit reports before rescanning the repository.
	/// </summary>
	public TimeSpan ReportCacheDuration { get; set; } = TimeSpan.FromSeconds(30);

	/// <summary>
	/// When true, writes the audit report to <see cref="AuditOutputPath"/> on each request.
	/// </summary>
	public bool WriteAuditToDisk { get; set; } = false;
}
