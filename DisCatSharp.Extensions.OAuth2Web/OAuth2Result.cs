// This file is part of the DisCatSharp project, based off DSharpPlus.
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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace DisCatSharp.Extensions.OAuth2Web;

/// <summary>
///     OAuth2 result
/// </summary>
/// <typeparam name="T">Type of result</typeparam>
public readonly struct OAuth2Result<T>
{
	/// <summary>
	///     Whether interactivity was timed out
	/// </summary>
	public bool TimedOut { get; }

	/// <summary>
	///     Result
	/// </summary>
	public T Result { get; }

	/// <summary>
	///     Initializes a new instance of the <see cref="OAuth2Result{T}" /> class.
	/// </summary>
	/// <param name="timedOut">If true, timed out.</param>
	/// <param name="result">The result.</param>
	internal OAuth2Result(bool timedOut, T result)
	{
		this.TimedOut = timedOut;
		this.Result = result;
	}
}
