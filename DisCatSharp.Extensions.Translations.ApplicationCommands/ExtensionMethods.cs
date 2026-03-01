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

using DisCatSharp.ApplicationCommands.Context;

namespace DisCatSharp.Extensions.Translations.ApplicationCommands;

/// <summary>
///     Defines various extensions specific to Translator.
/// </summary>
public static class ExtensionMethods
{
	/// <summary>
	/// Translates a key using the locale from the <see cref="InteractionContext"/>.
	/// </summary>
	/// <param name="ctx">The interaction context.</param>
	/// <param name="key">The translation key.</param>
	/// <param name="placeholders">The placeholders to replace in the translation.</param>
	/// <param name="forceGuildLocale">Whether to force the guild locale.</param>
	/// <returns>The translated string.</returns>
	public static string T(this InteractionContext ctx, string key, object? placeholders = null, bool forceGuildLocale = false) 
		=> ctx.Interaction.T(key, placeholders);

	/// <summary>
	/// Translates a key using the locale from the <see cref="ContextMenuContext"/>.
	/// </summary>
	/// <param name="ctx">The context menu context.</param>
	/// <param name="key">The translation key.</param>
	/// <param name="placeholders">The placeholders to replace in the translation.</param>
	/// <param name="forceGuildLocale">Whether to force the guild locale.</param>
	/// <returns>The translated string.</returns>
	public static string T(this ContextMenuContext ctx, string key, object? placeholders = null, bool forceGuildLocale = false) 
		=> ctx.Interaction.T(key, placeholders);

	/// <summary>
	/// Translates a key using the locale from the <see cref="AutocompleteContext"/>.
	/// </summary>
	/// <param name="ctx">The autocomplete context.</param>
	/// <param name="key">The translation key.</param>
	/// <param name="placeholders">The placeholders to replace in the translation.</param>
	/// <param name="forceGuildLocale">Whether to force the guild locale.</param>
	/// <returns>The translated string.</returns>
	public static string T(this AutocompleteContext ctx, string key, object? placeholders = null, bool forceGuildLocale = false) 
		=> ctx.Interaction.T(key, placeholders);
}
