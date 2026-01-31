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

using System;
using System.IO;
using System.Web;

using DisCatSharp.Entities;

using TwoFactorAuthNet;

namespace DisCatSharp.Extensions.TwoFactorCommands;

public static class TwoFactorExtensionUtilities
{
	/// <summary>
	///     Checks two factor registration for the given user id.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="user">The user id to check.</param>
	/// <returns>Whether the user is enrolled.</returns>
	public static bool CheckTwoFactorEnrollmentFor(this DiscordClient client, ulong user)
		=> client.GetTwoFactor()?.IsEnrolled(user) ?? false;

	/// <summary>
	///     Removes the two factor registration for the given user id.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="user">The user id to check.</param>
	public static void DisenrollTwoFactor(this DiscordClient client, ulong user)
		=> client.GetTwoFactor()?.DisenrollUser(user);

	/// <summary>
	///     Registers two factor for the given user.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="user">The user to check.</param>
	/// <returns>
	///     A <see cref="System.Tuple{T1, T2}" /> where <c>Secret</c> is a <see cref="string">string</see> with the
	///     secret itself and <c>QrCode</c> a <see cref="System.IO.MemoryStream">MemoryStream</see> with the qr code image.
	/// </returns>
	public static (string Secret, MemoryStream QrCode) EnrollTwoFactor(this DiscordClient client, DiscordUser user)
	{
		var ext = client.GetTwoFactor() ?? throw new InvalidOperationException("Two factor extension is not registered on this client.");
		var secret = ext.TwoFactorClient.CreateSecret(160, CryptoSecureRequirement.RequireSecure);
		ext.EnrollUser(user.Id, secret);
		var label = $"{ext.Configuration.ResponseConfiguration.AuthenticatorAccountPrefix}: {HttpUtility.UrlEncode(user.UsernameWithDiscriminator)}";
		var text = $"otpauth://totp/{label}?secret={secret}&issuer={ext.TwoFactorClient.Issuer}";
		var image = ext.TwoFactorClient.QrCodeProvider.GetQrCodeImage(text, 512);
		MemoryStream ms = new(image)
		{
			Position = 0
		};
		return (secret, ms);
	}
}
