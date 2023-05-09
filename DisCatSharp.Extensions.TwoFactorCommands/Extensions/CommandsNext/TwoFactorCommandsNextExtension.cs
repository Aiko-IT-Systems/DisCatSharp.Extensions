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

using System.Threading.Tasks;

using DisCatSharp.Attributes;
using DisCatSharp.CommandsNext;
using DisCatSharp.Extensions.TwoFactorCommands.Enums;
using DisCatSharp.Extensions.TwoFactorCommands.Properties;

namespace DisCatSharp.Extensions.TwoFactorCommands.CommandsNext;

public static class TwoFactorCommandsNextExtension
{
	// TODO: Implement
	/// <summary>
	/// <para>Asks the user via private for the two factor code.</para>
	/// <para>This uses DisCatSharp.Interactivity.</para>
	/// <para>To be used for commands next.</para>
	/// <para><note type="caution">Not implemented yet. Returns <see cref="TwoFactorResult.NotImplemented"/>.</note></para>
	/// </summary>
	/// <param name="ctx">The command context.</param>
	/// <returns>A <see cref="TwoFactorResult"/>.</returns>
	[Experimental("No support for this yet. Not implemented")]
	public static async Task<TwoFactorResponse> RequestTwoFactorAsync(CommandContext ctx)
		=> await Task.FromResult(new TwoFactorResponse() { Result = TwoFactorResult.NotImplemented });
}
