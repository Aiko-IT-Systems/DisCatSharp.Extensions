// This file is part of the DisCatSharp project.
//
// Copyright (c) 2021-2025 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to do so.
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
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Extensions.Translations.Manager;

internal sealed class AuditReportProvider
{
	private readonly TranslationsManagerConfiguration _configuration;
	private readonly StringsStore _store;
	private readonly ILogger<AuditReportProvider> _logger;
	private readonly SemaphoreSlim _gate = new(1, 1);

	private AuditReport? _cached;
	private DateTimeOffset _cacheExpiresAt;

	public AuditReportProvider(TranslationsManagerConfiguration configuration, StringsStore store, ILogger<AuditReportProvider> logger)
	{
		this._configuration = configuration;
		this._store = store;
		this._logger = logger;
	}

	public async Task<AuditReport> GetReportAsync(CancellationToken cancellationToken = default)
	{
		await this._gate.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			if (this._cached is not null && DateTimeOffset.UtcNow < this._cacheExpiresAt)
				return this._cached;

			var translations = this.LoadTranslations();
			var scan = Scanner.ScanSource(this.ResolveSourceDirectory(), this.ResolveRepoRoot());
			var report = AuditReport.Build(translations, scan, this._configuration, this._store.Path);

			if (this._configuration.WriteAuditToDisk)
				this.WriteReport(report);

			this._cached = report;
			this._cacheExpiresAt = DateTimeOffset.UtcNow.Add(this._configuration.ReportCacheDuration);
			return report;
		}
		catch (Exception ex)
		{
			this._logger.LogError(ex, "Failed to build translations audit report.");
			throw;
		}
		finally
		{
			this._gate.Release();
		}
	}

	private TranslationData LoadTranslations()
	{
		return !File.Exists(this._store.Path)
			? new TranslationData(
				new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase),
				[],
				new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase))
			: TranslationData.Load(this._store.Path);
	}

	private string ResolveRepoRoot() 
		=> Path.GetFullPath(this._configuration.RepoRoot);

	private string ResolveSourceDirectory()
	{
		var src = this._configuration.SourceDirectory;
		if (!Path.IsPathRooted(src))
			src = Path.Combine(this.ResolveRepoRoot(), src);
		return Path.GetFullPath(src);
	}

	private string ResolveAuditOutputPath()
	{
		var output = this._configuration.AuditOutputPath;
		if (!Path.IsPathRooted(output))
			output = Path.Combine(this.ResolveRepoRoot(), output);
		return Path.GetFullPath(output);
	}

	private void WriteReport(AuditReport report)
	{
		var outputPath = this.ResolveAuditOutputPath();
		var directory = Path.GetDirectoryName(outputPath);
		if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			Directory.CreateDirectory(directory);

		var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		});
		File.WriteAllText(outputPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
	}
}
