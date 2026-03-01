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

using Microsoft.CodeAnalysis.CSharp;

namespace DisCatSharp.Extensions.Translations.Manager;

internal static class Scanner
{
	public static ScanResult ScanSource(string sourceDir, string repoRoot)
	{
		var usedKeys = new HashSet<string>(StringComparer.Ordinal);
		var dynamicUsages = new List<DynamicUsage>();
		var usages = new List<UsageLocation>();
		if (!Directory.Exists(sourceDir))
			return new ScanResult(usedKeys, dynamicUsages, usages, 0);

		var options = new EnumerationOptions
		{
			RecurseSubdirectories = true,
			IgnoreInaccessible = true,
			MatchCasing = MatchCasing.CaseInsensitive
		};
		var files = Directory.EnumerateFiles(sourceDir, "*.cs", options);
		var processed = 0;

		foreach (var file in files)
		{
			if (IsIgnored(file))
				continue;

			processed++;
			// Skip the translator definition file to avoid self-references showing as dynamic keys
			if (string.Equals(Path.GetFileName(file), "TranslationEngine.cs", StringComparison.OrdinalIgnoreCase))
				continue;
			
			if (string.Equals(Path.GetFileName(file), "ExtensionMethods.cs", StringComparison.OrdinalIgnoreCase))
				continue;

			var text = File.ReadAllText(file);
			var tree = CSharpSyntaxTree.ParseText(text, new CSharpParseOptions(LanguageVersion.Preview));
			var walker = new TranslationUsageWalker(file, repoRoot, usedKeys, dynamicUsages, usages);
			walker.Visit(tree.GetRoot());
		}

		return new ScanResult(usedKeys, dynamicUsages, usages, processed);
	}

	private static bool IsIgnored(string filePath)
	{
		static bool ContainsSegment(string path, string segment) => path.Contains($"{Path.DirectorySeparatorChar}{segment}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);

		return ContainsSegment(filePath, "bin")
			|| ContainsSegment(filePath, "obj")
			|| ContainsSegment(filePath, ".git")
			|| ContainsSegment(filePath, "node_modules");
	}
}
