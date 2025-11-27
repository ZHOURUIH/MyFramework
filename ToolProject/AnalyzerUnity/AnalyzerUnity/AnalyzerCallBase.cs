using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;

public class AnalyzerCallBase
{
	public static readonly DiagnosticDescriptor CallBaseRule = new DiagnosticDescriptor(
		"BASE001",
		"Missing base call",
		"Method '{0}' overrides '{1}' but does not call base.{1}()",
		"Usage",
		DiagnosticSeverity.Error,
		true);
	public static void analyzeCallBaseVirtual(SyntaxNodeAnalysisContext context, string methodName)
	{
		var method = (MethodDeclarationSyntax)context.Node;
		IMethodSymbol symbol = context.SemanticModel.GetDeclaredSymbol(method);
		if (symbol?.IsOverride != true || symbol.Name != methodName || symbol.OverriddenMethod?.IsAbstract == true)
		{
			return;
		}
		if (method.Body == null || !method.Body.DescendantNodes().OfType<InvocationExpressionSyntax>().Any(i => i.ToString().Contains("base." + methodName + "(")))
		{
			context.ReportDiagnostic(Diagnostic.Create(CallBaseRule, method.Identifier.GetLocation(), symbol.Name, symbol.OverriddenMethod?.Name));
		}
	}
}