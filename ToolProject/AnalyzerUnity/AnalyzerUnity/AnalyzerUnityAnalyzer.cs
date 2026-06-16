using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using static AnalyzerCallBase;
using static AnalyzerResetProperty;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AnalyzerUnityAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(CallBaseRule, ResetPropertyRule, MissingResetPropertyRule);
	public override void Initialize(AnalysisContext analysisContext)
	{
		analysisContext.EnableConcurrentExecution();
		// 检测是否有调基类函数
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "init"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "lateInit"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "update"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "fixedUpdate"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "lateUpdate"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "exit"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "reset"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "destroy"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "recycle"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "resetProperty"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "onGameState"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "onHide"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "assignWindow"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "assignWindowInternal"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "canEnter"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "enter"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "leave"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "setActive"); }, SyntaxKind.MethodDeclaration);
		analysisContext.RegisterSyntaxNodeAction((context) => { analyzeCallBaseVirtual(context, "initComponents"); }, SyntaxKind.MethodDeclaration);
        // 检测resetProperty中是否完全重置成员变量
        analysisContext.RegisterSyntaxNodeAction(analyzeResetAllFields, SyntaxKind.MethodDeclaration);
        // 检测 ClassObject 子类有没有缺失 resetProperty
        analysisContext.RegisterSyntaxNodeAction(analyzeMissingResetProperty, SyntaxKind.ClassDeclaration);
    }
}