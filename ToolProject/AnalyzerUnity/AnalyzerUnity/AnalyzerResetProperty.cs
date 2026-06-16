using Microsoft.CodeAnalysis;
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

    public static readonly DiagnosticDescriptor MissingResetPropertyRule = new DiagnosticDescriptor(
        "RESET002",
        "Missing resetProperty",
        "{0}",
        "Usage",
        DiagnosticSeverity.Error,
        true);

    // 检测已经写了 resetProperty 的类,是否重置了全部字段
    public static void analyzeResetAllFields(SyntaxNodeAnalysisContext context)
    {
        if (!(context.Node is MethodDeclarationSyntax method))
        {
            return;
        }
        if (method.Identifier.Text != "resetProperty")
        {
            return;
        }
        if (!(context.SemanticModel.GetDeclaredSymbol(method) is IMethodSymbol methodSymbol))
        {
            return;
        }
        INamedTypeSymbol classSymbol = methodSymbol.ContainingType;
        if (classSymbol == null)
        {
            return;
        }
        List<IFieldSymbol> allFields = getOwnInstanceFields(classSymbol);
        if (allFields.Count == 0)
        {
            return;
        }
        BlockSyntax body = method.Body;
        if (body == null)
        {
            return;
        }

        string bodyText = body.ToString();
        foreach (IFieldSymbol field in allFields)
        {
            string name = field.Name;
            if (!bodyText.Contains(name + " = ") &&
                !bodyText.Contains(name + ".") &&
                !bodyText.Contains(name + "?.") &&
                !bodyText.Contains(name + ")") &&
                !bodyText.Contains(name + ", "))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ResetPropertyRule,
                    method.Identifier.GetLocation(),
                    "Field '" + name + "' is not reset in resetProperty()"
                ));
            }
        }
    }

    // 检测 ClassObject 子类是否缺失 resetProperty
    public static void analyzeMissingResetProperty(SyntaxNodeAnalysisContext context)
    {
        if (!(context.Node is ClassDeclarationSyntax classDeclaration))
        {
            return;
        }
        if (!(context.SemanticModel.GetDeclaredSymbol(classDeclaration) is INamedTypeSymbol classSymbol))
        {
            return;
        }
        // 只检测普通 class
        if (classSymbol.TypeKind != TypeKind.Class)
        {
            return;
        }
        // 只检测 ClassObject 子类
        if (!inheritsFrom(classSymbol, "ClassObject"))
        {
            return;
        }
        // 排除某些特殊的类一般就不需要写resetProperty了,因为不会被复用
        if (inheritsFrom(classSymbol, "myUGUIObject") || 
            inheritsFrom(classSymbol, "FrameSystem") || 
            inheritsFrom(classSymbol, "LayoutScript") ||
            inheritsFrom(classSymbol, "NetPacketBit") ||
            inheritsFrom(classSymbol, "SQLiteTable") ||
            inheritsFrom(classSymbol, "SQLiteData"))
        {
            return;
        }
        // 抽象类不检测
        if (classSymbol.IsAbstract)
        {
            return;
        }

        List<IFieldSymbol> allFields = getOwnInstanceFields(classSymbol);
        if (allFields.Count == 0)
        {
            return;
        }

        // 只判断当前类自己有没有声明 resetProperty
        // 父类继承来的 resetProperty 不算
        bool hasOwnResetProperty = false;
        foreach (IMethodSymbol method in classSymbol.GetMembers("resetProperty").OfType<IMethodSymbol>())
        {
            if (method.Parameters.Length != 0 || 
                !method.ReturnsVoid ||
                !SymbolEqualityComparer.Default.Equals(method.ContainingType, classSymbol))
            {
                continue;
            }
            hasOwnResetProperty = true;
            break;
        }

        if (hasOwnResetProperty)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MissingResetPropertyRule,
            classDeclaration.Identifier.GetLocation(),
            "Class '" + classSymbol.Name + "' inherits from ClassObject and has instance fields, but does not implement resetProperty()"
        ));
    }

    private static List<IFieldSymbol> getOwnInstanceFields(INamedTypeSymbol classSymbol)
    {
        return classSymbol
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(field => !field.IsStatic)
            .Where(field => !field.IsConst)
            .Where(field => SymbolEqualityComparer.Default.Equals(field.ContainingType, classSymbol))
            .ToList();
    }

    private static bool inheritsFrom(INamedTypeSymbol symbol, string baseTypeName)
    {
        INamedTypeSymbol cur = symbol;
        while (cur != null)
        {
            if (cur.Name == baseTypeName)
            {
                return true;
            }
            cur = cur.BaseType;
        }
        return false;
    }
}