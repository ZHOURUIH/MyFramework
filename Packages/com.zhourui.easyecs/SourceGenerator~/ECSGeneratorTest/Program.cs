using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ECSSourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ECSGeneratorTests
{
	internal static class Program
	{
		private const string ATTRIBUTE_SOURCE = @"
using System;
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Field)]
public sealed class ECSAttribute : Attribute
{
}
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Field)]
public sealed class NotECSAttribute : Attribute
{
}
";
		private static readonly MetadataReference[] mMetadataReferences = createMetadataReferences();
		private static int Main()
		{
			TestCase[] tests =
			{
				new TestCase("ECS默认SoA", testECSDefaultLayout),
				new TestCase("NotECS默认AoS", testNotECSDefaultLayout),
				new TestCase("ECS中NotECS字段覆盖", testECSFieldOverride),
				new TestCase("NotECS中ECS字段覆盖", testNotECSFieldOverride),
				new TestCase("Unsafe后端选择", testUnsafeBackend),
				new TestCase("SafeSpan后端选择", testSafeSpanBackend),
				new TestCase("SafeRegistry后端选择", testSafeRegistryBackend),
				new TestCase("Managed字段自动SafeSpan", testManagedFieldFallback),
				new TestCase("正常字段不生成@", testNormalIdentifierDoesNotEscape),
				new TestCase("关键字字段正确生成@", testKeywordIdentifierEscape),
				new TestCase("生成代码排版", testGeneratedCodeFormatting),
				new TestCase("Struct标签冲突ECS001", testStructAttributeConflict),
				new TestCase("Field标签冲突ECS001", testFieldAttributeConflict),
				new TestCase("嵌套Struct报ECS002", testNestedStructDiagnostic),
				new TestCase("泛型Struct报ECS002", testGenericStructDiagnostic),
				new TestCase("RefStruct报ECS002", testRefStructDiagnostic),
				new TestCase("实例Property报ECS002", testPropertyDiagnostic),
				new TestCase("Readonly字段报ECS003", testReadonlyFieldDiagnostic),
				new TestCase("Fixed字段报ECS003", testFixedFieldDiagnostic),
				new TestCase("Private字段报ECS003", testPrivateFieldDiagnostic),
				new TestCase("Column名称冲突报ECS004", testColumnNameConflictDiagnostic),
			};
			int failedCount = 0;
			Console.WriteLine("================ ECSGenerator Test Start ================");
			foreach (TestCase test in tests)
			{
				try
				{
					test.mAction();
					Console.WriteLine("[PASS] " + test.mName);
				}
				catch (Exception exception)
				{
					++failedCount;
					Console.WriteLine("[FAIL] " + test.mName);
					Console.WriteLine(exception.Message);
				}
			}
			Console.WriteLine("---------------------------------------------------------");
			Console.WriteLine("Total:" + tests.Length + ",Pass:" + (tests.Length - failedCount) + ",Fail:" + failedCount);
			Console.WriteLine(failedCount == 0 ? "================ ECSGenerator Test Pass =================" : "================ ECSGenerator Test Failed ===============");
			Console.ReadKey();
			return failedCount == 0 ? 0 : 1;
		}
		private static void testECSDefaultLayout()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
	public float mPositionX;
}
", false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public int[] mHP;");
			assertContains(result.mGeneratedSource, "public float[] mSpeed;");
			assertContains(result.mGeneratedSource, "public float[] mPositionX;");
			assertDoesNotContain(result.mGeneratedSource, "RoleDataAoSBlock");
			assertContains(result.mGeneratedSource, "getHPColumn()");
			assertContains(result.mGeneratedSource, "getSpeedColumn()");
			assertContains(result.mGeneratedSource, "getPositionXColumn()");
		}
		private static void testNotECSDefaultLayout()
		{
			GeneratorTestResult result = runGenerator(@"
[NotECS]
public struct RoleData
{
	public int mID;
	public int mModelID;
	public int mCamp;
}
", false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "internal struct RoleDataAoSBlock");
			assertContains(result.mGeneratedSource, "public int mID;");
			assertContains(result.mGeneratedSource, "public int mModelID;");
			assertContains(result.mGeneratedSource, "public int mCamp;");
			assertContains(result.mGeneratedSource, "public RoleDataAoSBlock[] mAoS;");
			assertDoesNotContain(result.mGeneratedSource, "getIDColumn()");
			assertDoesNotContain(result.mGeneratedSource, "getModelIDColumn()");
			assertDoesNotContain(result.mGeneratedSource, "getCampColumn()");
		}
		private static void testECSFieldOverride()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
	[NotECS] public int mID;
	[NotECS] public int mCamp;
}
", false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public int[] mHP;");
			assertContains(result.mGeneratedSource, "public float[] mSpeed;");
			assertContains(result.mGeneratedSource, "internal struct RoleDataAoSBlock");
			assertContains(result.mGeneratedSource, "public int mID;");
			assertContains(result.mGeneratedSource, "public int mCamp;");
			assertContains(result.mGeneratedSource, "getHPColumn()");
			assertContains(result.mGeneratedSource, "getSpeedColumn()");
			assertDoesNotContain(result.mGeneratedSource, "getIDColumn()");
			assertDoesNotContain(result.mGeneratedSource, "getCampColumn()");
		}
		private static void testNotECSFieldOverride()
		{
			GeneratorTestResult result = runGenerator(@"
[NotECS]
public struct RoleData
{
	[ECS] public int mHP;
	[ECS] public float mSpeed;
	public int mID;
	public int mCamp;
}
", false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public int[] mHP;");
			assertContains(result.mGeneratedSource, "public float[] mSpeed;");
			assertContains(result.mGeneratedSource, "internal struct RoleDataAoSBlock");
			assertContains(result.mGeneratedSource, "public int mID;");
			assertContains(result.mGeneratedSource, "public int mCamp;");
			assertContains(result.mGeneratedSource, "getHPColumn()");
			assertContains(result.mGeneratedSource, "getSpeedColumn()");
			assertDoesNotContain(result.mGeneratedSource, "getIDColumn()");
			assertDoesNotContain(result.mGeneratedSource, "getCampColumn()");
		}
		private static void testUnsafeBackend()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
	[NotECS] public int mID;
}
", true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const bool IsUnsafeBackend = true;");
			assertContains(result.mGeneratedSource, "public const string BackendName = \"Unsafe\";");
			assertContains(result.mGeneratedSource, "public const string BackendReason = \"AllowUnsafe=true,Unmanaged=true\";");
			assertContains(result.mGeneratedSource, "RoleDataStorage* mStorage;");
		}
		private static void testSafeSpanBackend()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
}
", false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const bool IsUnsafeBackend = false;");
			assertContains(result.mGeneratedSource, "public const string BackendName = \"SafeSpan\";");
			assertContains(result.mGeneratedSource, "public const string BackendReason = \"AllowUnsafe=false,Span=true\";");
			assertContains(result.mGeneratedSource, "global::System.Span<RoleDataStorage>");
		}
		private static void testSafeRegistryBackend()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
}
", false, "ECS_FORCE_SAFE_REGISTRY");
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"SafeRegistry\";");
			assertContains(result.mGeneratedSource, "public const string BackendReason = \"ECS_FORCE_SAFE_REGISTRY\";");
			assertContains(result.mGeneratedSource, "RoleDataStorageRegistry");
			assertContains(result.mGeneratedSource, "mStorageID");
			assertDoesNotContain(result.mGeneratedSource, "global::System.Span<RoleDataStorage>");
		}
		private static void testManagedFieldFallback()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public string mName;
}
", true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"SafeSpan\";");
			assertContains(result.mGeneratedSource, "public const string BackendReason = \"ContainsManagedField,Span=true\";");
			assertContains(result.mGeneratedSource, "public string[] mName;");
			assertDoesNotContain(result.mGeneratedSource, "RoleDataStorage* mStorage;");
		}
		private static void testNormalIdentifierDoesNotEscape()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
	public int mID;
}
", false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public int[] mHP;");
			assertContains(result.mGeneratedSource, "public float[] mSpeed;");
			assertContains(result.mGeneratedSource, "public int[] mID;");
			assertDoesNotContain(result.mGeneratedSource, "@mHP");
			assertDoesNotContain(result.mGeneratedSource, "@mSpeed");
			assertDoesNotContain(result.mGeneratedSource, "@mID");
		}
		private static void testKeywordIdentifierEscape()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int @class;
	public int mHP;
}
", false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public int[] @class;");
			assertContains(result.mGeneratedSource, "public int[] mHP;");
			assertDoesNotContain(result.mGeneratedSource, "@mHP");
		}
		private static void testGeneratedCodeFormatting()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
	public float mPositionX;
	public float mPositionY;
	[NotECS] public int mID;
	[NotECS] public int mModelID;
	[NotECS] public int mCamp;
}
", false);
			assertNoErrors(result);
			string source = normalizeLineEnding(result.mGeneratedSource);
			assertDoesNotContain(source, "if (capacity < 1) capacity = 1;");
			assertDoesNotContain(source, "if (mCount >= mCapacity) resize(mCapacity * 2);");
			assertDoesNotContain(source, "if (mCount == 0) return;");
			assertDoesNotContain(source, "if (mDisposed) return;");
			string invalidIf = findSingleLineIf(source);
			if (invalidIf != null)
			{
				throw new Exception("生成代码中存在未使用{}的单行if:\n" + invalidIf);
			}
			if (source.Contains("\n\n"))
			{
				throw new Exception("生成代码中存在空白行,生成代码应保持紧凑连续排版");
			}
			assertContains(source, "if (capacity < 1)\n\t\t{\n\t\t\tcapacity = 1;\n\t\t}");
			assertContains(source, "if (mCount >= mCapacity)\n\t\t{\n\t\t\tresize(mCapacity * 2);\n\t\t}");
			assertContains(source, "if ((uint)index >= (uint)mCount)\n\t\t{\n\t\t\tthrow new global::System.ArgumentOutOfRangeException(nameof(index));\n\t\t}");
		}
		private static void testStructAttributeConflict()
		{
			assertGeneratorDiagnostic(@"
[ECS]
[NotECS]
public struct RoleData
{
	public int mHP;
}
", false, "ECS001");
		}
		private static void testFieldAttributeConflict()
		{
			assertGeneratorDiagnostic(@"
[ECS]
public struct RoleData
{
	[ECS]
	[NotECS]
	public int mHP;
}
", false, "ECS001");
		}
		private static void testNestedStructDiagnostic()
		{
			assertGeneratorDiagnostic(@"
public class RoleContainer
{
	[ECS]
	public struct RoleData
	{
		public int mHP;
	}
}
", false, "ECS002");
		}
		private static void testGenericStructDiagnostic()
		{
			assertGeneratorDiagnostic(@"
[ECS]
public struct RoleData<T>
{
	public T mValue;
}
", false, "ECS002");
		}
		private static void testRefStructDiagnostic()
		{
			assertGeneratorDiagnostic(@"
[ECS]
public ref struct RoleData
{
	public int mHP;
}
", false, "ECS002");
		}
		private static void testPropertyDiagnostic()
		{
			assertGeneratorDiagnostic(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public int HP
	{
		get
		{
			return mHP;
		}
		set
		{
			mHP = value;
		}
	}
}
", false, "ECS002");
		}
		private static void testReadonlyFieldDiagnostic()
		{
			assertGeneratorDiagnostic(@"
[ECS]
public struct RoleData
{
	public readonly int mHP;
}
", false, "ECS003");
		}
		private static void testFixedFieldDiagnostic()
		{
			assertGeneratorDiagnostic(@"
[ECS]
public unsafe struct RoleData
{
	public fixed int mValues[4];
}
", true, "ECS003");
		}
		private static void testPrivateFieldDiagnostic()
		{
			assertGeneratorDiagnostic(@"
[ECS]
public struct RoleData
{
	private int mHP;
}
", false, "ECS003");
		}
		private static void testColumnNameConflictDiagnostic()
		{
			assertGeneratorDiagnostic(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public int HP;
}
", false, "ECS004");
		}
		private static GeneratorTestResult runGenerator(string source, bool allowUnsafe, params string[] preprocessorSymbols)
		{
			CSharpParseOptions parseOptions = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Parse, SourceCodeKind.Regular, preprocessorSymbols ?? Array.Empty<string>());
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(ATTRIBUTE_SOURCE + "\n" + source, parseOptions);
			CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, allowUnsafe: allowUnsafe);
			CSharpCompilation compilation = CSharpCompilation.Create("ECSGeneratorTest_" + Guid.NewGuid().ToString("N"), new[] { syntaxTree }, mMetadataReferences, compilationOptions);
			ISourceGenerator generator = new ECSGenerator();
			GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { generator }, parseOptions: parseOptions);
			driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out ImmutableArray<Diagnostic> driverDiagnostics);
			GeneratorDriverRunResult runResult = driver.GetRunResult();
			List<Diagnostic> generatorDiagnostics = new List<Diagnostic>();
			generatorDiagnostics.AddRange(driverDiagnostics);
			foreach (GeneratorRunResult generatorResult in runResult.Results)
			{
				generatorDiagnostics.AddRange(generatorResult.Diagnostics);
			}
			StringBuilder generatedSource = new StringBuilder();
			foreach (GeneratorRunResult generatorResult in runResult.Results)
			{
				foreach (GeneratedSourceResult generatedSourceResult in generatorResult.GeneratedSources)
				{
					if (generatedSource.Length > 0)
					{
						generatedSource.AppendLine();
					}
					generatedSource.Append(generatedSourceResult.SourceText.ToString());
				}
			}
			return new GeneratorTestResult
			{
				mGeneratorDiagnostics = generatorDiagnostics.ToImmutableArray(),
				mCompilationDiagnostics = outputCompilation.GetDiagnostics(),
				mGeneratedSource = generatedSource.ToString(),
			};
		}
		private static void assertGeneratorDiagnostic(string source, bool allowUnsafe, string expectedDiagnosticID)
		{
			GeneratorTestResult result = runGenerator(source, allowUnsafe);
			Diagnostic expectedDiagnostic = result.mGeneratorDiagnostics.FirstOrDefault(item => item.Id == expectedDiagnosticID);
			if (expectedDiagnostic == null)
			{
				throw new Exception("没有找到预期Diagnostic:" + expectedDiagnosticID + "\nGenerator Diagnostics:\n" + diagnosticsToString(result.mGeneratorDiagnostics) + "\nCompilation Diagnostics:\n" + diagnosticsToString(result.mCompilationDiagnostics));
			}
			if (expectedDiagnostic.Severity != DiagnosticSeverity.Error)
			{
				throw new Exception("Diagnostic:" + expectedDiagnosticID + "不是Error,Actual:" + expectedDiagnostic.Severity);
			}
			foreach (Diagnostic diagnostic in result.mGeneratorDiagnostics)
			{
				if (diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Id != expectedDiagnosticID)
				{
					throw new Exception("出现非预期Generator Error:" + diagnostic);
				}
			}
			assertNoCompilationErrors(result);
		}
		private static void assertNoErrors(GeneratorTestResult result)
		{
			foreach (Diagnostic diagnostic in result.mGeneratorDiagnostics)
			{
				if (diagnostic.Severity == DiagnosticSeverity.Error)
				{
					throw new Exception("Generator出现错误:\n" + diagnostic);
				}
			}
			assertNoCompilationErrors(result);
			if (string.IsNullOrWhiteSpace(result.mGeneratedSource))
			{
				throw new Exception("Generator没有生成任何代码");
			}
		}
		private static void assertNoCompilationErrors(GeneratorTestResult result)
		{
			Diagnostic[] errors = result.mCompilationDiagnostics.Where(item => item.Severity == DiagnosticSeverity.Error).ToArray();
			if (errors.Length > 0)
			{
				throw new Exception("生成后的代码存在编译错误:\n" + diagnosticsToString(errors));
			}
		}
		private static void assertContains(string source, string expected)
		{
			if (!source.Contains(expected))
			{
				throw new Exception("生成代码中没有找到:\n" + expected + "\n\nGenerated Source:\n" + source);
			}
		}
		private static void assertDoesNotContain(string source, string unexpected)
		{
			if (source.Contains(unexpected))
			{
				throw new Exception("生成代码中不应该出现:\n" + unexpected + "\n\nGenerated Source:\n" + source);
			}
		}
		private static string normalizeLineEnding(string source)
		{
			return source.Replace("\r\n", "\n").Replace('\r', '\n');
		}
		private static string findSingleLineIf(string source)
		{
			string[] lines = normalizeLineEnding(source).Split('\n');
			for (int i = 0; i < lines.Length; ++i)
			{
				string trimLine = lines[i].Trim();
				if (!trimLine.StartsWith("if (", StringComparison.Ordinal))
				{
					continue;
				}
				int closeIndex = trimLine.LastIndexOf(')');
				if (closeIndex < 0)
				{
					return "Line " + (i + 1) + ":" + trimLine;
				}
				string remain = trimLine.Substring(closeIndex + 1).Trim();
				if (remain.Length > 0)
				{
					return "Line " + (i + 1) + ":" + trimLine;
				}
				if (i + 1 >= lines.Length || lines[i + 1].Trim() != "{")
				{
					return "Line " + (i + 1) + ":" + trimLine;
				}
			}
			return null;
		}
		private static string diagnosticsToString(IEnumerable<Diagnostic> diagnostics)
		{
			StringBuilder builder = new StringBuilder();
			foreach (Diagnostic diagnostic in diagnostics)
			{
				builder.AppendLine(diagnostic.ToString());
			}
			return builder.Length == 0 ? "<none>" : builder.ToString();
		}
		private static MetadataReference[] createMetadataReferences()
		{
			string trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
			if (string.IsNullOrEmpty(trustedPlatformAssemblies))
			{
				throw new InvalidOperationException("无法获取TRUSTED_PLATFORM_ASSEMBLIES");
			}
			string[] assemblyPaths = trustedPlatformAssemblies.Split(Path.PathSeparator);
			MetadataReference[] references = new MetadataReference[assemblyPaths.Length];
			for (int i = 0; i < assemblyPaths.Length; ++i)
			{
				references[i] = MetadataReference.CreateFromFile(assemblyPaths[i]);
			}
			return references;
		}
		private readonly struct TestCase
		{
			public readonly string mName;
			public readonly Action mAction;
			public TestCase(string name, Action action)
			{
				mName = name;
				mAction = action;
			}
		}
		private sealed class GeneratorTestResult
		{
			public ImmutableArray<Diagnostic> mGeneratorDiagnostics;
			public ImmutableArray<Diagnostic> mCompilationDiagnostics;
			public string mGeneratedSource;
		}
	}
}