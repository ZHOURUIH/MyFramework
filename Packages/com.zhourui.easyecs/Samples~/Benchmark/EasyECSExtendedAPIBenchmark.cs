using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class EasyECSExtendedAPIBenchmark
{
	private const int LIST_ENTITY_COUNT = 20000;
	private const int RANGE_COUNT = 4096;
	private const int LIST_CAPACITY_BASE_COUNT = 4096;
	private const int LIST_CAPACITY_TARGET = 65536;
	private const int SEARCH_REPEAT_COUNT = 64;
	private const int BINARY_SEARCH_REPEAT_COUNT = 65536;
	private const int DICTIONARY_ENTITY_COUNT = 20000;
	private const int DICTIONARY_OPERATION_COUNT = 10000;
	private const int DICTIONARY_STRUCTURAL_COUNT = 4096;
	private const int DICTIONARY_CONTAINS_REPEAT_COUNT = 32;
	private const int DICTIONARY_CAPACITY_TARGET = 65536;
	private const int SAMPLE_COUNT = 9;
	private const int WARMUP_COUNT = 2;
	private const double MAX_ACCEPTABLE_SLOWDOWN = 1.05;
	private const double TINY_OPERATION_US = 0.05;
	private static double mResultSink;
	private static readonly object mSharedPayload = new object();
	private static readonly IComparer<RoleData> mRoleComparer = new RoleDataHPComparer();
	private static readonly IComparer<ManagedRoleDataStructuralBenchmarkData> mManagedComparer = new ManagedRoleDataHPComparer();
	private static readonly Predicate<RoleData> mRoleRemovePredicate = value => (value.mID & 3) == 0;
	private static readonly Predicate<RoleData> mRoleRemoveBlockPredicate = value => (value.mID & 15) < 8;
	private static readonly Predicate<RoleData> mRoleRemoveByHPPredicate = value => ((value.mHP - 100) & 3) == 0;
	private static readonly Predicate<RoleData> mRoleRemoveBlockByHPPredicate = value => ((value.mHP - 100) & 15) < 8;
	private static readonly Predicate<RoleData> mRoleFindByHPPredicate = value => value.mHP == 100 + LIST_ENTITY_COUNT - 1;
	private static readonly Predicate<ManagedRoleDataStructuralBenchmarkData> mManagedRemovePredicate = value => (value.mID & 3) == 0;
	private static readonly Predicate<ManagedRoleDataStructuralBenchmarkData> mManagedRemoveBlockPredicate = value => (value.mID & 15) < 8;
	private static readonly Predicate<ManagedRoleDataStructuralBenchmarkData> mManagedRemoveByHPPredicate = value => ((value.mHP - 100) & 3) == 0;
	private static readonly Predicate<ManagedRoleDataStructuralBenchmarkData> mManagedRemoveBlockByHPPredicate = value => ((value.mHP - 100) & 15) < 8;
	private static readonly Predicate<ManagedRoleDataStructuralBenchmarkData> mManagedFindByHPPredicate = value => value.mHP == 100 + LIST_ENTITY_COUNT - 1;
	private static readonly Predicate<int> mHPRemovePredicate = value => ((value - 100) & 3) == 0;
	private static readonly Predicate<int> mHPRemoveBlockPredicate = value => ((value - 100) & 15) < 8;
	private static readonly Predicate<int> mHPFindPredicate = value => value == 100 + LIST_ENTITY_COUNT - 1;
	private static RoleData[] mRoleRangeArray;
	private static List<RoleData> mRoleRangeList;
	private static RoleDataECSList mRoleRangeECSList;
	private static ManagedRoleDataStructuralBenchmarkData[] mManagedRangeArray;
	private static List<ManagedRoleDataStructuralBenchmarkData> mManagedRangeList;
	private static ManagedRoleDataStructuralBenchmarkDataECSList mManagedRangeECSList;
	private static List<RoleData> mRoleList;
	private static RoleDataECSList mRoleECSList;
	private static List<ManagedRoleDataStructuralBenchmarkData> mManagedList;
	private static ManagedRoleDataStructuralBenchmarkDataECSList mManagedECSList;
	private static RoleData[] mRoleCopyArray;
	private static ManagedRoleDataStructuralBenchmarkData[] mManagedCopyArray;
	private static int[] mManagedHPExportArray;
	private static string[] mManagedNameExportArray;
	private static object[] mManagedPayloadExportArray;
	private static Dictionary<int, RoleData> mDictionary;
	private static RoleDataECSDictionary<int> mECSDictionary;
	private static int[] mDictionaryKeys;
	private static readonly RoleData mDictionaryWriteValue = createRoleData(-1000000);
	private struct BenchmarkResult
	{
		public double mMedian;
		public double mMin;
		public double mMax;
		public double mUsPerOperation;
	}
	private sealed class RoleDataHPComparer : IComparer<RoleData>
	{
		public int Compare(RoleData left, RoleData right)
		{
			return left.mHP.CompareTo(right.mHP);
		}
	}
	private sealed class ManagedRoleDataHPComparer : IComparer<ManagedRoleDataStructuralBenchmarkData>
	{
		public int Compare(ManagedRoleDataStructuralBenchmarkData left, ManagedRoleDataStructuralBenchmarkData right)
		{
			return left.mHP.CompareTo(right.mHP);
		}
	}
	public static void runListBenchmark()
	{
		Debug.Log("\n================ ECSList Extended API Benchmark Start ================");
		Debug.Log("ListEntityCount:" + LIST_ENTITY_COUNT + ",RangeCount:" + RANGE_COUNT + ",SearchRepeatCount:" + SEARCH_REPEAT_COUNT + ",BinarySearchRepeatCount:" + BINARY_SEARCH_REPEAT_COUNT);
		prepareListSources();
		try
		{
			runUnmanagedListBenchmark();
			runManagedListBenchmark();
		}
		finally
		{
			cleanupListSources();
		}
		Debug.Log("ExtendedListResultSink:" + mResultSink);
		Debug.Log("================ ECSList Extended API Benchmark End ==================");
	}
	public static void runDictionaryBenchmark()
	{
		Debug.Log("\n================ ECSDictionary Extended API Benchmark Start ================");
		Debug.Log("DictionaryEntityCount:" + DICTIONARY_ENTITY_COUNT + ",OperationCount:" + DICTIONARY_OPERATION_COUNT + ",StructuralCount:" + DICTIONARY_STRUCTURAL_COUNT);
		prepareDictionaryKeys();
		runDictionaryExistingKeyBenchmark();
		runDictionaryStructuralBenchmark();
		mDictionaryKeys = null;
		Debug.Log("ExtendedDictionaryResultSink:" + mResultSink);
		Debug.Log("================ ECSDictionary Extended API Benchmark End ==================");
	}
	private static void runUnmanagedListBenchmark()
	{
		Debug.Log("\n================ Unmanaged ECSList Extended API ================");
		printListCompare("AddRange数组(无Resize)",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, RANGE_COUNT), runRoleListAddRangeArray, cleanupRoleList, 1),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, RANGE_COUNT), runRoleECSAddRangeArray, cleanupRoleECSList, 1));
		printListCompare("AddRange容器(无Resize)",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, RANGE_COUNT), runRoleListAddRangeContainer, cleanupRoleList, 1),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, RANGE_COUNT), runRoleECSAddRangeContainer, cleanupRoleECSList, 1));
		printListCompare("InsertRange数组中间(无Resize)",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, RANGE_COUNT), runRoleListInsertRangeArray, cleanupRoleList, 1),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, RANGE_COUNT), runRoleECSInsertRangeArray, cleanupRoleECSList, 1));
		printListCompare("InsertRange容器中间(无Resize)",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, RANGE_COUNT), runRoleListInsertRangeContainer, cleanupRoleList, 1),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, RANGE_COUNT), runRoleECSInsertRangeContainer, cleanupRoleECSList, 1));
		printListCompare("RemoveRange中间",
			measure(() => setupRoleList(LIST_ENTITY_COUNT + RANGE_COUNT, 0), runRoleListRemoveRange, cleanupRoleList, 1),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT + RANGE_COUNT, 0), runRoleECSRemoveRange, cleanupRoleECSList, 1));
		printListCompare("RemoveAll删除25%",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, 0), runRoleListRemoveAll, cleanupRoleList, 1),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, 0), runRoleECSRemoveAll, cleanupRoleECSList, 1));
		printListCompare("RemoveAll连续块删除50%",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, 0), runRoleListRemoveAllBlock, cleanupRoleList, 1),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, 0), runRoleECSRemoveAllBlock, cleanupRoleECSList, 1));
		printListCompare("RemoveAllByHP删除25%(ECS快速路径)",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, 0), runRoleListRemoveAllByHP, cleanupRoleList, 1),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, 0), runRoleECSRemoveAllByHP, cleanupRoleECSList, 1));
		printListCompare("RemoveAllByHP连续块删除50%(ECS快速路径)",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, 0), runRoleListRemoveAllBlockByHP, cleanupRoleList, 1),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, 0), runRoleECSRemoveAllBlockByHP, cleanupRoleECSList, 1));
		printListCompare("Reverse全量",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, 0), runRoleListReverse, cleanupRoleList, 1),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, 0), runRoleECSReverse, cleanupRoleECSList, 1));
		printListCompare("Sort全量",
			measure(() => setupRoleListDescending(LIST_ENTITY_COUNT), runRoleListSort, cleanupRoleList, 1),
			measure(() => setupRoleECSListDescending(LIST_ENTITY_COUNT), runRoleECSSort, cleanupRoleECSList, 1));
		printListCompare("SortByHP(ECS快速路径)",
			measure(() => setupRoleListDescending(LIST_ENTITY_COUNT), runRoleListSort, cleanupRoleList, 1),
			measure(() => setupRoleECSListDescending(LIST_ENTITY_COUNT), runRoleECSSortByHP, cleanupRoleECSList, 1));
		printListCompare("SortByHP重复Key(DirectSwap路径)",
			measure(() => setupRoleListDuplicateSort(LIST_ENTITY_COUNT), runRoleListSort, cleanupRoleList, 1),
			measure(() => setupRoleECSListDuplicateSort(LIST_ENTITY_COUNT), runRoleECSSortByHP, cleanupRoleECSList, 1));
		printListCompare("BinarySearch",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, 0), runRoleListBinarySearch, cleanupRoleList, BINARY_SEARCH_REPEAT_COUNT),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, 0), runRoleECSBinarySearch, cleanupRoleECSList, BINARY_SEARCH_REPEAT_COUNT));
		printListCompare("BinarySearchByHP(ECS快速路径)",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, 0), runRoleListBinarySearch, cleanupRoleList, BINARY_SEARCH_REPEAT_COUNT),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, 0), runRoleECSBinarySearchByHP, cleanupRoleECSList, BINARY_SEARCH_REPEAT_COUNT));
		printListCompare("Contains末尾命中",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, 0), runRoleListContains, cleanupRoleList, SEARCH_REPEAT_COUNT),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, 0), runRoleECSContains, cleanupRoleECSList, SEARCH_REPEAT_COUNT));
		printListCompare("IndexOf末尾命中",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, 0), runRoleListIndexOf, cleanupRoleList, SEARCH_REPEAT_COUNT),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, 0), runRoleECSIndexOf, cleanupRoleECSList, SEARCH_REPEAT_COUNT));
		printListCompare("ContainsByHP末尾命中(ECS快速路径)",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, 0), runRoleListContainsByHP, cleanupRoleList, SEARCH_REPEAT_COUNT),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, 0), runRoleECSContainsByHP, cleanupRoleECSList, SEARCH_REPEAT_COUNT));
		printListCompare("IndexOfByHP末尾命中(ECS快速路径)",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, 0), runRoleListIndexOfByHP, cleanupRoleList, SEARCH_REPEAT_COUNT),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, 0), runRoleECSIndexOfByHP, cleanupRoleECSList, SEARCH_REPEAT_COUNT));
		printListCompare("FindIndexByHP末尾命中(ECS快速路径)",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, 0), runRoleListFindIndexByHP, cleanupRoleList, SEARCH_REPEAT_COUNT),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, 0), runRoleECSFindIndexByHP, cleanupRoleECSList, SEARCH_REPEAT_COUNT));
		printListCompare("EnsureCapacity",
			measure(setupRoleListForCapacity, runRoleListEnsureCapacity, cleanupRoleList, 1),
			measure(setupRoleECSForCapacity, runRoleECSEnsureCapacity, cleanupRoleECSList, 1));
		printListCompare("TrimExcess",
			measure(setupRoleListForTrim, runRoleListTrimExcess, cleanupRoleList, 1),
			measure(setupRoleECSForTrim, runRoleECSTrimExcess, cleanupRoleECSList, 1));
		printListCompare("CopyTo全量",
			measure(setupRoleListCopyTo, runRoleListCopyTo, cleanupRoleListCopyTo, 1),
			measure(setupRoleECSCopyTo, runRoleECSCopyTo, cleanupRoleECSCopyTo, 1));
		printListCompare("ToArray全量",
			measure(() => setupRoleList(LIST_ENTITY_COUNT, 0), runRoleListToArray, cleanupRoleListCopyTo, 1),
			measure(() => setupRoleECSList(LIST_ENTITY_COUNT, 0), runRoleECSToArray, cleanupRoleECSCopyTo, 1));
	}
	private static void runManagedListBenchmark()
	{
		Debug.Log("\n================ Managed Hybrid ECSList Extended API ================");
		printListCompare("Managed AddRange数组(无Resize)",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, RANGE_COUNT), runManagedListAddRangeArray, cleanupManagedList, 1),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, RANGE_COUNT), runManagedECSAddRangeArray, cleanupManagedECSList, 1));
		printListCompare("Managed AddRange容器(无Resize)",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, RANGE_COUNT), runManagedListAddRangeContainer, cleanupManagedList, 1),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, RANGE_COUNT), runManagedECSAddRangeContainer, cleanupManagedECSList, 1));
		printListCompare("Managed InsertRange数组中间",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, RANGE_COUNT), runManagedListInsertRangeArray, cleanupManagedList, 1),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, RANGE_COUNT), runManagedECSInsertRangeArray, cleanupManagedECSList, 1));
		printListCompare("Managed InsertRange容器中间",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, RANGE_COUNT), runManagedListInsertRangeContainer, cleanupManagedList, 1),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, RANGE_COUNT), runManagedECSInsertRangeContainer, cleanupManagedECSList, 1));
		printListCompare("Managed RemoveRange中间",
			measure(() => setupManagedList(LIST_ENTITY_COUNT + RANGE_COUNT, 0), runManagedListRemoveRange, cleanupManagedList, 1),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT + RANGE_COUNT, 0), runManagedECSRemoveRange, cleanupManagedECSList, 1));
		printListCompare("Managed RemoveAll删除25%",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, 0), runManagedListRemoveAll, cleanupManagedList, 1),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, 0), runManagedECSRemoveAll, cleanupManagedECSList, 1));
		printListCompare("Managed RemoveAll连续块删除50%",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, 0), runManagedListRemoveAllBlock, cleanupManagedList, 1),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, 0), runManagedECSRemoveAllBlock, cleanupManagedECSList, 1));
		printListCompare("Managed RemoveAllByHP删除25%(ECS快速路径)",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, 0), runManagedListRemoveAllByHP, cleanupManagedList, 1),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, 0), runManagedECSRemoveAllByHP, cleanupManagedECSList, 1));
		printListCompare("Managed RemoveAllByHP连续块删除50%(ECS快速路径)",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, 0), runManagedListRemoveAllBlockByHP, cleanupManagedList, 1),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, 0), runManagedECSRemoveAllBlockByHP, cleanupManagedECSList, 1));
		printListCompare("Managed Reverse全量",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, 0), runManagedListReverse, cleanupManagedList, 1),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, 0), runManagedECSReverse, cleanupManagedECSList, 1));
		printListCompare("Managed Sort全量",
			measure(() => setupManagedListDescending(LIST_ENTITY_COUNT), runManagedListSort, cleanupManagedList, 1),
			measure(() => setupManagedECSListDescending(LIST_ENTITY_COUNT), runManagedECSSort, cleanupManagedECSList, 1));
		printListCompare("Managed SortByHP(ECS快速路径)",
			measure(() => setupManagedListDescending(LIST_ENTITY_COUNT), runManagedListSort, cleanupManagedList, 1),
			measure(() => setupManagedECSListDescending(LIST_ENTITY_COUNT), runManagedECSSortByHP, cleanupManagedECSList, 1));
		printListCompare("Managed SortByHP重复Key(Permutation路径)",
			measure(() => setupManagedListDuplicateSort(LIST_ENTITY_COUNT), runManagedListSort, cleanupManagedList, 1),
			measure(() => setupManagedECSListDuplicateSort(LIST_ENTITY_COUNT), runManagedECSSortByHP, cleanupManagedECSList, 1));
		printListCompare("Managed BinarySearch",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, 0), runManagedListBinarySearch, cleanupManagedList, BINARY_SEARCH_REPEAT_COUNT),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, 0), runManagedECSBinarySearch, cleanupManagedECSList, BINARY_SEARCH_REPEAT_COUNT));
		printListCompare("Managed BinarySearchByHP(ECS快速路径)",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, 0), runManagedListBinarySearch, cleanupManagedList, BINARY_SEARCH_REPEAT_COUNT),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, 0), runManagedECSBinarySearchByHP, cleanupManagedECSList, BINARY_SEARCH_REPEAT_COUNT));
		printListCompare("Managed Contains末尾命中",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, 0), runManagedListContains, cleanupManagedList, SEARCH_REPEAT_COUNT),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, 0), runManagedECSContains, cleanupManagedECSList, SEARCH_REPEAT_COUNT));
		printListCompare("Managed IndexOf末尾命中",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, 0), runManagedListIndexOf, cleanupManagedList, SEARCH_REPEAT_COUNT),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, 0), runManagedECSIndexOf, cleanupManagedECSList, SEARCH_REPEAT_COUNT));
		printListCompare("Managed ContainsByHP末尾命中(ECS快速路径)",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, 0), runManagedListContainsByHP, cleanupManagedList, SEARCH_REPEAT_COUNT),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, 0), runManagedECSContainsByHP, cleanupManagedECSList, SEARCH_REPEAT_COUNT));
		printListCompare("Managed IndexOfByHP末尾命中(ECS快速路径)",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, 0), runManagedListIndexOfByHP, cleanupManagedList, SEARCH_REPEAT_COUNT),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, 0), runManagedECSIndexOfByHP, cleanupManagedECSList, SEARCH_REPEAT_COUNT));
		printListCompare("Managed FindIndexByHP末尾命中(ECS快速路径)",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, 0), runManagedListFindIndexByHP, cleanupManagedList, SEARCH_REPEAT_COUNT),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, 0), runManagedECSFindIndexByHP, cleanupManagedECSList, SEARCH_REPEAT_COUNT));
		printListCompare("Managed EnsureCapacity",
			measure(setupManagedListForCapacity, runManagedListEnsureCapacity, cleanupManagedList, 1),
			measure(setupManagedECSForCapacity, runManagedECSEnsureCapacity, cleanupManagedECSList, 1));
		printListCompare("Managed TrimExcess",
			measure(setupManagedListForTrim, runManagedListTrimExcess, cleanupManagedList, 1),
			measure(setupManagedECSForTrim, runManagedECSTrimExcess, cleanupManagedECSList, 1));
		Debug.Log("\n================ Managed Export Path Breakdown ================");
		printListCompare("Managed Export仅HP->int[]",
			measure(setupManagedListExportColumns, runManagedListExportHPColumn, cleanupManagedListExportColumns, 1),
			measure(setupManagedECSExportColumns, runManagedECSExportHPColumn, cleanupManagedECSExportColumns, 1));
		printListCompare("Managed Export仅Name->string[]",
			measure(setupManagedListExportColumns, runManagedListExportNameColumn, cleanupManagedListExportColumns, 1),
			measure(setupManagedECSExportColumns, runManagedECSExportNameColumn, cleanupManagedECSExportColumns, 1));
		printListCompare("Managed Export仅Payload->object[]",
			measure(setupManagedListExportColumns, runManagedListExportPayloadColumn, cleanupManagedListExportColumns, 1),
			measure(setupManagedECSExportColumns, runManagedECSExportPayloadColumn, cleanupManagedECSExportColumns, 1));
		printListCompare("Managed Export三个ECS字段->独立数组",
			measure(setupManagedListExportColumns, runManagedListExportECSColumns, cleanupManagedListExportColumns, 1),
			measure(setupManagedECSExportColumns, runManagedECSExportECSColumns, cleanupManagedECSExportColumns, 1));
		printListCompare("Managed CopyTo全量",
			measure(setupManagedListCopyTo, runManagedListCopyTo, cleanupManagedListCopyTo, 1),
			measure(setupManagedECSCopyTo, runManagedECSCopyTo, cleanupManagedECSCopyTo, 1));
		printListCompare("Managed CopyTo局部4096",
			measure(setupManagedListCopyTo, runManagedListCopyToRange, cleanupManagedListCopyTo, 1),
			measure(setupManagedECSCopyTo, runManagedECSCopyToRange, cleanupManagedECSCopyTo, 1));
		printListCompare("Managed ToArray全量",
			measure(() => setupManagedList(LIST_ENTITY_COUNT, 0), runManagedListToArray, cleanupManagedListCopyTo, 1),
			measure(() => setupManagedECSList(LIST_ENTITY_COUNT, 0), runManagedECSToArray, cleanupManagedECSCopyTo, 1));
	}
	private static void runDictionaryExistingKeyBenchmark()
	{
		Debug.Log("\n================ ECSDictionary Existing-Key API ================");
		printDictionaryCompare("SetValue已有Key",
			measure(setupDictionary, runDictionarySetValue, cleanupDictionary, DICTIONARY_OPERATION_COUNT),
			measure(setupECSDictionary, runECSDictionarySetValue, cleanupECSDictionary, DICTIONARY_OPERATION_COUNT));
		printDictionaryCompare("TrySetValue已有Key",
			measure(setupDictionary, runDictionaryTrySetValue, cleanupDictionary, DICTIONARY_OPERATION_COUNT),
			measure(setupECSDictionary, runECSDictionaryTrySetValue, cleanupECSDictionary, DICTIONARY_OPERATION_COUNT));
		printDictionaryCompare("GetValueByHP已有Key(ECS字段快路径)",
			measure(setupDictionary, runDictionaryGetValueByHP, cleanupDictionary, DICTIONARY_OPERATION_COUNT),
			measure(setupECSDictionary, runECSDictionaryGetValueByHP, cleanupECSDictionary, DICTIONARY_OPERATION_COUNT));
		printDictionaryCompare("TryGetValueByHP已有Key(ECS字段快路径)",
			measure(setupDictionary, runDictionaryTryGetValueByHP, cleanupDictionary, DICTIONARY_OPERATION_COUNT),
			measure(setupECSDictionary, runECSDictionaryTryGetValueByHP, cleanupECSDictionary, DICTIONARY_OPERATION_COUNT));
		printDictionaryCompare("GetIndex已有Key+Direct读取HP(DenseIndex快路径)",
			measure(setupDictionary, runDictionaryGetIndexReadHP, cleanupDictionary, DICTIONARY_OPERATION_COUNT),
			measure(setupECSDictionary, runECSDictionaryGetIndexReadHP, cleanupECSDictionary, DICTIONARY_OPERATION_COUNT));
		printDictionaryCompare("GetOrAddIndex已有Key+Direct读取HP",
			measure(setupDictionary, runDictionaryGetOrAddIndexReadHP, cleanupDictionary, DICTIONARY_OPERATION_COUNT),
			measure(setupECSDictionary, runECSDictionaryGetOrAddIndexReadHP, cleanupECSDictionary, DICTIONARY_OPERATION_COUNT));
		printDictionaryCompare("SetValueByHP已有Key(ECS字段快路径)",
			measure(setupDictionary, runDictionarySetValueByHP, cleanupDictionary, DICTIONARY_OPERATION_COUNT),
			measure(setupECSDictionary, runECSDictionarySetValueByHP, cleanupECSDictionary, DICTIONARY_OPERATION_COUNT));
		printDictionaryCompare("TrySetValueByHP已有Key(ECS字段快路径)",
			measure(setupDictionary, runDictionaryTrySetValueByHP, cleanupDictionary, DICTIONARY_OPERATION_COUNT),
			measure(setupECSDictionary, runECSDictionaryTrySetValueByHP, cleanupECSDictionary, DICTIONARY_OPERATION_COUNT));
		printDictionaryCompare("修改4字段(ByXXX四次Key查找)",
			measure(setupDictionary, runDictionarySetFourFields, cleanupDictionary, DICTIONARY_OPERATION_COUNT),
			measure(setupECSDictionary, runECSDictionarySetFourFieldsByAPI, cleanupECSDictionary, DICTIONARY_OPERATION_COUNT));
		printDictionaryCompare("修改4字段(GetIndex一次+Direct)",
			measure(setupDictionary, runDictionarySetFourFields, cleanupDictionary, DICTIONARY_OPERATION_COUNT),
			measure(setupECSDictionary, runECSDictionarySetFourFieldsByIndex, cleanupECSDictionary, DICTIONARY_OPERATION_COUNT));
		printDictionaryCompare("SetOrAdd已有Key",
			measure(setupDictionary, runDictionarySetOrAddExisting, cleanupDictionary, DICTIONARY_OPERATION_COUNT),
			measure(setupECSDictionary, runECSDictionarySetOrAddExisting, cleanupECSDictionary, DICTIONARY_OPERATION_COUNT));
		printDictionaryCompare("GetOrAdd已有Key",
			measure(setupDictionary, runDictionaryGetOrAddExisting, cleanupDictionary, DICTIONARY_OPERATION_COUNT),
			measure(setupECSDictionary, runECSDictionaryGetOrAddExisting, cleanupECSDictionary, DICTIONARY_OPERATION_COUNT));
		printDictionaryCompare("ContainsValue未命中",
			measure(setupDictionary, runDictionaryContainsValueMissing, cleanupDictionary, DICTIONARY_CONTAINS_REPEAT_COUNT),
			measure(setupECSDictionary, runECSDictionaryContainsValueMissing, cleanupECSDictionary, DICTIONARY_CONTAINS_REPEAT_COUNT));
	}
	private static void runDictionaryStructuralBenchmark()
	{
		Debug.Log("\n================ ECSDictionary Structural Extended API ================");
		printDictionaryCompare("TryAdd新增Key(预留容量,单哈希路径)",
			measure(setupDictionaryForStructuralAdd, runDictionaryTryAddMissing, cleanupDictionary, DICTIONARY_STRUCTURAL_COUNT),
			measure(setupECSDictionaryForStructuralAdd, runECSDictionaryTryAddMissing, cleanupECSDictionary, DICTIONARY_STRUCTURAL_COUNT));
		printDictionaryCompare("SetOrAdd新增Key(预留容量)",
			measure(setupDictionaryForStructuralAdd, runDictionarySetOrAddMissing, cleanupDictionary, DICTIONARY_STRUCTURAL_COUNT),
			measure(setupECSDictionaryForStructuralAdd, runECSDictionarySetOrAddMissing, cleanupECSDictionary, DICTIONARY_STRUCTURAL_COUNT));
		printDictionaryCompare("GetOrAdd新增Key(预留容量)",
			measure(setupDictionaryForStructuralAdd, runDictionaryGetOrAddMissing, cleanupDictionary, DICTIONARY_STRUCTURAL_COUNT),
			measure(setupECSDictionaryForStructuralAdd, runECSDictionaryGetOrAddMissing, cleanupECSDictionary, DICTIONARY_STRUCTURAL_COUNT));
		printDictionaryCompare("GetOrAddIndex新增Key(预留容量)",
			measure(setupDictionaryForStructuralAdd, runDictionaryGetOrAddIndexMissing, cleanupDictionary, DICTIONARY_STRUCTURAL_COUNT),
			measure(setupECSDictionaryForStructuralAdd, runECSDictionaryGetOrAddIndexMissing, cleanupECSDictionary, DICTIONARY_STRUCTURAL_COUNT));
		printDictionaryCompare("Remove(key)",
			measure(setupDictionaryForRemove, runDictionaryRemove, cleanupDictionary, DICTIONARY_STRUCTURAL_COUNT),
			measure(setupECSDictionaryForRemove, runECSDictionaryRemove, cleanupECSDictionary, DICTIONARY_STRUCTURAL_COUNT));
		printDictionaryCompare("Remove(key,out value)",
			measure(setupDictionaryForRemove, runDictionaryRemoveOut, cleanupDictionary, DICTIONARY_STRUCTURAL_COUNT),
			measure(setupECSDictionaryForRemove, runECSDictionaryRemoveOut, cleanupECSDictionary, DICTIONARY_STRUCTURAL_COUNT));
		printDictionaryCompare("EnsureCapacity",
			measure(setupDictionaryForCapacity, runDictionaryEnsureCapacity, cleanupDictionary, 1),
			measure(setupECSDictionaryForCapacity, runECSDictionaryEnsureCapacity, cleanupECSDictionary, 1));
		printDictionaryCompare("TrimExcess",
			measure(setupDictionaryForTrim, runDictionaryTrimExcess, cleanupDictionary, 1),
			measure(setupECSDictionaryForTrim, runECSDictionaryTrimExcess, cleanupECSDictionary, 1));
	}
	private static void prepareListSources()
	{
		mRoleRangeArray = new RoleData[RANGE_COUNT];
		mManagedRangeArray = new ManagedRoleDataStructuralBenchmarkData[RANGE_COUNT];
		for (int i = 0; i < RANGE_COUNT; ++i)
		{
			mRoleRangeArray[i] = createRoleData(i + 100000);
			mManagedRangeArray[i] = createManagedData(i + 100000);
		}
		mRoleRangeList = new List<RoleData>(mRoleRangeArray);
		mManagedRangeList = new List<ManagedRoleDataStructuralBenchmarkData>(mManagedRangeArray);
		mRoleRangeECSList = new RoleDataECSList(RANGE_COUNT);
		mRoleRangeECSList.AddRange(mRoleRangeArray);
		mManagedRangeECSList = new ManagedRoleDataStructuralBenchmarkDataECSList(RANGE_COUNT);
		mManagedRangeECSList.AddRange(mManagedRangeArray);
	}
	private static void cleanupListSources()
	{
		if (mRoleRangeECSList != null)
		{
			mRoleRangeECSList.Dispose();
			mRoleRangeECSList = null;
		}
		if (mManagedRangeECSList != null)
		{
			mManagedRangeECSList.Dispose();
			mManagedRangeECSList = null;
		}
		mRoleRangeArray = null;
		mRoleRangeList = null;
		mManagedRangeArray = null;
		mManagedRangeList = null;
	}
	private static void prepareDictionaryKeys()
	{
		mDictionaryKeys = new int[DICTIONARY_OPERATION_COUNT];
		System.Random random = new System.Random(20260818);
		for (int i = 0; i < mDictionaryKeys.Length; ++i)
		{
			mDictionaryKeys[i] = random.Next(DICTIONARY_ENTITY_COUNT);
		}
	}
	private static void setupRoleList(int count, int extraCapacity)
	{
		mRoleList = new List<RoleData>(count + extraCapacity + 4);
		for (int i = 0; i < count; ++i)
		{
			mRoleList.Add(createRoleData(i));
		}
	}
	private static void setupRoleECSList(int count, int extraCapacity)
	{
		mRoleECSList = new RoleDataECSList(count + extraCapacity + 4);
		for (int i = 0; i < count; ++i)
		{
			mRoleECSList.Add(createRoleData(i));
		}
	}
	private static void setupManagedList(int count, int extraCapacity)
	{
		mManagedList = new List<ManagedRoleDataStructuralBenchmarkData>(count + extraCapacity + 4);
		for (int i = 0; i < count; ++i)
		{
			mManagedList.Add(createManagedData(i));
		}
	}
	private static void setupManagedECSList(int count, int extraCapacity)
	{
		mManagedECSList = new ManagedRoleDataStructuralBenchmarkDataECSList(count + extraCapacity + 4);
		for (int i = 0; i < count; ++i)
		{
			mManagedECSList.Add(createManagedData(i));
		}
	}
	private static void setupRoleListDescending(int count)
	{
		mRoleList = new List<RoleData>(count);
		for (int i = count - 1; i >= 0; --i)
		{
			mRoleList.Add(createRoleData(i));
		}
	}
	private static void setupRoleECSListDescending(int count)
	{
		mRoleECSList = new RoleDataECSList(count);
		for (int i = count - 1; i >= 0; --i)
		{
			mRoleECSList.Add(createRoleData(i));
		}
	}
	private static void setupManagedListDescending(int count)
	{
		mManagedList = new List<ManagedRoleDataStructuralBenchmarkData>(count);
		for (int i = count - 1; i >= 0; --i)
		{
			mManagedList.Add(createManagedData(i));
		}
	}
	private static void setupManagedECSListDescending(int count)
	{
		mManagedECSList = new ManagedRoleDataStructuralBenchmarkDataECSList(count);
		for (int i = count - 1; i >= 0; --i)
		{
			mManagedECSList.Add(createManagedData(i));
		}
	}
	private static void setupRoleListDuplicateSort(int count)
	{
		mRoleList = new List<RoleData>(count);
		for (int i = count - 1; i >= 0; --i)
		{
			RoleData value = createRoleData(i);
			value.mHP = 100 + i % 64;
			mRoleList.Add(value);
		}
	}
	private static void setupRoleECSListDuplicateSort(int count)
	{
		mRoleECSList = new RoleDataECSList(count);
		for (int i = count - 1; i >= 0; --i)
		{
			RoleData value = createRoleData(i);
			value.mHP = 100 + i % 64;
			mRoleECSList.Add(value);
		}
	}
	private static void setupManagedListDuplicateSort(int count)
	{
		mManagedList = new List<ManagedRoleDataStructuralBenchmarkData>(count);
		for (int i = count - 1; i >= 0; --i)
		{
			ManagedRoleDataStructuralBenchmarkData value = createManagedData(i);
			value.mHP = 100 + i % 64;
			mManagedList.Add(value);
		}
	}
	private static void setupManagedECSListDuplicateSort(int count)
	{
		mManagedECSList = new ManagedRoleDataStructuralBenchmarkDataECSList(count);
		for (int i = count - 1; i >= 0; --i)
		{
			ManagedRoleDataStructuralBenchmarkData value = createManagedData(i);
			value.mHP = 100 + i % 64;
			mManagedECSList.Add(value);
		}
	}
	private static void setupRoleListForCapacity()
	{
		mRoleList = new List<RoleData>(LIST_CAPACITY_BASE_COUNT);
		for (int i = 0; i < LIST_CAPACITY_BASE_COUNT; ++i)
		{
			mRoleList.Add(createRoleData(i));
		}
	}
	private static void setupRoleECSForCapacity()
	{
		mRoleECSList = new RoleDataECSList(LIST_CAPACITY_BASE_COUNT);
		for (int i = 0; i < LIST_CAPACITY_BASE_COUNT; ++i)
		{
			mRoleECSList.Add(createRoleData(i));
		}
	}
	private static void setupManagedListForCapacity()
	{
		mManagedList = new List<ManagedRoleDataStructuralBenchmarkData>(LIST_CAPACITY_BASE_COUNT);
		for (int i = 0; i < LIST_CAPACITY_BASE_COUNT; ++i)
		{
			mManagedList.Add(createManagedData(i));
		}
	}
	private static void setupManagedECSForCapacity()
	{
		mManagedECSList = new ManagedRoleDataStructuralBenchmarkDataECSList(LIST_CAPACITY_BASE_COUNT);
		for (int i = 0; i < LIST_CAPACITY_BASE_COUNT; ++i)
		{
			mManagedECSList.Add(createManagedData(i));
		}
	}
	private static void setupRoleListForTrim()
	{
		mRoleList = new List<RoleData>(LIST_CAPACITY_TARGET);
		for (int i = 0; i < LIST_CAPACITY_BASE_COUNT; ++i)
		{
			mRoleList.Add(createRoleData(i));
		}
	}
	private static void setupRoleECSForTrim()
	{
		mRoleECSList = new RoleDataECSList(LIST_CAPACITY_TARGET);
		for (int i = 0; i < LIST_CAPACITY_BASE_COUNT; ++i)
		{
			mRoleECSList.Add(createRoleData(i));
		}
	}
	private static void setupManagedListForTrim()
	{
		mManagedList = new List<ManagedRoleDataStructuralBenchmarkData>(LIST_CAPACITY_TARGET);
		for (int i = 0; i < LIST_CAPACITY_BASE_COUNT; ++i)
		{
			mManagedList.Add(createManagedData(i));
		}
	}
	private static void setupManagedECSForTrim()
	{
		mManagedECSList = new ManagedRoleDataStructuralBenchmarkDataECSList(LIST_CAPACITY_TARGET);
		for (int i = 0; i < LIST_CAPACITY_BASE_COUNT; ++i)
		{
			mManagedECSList.Add(createManagedData(i));
		}
	}
	private static void setupRoleListCopyTo()
	{
		setupRoleList(LIST_ENTITY_COUNT, 0);
		mRoleCopyArray = new RoleData[LIST_ENTITY_COUNT];
	}
	private static void setupRoleECSCopyTo()
	{
		setupRoleECSList(LIST_ENTITY_COUNT, 0);
		mRoleCopyArray = new RoleData[LIST_ENTITY_COUNT];
	}
	private static void setupManagedListCopyTo()
	{
		setupManagedList(LIST_ENTITY_COUNT, 0);
		mManagedCopyArray = new ManagedRoleDataStructuralBenchmarkData[LIST_ENTITY_COUNT];
	}
	private static void setupManagedECSCopyTo()
	{
		setupManagedECSList(LIST_ENTITY_COUNT, 0);
		mManagedCopyArray = new ManagedRoleDataStructuralBenchmarkData[LIST_ENTITY_COUNT];
	}
	private static void setupManagedListExportColumns()
	{
		setupManagedList(LIST_ENTITY_COUNT, 0);
		setupManagedExportArrays();
	}
	private static void setupManagedECSExportColumns()
	{
		setupManagedECSList(LIST_ENTITY_COUNT, 0);
		setupManagedExportArrays();
	}
	private static void setupManagedExportArrays()
	{
		mManagedHPExportArray = new int[LIST_ENTITY_COUNT];
		mManagedNameExportArray = new string[LIST_ENTITY_COUNT];
		mManagedPayloadExportArray = new object[LIST_ENTITY_COUNT];
	}
	private static void cleanupRoleList()
	{
		mRoleList = null;
	}
	private static void cleanupRoleECSList()
	{
		if (mRoleECSList != null)
		{
			mRoleECSList.Dispose();
			mRoleECSList = null;
		}
	}
	private static void cleanupManagedList()
	{
		mManagedList = null;
	}
	private static void cleanupManagedECSList()
	{
		if (mManagedECSList != null)
		{
			mManagedECSList.Dispose();
			mManagedECSList = null;
		}
	}
	private static void cleanupRoleListCopyTo()
	{
		mRoleCopyArray = null;
		cleanupRoleList();
	}
	private static void cleanupRoleECSCopyTo()
	{
		mRoleCopyArray = null;
		cleanupRoleECSList();
	}
	private static void cleanupManagedListCopyTo()
	{
		mManagedCopyArray = null;
		cleanupManagedList();
	}
	private static void cleanupManagedECSCopyTo()
	{
		mManagedCopyArray = null;
		cleanupManagedECSList();
	}
	private static void cleanupManagedListExportColumns()
	{
		cleanupManagedExportArrays();
		cleanupManagedList();
	}
	private static void cleanupManagedECSExportColumns()
	{
		cleanupManagedExportArrays();
		cleanupManagedECSList();
	}
	private static void cleanupManagedExportArrays()
	{
		mManagedHPExportArray = null;
		mManagedNameExportArray = null;
		mManagedPayloadExportArray = null;
	}
	private static void runRoleListAddRangeArray()
	{
		mRoleList.AddRange(mRoleRangeArray);
		mResultSink += mRoleList.Count + mRoleList[mRoleList.Count - 1].mHP;
	}
	private static void runRoleECSAddRangeArray()
	{
		mRoleECSList.AddRange(mRoleRangeArray);
		mResultSink += mRoleECSList.Count + mRoleECSList[mRoleECSList.Count - 1].mHP;
	}
	private static void runRoleListAddRangeContainer()
	{
		mRoleList.AddRange(mRoleRangeList);
		mResultSink += mRoleList.Count + mRoleList[mRoleList.Count - 1].mHP;
	}
	private static void runRoleECSAddRangeContainer()
	{
		mRoleECSList.AddRange(mRoleRangeECSList);
		mResultSink += mRoleECSList.Count + mRoleECSList[mRoleECSList.Count - 1].mHP;
	}
	private static void runRoleListInsertRangeArray()
	{
		mRoleList.InsertRange(mRoleList.Count >> 1, mRoleRangeArray);
		mResultSink += mRoleList.Count + mRoleList[mRoleList.Count >> 1].mHP;
	}
	private static void runRoleECSInsertRangeArray()
	{
		mRoleECSList.InsertRange(mRoleECSList.Count >> 1, mRoleRangeArray);
		mResultSink += mRoleECSList.Count + mRoleECSList[mRoleECSList.Count >> 1].mHP;
	}
	private static void runRoleListInsertRangeContainer()
	{
		mRoleList.InsertRange(mRoleList.Count >> 1, mRoleRangeList);
		mResultSink += mRoleList.Count + mRoleList[mRoleList.Count >> 1].mHP;
	}
	private static void runRoleECSInsertRangeContainer()
	{
		mRoleECSList.InsertRange(mRoleECSList.Count >> 1, mRoleRangeECSList);
		mResultSink += mRoleECSList.Count + mRoleECSList[mRoleECSList.Count >> 1].mHP;
	}
	private static void runRoleListRemoveRange()
	{
		mRoleList.RemoveRange((mRoleList.Count - RANGE_COUNT) >> 1, RANGE_COUNT);
		mResultSink += mRoleList.Count + mRoleList[mRoleList.Count >> 1].mHP;
	}
	private static void runRoleECSRemoveRange()
	{
		mRoleECSList.RemoveRange((mRoleECSList.Count - RANGE_COUNT) >> 1, RANGE_COUNT);
		mResultSink += mRoleECSList.Count + mRoleECSList[mRoleECSList.Count >> 1].mHP;
	}
	private static void runRoleListRemoveAll()
	{
		mResultSink += mRoleList.RemoveAll(mRoleRemovePredicate);
	}
	private static void runRoleECSRemoveAll()
	{
		mResultSink += mRoleECSList.RemoveAll(mRoleRemovePredicate);
	}
	private static void runRoleListRemoveAllBlock()
	{
		mResultSink += mRoleList.RemoveAll(mRoleRemoveBlockPredicate);
	}
	private static void runRoleECSRemoveAllBlock()
	{
		mResultSink += mRoleECSList.RemoveAll(mRoleRemoveBlockPredicate);
	}
	private static void runRoleListRemoveAllByHP()
	{
		mResultSink += mRoleList.RemoveAll(mRoleRemoveByHPPredicate);
	}
	private static void runRoleECSRemoveAllByHP()
	{
		mResultSink += mRoleECSList.RemoveAllByHP(mHPRemovePredicate);
	}
	private static void runRoleListRemoveAllBlockByHP()
	{
		mResultSink += mRoleList.RemoveAll(mRoleRemoveBlockByHPPredicate);
	}
	private static void runRoleECSRemoveAllBlockByHP()
	{
		mResultSink += mRoleECSList.RemoveAllByHP(mHPRemoveBlockPredicate);
	}
	private static void runRoleListReverse()
	{
		mRoleList.Reverse();
		mResultSink += mRoleList[0].mHP + mRoleList[mRoleList.Count - 1].mHP;
	}
	private static void runRoleECSReverse()
	{
		mRoleECSList.Reverse();
		mResultSink += mRoleECSList[0].mHP + mRoleECSList[mRoleECSList.Count - 1].mHP;
	}
	private static void runRoleListSort()
	{
		mRoleList.Sort(mRoleComparer);
		mResultSink += mRoleList[0].mHP + mRoleList[mRoleList.Count - 1].mHP;
	}
	private static void runRoleECSSort()
	{
		mRoleECSList.Sort(mRoleComparer);
		mResultSink += mRoleECSList[0].mHP + mRoleECSList[mRoleECSList.Count - 1].mHP;
	}
	private static void runRoleECSSortByHP()
	{
		mRoleECSList.SortByHP();
		mResultSink += mRoleECSList[0].mHP + mRoleECSList[mRoleECSList.Count - 1].mHP;
	}
	private static void runRoleListBinarySearch()
	{
		long sum = 0;
		for (int i = 0; i < BINARY_SEARCH_REPEAT_COUNT; ++i)
		{
			RoleData target = createRoleData(i % LIST_ENTITY_COUNT);
			sum += mRoleList.BinarySearch(target, mRoleComparer);
		}
		mResultSink += sum;
	}
	private static void runRoleECSBinarySearch()
	{
		long sum = 0;
		for (int i = 0; i < BINARY_SEARCH_REPEAT_COUNT; ++i)
		{
			RoleData target = createRoleData(i % LIST_ENTITY_COUNT);
			sum += mRoleECSList.BinarySearch(target, mRoleComparer);
		}
		mResultSink += sum;
	}
	private static void runRoleECSBinarySearchByHP()
	{
		long sum = 0;
		for (int i = 0; i < BINARY_SEARCH_REPEAT_COUNT; ++i)
		{
			sum += mRoleECSList.BinarySearchByHP(100 + i % LIST_ENTITY_COUNT);
		}
		mResultSink += sum;
	}
	private static void runRoleListContains()
	{
		RoleData target = createRoleData(LIST_ENTITY_COUNT - 1);
		int found = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			if (mRoleList.Contains(target))
			{
				++found;
			}
		}
		mResultSink += found;
	}
	private static void runRoleECSContains()
	{
		RoleData target = createRoleData(LIST_ENTITY_COUNT - 1);
		int found = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			if (mRoleECSList.Contains(target))
			{
				++found;
			}
		}
		mResultSink += found;
	}
	private static void runRoleListIndexOf()
	{
		RoleData target = createRoleData(LIST_ENTITY_COUNT - 1);
		long sum = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			sum += mRoleList.IndexOf(target);
		}
		mResultSink += sum;
	}
	private static void runRoleECSIndexOf()
	{
		RoleData target = createRoleData(LIST_ENTITY_COUNT - 1);
		long sum = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			sum += mRoleECSList.IndexOf(target);
		}
		mResultSink += sum;
	}
	private static void runRoleListContainsByHP()
	{
		int found = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			if (mRoleList.Exists(mRoleFindByHPPredicate))
			{
				++found;
			}
		}
		mResultSink += found;
	}
	private static void runRoleECSContainsByHP()
	{
		int target = 100 + LIST_ENTITY_COUNT - 1;
		int found = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			if (mRoleECSList.ContainsByHP(target))
			{
				++found;
			}
		}
		mResultSink += found;
	}
	private static void runRoleListIndexOfByHP()
	{
		long sum = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			sum += mRoleList.FindIndex(mRoleFindByHPPredicate);
		}
		mResultSink += sum;
	}
	private static void runRoleECSIndexOfByHP()
	{
		int target = 100 + LIST_ENTITY_COUNT - 1;
		long sum = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			sum += mRoleECSList.IndexOfByHP(target);
		}
		mResultSink += sum;
	}
	private static void runRoleListFindIndexByHP()
	{
		long sum = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			sum += mRoleList.FindIndex(mRoleFindByHPPredicate);
		}
		mResultSink += sum;
	}
	private static void runRoleECSFindIndexByHP()
	{
		long sum = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			sum += mRoleECSList.FindIndexByHP(mHPFindPredicate);
		}
		mResultSink += sum;
	}
	private static void runRoleListEnsureCapacity()
	{
		if (mRoleList.Capacity < LIST_CAPACITY_TARGET)
		{
			mRoleList.Capacity = LIST_CAPACITY_TARGET;
		}
		mResultSink += mRoleList.Capacity;
	}
	private static void runRoleECSEnsureCapacity()
	{
		mResultSink += mRoleECSList.EnsureCapacity(LIST_CAPACITY_TARGET);
	}
	private static void runRoleListTrimExcess()
	{
		mRoleList.TrimExcess();
		mResultSink += mRoleList.Count + mRoleList.Capacity;
	}
	private static void runRoleECSTrimExcess()
	{
		mRoleECSList.TrimExcess();
		mResultSink += mRoleECSList.Count + mRoleECSList.Capacity;
	}
	private static void runRoleListCopyTo()
	{
		mRoleList.CopyTo(mRoleCopyArray);
		mResultSink += mRoleCopyArray[0].mHP + mRoleCopyArray[mRoleCopyArray.Length - 1].mHP;
	}
	private static void runRoleECSCopyTo()
	{
		mRoleECSList.CopyTo(mRoleCopyArray);
		mResultSink += mRoleCopyArray[0].mHP + mRoleCopyArray[mRoleCopyArray.Length - 1].mHP;
	}
	private static void runRoleListToArray()
	{
		mRoleCopyArray = mRoleList.ToArray();
		mResultSink += mRoleCopyArray[0].mHP + mRoleCopyArray[mRoleCopyArray.Length - 1].mHP;
	}
	private static void runRoleECSToArray()
	{
		mRoleCopyArray = mRoleECSList.ToArray();
		mResultSink += mRoleCopyArray[0].mHP + mRoleCopyArray[mRoleCopyArray.Length - 1].mHP;
	}
	private static void runManagedListAddRangeArray()
	{
		mManagedList.AddRange(mManagedRangeArray);
		mResultSink += mManagedList.Count + mManagedList[mManagedList.Count - 1].mHP;
	}
	private static void runManagedECSAddRangeArray()
	{
		mManagedECSList.AddRange(mManagedRangeArray);
		mResultSink += mManagedECSList.Count + mManagedECSList[mManagedECSList.Count - 1].mHP;
	}
	private static void runManagedListAddRangeContainer()
	{
		mManagedList.AddRange(mManagedRangeList);
		mResultSink += mManagedList.Count + mManagedList[mManagedList.Count - 1].mHP;
	}
	private static void runManagedECSAddRangeContainer()
	{
		mManagedECSList.AddRange(mManagedRangeECSList);
		mResultSink += mManagedECSList.Count + mManagedECSList[mManagedECSList.Count - 1].mHP;
	}
	private static void runManagedListInsertRangeArray()
	{
		mManagedList.InsertRange(mManagedList.Count >> 1, mManagedRangeArray);
		mResultSink += mManagedList.Count + mManagedList[mManagedList.Count >> 1].mHP;
	}
	private static void runManagedECSInsertRangeArray()
	{
		mManagedECSList.InsertRange(mManagedECSList.Count >> 1, mManagedRangeArray);
		mResultSink += mManagedECSList.Count + mManagedECSList[mManagedECSList.Count >> 1].mHP;
	}
	private static void runManagedListInsertRangeContainer()
	{
		mManagedList.InsertRange(mManagedList.Count >> 1, mManagedRangeList);
		mResultSink += mManagedList.Count + mManagedList[mManagedList.Count >> 1].mHP;
	}
	private static void runManagedECSInsertRangeContainer()
	{
		mManagedECSList.InsertRange(mManagedECSList.Count >> 1, mManagedRangeECSList);
		mResultSink += mManagedECSList.Count + mManagedECSList[mManagedECSList.Count >> 1].mHP;
	}
	private static void runManagedListRemoveRange()
	{
		mManagedList.RemoveRange((mManagedList.Count - RANGE_COUNT) >> 1, RANGE_COUNT);
		mResultSink += mManagedList.Count + mManagedList[mManagedList.Count >> 1].mHP;
	}
	private static void runManagedECSRemoveRange()
	{
		mManagedECSList.RemoveRange((mManagedECSList.Count - RANGE_COUNT) >> 1, RANGE_COUNT);
		mResultSink += mManagedECSList.Count + mManagedECSList[mManagedECSList.Count >> 1].mHP;
	}
	private static void runManagedListRemoveAll()
	{
		mResultSink += mManagedList.RemoveAll(mManagedRemovePredicate);
	}
	private static void runManagedECSRemoveAll()
	{
		mResultSink += mManagedECSList.RemoveAll(mManagedRemovePredicate);
	}
	private static void runManagedListRemoveAllBlock()
	{
		mResultSink += mManagedList.RemoveAll(mManagedRemoveBlockPredicate);
	}
	private static void runManagedECSRemoveAllBlock()
	{
		mResultSink += mManagedECSList.RemoveAll(mManagedRemoveBlockPredicate);
	}
	private static void runManagedListRemoveAllByHP()
	{
		mResultSink += mManagedList.RemoveAll(mManagedRemoveByHPPredicate);
	}
	private static void runManagedECSRemoveAllByHP()
	{
		mResultSink += mManagedECSList.RemoveAllByHP(mHPRemovePredicate);
	}
	private static void runManagedListRemoveAllBlockByHP()
	{
		mResultSink += mManagedList.RemoveAll(mManagedRemoveBlockByHPPredicate);
	}
	private static void runManagedECSRemoveAllBlockByHP()
	{
		mResultSink += mManagedECSList.RemoveAllByHP(mHPRemoveBlockPredicate);
	}
	private static void runManagedListReverse()
	{
		mManagedList.Reverse();
		mResultSink += mManagedList[0].mHP + mManagedList[mManagedList.Count - 1].mHP;
	}
	private static void runManagedECSReverse()
	{
		mManagedECSList.Reverse();
		mResultSink += mManagedECSList[0].mHP + mManagedECSList[mManagedECSList.Count - 1].mHP;
	}
	private static void runManagedListSort()
	{
		mManagedList.Sort(mManagedComparer);
		mResultSink += mManagedList[0].mHP + mManagedList[mManagedList.Count - 1].mHP;
	}
	private static void runManagedECSSort()
	{
		mManagedECSList.Sort(mManagedComparer);
		mResultSink += mManagedECSList[0].mHP + mManagedECSList[mManagedECSList.Count - 1].mHP;
	}
	private static void runManagedECSSortByHP()
	{
		mManagedECSList.SortByHP();
		mResultSink += mManagedECSList[0].mHP + mManagedECSList[mManagedECSList.Count - 1].mHP;
	}
	private static void runManagedListBinarySearch()
	{
		long sum = 0;
		for (int i = 0; i < BINARY_SEARCH_REPEAT_COUNT; ++i)
		{
			ManagedRoleDataStructuralBenchmarkData target = createManagedData(i % LIST_ENTITY_COUNT);
			sum += mManagedList.BinarySearch(target, mManagedComparer);
		}
		mResultSink += sum;
	}
	private static void runManagedECSBinarySearch()
	{
		long sum = 0;
		for (int i = 0; i < BINARY_SEARCH_REPEAT_COUNT; ++i)
		{
			ManagedRoleDataStructuralBenchmarkData target = createManagedData(i % LIST_ENTITY_COUNT);
			sum += mManagedECSList.BinarySearch(target, mManagedComparer);
		}
		mResultSink += sum;
	}
	private static void runManagedECSBinarySearchByHP()
	{
		long sum = 0;
		for (int i = 0; i < BINARY_SEARCH_REPEAT_COUNT; ++i)
		{
			sum += mManagedECSList.BinarySearchByHP(100 + i % LIST_ENTITY_COUNT);
		}
		mResultSink += sum;
	}
	private static void runManagedListContains()
	{
		ManagedRoleDataStructuralBenchmarkData target = createManagedData(LIST_ENTITY_COUNT - 1);
		int found = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			if (mManagedList.Contains(target))
			{
				++found;
			}
		}
		mResultSink += found;
	}
	private static void runManagedECSContains()
	{
		ManagedRoleDataStructuralBenchmarkData target = createManagedData(LIST_ENTITY_COUNT - 1);
		int found = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			if (mManagedECSList.Contains(target))
			{
				++found;
			}
		}
		mResultSink += found;
	}
	private static void runManagedListIndexOf()
	{
		ManagedRoleDataStructuralBenchmarkData target = createManagedData(LIST_ENTITY_COUNT - 1);
		long sum = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			sum += mManagedList.IndexOf(target);
		}
		mResultSink += sum;
	}
	private static void runManagedECSIndexOf()
	{
		ManagedRoleDataStructuralBenchmarkData target = createManagedData(LIST_ENTITY_COUNT - 1);
		long sum = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			sum += mManagedECSList.IndexOf(target);
		}
		mResultSink += sum;
	}
	private static void runManagedListContainsByHP()
	{
		int found = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			if (mManagedList.Exists(mManagedFindByHPPredicate))
			{
				++found;
			}
		}
		mResultSink += found;
	}
	private static void runManagedECSContainsByHP()
	{
		int target = 100 + LIST_ENTITY_COUNT - 1;
		int found = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			if (mManagedECSList.ContainsByHP(target))
			{
				++found;
			}
		}
		mResultSink += found;
	}
	private static void runManagedListIndexOfByHP()
	{
		long sum = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			sum += mManagedList.FindIndex(mManagedFindByHPPredicate);
		}
		mResultSink += sum;
	}
	private static void runManagedECSIndexOfByHP()
	{
		int target = 100 + LIST_ENTITY_COUNT - 1;
		long sum = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			sum += mManagedECSList.IndexOfByHP(target);
		}
		mResultSink += sum;
	}
	private static void runManagedListFindIndexByHP()
	{
		long sum = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			sum += mManagedList.FindIndex(mManagedFindByHPPredicate);
		}
		mResultSink += sum;
	}
	private static void runManagedECSFindIndexByHP()
	{
		long sum = 0;
		for (int i = 0; i < SEARCH_REPEAT_COUNT; ++i)
		{
			sum += mManagedECSList.FindIndexByHP(mHPFindPredicate);
		}
		mResultSink += sum;
	}
	private static void runManagedListEnsureCapacity()
	{
		if (mManagedList.Capacity < LIST_CAPACITY_TARGET)
		{
			mManagedList.Capacity = LIST_CAPACITY_TARGET;
		}
		mResultSink += mManagedList.Capacity;
	}
	private static void runManagedECSEnsureCapacity()
	{
		mResultSink += mManagedECSList.EnsureCapacity(LIST_CAPACITY_TARGET);
	}
	private static void runManagedListTrimExcess()
	{
		mManagedList.TrimExcess();
		mResultSink += mManagedList.Count + mManagedList.Capacity;
	}
	private static void runManagedECSTrimExcess()
	{
		mManagedECSList.TrimExcess();
		mResultSink += mManagedECSList.Count + mManagedECSList.Capacity;
	}
	private static void runManagedListExportHPColumn()
	{
		for (int i = 0; i < LIST_ENTITY_COUNT; ++i)
		{
			mManagedHPExportArray[i] = mManagedList[i].mHP;
		}
		mResultSink += mManagedHPExportArray[0] + mManagedHPExportArray[LIST_ENTITY_COUNT - 1];
	}
	private static void runManagedECSExportHPColumn()
	{
		var hp = mManagedECSList.getHPColumn();
		for (int i = 0; i < LIST_ENTITY_COUNT; ++i)
		{
			mManagedHPExportArray[i] = hp[i];
		}
		mResultSink += mManagedHPExportArray[0] + mManagedHPExportArray[LIST_ENTITY_COUNT - 1];
	}
	private static void runManagedListExportNameColumn()
	{
		for (int i = 0; i < LIST_ENTITY_COUNT; ++i)
		{
			mManagedNameExportArray[i] = mManagedList[i].mName;
		}
		mResultSink += mManagedNameExportArray[0].Length + mManagedNameExportArray[LIST_ENTITY_COUNT - 1].Length;
	}
	private static void runManagedECSExportNameColumn()
	{
		var name = mManagedECSList.getNameColumn();
		for (int i = 0; i < LIST_ENTITY_COUNT; ++i)
		{
			mManagedNameExportArray[i] = name[i];
		}
		mResultSink += mManagedNameExportArray[0].Length + mManagedNameExportArray[LIST_ENTITY_COUNT - 1].Length;
	}
	private static void runManagedListExportPayloadColumn()
	{
		for (int i = 0; i < LIST_ENTITY_COUNT; ++i)
		{
			mManagedPayloadExportArray[i] = mManagedList[i].mPayload;
		}
		mResultSink += global::System.Object.ReferenceEquals(mManagedPayloadExportArray[LIST_ENTITY_COUNT - 1], mSharedPayload) ? 1.0 : 0.0;
	}
	private static void runManagedECSExportPayloadColumn()
	{
		var payload = mManagedECSList.getPayloadColumn();
		for (int i = 0; i < LIST_ENTITY_COUNT; ++i)
		{
			mManagedPayloadExportArray[i] = payload[i];
		}
		mResultSink += global::System.Object.ReferenceEquals(mManagedPayloadExportArray[LIST_ENTITY_COUNT - 1], mSharedPayload) ? 1.0 : 0.0;
	}
	private static void runManagedListExportECSColumns()
	{
		for (int i = 0; i < LIST_ENTITY_COUNT; ++i)
		{
			ManagedRoleDataStructuralBenchmarkData value = mManagedList[i];
			mManagedHPExportArray[i] = value.mHP;
			mManagedNameExportArray[i] = value.mName;
			mManagedPayloadExportArray[i] = value.mPayload;
		}
		mResultSink += mManagedHPExportArray[LIST_ENTITY_COUNT - 1] + mManagedNameExportArray[0].Length;
	}
	private static void runManagedECSExportECSColumns()
	{
		var hp = mManagedECSList.getHPColumn();
		var name = mManagedECSList.getNameColumn();
		var payload = mManagedECSList.getPayloadColumn();
		for (int i = 0; i < LIST_ENTITY_COUNT; ++i)
		{
			mManagedHPExportArray[i] = hp[i];
			mManagedNameExportArray[i] = name[i];
			mManagedPayloadExportArray[i] = payload[i];
		}
		mResultSink += mManagedHPExportArray[LIST_ENTITY_COUNT - 1] + mManagedNameExportArray[0].Length;
	}
	private static void runManagedListCopyTo()
	{
		mManagedList.CopyTo(mManagedCopyArray);
		mResultSink += mManagedCopyArray[0].mHP + mManagedCopyArray[mManagedCopyArray.Length - 1].mHP;
	}
	private static void runManagedECSCopyTo()
	{
		mManagedECSList.CopyTo(mManagedCopyArray);
		mResultSink += mManagedCopyArray[0].mHP + mManagedCopyArray[mManagedCopyArray.Length - 1].mHP;
	}
	private static void runManagedListCopyToRange()
	{
		mManagedList.CopyTo(0, mManagedCopyArray, 0, RANGE_COUNT);
		mResultSink += mManagedCopyArray[0].mHP + mManagedCopyArray[RANGE_COUNT - 1].mHP;
	}
	private static void runManagedECSCopyToRange()
	{
		mManagedECSList.CopyTo(0, mManagedCopyArray, 0, RANGE_COUNT);
		mResultSink += mManagedCopyArray[0].mHP + mManagedCopyArray[RANGE_COUNT - 1].mHP;
	}
	private static void runManagedListToArray()
	{
		mManagedCopyArray = mManagedList.ToArray();
		mResultSink += mManagedCopyArray[0].mHP + mManagedCopyArray[mManagedCopyArray.Length - 1].mHP;
	}
	private static void runManagedECSToArray()
	{
		mManagedCopyArray = mManagedECSList.ToArray();
		mResultSink += mManagedCopyArray[0].mHP + mManagedCopyArray[mManagedCopyArray.Length - 1].mHP;
	}
	private static void setupDictionary()
	{
		mDictionary = new Dictionary<int, RoleData>(DICTIONARY_ENTITY_COUNT);
		for (int i = 0; i < DICTIONARY_ENTITY_COUNT; ++i)
		{
			mDictionary.Add(i, createRoleData(i));
		}
	}
	private static void setupECSDictionary()
	{
		mECSDictionary = new RoleDataECSDictionary<int>(DICTIONARY_ENTITY_COUNT);
		for (int i = 0; i < DICTIONARY_ENTITY_COUNT; ++i)
		{
			mECSDictionary.Add(i, createRoleData(i));
		}
	}
	private static void setupDictionaryForStructuralAdd()
	{
		mDictionary = new Dictionary<int, RoleData>(DICTIONARY_ENTITY_COUNT + DICTIONARY_STRUCTURAL_COUNT);
		for (int i = 0; i < DICTIONARY_ENTITY_COUNT; ++i)
		{
			mDictionary.Add(i, createRoleData(i));
		}
	}
	private static void setupECSDictionaryForStructuralAdd()
	{
		mECSDictionary = new RoleDataECSDictionary<int>(DICTIONARY_ENTITY_COUNT + DICTIONARY_STRUCTURAL_COUNT);
		for (int i = 0; i < DICTIONARY_ENTITY_COUNT; ++i)
		{
			mECSDictionary.Add(i, createRoleData(i));
		}
	}
	private static void setupDictionaryForRemove()
	{
		mDictionary = new Dictionary<int, RoleData>(DICTIONARY_STRUCTURAL_COUNT + 4);
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			mDictionary.Add(i, createRoleData(i));
		}
	}
	private static void setupECSDictionaryForRemove()
	{
		mECSDictionary = new RoleDataECSDictionary<int>(DICTIONARY_STRUCTURAL_COUNT + 4);
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			mECSDictionary.Add(i, createRoleData(i));
		}
	}
	private static void setupDictionaryForCapacity()
	{
		mDictionary = new Dictionary<int, RoleData>(DICTIONARY_STRUCTURAL_COUNT);
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			mDictionary.Add(i, createRoleData(i));
		}
	}
	private static void setupECSDictionaryForCapacity()
	{
		mECSDictionary = new RoleDataECSDictionary<int>(DICTIONARY_STRUCTURAL_COUNT);
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			mECSDictionary.Add(i, createRoleData(i));
		}
	}
	private static void setupDictionaryForTrim()
	{
		mDictionary = new Dictionary<int, RoleData>(DICTIONARY_CAPACITY_TARGET);
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			mDictionary.Add(i, createRoleData(i));
		}
	}
	private static void setupECSDictionaryForTrim()
	{
		mECSDictionary = new RoleDataECSDictionary<int>(DICTIONARY_CAPACITY_TARGET);
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			mECSDictionary.Add(i, createRoleData(i));
		}
	}
	private static void cleanupDictionary()
	{
		mDictionary = null;
	}
	private static void cleanupECSDictionary()
	{
		if (mECSDictionary != null)
		{
			mECSDictionary.Dispose();
			mECSDictionary = null;
		}
	}
	private static void runDictionarySetValue()
	{
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			mDictionary[mDictionaryKeys[i]] = mDictionaryWriteValue;
		}
		mResultSink += mDictionary[mDictionaryKeys[DICTIONARY_OPERATION_COUNT - 1]].mHP;
	}
	private static void runECSDictionarySetValue()
	{
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			mECSDictionary.SetValue(mDictionaryKeys[i], mDictionaryWriteValue);
		}
		mResultSink += mECSDictionary[mDictionaryKeys[DICTIONARY_OPERATION_COUNT - 1]].mHP;
	}
	private static void runDictionaryTrySetValue()
	{
		int count = 0;
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			int key = mDictionaryKeys[i];
			if (mDictionary.TryGetValue(key, out RoleData value))
			{
				mDictionary[key] = mDictionaryWriteValue;
				++count;
			}
		}
		mResultSink += count;
	}
	private static void runECSDictionaryTrySetValue()
	{
		int count = 0;
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			if (mECSDictionary.TrySetValue(mDictionaryKeys[i], mDictionaryWriteValue))
			{
				++count;
			}
		}
		mResultSink += count;
	}
	private static void runDictionaryGetValueByHP()
	{
		long sum = 0;
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			sum += mDictionary[mDictionaryKeys[i]].mHP;
		}
		mResultSink += sum;
	}
	private static void runECSDictionaryGetValueByHP()
	{
		long sum = 0;
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			sum += mECSDictionary.GetValueByHP(mDictionaryKeys[i]);
		}
		mResultSink += sum;
	}
	private static void runDictionaryTryGetValueByHP()
	{
		long sum = 0;
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			if (mDictionary.TryGetValue(mDictionaryKeys[i], out RoleData value))
			{
				sum += value.mHP;
			}
		}
		mResultSink += sum;
	}
	private static void runECSDictionaryTryGetValueByHP()
	{
		long sum = 0;
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			if (mECSDictionary.TryGetValueByHP(mDictionaryKeys[i], out int hp))
			{
				sum += hp;
			}
		}
		mResultSink += sum;
	}
	private static void runDictionaryGetIndexReadHP()
	{
		long sum = 0;
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			sum += mDictionary[mDictionaryKeys[i]].mHP;
		}
		mResultSink += sum;
	}
	private static void runECSDictionaryGetIndexReadHP()
	{
		long sum = 0;
		var hp = mECSDictionary.getHPColumn();
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			int index = mECSDictionary.GetIndex(mDictionaryKeys[i]);
			sum += hp[index];
		}
		mResultSink += sum;
	}
	private static void runDictionaryGetOrAddIndexReadHP()
	{
		long sum = 0;
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			int key = mDictionaryKeys[i];
			if (!mDictionary.TryGetValue(key, out RoleData value))
			{
				value = mDictionaryWriteValue;
				mDictionary.Add(key, value);
			}
			sum += value.mHP;
		}
		mResultSink += sum;
	}
	private static void runECSDictionaryGetOrAddIndexReadHP()
	{
		long sum = 0;
		var hp = mECSDictionary.getHPColumn();
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			int index = mECSDictionary.GetOrAddIndex(mDictionaryKeys[i], mDictionaryWriteValue);
			sum += hp[index];
		}
		mResultSink += sum;
	}
	private static void runDictionarySetValueByHP()
	{
		int hp = mDictionaryWriteValue.mHP;
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			int key = mDictionaryKeys[i];
			RoleData value = mDictionary[key];
			value.mHP = hp;
			mDictionary[key] = value;
		}
		mResultSink += mDictionary[mDictionaryKeys[DICTIONARY_OPERATION_COUNT - 1]].mHP;
	}
	private static void runECSDictionarySetValueByHP()
	{
		int hp = mDictionaryWriteValue.mHP;
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			mECSDictionary.SetValueByHP(mDictionaryKeys[i], hp);
		}
		mResultSink += mECSDictionary.GetValueByHP(mDictionaryKeys[DICTIONARY_OPERATION_COUNT - 1]);
	}
	private static void runDictionaryTrySetValueByHP()
	{
		int count = 0;
		int hp = mDictionaryWriteValue.mHP;
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			int key = mDictionaryKeys[i];
			if (mDictionary.TryGetValue(key, out RoleData value))
			{
				value.mHP = hp;
				mDictionary[key] = value;
				++count;
			}
		}
		mResultSink += count;
	}
	private static void runECSDictionaryTrySetValueByHP()
	{
		int count = 0;
		int hp = mDictionaryWriteValue.mHP;
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			if (mECSDictionary.TrySetValueByHP(mDictionaryKeys[i], hp))
			{
				++count;
			}
		}
		mResultSink += count;
	}
	private static void runDictionarySetFourFields()
	{
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			int key = mDictionaryKeys[i];
			RoleData value = mDictionary[key];
			value.mHP = mDictionaryWriteValue.mHP;
			value.mSpeed = mDictionaryWriteValue.mSpeed;
			value.mPositionX = mDictionaryWriteValue.mPositionX;
			value.mPositionY = mDictionaryWriteValue.mPositionY;
			mDictionary[key] = value;
		}
		mResultSink += mDictionary[mDictionaryKeys[DICTIONARY_OPERATION_COUNT - 1]].mPositionY;
	}
	private static void runECSDictionarySetFourFieldsByAPI()
	{
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			int key = mDictionaryKeys[i];
			mECSDictionary.SetValueByHP(key, mDictionaryWriteValue.mHP);
			mECSDictionary.SetValueBySpeed(key, mDictionaryWriteValue.mSpeed);
			mECSDictionary.SetValueByPositionX(key, mDictionaryWriteValue.mPositionX);
			mECSDictionary.SetValueByPositionY(key, mDictionaryWriteValue.mPositionY);
		}
		mResultSink += mECSDictionary.GetValueByPositionY(mDictionaryKeys[DICTIONARY_OPERATION_COUNT - 1]);
	}
	private static void runECSDictionarySetFourFieldsByIndex()
	{
		var hp = mECSDictionary.getHPColumn();
		var speed = mECSDictionary.getSpeedColumn();
		var positionX = mECSDictionary.getPositionXColumn();
		var positionY = mECSDictionary.getPositionYColumn();
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			int index = mECSDictionary.GetIndex(mDictionaryKeys[i]);
			hp[index] = mDictionaryWriteValue.mHP;
			speed[index] = mDictionaryWriteValue.mSpeed;
			positionX[index] = mDictionaryWriteValue.mPositionX;
			positionY[index] = mDictionaryWriteValue.mPositionY;
		}
		mResultSink += positionY[mECSDictionary.GetIndex(mDictionaryKeys[DICTIONARY_OPERATION_COUNT - 1])];
	}
	private static void runDictionarySetOrAddExisting()
	{
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			mDictionary[mDictionaryKeys[i]] = mDictionaryWriteValue;
		}
		mResultSink += mDictionary.Count;
	}
	private static void runECSDictionarySetOrAddExisting()
	{
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			mECSDictionary.SetOrAdd(mDictionaryKeys[i], mDictionaryWriteValue);
		}
		mResultSink += mECSDictionary.Count;
	}
	private static void runDictionaryGetOrAddExisting()
	{
		long sum = 0;
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			int key = mDictionaryKeys[i];
			if (mDictionary.TryGetValue(key, out RoleData value))
			{
				sum += value.mHP;
			}
			else
			{
				mDictionary.Add(key, mDictionaryWriteValue);
				sum += mDictionaryWriteValue.mHP;
			}
		}
		mResultSink += sum;
	}
	private static void runECSDictionaryGetOrAddExisting()
	{
		long sum = 0;
		for (int i = 0; i < DICTIONARY_OPERATION_COUNT; ++i)
		{
			sum += mECSDictionary.GetOrAdd(mDictionaryKeys[i], mDictionaryWriteValue).mHP;
		}
		mResultSink += sum;
	}
	private static void runDictionaryContainsValueMissing()
	{
		RoleData missing = createRoleData(-2000000);
		int found = 0;
		for (int i = 0; i < DICTIONARY_CONTAINS_REPEAT_COUNT; ++i)
		{
			if (mDictionary.ContainsValue(missing))
			{
				++found;
			}
		}
		mResultSink += found;
	}
	private static void runECSDictionaryContainsValueMissing()
	{
		RoleData missing = createRoleData(-2000000);
		int found = 0;
		for (int i = 0; i < DICTIONARY_CONTAINS_REPEAT_COUNT; ++i)
		{
			if (mECSDictionary.ContainsValue(missing))
			{
				++found;
			}
		}
		mResultSink += found;
	}
	private static void runDictionaryTryAddMissing()
	{
		int count = 0;
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			if (mDictionary.TryAdd(DICTIONARY_ENTITY_COUNT + i, mDictionaryWriteValue))
			{
				++count;
			}
		}
		mResultSink += count + mDictionary.Count;
	}
	private static void runECSDictionaryTryAddMissing()
	{
		int count = 0;
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			if (mECSDictionary.TryAdd(DICTIONARY_ENTITY_COUNT + i, mDictionaryWriteValue))
			{
				++count;
			}
		}
		mResultSink += count + mECSDictionary.Count;
	}
	private static void runDictionarySetOrAddMissing()
	{
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			mDictionary[DICTIONARY_ENTITY_COUNT + i] = mDictionaryWriteValue;
		}
		mResultSink += mDictionary.Count;
	}
	private static void runECSDictionarySetOrAddMissing()
	{
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			mECSDictionary.SetOrAdd(DICTIONARY_ENTITY_COUNT + i, mDictionaryWriteValue);
		}
		mResultSink += mECSDictionary.Count;
	}
	private static void runDictionaryGetOrAddMissing()
	{
		long sum = 0;
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			int key = DICTIONARY_ENTITY_COUNT + i;
			if (!mDictionary.TryGetValue(key, out RoleData value))
			{
				value = mDictionaryWriteValue;
				mDictionary.Add(key, value);
			}
			sum += value.mHP;
		}
		mResultSink += sum;
	}
	private static void runECSDictionaryGetOrAddMissing()
	{
		long sum = 0;
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			sum += mECSDictionary.GetOrAdd(DICTIONARY_ENTITY_COUNT + i, mDictionaryWriteValue).mHP;
		}
		mResultSink += sum;
	}
	private static void runDictionaryGetOrAddIndexMissing()
	{
		long sum = 0;
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			int key = DICTIONARY_ENTITY_COUNT + i;
			if (!mDictionary.TryGetValue(key, out RoleData value))
			{
				value = mDictionaryWriteValue;
				mDictionary.Add(key, value);
			}
			sum += value.mHP;
		}
		mResultSink += sum;
	}
	private static void runECSDictionaryGetOrAddIndexMissing()
	{
		long sum = 0;
		var hp = mECSDictionary.getHPColumn();
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			int key = DICTIONARY_ENTITY_COUNT + i;
			int index = mECSDictionary.GetOrAddIndex(key, mDictionaryWriteValue);
			sum += hp[index];
		}
		mResultSink += sum;
	}
	private static void runDictionaryRemove()
	{
		int count = 0;
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			if (mDictionary.Remove(i))
			{
				++count;
			}
		}
		mResultSink += count + mDictionary.Count;
	}
	private static void runECSDictionaryRemove()
	{
		int count = 0;
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			if (mECSDictionary.Remove(i))
			{
				++count;
			}
		}
		mResultSink += count + mECSDictionary.Count;
	}
	private static void runDictionaryRemoveOut()
	{
		long sum = 0;
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			if (mDictionary.Remove(i, out RoleData value))
			{
				sum += value.mHP;
			}
		}
		mResultSink += sum + mDictionary.Count;
	}
	private static void runECSDictionaryRemoveOut()
	{
		long sum = 0;
		for (int i = 0; i < DICTIONARY_STRUCTURAL_COUNT; ++i)
		{
			if (mECSDictionary.Remove(i, out RoleData value))
			{
				sum += value.mHP;
			}
		}
		mResultSink += sum + mECSDictionary.Count;
	}
	private static void runDictionaryEnsureCapacity()
	{
		mResultSink += mDictionary.EnsureCapacity(DICTIONARY_CAPACITY_TARGET);
	}
	private static void runECSDictionaryEnsureCapacity()
	{
		mResultSink += mECSDictionary.EnsureCapacity(DICTIONARY_CAPACITY_TARGET);
	}
	private static void runDictionaryTrimExcess()
	{
		mDictionary.TrimExcess();
		mResultSink += mDictionary.Count;
	}
	private static void runECSDictionaryTrimExcess()
	{
		mECSDictionary.TrimExcess();
		mResultSink += mECSDictionary.Count + mECSDictionary.Capacity;
	}
	private static BenchmarkResult measure(Action setup, Action action, Action cleanup, int operationCount)
	{
		for (int i = 0; i < WARMUP_COUNT; ++i)
		{
			setup();
			try
			{
				action();
			}
			finally
			{
				cleanup();
			}
		}
		double[] samples = new double[SAMPLE_COUNT];
		for (int i = 0; i < SAMPLE_COUNT; ++i)
		{
			setup();
			try
			{
				long start = Stopwatch.GetTimestamp();
				action();
				long end = Stopwatch.GetTimestamp();
				samples[i] = (end - start) * 1000.0 / Stopwatch.Frequency;
			}
			finally
			{
				cleanup();
			}
		}
		Array.Sort(samples);
		double median = samples[SAMPLE_COUNT / 2];
		return new BenchmarkResult
		{
			mMedian = median,
			mMin = samples[0],
			mMax = samples[SAMPLE_COUNT - 1],
			mUsPerOperation = median * 1000.0 / operationCount,
		};
	}
	private static void printListCompare(string title, BenchmarkResult standard, BenchmarkResult ecs)
	{
		bool tinyOperation = standard.mUsPerOperation < TINY_OPERATION_US;
		double slowdown = standard.mMedian > 0.0 ? ecs.mMedian / standard.mMedian : 0.0;
		string gate = tinyOperation ? "SKIP(TinyOperation)" : (slowdown <= MAX_ACCEPTABLE_SLOWDOWN ? "PASS" : "FAIL");
		Debug.Log(
			"\n================ " + title + " ================\n" +
			format("List<T>", standard) + "\n" +
			format("ECSList", ecs) + "\n" +
			"--------------------------------------------------\n" +
			"List / ECS : " + ratio(standard.mMedian, ecs.mMedian) + "x\n" +
			"ECS/List    : " + slowdown.ToString("0.000") + "x\n" +
			"5% Gate     : " + gate + "\n" +
			"==================================================");
	}
	private static void printDictionaryCompare(string title, BenchmarkResult standard, BenchmarkResult ecs)
	{
		Debug.Log(
			"\n================ " + title + " ================\n" +
			format("Dictionary", standard) + "\n" +
			format("ECSDictionary", ecs) + "\n" +
			"--------------------------------------------------\n" +
			"Dictionary / ECS : " + ratio(standard.mMedian, ecs.mMedian) + "x\n" +
			"ECS / Dictionary : " + ratio(ecs.mMedian, standard.mMedian) + "x\n" +
			"==================================================");
	}
	private static string format(string name, BenchmarkResult result)
	{
		return name.PadRight(28) +
			"Median:" + result.mMedian.ToString("0.000").PadLeft(9) + " ms | " +
			"Min:" + result.mMin.ToString("0.000").PadLeft(8) + " | " +
			"Max:" + result.mMax.ToString("0.000").PadLeft(8) + " | " +
			result.mUsPerOperation.ToString("0.000").PadLeft(9) + " us/op";
	}
	private static string ratio(double left, double right)
	{
		if (right <= 0.0)
		{
			return "N/A";
		}
		return (left / right).ToString("0.00");
	}
	private static RoleData createRoleData(int id)
	{
		return new RoleData
		{
			mHP = 100 + id,
			mSpeed = id * 0.1f,
			mPositionX = id * 2.0f,
			mPositionY = id * 3.0f,
			mID = id,
			mCamp = id & 3,
		};
	}
	private static ManagedRoleDataStructuralBenchmarkData createManagedData(int id)
	{
		return new ManagedRoleDataStructuralBenchmarkData
		{
			mHP = 100 + id,
			mName = "SharedName",
			mPayload = mSharedPayload,
			mID = id,
			mPath = "Shared/Path",
		};
	}
}
