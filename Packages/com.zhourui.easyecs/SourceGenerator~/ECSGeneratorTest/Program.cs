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
	public static class Program
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
namespace EasyECS
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class ECSGeneratedForAttribute : Attribute
	{
		public Type SourceType { get; }
		public ECSGeneratedForAttribute(Type sourceType)
		{
			SourceType = sourceType;
		}
	}
}
";

		private const string BURST_STUB_SOURCE = @"
namespace Unity.Burst
{
	[global::System.AttributeUsage(global::System.AttributeTargets.Struct | global::System.AttributeTargets.Class | global::System.AttributeTargets.Method)]
	public sealed class BurstCompileAttribute : global::System.Attribute
	{
	}
}
namespace Unity.Collections.LowLevel.Unsafe
{
	[global::System.AttributeUsage(global::System.AttributeTargets.Field)]
	public sealed class NativeDisableUnsafePtrRestrictionAttribute : global::System.Attribute
	{
	}
}
namespace Unity.Jobs
{
	public struct JobHandle
	{
		public void Complete()
		{
		}
		public static JobHandle CombineDependencies(JobHandle left, JobHandle right)
		{
			return default(JobHandle);
		}
	}
	public interface IJobParallelFor
	{
		void Execute(int index);
	}
	public static class IJobParallelForExtensions
	{
		public static JobHandle Schedule<TJob>(TJob jobData, int arrayLength, int innerloopBatchCount, JobHandle dependsOn) where TJob : struct, IJobParallelFor
		{
			return default(JobHandle);
		}
	}
}
";
		private const string DICTIONARY_DATA_SOURCE = @"
[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
	public float mPositionX;
	public float mPositionY;
	[NotECS] public int mID;
	[NotECS] public int mCamp;
}
";
		private const string LIST_EXTENDED_USAGE_SOURCE = @"
[ECS]
public struct RangeData
{
	public int mValue;
	[NotECS] public int mID;
}
public static class ListExtendedUsage
{
	public static int Run()
	{
		RangeDataECSList list = new RangeDataECSList(1);
		RangeData[] source = new RangeData[]
		{
			new RangeData { mValue = 3, mID = 3 },
			new RangeData { mValue = 1, mID = 1 },
			new RangeData { mValue = 2, mID = 2 },
		};
		list.AddRange(source);
		list.InsertRange(1, source, 1, 2);
		RangeDataECSList other = new RangeDataECSList();
		other.AddRange(source);
		list.AddRange(other);
		list.InsertRange(0, other);
		list.RemoveRange(0, 1);
		list.RemoveAll(value => value.mValue < 0);
		list.Contains(source[0]);
		list.IndexOf(source[0]);
		list.LastIndexOf(source[0]);
		list.Remove(source[0]);
		list.Reverse();
		list.Reverse(0, list.Count);
		list.Sort((left, right) => left.mValue.CompareTo(right.mValue));
		global::System.Collections.Generic.IComparer<RangeData> comparer = global::System.Collections.Generic.Comparer<RangeData>.Create((left, right) => left.mValue.CompareTo(right.mValue));
		list.Sort(comparer);
		list.Sort(0, list.Count, comparer);
		list.SortByValue();
		global::System.Collections.Generic.IComparer<int> valueComparer = global::System.Collections.Generic.Comparer<int>.Default;
		list.SortByValue(valueComparer);
		list.SortByValue(0, list.Count, valueComparer);
		int search = list.BinarySearch(source[1], comparer);
		list.BinarySearch(0, list.Count, source[1], comparer);
		search += list.BinarySearchByValue(source[1].mValue);
		search += list.BinarySearchByValue(source[1].mValue, valueComparer);
		search += list.BinarySearchByValue(0, list.Count, source[1].mValue, valueComparer);
		list.ContainsByValue(source[1].mValue);
		search += list.IndexOfByValue(source[1].mValue);
		search += list.LastIndexOfByValue(source[1].mValue);
		list.ExistsByValue(value => value == 1);
		search += list.FindIndexByValue(value => value == 1);
		search += list.FindIndexByValue(0, value => value == 1);
		search += list.FindIndexByValue(0, list.Count, value => value == 1);
		search += list.RemoveAllByValue(value => value < 0);
		list.Exists(value => value.mValue == 1);
		list.Find(value => value.mValue == 1);
		list.FindIndex(value => value.mValue == 1);
		list.FindLast(value => value.mValue == 1);
		list.FindLastIndex(value => value.mValue == 1);
		list.TrueForAll(value => value.mValue >= 0);
		RangeData[] copy = list.ToArray();
		list.CopyTo(copy);
		list.CopyTo(copy, 0);
		list.CopyTo(0, copy, 0, list.Count);
		list.EnsureCapacity(64);
		list.TrimExcess();
		int result = list.Count + search;
		other.Dispose();
		list.Dispose();
		return result;
	}
}
";
		private const string DICTIONARY_USAGE_SOURCE = @"
public static class DictionaryUsage
{
	public static int Run()
	{
		RoleDataECSDictionary<int> dict = new RoleDataECSDictionary<int>(4);
		dict.Add(1, new RoleData { mHP = 10, mSpeed = 1.0f, mPositionX = 2.0f, mPositionY = 3.0f, mID = 1, mCamp = 2 });
		dict.TryAdd(2, new RoleData { mHP = 20, mSpeed = 2.0f, mPositionX = 4.0f, mPositionY = 6.0f, mID = 2, mCamp = 3 });
		int sum = 0;
		RoleDataRef indexValue = dict[1];
		indexValue.mHP += 1;
		if (dict.TryGetValue(1, out RoleDataRef found))
		{
			found.mHP += 1;
		}
		if (dict.TryGetIndex(1, out int index))
		{
			sum += index;
		}
		int directIndex = dict.GetIndex(1);
		sum += directIndex;
		foreach (var item in dict)
		{
			int key = item.Key;
			RoleDataRef value = item.Value;
			value.mHP += key;
			sum += value.mHP;
		}
		foreach (int key in dict.Keys)
		{
			sum += key;
		}
		foreach (RoleDataRef value in dict.Values)
		{
			value.mHP += 1;
			sum += value.mHP;
		}
		dict.SetValue(1, new RoleData { mHP = 30, mSpeed = 3.0f, mPositionX = 6.0f, mPositionY = 9.0f, mID = 1, mCamp = 4 });
		dict.TrySetValue(1, new RoleData { mHP = 31, mSpeed = 3.1f, mPositionX = 6.2f, mPositionY = 9.3f, mID = 1, mCamp = 4 });
		dict.SetValueByHP(1, 32);
		dict.TrySetValueByHP(1, 33);
		sum += dict.GetValueByHP(1);
		if (dict.TryGetValueByHP(1, out int hpValue))
		{
			sum += hpValue;
		}
		RoleDataRef setOrAdd = dict.SetOrAdd(3, new RoleData { mHP = 40, mID = 3 });
		RoleDataRef getOrAdd = dict.GetOrAdd(4, new RoleData { mHP = 50, mID = 4 });
		RoleDataRef getOrAddDefault = dict.GetOrAdd(5);
		int existingIndex = dict.GetOrAddIndex(4, new RoleData { mHP = 500, mID = 4 });
		int addedIndex = dict.GetOrAddIndex(6, new RoleData { mHP = 60, mID = 6 }, out bool addedIndexValue);
		sum += existingIndex + addedIndex + (addedIndexValue ? 1 : 0);
		if (dict.ContainsValue(new RoleData { mHP = 50, mID = 4 }))
		{
			sum += getOrAdd.mHP;
		}
		if (dict.Remove(5, out RoleData removed))
		{
			sum += removed.mHP;
		}
		dict.EnsureCapacity(32);
		dict.TrimExcess();
		RoleDataRef denseValue = dict.getValueAt(0);
		denseValue.mHP += dict.getKeyAt(0);
		var hp = dict.getHPColumn();
		hp[0] += 1;
		dict.Remove(2);
		dict.Clear();
		dict.Dispose();
		return sum;
	}
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
				new TestCase("Managed字段Hybrid Unsafe", testManagedFieldHybridUnsafe),
				new TestCase("全Managed字段自动SafeSpan", testManagedOnlyFieldFallback),
				new TestCase("Managed AoS使用托管存储", testManagedAoSHybridStorage),
				new TestCase("Managed ECS + Native AoS Hybrid", testManagedECSNativeAoSHybridStorage),
				new TestCase("Managed字段强制SafeRegistry", testManagedFieldForceSafeRegistry),
				new TestCase("ECSList Insert/RemoveAt API生成", testListInsertRemoveAtGeneration),
				new TestCase("ECSList Unsafe Insert/RemoveAt编译", testListInsertRemoveAtUnsafeCompile),
				new TestCase("ECSList Unsafe Insert/RemoveAt性能策略生成", testListUnsafeInsertRemoveAtPerformanceStrategy),
				new TestCase("ECSList Hybrid Unsafe结构移动分流", testListHybridUnsafeStructuralMoveSplit),
				new TestCase("ECSList SafeSpan Insert/RemoveAt编译", testListInsertRemoveAtSafeSpanCompile),
				new TestCase("ECSList SafeRegistry Insert/RemoveAt编译", testListInsertRemoveAtSafeRegistryCompile),
				new TestCase("ECSList Hybrid Unsafe Insert/RemoveAt编译", testListInsertRemoveAtHybridUnsafeCompile),
				new TestCase("ECSList Insert/RemoveAt Editor区间失效生成", testListInsertRemoveAtEditorInvalidationGeneration),
				new TestCase("ECSList扩展API生成", testListExtendedApiGeneration),
				new TestCase("ECSList SortBy自适应策略生成", testListSortByAdaptiveStrategyGeneration),
				new TestCase("ECSList数组转换自适应策略生成", testListArrayConversionStrategyGeneration),
				new TestCase("ECSList扩展API Unsafe编译", testListExtendedApiUnsafeCompile),
				new TestCase("ECSList扩展API SafeSpan编译", testListExtendedApiSafeSpanCompile),
				new TestCase("ECSList扩展API SafeRegistry编译", testListExtendedApiSafeRegistryCompile),
				new TestCase("正常字段不生成@", testNormalIdentifierDoesNotEscape),
				new TestCase("关键字字段正确生成@", testKeywordIdentifierEscape),
				new TestCase("生成代码排版", testGeneratedCodeFormatting),
				new TestCase("生成容器可导航到原始类型", testGeneratedContainerSourceNavigation),
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
				new TestCase("Burst不存在不生成Burst接口", testBurstUnavailableDoesNotGenerate),
				new TestCase("Burst存在Unsafe生成BurstView", testBurstUnsafeGeneration),
				new TestCase("Burst Managed Hybrid过滤Managed字段", testBurstHybridFieldFiltering),
				new TestCase("Burst Safe后端不生成Burst接口", testBurstSafeBackendDoesNotGenerate),
				new TestCase("Burst接口编译", testBurstUsageCompile),
				new TestCase("ECSDictionary生成", testDictionaryGeneration),
				new TestCase("ECSDictionary TryGetIndex生成", testDictionaryTryGetIndexGeneration),
				new TestCase("ECSDictionary DenseIndex/GetOrAddIndex生成", testDictionaryDenseIndexGeneration),
				new TestCase("ECSDictionary扩展API生成", testDictionaryExtendedApiGeneration),
				new TestCase("ECSDictionary Unsafe编译", testDictionaryUnsafeCompile),
				new TestCase("ECSDictionary Hybrid Unsafe编译", testDictionaryHybridUnsafeCompile),
				new TestCase("ECSDictionary SafeSpan编译", testDictionarySafeSpanCompile),
				new TestCase("ECSDictionary SafeRegistry编译", testDictionarySafeRegistryCompile),
				new TestCase("ECSDictionary Unsafe foreach快路径", testDictionaryUnsafeForeachFastPath),
				new TestCase("ECSDictionary SafeSpan foreach快路径", testDictionarySafeSpanForeachFastPath),
				new TestCase("ECSDictionary SafeRegistry foreach快路径", testDictionarySafeRegistryForeachFastPath),
				new TestCase("ECSDictionary Player Entry延迟读取Key", testDictionaryPlayerEntryLazyKey),
				new TestCase("ECSDictionary Keys和Values生成", testDictionaryKeysValuesGeneration),
				new TestCase("ECSDictionary Keys ReadOnlySpan Player快路径", testDictionaryKeysPlayerFastPath),
				new TestCase("ECSDictionary Keys Player不暴露可写Span", testDictionaryKeysReadOnlySpanSafety),
				new TestCase("ECSDictionary Values Unsafe快路径", testDictionaryValuesUnsafeFastPath),
				new TestCase("ECSDictionary Values Hybrid Unsafe快路径", testDictionaryValuesHybridUnsafeFastPath),
				new TestCase("ECSDictionary Values SafeSpan快路径", testDictionaryValuesSafeSpanFastPath),
				new TestCase("ECSDictionary Values SafeRegistry快路径", testDictionaryValuesSafeRegistryFastPath),
				new TestCase("ECSDictionary不实现集合接口", testDictionaryDoesNotImplementCollectionInterfaces),
				new TestCase("ECSDictionary Editor版本保护生成", testDictionaryEditorVersionValidationGeneration),
				new TestCase("ECSDictionary结构修改版本递增", testDictionaryStructuralVersionIncrementGeneration),
				new TestCase("SafeResize先分配后提交", testSafeResizeCommitAfterAllocation),
				new TestCase("Unsafe构造异常清理生成", testUnsafeConstructorCleanupGeneration),
				new TestCase("SafeRegistry构造异常清理生成", testSafeRegistryConstructorCleanupGeneration),
				new TestCase("Dictionary构造资源顺序", testDictionaryConstructorResourceOrder),
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
", true, "ECS_FORCE_SAFE_REGISTRY");
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const bool IsUnsafeBackend = false;");
			assertContains(result.mGeneratedSource, "public const string BackendName = \"SafeRegistry\";");
			assertContains(result.mGeneratedSource, "public const string BackendReason = \"ECS_FORCE_SAFE_REGISTRY\";");
			assertContains(result.mGeneratedSource, "RoleDataStorageRegistry");
			assertContains(result.mGeneratedSource, "mStorageID");
			assertDoesNotContain(result.mGeneratedSource, "global::System.Span<RoleDataStorage>");
		}
		private static void testManagedFieldHybridUnsafe()
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
			assertContains(result.mGeneratedSource, "public const string BackendName = \"Unsafe\";");
			assertContains(result.mGeneratedSource, "public const string BackendReason = \"AllowUnsafe=true,HybridStorage=true\";");
			assertContains(result.mGeneratedSource, "public int* mHP;");
			assertContains(result.mGeneratedSource, "internal sealed class RoleDataManagedStorage");
			assertContains(result.mGeneratedSource, "public string[] mName;");
			assertContains(result.mGeneratedSource, "return ref mManagedStorage.mName[mIndex];");
			assertDoesNotContain(result.mGeneratedSource, "string* mName;");
		}
		private static void testManagedOnlyFieldFallback()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public string mName;
	public object mPayload;
}
", true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"SafeSpan\";");
			assertContains(result.mGeneratedSource, "public const string BackendReason = \"NoNativeStorage,Span=true\";");
			assertContains(result.mGeneratedSource, "public string[] mName;");
			assertContains(result.mGeneratedSource, "public object[] mPayload;");
			assertDoesNotContain(result.mGeneratedSource, "RoleDataStorage* mStorage;");
		}
		private static void testManagedAoSHybridStorage()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public string mName;
	[NotECS] public int mID;
	[NotECS] public string mModelPath;
}
", true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"Unsafe\";");
			assertContains(result.mGeneratedSource, "internal struct RoleDataAoSBlock");
			assertContains(result.mGeneratedSource, "public int mID;");
			assertContains(result.mGeneratedSource, "public string mModelPath;");
			assertContains(result.mGeneratedSource, "public RoleDataAoSBlock[] mAoS;");
			assertContains(result.mGeneratedSource, "return ref mManagedStorage.mAoS[mIndex].mModelPath;");
			assertDoesNotContain(result.mGeneratedSource, "RoleDataAoSBlock* mAoS;");
		}
		private static void testManagedECSNativeAoSHybridStorage()
		{
			GeneratorTestResult result = runGenerator(@"
[NotECS]
public struct RoleData
{
	public int mID;
	public int mCamp;
	[ECS] public string mName;
}
", true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"Unsafe\";");
			assertContains(result.mGeneratedSource, "public const string BackendReason = \"AllowUnsafe=true,HybridStorage=true\";");
			assertContains(result.mGeneratedSource, "public RoleDataAoSBlock* mAoS;");
			assertContains(result.mGeneratedSource, "public string[] mName;");
			assertContains(result.mGeneratedSource, "return ref mStorage->mAoS[mIndex].mID;");
			assertContains(result.mGeneratedSource, "return ref mManagedStorage.mName[mIndex];");
		}
		private static void testManagedFieldForceSafeRegistry()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public string mName;
}
", true, "ECS_FORCE_SAFE_REGISTRY");
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"SafeRegistry\";");
			assertContains(result.mGeneratedSource, "public const string BackendReason = \"ECS_FORCE_SAFE_REGISTRY\";");
			assertContains(result.mGeneratedSource, "public string[] mName;");
			assertDoesNotContain(result.mGeneratedSource, "global::System.Span<RoleDataStorage>");
		}
		private static void testListInsertRemoveAtGeneration()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
	[NotECS] public int mID;
}
", false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public void Insert(int index, global::RoleData value)");
			assertContains(result.mGeneratedSource, "public void RemoveAt(int index)");
			assertContains(result.mGeneratedSource, "if ((uint)index > (uint)mCount)");
			assertContains(result.mGeneratedSource, "if ((uint)index >= (uint)mCount)");
			assertContains(result.mGeneratedSource, "global::System.Array.Copy(storage.mHP, index, storage.mHP, index + 1, mCount - index);");
			assertContains(result.mGeneratedSource, "global::System.Array.Copy(storage.mHP, index + 1, storage.mHP, index, moveCount);");
			assertContains(result.mGeneratedSource, "global::System.Array.Copy(storage.mAoS, index, storage.mAoS, index + 1, mCount - index);");
			assertContains(result.mGeneratedSource, "global::System.Array.Copy(storage.mAoS, index + 1, storage.mAoS, index, moveCount);");
		}
		private static void testListInsertRemoveAtUnsafeCompile()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
	[NotECS] public int mID;
}
public static class Usage
{
	public static int Run()
	{
		RoleDataECSList list = new RoleDataECSList(1);
		list.Add(new RoleData { mHP = 1, mSpeed = 1.0f, mID = 1 });
		list.Insert(0, new RoleData { mHP = 2, mSpeed = 2.0f, mID = 2 });
		list.Insert(list.Count, new RoleData { mHP = 3, mSpeed = 3.0f, mID = 3 });
		list.RemoveAt(1);
		int value = list[0].mHP + list[1].mID;
		list.Dispose();
		return value;
	}
}
", true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"Unsafe\";");
			assertContains(result.mGeneratedSource, "global::System.Buffer.MemoryCopy(mStorage->mHP + index, mStorage->mHP + index + 1, (long)moveCount * sizeof(int), (long)moveCount * sizeof(int));");
			assertContains(result.mGeneratedSource, "mStorage->mHP[i] = mStorage->mHP[i + 1];");
			assertContains(result.mGeneratedSource, "global::System.Buffer.MemoryCopy(mStorage->mAoS + index, mStorage->mAoS + index + 1, (long)moveCount * sizeof(RoleDataAoSBlock), (long)moveCount * sizeof(RoleDataAoSBlock));");
			assertContains(result.mGeneratedSource, "mStorage->mAoS[i] = mStorage->mAoS[i + 1];");
		}
		private static void testListUnsafeInsertRemoveAtPerformanceStrategy()
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
", true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "int moveCount = mCount - index;");
			assertContains(result.mGeneratedSource, "global::System.Buffer.MemoryCopy(mStorage->mHP + index, mStorage->mHP + index + 1, (long)moveCount * sizeof(int), (long)moveCount * sizeof(int));");
			assertContains(result.mGeneratedSource, "global::System.Buffer.MemoryCopy(mStorage->mSpeed + index, mStorage->mSpeed + index + 1, (long)moveCount * sizeof(float), (long)moveCount * sizeof(float));");
			assertContains(result.mGeneratedSource, "global::System.Buffer.MemoryCopy(mStorage->mAoS + index, mStorage->mAoS + index + 1, (long)moveCount * sizeof(RoleDataAoSBlock), (long)moveCount * sizeof(RoleDataAoSBlock));");
						assertContains(result.mGeneratedSource, "mStorage->mHP[i] = mStorage->mHP[i + 1];");
			assertContains(result.mGeneratedSource, "mStorage->mAoS[i] = mStorage->mAoS[i + 1];");
			assertContains(result.mGeneratedSource, "if (index == mCount)");
			assertDoesNotContain(result.mGeneratedSource, "for (int i = mCount; i > index; --i)");
			assertContains(result.mGeneratedSource, "for (int i = index; i < lastIndex; ++i)");
			assertDoesNotContain(result.mGeneratedSource, "UnsafeUtility.MemMove");
		}
		private static void testListHybridUnsafeStructuralMoveSplit()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
	public string mName;
	public object mPayload;
	[NotECS] public int mID;
	[NotECS] public string mPath;
}
", true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendReason = \"AllowUnsafe=true,HybridStorage=true\";");
			assertContains(result.mGeneratedSource, "global::System.Buffer.MemoryCopy(mStorage->mHP + index, mStorage->mHP + index + 1, (long)moveCount * sizeof(int), (long)moveCount * sizeof(int));");
			assertContains(result.mGeneratedSource, "mStorage->mSpeed[i] = mStorage->mSpeed[i + 1];");
			assertContains(result.mGeneratedSource, "global::System.Array.Copy(mManagedStorage.mName, index, mManagedStorage.mName, index + 1, mCount - index);");
			assertContains(result.mGeneratedSource, "global::System.Array.Copy(mManagedStorage.mPayload, index + 1, mManagedStorage.mPayload, index, lastIndex - index);");
			assertContains(result.mGeneratedSource, "global::System.Array.Copy(mManagedStorage.mAoS, index, mManagedStorage.mAoS, index + 1, mCount - index);");
			assertContains(result.mGeneratedSource, "global::System.Array.Copy(mManagedStorage.mAoS, index + 1, mManagedStorage.mAoS, index, lastIndex - index);");
			assertDoesNotContain(result.mGeneratedSource, "Buffer.MemoryCopy(mManagedStorage.");
		}
		private static void testListInsertRemoveAtSafeSpanCompile()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
	[NotECS] public int mID;
}
public static class Usage
{
	public static void Run()
	{
		RoleDataECSList list = new RoleDataECSList();
		list.Add(new RoleData { mHP = 1, mID = 1 });
		list.Insert(0, new RoleData { mHP = 2, mID = 2 });
		list.RemoveAt(1);
		list.Dispose();
	}
}
", false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"SafeSpan\";");
			assertContains(result.mGeneratedSource, "global::System.Array.Copy(storage.mHP, index, storage.mHP, index + 1, mCount - index);");
			assertContains(result.mGeneratedSource, "global::System.Array.Copy(storage.mHP, index + 1, storage.mHP, index, moveCount);");
		}
		private static void testListInsertRemoveAtSafeRegistryCompile()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
	[NotECS] public int mID;
}
public static class Usage
{
	public static void Run()
	{
		RoleDataECSList list = new RoleDataECSList();
		list.Add(new RoleData { mHP = 1, mID = 1 });
		list.Insert(0, new RoleData { mHP = 2, mID = 2 });
		list.RemoveAt(1);
		list.Dispose();
	}
}
", true, "ECS_FORCE_SAFE_REGISTRY");
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"SafeRegistry\";");
			assertContains(result.mGeneratedSource, "RoleDataStorage storage = RoleDataStorageRegistry.getStorage(mStorageID);");
			assertContains(result.mGeneratedSource, "public void Insert(int index, global::RoleData value)");
			assertContains(result.mGeneratedSource, "public void RemoveAt(int index)");
		}
		private static void testListInsertRemoveAtHybridUnsafeCompile()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public string mName;
	public object mPayload;
	[NotECS] public int mID;
	[NotECS] public string mPath;
}
public static class Usage
{
	public static void Run()
	{
		RoleDataECSList list = new RoleDataECSList(1);
		object payload = new object();
		list.Add(new RoleData { mHP = 1, mName = ""A"", mPayload = payload, mID = 1, mPath = ""P1"" });
		list.Insert(0, new RoleData { mHP = 2, mName = ""B"", mPayload = payload, mID = 2, mPath = ""P2"" });
		list.RemoveAt(1);
		list[0].mName = ""C"";
		list.Dispose();
	}
}
", true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendReason = \"AllowUnsafe=true,HybridStorage=true\";");
			assertContains(result.mGeneratedSource, "global::System.Buffer.MemoryCopy(mStorage->mHP + index, mStorage->mHP + index + 1, (long)moveCount * sizeof(int), (long)moveCount * sizeof(int));");
			assertContains(result.mGeneratedSource, "global::System.Array.Copy(mManagedStorage.mName, index, mManagedStorage.mName, index + 1, mCount - index);");
			assertContains(result.mGeneratedSource, "global::System.Array.Copy(mManagedStorage.mPayload, index + 1, mManagedStorage.mPayload, index, lastIndex - index);");
			assertContains(result.mGeneratedSource, "global::System.Array.Copy(mManagedStorage.mAoS, index, mManagedStorage.mAoS, index + 1, mCount - index);");
			assertContains(result.mGeneratedSource, "mManagedStorage.mName[lastIndex] = default(");
			assertContains(result.mGeneratedSource, "mManagedStorage.mPayload[lastIndex] = default(");
			assertContains(result.mGeneratedSource, "mManagedStorage.mAoS[lastIndex] = default(RoleDataAoSBlock);");
		}
		private static void testListInsertRemoveAtEditorInvalidationGeneration()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
}
", true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "private void invalidateRefsFrom(int index)");
			assertContains(result.mGeneratedSource, "for (int i = index; i < mCount; ++i)");
			int helperIndex = result.mGeneratedSource.IndexOf("private void invalidateRefsFrom(int index)", StringComparison.Ordinal);
			int helperIfIndex = result.mGeneratedSource.LastIndexOf("#if UNITY_EDITOR", helperIndex, StringComparison.Ordinal);
			int helperEndIfIndex = result.mGeneratedSource.IndexOf("#endif", helperIndex, StringComparison.Ordinal);
			if (helperIfIndex < 0 || helperEndIfIndex < 0 || helperIfIndex > helperIndex || helperEndIfIndex < helperIndex)
			{
				throw new Exception("invalidateRefsFrom没有完全生成在UNITY_EDITOR分支中");
			}
			int insertIndex = result.mGeneratedSource.IndexOf("public void Insert(int index, global::RoleData value)", StringComparison.Ordinal);
			int removeIndex = result.mGeneratedSource.IndexOf("public void RemoveAt(int index)", insertIndex, StringComparison.Ordinal);
			if (insertIndex < 0 || removeIndex < 0)
			{
				throw new Exception("没有找到Insert/RemoveAt生成代码");
			}
			int insertInvalidation = result.mGeneratedSource.IndexOf("invalidateRefsFrom(index);", insertIndex, StringComparison.Ordinal);
			int removeInvalidation = result.mGeneratedSource.IndexOf("invalidateRefsFrom(index);", removeIndex, StringComparison.Ordinal);
			if (insertInvalidation < 0 || insertInvalidation > removeIndex || removeInvalidation < 0)
			{
				throw new Exception("Insert/RemoveAt没有生成Ref区间失效逻辑");
			}
			assertContains(result.mGeneratedSource, "invalidateColumn();");
		}
		private static void testListExtendedApiGeneration()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	[NotECS] public int mID;
}
", true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public int EnsureCapacity(int capacity)");
			assertContains(result.mGeneratedSource, "public void TrimExcess()");
			assertContains(result.mGeneratedSource, "public void AddRange(global::RoleData[] values)");
			assertContains(result.mGeneratedSource, "public void AddRange(RoleDataECSList values)");
			assertContains(result.mGeneratedSource, "public void InsertRange(int index, global::RoleData[] values");
			assertContains(result.mGeneratedSource, "public void InsertRange(int index, RoleDataECSList values)");
			assertContains(result.mGeneratedSource, "public void RemoveRange(int index, int count)");
			assertContains(result.mGeneratedSource, "public int RemoveAll(global::System.Predicate<global::RoleData> match)");
			assertContains(result.mGeneratedSource, "public void Reverse(int index, int count)");
			assertContains(result.mGeneratedSource, "public void Sort(global::System.Comparison<global::RoleData> comparison)");
			assertContains(result.mGeneratedSource, "public int BinarySearch(int index, int count, global::RoleData value");
			assertContains(result.mGeneratedSource, "public void SortByHP()");
			assertContains(result.mGeneratedSource, "private void introSortBy");
			assertContains(result.mGeneratedSource, "pickPivotAndPartitionBy");
			assertDoesNotContain(result.mGeneratedSource, "[global::System.ThreadStaticAttribute]");
			assertDoesNotContain(result.mGeneratedSource, "private static int[] rentSortPermutation(int count)");
			assertDoesNotContain(result.mGeneratedSource, "private void applySortPermutation(int index, int count, int[] permutation)");
			assertDoesNotContain(result.mGeneratedSource, "introSortPermutation(permutation");
			assertContains(result.mGeneratedSource, "public int BinarySearchByHP(");
			assertContains(result.mGeneratedSource, "public bool ContainsByHP(");
			assertContains(result.mGeneratedSource, "public int IndexOfByHP(");
			assertContains(result.mGeneratedSource, "public int LastIndexOfByHP(");
			assertContains(result.mGeneratedSource, "public bool ExistsByHP(");
			assertContains(result.mGeneratedSource, "public int FindIndexByHP(");
			assertContains(result.mGeneratedSource, "public int RemoveAllByHP(");
			if (result.mGeneratedSource.IndexOf("ContainsByID(", StringComparison.Ordinal) >= 0 || result.mGeneratedSource.IndexOf("RemoveAllByID(", StringComparison.Ordinal) >= 0)
			{
				throw new Exception("NotECS字段不应生成ByColumn高速API");
			}
			assertContains(result.mGeneratedSource, "public global::RoleData[] ToArray()");
			assertContains(result.mGeneratedSource, "private void moveRange(int sourceIndex, int destinationIndex, int count)");
			assertContains(result.mGeneratedSource, "private void copyFromArray(global::RoleData[] source");
			assertContains(result.mGeneratedSource, "private void copyToArray(int sourceIndex, global::RoleData[] destination");
			assertContains(result.mGeneratedSource, "private void reverseRange(int index, int count)");
			assertContains(result.mGeneratedSource, "private void invalidateRefsRange(int index, int count)");
		}
		private static void testListSortByAdaptiveStrategyGeneration()
		{
			GeneratorTestResult unmanagedResult = runGenerator(@"
[ECS]
public struct UnmanagedRole
{
	public int mHP;
	public float mSpeed;
	[NotECS] public int mID;
}
", true);
			assertNoErrors(unmanagedResult);
			assertContains(unmanagedResult.mGeneratedSource, "private void introSortBy");
			assertContains(unmanagedResult.mGeneratedSource, "pickPivotAndPartitionBy");
			assertDoesNotContain(unmanagedResult.mGeneratedSource, "rentSortPermutation");
			assertDoesNotContain(unmanagedResult.mGeneratedSource, "applySortPermutation");
			GeneratorTestResult managedECSResult = runGenerator(@"
[ECS]
public struct ManagedRole
{
	public int mHP;
	public string mName;
	[NotECS] public int mID;
}
", true);
			assertNoErrors(managedECSResult);
			assertContains(managedECSResult.mGeneratedSource, "[global::System.ThreadStaticAttribute]");
			assertContains(managedECSResult.mGeneratedSource, "private static int[] rentSortPermutation(int count)");
			assertContains(managedECSResult.mGeneratedSource, "private void applySortPermutation(int index, int count, int[] permutation)");
			assertContains(managedECSResult.mGeneratedSource, "introSortPermutation(permutation");
			assertDoesNotContain(managedECSResult.mGeneratedSource, "private void introSortBy");
			GeneratorTestResult managedAoSResult = runGenerator(@"
[ECS]
public struct ManagedAoSRole
{
	public int mHP;
	[NotECS] public string mPath;
}
", true);
			assertNoErrors(managedAoSResult);
			assertContains(managedAoSResult.mGeneratedSource, "rentSortPermutation");
			assertContains(managedAoSResult.mGeneratedSource, "applySortPermutation");
			assertDoesNotContain(managedAoSResult.mGeneratedSource, "private void introSortBy");
		}
		private static void testListArrayConversionStrategyGeneration()
		{
			GeneratorTestResult unmanagedResult = runGenerator(@"
[ECS]
public struct UnmanagedBulkData
{
	public int mHP;
	public float mSpeed;
	[NotECS] public int mID;
}
", true);
			assertNoErrors(unmanagedResult);
			assertDoesNotContain(unmanagedResult.mGeneratedSource, "ref global::UnmanagedBulkData sourceValue = ref source[sourceCursor];");
			GeneratorTestResult managedUnsafeResult = runGenerator(@"
[ECS]
public struct ManagedBulkData
{
	public int mHP;
	public string mName;
	public object mPayload;
	[NotECS] public int mID;
	[NotECS] public string mPath;
}
", true);
			assertNoErrors(managedUnsafeResult);
			assertContains(managedUnsafeResult.mGeneratedSource, "int* column0 = mStorage->mHP;");
			assertContains(managedUnsafeResult.mGeneratedSource, "string[] column1 = mManagedStorage.mName;");
			assertContains(managedUnsafeResult.mGeneratedSource, "ref global::ManagedBulkData sourceValue = ref source[sourceCursor];");
			assertContains(managedUnsafeResult.mGeneratedSource, "ref global::ManagedBulkData destinationValue = ref destination[destinationCursor];");
			assertContains(managedUnsafeResult.mGeneratedSource, "destinationValue.mHP = column0[sourceCursor];");
			assertContains(managedUnsafeResult.mGeneratedSource, "destinationValue.mPath = aosColumn[sourceCursor].mPath;");
			assertDoesNotContain(managedUnsafeResult.mGeneratedSource, "global::ManagedBulkData destinationValue = default(global::ManagedBulkData);");
			assertDoesNotContain(managedUnsafeResult.mGeneratedSource, "destination[destinationCursor] = destinationValue;");
			GeneratorTestResult managedSafeSpanResult = runGenerator(@"
[ECS]
public struct ManagedBulkData
{
	public int mHP;
	public string mName;
	public object mPayload;
	[NotECS] public int mID;
	[NotECS] public string mPath;
}
", false);
			assertNoErrors(managedSafeSpanResult);
			assertContains(managedSafeSpanResult.mGeneratedSource, "public const string BackendName = \"SafeSpan\";");
			assertContains(managedSafeSpanResult.mGeneratedSource, "int[] column0 = storage.mHP;");
			assertContains(managedSafeSpanResult.mGeneratedSource, "ref global::ManagedBulkData destinationValue = ref destination[destinationCursor];");
			assertDoesNotContain(managedSafeSpanResult.mGeneratedSource, "global::ManagedBulkData destinationValue = default(global::ManagedBulkData);");
			GeneratorTestResult managedSafeRegistryResult = runGenerator(@"
[ECS]
public struct ManagedBulkData
{
	public int mHP;
	public string mName;
	public object mPayload;
	[NotECS] public int mID;
	[NotECS] public string mPath;
}
", false, "ECS_FORCE_SAFE_REGISTRY");
			assertNoErrors(managedSafeRegistryResult);
			assertContains(managedSafeRegistryResult.mGeneratedSource, "public const string BackendName = \"SafeRegistry\";");
			assertContains(managedSafeRegistryResult.mGeneratedSource, "string[] column1 = storage.mName;");
			assertContains(managedSafeRegistryResult.mGeneratedSource, "ref global::ManagedBulkData destinationValue = ref destination[destinationCursor];");
		}
		private static void testListExtendedApiUnsafeCompile()
		{
			GeneratorTestResult result = runGenerator(LIST_EXTENDED_USAGE_SOURCE, true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"Unsafe\";");
			assertContains(result.mGeneratedSource, "copyRangeFrom(RangeDataECSList source");
			assertContains(result.mGeneratedSource, "mStorage->mValue + sourceIndex, count).CopyTo");
			assertContains(result.mGeneratedSource, "getSortField_mValue");
			assertContains(result.mGeneratedSource, "private void introSortBy");
			assertDoesNotContain(result.mGeneratedSource, "applySortPermutation");
		}
		private static void testListExtendedApiSafeSpanCompile()
		{
			GeneratorTestResult result = runGenerator(LIST_EXTENDED_USAGE_SOURCE, false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"SafeSpan\";");
			assertContains(result.mGeneratedSource, "global::System.Array.Copy(storage.mValue, sourceIndex, storage.mValue, destinationIndex, count);");
			assertContains(result.mGeneratedSource, "private void introSortBy");
			assertDoesNotContain(result.mGeneratedSource, "applySortPermutation");
		}
		private static void testListExtendedApiSafeRegistryCompile()
		{
			GeneratorTestResult result = runGenerator(LIST_EXTENDED_USAGE_SOURCE, true, "ECS_FORCE_SAFE_REGISTRY");
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"SafeRegistry\";");
			assertContains(result.mGeneratedSource, "RangeDataStorage storage = RangeDataStorageRegistry.getStorage(mStorageID);");
			assertContains(result.mGeneratedSource, "private void introSortBy");
			assertDoesNotContain(result.mGeneratedSource, "applySortPermutation");
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
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, false);
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
			assertContains(source, "if ((uint)index >= (uint)mCount)\n\t\t{");
		}
		private static void testGeneratedContainerSourceNavigation()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
}
", false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "/// <see cref=\"RoleData\"/>的EasyECS List.");
			assertContains(result.mGeneratedSource, "[global::EasyECS.ECSGeneratedFor(typeof(global::RoleData))]");
			assertContains(result.mGeneratedSource, "public sealed class RoleDataECSList");
			assertContains(result.mGeneratedSource, "/// <see cref=\"RoleData\"/>的EasyECS Dictionary&lt;TKey&gt;.");
			assertContains(result.mGeneratedSource, "public sealed class RoleDataECSDictionary<TKey>");
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
		private static void testBurstUnavailableDoesNotGenerate()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, true);
			assertNoErrors(result);
			assertDoesNotContain(result.mGeneratedSource, "struct BurstView");
			assertDoesNotContain(result.mGeneratedSource, "ScheduleBurst<TJob>");
			assertDoesNotContain(result.mGeneratedSource, "CompleteBurstJobs()");
			assertDoesNotContain(result.mGeneratedSource, "global::Unity.Jobs");
		}
		private static void testBurstUnsafeGeneration()
		{
			GeneratorTestResult result = runGenerator(BURST_STUB_SOURCE + DICTIONARY_DATA_SOURCE, true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public readonly unsafe struct BurstView");
			assertContains(result.mGeneratedSource, "public readonly int* mHP;");
			assertContains(result.mGeneratedSource, "public readonly float* mSpeed;");
			assertContains(result.mGeneratedSource, "public readonly float* mPositionX;");
			assertContains(result.mGeneratedSource, "public readonly float* mPositionY;");
			assertContains(result.mGeneratedSource, "public readonly int Count;");
			assertContains(result.mGeneratedSource, "public BurstView(");
			assertContains(result.mGeneratedSource, "public BurstView GetBurstView()");
			assertContains(result.mGeneratedSource, "public global::Unity.Jobs.JobHandle ScheduleBurst<TJob>");
			assertContains(result.mGeneratedSource, "public global::Unity.Jobs.JobHandle GetBurstDependency()");
			assertContains(result.mGeneratedSource, "public void RegisterBurstJob(global::Unity.Jobs.JobHandle handle)");
			assertContains(result.mGeneratedSource, "public void CompleteBurstJobs()");
			assertContains(result.mGeneratedSource, "global::Unity.Jobs.JobHandle.CombineDependencies(mBurstJobHandle, dependsOn)");
			assertContains(result.mGeneratedSource, "global::Unity.Jobs.IJobParallelForExtensions.Schedule(job, mCount, innerloopBatchCount, dependsOn)");
			assertContains(result.mGeneratedSource, "completeBurstJobs();");
			assertContains(result.mGeneratedSource, "~RoleDataECSList()");
			assertContains(result.mGeneratedSource, "if (mHasPendingBurstJob)");
			assertContains(result.mGeneratedSource, "public RoleDataECSList.BurstView GetBurstView()");
		}
		private static void testBurstHybridFieldFiltering()
		{
			GeneratorTestResult result = runGenerator(BURST_STUB_SOURCE + @"
[ECS]
public struct BurstHybridData
{
	public int mHP;
	public float mSpeed;
	public string mName;
	public object mPayload;
	[NotECS] public int mID;
}
", true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendReason = \"AllowUnsafe=true,HybridStorage=true\";");
			assertContains(result.mGeneratedSource, "public readonly unsafe struct BurstView");
			assertContains(result.mGeneratedSource, "public readonly int* mHP;");
			assertContains(result.mGeneratedSource, "public readonly float* mSpeed;");
			assertDoesNotContain(result.mGeneratedSource, "public readonly string* mName;");
			assertDoesNotContain(result.mGeneratedSource, "public readonly object* mPayload;");
			assertDoesNotContain(result.mGeneratedSource, "public readonly int* mID;");
		}
		private static void testBurstSafeBackendDoesNotGenerate()
		{
			GeneratorTestResult safeSpanResult = runGenerator(BURST_STUB_SOURCE + DICTIONARY_DATA_SOURCE, false);
			assertNoErrors(safeSpanResult);
			assertContains(safeSpanResult.mGeneratedSource, "public const string BackendName = \"SafeSpan\";");
			assertDoesNotContain(safeSpanResult.mGeneratedSource, "struct BurstView");
			GeneratorTestResult safeRegistryResult = runGenerator(BURST_STUB_SOURCE + DICTIONARY_DATA_SOURCE, true, "ECS_FORCE_SAFE_REGISTRY");
			assertNoErrors(safeRegistryResult);
			assertContains(safeRegistryResult.mGeneratedSource, "public const string BackendName = \"SafeRegistry\";");
			assertDoesNotContain(safeRegistryResult.mGeneratedSource, "struct BurstView");
		}
		private static void testBurstUsageCompile()
		{
			GeneratorTestResult result = runGenerator(BURST_STUB_SOURCE + DICTIONARY_DATA_SOURCE + @"
public unsafe struct RoleBurstTestJob : Unity.Jobs.IJobParallelFor
{
	public RoleDataECSList.BurstView mData;
	public void Execute(int index)
	{
		mData.mHP[index] += 1;
		mData.mPositionX[index] += mData.mSpeed[index];
	}
}
public static class RoleBurstUsage
{
	public static int Run()
	{
		RoleDataECSList list = new RoleDataECSList(8);
		list.Add(new RoleData { mHP = 10, mSpeed = 2.0f, mPositionX = 1.0f });
		RoleDataECSList.BurstView view = list.GetBurstView();
		Unity.Jobs.JobHandle handle = list.ScheduleBurst(new RoleBurstTestJob { mData = view }, 64);
		list.CompleteBurstJobs();
		int value = list[0].mHP;
		list.Dispose();
		return value;
	}
}
", true);
			assertNoErrors(result);
		}
		private static void testDictionaryGeneration()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public sealed class RoleDataECSDictionary<TKey> : global::System.IDisposable");
			assertContains(result.mGeneratedSource, "private readonly global::System.Collections.Generic.Dictionary<TKey, int> mIndexMap;");
			assertContains(result.mGeneratedSource, "private readonly RoleDataECSList mValues;");
			assertContains(result.mGeneratedSource, "private TKey[] mKeys;");
			assertContains(result.mGeneratedSource, "public RoleDataRef this[TKey key]");
			assertContains(result.mGeneratedSource, "public void Add(TKey key, global::RoleData value)");
			assertContains(result.mGeneratedSource, "public bool TryAdd(TKey key, global::RoleData value)");
			assertContains(result.mGeneratedSource, "public bool ContainsKey(TKey key)");
			assertContains(result.mGeneratedSource, "public bool TryGetValue(TKey key, out RoleDataRef value)");
			assertContains(result.mGeneratedSource, "public bool Remove(TKey key)");
			assertContains(result.mGeneratedSource, "public TKey getKeyAt(int index)");
			assertContains(result.mGeneratedSource, "public RoleDataRef getValueAt(int index)");
		}
		private static void testDictionaryTryGetIndexGeneration()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public bool TryGetIndex(TKey key, out int index)");
			assertContains(result.mGeneratedSource, "return mIndexMap.TryGetValue(key, out index);");
		}
		private static void testDictionaryDenseIndexGeneration()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public int GetIndex(TKey key)");
			assertContains(result.mGeneratedSource, "return mIndexMap[key];");
			assertContains(result.mGeneratedSource, "public int GetOrAddIndex(TKey key)");
			assertContains(result.mGeneratedSource, "public int GetOrAddIndex(TKey key, global::RoleData value)");
			assertContains(result.mGeneratedSource, "public int GetOrAddIndex(TKey key, global::RoleData value, out bool added)");
			assertContains(result.mGeneratedSource, "return addValue(key, value);");
			assertContains(result.mGeneratedSource, "private int addValue(TKey key, global::RoleData value)");
			assertContains(result.mGeneratedSource, "return index;");
		}
		private static void testDictionaryExtendedApiGeneration()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public void SetValue(TKey key, global::RoleData value)");
			assertContains(result.mGeneratedSource, "public bool TrySetValue(TKey key, global::RoleData value)");
			assertContains(result.mGeneratedSource, "public RoleDataRef SetOrAdd(TKey key, global::RoleData value)");
			assertContains(result.mGeneratedSource, "public RoleDataRef GetOrAdd(TKey key)");
			assertContains(result.mGeneratedSource, "public int GetOrAddIndex(TKey key)");
			assertContains(result.mGeneratedSource, "public int GetOrAddIndex(TKey key, global::RoleData value, out bool added)");
			assertContains(result.mGeneratedSource, "public bool ContainsValue(global::RoleData value)");
			assertContains(result.mGeneratedSource, "public bool Remove(TKey key, out global::RoleData value)");
			assertContains(result.mGeneratedSource, "public int EnsureCapacity(int capacity)");
			assertContains(result.mGeneratedSource, "public void TrimExcess()");
			assertContains(result.mGeneratedSource, "public unsafe int GetValueByHP(TKey key)");
			assertContains(result.mGeneratedSource, "public unsafe bool TryGetValueByHP(TKey key, out int value)");
			assertContains(result.mGeneratedSource, "public unsafe void SetValueByHP(TKey key, int value)");
			assertContains(result.mGeneratedSource, "public unsafe bool TrySetValueByHP(TKey key, int value)");
			assertDoesNotContain(result.mGeneratedSource, "GetValueByID");
			assertContains(result.mGeneratedSource, "mValues.getDictionaryStorage()->mHP[index] = value;");
			assertContains(result.mGeneratedSource, "if (!mIndexMap.Remove(key, out int removeIndex))");
			assertContains(result.mGeneratedSource, "if (!mIndexMap.TryAdd(key, index))");
		}
		private static void testDictionaryUnsafeCompile()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE + DICTIONARY_USAGE_SOURCE, true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"Unsafe\";");
		}
		private static void testDictionaryHybridUnsafeCompile()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public string mName;
	[NotECS] public int mID;
	[NotECS] public string mModelPath;
}
public static class Usage
{
	public static void Run()
	{
		RoleDataECSDictionary<int> dict = new RoleDataECSDictionary<int>();
		dict.Add(1, new RoleData { mHP = 10, mName = ""A"", mID = 1, mModelPath = ""P"" });
		RoleDataRef value = dict[1];
		value.mHP += 1;
		value.mName = ""B"";
		var hp = dict.getHPColumn();
		var names = dict.getNameColumn();
		hp[0] += 1;
		names[0] = ""C"";
		dict.SetValueByHP(1, 12);
		dict.SetValueByName(1, ""Fast"");
		dict.TryGetValueByName(1, out string fastName);
		int denseIndex = dict.GetIndex(1);
		int getOrAddIndex = dict.GetOrAddIndex(2, new RoleData { mHP = 20, mName = ""Added"", mID = 2, mModelPath = ""Path/2"" }, out bool indexAdded);
		hp[denseIndex] += getOrAddIndex + (indexAdded ? 1 : 0);
		foreach (var item in dict)
		{
			item.Value.mHP += 1;
			item.Value.mName = ""D"";
		}
		dict.Dispose();
	}
}
", true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"Unsafe\";");
			assertContains(result.mGeneratedSource, "getDictionaryManagedStorage()");
			assertContains(result.mGeneratedSource, "private readonly RoleDataManagedStorage mManagedStorage;");
			assertContains(result.mGeneratedSource, "return new RoleDataRef(mStorage, mManagedStorage, mIndex);");
			assertContains(result.mGeneratedSource, "public unsafe void SetValueByHP(TKey key, int value)");
			assertContains(result.mGeneratedSource, "public void SetValueByName(TKey key, string value)");
			assertContains(result.mGeneratedSource, "mValues.getDictionaryStorage()->mHP[index] = value;");
			assertContains(result.mGeneratedSource, "mValues.getDictionaryManagedStorage().mName[index] = value;");
		}
		private static void testDictionarySafeSpanCompile()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE + DICTIONARY_USAGE_SOURCE, false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"SafeSpan\";");
			assertContains(result.mGeneratedSource, "mValues.getDictionaryStorage()[0].mHP[index] = value;");
		}
		private static void testDictionarySafeRegistryCompile()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE + DICTIONARY_USAGE_SOURCE, false, "ECS_FORCE_SAFE_REGISTRY");
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public const string BackendName = \"SafeRegistry\";");
			assertContains(result.mGeneratedSource, "RoleDataStorageRegistry.get_mHP(mValues.getDictionaryStorageID(), index) = value;");
		}
		private static void testDictionaryUnsafeForeachFastPath()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public unsafe ref struct Entry");
			assertContains(result.mGeneratedSource, "private readonly RoleDataStorage* mStorage;");
			assertContains(result.mGeneratedSource, "return new RoleDataRef(mStorage, mIndex);");
			assertContains(result.mGeneratedSource, "public unsafe ref struct Enumerator");
			assertContains(result.mGeneratedSource, "mStorage = owner.mValues.getDictionaryStorage();");
			assertContains(result.mGeneratedSource, "return new Entry(mKeys, new RoleDataRef(mStorage, mIndex), mIndex);");
			assertDoesNotContain(getPlayerDictionaryEnumeratorSource(result.mGeneratedSource), "mValues[mIndex]");
		}
		private static void testDictionarySafeSpanForeachFastPath()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "private readonly RoleDataStorage[] mStorage;");
			assertContains(result.mGeneratedSource, "return new RoleDataRef(mStorage, mIndex);");
			assertContains(result.mGeneratedSource, "mStorage = owner.mValues.getDictionaryStorage();");
			assertContains(result.mGeneratedSource, "return new Entry(mKeys, new RoleDataRef(mStorage, mIndex), mIndex);");
		}
		private static void testDictionarySafeRegistryForeachFastPath()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, false, "ECS_FORCE_SAFE_REGISTRY");
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "private readonly int mStorageID;");
			assertContains(result.mGeneratedSource, "return new RoleDataRef(mStorageID, mIndex);");
			assertContains(result.mGeneratedSource, "mStorageID = owner.mValues.getDictionaryStorageID();");
			assertContains(result.mGeneratedSource, "return new Entry(mKeys, new RoleDataRef(mStorageID, mIndex), mIndex);");
		}
		private static void testDictionaryPlayerEntryLazyKey()
		{
			GeneratorTestResult unsafeResult = runGenerator(DICTIONARY_DATA_SOURCE, true);
			assertNoErrors(unsafeResult);
			assertContains(unsafeResult.mGeneratedSource, "public Entry(TKey[] keys, RoleDataRef value, int index)");
			assertContains(unsafeResult.mGeneratedSource, "return mKeys[mIndex];");
			assertContains(unsafeResult.mGeneratedSource, "return new Entry(mKeys, new RoleDataRef(mStorage, mIndex), mIndex);");
			assertDoesNotContain(getPlayerDictionaryEnumeratorSource(unsafeResult.mGeneratedSource), "new Entry(mKeys[mIndex]");
			assertDoesNotContain(getPlayerDictionaryEntrySource(unsafeResult.mGeneratedSource), "RoleDataStorage* storage");
			assertDoesNotContain(getPlayerDictionaryEntrySource(unsafeResult.mGeneratedSource), "RoleDataStorage[] storage");
			assertDoesNotContain(getPlayerDictionaryEntrySource(unsafeResult.mGeneratedSource), "RoleDataManagedStorage managedStorage");

			GeneratorTestResult hybridResult = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public string mName;
	[NotECS] public int mID;
	[NotECS] public string mModelPath;
}
", true);
			assertNoErrors(hybridResult);
			assertContains(hybridResult.mGeneratedSource, "public Entry(TKey[] keys, RoleDataRef value, int index)");
			assertContains(hybridResult.mGeneratedSource, "return new Entry(mKeys, new RoleDataRef(mStorage, mManagedStorage, mIndex), mIndex);");

			GeneratorTestResult safeSpanResult = runGenerator(DICTIONARY_DATA_SOURCE, false);
			assertNoErrors(safeSpanResult);
			assertContains(safeSpanResult.mGeneratedSource, "public Entry(TKey[] keys, RoleDataRef value, int index)");
			assertContains(safeSpanResult.mGeneratedSource, "return new Entry(mKeys, new RoleDataRef(mStorage, mIndex), mIndex);");

			GeneratorTestResult registryResult = runGenerator(DICTIONARY_DATA_SOURCE, false, "ECS_FORCE_SAFE_REGISTRY");
			assertNoErrors(registryResult);
			assertContains(registryResult.mGeneratedSource, "public Entry(TKey[] keys, RoleDataRef value, int index)");
			assertContains(registryResult.mGeneratedSource, "return new Entry(mKeys, new RoleDataRef(mStorageID, mIndex), mIndex);");
		}
		private static string getPlayerDictionaryEntrySource(string generatedSource)
		{
			int start = generatedSource.IndexOf("#else", generatedSource.IndexOf("public ref struct Entry", StringComparison.Ordinal), StringComparison.Ordinal);
			if (start < 0)
			{
				start = generatedSource.IndexOf("#else", generatedSource.IndexOf("public unsafe ref struct Entry", StringComparison.Ordinal), StringComparison.Ordinal);
			}
			if (start < 0)
			{
				return generatedSource;
			}
			int end = generatedSource.IndexOf("#endif", start, StringComparison.Ordinal);
			return end >= 0 ? generatedSource.Substring(start, end - start) : generatedSource.Substring(start);
		}
		private static void testDictionaryKeysValuesGeneration()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public KeyEnumerable Keys");
			assertContains(result.mGeneratedSource, "public ValueEnumerable Values");
			assertContains(result.mGeneratedSource, "public readonly struct KeyEnumerable");
			assertContains(result.mGeneratedSource, "public struct KeyEnumerator");
			assertContains(result.mGeneratedSource, "public readonly struct ValueEnumerable");
			assertContains(result.mGeneratedSource, "public ref struct ValueEnumerator");
		}
		private static void testDictionaryKeysPlayerFastPath()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public global::System.ReadOnlySpan<TKey>.Enumerator GetEnumerator()");
			assertContains(result.mGeneratedSource, "return new global::System.ReadOnlySpan<TKey>(mOwner.mKeys, 0, mOwner.mValues.Count).GetEnumerator();");
			assertDoesNotContain(getPlayerDictionaryKeysSource(result.mGeneratedSource), "public struct KeyEnumerator");
			assertDoesNotContain(getPlayerDictionaryKeysSource(result.mGeneratedSource), "return mKeys[mIndex];");

			GeneratorTestResult safeSpanResult = runGenerator(DICTIONARY_DATA_SOURCE, false);
			assertNoErrors(safeSpanResult);
			assertContains(safeSpanResult.mGeneratedSource, "public global::System.ReadOnlySpan<TKey>.Enumerator GetEnumerator()");

			GeneratorTestResult registryResult = runGenerator(DICTIONARY_DATA_SOURCE, false, "ECS_FORCE_SAFE_REGISTRY");
			assertNoErrors(registryResult);
			assertContains(registryResult.mGeneratedSource, "public global::System.ReadOnlySpan<TKey>.Enumerator GetEnumerator()");
		}
		private static string getPlayerDictionaryKeysSource(string generatedSource)
		{
			int keyEnumerableStart = generatedSource.IndexOf("public readonly struct KeyEnumerable", StringComparison.Ordinal);
			if (keyEnumerableStart < 0)
			{
				return generatedSource;
			}
			int playerStart = generatedSource.IndexOf("#else", keyEnumerableStart, StringComparison.Ordinal);
			if (playerStart < 0)
			{
				return generatedSource.Substring(keyEnumerableStart);
			}
			int playerEnd = generatedSource.IndexOf("#endif", playerStart, StringComparison.Ordinal);
			return playerEnd >= 0 ? generatedSource.Substring(playerStart, playerEnd - playerStart) : generatedSource.Substring(playerStart);
		}
		private static void testDictionaryKeysReadOnlySpanSafety()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, true);
			assertNoErrors(result);
			string playerKeys = getPlayerDictionaryKeysSource(result.mGeneratedSource);
			assertContains(playerKeys, "global::System.ReadOnlySpan<TKey>.Enumerator");
			assertDoesNotContain(playerKeys, "global::System.Span<TKey>.Enumerator");
			assertDoesNotContain(playerKeys, "global::System.Span<TKey>(");
		}
		private static void testDictionaryValuesUnsafeFastPath()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public unsafe struct ValueEnumerator");
			assertContains(result.mGeneratedSource, "mStorage = owner.mValues.getDictionaryStorage();");
			assertContains(result.mGeneratedSource, "return new RoleDataRef(mStorage, mIndex);");
		}
		private static void testDictionaryValuesHybridUnsafeFastPath()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public string mName;
	[NotECS] public int mID;
	[NotECS] public string mModelPath;
}
", true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public unsafe struct ValueEnumerator");
			assertContains(result.mGeneratedSource, "mStorage = owner.mValues.getDictionaryStorage();");
			assertContains(result.mGeneratedSource, "mManagedStorage = owner.mValues.getDictionaryManagedStorage();");
			assertContains(result.mGeneratedSource, "return new RoleDataRef(mStorage, mManagedStorage, mIndex);");
		}
		private static void testDictionaryValuesSafeSpanFastPath()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, false);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public ref struct ValueEnumerator");
			assertDoesNotContain(getPlayerDictionaryValuesSource(result.mGeneratedSource), "public struct ValueEnumerator");
			assertContains(result.mGeneratedSource, "mStorage = owner.mValues.getDictionaryStorage();");
			assertContains(result.mGeneratedSource, "return new RoleDataRef(mStorage, mIndex);");
		}
		private static string getPlayerDictionaryValuesSource(string generatedSource)
		{
			int valueEnumerableStart = generatedSource.IndexOf("public readonly struct ValueEnumerable", StringComparison.Ordinal);
			if (valueEnumerableStart < 0)
			{
				return generatedSource;
			}
			int playerStart = generatedSource.IndexOf("#else", valueEnumerableStart, StringComparison.Ordinal);
			if (playerStart < 0)
			{
				return generatedSource.Substring(valueEnumerableStart);
			}
			int playerEnd = generatedSource.IndexOf("#endif", playerStart, StringComparison.Ordinal);
			return playerEnd >= 0 ? generatedSource.Substring(playerStart, playerEnd - playerStart) : generatedSource.Substring(playerStart);
		}
		private static void testDictionaryValuesSafeRegistryFastPath()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, false, "ECS_FORCE_SAFE_REGISTRY");
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "public struct ValueEnumerator");
			assertContains(result.mGeneratedSource, "mStorageID = owner.mValues.getDictionaryStorageID();");
			assertContains(result.mGeneratedSource, "return new RoleDataRef(mStorageID, mIndex);");
		}
		private static void testDictionaryDoesNotImplementCollectionInterfaces()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, false);
			assertNoErrors(result);
			assertDoesNotContain(result.mGeneratedSource, "RoleDataECSDictionary<TKey> : global::System.Collections.Generic.IEnumerable");
			assertDoesNotContain(result.mGeneratedSource, "global::System.Collections.Generic.IEnumerator<");
			assertDoesNotContain(result.mGeneratedSource, "global::System.Collections.IEnumerator");
			assertDoesNotContain(result.mGeneratedSource, "global::System.Collections.Generic.KeyValuePair<TKey, global::RoleData>");
		}
		private static void testDictionaryEditorVersionValidationGeneration()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "#if UNITY_EDITOR");
			assertContains(result.mGeneratedSource, "private int mVersion;");
			assertContains(result.mGeneratedSource, "private void validateEnumeratorVersion(int version)");
			assertContains(result.mGeneratedSource, "private void validateEnumeratorCurrent(int index, int count, int version)");
			assertContains(result.mGeneratedSource, "mOwner.validateEnumeratorVersion(mVersion);");
			assertContains(result.mGeneratedSource, "mOwner.validateEnumeratorCurrent(mIndex, mCount, mVersion);");
			assertContains(result.mGeneratedSource, "ECSDictionary在遍历期间发生了结构变化");
			assertContains(result.mGeneratedSource, "ECSDictionary Enumerator的Current当前无效");
		}
		private static void testDictionaryStructuralVersionIncrementGeneration()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, true);
			assertNoErrors(result);
			int incrementCount = countOccurrences(result.mGeneratedSource, "++mVersion;");
			if (incrementCount < 4)
			{
				throw new Exception("Dictionary结构修改的mVersion递增点不足,Expected>=4,Actual:" + incrementCount);
			}
			assertContains(result.mGeneratedSource, "mValues.RemoveAtSwapBack(removeIndex);");
			assertContains(result.mGeneratedSource, "mValues.Clear();");
			assertContains(result.mGeneratedSource, "mValues.Add(value);");
			assertContains(result.mGeneratedSource, "mValues.Dispose();");
		}
		private static void testSafeResizeCommitAfterAllocation()
		{
			GeneratorTestResult result = runGenerator(@"
[ECS]
public struct RoleData
{
	public int mHP;
	public float mSpeed;
	[NotECS] public int mID;
}
", false);
			assertNoErrors(result);
			assertBefore(result.mGeneratedSource, "int[] new_mHP = new int[capacity];", "storage.mHP = new_mHP;");
			assertBefore(result.mGeneratedSource, "float[] new_mSpeed = new float[capacity];", "storage.mHP = new_mHP;");
			assertBefore(result.mGeneratedSource, "RoleDataAoSBlock[] newAoS = new RoleDataAoSBlock[capacity];", "storage.mHP = new_mHP;");
			assertBefore(result.mGeneratedSource, "storage.mHP = new_mHP;", "mCapacity = capacity;");
			assertBefore(result.mGeneratedSource, "storage.mSpeed = new_mSpeed;", "mCapacity = capacity;");
			assertBefore(result.mGeneratedSource, "storage.mAoS = newAoS;", "mCapacity = capacity;");
		}
		private static void testUnsafeConstructorCleanupGeneration()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, true);
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "\t\tcatch");
			assertContains(result.mGeneratedSource, "global::System.Runtime.InteropServices.Marshal.FreeHGlobal(mRawMemory);");
			assertContains(result.mGeneratedSource, "global::System.Runtime.InteropServices.Marshal.FreeHGlobal(mStorageMemory);");
			assertContains(result.mGeneratedSource, "mDisposed = true;");
			assertContains(result.mGeneratedSource, "global::System.GC.SuppressFinalize(this);");
		}
		private static void testSafeRegistryConstructorCleanupGeneration()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, true, "ECS_FORCE_SAFE_REGISTRY");
			assertNoErrors(result);
			assertContains(result.mGeneratedSource, "RoleDataStorageRegistry.remove(mStorageID);");
			assertContains(result.mGeneratedSource, "mStorageID = -1;");
			assertContains(result.mGeneratedSource, "mDisposed = true;");
			assertContains(result.mGeneratedSource, "global::System.GC.SuppressFinalize(this);");
		}
		private static void testDictionaryConstructorResourceOrder()
		{
			GeneratorTestResult result = runGenerator(DICTIONARY_DATA_SOURCE, true);
			assertNoErrors(result);
			assertBefore(result.mGeneratedSource, "mIndexMap = new global::System.Collections.Generic.Dictionary<TKey, int>(capacity, comparer);", "mKeys = new TKey[capacity];");
			assertBefore(result.mGeneratedSource, "mKeys = new TKey[capacity];", "mValues = new RoleDataECSList(capacity);");
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
		private static void assertBefore(string source, string first, string second)
		{
			int firstIndex = source.IndexOf(first, StringComparison.Ordinal);
			if (firstIndex < 0)
			{
				throw new Exception("生成代码中没有找到:\n" + first);
			}
			int secondIndex = source.IndexOf(second, firstIndex + first.Length, StringComparison.Ordinal);
			if (secondIndex < 0)
			{
				throw new Exception("生成代码中没有在目标位置之后找到:\n" + second + "\nFirst:\n" + first);
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
		private static string getPlayerDictionaryEnumeratorSource(string source)
		{
			string normalized = normalizeLineEnding(source);
			int dictionaryIndex = normalized.IndexOf("sealed class RoleDataECSDictionary<TKey>", StringComparison.Ordinal);
			if (dictionaryIndex < 0)
			{
				throw new Exception("没有找到RoleDataECSDictionary<TKey>");
			}
			int playerIndex = normalized.IndexOf("#else", dictionaryIndex, StringComparison.Ordinal);
			if (playerIndex < 0)
			{
				throw new Exception("没有找到Dictionary Player分支");
			}
			int endIndex = normalized.IndexOf("#endif", playerIndex, StringComparison.Ordinal);
			if (endIndex < 0)
			{
				throw new Exception("没有找到Dictionary Player分支结束位置");
			}
			return normalized.Substring(playerIndex, endIndex - playerIndex);
		}
		private static int countOccurrences(string source, string value)
		{
			int count = 0;
			int index = 0;
			while (true)
			{
				index = source.IndexOf(value, index, StringComparison.Ordinal);
				if (index < 0)
				{
					return count;
				}
				++count;
				index += value.Length;
			}
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
