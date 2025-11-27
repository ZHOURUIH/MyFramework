using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Linq;

public class AnalyzerResetProperty
{
	public static readonly DiagnosticDescriptor ResetPropertyRule = new DiagnosticDescriptor(
		"RESET001",
		"Field not reset",
		"{0}",
		"Usage",
		DiagnosticSeverity.Error,
		true);
	public static void analyzeResetAllFields(SyntaxNodeAnalysisContext context)
	{
		var method = (MethodDeclarationSyntax)context.Node;
		if (method.Identifier.Text != "resetProperty")
		{
			return;
		}
		IMethodSymbol methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
		if (methodSymbol == null)
		{
			return;
		}
		INamedTypeSymbol classSymbol = methodSymbol.ContainingType;
		if (classSymbol == null)
		{
			return;
		}
		// 排除继承 myUGUIObject 的类
		if (inheritsFrom(classSymbol, "myUGUIObject"))
		{
			return;
		}

		// 获取类中全部实例字段 (非 static)
		List<IFieldSymbol> allFields = classSymbol
										.GetMembers()
										.OfType<IFieldSymbol>()
										.Where(f => !f.IsStatic)
										.ToList();

		// 获取 resetProperty 的代码体
		BlockSyntax body = method.Body;
		if (body == null)
		{
			return;
		}
		string bodyText = body.ToString();
		foreach (IFieldSymbol field in allFields)
		{
			string name = field.Name;
			// 检测是否重置
			if (!bodyText.Contains(name + " = ") && 
				!bodyText.Contains(name + ".") && 
				!bodyText.Contains(name + "?.") &&
				!bodyText.Contains(name + ")") &&
				!bodyText.Contains(name + ", "))
			{
				// 报告遗漏字段
				context.ReportDiagnostic(Diagnostic.Create(
					ResetPropertyRule,
					method.Identifier.GetLocation(),
					$"Field '{name}' is not reset in resetProperty()"
				));
			}
		}
	}
	private static bool inheritsFrom(INamedTypeSymbol symbol, string baseTypeName)
	{
		if (symbol.Name == baseTypeName)
		{
			return true;
		}
		INamedTypeSymbol baseType = symbol.BaseType;
		while (baseType != null)
		{
			if (inheritsFrom(baseType, baseTypeName))
			{
				return true;
			}
			baseType = baseType.BaseType;
		}
		return false;
	}
}