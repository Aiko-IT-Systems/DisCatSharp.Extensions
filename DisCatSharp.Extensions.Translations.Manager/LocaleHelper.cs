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

using System.Reflection;

using DisCatSharp.Entities;

using Microsoft.CodeAnalysis;

namespace DisCatSharp.Extensions.Translations.Manager;

internal static class LocaleHelper
{
	internal static string[] GetValidLocales()
	{
		var values = new List<string>();

		foreach (var field in typeof(DiscordLocales).GetFields(BindingFlags.Public | BindingFlags.Static))
		{
			var value = ToLocaleString(field.GetValue(null));
			if (!string.IsNullOrWhiteSpace(value))
				values.Add(value);
		}

		foreach (var prop in typeof(DiscordLocales).GetProperties(BindingFlags.Public | BindingFlags.Static))
		{
			if (!prop.CanRead)
				continue;
			var value = ToLocaleString(prop.GetValue(null));
			if (!string.IsNullOrWhiteSpace(value))
				values.Add(value);
		}

		values.AddRange(s_fallbackLocales);

		return values
			.Where(v => !string.IsNullOrWhiteSpace(v))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.OrderBy(v => v, Comparer<string>.Create((l, r) => PrimaryFirstThenAlpha(l, r, "en-US")))
			.ToArray();
	}

	private static string? ToLocaleString(object? value) => value switch
	{
		null => null,
		string s => s,
		_ => value.ToString()
	};

	private static readonly string[] s_fallbackLocales =
	[
		"da", "de", "en-GB", "en-US", "es-ES", "fr", "hr", "it", "lt", "hu", "nl", "no",
		"pl", "pt-BR", "ro", "fi", "sv-SE", "vi", "tr", "cs", "el", "bg", "ru", "uk",
		"hi", "th", "zh-CN", "ja", "zh-TW", "ko"
	];

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

	internal static bool ValidateLocales(Dictionary<string, string>? translations, string[] allowed, out string? error)
	{
		if (translations is null)
		{
			error = null;
			return true;
		}

		if (!translations.ContainsKey("en-US"))
		{
			error = "Primary locale en-US is required.";
			return false;
		}

		var set = new HashSet<string>(allowed, StringComparer.OrdinalIgnoreCase);
		foreach (var locale in translations.Keys)
		{
			if (!set.Contains(locale))
			{
				error = $"Invalid locale: {locale}";
				return false;
			}
		}

		// optional deeper validation via DiscordApplicationCommandLocalization
		var localization = new DiscordApplicationCommandLocalization();
		foreach (var locale in translations.Keys)
		{
			if (!localization.Validate(locale))
			{
				error = $"Invalid locale: {locale}";
				return false;
			}
		}

		error = null;
		return true;
	}
}
