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

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DisCatSharp.Extensions.Translations.Manager;

internal sealed record TranslationData(
	Dictionary<string, Dictionary<string, string>> Map,
	IReadOnlyList<string> DuplicateKeys,
	IReadOnlyDictionary<string, int> KeyCounts)
{
	public static TranslationData Load(string path)
	{
		var text = File.ReadAllText(path);
		var bytes = Encoding.UTF8.GetBytes(text);
		var duplicateKeys = new List<string>();
		var keyCounts = new Dictionary<string, int>(StringComparer.Ordinal);

		var reader = new Utf8JsonReader(bytes, new JsonReaderOptions
		{
			AllowTrailingCommas = true,
			CommentHandling = JsonCommentHandling.Skip
		});

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.PropertyName && reader.CurrentDepth == 1)
			{
				var key = reader.GetString() ?? string.Empty;
				if (keyCounts.TryGetValue(key, out var count))
				{
					keyCounts[key] = count + 1;
					if (count == 1)
						duplicateKeys.Add(key);
				}
				else
				{
					keyCounts[key] = 1;
				}
			}
		}

		var map = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(text, new JsonSerializerOptions
		{
			AllowTrailingCommas = true,
			ReadCommentHandling = JsonCommentHandling.Skip
		}) ?? new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

		return new TranslationData(new Dictionary<string, Dictionary<string, string>>(map, StringComparer.OrdinalIgnoreCase), duplicateKeys, keyCounts);
	}

	public static void Save(string path, Dictionary<string, Dictionary<string, string>> map, bool sort = false, string? primaryLocale = null)
	{
		IEnumerable<KeyValuePair<string, Dictionary<string, string>>> outer = map;
		if (sort)
			outer = outer.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase);

		var ordered = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
		foreach (var pair in outer)
		{
			IEnumerable<KeyValuePair<string, string>> inner = pair.Value;
			if (primaryLocale is not null)
				inner = inner.OrderBy(p => p.Key, Comparer<string>.Create((l, r) => PrimaryFirstThenAlpha(l, r, primaryLocale)));
			else if (sort)
				inner = inner.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase);

			ordered[pair.Key] = inner.ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);
		}

		var json = JsonSerializer.Serialize(ordered, new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			DefaultIgnoreCondition = JsonIgnoreCondition.Never
		});

		File.WriteAllText(path, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
	}

	private static int PrimaryFirstThenAlpha(string? left, string? right, string primary = "en-US")
	{
		var l = left ?? string.Empty;
		var r = right ?? string.Empty;
		return string.Equals(l, r, StringComparison.OrdinalIgnoreCase)
			? 0
			: string.Equals(l, primary, StringComparison.OrdinalIgnoreCase)
			? -1
			: string.Equals(r, primary, StringComparison.OrdinalIgnoreCase) ? 1 : string.Compare(l, r, StringComparison.OrdinalIgnoreCase);
	}
}
