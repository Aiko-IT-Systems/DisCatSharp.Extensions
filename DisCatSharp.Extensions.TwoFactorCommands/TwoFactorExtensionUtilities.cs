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
using System.Linq;
using System.Text;
using System.Web;

using DisCatSharp.Entities;

using QRCoder;

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

	/// <summary>
	///     Converts the specified payload into a QR code represented as block text.
	/// </summary>
	/// <param name="payload">The payload to encode.</param>
	/// <param name="quietZoneModules">Number of quiet-zone modules.</param>
	/// <returns>A string representing the QR code.</returns>
	public static string ToBlockText(this string payload, int quietZoneModules = 4)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);

        var modules = data.ModuleMatrix;
        var size = modules.Count;

        var total = size + (quietZoneModules * 2);

        bool Get(int x, int y)
        {
            x -= quietZoneModules;
            y -= quietZoneModules;

            return x >= 0 && x < size &&
                   y >= 0 && y < size &&
                   modules[y][x];
        }

        var sb = new StringBuilder();

        for (var y = 0; y < total; y += 2)
        {
            for (var x = 0; x < total; x++)
            {
                var top = Get(x, y);
                var bottom = Get(x, y + 1);

                sb.Append((top, bottom) switch
                {
                    (true,  true)  => '█',
                    (true,  false) => '▀',
                    (false, true)  => '▄',
                    _              => ' '
                });
            }

            sb.AppendLine();
        }

        return TrimHorizontalWhitespace(sb.ToString());
    }

	/// <summary>
	///		Trims horizontal whitespace from each line of the input string, preserving a specified minimum padding on both sides.
	/// </summary>
	/// <param name="input">The input string containing one or more lines to be trimmed of horizontal whitespace.</param>
	/// <param name="minPadding">The minimum number of characters to retain as padding on both the left and right sides of each trimmed line. Must
	/// be zero or greater.</param>
	/// <returns>A string in which each line has been trimmed of leading and trailing horizontal whitespace, with the specified
	/// minimum padding preserved. Returns the original input if no lines are present.</returns>
	private static string TrimHorizontalWhitespace(string input, int minPadding = 2)
	{
		var lines = input.Replace("\r", "").Split('\n');
		if (lines.Length == 0)
			return input;

		var width = lines.Max(l => l.Length);

		bool IsEmptyColumn(int col)
		{
			foreach (var line in lines)
			{
				if (col < line.Length && line[col] != ' ')
					return false;
			}
			return true;
		}

		var left = 0;
		while (left < width && IsEmptyColumn(left)) left++;

		var right = width - 1;
		while (right >= 0 && IsEmptyColumn(right)) right--;

		left = Math.Max(0, left - minPadding);
		right = Math.Min(width - 1, right + minPadding);

		var trimmed = lines.Select(line =>
		{
			if (line.Length <= left) return "";
			var len = Math.Min(right - left + 1, line.Length - left);
			return line.Substring(left, len).TrimEnd();
		});

		return string.Join("\n", trimmed);
	}
}
