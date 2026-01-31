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

using System.Text.RegularExpressions;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System;
using System.Collections.Concurrent;

namespace DisCatSharp.Extensions.Translations;

/// <summary>
///    Represents a translation engine.
/// </summary>
/// <param name="configuration">The translations configuration.</param>
public partial class TranslationEngine(TranslationsConfiguration configuration)
{
	/// <summary>
	/// Guard for initialization.
	/// </summary>
	private readonly Lock _initLock = new();

	/// <summary>
	/// Whether the translations have been initialized.
	/// </summary>
	private volatile bool _initialized;

	/// <summary>
	/// Provides a mapping of translation keys to localized string values for multiple languages.
	/// </summary>
	/// <remarks>Each entry in the outer dictionary represents a language, identified by its language code (such as
	/// "en" or "fr"). The inner dictionary maps translation keys to their corresponding localized strings for that
	/// language.</remarks>
    internal ConcurrentDictionary<string, Dictionary<string, string>> Translations { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Gets the default locale.
	/// </summary>
	internal string DefaultLocale { get; } = configuration.DefaultLocale ?? "en-US";

	/// <summary>
	/// Gets or sets the path to the translations file.
	/// </summary>
	internal string? PATH { get; set; }

	/// <summary>
	/// Gets the configuration.
	/// </summary>
	internal readonly TranslationsConfiguration Configuration = configuration;

	/// <summary>
	/// Ensures that the translations are loaded from the file.
	/// </summary>
	private void EnsureLoaded()
    {
        if (this._initialized)
            return;
        lock (this._initLock)
        {
            if (this._initialized)
                return;
			if (this.PATH is null)
				return;
			try
            {
                if (!File.Exists(this.PATH))
                {
					this.Translations = new(StringComparer.OrdinalIgnoreCase);
					this._initialized = true;
                    return;
                }

                var json = File.ReadAllText(this.PATH);
                var doc = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json) ?? [];
				this.Translations = new(doc, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
				this.Translations = new(StringComparer.OrdinalIgnoreCase);
            }
            finally
            {
				this._initialized = true;
            }
        }
    }

	/// <summary>
	/// Translates a key using the specified locale.
	/// </summary>
	/// <param name="locale">The locale to use for translation.</param>
	/// <param name="key">The translation key.</param>
	/// <param name="placeholders">The placeholders to replace in the translation.</param>
	/// <returns>The translated string.</returns>
	public string TLocale(string? locale, string key, object? placeholders = null)
    {
        this.EnsureLoaded();
        locale ??= "en-US";
		return !this.Translations.TryGetValue(key, out var langs)
			? this.ApplyPlaceholders(key, placeholders)
			: langs.TryGetValue(locale, out var text)
			? this.ApplyPlaceholders(text, placeholders)
			: langs.TryGetValue("en-US", out var fallback)
			? this.ApplyPlaceholders(fallback, placeholders)
			: this.ApplyPlaceholders(key, placeholders);
	}

	/// <summary>
	/// Applies placeholders to the template string.
	/// </summary>
	/// <param name="template">The template string.</param>
	/// <param name="placeholders">The placeholders to replace in the template.</param>
	/// <returns>The template string with placeholders replaced.</returns>
	private string ApplyPlaceholders(string template, object? placeholders)
    {
        if (placeholders is null)
            return template;

		if (placeholders is not Dictionary<string, object?> values)
		{
			values = new(StringComparer.OrdinalIgnoreCase);
			var props = placeholders.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			foreach (var p in props)
			{
				try
				{
					values[p.Name] = p.GetValue(placeholders);
				}
				catch
				{
					values[p.Name] = null;
				}
			}
		}

		var result = this.TemplateRegex().Replace(template, m =>
        {
            var name = m.Groups[1].Value;
			return values.TryGetValue(name, out var val) && val is not null ? val.ToString()! : string.Empty;
		});

        return result;
    }

	/// <summary>
	/// Reloads the translations from the file.
	/// </summary>
	public void Reload()
    {
        lock (this._initLock)
        {
			this._initialized = false;
            this.EnsureLoaded();
        }
    }

    /// <summary>
    /// Allows overriding the file path used for translations and loads from it immediately.
    /// </summary>
    public void UseFile(string filePath)
    {
        lock (this._initLock)
        {
			this.PATH = filePath;
			this._initialized = false;
            this.EnsureLoaded();
        }
    }

	/// <summary>
	/// Template regex for matching placeholders.
	/// </summary>
	[GeneratedRegex(@"\{\s*([a-zA-Z0-9_]+)\s*\}", RegexOptions.Compiled)]
	private partial Regex TemplateRegex();

	/// <summary>
	/// Initializes the translations engine.
	/// </summary>
	internal void Initialize()
	{
		if (!string.IsNullOrWhiteSpace(this.Configuration.TranslationsFolder) && !string.IsNullOrWhiteSpace(this.Configuration.TranslationsFileName))
		{
			var path = Path.Combine(this.Configuration.TranslationsFolder, this.Configuration.TranslationsFileName);
			if (File.Exists(path))
				this.UseFile(path);
			else
				throw new FileNotFoundException("Translations file not found at the specified path.", path);
		}
	}
}
