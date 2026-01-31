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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DisCatSharp.Extensions.Translations.Manager;

internal sealed class TranslationUsageWalker(string filePath, string repoRoot, HashSet<string> keys, List<DynamicUsage> dynamicUsages, List<UsageLocation> usages) : CSharpSyntaxWalker
{
	private readonly string _filePath = filePath;
	private readonly string _repoRoot = repoRoot;
	private readonly HashSet<string> _keys = keys;
	private readonly List<DynamicUsage> _dynamicUsages = dynamicUsages;
	private readonly List<UsageLocation> _usages = usages;

	public override void VisitInvocationExpression(InvocationExpressionSyntax node)
	{
		var methodName = GetMethodName(node.Expression);
		if (methodName is "T" or "TLocale")
			this.HandleTranslationCall(methodName, node);

		base.VisitInvocationExpression(node);
	}

	private void HandleTranslationCall(string methodName, InvocationExpressionSyntax node)
	{
		var args = node.ArgumentList?.Arguments;
		if (args is null || args.Value.Count is 0)
		{
			this.RecordDynamic(node, methodName, "no arguments", null);
			return;
		}

		ExpressionSyntax? keyExpr = null;
		if (string.Equals(methodName, "TLocale", StringComparison.Ordinal))
		{
			var member = node.Expression as MemberAccessExpressionSyntax;
			var receiverIsTranslator = member?.Expression is IdentifierNameSyntax id && string.Equals(id.Identifier.ValueText, "Translator", StringComparison.Ordinal);
			var index = receiverIsTranslator && args.Value.Count > 1 ? 1 : 0;
			if (index < args.Value.Count)
				keyExpr = args.Value[index].Expression;
		}
		else
		{
			keyExpr = args.Value[0].Expression;
		}

		if (keyExpr is null)
		{
			this.RecordDynamic(node, methodName, "dynamic key", null);
			return;
		}

		if (TryEvaluateString(keyExpr, out var key))
		{
			this._keys.Add(key);
			this.RecordUsage(node, methodName, key);
			return;
		}

		this.RecordDynamic(node, methodName, "dynamic key", keyExpr.ToString());
	}

	private static bool TryEvaluateString(ExpressionSyntax expr, out string value)
	{
		switch (expr)
		{
			case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression):
				value = literal.Token.ValueText;
				return true;
			case InterpolatedStringExpressionSyntax interpolated:
				if (interpolated.Contents.All(c => c is InterpolatedStringTextSyntax))
				{
					var sb = new StringBuilder();
					foreach (var part in interpolated.Contents.OfType<InterpolatedStringTextSyntax>())
						sb.Append(part.TextToken.ValueText);
					value = sb.ToString();
					return true;
				}
				break;
			case ParenthesizedExpressionSyntax paren:
				return TryEvaluateString(paren.Expression, out value);
			case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AddExpression):
				if (TryEvaluateString(binary.Left, out var left) && TryEvaluateString(binary.Right, out var right))
				{
					value = left + right;
					return true;
				}
				break;
		}

		value = string.Empty;
		return false;
	}

	private static string? GetMethodName(ExpressionSyntax expression) => expression switch
	{
		IdentifierNameSyntax id => id.Identifier.ValueText,
		MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
		_ => null
	};

	private void RecordDynamic(SyntaxNode node, string methodName, string reason, string? expression)
	{
		var span = node.GetLocation().GetLineSpan();
		var line = span.StartLinePosition.Line + 1;
		var relative = Path.GetRelativePath(this._repoRoot, this._filePath);
		this._dynamicUsages.Add(new DynamicUsage(relative, line, methodName, reason, expression ?? string.Empty));
	}

	private void RecordUsage(SyntaxNode node, string methodName, string key)
	{
		var span = node.GetLocation().GetLineSpan();
		var line = span.StartLinePosition.Line + 1;
		var relative = Path.GetRelativePath(this._repoRoot, this._filePath);
		this._usages.Add(new UsageLocation(key, relative, line, methodName));
	}
}
