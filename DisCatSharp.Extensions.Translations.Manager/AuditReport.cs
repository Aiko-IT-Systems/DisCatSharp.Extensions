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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DisCatSharp.Extensions.Translations.Manager;

internal sealed record AuditReport
{
	public required DateTimeOffset GeneratedAt { get; init; }
	public required string RepoRoot { get; init; }
	public required string SourceDirectory { get; init; }
	public required string StringsPath { get; init; }
	public required int UsedKeysCount { get; init; }
	public required int DefinedKeysCount { get; init; }
	public required List<string> MissingKeys { get; init; }
	public required List<string> UnusedKeys { get; init; }
	public required List<string> DuplicateKeys { get; init; }
	public required Dictionary<string, List<string>> DuplicateEnUsValues { get; init; }
	public required Dictionary<string, int> LocaleMissingCounts { get; init; }
	public required List<DynamicUsage> DynamicKeyUsages { get; init; }
	public required Dictionary<string, List<UsageLocation>> KeyUsages { get; init; }

	public static AuditReport Build(TranslationData translations, ScanResult scan, TranslationsManagerConfiguration configuration, string stringsPath)
	{
		var repoRoot = Path.GetFullPath(configuration.RepoRoot);
		var sourceDirectory = Path.GetFullPath(configuration.SourceDirectory);
		var resolvedStringsPath = Path.GetFullPath(stringsPath);
		var definedKeys = translations.Map.Keys.ToHashSet(StringComparer.Ordinal);
		var missing = scan.Keys.Where(k => !definedKeys.Contains(k)).OrderBy(k => k).ToList();
		var dynamicPrefixes = BuildDynamicPrefixes(scan.DynamicUsages);
		var unused = definedKeys
			.Where(k => !scan.Keys.Contains(k) && !IsCoveredByDynamic(k, dynamicPrefixes))
			.OrderBy(k => k)
			.ToList();

		var usageLookup = scan.Usages
			.GroupBy(u => u.Key, StringComparer.OrdinalIgnoreCase)
			.ToDictionary(g => g.Key, g => g.OrderBy(u => u.File).ThenBy(u => u.Line).ToList(), StringComparer.OrdinalIgnoreCase);

		var enPairs = translations.Map
			.Select(kv => kv.Value.TryGetValue("en-US", out var enValue) ? (kv.Key, Value: enValue) : (Key: null, Value: null))
			.Where(x => x.Key is not null && !string.IsNullOrWhiteSpace(x.Value))
			.Select(x => (Key: x.Key!, Value: x.Value!));

		var enDuplicates = enPairs
			.GroupBy(x => x.Value)
			.Where(g => g.Count() > 1)
			.ToDictionary(g => g.Key, g => g.Select(item => item.Key).OrderBy(x => x).ToList(), StringComparer.Ordinal);

		var localeMissing = BuildLocaleMissing(translations.Map);

		return new AuditReport
		{
			GeneratedAt = DateTimeOffset.UtcNow,
			RepoRoot = repoRoot,
			SourceDirectory = sourceDirectory,
			StringsPath = resolvedStringsPath,
			UsedKeysCount = scan.Keys.Count,
			DefinedKeysCount = translations.Map.Count,
			MissingKeys = missing,
			UnusedKeys = unused,
			DuplicateKeys = [.. translations.DuplicateKeys],
			DuplicateEnUsValues = enDuplicates,
			LocaleMissingCounts = localeMissing,
			DynamicKeyUsages = [.. scan.DynamicUsages],
			KeyUsages = usageLookup
		};
	}

	private static Dictionary<string, int> BuildLocaleMissing(Dictionary<string, Dictionary<string, string>> map)
	{
		var locales = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var value in map.Values)
		{
			foreach (var locale in value.Keys)
				locales.Add(locale);
		}

		var missing = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		foreach (var locale in locales)
		{
			var count = map.Count(kv => !kv.Value.TryGetValue(locale, out var text) || string.IsNullOrWhiteSpace(text));
			missing[locale] = count;
		}

		return missing;
	}

	private static IReadOnlyList<string> BuildDynamicPrefixes(IReadOnlyList<DynamicUsage> dynamicUsages)
	{
		var prefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var dyn in dynamicUsages)
		{
			var prefix = ExtractPrefix(dyn.Expression);
			if (!string.IsNullOrWhiteSpace(prefix))
				prefixes.Add(prefix);
		}

		return prefixes.ToList();
	}

	private static bool IsCoveredByDynamic(string key, IReadOnlyList<string> prefixes)
	{
		foreach (var prefix in prefixes)
		{
			if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
				return true;
		}
		return false;
	}

	private static string ExtractPrefix(string expression)
	{
		if (string.IsNullOrWhiteSpace(expression))
			return string.Empty;

		// Trim leading interpolation markers
		var expr = expression.Trim();
		if (expr.StartsWith("$") && expr.Length > 1)
			expr = expr.Substring(1);
		if (expr.StartsWith("@$") && expr.Length > 2)
			expr = expr.Substring(2);
		expr = expr.TrimStart('"');

		var brace = expr.IndexOf('{');
		if (brace >= 0)
			expr = expr.Substring(0, brace);

		// Strip trailing quote if still present
		expr = expr.TrimEnd('"');

		// Keep only prefixes that look like translation key prefixes (must contain a dot)
		return expr.Contains('.') ? expr : string.Empty;
	}
}