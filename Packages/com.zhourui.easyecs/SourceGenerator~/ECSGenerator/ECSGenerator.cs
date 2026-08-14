using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ECSSourceGenerator
{
	[Generator]
	public sealed class ECSGenerator : ISourceGenerator
	{
		private enum Backend
		{
			Unsafe,
			SafeSpan,
			SafeRegistry,
		}
		private static readonly DiagnosticDescriptor mConflictDiagnostic = new DiagnosticDescriptor("ECS001", "ECS标签冲突", "{0}不能同时标记[ECS]和[NotECS]", "ECS", DiagnosticSeverity.Error, true);
		private static readonly DiagnosticDescriptor mUnsupportedTypeDiagnostic = new DiagnosticDescriptor("ECS002", "不支持的ECS类型", "{0}当前不支持生成ECS代码:{1}", "ECS", DiagnosticSeverity.Error, true);
		private static readonly DiagnosticDescriptor mUnsupportedFieldDiagnostic = new DiagnosticDescriptor("ECS003", "不支持的ECS字段", "{0}当前不支持生成ECS字段:{1}", "ECS", DiagnosticSeverity.Error, true);
		private static readonly DiagnosticDescriptor mColumnNameConflictDiagnostic = new DiagnosticDescriptor("ECS004", "ECS列名称冲突", "{0}生成的Column方法名称发生冲突:{1}", "ECS", DiagnosticSeverity.Error, true);
		public void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForSyntaxNotifications(() => new ECSSyntaxReceiver());
		}
		public void Execute(GeneratorExecutionContext context)
		{
			if (!(context.SyntaxReceiver is ECSSyntaxReceiver receiver))
			{
				return;
			}
			CSharpCompilationOptions compilationOptions = context.Compilation.Options as CSharpCompilationOptions;
			bool allowUnsafe = compilationOptions != null && compilationOptions.AllowUnsafe;
			bool hasSpan = context.Compilation.GetTypeByMetadataName("System.Span`1") != null;
			bool forceSafeRegistry = hasPreprocessorSymbol(context.Compilation, "ECS_FORCE_SAFE_REGISTRY");
			bool needLeakTracker = false;
			HashSet<string> generatedTypeSet = new HashSet<string>();
			foreach (StructDeclarationSyntax declaration in receiver.mStructList)
			{
				SemanticModel semanticModel = context.Compilation.GetSemanticModel(declaration.SyntaxTree);
				if (!(semanticModel.GetDeclaredSymbol(declaration) is INamedTypeSymbol structSymbol))
				{
					continue;
				}
				string symbolKey = structSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
				if (!generatedTypeSet.Add(symbolKey))
				{
					continue;
				}
				bool structECS = hasAttribute(structSymbol, "ECSAttribute");
				bool structNotECS = hasAttribute(structSymbol, "NotECSAttribute");
				if (!structECS && !structNotECS)
				{
					continue;
				}
				if (structECS && structNotECS)
				{
					reportDiagnostic(context, mConflictDiagnostic, structSymbol, structSymbol.Name);
					continue;
				}
				if (!validateStruct(context, structSymbol))
				{
					continue;
				}
				List<IFieldSymbol> ecsFields = new List<IFieldSymbol>();
				List<IFieldSymbol> aosFields = new List<IFieldSymbol>();
				if (!collectFields(context, structSymbol, structECS, ecsFields, aosFields))
				{
					continue;
				}
				if (!validateColumnMethodNames(context, structSymbol, ecsFields))
				{
					continue;
				}
				Backend backend;
				string backendReason;
				if (allowUnsafe && structSymbol.IsUnmanagedType)
				{
					backend = Backend.Unsafe;
					backendReason = "AllowUnsafe=true,Unmanaged=true";
					needLeakTracker = true;
				}
				else if (hasSpan && !forceSafeRegistry)
				{
					backend = Backend.SafeSpan;
					backendReason = allowUnsafe ? "ContainsManagedField,Span=true" : "AllowUnsafe=false,Span=true";
				}
				else
				{
					backend = Backend.SafeRegistry;
					backendReason = forceSafeRegistry ? "ECS_FORCE_SAFE_REGISTRY" : "SpanUnavailable";
					needLeakTracker = true;
				}
				string source = generateCode(structSymbol, ecsFields, aosFields, backend, backendReason);
				context.AddSource(getHintName(structSymbol) + ".ECS.g.cs", SourceText.From(source, Encoding.UTF8));
			}
			if (needLeakTracker)
			{
				context.AddSource("__ECSListLeakTracker.g.cs", SourceText.From(generateLeakTrackerSource(), Encoding.UTF8));
			}
		}
		private static bool validateStruct(GeneratorExecutionContext context, INamedTypeSymbol structSymbol)
		{
			if (structSymbol.ContainingType != null)
			{
				reportDiagnostic(context, mUnsupportedTypeDiagnostic, structSymbol, structSymbol.Name, "暂不支持嵌套struct");
				return false;
			}
			if (structSymbol.TypeParameters.Length > 0)
			{
				reportDiagnostic(context, mUnsupportedTypeDiagnostic, structSymbol, structSymbol.Name, "暂不支持泛型struct");
				return false;
			}
			if (structSymbol.IsRefLikeType)
			{
				reportDiagnostic(context, mUnsupportedTypeDiagnostic, structSymbol, structSymbol.Name, "暂不支持ref struct作为ECS数据定义");
				return false;
			}
			foreach (IPropertySymbol property in structSymbol.GetMembers().OfType<IPropertySymbol>())
			{
				if (!property.IsStatic)
				{
					reportDiagnostic(context, mUnsupportedTypeDiagnostic, property, structSymbol.Name, "暂时只支持字段,不支持实例Property:" + property.Name);
					return false;
				}
			}
			return true;
		}
		private static bool collectFields(GeneratorExecutionContext context, INamedTypeSymbol structSymbol, bool defaultECS, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			foreach (IFieldSymbol field in structSymbol.GetMembers().OfType<IFieldSymbol>())
			{
				if (field.IsStatic || field.IsImplicitlyDeclared)
				{
					continue;
				}
				if (field.IsReadOnly)
				{
					reportDiagnostic(context, mUnsupportedFieldDiagnostic, field, structSymbol.Name + "." + field.Name, "readonly字段");
					return false;
				}
				if (field.IsFixedSizeBuffer)
				{
					reportDiagnostic(context, mUnsupportedFieldDiagnostic, field, structSymbol.Name + "." + field.Name, "fixed buffer");
					return false;
				}
				if (field.DeclaredAccessibility != Accessibility.Public && field.DeclaredAccessibility != Accessibility.Internal)
				{
					reportDiagnostic(context, mUnsupportedFieldDiagnostic, field, structSymbol.Name + "." + field.Name, "字段必须是public或internal");
					return false;
				}
				bool fieldECS = hasAttribute(field, "ECSAttribute");
				bool fieldNotECS = hasAttribute(field, "NotECSAttribute");
				if (fieldECS && fieldNotECS)
				{
					reportDiagnostic(context, mConflictDiagnostic, field, structSymbol.Name + "." + field.Name);
					return false;
				}
				bool useECS = fieldECS || (!fieldNotECS && defaultECS);
				if (useECS)
				{
					ecsFields.Add(field);
				}
				else
				{
					aosFields.Add(field);
				}
			}
			return true;
		}
		private static bool validateColumnMethodNames(GeneratorExecutionContext context, INamedTypeSymbol structSymbol, List<IFieldSymbol> ecsFields)
		{
			HashSet<string> methodNameSet = new HashSet<string>();
			foreach (IFieldSymbol field in ecsFields)
			{
				string methodName = getColumnMethodName(field.Name);
				if (!methodNameSet.Add(methodName))
				{
					reportDiagnostic(context, mColumnNameConflictDiagnostic, field, structSymbol.Name, methodName);
					return false;
				}
			}
			return true;
		}
		private static string generateCode(INamedTypeSymbol structSymbol, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, Backend backend, string backendReason)
		{
			string typeName = structSymbol.Name;
			string fullTypeName = getTypeName(structSymbol);
			string namespaceName = structSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : structSymbol.ContainingNamespace.ToDisplayString();
			string accessibility = structSymbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
			StringBuilder builder = new StringBuilder(32768);
			builder.AppendLine("// <auto-generated/>");
			if (!string.IsNullOrEmpty(namespaceName))
			{
				builder.AppendLine("namespace " + namespaceName);
				builder.AppendLine("{");
			}
			if (aosFields.Count > 0)
			{
				generateAoSBlock(builder, typeName, aosFields);
			}
			switch (backend)
			{
				case Backend.Unsafe:
					generateUnsafeStorage(builder, typeName, ecsFields, aosFields);
					generateUnsafeRef(builder, accessibility, typeName, ecsFields, aosFields);
					generateUnsafeList(builder, accessibility, typeName, fullTypeName, ecsFields, aosFields, backendReason);
					break;
				case Backend.SafeSpan:
					generateSafeSpanStorage(builder, typeName, ecsFields, aosFields);
					generateSafeSpanRef(builder, accessibility, typeName, ecsFields, aosFields);
					generateSafeSpanList(builder, accessibility, typeName, fullTypeName, ecsFields, aosFields, backendReason);
					break;
				case Backend.SafeRegistry:
					generateSafeRegistryStorage(builder, typeName, ecsFields, aosFields);
					generateSafeRegistry(builder, typeName, ecsFields, aosFields);
					generateSafeRegistryRef(builder, accessibility, typeName, ecsFields, aosFields);
					generateSafeRegistryList(builder, accessibility, typeName, fullTypeName, ecsFields, aosFields, backendReason);
					break;
			}
			if (!string.IsNullOrEmpty(namespaceName))
			{
				builder.AppendLine("}");
			}
			return builder.ToString();
		}
		private static void generateAoSBlock(StringBuilder builder, string typeName, List<IFieldSymbol> fields)
		{
			builder.AppendLine("internal struct " + typeName + "AoSBlock");
			builder.AppendLine("{");
			foreach (IFieldSymbol field in fields)
			{
				builder.AppendLine("\tpublic " + getTypeName(field.Type) + " " + fieldAccess(field) + ";");
			}
			builder.AppendLine("}");
		}
		private static void generateUnsafeStorage(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			builder.AppendLine("internal unsafe struct " + typeName + "Storage");
			builder.AppendLine("{");
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\tpublic " + getTypeName(field.Type) + "* " + fieldAccess(field) + ";");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\tpublic " + typeName + "AoSBlock* mAoS;");
			}
			builder.AppendLine("}");
		}
		private static void generateSafeSpanStorage(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			builder.AppendLine("internal struct " + typeName + "Storage");
			builder.AppendLine("{");
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\tpublic " + getTypeName(field.Type) + "[] " + fieldAccess(field) + ";");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\tpublic " + typeName + "AoSBlock[] mAoS;");
			}
			builder.AppendLine("}");
		}
		private static void generateSafeRegistryStorage(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			builder.AppendLine("internal sealed class " + typeName + "Storage");
			builder.AppendLine("{");
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\tpublic " + getTypeName(field.Type) + "[] " + fieldAccess(field) + ";");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\tpublic " + typeName + "AoSBlock[] mAoS;");
			}
			builder.AppendLine("\tpublic " + typeName + "Storage(int capacity)");
			builder.AppendLine("\t{");
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\t" + fieldAccess(field) + " = new " + getTypeName(field.Type) + "[capacity];");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\tmAoS = new " + typeName + "AoSBlock[capacity];");
			}
			builder.AppendLine("\t}");
			builder.AppendLine("}");
		}
		private static void generateUnsafeRef(StringBuilder builder, string accessibility, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			builder.AppendLine(accessibility + " unsafe ref struct " + typeName + "Ref");
			builder.AppendLine("{");
			builder.AppendLine("\tprivate readonly " + typeName + "Storage* mStorage;");
			builder.AppendLine("\tprivate readonly int mIndex;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\tprivate readonly " + typeName + "ECSList mOwner;");
			builder.AppendLine("\tprivate readonly int mGeneration;");
			builder.AppendLine("#endif");
			builder.AppendLine("#if UNITY_EDITOR");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tinternal " + typeName + "Ref(" + typeName + "Storage* storage, int index, " + typeName + "ECSList owner, int generation)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tmStorage = storage;");
			builder.AppendLine("\t\tmIndex = index;");
			builder.AppendLine("\t\tmOwner = owner;");
			builder.AppendLine("\t\tmGeneration = generation;");
			builder.AppendLine("\t}");
			builder.AppendLine("#else");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tinternal " + typeName + "Ref(" + typeName + "Storage* storage, int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tmStorage = storage;");
			builder.AppendLine("\t\tmIndex = index;");
			builder.AppendLine("\t}");
			builder.AppendLine("#endif");
			foreach (IFieldSymbol field in ecsFields)
			{
				generateUnsafeRefProperty(builder, field, false);
			}
			foreach (IFieldSymbol field in aosFields)
			{
				generateUnsafeRefProperty(builder, field, true);
			}
			builder.AppendLine("}");
		}
		private static void generateUnsafeRefProperty(StringBuilder builder, IFieldSymbol field, bool aos)
		{
			string accessibility = field.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
			builder.AppendLine("\t" + accessibility + " ref " + getTypeName(field.Type) + " " + fieldAccess(field));
			builder.AppendLine("\t{");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tget");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tmOwner.validateRef(mIndex, mGeneration);");
			builder.AppendLine("#endif");
			if (aos)
			{
				builder.AppendLine("\t\t\treturn ref mStorage->mAoS[mIndex]." + fieldAccess(field) + ";");
			}
			else
			{
				builder.AppendLine("\t\t\treturn ref mStorage->" + fieldAccess(field) + "[mIndex];");
			}
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateSafeSpanRef(StringBuilder builder, string accessibility, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			builder.AppendLine(accessibility + " ref struct " + typeName + "Ref");
			builder.AppendLine("{");
			builder.AppendLine("\tprivate global::System.Span<" + typeName + "Storage> mStorage;");
			builder.AppendLine("\tprivate int mIndex;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\tprivate readonly " + typeName + "ECSList mOwner;");
			builder.AppendLine("\tprivate readonly int mGeneration;");
			builder.AppendLine("#endif");
			builder.AppendLine("#if UNITY_EDITOR");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tinternal " + typeName + "Ref(" + typeName + "Storage[] storage, int index, " + typeName + "ECSList owner, int generation)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tmStorage = new global::System.Span<" + typeName + "Storage>(storage, 0, 1);");
			builder.AppendLine("\t\tmIndex = index;");
			builder.AppendLine("\t\tmOwner = owner;");
			builder.AppendLine("\t\tmGeneration = generation;");
			builder.AppendLine("\t}");
			builder.AppendLine("#else");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tinternal " + typeName + "Ref(" + typeName + "Storage[] storage, int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tmStorage = new global::System.Span<" + typeName + "Storage>(storage, 0, 1);");
			builder.AppendLine("\t\tmIndex = index;");
			builder.AppendLine("\t}");
			builder.AppendLine("#endif");
			foreach (IFieldSymbol field in ecsFields)
			{
				generateSafeSpanRefProperty(builder, field, false);
			}
			foreach (IFieldSymbol field in aosFields)
			{
				generateSafeSpanRefProperty(builder, field, true);
			}
			builder.AppendLine("}");
		}
		private static void generateSafeSpanRefProperty(StringBuilder builder, IFieldSymbol field, bool aos)
		{
			string accessibility = field.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
			builder.AppendLine("\t" + accessibility + " ref " + getTypeName(field.Type) + " " + fieldAccess(field));
			builder.AppendLine("\t{");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tget");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tmOwner.validateRef(mIndex, mGeneration);");
			builder.AppendLine("#endif");
			if (aos)
			{
				builder.AppendLine("\t\t\treturn ref mStorage[0].mAoS[mIndex]." + fieldAccess(field) + ";");
			}
			else
			{
				builder.AppendLine("\t\t\treturn ref mStorage[0]." + fieldAccess(field) + "[mIndex];");
			}
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateSafeRegistry(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			builder.AppendLine("internal static class " + typeName + "StorageRegistry");
			builder.AppendLine("{");
			builder.AppendLine("\tprivate static " + typeName + "Storage[] mStorageList = new " + typeName + "Storage[8];");
			builder.AppendLine("\tprivate static int mNextID;");
			builder.AppendLine("\tprivate static readonly object mLock = new object();");
			builder.AppendLine("\tinternal static int add(int capacity)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tlock (mLock)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tif (mNextID == int.MaxValue)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tthrow new global::System.InvalidOperationException(\"ECS Storage ID已耗尽\");");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tint id = mNextID++;");
			builder.AppendLine("\t\t\tif (id >= mStorageList.Length)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tresizeStorageList(id + 1);");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tmStorageList[id] = new " + typeName + "Storage(capacity);");
			builder.AppendLine("\t\t\treturn id;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tinternal static " + typeName + "Storage getStorage(int id)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\treturn mStorageList[id];");
			builder.AppendLine("\t}");
			foreach (IFieldSymbol field in ecsFields)
			{
				appendAggressiveInlining(builder, 1);
				builder.AppendLine("\tinternal static ref " + getTypeName(field.Type) + " get_" + field.Name + "(int storageID, int index)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\treturn ref mStorageList[storageID]." + fieldAccess(field) + "[index];");
				builder.AppendLine("\t}");
			}
			foreach (IFieldSymbol field in aosFields)
			{
				appendAggressiveInlining(builder, 1);
				builder.AppendLine("\tinternal static ref " + getTypeName(field.Type) + " get_" + field.Name + "(int storageID, int index)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\treturn ref mStorageList[storageID].mAoS[index]." + fieldAccess(field) + ";");
				builder.AppendLine("\t}");
			}
			builder.AppendLine("\tinternal static void remove(int id)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tlock (mLock)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tif ((uint)id < (uint)mStorageList.Length)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmStorageList[id] = null;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate static void resizeStorageList(int minimumCapacity)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tint newCapacity = mStorageList.Length;");
			builder.AppendLine("\t\twhile (newCapacity < minimumCapacity)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tnewCapacity *= 2;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\t" + typeName + "Storage[] newList = new " + typeName + "Storage[newCapacity];");
			builder.AppendLine("\t\tglobal::System.Array.Copy(mStorageList, newList, mStorageList.Length);");
			builder.AppendLine("\t\tmStorageList = newList;");
			builder.AppendLine("\t}");
			builder.AppendLine("}");
		}
		private static void generateSafeRegistryRef(StringBuilder builder, string accessibility, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			builder.AppendLine(accessibility + " readonly struct " + typeName + "Ref");
			builder.AppendLine("{");
			builder.AppendLine("\tprivate readonly int mStorageID;");
			builder.AppendLine("\tprivate readonly int mIndex;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\tprivate readonly " + typeName + "ECSList mOwner;");
			builder.AppendLine("\tprivate readonly int mGeneration;");
			builder.AppendLine("#endif");
			builder.AppendLine("#if UNITY_EDITOR");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tinternal " + typeName + "Ref(int storageID, int index, " + typeName + "ECSList owner, int generation)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tmStorageID = storageID;");
			builder.AppendLine("\t\tmIndex = index;");
			builder.AppendLine("\t\tmOwner = owner;");
			builder.AppendLine("\t\tmGeneration = generation;");
			builder.AppendLine("\t}");
			builder.AppendLine("#else");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tinternal " + typeName + "Ref(int storageID, int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tmStorageID = storageID;");
			builder.AppendLine("\t\tmIndex = index;");
			builder.AppendLine("\t}");
			builder.AppendLine("#endif");
			foreach (IFieldSymbol field in ecsFields)
			{
				generateSafeRegistryRefProperty(builder, typeName, field);
			}
			foreach (IFieldSymbol field in aosFields)
			{
				generateSafeRegistryRefProperty(builder, typeName, field);
			}
			builder.AppendLine("}");
		}
		private static void generateSafeRegistryRefProperty(StringBuilder builder, string typeName, IFieldSymbol field)
		{
			string accessibility = field.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
			builder.AppendLine("\t" + accessibility + " ref " + getTypeName(field.Type) + " " + fieldAccess(field));
			builder.AppendLine("\t{");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tget");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tmOwner.validateRef(mIndex, mGeneration);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\treturn ref " + typeName + "StorageRegistry.get_" + field.Name + "(mStorageID, mIndex);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static string generateLeakTrackerSource()
		{
			StringBuilder builder = new StringBuilder(8192);
			builder.AppendLine("// <auto-generated/>");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("namespace ECSSourceGeneratorGenerated");
			builder.AppendLine("{");
			builder.AppendLine("\t[global::UnityEditor.InitializeOnLoad]");
			builder.AppendLine("\tinternal static class ECSListLeakTracker");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate sealed class LeakInfo");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tpublic int mID;");
			builder.AppendLine("\t\t\tpublic string mTypeName;");
			builder.AppendLine("\t\t\tpublic string mBackend;");
			builder.AppendLine("\t\t\tpublic string mStackTrace;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tprivate static readonly object mLock = new object();");
			builder.AppendLine("\t\tprivate static readonly global::System.Collections.Generic.Dictionary<int, LeakInfo> mActive = new global::System.Collections.Generic.Dictionary<int, LeakInfo>();");
			builder.AppendLine("\t\tprivate static readonly global::System.Collections.Generic.Queue<LeakInfo> mPendingFinalizerWarnings = new global::System.Collections.Generic.Queue<LeakInfo>();");
			builder.AppendLine("\t\tprivate static int mNextID;");
			builder.AppendLine("\t\tprivate static bool mPlayModeExitRequested;");
			builder.AppendLine("\t\tstatic ECSListLeakTracker()");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tglobal::UnityEditor.EditorApplication.update += update;");
			builder.AppendLine("\t\t\tglobal::UnityEditor.EditorApplication.playModeStateChanged += onPlayModeStateChanged;");
			builder.AppendLine("\t\t\tglobal::UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += onBeforeAssemblyReload;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tinternal static int register(string typeName, string backend)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tint id = global::System.Threading.Interlocked.Increment(ref mNextID);");
			builder.AppendLine("\t\t\tLeakInfo info = new LeakInfo();");
			builder.AppendLine("\t\t\tinfo.mID = id;");
			builder.AppendLine("\t\t\tinfo.mTypeName = typeName;");
			builder.AppendLine("\t\t\tinfo.mBackend = backend;");
			builder.AppendLine("\t\t\tinfo.mStackTrace = new global::System.Diagnostics.StackTrace(2, true).ToString();");
			builder.AppendLine("\t\t\tlock (mLock)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmActive[id] = info;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\treturn id;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tinternal static void unregister(int id)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tif (id == 0)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\treturn;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tlock (mLock)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmActive.Remove(id);");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tinternal static void finalizedWithoutDispose(int id)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tif (id == 0)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\treturn;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tlock (mLock)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tLeakInfo info;");
			builder.AppendLine("\t\t\t\tif (mActive.TryGetValue(id, out info))");
			builder.AppendLine("\t\t\t\t{");
			builder.AppendLine("\t\t\t\t\tmActive.Remove(id);");
			builder.AppendLine("\t\t\t\t\tmPendingFinalizerWarnings.Enqueue(info);");
			builder.AppendLine("\t\t\t\t}");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tinternal static void flushPendingWarnings()");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tLeakInfo[] pending = null;");
			builder.AppendLine("\t\t\tlock (mLock)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tif (mPendingFinalizerWarnings.Count > 0)");
			builder.AppendLine("\t\t\t\t{");
			builder.AppendLine("\t\t\t\t\tpending = new LeakInfo[mPendingFinalizerWarnings.Count];");
			builder.AppendLine("\t\t\t\t\tfor (int i = 0; i < pending.Length; ++i)");
			builder.AppendLine("\t\t\t\t\t{");
			builder.AppendLine("\t\t\t\t\t\tpending[i] = mPendingFinalizerWarnings.Dequeue();");
			builder.AppendLine("\t\t\t\t\t}");
			builder.AppendLine("\t\t\t\t}");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tif (pending == null)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\treturn;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tfor (int i = 0; i < pending.Length; ++i)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tLeakInfo info = pending[i];");
			builder.AppendLine("\t\t\t\tglobal::UnityEngine.Debug.LogError(\"[ECS] \" + info.mTypeName + \"(\" + info.mBackend + \")未主动调用Dispose,已由Finalizer兜底释放.\\n创建位置:\\n\" + info.mStackTrace);");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tprivate static void update()");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tflushPendingWarnings();");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tprivate static void onPlayModeStateChanged(global::UnityEditor.PlayModeStateChange state)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tif (state == global::UnityEditor.PlayModeStateChange.ExitingPlayMode)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmPlayModeExitRequested = true;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\telse if (state == global::UnityEditor.PlayModeStateChange.EnteredEditMode)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tif (mPlayModeExitRequested)");
			builder.AppendLine("\t\t\t\t{");
			builder.AppendLine("\t\t\t\t\treportAndClearActive(\"退出PlayMode时仍未调用Dispose\");");
			builder.AppendLine("\t\t\t\t}");
			builder.AppendLine("\t\t\t\tmPlayModeExitRequested = false;");
			builder.AppendLine("\t\t\t\tflushPendingWarnings();");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\telse if (state == global::UnityEditor.PlayModeStateChange.EnteredPlayMode)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmPlayModeExitRequested = false;");
			builder.AppendLine("\t\t\t\tclearActive();");
			builder.AppendLine("\t\t\t\tflushPendingWarnings();");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tprivate static void onBeforeAssemblyReload()");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tif (mPlayModeExitRequested)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\treportAndClearActive(\"退出PlayMode时仍未调用Dispose\");");
			builder.AppendLine("\t\t\t\tmPlayModeExitRequested = false;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\telse");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tclearActive();");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tflushPendingWarnings();");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tprivate static void reportAndClearActive(string reason)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tLeakInfo[] active = null;");
			builder.AppendLine("\t\t\tlock (mLock)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tif (mActive.Count > 0)");
			builder.AppendLine("\t\t\t\t{");
			builder.AppendLine("\t\t\t\t\tactive = new LeakInfo[mActive.Count];");
			builder.AppendLine("\t\t\t\t\tmActive.Values.CopyTo(active, 0);");
			builder.AppendLine("\t\t\t\t\tmActive.Clear();");
			builder.AppendLine("\t\t\t\t}");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tif (active == null)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\treturn;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tfor (int i = 0; i < active.Length; ++i)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tLeakInfo info = active[i];");
			builder.AppendLine("\t\t\t\tglobal::UnityEngine.Debug.LogError(\"[ECS] \" + info.mTypeName + \"(\" + info.mBackend + \")\" + reason + \".\\n创建位置:\\n\" + info.mStackTrace);");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tprivate static void clearActive()");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tlock (mLock)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmActive.Clear();");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("}");
			builder.AppendLine("#endif");
			return builder.ToString();
		}
		private static void generateUnsafeList(StringBuilder builder, string accessibility, string typeName, string fullTypeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, string backendReason)
		{
			builder.AppendLine(accessibility + " unsafe sealed class " + typeName + "ECSList : global::System.IDisposable");
			builder.AppendLine("{");
			builder.AppendLine("\tprivate const int ALIGNMENT = 64;");
			builder.AppendLine("\tpublic const bool IsUnsafeBackend = true;");
			builder.AppendLine("\tpublic const string BackendName = \"Unsafe\";");
			builder.AppendLine("\tpublic const string BackendReason = \"" + backendReason + "\";");
			builder.AppendLine("\tprivate global::System.IntPtr mRawMemory;");
			builder.AppendLine("\tprivate global::System.IntPtr mStorageMemory;");
			builder.AppendLine("\tprivate " + typeName + "Storage* mStorage;");
			builder.AppendLine("\tprivate int mCount;");
			builder.AppendLine("\tprivate int mCapacity;");
			builder.AppendLine("\tprivate bool mDisposed;");
			generateEditorValidationFields(builder, true);
			generateUnsafeProperties(builder, typeName);
			foreach (IFieldSymbol field in ecsFields)
			{
				generateUnsafeColumn(builder, typeName, field);
			}
			generateUnsafeConstructor(builder, typeName);
			generateUnsafeContainerMethods(builder, typeName, fullTypeName, ecsFields, aosFields);
			generateUnsafeResize(builder, typeName, ecsFields, aosFields);
			generateUnsafeAllocateColumns(builder, typeName, ecsFields, aosFields);
			generateUnsafeDispose(builder);
			generateUnsafeHelpers(builder);
			generateEditorValidationMethods(builder, typeName, true);
			builder.AppendLine("}");
		}
		private static void generateSafeSpanList(StringBuilder builder, string accessibility, string typeName, string fullTypeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, string backendReason)
		{
			builder.AppendLine(accessibility + " sealed class " + typeName + "ECSList : global::System.IDisposable");
			builder.AppendLine("{");
			builder.AppendLine("\tpublic const bool IsUnsafeBackend = false;");
			builder.AppendLine("\tpublic const string BackendName = \"SafeSpan\";");
			builder.AppendLine("\tpublic const string BackendReason = \"" + backendReason + "\";");
			builder.AppendLine("\tprivate readonly " + typeName + "Storage[] mStorage;");
			builder.AppendLine("\tprivate int mCount;");
			builder.AppendLine("\tprivate int mCapacity;");
			builder.AppendLine("\tprivate bool mDisposed;");
			generateEditorValidationFields(builder, false);
			generateSafeSpanProperties(builder, typeName);
			foreach (IFieldSymbol field in ecsFields)
			{
				generateSafeSpanColumn(builder, typeName, field);
			}
			generateSafeSpanConstructor(builder, typeName, ecsFields, aosFields);
			generateSafeContainerMethods(builder, typeName, fullTypeName, ecsFields, aosFields, true);
			generateSafeSpanResize(builder, typeName, ecsFields, aosFields);
			generateSafeSpanDispose(builder, typeName);
			generateEditorValidationMethods(builder, typeName, false);
			builder.AppendLine("}");
		}
		private static void generateSafeRegistryList(StringBuilder builder, string accessibility, string typeName, string fullTypeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, string backendReason)
		{
			builder.AppendLine(accessibility + " sealed class " + typeName + "ECSList : global::System.IDisposable");
			builder.AppendLine("{");
			builder.AppendLine("\tpublic const bool IsUnsafeBackend = false;");
			builder.AppendLine("\tpublic const string BackendName = \"SafeRegistry\";");
			builder.AppendLine("\tpublic const string BackendReason = \"" + backendReason + "\";");
			builder.AppendLine("\tprivate readonly int mStorageID = -1;");
			builder.AppendLine("\tprivate int mCount;");
			builder.AppendLine("\tprivate int mCapacity;");
			builder.AppendLine("\tprivate bool mDisposed;");
			generateEditorValidationFields(builder, true);
			generateSafeRegistryProperties(builder, typeName);
			foreach (IFieldSymbol field in ecsFields)
			{
				generateSafeRegistryColumn(builder, typeName, field);
			}
			generateSafeRegistryConstructor(builder, typeName);
			generateSafeContainerMethods(builder, typeName, fullTypeName, ecsFields, aosFields, false);
			generateSafeRegistryResize(builder, typeName, ecsFields, aosFields);
			generateSafeRegistryDispose(builder, typeName);
			generateEditorValidationMethods(builder, typeName, true);
			builder.AppendLine("}");
		}
		private static void generateEditorValidationFields(StringBuilder builder, bool trackDispose)
		{
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\tprivate int[] mRefGeneration;");
			builder.AppendLine("\tprivate int mColumnVersion;");
			if (trackDispose)
			{
				builder.AppendLine("\tprivate int mDebugLifecycleID;");
				builder.AppendLine("\tprivate static int mUndisposedFinalizerCount;");
				builder.AppendLine("\tprivate static int mTotalUndisposedFinalizerCount;");
				builder.AppendLine("\tpublic static int DebugUndisposedFinalizerCount");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tget");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\treturn global::System.Threading.Volatile.Read(ref mUndisposedFinalizerCount);");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic static int DebugTotalUndisposedFinalizerCount");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tget");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\treturn global::System.Threading.Volatile.Read(ref mTotalUndisposedFinalizerCount);");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t}");
			}
			builder.AppendLine("#endif");
		}
		private static void generateEditorValidationMethods(StringBuilder builder, string typeName, bool trackDispose)
		{
			builder.AppendLine("#if UNITY_EDITOR");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate void validateAlive()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (mDisposed)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSList\");");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate void validateIndex(int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("\t\tif ((uint)index >= (uint)mCount)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.ArgumentOutOfRangeException(nameof(index), \"" + typeName + "ECSList索引越界,Index:\" + index + \",Count:\" + mCount);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tinternal void validateRef(int index, int generation)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("\t\tif ((uint)index >= (uint)mCount)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.InvalidOperationException(\"" + typeName + "Ref指向的元素已经不存在,Index:\" + index + \",Count:\" + mCount);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tif (mRefGeneration[index] != generation)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.InvalidOperationException(\"" + typeName + "Ref在元素结构发生变化后被继续使用,Index:\" + index);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tinternal void validateColumn(int index, int version)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("\t\tif (version != mColumnVersion)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.InvalidOperationException(\"" + typeName + " Column在ECSList结构发生变化后被继续使用\");");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tif ((uint)index >= (uint)mCount)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.ArgumentOutOfRangeException(nameof(index), \"" + typeName + " Column索引越界,Index:\" + index + \",Count:\" + mCount);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate void invalidateRef(int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\t++mRefGeneration[index];");
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void invalidateAllRefs()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tfor (int i = 0; i < mCount; ++i)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\t++mRefGeneration[i];");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate void invalidateColumn()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\t++mColumnVersion;");
			builder.AppendLine("\t}");
			if (trackDispose)
			{
				builder.AppendLine("\tpublic static void DebugFlushUndisposedFinalizerWarning()");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tglobal::System.Threading.Interlocked.Exchange(ref mUndisposedFinalizerCount, 0);");
				builder.AppendLine("\t\tglobal::ECSSourceGeneratorGenerated.ECSListLeakTracker.flushPendingWarnings();");
				builder.AppendLine("\t}");
			}
			builder.AppendLine("#endif");
		}
		private static void generateUnsafeProperties(StringBuilder builder, string typeName)
		{
			generateCountCapacity(builder);
			builder.AppendLine("\tpublic " + typeName + "Ref this[int index]");
			builder.AppendLine("\t{");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tget");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tvalidateIndex(index);");
			builder.AppendLine("\t\t\treturn new " + typeName + "Ref(mStorage, index, this, mRefGeneration[index]);");
			builder.AppendLine("#else");
			builder.AppendLine("\t\t\treturn new " + typeName + "Ref(mStorage, index);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateSafeSpanProperties(StringBuilder builder, string typeName)
		{
			generateCountCapacity(builder);
			builder.AppendLine("\tpublic " + typeName + "Ref this[int index]");
			builder.AppendLine("\t{");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tget");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tvalidateIndex(index);");
			builder.AppendLine("\t\t\treturn new " + typeName + "Ref(mStorage, index, this, mRefGeneration[index]);");
			builder.AppendLine("#else");
			builder.AppendLine("\t\t\treturn new " + typeName + "Ref(mStorage, index);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateSafeRegistryProperties(StringBuilder builder, string typeName)
		{
			generateCountCapacity(builder);
			builder.AppendLine("\tpublic " + typeName + "Ref this[int index]");
			builder.AppendLine("\t{");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tget");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tvalidateIndex(index);");
			builder.AppendLine("\t\t\treturn new " + typeName + "Ref(mStorageID, index, this, mRefGeneration[index]);");
			builder.AppendLine("#else");
			builder.AppendLine("\t\t\treturn new " + typeName + "Ref(mStorageID, index);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateCountCapacity(StringBuilder builder)
		{
			builder.AppendLine("\tpublic int Count");
			builder.AppendLine("\t{");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tget");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\treturn mCount;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic int Capacity");
			builder.AppendLine("\t{");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tget");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\treturn mCapacity;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateUnsafeColumn(StringBuilder builder, string typeName, IFieldSymbol field)
		{
			string fieldType = getTypeName(field.Type);
			string columnType = getColumnTypeName(field.Name);
			string methodName = getColumnMethodName(field.Name);
			string accessibility = field.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
			builder.AppendLine("\t" + accessibility + " unsafe ref struct " + columnType);
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate readonly " + fieldType + "* mPointer;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tprivate readonly " + typeName + "ECSList mOwner;");
			builder.AppendLine("\t\tprivate readonly int mVersion;");
			builder.AppendLine("#endif");
			builder.AppendLine("#if UNITY_EDITOR");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tinternal " + columnType + "(" + fieldType + "* pointer, " + typeName + "ECSList owner, int version)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmPointer = pointer;");
			builder.AppendLine("\t\t\tmOwner = owner;");
			builder.AppendLine("\t\t\tmVersion = version;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("#else");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tinternal " + columnType + "(" + fieldType + "* pointer)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmPointer = pointer;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tpublic ref " + fieldType + " this[int index]");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\t\tmOwner.validateColumn(index, mVersion);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\t\treturn ref mPointer[index];");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\t" + accessibility + " " + columnType + " " + methodName + "()");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("\t\treturn new " + columnType + "(mStorage->" + fieldAccess(field) + ", this, mColumnVersion);");
			builder.AppendLine("#else");
			builder.AppendLine("\t\treturn new " + columnType + "(mStorage->" + fieldAccess(field) + ");");
			builder.AppendLine("#endif");
			builder.AppendLine("\t}");
		}
		private static void generateSafeSpanColumn(StringBuilder builder, string typeName, IFieldSymbol field)
		{
			generateSafeColumnType(builder, typeName, field, true);
			string accessibility = field.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
			string columnType = getColumnTypeName(field.Name);
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\t" + accessibility + " " + columnType + " " + getColumnMethodName(field.Name) + "()");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("\t\treturn new " + columnType + "(mStorage[0]." + fieldAccess(field) + ", this, mColumnVersion);");
			builder.AppendLine("#else");
			builder.AppendLine("\t\treturn new " + columnType + "(mStorage[0]." + fieldAccess(field) + ");");
			builder.AppendLine("#endif");
			builder.AppendLine("\t}");
		}
		private static void generateSafeRegistryColumn(StringBuilder builder, string typeName, IFieldSymbol field)
		{
			generateSafeColumnType(builder, typeName, field, false);
			string accessibility = field.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
			string columnType = getColumnTypeName(field.Name);
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\t" + accessibility + " " + columnType + " " + getColumnMethodName(field.Name) + "()");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t" + typeName + "Storage storage = " + typeName + "StorageRegistry.getStorage(mStorageID);");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\treturn new " + columnType + "(storage." + fieldAccess(field) + ", this, mColumnVersion);");
			builder.AppendLine("#else");
			builder.AppendLine("\t\treturn new " + columnType + "(storage." + fieldAccess(field) + ");");
			builder.AppendLine("#endif");
			builder.AppendLine("\t}");
		}
		private static void generateSafeColumnType(StringBuilder builder, string typeName, IFieldSymbol field, bool refStruct)
		{
			string fieldType = getTypeName(field.Type);
			string columnType = getColumnTypeName(field.Name);
			string accessibility = field.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
			builder.AppendLine("\t" + accessibility + (refStruct ? " ref struct " : " readonly struct ") + columnType);
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate readonly " + fieldType + "[] mArray;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tprivate readonly " + typeName + "ECSList mOwner;");
			builder.AppendLine("\t\tprivate readonly int mVersion;");
			builder.AppendLine("#endif");
			builder.AppendLine("#if UNITY_EDITOR");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tinternal " + columnType + "(" + fieldType + "[] array, " + typeName + "ECSList owner, int version)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmArray = array;");
			builder.AppendLine("\t\t\tmOwner = owner;");
			builder.AppendLine("\t\t\tmVersion = version;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("#else");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tinternal " + columnType + "(" + fieldType + "[] array)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmArray = array;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tpublic ref " + fieldType + " this[int index]");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\t\tmOwner.validateColumn(index, mVersion);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\t\treturn ref mArray[index];");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateUnsafeConstructor(StringBuilder builder, string typeName)
		{
			builder.AppendLine("\tpublic " + typeName + "ECSList(int capacity = 4)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (capacity < 1)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tcapacity = 1;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tmStorageMemory = global::System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(" + typeName + "Storage));");
			builder.AppendLine("\t\tmStorage = (" + typeName + "Storage*)mStorageMemory.ToPointer();");
			builder.AppendLine("\t\t" + typeName + "Storage initialStorage;");
			builder.AppendLine("\t\tallocateColumns(capacity, out mRawMemory, out initialStorage);");
			builder.AppendLine("\t\t*mStorage = initialStorage;");
			builder.AppendLine("\t\tmCapacity = capacity;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tmRefGeneration = new int[capacity];");
			builder.AppendLine("\t\tmDebugLifecycleID = global::ECSSourceGeneratorGenerated.ECSListLeakTracker.register(\"" + typeName + "ECSList\", \"Unsafe\");");
			builder.AppendLine("#endif");
			builder.AppendLine("\t}");
			builder.AppendLine("\t~" + typeName + "ECSList()");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tif (!mDisposed)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tglobal::System.Threading.Interlocked.Increment(ref mUndisposedFinalizerCount);");
			builder.AppendLine("\t\t\tglobal::System.Threading.Interlocked.Increment(ref mTotalUndisposedFinalizerCount);");
			builder.AppendLine("\t\t\tglobal::ECSSourceGeneratorGenerated.ECSListLeakTracker.finalizedWithoutDispose(mDebugLifecycleID);");
			builder.AppendLine("\t\t\tmDebugLifecycleID = 0;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tdispose();");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic void Dispose()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tdispose();");
			builder.AppendLine("\t\tglobal::System.GC.SuppressFinalize(this);");
			builder.AppendLine("\t}");
		}
		private static void generateSafeSpanConstructor(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			builder.AppendLine("\tpublic " + typeName + "ECSList(int capacity = 4)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (capacity < 1)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tcapacity = 1;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tmStorage = new " + typeName + "Storage[1];");
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\tmStorage[0]." + fieldAccess(field) + " = new " + getTypeName(field.Type) + "[capacity];");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\tmStorage[0].mAoS = new " + typeName + "AoSBlock[capacity];");
			}
			builder.AppendLine("\t\tmCapacity = capacity;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tmRefGeneration = new int[capacity];");
			builder.AppendLine("#endif");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic void Dispose()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tdispose();");
			builder.AppendLine("\t}");
		}
		private static void generateSafeRegistryConstructor(StringBuilder builder, string typeName)
		{
			builder.AppendLine("\tpublic " + typeName + "ECSList(int capacity = 4)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (capacity < 1)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tcapacity = 1;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tmStorageID = " + typeName + "StorageRegistry.add(capacity);");
			builder.AppendLine("\t\tmCapacity = capacity;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tmRefGeneration = new int[capacity];");
			builder.AppendLine("\t\tmDebugLifecycleID = global::ECSSourceGeneratorGenerated.ECSListLeakTracker.register(\"" + typeName + "ECSList\", \"SafeRegistry\");");
			builder.AppendLine("#endif");
			builder.AppendLine("\t}");
			builder.AppendLine("\t~" + typeName + "ECSList()");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tif (!mDisposed)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tglobal::System.Threading.Interlocked.Increment(ref mUndisposedFinalizerCount);");
			builder.AppendLine("\t\t\tglobal::System.Threading.Interlocked.Increment(ref mTotalUndisposedFinalizerCount);");
			builder.AppendLine("\t\t\tglobal::ECSSourceGeneratorGenerated.ECSListLeakTracker.finalizedWithoutDispose(mDebugLifecycleID);");
			builder.AppendLine("\t\t\tmDebugLifecycleID = 0;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tdispose();");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic void Dispose()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tdispose();");
			builder.AppendLine("\t\tglobal::System.GC.SuppressFinalize(this);");
			builder.AppendLine("\t}");
		}
		private static void generateUnsafeContainerMethods(StringBuilder builder, string typeName, string fullTypeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tpublic void Add(" + fullTypeName + " value)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("\t\tinvalidateColumn();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tif (mCount >= mCapacity)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tresize(mCapacity * 2);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tsetValue(mCount, value);");
			builder.AppendLine("\t\t++mCount;");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tpublic " + fullTypeName + " Get(int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateIndex(index);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t" + fullTypeName + " value = default(" + fullTypeName + ");");
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\tvalue." + fieldAccess(field) + " = mStorage->" + fieldAccess(field) + "[index];");
			}
			foreach (IFieldSymbol field in aosFields)
			{
				builder.AppendLine("\t\tvalue." + fieldAccess(field) + " = mStorage->mAoS[index]." + fieldAccess(field) + ";");
			}
			builder.AppendLine("\t\treturn value;");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tpublic void Set(int index, " + fullTypeName + " value)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateIndex(index);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tsetValue(index, value);");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic void Clear()");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tif (mCount == 0)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tinvalidateAllRefs();");
			builder.AppendLine("\t\tinvalidateColumn();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tmCount = 0;");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic void RemoveAtSwapBack(int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateIndex(index);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tint lastIndex = mCount - 1;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tinvalidateRef(index);");
			builder.AppendLine("\t\tif (index != lastIndex)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tinvalidateRef(lastIndex);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tinvalidateColumn();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tif (index != lastIndex)");
			builder.AppendLine("\t\t{");
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\t\tmStorage->" + fieldAccess(field) + "[index] = mStorage->" + fieldAccess(field) + "[lastIndex];");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\t\tmStorage->mAoS[index] = mStorage->mAoS[lastIndex];");
			}
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\t--mCount;");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate void setValue(int index, " + fullTypeName + " value)");
			builder.AppendLine("\t{");
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\tmStorage->" + fieldAccess(field) + "[index] = value." + fieldAccess(field) + ";");
			}
			foreach (IFieldSymbol field in aosFields)
			{
				builder.AppendLine("\t\tmStorage->mAoS[index]." + fieldAccess(field) + " = value." + fieldAccess(field) + ";");
			}
			builder.AppendLine("\t}");
		}
		private static void generateSafeContainerMethods(StringBuilder builder, string typeName, string fullTypeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, bool spanBackend)
		{
			bool hasManagedECS = ecsFields.Any(field => !field.Type.IsUnmanagedType);
			bool hasManagedAoS = aosFields.Any(field => !field.Type.IsUnmanagedType);
			string storageDeclaration = spanBackend ? "ref " + typeName + "Storage storage = ref mStorage[0];" : typeName + "Storage storage = " + typeName + "StorageRegistry.getStorage(mStorageID);";
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tpublic void Add(" + fullTypeName + " value)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("\t\tinvalidateColumn();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tif (mCount >= mCapacity)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tresize(mCapacity * 2);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tsetValue(mCount, value);");
			builder.AppendLine("\t\t++mCount;");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tpublic " + fullTypeName + " Get(int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateIndex(index);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t" + storageDeclaration);
			builder.AppendLine("\t\t" + fullTypeName + " value = default(" + fullTypeName + ");");
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\tvalue." + fieldAccess(field) + " = storage." + fieldAccess(field) + "[index];");
			}
			foreach (IFieldSymbol field in aosFields)
			{
				builder.AppendLine("\t\tvalue." + fieldAccess(field) + " = storage.mAoS[index]." + fieldAccess(field) + ";");
			}
			builder.AppendLine("\t\treturn value;");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tpublic void Set(int index, " + fullTypeName + " value)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateIndex(index);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tsetValue(index, value);");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic void Clear()");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tif (mCount == 0)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tinvalidateAllRefs();");
			builder.AppendLine("\t\tinvalidateColumn();");
			builder.AppendLine("#endif");
			if (hasManagedECS || hasManagedAoS)
			{
				builder.AppendLine("\t\t" + storageDeclaration);
				foreach (IFieldSymbol field in ecsFields)
				{
					if (!field.Type.IsUnmanagedType)
					{
						builder.AppendLine("\t\tglobal::System.Array.Clear(storage." + fieldAccess(field) + ", 0, mCount);");
					}
				}
				if (hasManagedAoS)
				{
					builder.AppendLine("\t\tglobal::System.Array.Clear(storage.mAoS, 0, mCount);");
				}
			}
			builder.AppendLine("\t\tmCount = 0;");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic void RemoveAtSwapBack(int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateIndex(index);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t" + storageDeclaration);
			builder.AppendLine("\t\tint lastIndex = mCount - 1;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tinvalidateRef(index);");
			builder.AppendLine("\t\tif (index != lastIndex)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tinvalidateRef(lastIndex);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tinvalidateColumn();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tif (index != lastIndex)");
			builder.AppendLine("\t\t{");
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\t\tstorage." + fieldAccess(field) + "[index] = storage." + fieldAccess(field) + "[lastIndex];");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\t\tstorage.mAoS[index] = storage.mAoS[lastIndex];");
			}
			builder.AppendLine("\t\t}");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (!field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\tstorage." + fieldAccess(field) + "[lastIndex] = default(" + getTypeName(field.Type) + ");");
				}
			}
			if (hasManagedAoS)
			{
				builder.AppendLine("\t\tstorage.mAoS[lastIndex] = default(" + typeName + "AoSBlock);");
			}
			builder.AppendLine("\t\t--mCount;");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate void setValue(int index, " + fullTypeName + " value)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\t" + storageDeclaration);
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\tstorage." + fieldAccess(field) + "[index] = value." + fieldAccess(field) + ";");
			}
			foreach (IFieldSymbol field in aosFields)
			{
				builder.AppendLine("\t\tstorage.mAoS[index]." + fieldAccess(field) + " = value." + fieldAccess(field) + ";");
			}
			builder.AppendLine("\t}");
		}
		private static void generateUnsafeResize(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			builder.AppendLine("\tprivate void resize(int capacity)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tglobal::System.IntPtr newRawMemory;");
			builder.AppendLine("\t\t" + typeName + "Storage newStorage;");
			builder.AppendLine("\t\tallocateColumns(capacity, out newRawMemory, out newStorage);");
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\tcopyMemory(newStorage." + fieldAccess(field) + ", mStorage->" + fieldAccess(field) + ", (long)mCount * sizeof(" + getTypeName(field.Type) + "));");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\tcopyMemory(newStorage.mAoS, mStorage->mAoS, (long)mCount * sizeof(" + typeName + "AoSBlock));");
			}
			builder.AppendLine("\t\tglobal::System.Runtime.InteropServices.Marshal.FreeHGlobal(mRawMemory);");
			builder.AppendLine("\t\tmRawMemory = newRawMemory;");
			builder.AppendLine("\t\t*mStorage = newStorage;");
			builder.AppendLine("\t\tmCapacity = capacity;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tglobal::System.Array.Resize(ref mRefGeneration, capacity);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t}");
		}
		private static void generateSafeSpanResize(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			builder.AppendLine("\tprivate void resize(int capacity)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tref " + typeName + "Storage storage = ref mStorage[0];");
			generateSafeResizeBody(builder, typeName, ecsFields, aosFields);
			builder.AppendLine("\t}");
		}
		private static void generateSafeRegistryResize(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			builder.AppendLine("\tprivate void resize(int capacity)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\t" + typeName + "Storage storage = " + typeName + "StorageRegistry.getStorage(mStorageID);");
			generateSafeResizeBody(builder, typeName, ecsFields, aosFields);
			builder.AppendLine("\t}");
		}
		private static void generateSafeResizeBody(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			foreach (IFieldSymbol field in ecsFields)
			{
				string fieldType = getTypeName(field.Type);
				string localName = "new_" + field.Name;
				builder.AppendLine("\t\t" + fieldType + "[] " + localName + " = new " + fieldType + "[capacity];");
				builder.AppendLine("\t\tglobal::System.Array.Copy(storage." + fieldAccess(field) + ", " + localName + ", mCount);");
				builder.AppendLine("\t\tstorage." + fieldAccess(field) + " = " + localName + ";");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\t" + typeName + "AoSBlock[] newAoS = new " + typeName + "AoSBlock[capacity];");
				builder.AppendLine("\t\tglobal::System.Array.Copy(storage.mAoS, newAoS, mCount);");
				builder.AppendLine("\t\tstorage.mAoS = newAoS;");
			}
			builder.AppendLine("\t\tmCapacity = capacity;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tglobal::System.Array.Resize(ref mRefGeneration, capacity);");
			builder.AppendLine("#endif");
		}
		private static void generateUnsafeAllocateColumns(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			builder.AppendLine("\tprivate static void allocateColumns(int capacity, out global::System.IntPtr rawMemory, out " + typeName + "Storage storage)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tstorage = default(" + typeName + "Storage);");
			builder.AppendLine("\t\tlong totalBytes = ALIGNMENT;");
			foreach (IFieldSymbol field in ecsFields)
			{
				string bytesName = getBytesVariableName(field.Name);
				builder.AppendLine("\t\tlong " + bytesName + " = alignUp((long)capacity * sizeof(" + getTypeName(field.Type) + "), ALIGNMENT);");
				builder.AppendLine("\t\ttotalBytes += " + bytesName + ";");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\tlong aosBytes = alignUp((long)capacity * sizeof(" + typeName + "AoSBlock), ALIGNMENT);");
				builder.AppendLine("\t\ttotalBytes += aosBytes;");
			}
			builder.AppendLine("\t\trawMemory = global::System.Runtime.InteropServices.Marshal.AllocHGlobal(new global::System.IntPtr(totalBytes));");
			builder.AppendLine("\t\tbyte* current = alignPointer((byte*)rawMemory.ToPointer(), ALIGNMENT);");
			foreach (IFieldSymbol field in ecsFields)
			{
				string bytesName = getBytesVariableName(field.Name);
				builder.AppendLine("\t\tstorage." + fieldAccess(field) + " = (" + getTypeName(field.Type) + "*)current;");
				builder.AppendLine("\t\tcurrent += " + bytesName + ";");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\tstorage.mAoS = (" + typeName + "AoSBlock*)current;");
			}
			builder.AppendLine("\t}");
		}
		private static void generateUnsafeDispose(StringBuilder builder)
		{
			builder.AppendLine("\tprivate void dispose()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (mDisposed)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tmDisposed = true;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tif (mDebugLifecycleID != 0)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tglobal::ECSSourceGeneratorGenerated.ECSListLeakTracker.unregister(mDebugLifecycleID);");
			builder.AppendLine("\t\t\tmDebugLifecycleID = 0;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tif (mRawMemory != global::System.IntPtr.Zero)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tglobal::System.Runtime.InteropServices.Marshal.FreeHGlobal(mRawMemory);");
			builder.AppendLine("\t\t\tmRawMemory = global::System.IntPtr.Zero;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tif (mStorageMemory != global::System.IntPtr.Zero)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tglobal::System.Runtime.InteropServices.Marshal.FreeHGlobal(mStorageMemory);");
			builder.AppendLine("\t\t\tmStorageMemory = global::System.IntPtr.Zero;");
			builder.AppendLine("\t\t\tmStorage = null;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tmCount = 0;");
			builder.AppendLine("\t\tmCapacity = 0;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tmRefGeneration = null;");
			builder.AppendLine("#endif");
			builder.AppendLine("\t}");
		}
		private static void generateSafeSpanDispose(StringBuilder builder, string typeName)
		{
			builder.AppendLine("\tprivate void dispose()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (mDisposed)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tmDisposed = true;");
			builder.AppendLine("\t\tmStorage[0] = default(" + typeName + "Storage);");
			builder.AppendLine("\t\tmCount = 0;");
			builder.AppendLine("\t\tmCapacity = 0;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tmRefGeneration = null;");
			builder.AppendLine("#endif");
			builder.AppendLine("\t}");
		}
		private static void generateSafeRegistryDispose(StringBuilder builder, string typeName)
		{
			builder.AppendLine("\tprivate void dispose()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (mDisposed)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tmDisposed = true;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tif (mDebugLifecycleID != 0)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tglobal::ECSSourceGeneratorGenerated.ECSListLeakTracker.unregister(mDebugLifecycleID);");
			builder.AppendLine("\t\t\tmDebugLifecycleID = 0;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tif (mStorageID >= 0)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\t" + typeName + "StorageRegistry.remove(mStorageID);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tmCount = 0;");
			builder.AppendLine("\t\tmCapacity = 0;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tmRefGeneration = null;");
			builder.AppendLine("#endif");
			builder.AppendLine("\t}");
		}
		private static void generateUnsafeHelpers(StringBuilder builder)
		{
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate static void copyMemory(void* destination, void* source, long bytes)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (bytes > 0)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tglobal::System.Buffer.MemoryCopy(source, destination, bytes, bytes);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate static long alignUp(long value, int alignment)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\treturn (value + alignment - 1L) & ~(alignment - 1L);");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate static byte* alignPointer(byte* pointer, int alignment)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tulong address = (ulong)pointer;");
			builder.AppendLine("\t\treturn (byte*)((address + (ulong)alignment - 1UL) & ~((ulong)alignment - 1UL));");
			builder.AppendLine("\t}");
		}
		private static void appendAggressiveInlining(StringBuilder builder, int tabCount)
		{
			builder.Append(new string('\t', tabCount));
			builder.AppendLine("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
		}
		private static bool hasAttribute(ISymbol symbol, string attributeName)
		{
			foreach (AttributeData attribute in symbol.GetAttributes())
			{
				if (attribute.AttributeClass?.Name == attributeName)
				{
					return true;
				}
			}
			return false;
		}
		private static bool hasPreprocessorSymbol(Compilation compilation, string symbol)
		{
			foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
			{
				if (syntaxTree.Options is CSharpParseOptions options && options.PreprocessorSymbolNames.Contains(symbol))
				{
					return true;
				}
			}
			return false;
		}
		private static string getTypeName(ITypeSymbol type)
		{
			return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		}
		private static string fieldAccess(IFieldSymbol field)
		{
			string name = field.Name;
			if (SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None)
			{
				return "@" + name;
			}
			return name;
		}
		private static string getColumnTypeName(string fieldName)
		{
			return "__ECSColumn_" + fieldName;
		}
		private static string getColumnMethodName(string fieldName)
		{
			string name = fieldName;
			if (name.Length > 1 && name[0] == 'm' && char.IsUpper(name[1]))
			{
				name = name.Substring(1);
			}
			else if (name.Length > 0 && char.IsLower(name[0]))
			{
				name = char.ToUpperInvariant(name[0]) + name.Substring(1);
			}
			return "get" + name + "Column";
		}
		private static string getBytesVariableName(string fieldName)
		{
			return "bytes_" + fieldName;
		}
		private static string getHintName(INamedTypeSymbol structSymbol)
		{
			string name = structSymbol.ToDisplayString();
			StringBuilder builder = new StringBuilder(name.Length);
			foreach (char character in name)
			{
				if (char.IsLetterOrDigit(character) || character == '_')
				{
					builder.Append(character);
				}
				else
				{
					builder.Append('_');
				}
			}
			return builder.ToString();
		}
		private static void reportDiagnostic(GeneratorExecutionContext context, DiagnosticDescriptor descriptor, ISymbol symbol, params object[] args)
		{
			Location location = symbol.Locations.FirstOrDefault() ?? Location.None;
			context.ReportDiagnostic(Diagnostic.Create(descriptor, location, args));
		}
	}
	internal sealed class ECSSyntaxReceiver : ISyntaxReceiver
	{
		public readonly List<StructDeclarationSyntax> mStructList = new List<StructDeclarationSyntax>();
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if (syntaxNode is StructDeclarationSyntax declaration && declaration.AttributeLists.Count > 0)
			{
				mStructList.Add(declaration);
			}
		}
	}
}