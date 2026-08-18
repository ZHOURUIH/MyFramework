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
			bool hasSpan = context.Compilation.GetTypeByMetadataName("System.Span`1") != null && context.Compilation.GetTypeByMetadataName("System.ReadOnlySpan`1") != null;
			bool hasBurstJobs = context.Compilation.GetTypeByMetadataName("Unity.Burst.BurstCompileAttribute") != null
				&& context.Compilation.GetTypeByMetadataName("Unity.Jobs.JobHandle") != null
				&& context.Compilation.GetTypeByMetadataName("Unity.Jobs.IJobParallelFor") != null
				&& context.Compilation.GetTypeByMetadataName("Unity.Jobs.IJobParallelForExtensions") != null
				&& context.Compilation.GetTypeByMetadataName("Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestrictionAttribute") != null;
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
				bool hasNativeECS = ecsFields.Any(field => field.Type.IsUnmanagedType);
				bool hasManagedECS = ecsFields.Any(field => !field.Type.IsUnmanagedType);
				bool hasManagedAoS = aosFields.Any(field => !field.Type.IsUnmanagedType);
				bool hasNativeAoS = aosFields.Count > 0 && !hasManagedAoS;
				bool hasNativeStorage = hasNativeECS || hasNativeAoS;
				bool hasManagedStorage = hasManagedECS || hasManagedAoS;
				Backend backend;
				string backendReason;
				if (forceSafeRegistry)
				{
					backend = Backend.SafeRegistry;
					backendReason = "ECS_FORCE_SAFE_REGISTRY";
					needLeakTracker = true;
				}
				else if (allowUnsafe && hasNativeStorage)
				{
					backend = Backend.Unsafe;
					backendReason = hasManagedStorage ? "AllowUnsafe=true,HybridStorage=true" : "AllowUnsafe=true,Unmanaged=true";
					needLeakTracker = true;
				}
				else if (hasSpan)
				{
					backend = Backend.SafeSpan;
					backendReason = allowUnsafe ? "NoNativeStorage,Span=true" : "AllowUnsafe=false,Span=true";
				}
				else
				{
					backend = Backend.SafeRegistry;
					backendReason = "SpanUnavailable";
					needLeakTracker = true;
				}
				string source = generateCode(structSymbol, ecsFields, aosFields, backend, backendReason, hasSpan, hasBurstJobs);
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
		private static string generateCode(INamedTypeSymbol structSymbol, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, Backend backend, string backendReason, bool hasSpan, bool hasBurstJobs)
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
					generateUnsafeManagedStorage(builder, typeName, ecsFields, aosFields);
					generateUnsafeRef(builder, accessibility, typeName, ecsFields, aosFields);
					generateUnsafeList(builder, accessibility, typeName, fullTypeName, ecsFields, aosFields, backendReason, hasSpan, hasBurstJobs);
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
			generateDictionary(builder, accessibility, typeName, fullTypeName, ecsFields, aosFields, backend, hasSpan, hasBurstJobs);
			if (!string.IsNullOrEmpty(namespaceName))
			{
				builder.AppendLine("}");
			}
			return builder.ToString();
		}
		private static void generateDictionary(StringBuilder builder, string accessibility, string typeName, string fullTypeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, Backend backend, bool hasSpan, bool hasBurstJobs)
		{
			bool hasBurstIntegration = backend == Backend.Unsafe && hasBurstJobs && ecsFields.Any(field => isBurstCompatibleType(field.Type));
			appendGeneratedFor(builder, typeName, fullTypeName, "Dictionary&lt;TKey&gt;");
			builder.AppendLine(accessibility + " sealed class " + typeName + "ECSDictionary<TKey> : global::System.IDisposable");
			builder.AppendLine("{");
			builder.AppendLine("\tprivate readonly global::System.Collections.Generic.Dictionary<TKey, int> mIndexMap;");
			builder.AppendLine("\tprivate readonly " + typeName + "ECSList mValues;");
			builder.AppendLine("\tprivate TKey[] mKeys;");
			builder.AppendLine("\tprivate bool mDisposed;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\tprivate int mVersion;");
			builder.AppendLine("#endif");
			generateDictionaryEntry(builder, typeName, backend, backend == Backend.Unsafe && (ecsFields.Any(field => !field.Type.IsUnmanagedType) || aosFields.Any(field => !field.Type.IsUnmanagedType)));
			generateDictionaryEnumerator(builder, typeName, backend, backend == Backend.Unsafe && (ecsFields.Any(field => !field.Type.IsUnmanagedType) || aosFields.Any(field => !field.Type.IsUnmanagedType)));
			generateDictionaryKeyEnumerable(builder, typeName, hasSpan);
			generateDictionaryValueEnumerable(
				builder,
				typeName,
				backend,
				backend == Backend.Unsafe && (ecsFields.Any(field => !field.Type.IsUnmanagedType) || aosFields.Any(field => !field.Type.IsUnmanagedType)));
			builder.AppendLine("\tpublic int Count");
			builder.AppendLine("\t{");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tget");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\treturn mValues.Count;");
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
			builder.AppendLine("\t\t\treturn mValues.Capacity;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic global::System.Collections.Generic.IEqualityComparer<TKey> Comparer");
			builder.AppendLine("\t{");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tget");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\treturn mIndexMap.Comparer;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic KeyEnumerable Keys");
			builder.AppendLine("\t{");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tget");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\treturn new KeyEnumerable(this);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic ValueEnumerable Values");
			builder.AppendLine("\t{");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tget");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\treturn new ValueEnumerable(this);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			if (hasBurstIntegration)
			{
				builder.AppendLine("\tpublic " + typeName + "ECSList.BurstView GetBurstView()");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\treturn mValues.GetBurstView();");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic global::Unity.Jobs.JobHandle ScheduleBurst<TJob>(TJob job, int innerloopBatchCount = 64) where TJob : struct, global::Unity.Jobs.IJobParallelFor");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\treturn mValues.ScheduleBurst(job, innerloopBatchCount);");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic global::Unity.Jobs.JobHandle ScheduleBurst<TJob>(TJob job, int innerloopBatchCount, global::Unity.Jobs.JobHandle dependsOn) where TJob : struct, global::Unity.Jobs.IJobParallelFor");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\treturn mValues.ScheduleBurst(job, innerloopBatchCount, dependsOn);");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic global::Unity.Jobs.JobHandle GetBurstDependency()");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\treturn mValues.GetBurstDependency();");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic void RegisterBurstJob(global::Unity.Jobs.JobHandle handle)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tmValues.RegisterBurstJob(handle);");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic void CompleteBurstJobs()");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tmValues.CompleteBurstJobs();");
				builder.AppendLine("\t}");
			}
			builder.AppendLine("\tpublic " + typeName + "Ref this[TKey key]");
			builder.AppendLine("\t{");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tget");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\treturn mValues[mIndexMap[key]];");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic " + typeName + "ECSDictionary(int capacity = 4) : this(capacity, null)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic " + typeName + "ECSDictionary(global::System.Collections.Generic.IEqualityComparer<TKey> comparer) : this(4, comparer)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic " + typeName + "ECSDictionary(int capacity, global::System.Collections.Generic.IEqualityComparer<TKey> comparer)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (capacity < 1)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tcapacity = 1;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tmIndexMap = new global::System.Collections.Generic.Dictionary<TKey, int>(capacity, comparer);");
			builder.AppendLine("\t\tmKeys = new TKey[capacity];");
			builder.AppendLine("\t\tmValues = new " + typeName + "ECSList(capacity);");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tpublic Enumerator GetEnumerator()");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\treturn new Enumerator(this);");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic void Add(TKey key, " + fullTypeName + " value)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\taddValue(key, value);");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic bool TryAdd(TKey key, " + fullTypeName + " value)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\treturn tryAddValue(key, value);");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tpublic bool ContainsKey(TKey key)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\treturn mIndexMap.ContainsKey(key);");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tpublic bool TryGetValue(TKey key, out " + typeName + "Ref value)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tif (!mIndexMap.TryGetValue(key, out int index))");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tvalue = default;");
			builder.AppendLine("\t\t\treturn false;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tvalue = mValues[index];");
			builder.AppendLine("\t\treturn true;");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tpublic int GetIndex(TKey key)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\treturn mIndexMap[key];");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tpublic bool TryGetIndex(TKey key, out int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\treturn mIndexMap.TryGetValue(key, out index);");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic bool Remove(TKey key)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tif (!mIndexMap.Remove(key, out int removeIndex))");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn false;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tint lastIndex = mValues.Count - 1;");
			builder.AppendLine("\t\tTKey lastKey = mKeys[lastIndex];");
			builder.AppendLine("\t\tmValues.RemoveAtSwapBack(removeIndex);");
			builder.AppendLine("\t\tif (removeIndex != lastIndex)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmKeys[removeIndex] = lastKey;");
			builder.AppendLine("\t\t\tmIndexMap[lastKey] = removeIndex;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tmKeys[lastIndex] = default(TKey);");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t++mVersion;");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\treturn true;");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic void Clear()");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tint count = mValues.Count;");
			builder.AppendLine("\t\tif (count == 0)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tglobal::System.Array.Clear(mKeys, 0, count);");
			builder.AppendLine("\t\tmIndexMap.Clear();");
			builder.AppendLine("\t\tmValues.Clear();");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t++mVersion;");
			builder.AppendLine("#endif");
			builder.AppendLine("\t}");
			generateExtendedDictionaryMethods(builder, typeName, fullTypeName);
			generateDictionaryFieldMethods(builder, typeName, ecsFields, backend);
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tpublic TKey getKeyAt(int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateIndex(index);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\treturn mKeys[index];");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tpublic " + typeName + "Ref getValueAt(int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateIndex(index);");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\treturn mValues[index];");
			builder.AppendLine("\t}");
			foreach (IFieldSymbol field in ecsFields)
			{
				string fieldAccessibility = field.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
				string columnType = typeName + "ECSList." + getColumnTypeName(field.Name);
				string methodName = getColumnMethodName(field.Name);
				appendAggressiveInlining(builder, 1);
				builder.AppendLine("\t" + fieldAccessibility + " " + columnType + " " + methodName + "()");
				builder.AppendLine("\t{");
				builder.AppendLine("#if UNITY_EDITOR");
				builder.AppendLine("\t\tvalidateAlive();");
				builder.AppendLine("#endif");
				builder.AppendLine("\t\treturn mValues." + methodName + "();");
				builder.AppendLine("\t}");
			}
			builder.AppendLine("\tpublic void Dispose()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (mDisposed)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t++mVersion;");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tmDisposed = true;");
			builder.AppendLine("\t\tmIndexMap.Clear();");
			builder.AppendLine("\t\tif (mKeys != null)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tglobal::System.Array.Clear(mKeys, 0, mKeys.Length);");
			builder.AppendLine("\t\t\tmKeys = null;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tmValues.Dispose();");
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate int addValue(TKey key, " + fullTypeName + " value)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tint index = mValues.Count;");
			builder.AppendLine("\t\tmIndexMap.Add(key, index);");
			builder.AppendLine("\t\ttry");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tensureKeyCapacity(index + 1);");
			builder.AppendLine("\t\t\tmKeys[index] = key;");
			builder.AppendLine("\t\t\tmValues.Add(value);");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\t++mVersion;");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\treturn index;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tcatch");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tif (mKeys != null && (uint)index < (uint)mKeys.Length)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmKeys[index] = default(TKey);");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tmIndexMap.Remove(key);");
			builder.AppendLine("\t\t\tthrow;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate bool tryAddValue(TKey key, " + fullTypeName + " value)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tint index = mValues.Count;");
			builder.AppendLine("\t\tif (!mIndexMap.TryAdd(key, index))");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn false;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\ttry");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tensureKeyCapacity(index + 1);");
			builder.AppendLine("\t\t\tmKeys[index] = key;");
			builder.AppendLine("\t\t\tmValues.Add(value);");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\t++mVersion;");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\treturn true;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tcatch");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tif (mKeys != null && (uint)index < (uint)mKeys.Length)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmKeys[index] = default(TKey);");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tmIndexMap.Remove(key);");
			builder.AppendLine("\t\t\tthrow;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void ensureKeyCapacity(int minimumCapacity)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (mKeys.Length >= minimumCapacity)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tint newCapacity = mKeys.Length;");
			builder.AppendLine("\t\tif (newCapacity < 1)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tnewCapacity = 1;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\twhile (newCapacity < minimumCapacity)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tnewCapacity *= 2;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tglobal::System.Array.Resize(ref mKeys, newCapacity);");
			builder.AppendLine("\t}");
			builder.AppendLine("#if UNITY_EDITOR");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate void validateAlive()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (mDisposed)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSDictionary\");");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate void validateIndex(int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("\t\tif ((uint)index >= (uint)mValues.Count)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.ArgumentOutOfRangeException(nameof(index), \"" + typeName + "ECSDictionary索引越界,Index:\" + index + \",Count:\" + mValues.Count);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate void validateEnumeratorVersion(int version)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("\t\tif (version != mVersion)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.InvalidOperationException(\"" + typeName + "ECSDictionary在遍历期间发生了结构变化\");");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate void validateEnumeratorCurrent(int index, int count, int version)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tvalidateEnumeratorVersion(version);");
			builder.AppendLine("\t\tif ((uint)index >= (uint)count)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.InvalidOperationException(\"" + typeName + "ECSDictionary Enumerator的Current当前无效\");");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("#endif");
			builder.AppendLine("}");
		}
		private static void generateDictionaryFieldMethods(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, Backend backend)
		{
			foreach (IFieldSymbol field in ecsFields)
			{
				string fieldType = getTypeName(field.Type);
				string suffix = getSortMethodSuffix(field.Name);
				string access = fieldAccess(field);
				string fieldExpression;
				string unsafeModifier = backend == Backend.Unsafe && field.Type.IsUnmanagedType ? "unsafe " : string.Empty;
				if (backend == Backend.Unsafe)
				{
					fieldExpression = field.Type.IsUnmanagedType ? "mValues.getDictionaryStorage()->" + access + "[index]" : "mValues.getDictionaryManagedStorage()." + access + "[index]";
				}
				else if (backend == Backend.SafeSpan)
				{
					fieldExpression = "mValues.getDictionaryStorage()[0]." + access + "[index]";
				}
				else
				{
					fieldExpression = typeName + "StorageRegistry.get_" + field.Name + "(mValues.getDictionaryStorageID(), index)";
				}
				appendAggressiveInlining(builder, 1);
				builder.AppendLine("\tpublic " + unsafeModifier + fieldType + " GetValueBy" + suffix + "(TKey key)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tif (mDisposed)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSDictionary\");");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tif (!mIndexMap.TryGetValue(key, out int index))");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.Collections.Generic.KeyNotFoundException();");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\treturn " + fieldExpression + ";");
				builder.AppendLine("\t}");
				appendAggressiveInlining(builder, 1);
				builder.AppendLine("\tpublic " + unsafeModifier + "bool TryGetValueBy" + suffix + "(TKey key, out " + fieldType + " value)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tif (mDisposed)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSDictionary\");");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tif (!mIndexMap.TryGetValue(key, out int index))");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tvalue = default(" + fieldType + ");");
				builder.AppendLine("\t\t\treturn false;");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tvalue = " + fieldExpression + ";");
				builder.AppendLine("\t\treturn true;");
				builder.AppendLine("\t}");
				appendAggressiveInlining(builder, 1);
				builder.AppendLine("\tpublic " + unsafeModifier + "void SetValueBy" + suffix + "(TKey key, " + fieldType + " value)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tif (mDisposed)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSDictionary\");");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tif (!mIndexMap.TryGetValue(key, out int index))");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.Collections.Generic.KeyNotFoundException();");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\t" + fieldExpression + " = value;");
				builder.AppendLine("\t}");
				appendAggressiveInlining(builder, 1);
				builder.AppendLine("\tpublic " + unsafeModifier + "bool TrySetValueBy" + suffix + "(TKey key, " + fieldType + " value)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tif (mDisposed)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSDictionary\");");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tif (!mIndexMap.TryGetValue(key, out int index))");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\treturn false;");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\t" + fieldExpression + " = value;");
				builder.AppendLine("\t\treturn true;");
				builder.AppendLine("\t}");
			}
		}
		private static void generateExtendedDictionaryMethods(StringBuilder builder, string typeName, string fullTypeName)
		{
			string source = @"
	public void SetValue(TKey key, __ECS_TYPE__ value)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_DICT__"");
		}
		if (!mIndexMap.TryGetValue(key, out int index))
		{
			throw new global::System.Collections.Generic.KeyNotFoundException();
		}
		mValues.Set(index, value);
	}
	public bool TrySetValue(TKey key, __ECS_TYPE__ value)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_DICT__"");
		}
		if (!mIndexMap.TryGetValue(key, out int index))
		{
			return false;
		}
		mValues.Set(index, value);
		return true;
	}
	public __ECS_REF__ SetOrAdd(TKey key, __ECS_TYPE__ value)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_DICT__"");
		}
		if (mIndexMap.TryGetValue(key, out int index))
		{
			mValues.Set(index, value);
			return mValues[index];
		}
		int addedIndex = addValue(key, value);
		return mValues[addedIndex];
	}
	public __ECS_REF__ GetOrAdd(TKey key)
	{
		return GetOrAdd(key, default(__ECS_TYPE__));
	}
	public __ECS_REF__ GetOrAdd(TKey key, __ECS_TYPE__ value)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_DICT__"");
		}
		if (mIndexMap.TryGetValue(key, out int index))
		{
			return mValues[index];
		}
		int addedIndex = addValue(key, value);
		return mValues[addedIndex];
	}
	public int GetOrAddIndex(TKey key)
	{
		return GetOrAddIndex(key, default(__ECS_TYPE__));
	}
	public int GetOrAddIndex(TKey key, __ECS_TYPE__ value)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_DICT__"");
		}
		if (mIndexMap.TryGetValue(key, out int index))
		{
			return index;
		}
		return addValue(key, value);
	}
	public int GetOrAddIndex(TKey key, __ECS_TYPE__ value, out bool added)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_DICT__"");
		}
		if (mIndexMap.TryGetValue(key, out int index))
		{
			added = false;
			return index;
		}
		added = true;
		return addValue(key, value);
	}
	public bool ContainsValue(__ECS_TYPE__ value)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_DICT__"");
		}
		global::System.Collections.Generic.EqualityComparer<__ECS_TYPE__> comparer = global::System.Collections.Generic.EqualityComparer<__ECS_TYPE__>.Default;
		int count = mValues.Count;
		for (int i = 0; i < count; ++i)
		{
			if (comparer.Equals(mValues.Get(i), value))
			{
				return true;
			}
		}
		return false;
	}
	public bool Remove(TKey key, out __ECS_TYPE__ value)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_DICT__"");
		}
		if (!mIndexMap.Remove(key, out int removeIndex))
		{
			value = default(__ECS_TYPE__);
			return false;
		}
		value = mValues.Get(removeIndex);
		int lastIndex = mValues.Count - 1;
		TKey lastKey = mKeys[lastIndex];
		mValues.RemoveAtSwapBack(removeIndex);
		if (removeIndex != lastIndex)
		{
			mKeys[removeIndex] = lastKey;
			mIndexMap[lastKey] = removeIndex;
		}
		mKeys[lastIndex] = default(TKey);
#if UNITY_EDITOR
		++mVersion;
#endif
		return true;
	}
	public int EnsureCapacity(int capacity)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_DICT__"");
		}
		if (capacity < 0)
		{
			throw new global::System.ArgumentOutOfRangeException(nameof(capacity));
		}
		int oldValueCapacity = mValues.Capacity;
		int oldKeyCapacity = mKeys.Length;
		mIndexMap.EnsureCapacity(capacity);
		ensureKeyCapacity(capacity);
		int result = mValues.EnsureCapacity(capacity);
#if UNITY_EDITOR
		if (result != oldValueCapacity || mKeys.Length != oldKeyCapacity)
		{
			++mVersion;
		}
#endif
		return result;
	}
	public void TrimExcess()
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_DICT__"");
		}
		int oldValueCapacity = mValues.Capacity;
		int oldKeyCapacity = mKeys.Length;
		mIndexMap.TrimExcess();
		mValues.TrimExcess();
		int targetCapacity = mValues.Capacity;
		if (mKeys.Length != targetCapacity)
		{
			global::System.Array.Resize(ref mKeys, targetCapacity);
		}
#if UNITY_EDITOR
		if (mValues.Capacity != oldValueCapacity || mKeys.Length != oldKeyCapacity)
		{
			++mVersion;
		}
#endif
	}
";
			builder.Append(source.TrimStart('\r', '\n').Replace("__ECS_TYPE__", fullTypeName).Replace("__ECS_REF__", typeName + "Ref").Replace("__ECS_DICT__", typeName + "ECSDictionary"));
		}
		private static void generateDictionaryEntry(StringBuilder builder, string typeName, Backend backend, bool unsafeHasManagedStorage)
		{
			builder.AppendLine("#if UNITY_EDITOR");
			generateDictionaryEntryEditor(builder, typeName);
			builder.AppendLine("#else");
			if (backend == Backend.Unsafe)
			{
				generateUnsafeDictionaryEntryPlayer(builder, typeName, unsafeHasManagedStorage);
			}
			else if (backend == Backend.SafeSpan)
			{
				generateSafeSpanDictionaryEntryPlayer(builder, typeName);
			}
			else
			{
				generateSafeRegistryDictionaryEntryPlayer(builder, typeName);
			}
			builder.AppendLine("#endif");
		}
		private static void generateDictionaryEntryEditor(StringBuilder builder, string typeName)
		{
			builder.AppendLine("\tpublic ref struct Entry");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate readonly TKey mKey;");
			builder.AppendLine("\t\tprivate " + typeName + "Ref mValue;");
			builder.AppendLine("\t\tprivate readonly " + typeName + "ECSDictionary<TKey> mOwner;");
			builder.AppendLine("\t\tprivate readonly int mVersion;");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tinternal Entry(TKey key, " + typeName + "Ref value, " + typeName + "ECSDictionary<TKey> owner, int version)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmKey = key;");
			builder.AppendLine("\t\t\tmValue = value;");
			builder.AppendLine("\t\t\tmOwner = owner;");
			builder.AppendLine("\t\t\tmVersion = version;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic TKey Key");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmOwner.validateEnumeratorVersion(mVersion);");
			builder.AppendLine("\t\t\t\treturn mKey;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic " + typeName + "Ref Value");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmOwner.validateEnumeratorVersion(mVersion);");
			builder.AppendLine("\t\t\t\treturn mValue;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateUnsafeDictionaryEntryPlayer(StringBuilder builder, string typeName, bool hasManagedStorage)
		{
			builder.AppendLine("\tpublic unsafe ref struct Entry");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate readonly TKey[] mKeys;");
			builder.AppendLine("\t\tprivate readonly " + typeName + "Ref mValue;");
			builder.AppendLine("\t\tprivate readonly int mIndex;");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic Entry(TKey[] keys, " + typeName + "Ref value, int index)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmKeys = keys;");
			builder.AppendLine("\t\t\tmValue = value;");
			builder.AppendLine("\t\t\tmIndex = index;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic TKey Key");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\treturn mKeys[mIndex];");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic " + typeName + "Ref Value");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\treturn mValue;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateSafeSpanDictionaryEntryPlayer(StringBuilder builder, string typeName)
		{
			builder.AppendLine("\tpublic ref struct Entry");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate readonly TKey[] mKeys;");
			builder.AppendLine("\t\tprivate readonly " + typeName + "Ref mValue;");
			builder.AppendLine("\t\tprivate readonly int mIndex;");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic Entry(TKey[] keys, " + typeName + "Ref value, int index)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmKeys = keys;");
			builder.AppendLine("\t\t\tmValue = value;");
			builder.AppendLine("\t\t\tmIndex = index;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic TKey Key");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\treturn mKeys[mIndex];");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic " + typeName + "Ref Value");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\treturn mValue;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateSafeRegistryDictionaryEntryPlayer(StringBuilder builder, string typeName)
		{
			builder.AppendLine("\tpublic readonly struct Entry");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate readonly TKey[] mKeys;");
			builder.AppendLine("\t\tprivate readonly " + typeName + "Ref mValue;");
			builder.AppendLine("\t\tprivate readonly int mIndex;");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic Entry(TKey[] keys, " + typeName + "Ref value, int index)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmKeys = keys;");
			builder.AppendLine("\t\t\tmValue = value;");
			builder.AppendLine("\t\t\tmIndex = index;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic TKey Key");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\treturn mKeys[mIndex];");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic " + typeName + "Ref Value");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\treturn mValue;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateDictionaryEnumerator(StringBuilder builder, string typeName, Backend backend, bool unsafeHasManagedStorage)
		{
			builder.AppendLine("#if UNITY_EDITOR");
			generateDictionaryEnumeratorEditor(builder, typeName);
			builder.AppendLine("#else");
			if (backend == Backend.Unsafe)
			{
				generateUnsafeDictionaryEnumeratorPlayer(builder, typeName, unsafeHasManagedStorage);
			}
			else if (backend == Backend.SafeSpan)
			{
				generateSafeSpanDictionaryEnumeratorPlayer(builder, typeName);
			}
			else
			{
				generateSafeRegistryDictionaryEnumeratorPlayer(builder, typeName);
			}
			builder.AppendLine("#endif");
		}
		private static void generateDictionaryEnumeratorEditor(StringBuilder builder, string typeName)
		{
			builder.AppendLine("\tpublic ref struct Enumerator");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate readonly TKey[] mKeys;");
			builder.AppendLine("\t\tprivate readonly " + typeName + "ECSList mValues;");
			builder.AppendLine("\t\tprivate readonly int mCount;");
			builder.AppendLine("\t\tprivate int mIndex;");
			builder.AppendLine("\t\tprivate Entry mCurrent;");
			builder.AppendLine("\t\tprivate readonly " + typeName + "ECSDictionary<TKey> mOwner;");
			builder.AppendLine("\t\tprivate readonly int mVersion;");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tinternal Enumerator(" + typeName + "ECSDictionary<TKey> owner)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmKeys = owner.mKeys;");
			builder.AppendLine("\t\t\tmValues = owner.mValues;");
			builder.AppendLine("\t\t\tmCount = owner.mValues.Count;");
			builder.AppendLine("\t\t\tmIndex = -1;");
			builder.AppendLine("\t\t\tmCurrent = default;");
			builder.AppendLine("\t\t\tmOwner = owner;");
			builder.AppendLine("\t\t\tmVersion = owner.mVersion;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic Entry Current");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmOwner.validateEnumeratorCurrent(mIndex, mCount, mVersion);");
			builder.AppendLine("\t\t\t\treturn mCurrent;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic bool MoveNext()");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmOwner.validateEnumeratorVersion(mVersion);");
			builder.AppendLine("\t\t\tint index = mIndex + 1;");
			builder.AppendLine("\t\t\tif ((uint)index >= (uint)mCount)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmIndex = mCount;");
			builder.AppendLine("\t\t\t\tmCurrent = default;");
			builder.AppendLine("\t\t\t\treturn false;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tmIndex = index;");
			builder.AppendLine("\t\t\tmCurrent = new Entry(mKeys[index], mValues[index], mOwner, mVersion);");
			builder.AppendLine("\t\t\treturn true;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateUnsafeDictionaryEnumeratorPlayer(StringBuilder builder, string typeName, bool hasManagedStorage)
		{
			builder.AppendLine("\tpublic unsafe ref struct Enumerator");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate readonly TKey[] mKeys;");
			builder.AppendLine("\t\tprivate readonly " + typeName + "Storage* mStorage;");
			if (hasManagedStorage)
			{
				builder.AppendLine("\t\tprivate readonly " + typeName + "ManagedStorage mManagedStorage;");
			}
			builder.AppendLine("\t\tprivate readonly int mCount;");
			builder.AppendLine("\t\tprivate int mIndex;");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic Enumerator(" + typeName + "ECSDictionary<TKey> owner)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmKeys = owner.mKeys;");
			builder.AppendLine("\t\t\tmStorage = owner.mValues.getDictionaryStorage();");
			if (hasManagedStorage)
			{
				builder.AppendLine("\t\t\tmManagedStorage = owner.mValues.getDictionaryManagedStorage();");
			}
			builder.AppendLine("\t\t\tmCount = owner.mValues.Count;");
			builder.AppendLine("\t\t\tmIndex = -1;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic Entry Current");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\treturn new Entry(mKeys, new " + typeName + "Ref(mStorage" + (hasManagedStorage ? ", mManagedStorage" : string.Empty) + ", mIndex), mIndex);");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic bool MoveNext()");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tint index = mIndex + 1;");
			builder.AppendLine("\t\t\tif ((uint)index >= (uint)mCount)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmIndex = mCount;");
			builder.AppendLine("\t\t\t\treturn false;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tmIndex = index;");
			builder.AppendLine("\t\t\treturn true;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateSafeSpanDictionaryEnumeratorPlayer(StringBuilder builder, string typeName)
		{
			builder.AppendLine("\tpublic ref struct Enumerator");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate readonly TKey[] mKeys;");
			builder.AppendLine("\t\tprivate readonly " + typeName + "Storage[] mStorage;");
			builder.AppendLine("\t\tprivate readonly int mCount;");
			builder.AppendLine("\t\tprivate int mIndex;");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic Enumerator(" + typeName + "ECSDictionary<TKey> owner)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmKeys = owner.mKeys;");
			builder.AppendLine("\t\t\tmStorage = owner.mValues.getDictionaryStorage();");
			builder.AppendLine("\t\t\tmCount = owner.mValues.Count;");
			builder.AppendLine("\t\t\tmIndex = -1;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic Entry Current");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\treturn new Entry(mKeys, new " + typeName + "Ref(mStorage, mIndex), mIndex);");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic bool MoveNext()");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tint index = mIndex + 1;");
			builder.AppendLine("\t\t\tif ((uint)index >= (uint)mCount)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmIndex = mCount;");
			builder.AppendLine("\t\t\t\treturn false;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tmIndex = index;");
			builder.AppendLine("\t\t\treturn true;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateSafeRegistryDictionaryEnumeratorPlayer(StringBuilder builder, string typeName)
		{
			builder.AppendLine("\tpublic struct Enumerator");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate readonly TKey[] mKeys;");
			builder.AppendLine("\t\tprivate readonly int mStorageID;");
			builder.AppendLine("\t\tprivate readonly int mCount;");
			builder.AppendLine("\t\tprivate int mIndex;");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic Enumerator(" + typeName + "ECSDictionary<TKey> owner)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmKeys = owner.mKeys;");
			builder.AppendLine("\t\t\tmStorageID = owner.mValues.getDictionaryStorageID();");
			builder.AppendLine("\t\t\tmCount = owner.mValues.Count;");
			builder.AppendLine("\t\t\tmIndex = -1;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic Entry Current");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\treturn new Entry(mKeys, new " + typeName + "Ref(mStorageID, mIndex), mIndex);");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic bool MoveNext()");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tint index = mIndex + 1;");
			builder.AppendLine("\t\t\tif ((uint)index >= (uint)mCount)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmIndex = mCount;");
			builder.AppendLine("\t\t\t\treturn false;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tmIndex = index;");
			builder.AppendLine("\t\t\treturn true;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
		}
		private static void generateDictionaryKeyEnumerable(StringBuilder builder, string typeName, bool hasSpan)
		{
			builder.AppendLine("\tpublic readonly struct KeyEnumerable");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate readonly " + typeName + "ECSDictionary<TKey> mOwner;");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic KeyEnumerable(" + typeName + "ECSDictionary<TKey> owner)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmOwner = owner;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic int Count");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\t\tmOwner.validateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\t\treturn mOwner.mValues.Count;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			if (hasSpan)
			{
				builder.AppendLine("#if UNITY_EDITOR");
				appendAggressiveInlining(builder, 2);
				builder.AppendLine("\t\tpublic KeyEnumerator GetEnumerator()");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tmOwner.validateAlive();");
				builder.AppendLine("\t\t\treturn new KeyEnumerator(mOwner);");
				builder.AppendLine("\t\t}");
				builder.AppendLine("#else");
				appendAggressiveInlining(builder, 2);
				builder.AppendLine("\t\tpublic global::System.ReadOnlySpan<TKey>.Enumerator GetEnumerator()");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\treturn new global::System.ReadOnlySpan<TKey>(mOwner.mKeys, 0, mOwner.mValues.Count).GetEnumerator();");
				builder.AppendLine("\t\t}");
				builder.AppendLine("#endif");
			}
			else
			{
				appendAggressiveInlining(builder, 2);
				builder.AppendLine("\t\tpublic KeyEnumerator GetEnumerator()");
				builder.AppendLine("\t\t{");
				builder.AppendLine("#if UNITY_EDITOR");
				builder.AppendLine("\t\t\tmOwner.validateAlive();");
				builder.AppendLine("#endif");
				builder.AppendLine("\t\t\treturn new KeyEnumerator(mOwner);");
				builder.AppendLine("\t\t}");
			}
			builder.AppendLine("\t}");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\tpublic struct KeyEnumerator");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate readonly " + typeName + "ECSDictionary<TKey> mOwner;");
			builder.AppendLine("\t\tprivate readonly int mCount;");
			builder.AppendLine("\t\tprivate int mIndex;");
			builder.AppendLine("\t\tprivate readonly int mVersion;");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic KeyEnumerator(" + typeName + "ECSDictionary<TKey> owner)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmOwner = owner;");
			builder.AppendLine("\t\t\tmCount = owner.mValues.Count;");
			builder.AppendLine("\t\t\tmIndex = -1;");
			builder.AppendLine("\t\t\tmVersion = owner.mVersion;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic TKey Current");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmOwner.validateEnumeratorCurrent(mIndex, mCount, mVersion);");
			builder.AppendLine("\t\t\t\treturn mOwner.mKeys[mIndex];");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic bool MoveNext()");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmOwner.validateEnumeratorVersion(mVersion);");
			builder.AppendLine("\t\t\tint nextIndex = mIndex + 1;");
			builder.AppendLine("\t\t\tif ((uint)nextIndex < (uint)mCount)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmIndex = nextIndex;");
			builder.AppendLine("\t\t\t\treturn true;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tmIndex = mCount;");
			builder.AppendLine("\t\t\treturn false;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			if (!hasSpan)
			{
				builder.AppendLine("#else");
				builder.AppendLine("\tpublic struct KeyEnumerator");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tprivate readonly TKey[] mKeys;");
				builder.AppendLine("\t\tprivate readonly int mCount;");
				builder.AppendLine("\t\tprivate int mIndex;");
				appendAggressiveInlining(builder, 2);
				builder.AppendLine("\t\tpublic KeyEnumerator(" + typeName + "ECSDictionary<TKey> owner)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tmKeys = owner.mKeys;");
				builder.AppendLine("\t\t\tmCount = owner.mValues.Count;");
				builder.AppendLine("\t\t\tmIndex = -1;");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tpublic TKey Current");
				builder.AppendLine("\t\t{");
				appendAggressiveInlining(builder, 3);
				builder.AppendLine("\t\t\tget");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\treturn mKeys[mIndex];");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t}");
				appendAggressiveInlining(builder, 2);
				builder.AppendLine("\t\tpublic bool MoveNext()");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tint nextIndex = mIndex + 1;");
				builder.AppendLine("\t\t\tif ((uint)nextIndex < (uint)mCount)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\tmIndex = nextIndex;");
				builder.AppendLine("\t\t\t\treturn true;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t\tmIndex = mCount;");
				builder.AppendLine("\t\t\treturn false;");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t}");
			}
			builder.AppendLine("#endif");
		}
		private static void generateDictionaryValueEnumerable(StringBuilder builder, string typeName, Backend backend, bool unsafeHasManagedStorage)
		{
			builder.AppendLine("\tpublic readonly struct ValueEnumerable");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate readonly " + typeName + "ECSDictionary<TKey> mOwner;");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic ValueEnumerable(" + typeName + "ECSDictionary<TKey> owner)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmOwner = owner;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic int Count");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\t\tmOwner.validateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\t\treturn mOwner.mValues.Count;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic ValueEnumerator GetEnumerator()");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tmOwner.validateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\treturn new ValueEnumerator(mOwner);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\tpublic struct ValueEnumerator");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate readonly " + typeName + "ECSDictionary<TKey> mOwner;");
			builder.AppendLine("\t\tprivate readonly int mCount;");
			builder.AppendLine("\t\tprivate int mIndex;");
			builder.AppendLine("\t\tprivate readonly int mVersion;");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic ValueEnumerator(" + typeName + "ECSDictionary<TKey> owner)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmOwner = owner;");
			builder.AppendLine("\t\t\tmCount = owner.mValues.Count;");
			builder.AppendLine("\t\t\tmIndex = -1;");
			builder.AppendLine("\t\t\tmVersion = owner.mVersion;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tpublic " + typeName + "Ref Current");
			builder.AppendLine("\t\t{");
			appendAggressiveInlining(builder, 3);
			builder.AppendLine("\t\t\tget");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmOwner.validateEnumeratorCurrent(mIndex, mCount, mVersion);");
			builder.AppendLine("\t\t\t\treturn mOwner.mValues[mIndex];");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t}");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic bool MoveNext()");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmOwner.validateEnumeratorVersion(mVersion);");
			builder.AppendLine("\t\t\tint nextIndex = mIndex + 1;");
			builder.AppendLine("\t\t\tif ((uint)nextIndex < (uint)mCount)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmIndex = nextIndex;");
			builder.AppendLine("\t\t\t\treturn true;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tmIndex = mCount;");
			builder.AppendLine("\t\t\treturn false;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("#else");
			if (backend == Backend.Unsafe)
			{
				builder.AppendLine("\tpublic unsafe struct ValueEnumerator");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tprivate readonly " + typeName + "Storage* mStorage;");
				if (unsafeHasManagedStorage)
				{
					builder.AppendLine("\t\tprivate readonly " + typeName + "ManagedStorage mManagedStorage;");
				}
				builder.AppendLine("\t\tprivate readonly int mCount;");
				builder.AppendLine("\t\tprivate int mIndex;");
				appendAggressiveInlining(builder, 2);
				builder.AppendLine("\t\tpublic ValueEnumerator(" + typeName + "ECSDictionary<TKey> owner)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tmStorage = owner.mValues.getDictionaryStorage();");
				if (unsafeHasManagedStorage)
				{
					builder.AppendLine("\t\t\tmManagedStorage = owner.mValues.getDictionaryManagedStorage();");
				}
				builder.AppendLine("\t\t\tmCount = owner.mValues.Count;");
				builder.AppendLine("\t\t\tmIndex = -1;");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tpublic " + typeName + "Ref Current");
				builder.AppendLine("\t\t{");
				appendAggressiveInlining(builder, 3);
				builder.AppendLine("\t\t\tget");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\treturn new " + typeName + "Ref(mStorage" + (unsafeHasManagedStorage ? ", mManagedStorage" : string.Empty) + ", mIndex);");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t}");
				appendDictionarySimpleMoveNext(builder);
				builder.AppendLine("\t}");
			}
			else if (backend == Backend.SafeSpan)
			{
				builder.AppendLine("\tpublic ref struct ValueEnumerator");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tprivate readonly " + typeName + "Storage[] mStorage;");
				builder.AppendLine("\t\tprivate readonly int mCount;");
				builder.AppendLine("\t\tprivate int mIndex;");
				appendAggressiveInlining(builder, 2);
				builder.AppendLine("\t\tpublic ValueEnumerator(" + typeName + "ECSDictionary<TKey> owner)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tmStorage = owner.mValues.getDictionaryStorage();");
				builder.AppendLine("\t\t\tmCount = owner.mValues.Count;");
				builder.AppendLine("\t\t\tmIndex = -1;");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tpublic " + typeName + "Ref Current");
				builder.AppendLine("\t\t{");
				appendAggressiveInlining(builder, 3);
				builder.AppendLine("\t\t\tget");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\treturn new " + typeName + "Ref(mStorage, mIndex);");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t}");
				appendDictionarySimpleMoveNext(builder);
				builder.AppendLine("\t}");
			}
			else
			{
				builder.AppendLine("\tpublic struct ValueEnumerator");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tprivate readonly int mStorageID;");
				builder.AppendLine("\t\tprivate readonly int mCount;");
				builder.AppendLine("\t\tprivate int mIndex;");
				appendAggressiveInlining(builder, 2);
				builder.AppendLine("\t\tpublic ValueEnumerator(" + typeName + "ECSDictionary<TKey> owner)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tmStorageID = owner.mValues.getDictionaryStorageID();");
				builder.AppendLine("\t\t\tmCount = owner.mValues.Count;");
				builder.AppendLine("\t\t\tmIndex = -1;");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tpublic " + typeName + "Ref Current");
				builder.AppendLine("\t\t{");
				appendAggressiveInlining(builder, 3);
				builder.AppendLine("\t\t\tget");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\treturn new " + typeName + "Ref(mStorageID, mIndex);");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t}");
				appendDictionarySimpleMoveNext(builder);
				builder.AppendLine("\t}");
			}
			builder.AppendLine("#endif");
		}
		private static void appendDictionarySimpleMoveNext(StringBuilder builder)
		{
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tpublic bool MoveNext()");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tint nextIndex = mIndex + 1;");
			builder.AppendLine("\t\t\tif ((uint)nextIndex < (uint)mCount)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tmIndex = nextIndex;");
			builder.AppendLine("\t\t\t\treturn true;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tmIndex = mCount;");
			builder.AppendLine("\t\t\treturn false;");
			builder.AppendLine("\t\t}");
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
			bool hasManagedAoS = aosFields.Any(field => !field.Type.IsUnmanagedType);
			builder.AppendLine("internal unsafe struct " + typeName + "Storage");
			builder.AppendLine("{");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\tpublic " + getTypeName(field.Type) + "* " + fieldAccess(field) + ";");
				}
			}
			if (aosFields.Count > 0 && !hasManagedAoS)
			{
				builder.AppendLine("\tpublic " + typeName + "AoSBlock* mAoS;");
			}
			builder.AppendLine("}");
		}
		private static void generateUnsafeManagedStorage(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			bool hasManagedECS = ecsFields.Any(field => !field.Type.IsUnmanagedType);
			bool hasManagedAoS = aosFields.Any(field => !field.Type.IsUnmanagedType);
			if (!hasManagedECS && !hasManagedAoS)
			{
				return;
			}
			builder.AppendLine("internal sealed class " + typeName + "ManagedStorage");
			builder.AppendLine("{");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (!field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\tpublic " + getTypeName(field.Type) + "[] " + fieldAccess(field) + ";");
				}
			}
			if (hasManagedAoS)
			{
				builder.AppendLine("\tpublic " + typeName + "AoSBlock[] mAoS;");
			}
			builder.AppendLine("\tpublic " + typeName + "ManagedStorage(int capacity)");
			builder.AppendLine("\t{");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (!field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\t" + fieldAccess(field) + " = new " + getTypeName(field.Type) + "[capacity];");
				}
			}
			if (hasManagedAoS)
			{
				builder.AppendLine("\t\tmAoS = new " + typeName + "AoSBlock[capacity];");
			}
			builder.AppendLine("\t}");
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
			bool hasManagedStorage = ecsFields.Any(field => !field.Type.IsUnmanagedType) || aosFields.Any(field => !field.Type.IsUnmanagedType);
			bool hasManagedAoS = aosFields.Any(field => !field.Type.IsUnmanagedType);
			builder.AppendLine(accessibility + " unsafe ref struct " + typeName + "Ref");
			builder.AppendLine("{");
			builder.AppendLine("\tprivate readonly " + typeName + "Storage* mStorage;");
			if (hasManagedStorage)
			{
				builder.AppendLine("\tprivate readonly " + typeName + "ManagedStorage mManagedStorage;");
			}
			builder.AppendLine("\tprivate readonly int mIndex;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\tprivate readonly " + typeName + "ECSList mOwner;");
			builder.AppendLine("\tprivate readonly int mGeneration;");
			builder.AppendLine("#endif");
			builder.AppendLine("#if UNITY_EDITOR");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tinternal " + typeName + "Ref(" + typeName + "Storage* storage" + (hasManagedStorage ? ", " + typeName + "ManagedStorage managedStorage" : string.Empty) + ", int index, " + typeName + "ECSList owner, int generation)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tmStorage = storage;");
			if (hasManagedStorage)
			{
				builder.AppendLine("\t\tmManagedStorage = managedStorage;");
			}
			builder.AppendLine("\t\tmIndex = index;");
			builder.AppendLine("\t\tmOwner = owner;");
			builder.AppendLine("\t\tmGeneration = generation;");
			builder.AppendLine("\t}");
			builder.AppendLine("#else");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tinternal " + typeName + "Ref(" + typeName + "Storage* storage" + (hasManagedStorage ? ", " + typeName + "ManagedStorage managedStorage" : string.Empty) + ", int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tmStorage = storage;");
			if (hasManagedStorage)
			{
				builder.AppendLine("\t\tmManagedStorage = managedStorage;");
			}
			builder.AppendLine("\t\tmIndex = index;");
			builder.AppendLine("\t}");
			builder.AppendLine("#endif");
			foreach (IFieldSymbol field in ecsFields)
			{
				generateUnsafeRefProperty(builder, field, false, hasManagedAoS);
			}
			foreach (IFieldSymbol field in aosFields)
			{
				generateUnsafeRefProperty(builder, field, true, hasManagedAoS);
			}
			builder.AppendLine("}");
		}
		private static void generateUnsafeRefProperty(StringBuilder builder, IFieldSymbol field, bool aos, bool managedAoS)
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
				if (managedAoS)
				{
					builder.AppendLine("\t\t\treturn ref mManagedStorage.mAoS[mIndex]." + fieldAccess(field) + ";");
				}
				else
				{
					builder.AppendLine("\t\t\treturn ref mStorage->mAoS[mIndex]." + fieldAccess(field) + ";");
				}
			}
			else if (field.Type.IsUnmanagedType)
			{
				builder.AppendLine("\t\t\treturn ref mStorage->" + fieldAccess(field) + "[mIndex];");
			}
			else
			{
				builder.AppendLine("\t\t\treturn ref mManagedStorage." + fieldAccess(field) + "[mIndex];");
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
		private static void generateUnsafeList(StringBuilder builder, string accessibility, string typeName, string fullTypeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, string backendReason, bool hasSpan, bool hasBurstJobs)
		{
			bool hasManagedStorage = ecsFields.Any(field => !field.Type.IsUnmanagedType) || aosFields.Any(field => !field.Type.IsUnmanagedType);
			List<IFieldSymbol> burstFields = hasBurstJobs ? ecsFields.Where(field => isBurstCompatibleType(field.Type)).ToList() : new List<IFieldSymbol>();
			bool hasBurstIntegration = burstFields.Count > 0;
			appendGeneratedFor(builder, typeName, fullTypeName, "List");
			builder.AppendLine(accessibility + " unsafe sealed class " + typeName + "ECSList : global::System.IDisposable");
			builder.AppendLine("{");
			builder.AppendLine("\tprivate const int ALIGNMENT = 64;");
			builder.AppendLine("\tpublic const bool IsUnsafeBackend = true;");
			builder.AppendLine("\tpublic const string BackendName = \"Unsafe\";");
			builder.AppendLine("\tpublic const string BackendReason = \"" + backendReason + "\";");
			builder.AppendLine("\tprivate global::System.IntPtr mRawMemory;");
			builder.AppendLine("\tprivate global::System.IntPtr mStorageMemory;");
			builder.AppendLine("\tprivate " + typeName + "Storage* mStorage;");
			if (hasManagedStorage)
			{
				builder.AppendLine("\tprivate " + typeName + "ManagedStorage mManagedStorage;");
			}
			builder.AppendLine("\tprivate int mCount;");
			builder.AppendLine("\tprivate int mCapacity;");
			builder.AppendLine("\tprivate bool mDisposed;");
			if (hasBurstIntegration)
			{
				builder.AppendLine("\tprivate global::Unity.Jobs.JobHandle mBurstJobHandle;");
				builder.AppendLine("\tprivate bool mHasPendingBurstJob;");
			}
			generateEditorValidationFields(builder, true);
			generateUnsafeProperties(builder, typeName, hasManagedStorage);
			generateUnsafeDictionaryStorageAccessor(builder, typeName, hasManagedStorage);
			foreach (IFieldSymbol field in ecsFields)
			{
				if (field.Type.IsUnmanagedType)
				{
					generateUnsafeColumn(builder, typeName, field);
				}
				else
				{
					generateUnsafeManagedColumn(builder, typeName, field);
				}
			}
			if (hasBurstIntegration)
			{
				generateUnsafeBurstIntegration(builder, typeName, burstFields);
			}
			generateUnsafeConstructor(builder, typeName, ecsFields, aosFields, hasBurstIntegration);
			generateUnsafeContainerMethods(builder, typeName, fullTypeName, ecsFields, aosFields);
			generateExtendedListMethods(builder, typeName, fullTypeName, ecsFields, aosFields);
			generateUnsafeExtendedListHelpers(builder, typeName, fullTypeName, ecsFields, aosFields, hasSpan);
			generateUnsafeResize(builder, typeName, ecsFields, aosFields, hasBurstIntegration);
			generateUnsafeAllocateColumns(builder, typeName, ecsFields, aosFields);
			generateUnsafeDispose(builder, typeName, ecsFields, aosFields, hasBurstIntegration);
			generateUnsafeHelpers(builder);
			generateEditorValidationMethods(builder, typeName, true);
			builder.AppendLine("}");
		}
		private static void generateSafeSpanList(StringBuilder builder, string accessibility, string typeName, string fullTypeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, string backendReason)
		{
			bool usePermutationSort = ecsFields.Any(field => !field.Type.IsUnmanagedType) || aosFields.Any(field => !field.Type.IsUnmanagedType);
			appendGeneratedFor(builder, typeName, fullTypeName, "List");
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
			generateSafeSpanDictionaryStorageAccessor(builder, typeName);
			foreach (IFieldSymbol field in ecsFields)
			{
				generateSafeSpanColumn(builder, typeName, field);
			}
			generateSafeSpanConstructor(builder, typeName, ecsFields, aosFields);
			generateSafeContainerMethods(builder, typeName, fullTypeName, ecsFields, aosFields, true);
			generateExtendedListMethods(builder, typeName, fullTypeName, ecsFields, aosFields);
			generateSafeExtendedListHelpers(builder, typeName, fullTypeName, ecsFields, aosFields, true);
			generateSafeSpanResize(builder, typeName, ecsFields, aosFields);
			generateSafeSpanDispose(builder, typeName);
			generateEditorValidationMethods(builder, typeName, false);
			builder.AppendLine("}");
		}
		private static void generateSafeRegistryList(StringBuilder builder, string accessibility, string typeName, string fullTypeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, string backendReason)
		{
			bool usePermutationSort = ecsFields.Any(field => !field.Type.IsUnmanagedType) || aosFields.Any(field => !field.Type.IsUnmanagedType);
			appendGeneratedFor(builder, typeName, fullTypeName, "List");
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
			generateSafeRegistryDictionaryStorageAccessor(builder);
			foreach (IFieldSymbol field in ecsFields)
			{
				generateSafeRegistryColumn(builder, typeName, field);
			}
			generateSafeRegistryConstructor(builder, typeName);
			generateSafeContainerMethods(builder, typeName, fullTypeName, ecsFields, aosFields, false);
			generateExtendedListMethods(builder, typeName, fullTypeName, ecsFields, aosFields);
			generateSafeExtendedListHelpers(builder, typeName, fullTypeName, ecsFields, aosFields, false);
			generateSafeRegistryResize(builder, typeName, ecsFields, aosFields);
			generateSafeRegistryDispose(builder, typeName);
			generateEditorValidationMethods(builder, typeName, true);
			builder.AppendLine("}");
		}
		private static void generateUnsafeDictionaryStorageAccessor(StringBuilder builder, string typeName, bool hasManagedStorage)
		{
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tinternal " + typeName + "Storage* getDictionaryStorage()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\treturn mStorage;");
			builder.AppendLine("\t}");
			if (hasManagedStorage)
			{
				appendAggressiveInlining(builder, 1);
				builder.AppendLine("\tinternal " + typeName + "ManagedStorage getDictionaryManagedStorage()");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\treturn mManagedStorage;");
				builder.AppendLine("\t}");
			}
		}
		private static void generateSafeSpanDictionaryStorageAccessor(StringBuilder builder, string typeName)
		{
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tinternal " + typeName + "Storage[] getDictionaryStorage()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\treturn mStorage;");
			builder.AppendLine("\t}");
		}
		private static void generateSafeRegistryDictionaryStorageAccessor(StringBuilder builder)
		{
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tinternal int getDictionaryStorageID()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\treturn mStorageID;");
			builder.AppendLine("\t}");
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
			builder.AppendLine("\tprivate void invalidateRefsFrom(int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tfor (int i = index; i < mCount; ++i)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\t++mRefGeneration[i];");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void invalidateRefsRange(int index, int count)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tint end = index + count;");
			builder.AppendLine("\t\tfor (int i = index; i < end; ++i)");
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
		private static void generateUnsafeProperties(StringBuilder builder, string typeName, bool hasManagedStorage)
		{
			generateCountCapacity(builder);
			builder.AppendLine("\tpublic " + typeName + "Ref this[int index]");
			builder.AppendLine("\t{");
			appendAggressiveInlining(builder, 2);
			builder.AppendLine("\t\tget");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tvalidateIndex(index);");
			builder.AppendLine("\t\t\treturn new " + typeName + "Ref(mStorage" + (hasManagedStorage ? ", mManagedStorage" : string.Empty) + ", index, this, mRefGeneration[index]);");
			builder.AppendLine("#else");
			builder.AppendLine("\t\t\treturn new " + typeName + "Ref(mStorage" + (hasManagedStorage ? ", mManagedStorage" : string.Empty) + ", index);");
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
		private static void generateUnsafeManagedColumn(StringBuilder builder, string typeName, IFieldSymbol field)
		{
			generateSafeColumnType(builder, typeName, field, true);
			string accessibility = field.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
			string columnType = getColumnTypeName(field.Name);
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\t" + accessibility + " " + columnType + " " + getColumnMethodName(field.Name) + "()");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("\t\treturn new " + columnType + "(mManagedStorage." + fieldAccess(field) + ", this, mColumnVersion);");
			builder.AppendLine("#else");
			builder.AppendLine("\t\treturn new " + columnType + "(mManagedStorage." + fieldAccess(field) + ");");
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
		private static void generateUnsafeBurstIntegration(StringBuilder builder, string typeName, List<IFieldSymbol> burstFields)
		{
			builder.AppendLine("\tpublic readonly unsafe struct BurstView");
			builder.AppendLine("\t{");
			foreach (IFieldSymbol field in burstFields)
			{
				builder.AppendLine("\t\t[global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]");
				builder.AppendLine("\t\tpublic readonly " + getTypeName(field.Type) + "* " + fieldAccess(field) + ";");
			}
			builder.AppendLine("\t\tpublic readonly int Count;");
			builder.Append("\t\tpublic BurstView(");
			for (int i = 0; i < burstFields.Count; ++i)
			{
				if (i > 0)
				{
					builder.Append(", ");
				}
				IFieldSymbol field = burstFields[i];
				builder.Append(getTypeName(field.Type) + "* " + fieldAccess(field));
			}
			builder.AppendLine(", int count)");
			builder.AppendLine("\t\t{");
			foreach (IFieldSymbol field in burstFields)
			{
				builder.AppendLine("\t\t\tthis." + fieldAccess(field) + " = " + fieldAccess(field) + ";");
			}
			builder.AppendLine("\t\t\tCount = count;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic BurstView GetBurstView()");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.Append("\t\treturn new BurstView(");
			for (int i = 0; i < burstFields.Count; ++i)
			{
				if (i > 0)
				{
					builder.Append(", ");
				}
				builder.Append("mStorage->" + fieldAccess(burstFields[i]));
			}
			builder.AppendLine(", mCount);");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic global::Unity.Jobs.JobHandle ScheduleBurst<TJob>(TJob job, int innerloopBatchCount = 64) where TJob : struct, global::Unity.Jobs.IJobParallelFor");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\treturn ScheduleBurst(job, innerloopBatchCount, default(global::Unity.Jobs.JobHandle));");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic global::Unity.Jobs.JobHandle ScheduleBurst<TJob>(TJob job, int innerloopBatchCount, global::Unity.Jobs.JobHandle dependsOn) where TJob : struct, global::Unity.Jobs.IJobParallelFor");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tif (innerloopBatchCount < 1)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.ArgumentOutOfRangeException(nameof(innerloopBatchCount));");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tif (mHasPendingBurstJob)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tdependsOn = global::Unity.Jobs.JobHandle.CombineDependencies(mBurstJobHandle, dependsOn);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tglobal::Unity.Jobs.JobHandle handle = global::Unity.Jobs.IJobParallelForExtensions.Schedule(job, mCount, innerloopBatchCount, dependsOn);");
			builder.AppendLine("\t\tmBurstJobHandle = handle;");
			builder.AppendLine("\t\tmHasPendingBurstJob = true;");
			builder.AppendLine("\t\treturn handle;");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic global::Unity.Jobs.JobHandle GetBurstDependency()");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\treturn mHasPendingBurstJob ? mBurstJobHandle : default(global::Unity.Jobs.JobHandle);");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic void RegisterBurstJob(global::Unity.Jobs.JobHandle handle)");
			builder.AppendLine("\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tvalidateAlive();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tif (mHasPendingBurstJob)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmBurstJobHandle = global::Unity.Jobs.JobHandle.CombineDependencies(mBurstJobHandle, handle);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\telse");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmBurstJobHandle = handle;");
			builder.AppendLine("\t\t\tmHasPendingBurstJob = true;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic void CompleteBurstJobs()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tcompleteBurstJobs();");
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void completeBurstJobs()");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (!mHasPendingBurstJob)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tmBurstJobHandle.Complete();");
			builder.AppendLine("\t\tmBurstJobHandle = default(global::Unity.Jobs.JobHandle);");
			builder.AppendLine("\t\tmHasPendingBurstJob = false;");
			builder.AppendLine("\t}");
		}
		private static bool isBurstCompatibleType(ITypeSymbol type)
		{
			return isBurstCompatibleType(type, new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default));
		}
		private static bool isBurstCompatibleType(ITypeSymbol type, HashSet<ITypeSymbol> visiting)
		{
			if (type == null || !type.IsUnmanagedType)
			{
				return false;
			}
			if (type.TypeKind == TypeKind.Enum)
			{
				return true;
			}
			if (type is IPointerTypeSymbol pointerType)
			{
				return isBurstCompatibleType(pointerType.PointedAtType, visiting);
			}
			switch (type.SpecialType)
			{
				case SpecialType.System_Boolean:
				case SpecialType.System_Byte:
				case SpecialType.System_SByte:
				case SpecialType.System_Int16:
				case SpecialType.System_UInt16:
				case SpecialType.System_Int32:
				case SpecialType.System_UInt32:
				case SpecialType.System_Int64:
				case SpecialType.System_UInt64:
				case SpecialType.System_Single:
				case SpecialType.System_Double:
					return true;
				case SpecialType.System_Char:
				case SpecialType.System_Decimal:
					return false;
			}
			string metadataName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			if (metadataName == "global::System.IntPtr" || metadataName == "global::System.UIntPtr")
			{
				return true;
			}
			if (!(type is INamedTypeSymbol namedType) || namedType.TypeKind != TypeKind.Struct)
			{
				return false;
			}
			if (!visiting.Add(type))
			{
				return true;
			}
			foreach (IFieldSymbol field in namedType.GetMembers().OfType<IFieldSymbol>())
			{
				if (field.IsStatic || field.IsImplicitlyDeclared)
				{
					continue;
				}
				if (!isBurstCompatibleType(field.Type, visiting))
				{
					visiting.Remove(type);
					return false;
				}
			}
			visiting.Remove(type);
			return true;
		}
		private static void generateUnsafeConstructor(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, bool hasBurstIntegration)
		{
			bool hasManagedStorage = ecsFields.Any(field => !field.Type.IsUnmanagedType) || aosFields.Any(field => !field.Type.IsUnmanagedType);
			builder.AppendLine("\tpublic " + typeName + "ECSList(int capacity = 4)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (capacity < 1)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tcapacity = 1;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\ttry");
			builder.AppendLine("\t\t{");
			if (hasManagedStorage)
			{
				builder.AppendLine("\t\t\tmManagedStorage = new " + typeName + "ManagedStorage(capacity);");
			}
			builder.AppendLine("\t\t\tmStorageMemory = global::System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(" + typeName + "Storage));");
			builder.AppendLine("\t\t\tmStorage = (" + typeName + "Storage*)mStorageMemory.ToPointer();");
			builder.AppendLine("\t\t\t" + typeName + "Storage initialStorage;");
			builder.AppendLine("\t\t\tallocateColumns(capacity, out mRawMemory, out initialStorage);");
			builder.AppendLine("\t\t\t*mStorage = initialStorage;");
			builder.AppendLine("\t\t\tmCapacity = capacity;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tmRefGeneration = new int[capacity];");
			builder.AppendLine("\t\t\tmDebugLifecycleID = global::ECSSourceGeneratorGenerated.ECSListLeakTracker.register(\"" + typeName + "ECSList\", \"Unsafe\");");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tcatch");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tif (mDebugLifecycleID != 0)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tglobal::ECSSourceGeneratorGenerated.ECSListLeakTracker.unregister(mDebugLifecycleID);");
			builder.AppendLine("\t\t\t\tmDebugLifecycleID = 0;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tmRefGeneration = null;");
			builder.AppendLine("#endif");
			if (hasManagedStorage)
			{
				builder.AppendLine("\t\t\tmManagedStorage = null;");
			}
			builder.AppendLine("\t\t\tif (mRawMemory != global::System.IntPtr.Zero)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tglobal::System.Runtime.InteropServices.Marshal.FreeHGlobal(mRawMemory);");
			builder.AppendLine("\t\t\t\tmRawMemory = global::System.IntPtr.Zero;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tif (mStorageMemory != global::System.IntPtr.Zero)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tglobal::System.Runtime.InteropServices.Marshal.FreeHGlobal(mStorageMemory);");
			builder.AppendLine("\t\t\t\tmStorageMemory = global::System.IntPtr.Zero;");
			builder.AppendLine("\t\t\t\tmStorage = null;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tmCount = 0;");
			builder.AppendLine("\t\t\tmCapacity = 0;");
			builder.AppendLine("\t\t\tmDisposed = true;");
			builder.AppendLine("\t\t\tglobal::System.GC.SuppressFinalize(this);");
			builder.AppendLine("\t\t\tthrow;");
			builder.AppendLine("\t\t}");
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
			if (hasBurstIntegration)
			{
				builder.AppendLine("\t\tif (mHasPendingBurstJob)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\treturn;");
				builder.AppendLine("\t\t}");
			}
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
			builder.AppendLine("\t\ttry");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmStorageID = " + typeName + "StorageRegistry.add(capacity);");
			builder.AppendLine("\t\t\tmCapacity = capacity;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tmRefGeneration = new int[capacity];");
			builder.AppendLine("\t\t\tmDebugLifecycleID = global::ECSSourceGeneratorGenerated.ECSListLeakTracker.register(\"" + typeName + "ECSList\", \"SafeRegistry\");");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tcatch");
			builder.AppendLine("\t\t{");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\t\tif (mDebugLifecycleID != 0)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\tglobal::ECSSourceGeneratorGenerated.ECSListLeakTracker.unregister(mDebugLifecycleID);");
			builder.AppendLine("\t\t\t\tmDebugLifecycleID = 0;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tmRefGeneration = null;");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\t\tif (mStorageID >= 0)");
			builder.AppendLine("\t\t\t{");
			builder.AppendLine("\t\t\t\t" + typeName + "StorageRegistry.remove(mStorageID);");
			builder.AppendLine("\t\t\t\tmStorageID = -1;");
			builder.AppendLine("\t\t\t}");
			builder.AppendLine("\t\t\tmCount = 0;");
			builder.AppendLine("\t\t\tmCapacity = 0;");
			builder.AppendLine("\t\t\tmDisposed = true;");
			builder.AppendLine("\t\t\tglobal::System.GC.SuppressFinalize(this);");
			builder.AppendLine("\t\t\tthrow;");
			builder.AppendLine("\t\t}");
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
			bool hasManagedECS = ecsFields.Any(field => !field.Type.IsUnmanagedType);
			bool hasManagedAoS = aosFields.Any(field => !field.Type.IsUnmanagedType);
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
				if (field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\tvalue." + fieldAccess(field) + " = mStorage->" + fieldAccess(field) + "[index];");
				}
				else
				{
					builder.AppendLine("\t\tvalue." + fieldAccess(field) + " = mManagedStorage." + fieldAccess(field) + "[index];");
				}
			}
			foreach (IFieldSymbol field in aosFields)
			{
				if (hasManagedAoS)
				{
					builder.AppendLine("\t\tvalue." + fieldAccess(field) + " = mManagedStorage.mAoS[index]." + fieldAccess(field) + ";");
				}
				else
				{
					builder.AppendLine("\t\tvalue." + fieldAccess(field) + " = mStorage->mAoS[index]." + fieldAccess(field) + ";");
				}
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
			if (hasManagedECS)
			{
				foreach (IFieldSymbol field in ecsFields)
				{
					if (!field.Type.IsUnmanagedType)
					{
						builder.AppendLine("\t\tglobal::System.Array.Clear(mManagedStorage." + fieldAccess(field) + ", 0, mCount);");
					}
				}
			}
			if (hasManagedAoS)
			{
				builder.AppendLine("\t\tglobal::System.Array.Clear(mManagedStorage.mAoS, 0, mCount);");
			}
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
				if (field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\t\tmStorage->" + fieldAccess(field) + "[index] = mStorage->" + fieldAccess(field) + "[lastIndex];");
				}
				else
				{
					builder.AppendLine("\t\t\tmManagedStorage." + fieldAccess(field) + "[index] = mManagedStorage." + fieldAccess(field) + "[lastIndex];");
				}
			}
			if (aosFields.Count > 0)
			{
				if (hasManagedAoS)
				{
					builder.AppendLine("\t\t\tmManagedStorage.mAoS[index] = mManagedStorage.mAoS[lastIndex];");
				}
				else
				{
					builder.AppendLine("\t\t\tmStorage->mAoS[index] = mStorage->mAoS[lastIndex];");
				}
			}
			builder.AppendLine("\t\t}");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (!field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\tmManagedStorage." + fieldAccess(field) + "[lastIndex] = default(" + getTypeName(field.Type) + ");");
				}
			}
			if (hasManagedAoS)
			{
				builder.AppendLine("\t\tmManagedStorage.mAoS[lastIndex] = default(" + typeName + "AoSBlock);");
			}
			builder.AppendLine("\t\t--mCount;");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic void Insert(int index, " + fullTypeName + " value)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (mDisposed)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSList\");");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tif ((uint)index > (uint)mCount)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.ArgumentOutOfRangeException(nameof(index));");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tif (mCount >= mCapacity)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tresize(mCapacity * 2);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tinvalidateRefsFrom(index);");
			builder.AppendLine("\t\tinvalidateColumn();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tif (index == mCount)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tsetValue(index, value);");
			builder.AppendLine("\t\t\t++mCount;");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tint moveCount = mCount - index;");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (field.Type.IsUnmanagedType)
				{
					string fieldType = getTypeName(field.Type);
					builder.AppendLine("\t\tglobal::System.Buffer.MemoryCopy(mStorage->" + fieldAccess(field) + " + index, mStorage->" + fieldAccess(field) + " + index + 1, (long)moveCount * sizeof(" + fieldType + "), (long)moveCount * sizeof(" + fieldType + "));");
				}
			}
			if (aosFields.Count > 0 && !hasManagedAoS)
			{
				builder.AppendLine("\t\tglobal::System.Buffer.MemoryCopy(mStorage->mAoS + index, mStorage->mAoS + index + 1, (long)moveCount * sizeof(" + typeName + "AoSBlock), (long)moveCount * sizeof(" + typeName + "AoSBlock));");
			}
			foreach (IFieldSymbol field in ecsFields)
			{
				if (!field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\tglobal::System.Array.Copy(mManagedStorage." + fieldAccess(field) + ", index, mManagedStorage." + fieldAccess(field) + ", index + 1, mCount - index);");
				}
			}
			if (hasManagedAoS)
			{
				builder.AppendLine("\t\tglobal::System.Array.Copy(mManagedStorage.mAoS, index, mManagedStorage.mAoS, index + 1, mCount - index);");
			}
			builder.AppendLine("\t\tsetValue(index, value);");
			builder.AppendLine("\t\t++mCount;");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic void RemoveAt(int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (mDisposed)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSList\");");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tif ((uint)index >= (uint)mCount)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.ArgumentOutOfRangeException(nameof(index));");
			builder.AppendLine("\t\t}");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tinvalidateRefsFrom(index);");
			builder.AppendLine("\t\tinvalidateColumn();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tint lastIndex = mCount - 1;");
			builder.AppendLine("\t\tfor (int i = index; i < lastIndex; ++i)");
			builder.AppendLine("\t\t{");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\t\tmStorage->" + fieldAccess(field) + "[i] = mStorage->" + fieldAccess(field) + "[i + 1];");
				}
			}
			if (aosFields.Count > 0 && !hasManagedAoS)
			{
				builder.AppendLine("\t\t\tmStorage->mAoS[i] = mStorage->mAoS[i + 1];");
			}
			builder.AppendLine("\t\t}");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (!field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\tif (index < lastIndex)");
					builder.AppendLine("\t\t{");
					builder.AppendLine("\t\t\tglobal::System.Array.Copy(mManagedStorage." + fieldAccess(field) + ", index + 1, mManagedStorage." + fieldAccess(field) + ", index, lastIndex - index);");
					builder.AppendLine("\t\t}");
					builder.AppendLine("\t\tmManagedStorage." + fieldAccess(field) + "[lastIndex] = default(" + getTypeName(field.Type) + ");");
				}
			}
			if (hasManagedAoS)
			{
				builder.AppendLine("\t\tif (index < lastIndex)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tglobal::System.Array.Copy(mManagedStorage.mAoS, index + 1, mManagedStorage.mAoS, index, lastIndex - index);");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tmManagedStorage.mAoS[lastIndex] = default(" + typeName + "AoSBlock);");
			}
			builder.AppendLine("\t\t--mCount;");
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate void setValue(int index, " + fullTypeName + " value)");
			builder.AppendLine("\t{");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\tmStorage->" + fieldAccess(field) + "[index] = value." + fieldAccess(field) + ";");
				}
				else
				{
					builder.AppendLine("\t\tmManagedStorage." + fieldAccess(field) + "[index] = value." + fieldAccess(field) + ";");
				}
			}
			foreach (IFieldSymbol field in aosFields)
			{
				if (hasManagedAoS)
				{
					builder.AppendLine("\t\tmManagedStorage.mAoS[index]." + fieldAccess(field) + " = value." + fieldAccess(field) + ";");
				}
				else
				{
					builder.AppendLine("\t\tmStorage->mAoS[index]." + fieldAccess(field) + " = value." + fieldAccess(field) + ";");
				}
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
			builder.AppendLine("\tpublic void Insert(int index, " + fullTypeName + " value)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (mDisposed)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSList\");");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tif ((uint)index > (uint)mCount)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.ArgumentOutOfRangeException(nameof(index));");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tif (mCount >= mCapacity)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tresize(mCapacity * 2);");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\t" + storageDeclaration);
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tinvalidateRefsFrom(index);");
			builder.AppendLine("\t\tinvalidateColumn();");
			builder.AppendLine("#endif");
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\tglobal::System.Array.Copy(storage." + fieldAccess(field) + ", index, storage." + fieldAccess(field) + ", index + 1, mCount - index);");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\tglobal::System.Array.Copy(storage.mAoS, index, storage.mAoS, index + 1, mCount - index);");
			}
			builder.AppendLine("\t\tsetValue(index, value);");
			builder.AppendLine("\t\t++mCount;");
			builder.AppendLine("\t}");
			builder.AppendLine("\tpublic void RemoveAt(int index)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (mDisposed)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSList\");");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tif ((uint)index >= (uint)mCount)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tthrow new global::System.ArgumentOutOfRangeException(nameof(index));");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\t" + storageDeclaration);
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tinvalidateRefsFrom(index);");
			builder.AppendLine("\t\tinvalidateColumn();");
			builder.AppendLine("#endif");
			builder.AppendLine("\t\tint lastIndex = mCount - 1;");
			builder.AppendLine("\t\tint moveCount = lastIndex - index;");
			builder.AppendLine("\t\tif (moveCount > 0)");
			builder.AppendLine("\t\t{");
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\t\tglobal::System.Array.Copy(storage." + fieldAccess(field) + ", index + 1, storage." + fieldAccess(field) + ", index, moveCount);");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\t\tglobal::System.Array.Copy(storage.mAoS, index + 1, storage.mAoS, index, moveCount);");
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
		private static void generateExtendedListMethods(StringBuilder builder, string typeName, string fullTypeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			bool usePermutationSort = ecsFields.Any(field => !field.Type.IsUnmanagedType) || aosFields.Any(field => !field.Type.IsUnmanagedType);
			string source = @"
	private interface IECSListSortComparer
	{
		int Compare(__ECS_TYPE__ left, __ECS_TYPE__ right);
	}
	private interface IECSListIndexSortComparer
	{
		int Compare(int leftIndex, int rightIndex);
	}
	private readonly struct ECSListComparerAdapter : IECSListSortComparer
	{
		private readonly global::System.Collections.Generic.IComparer<__ECS_TYPE__> mComparer;
		public ECSListComparerAdapter(global::System.Collections.Generic.IComparer<__ECS_TYPE__> comparer)
		{
			mComparer = comparer;
		}
		[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public int Compare(__ECS_TYPE__ left, __ECS_TYPE__ right)
		{
			return mComparer.Compare(left, right);
		}
	}
	private readonly struct ECSListComparisonAdapter : IECSListSortComparer
	{
		private readonly global::System.Comparison<__ECS_TYPE__> mComparison;
		public ECSListComparisonAdapter(global::System.Comparison<__ECS_TYPE__> comparison)
		{
			mComparison = comparison;
		}
		[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public int Compare(__ECS_TYPE__ left, __ECS_TYPE__ right)
		{
			return mComparison(left, right);
		}
	}
	public int EnsureCapacity(int capacity)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		if (capacity < 0)
		{
			throw new global::System.ArgumentOutOfRangeException(nameof(capacity));
		}
		if (capacity > mCapacity)
		{
			resize(capacity);
#if UNITY_EDITOR
			invalidateColumn();
#endif
		}
		return mCapacity;
	}
	public void TrimExcess()
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		int threshold = (int)((long)mCapacity * 9L / 10L);
		if (mCount >= threshold)
		{
			return;
		}
		int targetCapacity = mCount > 0 ? mCount : 1;
		resize(targetCapacity);
#if UNITY_EDITOR
		invalidateColumn();
#endif
	}
	public void AddRange(__ECS_TYPE__[] values)
	{
		if (values == null)
		{
			throw new global::System.ArgumentNullException(nameof(values));
		}
		InsertRange(mCount, values, 0, values.Length);
	}
	public void AddRange(__ECS_TYPE__[] values, int sourceIndex, int count)
	{
		InsertRange(mCount, values, sourceIndex, count);
	}
	public void AddRange(__ECS_LIST__ values)
	{
		if (values == null)
		{
			throw new global::System.ArgumentNullException(nameof(values));
		}
		InsertRange(mCount, values);
	}
	public void InsertRange(int index, __ECS_TYPE__[] values)
	{
		if (values == null)
		{
			throw new global::System.ArgumentNullException(nameof(values));
		}
		InsertRange(index, values, 0, values.Length);
	}
	public void InsertRange(int index, __ECS_TYPE__[] values, int sourceIndex, int count)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		if (values == null)
		{
			throw new global::System.ArgumentNullException(nameof(values));
		}
		if ((uint)index > (uint)mCount)
		{
			throw new global::System.ArgumentOutOfRangeException(nameof(index));
		}
		if ((uint)sourceIndex > (uint)values.Length)
		{
			throw new global::System.ArgumentOutOfRangeException(nameof(sourceIndex));
		}
		if (count < 0 || sourceIndex > values.Length - count)
		{
			throw new global::System.ArgumentOutOfRangeException(nameof(count));
		}
		if (count == 0)
		{
			return;
		}
		if (count > global::System.Int32.MaxValue - mCount)
		{
			throw new global::System.OutOfMemoryException();
		}
		int oldCount = mCount;
		ensureCapacityFor(oldCount + count);
#if UNITY_EDITOR
		invalidateRefsFrom(index);
		invalidateColumn();
#endif
		int moveCount = oldCount - index;
		if (moveCount > 0)
		{
			moveRange(index, index + count, moveCount);
		}
		copyFromArray(values, sourceIndex, index, count);
		mCount = oldCount + count;
	}
	public void InsertRange(int index, __ECS_LIST__ values)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		if (values == null)
		{
			throw new global::System.ArgumentNullException(nameof(values));
		}
		if ((uint)index > (uint)mCount)
		{
			throw new global::System.ArgumentOutOfRangeException(nameof(index));
		}
#if UNITY_EDITOR
		values.validateAlive();
#endif
		int insertCount = values.mCount;
		if (insertCount == 0)
		{
			return;
		}
		if (insertCount > global::System.Int32.MaxValue - mCount)
		{
			throw new global::System.OutOfMemoryException();
		}
		int oldCount = mCount;
		ensureCapacityFor(oldCount + insertCount);
#if UNITY_EDITOR
		invalidateRefsFrom(index);
		invalidateColumn();
#endif
		if (global::System.Object.ReferenceEquals(this, values))
		{
			if (index < oldCount)
			{
				moveRange(index, index + oldCount, oldCount - index);
			}
			if (index > 0)
			{
				moveRange(0, index, index);
			}
			if (index < oldCount)
			{
				moveRange(index + oldCount, index + index, oldCount - index);
			}
			mCount = oldCount + oldCount;
			return;
		}
		int moveCount = oldCount - index;
		if (moveCount > 0)
		{
			moveRange(index, index + insertCount, moveCount);
		}
		copyRangeFrom(values, 0, index, insertCount);
		mCount = oldCount + insertCount;
	}
	public void RemoveRange(int index, int count)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		if (index < 0)
		{
			throw new global::System.ArgumentOutOfRangeException(nameof(index));
		}
		if (count < 0)
		{
			throw new global::System.ArgumentOutOfRangeException(nameof(count));
		}
		if (index > mCount - count)
		{
			throw new global::System.ArgumentException(""index和count超出ECSList有效范围"");
		}
		if (count == 0)
		{
			return;
		}
#if UNITY_EDITOR
		invalidateRefsFrom(index);
		invalidateColumn();
#endif
		int newCount = mCount - count;
		int moveCount = newCount - index;
		if (moveCount > 0)
		{
			moveRange(index + count, index, moveCount);
		}
		clearRange(newCount, count);
		mCount = newCount;
	}
	public bool Contains(__ECS_TYPE__ value)
	{
		return IndexOf(value) >= 0;
	}
	public int IndexOf(__ECS_TYPE__ value)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		global::System.Collections.Generic.EqualityComparer<__ECS_TYPE__> comparer = global::System.Collections.Generic.EqualityComparer<__ECS_TYPE__>.Default;
		for (int i = 0; i < mCount; ++i)
		{
			if (comparer.Equals(Get(i), value))
			{
				return i;
			}
		}
		return -1;
	}
	public int LastIndexOf(__ECS_TYPE__ value)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		global::System.Collections.Generic.EqualityComparer<__ECS_TYPE__> comparer = global::System.Collections.Generic.EqualityComparer<__ECS_TYPE__>.Default;
		for (int i = mCount - 1; i >= 0; --i)
		{
			if (comparer.Equals(Get(i), value))
			{
				return i;
			}
		}
		return -1;
	}
	public bool Remove(__ECS_TYPE__ value)
	{
		int index = IndexOf(value);
		if (index < 0)
		{
			return false;
		}
		RemoveAt(index);
		return true;
	}
	public int RemoveAll(global::System.Predicate<__ECS_TYPE__> match)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		if (match == null)
		{
			throw new global::System.ArgumentNullException(nameof(match));
		}
		int freeIndex = 0;
		while (freeIndex < mCount && !match(Get(freeIndex)))
		{
			++freeIndex;
		}
		if (freeIndex >= mCount)
		{
			return 0;
		}
		int firstRemovedIndex = freeIndex;
		int current = freeIndex + 1;
		while (current < mCount)
		{
			while (current < mCount && match(Get(current)))
			{
				++current;
			}
			if (current >= mCount)
			{
				break;
			}
			int runStart = current;
			++current;
			while (current < mCount && !match(Get(current)))
			{
				++current;
			}
			int runCount = current - runStart;
			if (runCount >= 8)
			{
				moveRange(runStart, freeIndex, runCount);
			}
			else
			{
				for (int i = 0; i < runCount; ++i)
				{
					copyValue(runStart + i, freeIndex + i);
				}
			}
			freeIndex += runCount;
			if (current < mCount)
			{
				++current;
			}
		}
		int removedCount = mCount - freeIndex;
#if UNITY_EDITOR
		invalidateRefsFrom(firstRemovedIndex);
		invalidateColumn();
#endif
		clearRange(freeIndex, removedCount);
		mCount = freeIndex;
		return removedCount;
	}
	public void Reverse()
	{
		Reverse(0, mCount);
	}
	public void Reverse(int index, int count)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		validateRange(index, count);
		if (count <= 1)
		{
			return;
		}
#if UNITY_EDITOR
		invalidateRefsRange(index, count);
		invalidateColumn();
#endif
		reverseRange(index, count);
	}
	public void Sort()
	{
		Sort(0, mCount, null);
	}
	public void Sort(global::System.Collections.Generic.IComparer<__ECS_TYPE__> comparer)
	{
		Sort(0, mCount, comparer);
	}
	public void Sort(global::System.Comparison<__ECS_TYPE__> comparison)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		if (comparison == null)
		{
			throw new global::System.ArgumentNullException(nameof(comparison));
		}
		if (mCount <= 1)
		{
			return;
		}
#if UNITY_EDITOR
		invalidateRefsRange(0, mCount);
		invalidateColumn();
#endif
		sortCore(0, mCount, new ECSListComparisonAdapter(comparison));
	}
	public void Sort(int index, int count, global::System.Collections.Generic.IComparer<__ECS_TYPE__> comparer)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		validateRange(index, count);
		if (count <= 1)
		{
			return;
		}
		if (comparer == null)
		{
			comparer = global::System.Collections.Generic.Comparer<__ECS_TYPE__>.Default;
		}
#if UNITY_EDITOR
		invalidateRefsRange(index, count);
		invalidateColumn();
#endif
		sortCore(index, count, new ECSListComparerAdapter(comparer));
	}
	public int BinarySearch(__ECS_TYPE__ value)
	{
		return BinarySearch(0, mCount, value, null);
	}
	public int BinarySearch(__ECS_TYPE__ value, global::System.Collections.Generic.IComparer<__ECS_TYPE__> comparer)
	{
		return BinarySearch(0, mCount, value, comparer);
	}
	public int BinarySearch(int index, int count, __ECS_TYPE__ value, global::System.Collections.Generic.IComparer<__ECS_TYPE__> comparer)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		validateRange(index, count);
		if (comparer == null)
		{
			comparer = global::System.Collections.Generic.Comparer<__ECS_TYPE__>.Default;
		}
		int low = index;
		int high = index + count - 1;
		while (low <= high)
		{
			int middle = low + ((high - low) >> 1);
			int compare = comparer.Compare(Get(middle), value);
			if (compare == 0)
			{
				return middle;
			}
			if (compare < 0)
			{
				low = middle + 1;
			}
			else
			{
				high = middle - 1;
			}
		}
		return ~low;
	}
	public __ECS_TYPE__[] ToArray()
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		__ECS_TYPE__[] result = new __ECS_TYPE__[mCount];
		CopyTo(0, result, 0, mCount);
		return result;
	}
	public void CopyTo(__ECS_TYPE__[] array)
	{
		CopyTo(0, array, 0, mCount);
	}
	public void CopyTo(__ECS_TYPE__[] array, int arrayIndex)
	{
		CopyTo(0, array, arrayIndex, mCount);
	}
	public void CopyTo(int index, __ECS_TYPE__[] array, int arrayIndex, int count)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		if (array == null)
		{
			throw new global::System.ArgumentNullException(nameof(array));
		}
		validateRange(index, count);
		if ((uint)arrayIndex > (uint)array.Length)
		{
			throw new global::System.ArgumentOutOfRangeException(nameof(arrayIndex));
		}
		if (count > array.Length - arrayIndex)
		{
			throw new global::System.ArgumentException(""目标数组空间不足"");
		}
		copyToArray(index, array, arrayIndex, count);
	}
	public bool Exists(global::System.Predicate<__ECS_TYPE__> match)
	{
		return FindIndex(match) >= 0;
	}
	public __ECS_TYPE__ Find(global::System.Predicate<__ECS_TYPE__> match)
	{
		int index = FindIndex(match);
		return index >= 0 ? Get(index) : default(__ECS_TYPE__);
	}
	public int FindIndex(global::System.Predicate<__ECS_TYPE__> match)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		if (match == null)
		{
			throw new global::System.ArgumentNullException(nameof(match));
		}
		for (int i = 0; i < mCount; ++i)
		{
			if (match(Get(i)))
			{
				return i;
			}
		}
		return -1;
	}
	public __ECS_TYPE__ FindLast(global::System.Predicate<__ECS_TYPE__> match)
	{
		int index = FindLastIndex(match);
		return index >= 0 ? Get(index) : default(__ECS_TYPE__);
	}
	public int FindLastIndex(global::System.Predicate<__ECS_TYPE__> match)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		if (match == null)
		{
			throw new global::System.ArgumentNullException(nameof(match));
		}
		for (int i = mCount - 1; i >= 0; --i)
		{
			if (match(Get(i)))
			{
				return i;
			}
		}
		return -1;
	}
	public bool TrueForAll(global::System.Predicate<__ECS_TYPE__> match)
	{
		if (mDisposed)
		{
			throw new global::System.ObjectDisposedException(""__ECS_LIST__"");
		}
		if (match == null)
		{
			throw new global::System.ArgumentNullException(nameof(match));
		}
		for (int i = 0; i < mCount; ++i)
		{
			if (!match(Get(i)))
			{
				return false;
			}
		}
		return true;
	}
	private void ensureCapacityFor(int minimumCapacity)
	{
		if (minimumCapacity <= mCapacity)
		{
			return;
		}
		int newCapacity = mCapacity > 0 ? mCapacity : 1;
		while (newCapacity < minimumCapacity)
		{
			if (newCapacity > global::System.Int32.MaxValue / 2)
			{
				newCapacity = minimumCapacity;
				break;
			}
			newCapacity *= 2;
		}
		resize(newCapacity);
	}
	private void validateRange(int index, int count)
	{
		if (index < 0)
		{
			throw new global::System.ArgumentOutOfRangeException(nameof(index));
		}
		if (count < 0)
		{
			throw new global::System.ArgumentOutOfRangeException(nameof(count));
		}
		if (index > mCount - count)
		{
			throw new global::System.ArgumentException(""index和count超出ECSList有效范围"");
		}
	}
	private void sortCore<TComparer>(int index, int count, TComparer comparer) where TComparer : struct, IECSListSortComparer
	{
		introSort(index, index + count - 1, 2 * floorLog2PlusOne(count), comparer);
	}
	private void introSort<TComparer>(int low, int high, int depthLimit, TComparer comparer) where TComparer : struct, IECSListSortComparer
	{
		while (high > low)
		{
			int partitionSize = high - low + 1;
			if (partitionSize <= 16)
			{
				if (partitionSize == 2)
				{
					swapIfGreater(low, high, comparer);
					return;
				}
				if (partitionSize == 3)
				{
					swapIfGreater(low, high - 1, comparer);
					swapIfGreater(low, high, comparer);
					swapIfGreater(high - 1, high, comparer);
					return;
				}
				insertionSort(low, high, comparer);
				return;
			}
			if (depthLimit == 0)
			{
				heapSort(low, high, comparer);
				return;
			}
			--depthLimit;
			int partition = pickPivotAndPartition(low, high, comparer);
			introSort(partition + 1, high, depthLimit, comparer);
			high = partition - 1;
		}
	}
	private int pickPivotAndPartition<TComparer>(int low, int high, TComparer comparer) where TComparer : struct, IECSListSortComparer
	{
		int middle = low + ((high - low) >> 1);
		swapIfGreater(low, middle, comparer);
		swapIfGreater(low, high, comparer);
		swapIfGreater(middle, high, comparer);
		__ECS_TYPE__ pivot = Get(middle);
		swapValue(middle, high - 1);
		int left = low;
		int right = high - 1;
		while (true)
		{
			while (comparer.Compare(Get(++left), pivot) < 0)
			{
			}
			while (comparer.Compare(pivot, Get(--right)) < 0)
			{
			}
			if (left >= right)
			{
				break;
			}
			swapValue(left, right);
		}
		swapValue(left, high - 1);
		return left;
	}
	private void insertionSort<TComparer>(int low, int high, TComparer comparer) where TComparer : struct, IECSListSortComparer
	{
		for (int i = low; i < high; ++i)
		{
			int current = i;
			__ECS_TYPE__ value = Get(i + 1);
			while (current >= low && comparer.Compare(value, Get(current)) < 0)
			{
				copyValue(current, current + 1);
				--current;
			}
			setValue(current + 1, value);
		}
	}
	private void heapSort<TComparer>(int low, int high, TComparer comparer) where TComparer : struct, IECSListSortComparer
	{
		int n = high - low + 1;
		for (int i = n >> 1; i >= 1; --i)
		{
			downHeap(i, n, low, comparer);
		}
		for (int i = n; i > 1; --i)
		{
			swapValue(low, low + i - 1);
			downHeap(1, i - 1, low, comparer);
		}
	}
	private void downHeap<TComparer>(int index, int count, int low, TComparer comparer) where TComparer : struct, IECSListSortComparer
	{
		__ECS_TYPE__ value = Get(low + index - 1);
		while (index <= count >> 1)
		{
			int child = index << 1;
			if (child < count && comparer.Compare(Get(low + child - 1), Get(low + child)) < 0)
			{
				++child;
			}
			if (comparer.Compare(value, Get(low + child - 1)) >= 0)
			{
				break;
			}
			copyValue(low + child - 1, low + index - 1);
			index = child;
		}
		setValue(low + index - 1, value);
	}
	private void swapIfGreater<TComparer>(int firstIndex, int secondIndex, TComparer comparer) where TComparer : struct, IECSListSortComparer
	{
		if (firstIndex != secondIndex && comparer.Compare(Get(firstIndex), Get(secondIndex)) > 0)
		{
			swapValue(firstIndex, secondIndex);
		}
	}
		private static int floorLog2PlusOne(int value)
	{
		int result = 0;
		while (value >= 1)
		{
			++result;
			value >>= 1;
		}
		return result;
	}
";
			builder.Append(source.TrimStart('\r', '\n').Replace("__ECS_TYPE__", fullTypeName).Replace("__ECS_LIST__", typeName + "ECSList"));
			if (usePermutationSort)
			{
				builder.Append(@"	[global::System.ThreadStaticAttribute]
	private static int[] sSortPermutationCache;
	private void sortByCore<TComparer>(int index, int count, TComparer comparer) where TComparer : struct, IECSListIndexSortComparer
	{
		int[] permutation = rentSortPermutation(count);
		try
		{
			for (int i = 0; i < count; ++i)
			{
				permutation[i] = index + i;
			}
			introSortPermutation(permutation, 0, count - 1, 2 * floorLog2PlusOne(count), comparer);
			applySortPermutation(index, count, permutation);
		}
		finally
		{
			returnSortPermutation(permutation);
		}
	}
	private static int[] rentSortPermutation(int count)
	{
		int[] permutation = sSortPermutationCache;
		sSortPermutationCache = null;
		if (permutation == null || permutation.Length < count)
		{
			int capacity = 16;
			while (capacity < count)
			{
				if (capacity > global::System.Int32.MaxValue / 2)
				{
					capacity = count;
					break;
				}
				capacity *= 2;
			}
			permutation = new int[capacity];
		}
		return permutation;
	}
	private static void returnSortPermutation(int[] permutation)
	{
		if (permutation == null)
		{
			return;
		}
		int[] cached = sSortPermutationCache;
		if (cached == null || cached.Length < permutation.Length)
		{
			sSortPermutationCache = permutation;
		}
	}
	private static void introSortPermutation<TComparer>(int[] permutation, int low, int high, int depthLimit, TComparer comparer) where TComparer : struct, IECSListIndexSortComparer
	{
		while (high > low)
		{
			int partitionSize = high - low + 1;
			if (partitionSize <= 16)
			{
				insertionSortPermutation(permutation, low, high, comparer);
				return;
			}
			if (depthLimit == 0)
			{
				heapSortPermutation(permutation, low, high, comparer);
				return;
			}
			--depthLimit;
			int partition = pickPivotAndPartitionPermutation(permutation, low, high, comparer);
			if (partition - low < high - partition)
			{
				introSortPermutation(permutation, low, partition - 1, depthLimit, comparer);
				low = partition + 1;
			}
			else
			{
				introSortPermutation(permutation, partition + 1, high, depthLimit, comparer);
				high = partition - 1;
			}
		}
	}
	private static int pickPivotAndPartitionPermutation<TComparer>(int[] permutation, int low, int high, TComparer comparer) where TComparer : struct, IECSListIndexSortComparer
	{
		int middle = low + ((high - low) >> 1);
		swapPermutationIfGreater(permutation, low, middle, comparer);
		swapPermutationIfGreater(permutation, low, high, comparer);
		swapPermutationIfGreater(permutation, middle, high, comparer);
		int pivot = permutation[middle];
		permutation[middle] = permutation[high - 1];
		permutation[high - 1] = pivot;
		int left = low;
		int right = high - 1;
		while (true)
		{
			while (comparer.Compare(permutation[++left], pivot) < 0)
			{
			}
			while (comparer.Compare(pivot, permutation[--right]) < 0)
			{
			}
			if (left >= right)
			{
				break;
			}
			int temp = permutation[left];
			permutation[left] = permutation[right];
			permutation[right] = temp;
		}
		permutation[high - 1] = permutation[left];
		permutation[left] = pivot;
		return left;
	}
	private static void insertionSortPermutation<TComparer>(int[] permutation, int low, int high, TComparer comparer) where TComparer : struct, IECSListIndexSortComparer
	{
		for (int i = low + 1; i <= high; ++i)
		{
			int value = permutation[i];
			int current = i - 1;
			while (current >= low && comparer.Compare(value, permutation[current]) < 0)
			{
				permutation[current + 1] = permutation[current];
				--current;
			}
			permutation[current + 1] = value;
		}
	}
	private static void heapSortPermutation<TComparer>(int[] permutation, int low, int high, TComparer comparer) where TComparer : struct, IECSListIndexSortComparer
	{
		int count = high - low + 1;
		for (int i = count >> 1; i >= 1; --i)
		{
			downHeapPermutation(permutation, i, count, low, comparer);
		}
		for (int i = count; i > 1; --i)
		{
			int temp = permutation[low];
			permutation[low] = permutation[low + i - 1];
			permutation[low + i - 1] = temp;
			downHeapPermutation(permutation, 1, i - 1, low, comparer);
		}
	}
	private static void downHeapPermutation<TComparer>(int[] permutation, int index, int count, int low, TComparer comparer) where TComparer : struct, IECSListIndexSortComparer
	{
		int value = permutation[low + index - 1];
		while (index <= count >> 1)
		{
			int child = index << 1;
			if (child < count && comparer.Compare(permutation[low + child - 1], permutation[low + child]) < 0)
			{
				++child;
			}
			if (comparer.Compare(value, permutation[low + child - 1]) >= 0)
			{
				break;
			}
			permutation[low + index - 1] = permutation[low + child - 1];
			index = child;
		}
		permutation[low + index - 1] = value;
	}
	private static void swapPermutationIfGreater<TComparer>(int[] permutation, int firstIndex, int secondIndex, TComparer comparer) where TComparer : struct, IECSListIndexSortComparer
	{
		if (firstIndex != secondIndex && comparer.Compare(permutation[firstIndex], permutation[secondIndex]) > 0)
		{
			int temp = permutation[firstIndex];
			permutation[firstIndex] = permutation[secondIndex];
			permutation[secondIndex] = temp;
		}
	}
");
			}
			else
			{
				builder.Append(@"private void sortByCore<TComparer>(int index, int count, TComparer comparer) where TComparer : struct, IECSListIndexSortComparer
	{
		introSortBy(index, index + count - 1, 2 * floorLog2PlusOne(count), comparer);
	}
	private void introSortBy<TComparer>(int low, int high, int depthLimit, TComparer comparer) where TComparer : struct, IECSListIndexSortComparer
	{
		while (high > low)
		{
			int partitionSize = high - low + 1;
			if (partitionSize <= 16)
			{
				insertionSortBy(low, high, comparer);
				return;
			}
			if (depthLimit == 0)
			{
				heapSortBy(low, high, comparer);
				return;
			}
			--depthLimit;
			int partition = pickPivotAndPartitionBy(low, high, comparer);
			if (partition - low < high - partition)
			{
				introSortBy(low, partition - 1, depthLimit, comparer);
				low = partition + 1;
			}
			else
			{
				introSortBy(partition + 1, high, depthLimit, comparer);
				high = partition - 1;
			}
		}
	}
	private int pickPivotAndPartitionBy<TComparer>(int low, int high, TComparer comparer) where TComparer : struct, IECSListIndexSortComparer
	{
		int middle = low + ((high - low) >> 1);
		swapIfGreaterBy(low, middle, comparer);
		swapIfGreaterBy(low, high, comparer);
		swapIfGreaterBy(middle, high, comparer);
		swapValue(middle, high - 1);
		int pivotIndex = high - 1;
		int left = low;
		int right = high - 1;
		while (true)
		{
			while (comparer.Compare(++left, pivotIndex) < 0)
			{
			}
			while (comparer.Compare(pivotIndex, --right) < 0)
			{
			}
			if (left >= right)
			{
				break;
			}
			swapValue(left, right);
		}
		swapValue(left, pivotIndex);
		return left;
	}
	private void insertionSortBy<TComparer>(int low, int high, TComparer comparer) where TComparer : struct, IECSListIndexSortComparer
	{
		for (int i = low + 1; i <= high; ++i)
		{
			int current = i;
			while (current > low && comparer.Compare(current, current - 1) < 0)
			{
				swapValue(current, current - 1);
				--current;
			}
		}
	}
	private void heapSortBy<TComparer>(int low, int high, TComparer comparer) where TComparer : struct, IECSListIndexSortComparer
	{
		int count = high - low + 1;
		for (int i = (count >> 1) - 1; i >= 0; --i)
		{
			downHeapBy(i, count, low, comparer);
		}
		for (int end = count - 1; end > 0; --end)
		{
			swapValue(low, low + end);
			downHeapBy(0, end, low, comparer);
		}
	}
	private void downHeapBy<TComparer>(int root, int count, int low, TComparer comparer) where TComparer : struct, IECSListIndexSortComparer
	{
		while (true)
		{
			int child = (root << 1) + 1;
			if (child >= count)
			{
				return;
			}
			if (child + 1 < count && comparer.Compare(low + child, low + child + 1) < 0)
			{
				++child;
			}
			if (comparer.Compare(low + root, low + child) >= 0)
			{
				return;
			}
			swapValue(low + root, low + child);
			root = child;
		}
	}
	private void swapIfGreaterBy<TComparer>(int firstIndex, int secondIndex, TComparer comparer) where TComparer : struct, IECSListIndexSortComparer
	{
		if (firstIndex != secondIndex && comparer.Compare(firstIndex, secondIndex) > 0)
		{
			swapValue(firstIndex, secondIndex);
		}
	}
");
			}
			generateFieldSortMethods(builder, typeName, ecsFields);
			generateFieldSearchMethods(builder, typeName, ecsFields);
		}
		private static void generateFieldSortMethods(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields)
		{
			foreach (IFieldSymbol field in ecsFields)
			{
				string fieldType = getTypeName(field.Type);
				string fieldName = field.Name;
				string suffix = getSortMethodSuffix(fieldName);
				string comparerType = "__ECSFieldSortComparer_" + fieldName;
				string sortMethod = "SortBy" + suffix;
				string binarySearchMethod = "BinarySearchBy" + suffix;
				string getMethod = "getSortField_" + fieldName;
				builder.AppendLine("\tprivate readonly struct " + comparerType + " : IECSListIndexSortComparer");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tprivate readonly " + typeName + "ECSList mOwner;");
				builder.AppendLine("\t\tprivate readonly global::System.Collections.Generic.IComparer<" + fieldType + "> mComparer;");
				builder.AppendLine("\t\tpublic " + comparerType + "(" + typeName + "ECSList owner, global::System.Collections.Generic.IComparer<" + fieldType + "> comparer)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tmOwner = owner;");
				builder.AppendLine("\t\t\tmComparer = comparer;");
				builder.AppendLine("\t\t}");
				appendAggressiveInlining(builder, 2);
				builder.AppendLine("\t\tpublic int Compare(int leftIndex, int rightIndex)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\treturn mComparer.Compare(mOwner." + getMethod + "(leftIndex), mOwner." + getMethod + "(rightIndex));");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic void " + sortMethod + "()");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\t" + sortMethod + "(0, mCount, null);");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic void " + sortMethod + "(global::System.Collections.Generic.IComparer<" + fieldType + "> comparer)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\t" + sortMethod + "(0, mCount, comparer);");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic void " + sortMethod + "(int index, int count, global::System.Collections.Generic.IComparer<" + fieldType + "> comparer)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tif (mDisposed)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSList\");");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tvalidateRange(index, count);");
				builder.AppendLine("\t\tif (count <= 1)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\treturn;");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tif (comparer == null)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tcomparer = global::System.Collections.Generic.Comparer<" + fieldType + ">.Default;");
				builder.AppendLine("\t\t}");
				builder.AppendLine("#if UNITY_EDITOR");
				builder.AppendLine("\t\tinvalidateRefsRange(index, count);");
				builder.AppendLine("\t\tinvalidateColumn();");
				builder.AppendLine("#endif");
				builder.AppendLine("\t\tsortByCore(index, count, new " + comparerType + "(this, comparer));");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic int " + binarySearchMethod + "(" + fieldType + " value)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\treturn " + binarySearchMethod + "(0, mCount, value, null);");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic int " + binarySearchMethod + "(" + fieldType + " value, global::System.Collections.Generic.IComparer<" + fieldType + "> comparer)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\treturn " + binarySearchMethod + "(0, mCount, value, comparer);");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic int " + binarySearchMethod + "(int index, int count, " + fieldType + " value, global::System.Collections.Generic.IComparer<" + fieldType + "> comparer)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tif (mDisposed)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSList\");");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tvalidateRange(index, count);");
				builder.AppendLine("\t\tif (comparer == null)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tcomparer = global::System.Collections.Generic.Comparer<" + fieldType + ">.Default;");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tint low = index;");
				builder.AppendLine("\t\tint high = index + count - 1;");
				builder.AppendLine("\t\twhile (low <= high)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tint middle = low + ((high - low) >> 1);");
				builder.AppendLine("\t\t\tint compare = comparer.Compare(" + getMethod + "(middle), value);");
				builder.AppendLine("\t\t\tif (compare == 0)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\treturn middle;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t\tif (compare < 0)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\tlow = middle + 1;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t\telse");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\thigh = middle - 1;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\treturn ~low;");
				builder.AppendLine("\t}");
			}
		}
		private static void generateFieldSearchMethods(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields)
		{
			foreach (IFieldSymbol field in ecsFields)
			{
				string fieldType = getTypeName(field.Type);
				string fieldName = field.Name;
				string suffix = getSortMethodSuffix(fieldName);
				string getMethod = "getSortField_" + fieldName;
				string containsMethod = "ContainsBy" + suffix;
				string indexOfMethod = "IndexOfBy" + suffix;
				string lastIndexOfMethod = "LastIndexOfBy" + suffix;
				string existsMethod = "ExistsBy" + suffix;
				string findIndexMethod = "FindIndexBy" + suffix;
				string removeAllMethod = "RemoveAllBy" + suffix;
				builder.AppendLine("\tpublic bool " + containsMethod + "(" + fieldType + " value)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\treturn " + indexOfMethod + "(value) >= 0;");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic int " + indexOfMethod + "(" + fieldType + " value)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tif (mDisposed)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSList\");");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tglobal::System.Collections.Generic.EqualityComparer<" + fieldType + "> comparer = global::System.Collections.Generic.EqualityComparer<" + fieldType + ">.Default;");
				builder.AppendLine("\t\tfor (int i = 0; i < mCount; ++i)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tif (comparer.Equals(" + getMethod + "(i), value))");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\treturn i;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\treturn -1;");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic int " + lastIndexOfMethod + "(" + fieldType + " value)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tif (mDisposed)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSList\");");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tglobal::System.Collections.Generic.EqualityComparer<" + fieldType + "> comparer = global::System.Collections.Generic.EqualityComparer<" + fieldType + ">.Default;");
				builder.AppendLine("\t\tfor (int i = mCount - 1; i >= 0; --i)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tif (comparer.Equals(" + getMethod + "(i), value))");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\treturn i;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\treturn -1;");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic bool " + existsMethod + "(global::System.Predicate<" + fieldType + "> match)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\treturn " + findIndexMethod + "(match) >= 0;");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic int " + findIndexMethod + "(global::System.Predicate<" + fieldType + "> match)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\treturn " + findIndexMethod + "(0, mCount, match);");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic int " + findIndexMethod + "(int startIndex, global::System.Predicate<" + fieldType + "> match)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tif (startIndex < 0 || startIndex > mCount)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.ArgumentOutOfRangeException(nameof(startIndex));");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\treturn " + findIndexMethod + "(startIndex, mCount - startIndex, match);");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic int " + findIndexMethod + "(int startIndex, int count, global::System.Predicate<" + fieldType + "> match)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tif (mDisposed)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSList\");");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tif (match == null)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.ArgumentNullException(nameof(match));");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tvalidateRange(startIndex, count);");
				builder.AppendLine("\t\tint endIndex = startIndex + count;");
				builder.AppendLine("\t\tfor (int i = startIndex; i < endIndex; ++i)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tif (match(" + getMethod + "(i)))");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\treturn i;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\treturn -1;");
				builder.AppendLine("\t}");
				builder.AppendLine("\tpublic int " + removeAllMethod + "(global::System.Predicate<" + fieldType + "> match)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tif (mDisposed)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.ObjectDisposedException(\"" + typeName + "ECSList\");");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tif (match == null)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tthrow new global::System.ArgumentNullException(nameof(match));");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tint freeIndex = 0;");
				builder.AppendLine("\t\twhile (freeIndex < mCount && !match(" + getMethod + "(freeIndex)))");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\t++freeIndex;");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tif (freeIndex >= mCount)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\treturn 0;");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tint firstRemovedIndex = freeIndex;");
				builder.AppendLine("\t\tint current = freeIndex + 1;");
				builder.AppendLine("\t\twhile (current < mCount)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\twhile (current < mCount && match(" + getMethod + "(current)))");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\t++current;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t\tif (current >= mCount)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\tbreak;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t\tint runStart = current;");
				builder.AppendLine("\t\t\t++current;");
				builder.AppendLine("\t\t\twhile (current < mCount && !match(" + getMethod + "(current)))");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\t++current;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t\tint runCount = current - runStart;");
				builder.AppendLine("\t\t\tif (runCount >= 8)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\tmoveRange(runStart, freeIndex, runCount);");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t\telse");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\tfor (int i = 0; i < runCount; ++i)");
				builder.AppendLine("\t\t\t\t{");
				builder.AppendLine("\t\t\t\t\tcopyValue(runStart + i, freeIndex + i);");
				builder.AppendLine("\t\t\t\t}");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t\tfreeIndex += runCount;");
				builder.AppendLine("\t\t\tif (current < mCount)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\t++current;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\tint removedCount = mCount - freeIndex;");
				builder.AppendLine("#if UNITY_EDITOR");
				builder.AppendLine("\t\tinvalidateRefsFrom(firstRemovedIndex);");
				builder.AppendLine("\t\tinvalidateColumn();");
				builder.AppendLine("#endif");
				builder.AppendLine("\t\tclearRange(freeIndex, removedCount);");
				builder.AppendLine("\t\tmCount = freeIndex;");
				builder.AppendLine("\t\treturn removedCount;");
				builder.AppendLine("\t}");
			}
		}
		private static void generateUnsafeExtendedListHelpers(StringBuilder builder, string typeName, string fullTypeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, bool hasSpan)
		{
			bool hasManagedECS = ecsFields.Any(field => !field.Type.IsUnmanagedType);
			bool hasManagedAoS = aosFields.Any(field => !field.Type.IsUnmanagedType);
			bool usePermutationSort = hasManagedECS || hasManagedAoS;
			builder.AppendLine("\tprivate void moveRange(int sourceIndex, int destinationIndex, int count)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (count <= 0 || sourceIndex == destinationIndex)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tbool overlap = sourceIndex < destinationIndex + count && destinationIndex < sourceIndex + count;");
			if (hasSpan)
			{
				foreach (IFieldSymbol field in ecsFields)
				{
					if (field.Type.IsUnmanagedType)
					{
						string fieldType = getTypeName(field.Type);
						builder.AppendLine("\t\tif (overlap)");
						builder.AppendLine("\t\t{");
						builder.AppendLine("\t\t\tnew global::System.Span<" + fieldType + ">(mStorage->" + fieldAccess(field) + " + sourceIndex, count).CopyTo(new global::System.Span<" + fieldType + ">(mStorage->" + fieldAccess(field) + " + destinationIndex, count));");
						builder.AppendLine("\t\t}");
						builder.AppendLine("\t\telse");
						builder.AppendLine("\t\t{");
						builder.AppendLine("\t\t\tcopyMemory(mStorage->" + fieldAccess(field) + " + destinationIndex, mStorage->" + fieldAccess(field) + " + sourceIndex, (long)count * sizeof(" + fieldType + "));");
						builder.AppendLine("\t\t}");
					}
				}
				if (aosFields.Count > 0 && !hasManagedAoS)
				{
					builder.AppendLine("\t\tif (overlap)");
					builder.AppendLine("\t\t{");
					builder.AppendLine("\t\t\tnew global::System.Span<" + typeName + "AoSBlock>(mStorage->mAoS + sourceIndex, count).CopyTo(new global::System.Span<" + typeName + "AoSBlock>(mStorage->mAoS + destinationIndex, count));");
					builder.AppendLine("\t\t}");
					builder.AppendLine("\t\telse");
					builder.AppendLine("\t\t{");
					builder.AppendLine("\t\t\tcopyMemory(mStorage->mAoS + destinationIndex, mStorage->mAoS + sourceIndex, (long)count * sizeof(" + typeName + "AoSBlock));");
					builder.AppendLine("\t\t}");
				}
			}
			else
			{
				builder.AppendLine("\t\tif (!overlap)");
				builder.AppendLine("\t\t{");
				foreach (IFieldSymbol field in ecsFields)
				{
					if (field.Type.IsUnmanagedType)
					{
						string fieldType = getTypeName(field.Type);
						builder.AppendLine("\t\t\tcopyMemory(mStorage->" + fieldAccess(field) + " + destinationIndex, mStorage->" + fieldAccess(field) + " + sourceIndex, (long)count * sizeof(" + fieldType + "));");
					}
				}
				if (aosFields.Count > 0 && !hasManagedAoS)
				{
					builder.AppendLine("\t\t\tcopyMemory(mStorage->mAoS + destinationIndex, mStorage->mAoS + sourceIndex, (long)count * sizeof(" + typeName + "AoSBlock));");
				}
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\telse if (destinationIndex > sourceIndex)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tfor (int i = count - 1; i >= 0; --i)");
				builder.AppendLine("\t\t\t{");
				foreach (IFieldSymbol field in ecsFields)
				{
					if (field.Type.IsUnmanagedType)
					{
						builder.AppendLine("\t\t\t\tmStorage->" + fieldAccess(field) + "[destinationIndex + i] = mStorage->" + fieldAccess(field) + "[sourceIndex + i];");
					}
				}
				if (aosFields.Count > 0 && !hasManagedAoS)
				{
					builder.AppendLine("\t\t\t\tmStorage->mAoS[destinationIndex + i] = mStorage->mAoS[sourceIndex + i];");
				}
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t\telse");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tfor (int i = 0; i < count; ++i)");
				builder.AppendLine("\t\t\t{");
				foreach (IFieldSymbol field in ecsFields)
				{
					if (field.Type.IsUnmanagedType)
					{
						builder.AppendLine("\t\t\t\tmStorage->" + fieldAccess(field) + "[destinationIndex + i] = mStorage->" + fieldAccess(field) + "[sourceIndex + i];");
					}
				}
				if (aosFields.Count > 0 && !hasManagedAoS)
				{
					builder.AppendLine("\t\t\t\tmStorage->mAoS[destinationIndex + i] = mStorage->mAoS[sourceIndex + i];");
				}
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t}");
			}
			foreach (IFieldSymbol field in ecsFields)
			{
				if (!field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\tglobal::System.Array.Copy(mManagedStorage." + fieldAccess(field) + ", sourceIndex, mManagedStorage." + fieldAccess(field) + ", destinationIndex, count);");
				}
			}
			if (hasManagedAoS)
			{
				builder.AppendLine("\t\tglobal::System.Array.Copy(mManagedStorage.mAoS, sourceIndex, mManagedStorage.mAoS, destinationIndex, count);");
			}
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void copyRangeFrom(" + typeName + "ECSList source, int sourceIndex, int destinationIndex, int count)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (count <= 0)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\tif (global::System.Object.ReferenceEquals(this, source))");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tmoveRange(sourceIndex, destinationIndex, count);");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (field.Type.IsUnmanagedType)
				{
					string fieldType = getTypeName(field.Type);
					builder.AppendLine("\t\tcopyMemory(mStorage->" + fieldAccess(field) + " + destinationIndex, source.mStorage->" + fieldAccess(field) + " + sourceIndex, (long)count * sizeof(" + fieldType + "));");
				}
				else
				{
					builder.AppendLine("\t\tglobal::System.Array.Copy(source.mManagedStorage." + fieldAccess(field) + ", sourceIndex, mManagedStorage." + fieldAccess(field) + ", destinationIndex, count);");
				}
			}
			if (aosFields.Count > 0)
			{
				if (hasManagedAoS)
				{
					builder.AppendLine("\t\tglobal::System.Array.Copy(source.mManagedStorage.mAoS, sourceIndex, mManagedStorage.mAoS, destinationIndex, count);");
				}
				else
				{
					builder.AppendLine("\t\tcopyMemory(mStorage->mAoS + destinationIndex, source.mStorage->mAoS + sourceIndex, (long)count * sizeof(" + typeName + "AoSBlock));");
				}
			}
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void copyFromArray(" + fullTypeName + "[] source, int sourceIndex, int destinationIndex, int count)");
			builder.AppendLine("\t{");
			if (hasManagedECS || hasManagedAoS)
			{
				for (int fieldIndex = 0; fieldIndex < ecsFields.Count; ++fieldIndex)
				{
					IFieldSymbol field = ecsFields[fieldIndex];
					string fieldType = getTypeName(field.Type);
					if (field.Type.IsUnmanagedType)
					{
						builder.AppendLine("\t\t" + fieldType + "* column" + fieldIndex + " = mStorage->" + fieldAccess(field) + ";");
					}
					else
					{
						builder.AppendLine("\t\t" + fieldType + "[] column" + fieldIndex + " = mManagedStorage." + fieldAccess(field) + ";");
					}
				}
				if (aosFields.Count > 0)
				{
					if (hasManagedAoS)
					{
						builder.AppendLine("\t\t" + typeName + "AoSBlock[] aosColumn = mManagedStorage.mAoS;");
					}
					else
					{
						builder.AppendLine("\t\t" + typeName + "AoSBlock* aosColumn = mStorage->mAoS;");
					}
				}
				builder.AppendLine("\t\tint sourceCursor = sourceIndex;");
				builder.AppendLine("\t\tint destinationCursor = destinationIndex;");
				builder.AppendLine("\t\tint sourceEnd = sourceIndex + count;");
				builder.AppendLine("\t\tfor (; sourceCursor < sourceEnd; ++sourceCursor, ++destinationCursor)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tref " + fullTypeName + " sourceValue = ref source[sourceCursor];");
				for (int fieldIndex = 0; fieldIndex < ecsFields.Count; ++fieldIndex)
				{
					IFieldSymbol field = ecsFields[fieldIndex];
					builder.AppendLine("\t\t\tcolumn" + fieldIndex + "[destinationCursor] = sourceValue." + fieldAccess(field) + ";");
				}
				if (aosFields.Count > 0)
				{
					builder.AppendLine("\t\t\t" + typeName + "AoSBlock aosValue = default(" + typeName + "AoSBlock);");
					foreach (IFieldSymbol field in aosFields)
					{
						builder.AppendLine("\t\t\taosValue." + fieldAccess(field) + " = sourceValue." + fieldAccess(field) + ";");
					}
					builder.AppendLine("\t\t\taosColumn[destinationCursor] = aosValue;");
				}
				builder.AppendLine("\t\t}");
			}
			else
			{
				foreach (IFieldSymbol field in ecsFields)
				{
					builder.AppendLine("\t\tfor (int i = 0; i < count; ++i)");
					builder.AppendLine("\t\t{");
					builder.AppendLine("\t\t\tmStorage->" + fieldAccess(field) + "[destinationIndex + i] = source[sourceIndex + i]." + fieldAccess(field) + ";");
					builder.AppendLine("\t\t}");
				}
				if (aosFields.Count > 0)
				{
					builder.AppendLine("\t\tfor (int i = 0; i < count; ++i)");
					builder.AppendLine("\t\t{");
					foreach (IFieldSymbol field in aosFields)
					{
						builder.AppendLine("\t\t\tmStorage->mAoS[destinationIndex + i]." + fieldAccess(field) + " = source[sourceIndex + i]." + fieldAccess(field) + ";");
					}
					builder.AppendLine("\t\t}");
				}
			}
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void copyToArray(int sourceIndex, " + fullTypeName + "[] destination, int destinationIndex, int count)");
			builder.AppendLine("\t{");
			if (hasManagedECS || hasManagedAoS)
			{
				for (int fieldIndex = 0; fieldIndex < ecsFields.Count; ++fieldIndex)
				{
					IFieldSymbol field = ecsFields[fieldIndex];
					string fieldType = getTypeName(field.Type);
					if (field.Type.IsUnmanagedType)
					{
						builder.AppendLine("\t\t" + fieldType + "* column" + fieldIndex + " = mStorage->" + fieldAccess(field) + ";");
					}
					else
					{
						builder.AppendLine("\t\t" + fieldType + "[] column" + fieldIndex + " = mManagedStorage." + fieldAccess(field) + ";");
					}
				}
				if (aosFields.Count > 0)
				{
					if (hasManagedAoS)
					{
						builder.AppendLine("\t\t" + typeName + "AoSBlock[] aosColumn = mManagedStorage.mAoS;");
					}
					else
					{
						builder.AppendLine("\t\t" + typeName + "AoSBlock* aosColumn = mStorage->mAoS;");
					}
				}
				builder.AppendLine("\t\tint sourceCursor = sourceIndex;");
				builder.AppendLine("\t\tint destinationCursor = destinationIndex;");
				builder.AppendLine("\t\tint sourceEnd = sourceIndex + count;");
				builder.AppendLine("\t\tfor (; sourceCursor < sourceEnd; ++sourceCursor, ++destinationCursor)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tref " + fullTypeName + " destinationValue = ref destination[destinationCursor];");
				for (int fieldIndex = 0; fieldIndex < ecsFields.Count; ++fieldIndex)
				{
					IFieldSymbol field = ecsFields[fieldIndex];
					builder.AppendLine("\t\t\tdestinationValue." + fieldAccess(field) + " = column" + fieldIndex + "[sourceCursor];");
				}
				if (aosFields.Count > 0)
				{
					foreach (IFieldSymbol field in aosFields)
					{
						builder.AppendLine("\t\t\tdestinationValue." + fieldAccess(field) + " = aosColumn[sourceCursor]." + fieldAccess(field) + ";");
					}
				}
				builder.AppendLine("\t\t}");
			}
			else
			{
				builder.AppendLine("\t\tfor (int i = 0; i < count; ++i)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tref " + fullTypeName + " value = ref destination[destinationIndex + i];");
				foreach (IFieldSymbol field in ecsFields)
				{
					builder.AppendLine("\t\t\tvalue." + fieldAccess(field) + " = mStorage->" + fieldAccess(field) + "[sourceIndex + i];");
				}
				foreach (IFieldSymbol field in aosFields)
				{
					builder.AppendLine("\t\t\tvalue." + fieldAccess(field) + " = mStorage->mAoS[sourceIndex + i]." + fieldAccess(field) + ";");
				}
				builder.AppendLine("\t\t}");
			}
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void clearRange(int index, int count)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (count <= 0)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (!field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\tglobal::System.Array.Clear(mManagedStorage." + fieldAccess(field) + ", index, count);");
				}
			}
			if (hasManagedAoS)
			{
				builder.AppendLine("\t\tglobal::System.Array.Clear(mManagedStorage.mAoS, index, count);");
			}
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate void copyValue(int sourceIndex, int destinationIndex)");
			builder.AppendLine("\t{");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\tmStorage->" + fieldAccess(field) + "[destinationIndex] = mStorage->" + fieldAccess(field) + "[sourceIndex];");
				}
				else
				{
					builder.AppendLine("\t\tmManagedStorage." + fieldAccess(field) + "[destinationIndex] = mManagedStorage." + fieldAccess(field) + "[sourceIndex];");
				}
			}
			if (aosFields.Count > 0)
			{
				if (hasManagedAoS)
				{
					builder.AppendLine("\t\tmManagedStorage.mAoS[destinationIndex] = mManagedStorage.mAoS[sourceIndex];");
				}
				else
				{
					builder.AppendLine("\t\tmStorage->mAoS[destinationIndex] = mStorage->mAoS[sourceIndex];");
				}
			}
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void reverseRange(int index, int count)");
			builder.AppendLine("\t{");
			int reverseIndex = 0;
			foreach (IFieldSymbol field in ecsFields)
			{
				string fieldType = getTypeName(field.Type);
				string leftName = "left" + reverseIndex;
				string rightName = "right" + reverseIndex;
				string tempName = "tempReverse" + reverseIndex++;
				builder.AppendLine("\t\tint " + leftName + " = index;");
				builder.AppendLine("\t\tint " + rightName + " = index + count - 1;");
				builder.AppendLine("\t\twhile (" + leftName + " < " + rightName + ")");
				builder.AppendLine("\t\t{");
				if (field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\t\t" + fieldType + " " + tempName + " = mStorage->" + fieldAccess(field) + "[" + leftName + "];");
					builder.AppendLine("\t\t\tmStorage->" + fieldAccess(field) + "[" + leftName + "] = mStorage->" + fieldAccess(field) + "[" + rightName + "];");
					builder.AppendLine("\t\t\tmStorage->" + fieldAccess(field) + "[" + rightName + "] = " + tempName + ";");
				}
				else
				{
					builder.AppendLine("\t\t\t" + fieldType + " " + tempName + " = mManagedStorage." + fieldAccess(field) + "[" + leftName + "];");
					builder.AppendLine("\t\t\tmManagedStorage." + fieldAccess(field) + "[" + leftName + "] = mManagedStorage." + fieldAccess(field) + "[" + rightName + "];");
					builder.AppendLine("\t\t\tmManagedStorage." + fieldAccess(field) + "[" + rightName + "] = " + tempName + ";");
				}
				builder.AppendLine("\t\t\t++" + leftName + ";");
				builder.AppendLine("\t\t\t--" + rightName + ";");
				builder.AppendLine("\t\t}");
			}
			if (aosFields.Count > 0)
			{
				string leftName = "left" + reverseIndex;
				string rightName = "right" + reverseIndex;
				string tempName = "tempReverse" + reverseIndex;
				builder.AppendLine("\t\tint " + leftName + " = index;");
				builder.AppendLine("\t\tint " + rightName + " = index + count - 1;");
				builder.AppendLine("\t\twhile (" + leftName + " < " + rightName + ")");
				builder.AppendLine("\t\t{");
				if (hasManagedAoS)
				{
					builder.AppendLine("\t\t\t" + typeName + "AoSBlock " + tempName + " = mManagedStorage.mAoS[" + leftName + "];");
					builder.AppendLine("\t\t\tmManagedStorage.mAoS[" + leftName + "] = mManagedStorage.mAoS[" + rightName + "];");
					builder.AppendLine("\t\t\tmManagedStorage.mAoS[" + rightName + "] = " + tempName + ";");
				}
				else
				{
					builder.AppendLine("\t\t\t" + typeName + "AoSBlock " + tempName + " = mStorage->mAoS[" + leftName + "];");
					builder.AppendLine("\t\t\tmStorage->mAoS[" + leftName + "] = mStorage->mAoS[" + rightName + "];");
					builder.AppendLine("\t\t\tmStorage->mAoS[" + rightName + "] = " + tempName + ";");
				}
				builder.AppendLine("\t\t\t++" + leftName + ";");
				builder.AppendLine("\t\t\t--" + rightName + ";");
				builder.AppendLine("\t\t}");
			}
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void swapValue(int firstIndex, int secondIndex)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (firstIndex == secondIndex)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			int tempIndex = 0;
			foreach (IFieldSymbol field in ecsFields)
			{
				string fieldType = getTypeName(field.Type);
				string tempName = "temp" + tempIndex++;
				if (field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\t" + fieldType + " " + tempName + " = mStorage->" + fieldAccess(field) + "[firstIndex];");
					builder.AppendLine("\t\tmStorage->" + fieldAccess(field) + "[firstIndex] = mStorage->" + fieldAccess(field) + "[secondIndex];");
					builder.AppendLine("\t\tmStorage->" + fieldAccess(field) + "[secondIndex] = " + tempName + ";");
				}
				else
				{
					builder.AppendLine("\t\t" + fieldType + " " + tempName + " = mManagedStorage." + fieldAccess(field) + "[firstIndex];");
					builder.AppendLine("\t\tmManagedStorage." + fieldAccess(field) + "[firstIndex] = mManagedStorage." + fieldAccess(field) + "[secondIndex];");
					builder.AppendLine("\t\tmManagedStorage." + fieldAccess(field) + "[secondIndex] = " + tempName + ";");
				}
			}
			if (aosFields.Count > 0)
			{
				string tempName = "temp" + tempIndex;
				if (hasManagedAoS)
				{
					builder.AppendLine("\t\t" + typeName + "AoSBlock " + tempName + " = mManagedStorage.mAoS[firstIndex];");
					builder.AppendLine("\t\tmManagedStorage.mAoS[firstIndex] = mManagedStorage.mAoS[secondIndex];");
					builder.AppendLine("\t\tmManagedStorage.mAoS[secondIndex] = " + tempName + ";");
				}
				else
				{
					builder.AppendLine("\t\t" + typeName + "AoSBlock " + tempName + " = mStorage->mAoS[firstIndex];");
					builder.AppendLine("\t\tmStorage->mAoS[firstIndex] = mStorage->mAoS[secondIndex];");
					builder.AppendLine("\t\tmStorage->mAoS[secondIndex] = " + tempName + ";");
				}
			}
			builder.AppendLine("\t}");
			if (usePermutationSort)
			{
				builder.AppendLine("\tprivate void applySortPermutation(int index, int count, int[] permutation)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\tfor (int start = 0; start < count; ++start)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tint firstSource = permutation[start];");
				builder.AppendLine("\t\t\tif (firstSource < 0)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\tcontinue;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t\tif (firstSource == index + start)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\tpermutation[start] = ~firstSource;");
				builder.AppendLine("\t\t\t\tcontinue;");
				builder.AppendLine("\t\t\t}");
				int permutationTempIndex = 0;
				foreach (IFieldSymbol field in ecsFields)
				{
					string fieldType = getTypeName(field.Type);
					string tempName = "sortTemp" + permutationTempIndex++;
					if (field.Type.IsUnmanagedType)
					{
						builder.AppendLine("\t\t\t" + fieldType + " " + tempName + " = mStorage->" + fieldAccess(field) + "[index + start];");
					}
					else
					{
						builder.AppendLine("\t\t\t" + fieldType + " " + tempName + " = mManagedStorage." + fieldAccess(field) + "[index + start];");
					}
				}
				string aosPermutationTempName = null;
				if (aosFields.Count > 0)
				{
					aosPermutationTempName = "sortTemp" + permutationTempIndex;
					if (hasManagedAoS)
					{
						builder.AppendLine("\t\t\t" + typeName + "AoSBlock " + aosPermutationTempName + " = mManagedStorage.mAoS[index + start];");
					}
					else
					{
						builder.AppendLine("\t\t\t" + typeName + "AoSBlock " + aosPermutationTempName + " = mStorage->mAoS[index + start];");
					}
				}
				builder.AppendLine("\t\t\tint current = start;");
				builder.AppendLine("\t\t\twhile (true)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\tint sourceAbsolute = permutation[current];");
				builder.AppendLine("\t\t\t\tint sourceOffset = sourceAbsolute - index;");
				builder.AppendLine("\t\t\t\tpermutation[current] = ~sourceAbsolute;");
				builder.AppendLine("\t\t\t\tif (sourceOffset == start)");
				builder.AppendLine("\t\t\t\t{");
				builder.AppendLine("\t\t\t\t\tbreak;");
				builder.AppendLine("\t\t\t\t}");
				builder.AppendLine("\t\t\t\tint destinationIndex = index + current;");
				builder.AppendLine("\t\t\t\tint sourceIndex = index + sourceOffset;");
				foreach (IFieldSymbol field in ecsFields)
				{
					if (field.Type.IsUnmanagedType)
					{
						builder.AppendLine("\t\t\t\tmStorage->" + fieldAccess(field) + "[destinationIndex] = mStorage->" + fieldAccess(field) + "[sourceIndex];");
					}
					else
					{
						builder.AppendLine("\t\t\t\tmManagedStorage." + fieldAccess(field) + "[destinationIndex] = mManagedStorage." + fieldAccess(field) + "[sourceIndex];");
					}
				}
				if (aosFields.Count > 0)
				{
					if (hasManagedAoS)
					{
						builder.AppendLine("\t\t\t\tmManagedStorage.mAoS[destinationIndex] = mManagedStorage.mAoS[sourceIndex];");
					}
					else
					{
						builder.AppendLine("\t\t\t\tmStorage->mAoS[destinationIndex] = mStorage->mAoS[sourceIndex];");
					}
				}
				builder.AppendLine("\t\t\t\tcurrent = sourceOffset;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t\tint finalIndex = index + current;");
				permutationTempIndex = 0;
				foreach (IFieldSymbol field in ecsFields)
				{
					string tempName = "sortTemp" + permutationTempIndex++;
					if (field.Type.IsUnmanagedType)
					{
						builder.AppendLine("\t\t\tmStorage->" + fieldAccess(field) + "[finalIndex] = " + tempName + ";");
					}
					else
					{
						builder.AppendLine("\t\t\tmManagedStorage." + fieldAccess(field) + "[finalIndex] = " + tempName + ";");
					}
				}
				if (aosFields.Count > 0)
				{
					if (hasManagedAoS)
					{
						builder.AppendLine("\t\t\tmManagedStorage.mAoS[finalIndex] = " + aosPermutationTempName + ";");
					}
					else
					{
						builder.AppendLine("\t\t\tmStorage->mAoS[finalIndex] = " + aosPermutationTempName + ";");
					}
				}
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t}");
			}
			foreach (IFieldSymbol field in ecsFields)
			{
				appendAggressiveInlining(builder, 1);
				builder.AppendLine("\tprivate " + getTypeName(field.Type) + " getSortField_" + field.Name + "(int index)");
				builder.AppendLine("\t{");
				if (field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\treturn mStorage->" + fieldAccess(field) + "[index];");
				}
				else
				{
					builder.AppendLine("\t\treturn mManagedStorage." + fieldAccess(field) + "[index];");
				}
				builder.AppendLine("\t}");
			}
		}
		private static void generateSafeExtendedListHelpers(StringBuilder builder, string typeName, string fullTypeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, bool spanBackend)
		{
			bool hasManagedECS = ecsFields.Any(field => !field.Type.IsUnmanagedType);
			bool hasManagedAoS = aosFields.Any(field => !field.Type.IsUnmanagedType);
			bool usePermutationSort = hasManagedECS || hasManagedAoS;
			string storageDeclaration = spanBackend ? "ref " + typeName + "Storage storage = ref mStorage[0];" : typeName + "Storage storage = " + typeName + "StorageRegistry.getStorage(mStorageID);";
			string sourceStorageDeclaration = spanBackend ? "ref " + typeName + "Storage sourceStorage = ref source.mStorage[0];" : typeName + "Storage sourceStorage = " + typeName + "StorageRegistry.getStorage(source.mStorageID);";
			builder.AppendLine("\tprivate void moveRange(int sourceIndex, int destinationIndex, int count)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (count <= 0 || sourceIndex == destinationIndex)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\t" + storageDeclaration);
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\tglobal::System.Array.Copy(storage." + fieldAccess(field) + ", sourceIndex, storage." + fieldAccess(field) + ", destinationIndex, count);");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\tglobal::System.Array.Copy(storage.mAoS, sourceIndex, storage.mAoS, destinationIndex, count);");
			}
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void copyRangeFrom(" + typeName + "ECSList source, int sourceIndex, int destinationIndex, int count)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (count <= 0)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\t" + storageDeclaration);
			builder.AppendLine("\t\t" + sourceStorageDeclaration);
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\tglobal::System.Array.Copy(sourceStorage." + fieldAccess(field) + ", sourceIndex, storage." + fieldAccess(field) + ", destinationIndex, count);");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\tglobal::System.Array.Copy(sourceStorage.mAoS, sourceIndex, storage.mAoS, destinationIndex, count);");
			}
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void copyFromArray(" + fullTypeName + "[] source, int sourceIndex, int destinationIndex, int count)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\t" + storageDeclaration);
			if (hasManagedECS || hasManagedAoS)
			{
				for (int fieldIndex = 0; fieldIndex < ecsFields.Count; ++fieldIndex)
				{
					IFieldSymbol field = ecsFields[fieldIndex];
					builder.AppendLine("\t\t" + getTypeName(field.Type) + "[] column" + fieldIndex + " = storage." + fieldAccess(field) + ";");
				}
				if (aosFields.Count > 0)
				{
					builder.AppendLine("\t\t" + typeName + "AoSBlock[] aosColumn = storage.mAoS;");
				}
				builder.AppendLine("\t\tint sourceCursor = sourceIndex;");
				builder.AppendLine("\t\tint destinationCursor = destinationIndex;");
				builder.AppendLine("\t\tint sourceEnd = sourceIndex + count;");
				builder.AppendLine("\t\tfor (; sourceCursor < sourceEnd; ++sourceCursor, ++destinationCursor)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tref " + fullTypeName + " sourceValue = ref source[sourceCursor];");
				for (int fieldIndex = 0; fieldIndex < ecsFields.Count; ++fieldIndex)
				{
					IFieldSymbol field = ecsFields[fieldIndex];
					builder.AppendLine("\t\t\tcolumn" + fieldIndex + "[destinationCursor] = sourceValue." + fieldAccess(field) + ";");
				}
				if (aosFields.Count > 0)
				{
					builder.AppendLine("\t\t\t" + typeName + "AoSBlock aosValue = default(" + typeName + "AoSBlock);");
					foreach (IFieldSymbol field in aosFields)
					{
						builder.AppendLine("\t\t\taosValue." + fieldAccess(field) + " = sourceValue." + fieldAccess(field) + ";");
					}
					builder.AppendLine("\t\t\taosColumn[destinationCursor] = aosValue;");
				}
				builder.AppendLine("\t\t}");
			}
			else
			{
				foreach (IFieldSymbol field in ecsFields)
				{
					builder.AppendLine("\t\tfor (int i = 0; i < count; ++i)");
					builder.AppendLine("\t\t{");
					builder.AppendLine("\t\t\tstorage." + fieldAccess(field) + "[destinationIndex + i] = source[sourceIndex + i]." + fieldAccess(field) + ";");
					builder.AppendLine("\t\t}");
				}
				if (aosFields.Count > 0)
				{
					builder.AppendLine("\t\tfor (int i = 0; i < count; ++i)");
					builder.AppendLine("\t\t{");
					foreach (IFieldSymbol field in aosFields)
					{
						builder.AppendLine("\t\t\tstorage.mAoS[destinationIndex + i]." + fieldAccess(field) + " = source[sourceIndex + i]." + fieldAccess(field) + ";");
					}
					builder.AppendLine("\t\t}");
				}
			}
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void copyToArray(int sourceIndex, " + fullTypeName + "[] destination, int destinationIndex, int count)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\t" + storageDeclaration);
			if (hasManagedECS || hasManagedAoS)
			{
				for (int fieldIndex = 0; fieldIndex < ecsFields.Count; ++fieldIndex)
				{
					IFieldSymbol field = ecsFields[fieldIndex];
					builder.AppendLine("\t\t" + getTypeName(field.Type) + "[] column" + fieldIndex + " = storage." + fieldAccess(field) + ";");
				}
				if (aosFields.Count > 0)
				{
					builder.AppendLine("\t\t" + typeName + "AoSBlock[] aosColumn = storage.mAoS;");
				}
				builder.AppendLine("\t\tint sourceCursor = sourceIndex;");
				builder.AppendLine("\t\tint destinationCursor = destinationIndex;");
				builder.AppendLine("\t\tint sourceEnd = sourceIndex + count;");
				builder.AppendLine("\t\tfor (; sourceCursor < sourceEnd; ++sourceCursor, ++destinationCursor)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tref " + fullTypeName + " destinationValue = ref destination[destinationCursor];");
				for (int fieldIndex = 0; fieldIndex < ecsFields.Count; ++fieldIndex)
				{
					IFieldSymbol field = ecsFields[fieldIndex];
					builder.AppendLine("\t\t\tdestinationValue." + fieldAccess(field) + " = column" + fieldIndex + "[sourceCursor];");
				}
				if (aosFields.Count > 0)
				{
					foreach (IFieldSymbol field in aosFields)
					{
						builder.AppendLine("\t\t\tdestinationValue." + fieldAccess(field) + " = aosColumn[sourceCursor]." + fieldAccess(field) + ";");
					}
				}
				builder.AppendLine("\t\t}");
			}
			else
			{
				builder.AppendLine("\t\tfor (int i = 0; i < count; ++i)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tref " + fullTypeName + " value = ref destination[destinationIndex + i];");
				foreach (IFieldSymbol field in ecsFields)
				{
					builder.AppendLine("\t\t\tvalue." + fieldAccess(field) + " = storage." + fieldAccess(field) + "[sourceIndex + i];");
				}
				foreach (IFieldSymbol field in aosFields)
				{
					builder.AppendLine("\t\t\tvalue." + fieldAccess(field) + " = storage.mAoS[sourceIndex + i]." + fieldAccess(field) + ";");
				}
				builder.AppendLine("\t\t}");
			}
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void clearRange(int index, int count)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (count <= 0)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			if (hasManagedECS || hasManagedAoS)
			{
				builder.AppendLine("\t\t" + storageDeclaration);
				foreach (IFieldSymbol field in ecsFields)
				{
					if (!field.Type.IsUnmanagedType)
					{
						builder.AppendLine("\t\tglobal::System.Array.Clear(storage." + fieldAccess(field) + ", index, count);");
					}
				}
				if (hasManagedAoS)
				{
					builder.AppendLine("\t\tglobal::System.Array.Clear(storage.mAoS, index, count);");
				}
			}
			builder.AppendLine("\t}");
			appendAggressiveInlining(builder, 1);
			builder.AppendLine("\tprivate void copyValue(int sourceIndex, int destinationIndex)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\t" + storageDeclaration);
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\tstorage." + fieldAccess(field) + "[destinationIndex] = storage." + fieldAccess(field) + "[sourceIndex];");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\tstorage.mAoS[destinationIndex] = storage.mAoS[sourceIndex];");
			}
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void reverseRange(int index, int count)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\t" + storageDeclaration);
			int reverseIndex = 0;
			foreach (IFieldSymbol field in ecsFields)
			{
				string fieldType = getTypeName(field.Type);
				string leftName = "left" + reverseIndex;
				string rightName = "right" + reverseIndex;
				string tempName = "tempReverse" + reverseIndex++;
				builder.AppendLine("\t\tint " + leftName + " = index;");
				builder.AppendLine("\t\tint " + rightName + " = index + count - 1;");
				builder.AppendLine("\t\twhile (" + leftName + " < " + rightName + ")");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\t" + fieldType + " " + tempName + " = storage." + fieldAccess(field) + "[" + leftName + "];");
				builder.AppendLine("\t\t\tstorage." + fieldAccess(field) + "[" + leftName + "] = storage." + fieldAccess(field) + "[" + rightName + "];");
				builder.AppendLine("\t\t\tstorage." + fieldAccess(field) + "[" + rightName + "] = " + tempName + ";");
				builder.AppendLine("\t\t\t++" + leftName + ";");
				builder.AppendLine("\t\t\t--" + rightName + ";");
				builder.AppendLine("\t\t}");
			}
			if (aosFields.Count > 0)
			{
				string leftName = "left" + reverseIndex;
				string rightName = "right" + reverseIndex;
				string tempName = "tempReverse" + reverseIndex;
				builder.AppendLine("\t\tint " + leftName + " = index;");
				builder.AppendLine("\t\tint " + rightName + " = index + count - 1;");
				builder.AppendLine("\t\twhile (" + leftName + " < " + rightName + ")");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\t" + typeName + "AoSBlock " + tempName + " = storage.mAoS[" + leftName + "];");
				builder.AppendLine("\t\t\tstorage.mAoS[" + leftName + "] = storage.mAoS[" + rightName + "];");
				builder.AppendLine("\t\t\tstorage.mAoS[" + rightName + "] = " + tempName + ";");
				builder.AppendLine("\t\t\t++" + leftName + ";");
				builder.AppendLine("\t\t\t--" + rightName + ";");
				builder.AppendLine("\t\t}");
			}
			builder.AppendLine("\t}");
			builder.AppendLine("\tprivate void swapValue(int firstIndex, int secondIndex)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tif (firstIndex == secondIndex)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\treturn;");
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t\t" + storageDeclaration);
			int tempIndex = 0;
			foreach (IFieldSymbol field in ecsFields)
			{
				string fieldType = getTypeName(field.Type);
				string tempName = "temp" + tempIndex++;
				builder.AppendLine("\t\t" + fieldType + " " + tempName + " = storage." + fieldAccess(field) + "[firstIndex];");
				builder.AppendLine("\t\tstorage." + fieldAccess(field) + "[firstIndex] = storage." + fieldAccess(field) + "[secondIndex];");
				builder.AppendLine("\t\tstorage." + fieldAccess(field) + "[secondIndex] = " + tempName + ";");
			}
			if (aosFields.Count > 0)
			{
				string tempName = "temp" + tempIndex;
				builder.AppendLine("\t\t" + typeName + "AoSBlock " + tempName + " = storage.mAoS[firstIndex];");
				builder.AppendLine("\t\tstorage.mAoS[firstIndex] = storage.mAoS[secondIndex];");
				builder.AppendLine("\t\tstorage.mAoS[secondIndex] = " + tempName + ";");
			}
			builder.AppendLine("\t}");
			if (usePermutationSort)
			{
				builder.AppendLine("\tprivate void applySortPermutation(int index, int count, int[] permutation)");
				builder.AppendLine("\t{");
				builder.AppendLine("\t\t" + storageDeclaration);
				builder.AppendLine("\t\tfor (int start = 0; start < count; ++start)");
				builder.AppendLine("\t\t{");
				builder.AppendLine("\t\t\tint firstSource = permutation[start];");
				builder.AppendLine("\t\t\tif (firstSource < 0)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\tcontinue;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t\tif (firstSource == index + start)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\tpermutation[start] = ~firstSource;");
				builder.AppendLine("\t\t\t\tcontinue;");
				builder.AppendLine("\t\t\t}");
				int permutationTempIndex = 0;
				foreach (IFieldSymbol field in ecsFields)
				{
					string fieldType = getTypeName(field.Type);
					string tempName = "sortTemp" + permutationTempIndex++;
					builder.AppendLine("\t\t\t" + fieldType + " " + tempName + " = storage." + fieldAccess(field) + "[index + start];");
				}
				string aosPermutationTempName = null;
				if (aosFields.Count > 0)
				{
					aosPermutationTempName = "sortTemp" + permutationTempIndex;
					builder.AppendLine("\t\t\t" + typeName + "AoSBlock " + aosPermutationTempName + " = storage.mAoS[index + start];");
				}
				builder.AppendLine("\t\t\tint current = start;");
				builder.AppendLine("\t\t\twhile (true)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine("\t\t\t\tint sourceAbsolute = permutation[current];");
				builder.AppendLine("\t\t\t\tint sourceOffset = sourceAbsolute - index;");
				builder.AppendLine("\t\t\t\tpermutation[current] = ~sourceAbsolute;");
				builder.AppendLine("\t\t\t\tif (sourceOffset == start)");
				builder.AppendLine("\t\t\t\t{");
				builder.AppendLine("\t\t\t\t\tbreak;");
				builder.AppendLine("\t\t\t\t}");
				builder.AppendLine("\t\t\t\tint destinationIndex = index + current;");
				builder.AppendLine("\t\t\t\tint sourceIndex = index + sourceOffset;");
				foreach (IFieldSymbol field in ecsFields)
				{
					builder.AppendLine("\t\t\t\tstorage." + fieldAccess(field) + "[destinationIndex] = storage." + fieldAccess(field) + "[sourceIndex];");
				}
				if (aosFields.Count > 0)
				{
					builder.AppendLine("\t\t\t\tstorage.mAoS[destinationIndex] = storage.mAoS[sourceIndex];");
				}
				builder.AppendLine("\t\t\t\tcurrent = sourceOffset;");
				builder.AppendLine("\t\t\t}");
				builder.AppendLine("\t\t\tint finalIndex = index + current;");
				permutationTempIndex = 0;
				foreach (IFieldSymbol field in ecsFields)
				{
					string tempName = "sortTemp" + permutationTempIndex++;
					builder.AppendLine("\t\t\tstorage." + fieldAccess(field) + "[finalIndex] = " + tempName + ";");
				}
				if (aosFields.Count > 0)
				{
					builder.AppendLine("\t\t\tstorage.mAoS[finalIndex] = " + aosPermutationTempName + ";");
				}
				builder.AppendLine("\t\t}");
				builder.AppendLine("\t}");
			}
			foreach (IFieldSymbol field in ecsFields)
			{
				appendAggressiveInlining(builder, 1);
				builder.AppendLine("\tprivate " + getTypeName(field.Type) + " getSortField_" + field.Name + "(int index)");
				builder.AppendLine("\t{");
				if (spanBackend)
				{
					builder.AppendLine("\t\treturn mStorage[0]." + fieldAccess(field) + "[index];");
				}
				else
				{
					builder.AppendLine("\t\t" + typeName + "Storage storage = " + typeName + "StorageRegistry.getStorage(mStorageID);");
					builder.AppendLine("\t\treturn storage." + fieldAccess(field) + "[index];");
				}
				builder.AppendLine("\t}");
			}
		}
		private static void generateUnsafeResize(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, bool hasBurstIntegration)
		{
			bool hasManagedAoS = aosFields.Any(field => !field.Type.IsUnmanagedType);
			builder.AppendLine("\tprivate void resize(int capacity)");
			builder.AppendLine("\t{");
			if (hasBurstIntegration)
			{
				builder.AppendLine("\t\tcompleteBurstJobs();");
			}
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tint[] newRefGeneration = new int[capacity];");
			builder.AppendLine("\t\tint generationCopyCount = mRefGeneration.Length < capacity ? mRefGeneration.Length : capacity;");
			builder.AppendLine("\t\tglobal::System.Array.Copy(mRefGeneration, newRefGeneration, generationCopyCount);");
			builder.AppendLine("#endif");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (!field.Type.IsUnmanagedType)
				{
					string fieldType = getTypeName(field.Type);
					string localName = "new_" + field.Name;
					builder.AppendLine("\t\t" + fieldType + "[] " + localName + " = new " + fieldType + "[capacity];");
					builder.AppendLine("\t\tglobal::System.Array.Copy(mManagedStorage." + fieldAccess(field) + ", " + localName + ", mCount);");
				}
			}
			if (hasManagedAoS)
			{
				builder.AppendLine("\t\t" + typeName + "AoSBlock[] newAoS = new " + typeName + "AoSBlock[capacity];");
				builder.AppendLine("\t\tglobal::System.Array.Copy(mManagedStorage.mAoS, newAoS, mCount);");
			}
			builder.AppendLine("\t\tglobal::System.IntPtr newRawMemory;");
			builder.AppendLine("\t\t" + typeName + "Storage newStorage;");
			builder.AppendLine("\t\tallocateColumns(capacity, out newRawMemory, out newStorage);");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\tcopyMemory(newStorage." + fieldAccess(field) + ", mStorage->" + fieldAccess(field) + ", (long)mCount * sizeof(" + getTypeName(field.Type) + "));");
				}
			}
			if (aosFields.Count > 0 && !hasManagedAoS)
			{
				builder.AppendLine("\t\tcopyMemory(newStorage.mAoS, mStorage->mAoS, (long)mCount * sizeof(" + typeName + "AoSBlock));");
			}
			builder.AppendLine("\t\tglobal::System.Runtime.InteropServices.Marshal.FreeHGlobal(mRawMemory);");
			builder.AppendLine("\t\tmRawMemory = newRawMemory;");
			builder.AppendLine("\t\t*mStorage = newStorage;");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (!field.Type.IsUnmanagedType)
				{
					builder.AppendLine("\t\tmManagedStorage." + fieldAccess(field) + " = new_" + field.Name + ";");
				}
			}
			if (hasManagedAoS)
			{
				builder.AppendLine("\t\tmManagedStorage.mAoS = newAoS;");
			}
			builder.AppendLine("\t\tmCapacity = capacity;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tmRefGeneration = newRefGeneration;");
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
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tint[] newRefGeneration = new int[capacity];");
			builder.AppendLine("\t\tint generationCopyCount = mRefGeneration.Length < capacity ? mRefGeneration.Length : capacity;");
			builder.AppendLine("\t\tglobal::System.Array.Copy(mRefGeneration, newRefGeneration, generationCopyCount);");
			builder.AppendLine("#endif");
			foreach (IFieldSymbol field in ecsFields)
			{
				string fieldType = getTypeName(field.Type);
				string localName = "new_" + field.Name;
				builder.AppendLine("\t\t" + fieldType + "[] " + localName + " = new " + fieldType + "[capacity];");
				builder.AppendLine("\t\tglobal::System.Array.Copy(storage." + fieldAccess(field) + ", " + localName + ", mCount);");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\t" + typeName + "AoSBlock[] newAoS = new " + typeName + "AoSBlock[capacity];");
				builder.AppendLine("\t\tglobal::System.Array.Copy(storage.mAoS, newAoS, mCount);");
			}
			foreach (IFieldSymbol field in ecsFields)
			{
				builder.AppendLine("\t\tstorage." + fieldAccess(field) + " = new_" + field.Name + ";");
			}
			if (aosFields.Count > 0)
			{
				builder.AppendLine("\t\tstorage.mAoS = newAoS;");
			}
			builder.AppendLine("\t\tmCapacity = capacity;");
			builder.AppendLine("#if UNITY_EDITOR");
			builder.AppendLine("\t\tmRefGeneration = newRefGeneration;");
			builder.AppendLine("#endif");
		}
		private static void generateUnsafeAllocateColumns(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields)
		{
			bool hasManagedAoS = aosFields.Any(field => !field.Type.IsUnmanagedType);
			builder.AppendLine("\tprivate static void allocateColumns(int capacity, out global::System.IntPtr rawMemory, out " + typeName + "Storage storage)");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tstorage = default(" + typeName + "Storage);");
			builder.AppendLine("\t\tlong totalBytes = ALIGNMENT;");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (field.Type.IsUnmanagedType)
				{
					string bytesName = getBytesVariableName(field.Name);
					builder.AppendLine("\t\tlong " + bytesName + " = alignUp((long)capacity * sizeof(" + getTypeName(field.Type) + "), ALIGNMENT);");
					builder.AppendLine("\t\ttotalBytes += " + bytesName + ";");
				}
			}
			if (aosFields.Count > 0 && !hasManagedAoS)
			{
				builder.AppendLine("\t\tlong aosBytes = alignUp((long)capacity * sizeof(" + typeName + "AoSBlock), ALIGNMENT);");
				builder.AppendLine("\t\ttotalBytes += aosBytes;");
			}
			builder.AppendLine("\t\trawMemory = global::System.Runtime.InteropServices.Marshal.AllocHGlobal(new global::System.IntPtr(totalBytes));");
			builder.AppendLine("\t\tbyte* current = alignPointer((byte*)rawMemory.ToPointer(), ALIGNMENT);");
			foreach (IFieldSymbol field in ecsFields)
			{
				if (field.Type.IsUnmanagedType)
				{
					string bytesName = getBytesVariableName(field.Name);
					builder.AppendLine("\t\tstorage." + fieldAccess(field) + " = (" + getTypeName(field.Type) + "*)current;");
					builder.AppendLine("\t\tcurrent += " + bytesName + ";");
				}
			}
			if (aosFields.Count > 0 && !hasManagedAoS)
			{
				builder.AppendLine("\t\tstorage.mAoS = (" + typeName + "AoSBlock*)current;");
			}
			builder.AppendLine("\t}");
		}
		private static void generateUnsafeDispose(StringBuilder builder, string typeName, List<IFieldSymbol> ecsFields, List<IFieldSymbol> aosFields, bool hasBurstIntegration)
		{
			bool hasManagedStorage = ecsFields.Any(field => !field.Type.IsUnmanagedType) || aosFields.Any(field => !field.Type.IsUnmanagedType);
			bool hasManagedAoS = aosFields.Any(field => !field.Type.IsUnmanagedType);
			builder.AppendLine("\tprivate void dispose()");
			builder.AppendLine("\t{");
			if (hasBurstIntegration)
			{
				builder.AppendLine("\t\tcompleteBurstJobs();");
			}
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
			if (hasManagedStorage)
			{
				builder.AppendLine("\t\tif (mManagedStorage != null)");
				builder.AppendLine("\t\t{");
				foreach (IFieldSymbol field in ecsFields)
				{
					if (!field.Type.IsUnmanagedType)
					{
						builder.AppendLine("\t\t\tif (mCount > 0)");
						builder.AppendLine("\t\t\t{");
						builder.AppendLine("\t\t\t\tglobal::System.Array.Clear(mManagedStorage." + fieldAccess(field) + ", 0, mCount);");
						builder.AppendLine("\t\t\t}");
					}
				}
				if (hasManagedAoS)
				{
					builder.AppendLine("\t\t\tif (mCount > 0)");
					builder.AppendLine("\t\t\t{");
					builder.AppendLine("\t\t\t\tglobal::System.Array.Clear(mManagedStorage.mAoS, 0, mCount);");
					builder.AppendLine("\t\t\t}");
				}
				builder.AppendLine("\t\t\tmManagedStorage = null;");
				builder.AppendLine("\t\t}");
			}
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
		private static void appendGeneratedFor(StringBuilder builder, string typeName, string fullTypeName, string containerName)
		{
			builder.AppendLine("/// <summary>");
			builder.AppendLine("/// <see cref=\"" + typeName + "\"/>的EasyECS " + containerName + ".");
			builder.AppendLine("/// </summary>");
			builder.AppendLine("[global::EasyECS.ECSGeneratedFor(typeof(" + fullTypeName + "))]");
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
		private static string getSortMethodSuffix(string fieldName)
		{
			string columnMethodName = getColumnMethodName(fieldName);
			return columnMethodName.Substring(3, columnMethodName.Length - 3 - "Column".Length);
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