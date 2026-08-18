using System;
using System.Collections.Generic;
using EasyECS;
using UnityEngine;

[ECS]
public struct EasyECSRuntimeTestData
{
	public int mHP;
	public float mSpeed;
	public float mPositionX;
	public float mPositionY;
	[NotECS] public int mID;
	[NotECS] public int mCamp;
}

[ECS]
public struct EasyECSManagedRuntimeTestData
{
	public int mHP;
	public string mName;
	public object mPayload;
	[NotECS] public int mID;
	[NotECS] public string mModelPath;
}

[ECS]
public struct EasyECSAllManagedRuntimeTestData
{
	public string mName;
	public object mPayload;
	[NotECS] public string mPath;
}

public sealed class EasyECSRuntimeUnitTest : MonoBehaviour
{
	private int mPassCount;
	private int mFailCount;
	private void Awake()
	{
		runAllTests();
	}
	[ContextMenu("Run EasyECS Runtime Unit Tests")]
	public void runAllTests()
	{
		mPassCount = 0;
		mFailCount = 0;
		Debug.Log("================ EasyECS Runtime Unit Test Start ================");
		Debug.Log("Backend:" + EasyECSRuntimeTestDataECSList.BackendName + ",Reason:" + EasyECSRuntimeTestDataECSList.BackendReason);
		Debug.Log("Managed Backend:" + EasyECSManagedRuntimeTestDataECSList.BackendName + ",Reason:" + EasyECSManagedRuntimeTestDataECSList.BackendReason);
		Debug.Log("AllManaged Backend:" + EasyECSAllManagedRuntimeTestDataECSList.BackendName + ",Reason:" + EasyECSAllManagedRuntimeTestDataECSList.BackendReason);
		runTest("Backend选择检查", testBackendSelection);
		runTest("List构造/Count/Capacity", testListConstructor);
		runTest("List Add/Get/Resize/全部字段", testListAddGetResize);
		runTest("List Set", testListSet);
		runTest("List indexer直接修改", testListIndexerMutation);
		runTest("List LocalRef修改", testListLocalRefMutation);
		runTest("List Resize后Ref保持有效", testListRefAfterResize);
		runTest("List Direct Column读写", testListDirectColumn);
		runTest("List Set不使Column失效", testListSetKeepsColumnValid);
		runTest("List RemoveAtSwapBack", testListRemoveAtSwapBack);
		runTest("List RemoveAtSwapBack删除最后元素", testListRemoveLast);
		runTest("List Insert保持顺序", testListInsertOrder);
		runTest("List Insert触发Resize", testListInsertResize);
		runTest("List RemoveAt保持顺序", testListRemoveAtOrder);
		runTest("List Insert/RemoveAt与System.List一致", testListInsertRemoveAtCompatibility);
		runTest("List 大块Insert/RemoveAt移动", testListLargeStructuralMove);
		runTest("List Insert/RemoveAt边界检查", testListInsertRemoveAtBounds);
		runTest("List Clear后重新使用", testListClearReuse);
		runTest("List 空Clear", testListEmptyClear);
		runTest("List AddRange数组/ECSList/自身", testListAddRange);
		runTest("List InsertRange数组/ECSList/自身", testListInsertRange);
		runTest("List RemoveRange", testListRemoveRange);
		runTest("List Contains/IndexOf/RemoveAll", testListSearchAndRemoveAll);
		runTest("List Reverse/Sort/BinarySearch", testListReverseSortBinarySearch);
		runTest("List SortBy自适应策略/Range/重复Key", testListSortByPermutation);
		runTest("List ByColumn Search/RemoveAll", testListByColumnSearchRemoveAll);
		runTest("List CopyTo/ToArray/Find/Capacity", testListCopyFindCapacity);
		runTest("List 重复Dispose", testListDoubleDispose);
		runTest("Managed字段 Add/Get/Set/Clear/Remove", testManagedFields);
		runTest("Managed List Add/Get/Resize/全部字段", testManagedListAddGetResize);
		runTest("Managed List Resize后Ref保持有效", testManagedListRefAfterResize);
		runTest("Managed List Direct Column", testManagedListDirectColumn);
		runTest("Managed List RemoveAtSwapBack", testManagedListRemoveAtSwapBack);
		runTest("Managed List Insert/RemoveAt", testManagedListInsertRemoveAt);
		runTest("Managed List 大块Insert/RemoveAt移动", testManagedListLargeStructuralMove);
		runTest("AllManaged List Insert/RemoveAt", testAllManagedListInsertRemoveAt);
		runTest("Managed List null字段", testManagedListNullFields);
		runTest("Managed List Range/Sort", testManagedListRangeSort);
		runTest("Managed List 数组批量转换/局部区间", testManagedListArrayConversion);
		runTest("Dictionary构造/Count/Capacity/Comparer", testDictionaryConstructor);
		runTest("Dictionary Add/Indexer/Resize", testDictionaryAddIndexerResize);
		runTest("Dictionary Add重复Key抛异常", testDictionaryDuplicateAdd);
		runTest("Dictionary Indexer不存在Key抛异常", testDictionaryMissingIndexer);
		runTest("Dictionary TryAdd与重复Key", testDictionaryTryAdd);
		runTest("Dictionary ContainsKey", testDictionaryContainsKey);
		runTest("Dictionary TryGetValue与Ref修改", testDictionaryTryGetValue);
		runTest("Dictionary TryGetIndex", testDictionaryTryGetIndex);
		runTest("Dictionary getKeyAt/getValueAt", testDictionaryDenseAccess);
		runTest("Dictionary Remove与SwapBack映射", testDictionaryRemoveSwapBack);
		runTest("Dictionary Remove最后元素", testDictionaryRemoveLast);
		runTest("Dictionary Remove不存在Key", testDictionaryRemoveMissing);
		runTest("Dictionary foreach Key+Value读取", testDictionaryForeachRead);
		runTest("Dictionary foreach Value修改", testDictionaryForeachWrite);
		runTest("Dictionary foreach Keys", testDictionaryKeys);
		runTest("Dictionary foreach Values", testDictionaryValues);
		runTest("Dictionary 手动Enumerator", testDictionaryManualEnumerator);
		runTest("Dictionary Direct Column", testDictionaryDirectColumn);
		runTest("Dictionary TryGetIndex+Direct Column", testDictionaryTryGetIndexDirectColumn);
		runTest("Dictionary GetIndex/GetOrAddIndex+Direct Column", testDictionaryDenseIndexFastPath);
		runTest("Dictionary Clear后重新使用", testDictionaryClearReuse);
		runTest("Dictionary 自定义Comparer", testDictionaryComparer);
		runTest("Dictionary Set/TrySet/SetOrAdd/GetOrAdd", testDictionarySetAndGetOrAdd);
		runTest("Dictionary 字段级Get/Set快速路径", testDictionaryFieldFastPath);
		runTest("Dictionary ContainsValue/Remove out", testDictionaryContainsValueRemoveOut);
		runTest("Dictionary EnsureCapacity/TrimExcess", testDictionaryCapacityMethods);
		runTest("Dictionary 重复Dispose", testDictionaryDoubleDispose);
		runTest("Managed Dictionary Add/Indexer/Resize", testManagedDictionaryAddIndexerResize);
		runTest("Managed Dictionary TryGetValue与Ref修改", testManagedDictionaryTryGetValue);
		runTest("Managed Dictionary foreach Key+Value", testManagedDictionaryForeach);
		runTest("Managed Dictionary foreach Values修改", testManagedDictionaryValues);
		runTest("Managed Dictionary Remove与SwapBack", testManagedDictionaryRemoveSwapBack);
		runTest("Managed Dictionary Direct Column", testManagedDictionaryDirectColumn);
		runTest("Managed Dictionary Clear后重新使用", testManagedDictionaryClearReuse);
		runTest("Managed Dictionary null字段", testManagedDictionaryNullFields);
#if UNITY_EDITOR
		runTest("Editor List索引越界", testEditorListIndexOutOfRange);
		runTest("Editor List Dispose后访问检测", testEditorListDisposedAccess);
		runTest("Editor List Clear后Ref失效", testEditorListRefAfterClear);
		runTest("Editor List Remove后Ref失效", testEditorListRemovedRef);
		runTest("Editor List SwapBack移动Ref失效", testEditorListMovedLastRef);
		runTest("Editor List Remove无关Ref保持有效", testEditorListUnrelatedRef);
		runTest("Editor List Insert影响区间Ref失效", testEditorListInsertAffectedRefs);
		runTest("Editor List Insert前方Ref保持有效", testEditorListInsertEarlierRef);
		runTest("Editor List Insert末尾旧Ref保持有效", testEditorListInsertAtEndRefs);
		runTest("Editor List RemoveAt影响区间Ref失效", testEditorListRemoveAtAffectedRefs);
		runTest("Editor List RemoveAt前方Ref保持有效", testEditorListRemoveAtEarlierRef);
		runTest("Editor List RemoveAt旧Ref不会复活", testEditorListRemoveAtRefDoesNotRevive);
		runTest("Editor List InsertRange/RemoveRange Ref失效", testEditorListRangeRefInvalidation);
		runTest("Editor List Sort/Reverse Ref失效", testEditorListSortReverseRefInvalidation);
		runTest("Editor List Resize后Ref保持有效", testEditorListRefAfterResize);
		runTest("Editor List Add后Column失效", testEditorListColumnAfterAdd);
		runTest("Editor List Insert后Column失效", testEditorListColumnAfterInsert);
		runTest("Editor List Remove后Column失效", testEditorListColumnAfterRemove);
		runTest("Editor List RemoveAt后Column失效", testEditorListColumnAfterOrderedRemove);
		runTest("Editor List Clear后Column失效", testEditorListColumnAfterClear);
		runTest("Editor List Dispose后Column失效", testEditorListColumnAfterDispose);
		runTest("Editor List Column越界", testEditorListColumnOutOfRange);
		runTest("Editor List Set后Column保持有效", testEditorListColumnAfterSet);
		runTest("Editor Dictionary Current在MoveNext前无效", testEditorDictionaryCurrentBeforeMoveNext);
		runTest("Editor Dictionary Current在遍历结束后无效", testEditorDictionaryCurrentAfterEnd);
		runTest("Editor Dictionary Add使Enumerator失效", testEditorDictionaryEnumeratorAfterAdd);
		runTest("Editor Dictionary成功TryAdd使Enumerator失效", testEditorDictionaryEnumeratorAfterSuccessfulTryAdd);
		runTest("Editor Dictionary重复TryAdd不使Enumerator失效", testEditorDictionaryEnumeratorAfterFailedTryAdd);
		runTest("Editor Dictionary Remove使Enumerator失效", testEditorDictionaryEnumeratorAfterRemove);
		runTest("Editor Dictionary删除不存在Key不使Enumerator失效", testEditorDictionaryEnumeratorAfterMissingRemove);
		runTest("Editor Dictionary Clear使Enumerator失效", testEditorDictionaryEnumeratorAfterClear);
		runTest("Editor Dictionary Dispose使Enumerator失效", testEditorDictionaryEnumeratorAfterDispose);
		runTest("Editor Dictionary Value修改不使Enumerator失效", testEditorDictionaryEnumeratorAfterValueMutation);
		runTest("Editor Dictionary获取Column不使Enumerator失效", testEditorDictionaryEnumeratorAfterGetColumn);
		runTest("Editor Dictionary Entry结构变化检测", testEditorDictionaryEntryAfterStructuralChange);
		runTest("Editor Dictionary Add后Column失效", testEditorDictionaryColumnAfterAdd);
		runTest("Editor Dictionary Remove后Column失效", testEditorDictionaryColumnAfterRemove);
		runTest("Editor Dictionary Clear后Column失效", testEditorDictionaryColumnAfterClear);
		runTest("Editor Dictionary Dispose后Column失效", testEditorDictionaryColumnAfterDispose);
		runTest("Editor Dictionary Keys Enumerator结构变化检测", testEditorDictionaryKeyEnumeratorStructuralChange);
		runTest("Editor Dictionary Keys Current边界检测", testEditorDictionaryKeyEnumeratorCurrentBoundary);
		runTest("Editor Dictionary Values Enumerator结构变化检测", testEditorDictionaryValueEnumeratorStructuralChange);
		runTest("Editor Dictionary Values Current边界检测", testEditorDictionaryValueEnumeratorCurrentBoundary);
		runTest("Editor Dictionary Dense索引越界", testEditorDictionaryDenseIndexOutOfRange);
		runTest("Editor Dictionary Dispose后访问检测", testEditorDictionaryDisposedAccess);
#endif
		Debug.Log("---------------------------------------------------------");
		Debug.Log("Total:" + (mPassCount + mFailCount) + ",Pass:" + mPassCount + ",Fail:" + mFailCount);
		if (mFailCount != 0)
		{
			Debug.LogError("================ EasyECS Runtime Unit Test Failed ================");
			throw new Exception("EasyECS Runtime Unit Test failed,FailCount:" + mFailCount);
		}
		Debug.Log("================ EasyECS Runtime Unit Test Pass =================");
	}
	private void runTest(string name, Action action)
	{
		try
		{
			action();
			++mPassCount;
			Debug.Log("[PASS] " + name);
		}
		catch (Exception exception)
		{
			++mFailCount;
			Debug.LogError("[FAIL] " + name + "\n" + exception);
		}
	}
	private static EasyECSRuntimeTestData createData(int id, int hp = 100, float speed = 2.0f, int camp = 1)
	{
		return new EasyECSRuntimeTestData
		{
			mHP = hp,
			mSpeed = speed,
			mPositionX = id * 10.0f,
			mPositionY = id * -5.0f,
			mID = id,
			mCamp = camp,
		};
	}
	private static EasyECSManagedRuntimeTestData createManagedData(int id, int hp = 100, string name = null, object payload = null, string modelPath = null)
	{
		return new EasyECSManagedRuntimeTestData
		{
			mHP = hp,
			mName = name ?? "Role" + id,
			mPayload = payload,
			mID = id,
			mModelPath = modelPath ?? "Model/" + id,
		};
	}
	private static void assertSame(object expected, object actual, string message)
	{
		if (!ReferenceEquals(expected, actual))
		{
			throw new Exception(message);
		}
	}
	private static void assertManagedDataEqual(EasyECSManagedRuntimeTestData expected, EasyECSManagedRuntimeTestData actual, string message)
	{
		assertEqual(expected.mHP, actual.mHP, message + ",mHP");
		assertEqual(expected.mName, actual.mName, message + ",mName");
		assertSame(expected.mPayload, actual.mPayload, message + ",mPayload");
		assertEqual(expected.mID, actual.mID, message + ",mID");
		assertEqual(expected.mModelPath, actual.mModelPath, message + ",mModelPath");
	}
	private static void testBackendSelection()
	{
		string backend = EasyECSRuntimeTestDataECSList.BackendName;
		string managedBackend = EasyECSManagedRuntimeTestDataECSList.BackendName;
		string allManagedBackend = EasyECSAllManagedRuntimeTestDataECSList.BackendName;
#if ECS_FORCE_SAFE_REGISTRY
		assertEqual("SafeRegistry", backend, "ECS_FORCE_SAFE_REGISTRY下普通数据后端错误");
		assertEqual("SafeRegistry", managedBackend, "ECS_FORCE_SAFE_REGISTRY下Managed数据后端错误");
		assertFalse(EasyECSRuntimeTestDataECSList.IsUnsafeBackend, "ECS_FORCE_SAFE_REGISTRY下普通数据不应生成Unsafe后端");
		assertFalse(EasyECSManagedRuntimeTestDataECSList.IsUnsafeBackend, "ECS_FORCE_SAFE_REGISTRY下Managed数据不应生成Unsafe后端");
		assertEqual("SafeRegistry", allManagedBackend, "ECS_FORCE_SAFE_REGISTRY下AllManaged数据后端错误");
		assertFalse(EasyECSAllManagedRuntimeTestDataECSList.IsUnsafeBackend, "AllManaged数据不应生成Unsafe后端");
#else
		assertTrue(backend == "Unsafe" || backend == "SafeSpan" || backend == "SafeRegistry", "普通数据生成了未知Backend:" + backend);
		assertTrue(managedBackend == "Unsafe" || managedBackend == "SafeSpan" || managedBackend == "SafeRegistry", "Managed数据生成了未知Backend:" + managedBackend);
		assertTrue(allManagedBackend == "SafeSpan" || allManagedBackend == "SafeRegistry", "AllManaged数据只能使用SafeSpan或SafeRegistry,Actual:" + allManagedBackend);
		assertFalse(EasyECSAllManagedRuntimeTestDataECSList.IsUnsafeBackend, "AllManaged数据没有Native候选,不应使用Unsafe");
		if (backend == "Unsafe")
		{
			assertEqual("Unsafe", managedBackend, "Allow Unsafe开启时包含Native字段的Managed结构体应使用Hybrid Unsafe");
			assertTrue(EasyECSManagedRuntimeTestDataECSList.IsUnsafeBackend, "Hybrid Managed结构体应标记为Unsafe Backend");
			assertEqual("AllowUnsafe=true,HybridStorage=true", EasyECSManagedRuntimeTestDataECSList.BackendReason, "Hybrid Unsafe后端原因错误");
		}
		else if (backend == "SafeSpan")
		{
			assertEqual("SafeSpan", managedBackend, "普通数据为SafeSpan时Managed数据也应为SafeSpan");
			assertFalse(EasyECSManagedRuntimeTestDataECSList.IsUnsafeBackend, "SafeSpan下Managed结构体不应标记Unsafe");
		}
		else if (backend == "SafeRegistry")
		{
			assertEqual("SafeRegistry", managedBackend, "普通数据为SafeRegistry时Managed数据也应为SafeRegistry");
			assertFalse(EasyECSManagedRuntimeTestDataECSList.IsUnsafeBackend, "SafeRegistry下Managed结构体不应标记Unsafe");
		}
#endif
	}
	private static void assertTrue(bool value, string message)
	{
		if (!value)
		{
			throw new Exception(message);
		}
	}
	private static void assertFalse(bool value, string message)
	{
		if (value)
		{
			throw new Exception(message);
		}
	}
	private static void assertEqual(int expected, int actual, string message)
	{
		if (expected != actual)
		{
			throw new Exception(message + ",Expected:" + expected + ",Actual:" + actual);
		}
	}
	private static void assertEqual(float expected, float actual, string message)
	{
		if (Mathf.Abs(expected - actual) > 0.0001f)
		{
			throw new Exception(message + ",Expected:" + expected + ",Actual:" + actual);
		}
	}
	private static void assertEqual(string expected, string actual, string message)
	{
		if (!string.Equals(expected, actual, StringComparison.Ordinal))
		{
			throw new Exception(message + ",Expected:" + expected + ",Actual:" + actual);
		}
	}
	private static void assertData(EasyECSRuntimeTestData value, int id, int hp, float speed, int camp, string message)
	{
		assertEqual(id, value.mID, message + ".mID");
		assertEqual(hp, value.mHP, message + ".mHP");
		assertEqual(speed, value.mSpeed, message + ".mSpeed");
		assertEqual(id * 10.0f, value.mPositionX, message + ".mPositionX");
		assertEqual(id * -5.0f, value.mPositionY, message + ".mPositionY");
		assertEqual(camp, value.mCamp, message + ".mCamp");
	}
	private static void testListConstructor()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList(0);
		try
		{
			assertEqual(0, list.Count, "新List Count错误");
			assertTrue(list.Capacity >= 1, "capacity<=0时应自动修正为至少1");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListAddGetResize()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList(2);
		try
		{
			for (int i = 0; i < 10; ++i)
			{
				list.Add(createData(i, 100 + i, 1.5f + i, i % 3));
			}
			assertEqual(10, list.Count, "Add后Count错误");
			assertTrue(list.Capacity >= 10, "Resize后Capacity不足");
			for (int i = 0; i < 10; ++i)
			{
				assertData(list.Get(i), i, 100 + i, 1.5f + i, i % 3, "Resize后数据错误,Index:" + i);
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListSet()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			list.Set(0, createData(99, 777, 8.5f, 4));
			assertData(list.Get(0), 99, 777, 8.5f, 4, "Set结果错误");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListIndexerMutation()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			list[0].mHP -= 10;
			list[0].mSpeed += 3.0f;
			list[0].mCamp = 8;
			EasyECSRuntimeTestData value = list.Get(0);
			assertEqual(90, value.mHP, "Indexer修改mHP失败");
			assertEqual(5.0f, value.mSpeed, "Indexer修改mSpeed失败");
			assertEqual(8, value.mCamp, "Indexer修改AoS字段失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListLocalRefMutation()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(5));
			EasyECSRuntimeTestDataRef value = list[0];
			value.mHP = 321;
			value.mPositionX = 77.0f;
			value.mID = 123;
			assertEqual(321, list.Get(0).mHP, "Ref修改mHP失败");
			assertEqual(77.0f, list.Get(0).mPositionX, "Ref修改mPositionX失败");
			assertEqual(123, list.Get(0).mID, "Ref修改AoS字段失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListRefAfterResize()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList(1);
		try
		{
			list.Add(createData(1));
			EasyECSRuntimeTestDataRef first = list[0];
			for (int i = 2; i < 64; ++i)
			{
				list.Add(createData(i));
			}
			first.mHP = 555;
			assertEqual(555, list.Get(0).mHP, "Resize后旧Ref没有继续指向原位置");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListDirectColumn()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1, 10));
			list.Add(createData(2, 20));
			var hp = list.getHPColumn();
			var speed = list.getSpeedColumn();
			hp[0] += 100;
			hp[1] += 200;
			speed[0] = 9.0f;
			assertEqual(110, list.Get(0).mHP, "Direct Column修改0失败");
			assertEqual(220, list.Get(1).mHP, "Direct Column修改1失败");
			assertEqual(9.0f, list.Get(0).mSpeed, "Direct Column修改Speed失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListSetKeepsColumnValid()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			var hp = list.getHPColumn();
			list.Set(0, createData(2, 300));
			hp[0] += 5;
			assertEqual(305, list.Get(0).mHP, "Set不是结构变化,旧Column应继续有效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListRemoveAtSwapBack()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(10, 110));
			list.Add(createData(20, 120));
			list.Add(createData(30, 130));
			list.RemoveAtSwapBack(1);
			assertEqual(2, list.Count, "RemoveAtSwapBack后Count错误");
			assertEqual(10, list.Get(0).mID, "首元素不应变化");
			assertEqual(30, list.Get(1).mID, "最后元素应移动到删除位置");
			assertEqual(130, list.Get(1).mHP, "SwapBack后的SoA字段错误");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListRemoveLast()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1, 101));
			list.Add(createData(2, 102));
			list.RemoveAtSwapBack(1);
			assertEqual(1, list.Count, "删除最后元素后Count错误");
			assertEqual(1, list.Get(0).mID, "删除最后元素不应修改前面元素");
			list.RemoveAtSwapBack(0);
			assertEqual(0, list.Count, "删除唯一元素后Count错误");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListInsertOrder()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1, 101, 2.0f, 1 % 3));
			list.Add(createData(2, 102, 2.0f, 2 % 3));
			list.Add(createData(3, 103, 2.0f, 3 % 3));
			list.Insert(0, createData(10, 110, 2.0f, 10 % 3));
			list.Insert(2, createData(20, 120, 2.0f, 20 % 3));
			list.Insert(list.Count, createData(30, 130, 2.0f, 30 % 3));
			int[] expectedIDs = { 10, 1, 20, 2, 3, 30 };
			int[] expectedHP = { 110, 101, 120, 102, 103, 130 };
			assertEqual(expectedIDs.Length, list.Count, "Insert后Count错误");
			for (int i = 0; i < expectedIDs.Length; ++i)
			{
				EasyECSRuntimeTestData value = list.Get(i);
				assertEqual(expectedIDs[i], value.mID, "Insert顺序错误,Index:" + i);
				assertEqual(expectedHP[i], value.mHP, "Insert SoA字段错误,Index:" + i);
				assertEqual(expectedIDs[i] % 3, value.mCamp, "Insert AoS字段错误,Index:" + i);
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListInsertResize()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList(1);
		try
		{
			list.Add(createData(1, 101));
			int oldCapacity = list.Capacity;
			list.Insert(0, createData(2, 202));
			assertEqual(2, list.Count, "Insert Resize后Count错误");
			assertTrue(list.Capacity > oldCapacity, "Insert满容量时没有Resize");
			assertEqual(2, list.Get(0).mID, "Insert Resize新元素位置错误");
			assertEqual(1, list.Get(1).mID, "Insert Resize旧元素移动错误");
			list.Insert(list.Count, createData(3, 303));
			assertEqual(3, list.Get(2).mID, "Insert Count应等价于尾部插入");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListRemoveAtOrder()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			for (int i = 1; i <= 5; ++i)
			{
				list.Add(createData(i, 100 + i));
			}
			list.RemoveAt(0);
			assertListIDs(list, new[] { 2, 3, 4, 5 }, "RemoveAt首元素");
			list.RemoveAt(1);
			assertListIDs(list, new[] { 2, 4, 5 }, "RemoveAt中间元素");
			list.RemoveAt(list.Count - 1);
			assertListIDs(list, new[] { 2, 4 }, "RemoveAt最后元素");
			assertEqual(104, list.Get(1).mHP, "RemoveAt后SoA字段错误");
			assertEqual(4 % 3, list.Get(1).mCamp, "RemoveAt后AoS字段错误");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListInsertRemoveAtCompatibility()
	{
		List<EasyECSRuntimeTestData> standard = new List<EasyECSRuntimeTestData>();
		EasyECSRuntimeTestDataECSList ecs = new EasyECSRuntimeTestDataECSList(1);
		try
		{
			for (int i = 0; i < 64; ++i)
			{
				EasyECSRuntimeTestData value = createData(i, 1000 + i, 1.0f + i, i % 3);
				standard.Add(value);
				ecs.Add(value);
			}
			System.Random random = new System.Random(20260816);
			int nextID = 10000;
			for (int operation = 0; operation < 1000; ++operation)
			{
				bool doInsert = standard.Count == 0 || (operation % 3 != 0);
				if (doInsert)
				{
					int index = random.Next(standard.Count + 1);
					EasyECSRuntimeTestData value = createData(nextID, 2000 + operation, operation * 0.25f, operation % 5);
					++nextID;
					standard.Insert(index, value);
					ecs.Insert(index, value);
				}
				else
				{
					int index = random.Next(standard.Count);
					standard.RemoveAt(index);
					ecs.RemoveAt(index);
				}
				if ((operation & 31) == 0)
				{
					assertListEquals(standard, ecs, "随机Insert/RemoveAt中间检查,Operation:" + operation);
				}
			}
			assertListEquals(standard, ecs, "随机Insert/RemoveAt最终检查");
		}
		finally
		{
			ecs.Dispose();
		}
	}
	private static void testListLargeStructuralMove()
	{
		const int count = 20000;
		List<EasyECSRuntimeTestData> standard = new List<EasyECSRuntimeTestData>(count + 4);
		EasyECSRuntimeTestDataECSList ecs = new EasyECSRuntimeTestDataECSList(count + 4);
		try
		{
			for (int i = 0; i < count; ++i)
			{
				EasyECSRuntimeTestData value = createData(i, 100000 + i, i * 0.125f, i % 7);
				standard.Add(value);
				ecs.Add(value);
			}
			EasyECSRuntimeTestData head = createData(30001, 300001, 3.25f, 5);
			EasyECSRuntimeTestData middle = createData(30002, 300002, 6.5f, 6);
			standard.Insert(0, head);
			ecs.Insert(0, head);
			int middleIndex = standard.Count >> 1;
			standard.Insert(middleIndex, middle);
			ecs.Insert(middleIndex, middle);
			assertListEquals(standard, ecs, "大块Insert后完整检查");
			standard.RemoveAt(0);
			ecs.RemoveAt(0);
			middleIndex = standard.Count >> 1;
			standard.RemoveAt(middleIndex);
			ecs.RemoveAt(middleIndex);
			assertListEquals(standard, ecs, "大块RemoveAt后完整检查");
		}
		finally
		{
			ecs.Dispose();
		}
	}
	private static void testListInsertRemoveAtBounds()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			assertThrowsArgumentOutOfRange(() => list.Insert(-1, createData(2)), "Insert负索引应抛ArgumentOutOfRangeException");
			assertThrowsArgumentOutOfRange(() => list.Insert(list.Count + 1, createData(2)), "Insert大于Count应抛ArgumentOutOfRangeException");
			assertThrowsArgumentOutOfRange(() => list.RemoveAt(-1), "RemoveAt负索引应抛ArgumentOutOfRangeException");
			assertThrowsArgumentOutOfRange(() => list.RemoveAt(list.Count), "RemoveAt等于Count应抛ArgumentOutOfRangeException");
			assertEqual(1, list.Count, "边界异常后Count不应变化");
		}
		finally
		{
			list.Dispose();
		}
		EasyECSRuntimeTestDataECSList disposed = new EasyECSRuntimeTestDataECSList();
		disposed.Dispose();
		bool insertDisposedCaught = false;
		try
		{
			disposed.Insert(0, createData(1));
		}
		catch (ObjectDisposedException)
		{
			insertDisposedCaught = true;
		}
		assertTrue(insertDisposedCaught, "Dispose后Insert应抛ObjectDisposedException");
		bool removeDisposedCaught = false;
		try
		{
			disposed.RemoveAt(0);
		}
		catch (ObjectDisposedException)
		{
			removeDisposedCaught = true;
		}
		assertTrue(removeDisposedCaught, "Dispose后RemoveAt应抛ObjectDisposedException");
	}
	private static void assertListIDs(EasyECSRuntimeTestDataECSList list, int[] expectedIDs, string message)
	{
		assertEqual(expectedIDs.Length, list.Count, message + ":Count错误");
		for (int i = 0; i < expectedIDs.Length; ++i)
		{
			assertEqual(expectedIDs[i], list.Get(i).mID, message + ":Index:" + i);
		}
	}
	private static void assertListEquals(List<EasyECSRuntimeTestData> standard, EasyECSRuntimeTestDataECSList ecs, string message)
	{
		assertEqual(standard.Count, ecs.Count, message + ":Count错误");
		for (int i = 0; i < standard.Count; ++i)
		{
			EasyECSRuntimeTestData expected = standard[i];
			EasyECSRuntimeTestData actual = ecs.Get(i);
			assertEqual(expected.mID, actual.mID, message + ":mID错误,Index:" + i);
			assertEqual(expected.mHP, actual.mHP, message + ":mHP错误,Index:" + i);
			assertEqual(expected.mSpeed, actual.mSpeed, message + ":mSpeed错误,Index:" + i);
			assertEqual(expected.mPositionX, actual.mPositionX, message + ":mPositionX错误,Index:" + i);
			assertEqual(expected.mPositionY, actual.mPositionY, message + ":mPositionY错误,Index:" + i);
			assertEqual(expected.mCamp, actual.mCamp, message + ":mCamp错误,Index:" + i);
		}
	}
	private static void assertThrowsArgumentOutOfRange(Action action, string message)
	{
		bool caught = false;
		try
		{
			action();
		}
		catch (ArgumentOutOfRangeException)
		{
			caught = true;
		}
		assertTrue(caught, message);
	}
	private static void testListClearReuse()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			list.Add(createData(2));
			int capacity = list.Capacity;
			list.Clear();
			assertEqual(0, list.Count, "Clear后Count不为0");
			assertEqual(capacity, list.Capacity, "Clear不应改变Capacity");
			list.Add(createData(99, 999));
			assertData(list.Get(0), 99, 999, 2.0f, 1, "Clear后重新使用失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListEmptyClear()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Clear();
			list.Clear();
			assertEqual(0, list.Count, "空Clear后Count错误");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListAddRange()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList(1);
		EasyECSRuntimeTestDataECSList other = new EasyECSRuntimeTestDataECSList(1);
		try
		{
			EasyECSRuntimeTestData[] source = { createData(1), createData(2), createData(3) };
			list.AddRange(source);
			assertEqual(3, list.Count, "AddRange数组Count错误");
			for (int i = 0; i < 3; ++i)
			{
				assertData(list.Get(i), i + 1, 100, 2.0f, 1, "AddRange数组数据错误,Index:" + i);
			}
			other.Add(createData(4));
			other.Add(createData(5));
			list.AddRange(other);
			assertEqual(5, list.Count, "AddRange ECSList Count错误");
			assertEqual(4, list.Get(3).mID, "AddRange ECSList第一个元素错误");
			assertEqual(5, list.Get(4).mID, "AddRange ECSList第二个元素错误");
			list.AddRange(list);
			assertEqual(10, list.Count, "AddRange自身Count错误");
			for (int i = 0; i < 5; ++i)
			{
				assertEqual(list.Get(i).mID, list.Get(i + 5).mID, "AddRange自身数据错误,Index:" + i);
			}
		}
		finally
		{
			other.Dispose();
			list.Dispose();
		}
	}
	private static void testListInsertRange()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList(1);
		EasyECSRuntimeTestDataECSList other = new EasyECSRuntimeTestDataECSList(1);
		EasyECSRuntimeTestDataECSList self = new EasyECSRuntimeTestDataECSList(1);
		try
		{
			list.Add(createData(1));
			list.Add(createData(4));
			EasyECSRuntimeTestData[] source = { createData(2), createData(3) };
			list.InsertRange(1, source);
			int[] expected = { 1, 2, 3, 4 };
			for (int i = 0; i < expected.Length; ++i)
			{
				assertEqual(expected[i], list.Get(i).mID, "InsertRange数组顺序错误,Index:" + i);
			}
			other.Add(createData(8));
			other.Add(createData(9));
			list.InsertRange(2, other);
			expected = new[] { 1, 2, 8, 9, 3, 4 };
			for (int i = 0; i < expected.Length; ++i)
			{
				assertEqual(expected[i], list.Get(i).mID, "InsertRange ECSList顺序错误,Index:" + i);
			}
			self.Add(createData(1));
			self.Add(createData(2));
			self.Add(createData(3));
			self.InsertRange(1, self);
			expected = new[] { 1, 1, 2, 3, 2, 3 };
			assertEqual(expected.Length, self.Count, "InsertRange自身Count错误");
			for (int i = 0; i < expected.Length; ++i)
			{
				assertEqual(expected[i], self.Get(i).mID, "InsertRange自身顺序错误,Index:" + i);
			}
		}
		finally
		{
			self.Dispose();
			other.Dispose();
			list.Dispose();
		}
	}
	private static void testListRemoveRange()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			for (int i = 1; i <= 6; ++i)
			{
				list.Add(createData(i));
			}
			list.RemoveRange(1, 3);
			int[] expected = { 1, 5, 6 };
			assertEqual(expected.Length, list.Count, "RemoveRange Count错误");
			for (int i = 0; i < expected.Length; ++i)
			{
				assertEqual(expected[i], list.Get(i).mID, "RemoveRange顺序错误,Index:" + i);
			}
			list.RemoveRange(2, 1);
			assertEqual(2, list.Count, "RemoveRange尾部Count错误");
			list.RemoveRange(0, list.Count);
			assertEqual(0, list.Count, "RemoveRange全部元素后Count错误");
			list.Add(createData(9));
			assertEqual(9, list.Get(0).mID, "RemoveRange后重新Add失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListSearchAndRemoveAll()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			EasyECSRuntimeTestData duplicate = createData(2, 202, 3.0f, 4);
			list.Add(createData(1));
			list.Add(duplicate);
			list.Add(createData(3));
			list.Add(duplicate);
			assertTrue(list.Contains(duplicate), "Contains已有元素失败");
			assertEqual(1, list.IndexOf(duplicate), "IndexOf错误");
			assertEqual(3, list.LastIndexOf(duplicate), "LastIndexOf错误");
			assertTrue(list.Remove(duplicate), "Remove已有元素失败");
			assertEqual(2, list.LastIndexOf(duplicate), "Remove后剩余重复元素位置错误");
			list.Add(createData(4));
			list.Add(createData(5));
			int predicateCount = 0;
			int removed = list.RemoveAll(value =>
			{
				++predicateCount;
				return (value.mID & 1) == 0;
			});
			assertEqual(5, predicateCount, "RemoveAll Predicate应对每个元素只调用一次");
			assertEqual(2, removed, "RemoveAll删除数量错误");
			int[] expected = { 1, 3, 5 };
			assertEqual(expected.Length, list.Count, "RemoveAll Count错误");
			for (int i = 0; i < expected.Length; ++i)
			{
				assertEqual(expected[i], list.Get(i).mID, "RemoveAll顺序错误,Index:" + i);
			}
			list.Clear();
			for (int i = 0; i < 16; ++i)
			{
				list.Add(createData(i));
			}
			removed = list.RemoveAll(value => value.mID < 8);
			assertEqual(8, removed, "RemoveAll连续区间删除数量错误");
			assertEqual(8, list.Count, "RemoveAll连续区间Count错误");
			for (int i = 0; i < 8; ++i)
			{
				assertEqual(i + 8, list.Get(i).mID, "RemoveAll连续区间批量搬移错误,Index:" + i);
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListReverseSortBinarySearch()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(3));
			list.Add(createData(1));
			list.Add(createData(4));
			list.Add(createData(2));
			list.Reverse();
			int[] reversed = { 2, 4, 1, 3 };
			for (int i = 0; i < reversed.Length; ++i)
			{
				assertEqual(reversed[i], list.Get(i).mID, "Reverse顺序错误,Index:" + i);
			}
			list.Sort((left, right) => left.mID.CompareTo(right.mID));
			for (int i = 0; i < 4; ++i)
			{
				assertEqual(i + 1, list.Get(i).mID, "Sort Comparison顺序错误,Index:" + i);
			}
			IComparer<EasyECSRuntimeTestData> comparer = Comparer<EasyECSRuntimeTestData>.Create((left, right) => left.mID.CompareTo(right.mID));
			assertEqual(2, list.BinarySearch(createData(3), comparer), "BinarySearch已有元素错误");
			EasyECSRuntimeTestData missing = createData(5);
			assertEqual(~4, list.BinarySearch(missing, comparer), "BinarySearch不存在元素插入位置错误");
			list.Clear();
			for (int i = 5; i >= 1; --i)
			{
				list.Add(createData(i));
			}
			list.Sort(1, 3, comparer);
			int[] rangeSorted = { 5, 2, 3, 4, 1 };
			for (int i = 0; i < rangeSorted.Length; ++i)
			{
				assertEqual(rangeSorted[i], list.Get(i).mID, "Sort Range顺序错误,Index:" + i);
			}
			list.Clear();
			list.Add(createData(3, 30));
			list.Add(createData(1, 10));
			list.Add(createData(4, 40));
			list.Add(createData(2, 20));
			list.SortByHP();
			for (int i = 0; i < 4; ++i)
			{
				assertEqual((i + 1) * 10, list.Get(i).mHP, "SortByHP HP顺序错误,Index:" + i);
				assertEqual(i + 1, list.Get(i).mID, "SortByHP行数据同步错误,Index:" + i);
			}
			assertEqual(2, list.BinarySearchByHP(30), "BinarySearchByHP已有元素错误");
			assertEqual(~4, list.BinarySearchByHP(50), "BinarySearchByHP不存在元素插入位置错误");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListSortByPermutation()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList(1);
		try
		{
			const int count = 257;
			bool[] idSeen = new bool[count + 1];
			for (int i = 0; i < count; ++i)
			{
				int id = i + 1;
				int hp = (i * 37) % 23;
				list.Add(createData(id, hp, id + 0.25f, id % 5));
			}
			list.SortByHP();
			int previousHP = int.MinValue;
			for (int i = 0; i < count; ++i)
			{
				EasyECSRuntimeTestData value = list.Get(i);
				assertTrue(value.mHP >= previousHP, "SortByHP重复Key顺序错误,Index:" + i);
				previousHP = value.mHP;
				assertTrue(value.mID >= 1 && value.mID <= count, "SortByHP ID范围错误,Index:" + i);
				assertFalse(idSeen[value.mID], "SortByHP出现重复行,ID:" + value.mID);
				idSeen[value.mID] = true;
				assertEqual((value.mID - 1) * 37 % 23, value.mHP, "SortByHP HP与ID错位,ID:" + value.mID);
				assertEqual(value.mID * 10.0f, value.mPositionX, "SortByHP PositionX与ID错位,ID:" + value.mID);
				assertEqual(value.mID * -5.0f, value.mPositionY, "SortByHP PositionY与ID错位,ID:" + value.mID);
				assertEqual(value.mID + 0.25f, value.mSpeed, "SortByHP Speed与ID错位,ID:" + value.mID);
				assertEqual(value.mID % 5, value.mCamp, "SortByHP NotECS字段与ID错位,ID:" + value.mID);
			}
			list.Clear();
			for (int i = 0; i < 20; ++i)
			{
				list.Add(createData(i + 1, 200 - i * 3));
			}
			int firstOutsideHP = list.Get(4).mHP;
			int lastOutsideHP = list.Get(15).mHP;
			list.SortByHP(5, 10, Comparer<int>.Default);
			assertEqual(firstOutsideHP, list.Get(4).mHP, "SortByHP Range修改了前方元素");
			assertEqual(lastOutsideHP, list.Get(15).mHP, "SortByHP Range修改了后方元素");
			for (int i = 6; i < 15; ++i)
			{
				assertTrue(list.Get(i - 1).mHP <= list.Get(i).mHP, "SortByHP Range顺序错误,Index:" + i);
			}
			IComparer<int> descending = Comparer<int>.Create((left, right) => right.CompareTo(left));
			list.SortByHP(descending);
			for (int i = 1; i < list.Count; ++i)
			{
				assertTrue(list.Get(i - 1).mHP >= list.Get(i).mHP, "SortByHP降序Comparer错误,Index:" + i);
			}
			list.SortByHP();
			for (int i = 1; i < list.Count; ++i)
			{
				assertTrue(list.Get(i - 1).mHP <= list.Get(i).mHP, "SortByHP缓存复用后二次排序错误,Index:" + i);
			}
		}
		finally
		{
			list.Dispose();
		}
		EasyECSManagedRuntimeTestDataECSList managedList = new EasyECSManagedRuntimeTestDataECSList(1);
		try
		{
			const int managedCount = 65;
			object[] payloads = new object[managedCount + 1];
			for (int i = 0; i < managedCount; ++i)
			{
				int id = i + 1;
				payloads[id] = new object();
				managedList.Add(createManagedData(id, (i * 19) % 11, "Name" + id, payloads[id], "Path/" + id));
			}
			managedList.SortByHP();
			int previousHP = int.MinValue;
			for (int i = 0; i < managedCount; ++i)
			{
				EasyECSManagedRuntimeTestData value = managedList.Get(i);
				assertTrue(value.mHP >= previousHP, "Managed SortByHP重复Key顺序错误,Index:" + i);
				previousHP = value.mHP;
				assertEqual((value.mID - 1) * 19 % 11, value.mHP, "Managed SortByHP HP与ID错位,ID:" + value.mID);
				assertEqual("Name" + value.mID, value.mName, "Managed SortByHP Name与ID错位,ID:" + value.mID);
				assertSame(payloads[value.mID], value.mPayload, "Managed SortByHP Payload与ID错位,ID:" + value.mID);
				assertEqual("Path/" + value.mID, value.mModelPath, "Managed SortByHP NotECS字段与ID错位,ID:" + value.mID);
			}
		}
		finally
		{
			managedList.Dispose();
		}
	}
	private static void testListByColumnSearchRemoveAll()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1, 10));
			list.Add(createData(2, 20));
			list.Add(createData(3, 20));
			list.Add(createData(4, 30));
			list.Add(createData(5, 40));
			assertTrue(list.ContainsByHP(20), "ContainsByHP已有值失败");
			assertFalse(list.ContainsByHP(99), "ContainsByHP不存在值错误");
			assertEqual(1, list.IndexOfByHP(20), "IndexOfByHP错误");
			assertEqual(2, list.LastIndexOfByHP(20), "LastIndexOfByHP错误");
			assertTrue(list.ExistsByHP(value => value >= 30), "ExistsByHP错误");
			assertEqual(3, list.FindIndexByHP(value => value == 30), "FindIndexByHP错误");
			assertEqual(2, list.FindIndexByHP(2, value => value == 20), "FindIndexByHP startIndex错误");
			assertEqual(-1, list.FindIndexByHP(3, 2, value => value == 20), "FindIndexByHP Range错误");
			int predicateCount = 0;
			int removed = list.RemoveAllByHP(value =>
			{
				++predicateCount;
				return value == 20 || value == 40;
			});
			assertEqual(5, predicateCount, "RemoveAllByHP Predicate应对每个元素只调用一次");
			assertEqual(3, removed, "RemoveAllByHP删除数量错误");
			assertEqual(2, list.Count, "RemoveAllByHP Count错误");
			assertEqual(1, list.Get(0).mID, "RemoveAllByHP首行同步错误");
			assertEqual(10, list.Get(0).mHP, "RemoveAllByHP首行HP错误");
			assertEqual(4, list.Get(1).mID, "RemoveAllByHP末行同步错误");
			assertEqual(30, list.Get(1).mHP, "RemoveAllByHP末行HP错误");
		}
		finally
		{
			list.Dispose();
		}
		EasyECSManagedRuntimeTestDataECSList managedList = new EasyECSManagedRuntimeTestDataECSList();
		try
		{
			object payload1 = new object();
			object payload2 = new object();
			managedList.Add(createManagedData(1, 10, "A", payload1));
			managedList.Add(createManagedData(2, 20, "B", payload2));
			managedList.Add(createManagedData(3, 30, "B", payload1));
			managedList.Add(createManagedData(4, 40, "C", payload2));
			assertTrue(managedList.ContainsByName("B"), "Managed ContainsByName错误");
			assertEqual(1, managedList.IndexOfByName("B"), "Managed IndexOfByName错误");
			assertEqual(2, managedList.LastIndexOfByName("B"), "Managed LastIndexOfByName错误");
			assertEqual(3, managedList.FindIndexByName(value => value == "C"), "Managed FindIndexByName错误");
			int removed = managedList.RemoveAllByName(value => value == "B");
			assertEqual(2, removed, "Managed RemoveAllByName删除数量错误");
			assertEqual(2, managedList.Count, "Managed RemoveAllByName Count错误");
			assertEqual(1, managedList.Get(0).mID, "Managed RemoveAllByName首行ID错误");
			assertTrue(object.ReferenceEquals(payload1, managedList.Get(0).mPayload), "Managed RemoveAllByName首行Payload错误");
			assertEqual(4, managedList.Get(1).mID, "Managed RemoveAllByName末行ID错误");
			assertTrue(object.ReferenceEquals(payload2, managedList.Get(1).mPayload), "Managed RemoveAllByName末行Payload错误");
		}
		finally
		{
			managedList.Dispose();
		}
	}
	private static void testListCopyFindCapacity()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList(1);
		try
		{
			assertTrue(list.EnsureCapacity(64) >= 64, "EnsureCapacity扩容失败");
			list.Add(createData(1));
			list.Add(createData(2));
			list.Add(createData(3));
			EasyECSRuntimeTestData[] array = list.ToArray();
			assertEqual(3, array.Length, "ToArray长度错误");
			for (int i = 0; i < array.Length; ++i)
			{
				assertEqual(i + 1, array[i].mID, "ToArray数据错误,Index:" + i);
			}
			EasyECSRuntimeTestData[] target = new EasyECSRuntimeTestData[5];
			list.CopyTo(target, 1);
			assertEqual(1, target[1].mID, "CopyTo arrayIndex第一个数据错误");
			assertEqual(3, target[3].mID, "CopyTo arrayIndex最后数据错误");
			assertTrue(list.Exists(value => value.mID == 2), "Exists失败");
			assertEqual(2, list.Find(value => value.mID == 2).mID, "Find失败");
			assertEqual(1, list.FindIndex(value => value.mID == 2), "FindIndex失败");
			assertEqual(3, list.FindLast(value => value.mID <= 3).mID, "FindLast失败");
			assertEqual(2, list.FindLastIndex(value => value.mID <= 3), "FindLastIndex失败");
			assertTrue(list.TrueForAll(value => value.mID > 0), "TrueForAll失败");
			list.TrimExcess();
			assertEqual(3, list.Capacity, "TrimExcess后Capacity应收缩到Count");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testListDoubleDispose()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		list.Add(createData(1));
		list.Dispose();
		list.Dispose();
	}
	private static void testManagedFields()
	{
		EasyECSManagedRuntimeTestDataECSList list = new EasyECSManagedRuntimeTestDataECSList(1);
		try
		{
			object payloadA = new object();
			object payloadB = new object();
			list.Add(createManagedData(1, 10, "A", payloadA, "Path/A"));
			list.Add(createManagedData(2, 20, "B", payloadB, "Path/B"));
			assertEqual("A", list.Get(0).mName, "Managed字段Get失败");
			assertSame(payloadA, list.Get(0).mPayload, "Managed object字段Get失败");
			assertEqual("Path/A", list.Get(0).mModelPath, "Managed AoS字段Get失败");
			list[1].mName = "B2";
			list[1].mModelPath = "Path/B2";
			assertEqual("B2", list.Get(1).mName, "Managed字段Ref修改失败");
			assertEqual("Path/B2", list.Get(1).mModelPath, "Managed AoS字段Ref修改失败");
			list.Set(0, createManagedData(3, 30, "A3", payloadB, "Path/A3"));
			assertEqual("A3", list.Get(0).mName, "Managed字段Set失败");
			assertSame(payloadB, list.Get(0).mPayload, "Managed object字段Set失败");
			assertEqual("Path/A3", list.Get(0).mModelPath, "Managed AoS字段Set失败");
			list.RemoveAtSwapBack(0);
			assertEqual(1, list.Count, "Managed字段Remove Count错误");
			assertEqual("B2", list.Get(0).mName, "Managed字段SwapBack失败");
			assertEqual("Path/B2", list.Get(0).mModelPath, "Managed AoS字段SwapBack失败");
			list.Clear();
			list.Add(createManagedData(4, 40, "C", payloadA, "Path/C"));
			assertEqual("C", list.Get(0).mName, "Managed字段Clear后重用失败");
			assertSame(payloadA, list.Get(0).mPayload, "Managed object字段Clear后重用失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testManagedListAddGetResize()
	{
		EasyECSManagedRuntimeTestDataECSList list = new EasyECSManagedRuntimeTestDataECSList(1);
		try
		{
			object[] payloads = new object[32];
			for (int i = 0; i < payloads.Length; ++i)
			{
				payloads[i] = new object();
				list.Add(createManagedData(i, 100 + i, "Name" + i, payloads[i], "Path/" + i));
			}
			assertEqual(payloads.Length, list.Count, "Managed List Resize后Count错误");
			assertTrue(list.Capacity >= payloads.Length, "Managed List Resize后Capacity不足");
			for (int i = 0; i < payloads.Length; ++i)
			{
				EasyECSManagedRuntimeTestData value = list.Get(i);
				assertEqual(i, value.mID, "Managed List mID错误,Index:" + i);
				assertEqual(100 + i, value.mHP, "Managed List mHP错误,Index:" + i);
				assertEqual("Name" + i, value.mName, "Managed List mName错误,Index:" + i);
				assertSame(payloads[i], value.mPayload, "Managed List mPayload错误,Index:" + i);
				assertEqual("Path/" + i, value.mModelPath, "Managed List mModelPath错误,Index:" + i);
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testManagedListRefAfterResize()
	{
		EasyECSManagedRuntimeTestDataECSList list = new EasyECSManagedRuntimeTestDataECSList(1);
		try
		{
			object originalPayload = new object();
			object changedPayload = new object();
			list.Add(createManagedData(1, 10, "Before", originalPayload, "BeforePath"));
			EasyECSManagedRuntimeTestDataRef first = list[0];
			for (int i = 2; i <= 64; ++i)
			{
				list.Add(createManagedData(i));
			}
			first.mHP = 999;
			first.mName = "After";
			first.mPayload = changedPayload;
			first.mID = 777;
			first.mModelPath = "AfterPath";
			EasyECSManagedRuntimeTestData value = list.Get(0);
			assertEqual(999, value.mHP, "Managed Resize后旧Ref修改mHP失败");
			assertEqual("After", value.mName, "Managed Resize后旧Ref修改mName失败");
			assertSame(changedPayload, value.mPayload, "Managed Resize后旧Ref修改mPayload失败");
			assertEqual(777, value.mID, "Managed Resize后旧Ref修改mID失败");
			assertEqual("AfterPath", value.mModelPath, "Managed Resize后旧Ref修改mModelPath失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testManagedListDirectColumn()
	{
		EasyECSManagedRuntimeTestDataECSList list = new EasyECSManagedRuntimeTestDataECSList();
		try
		{
			object payload0 = new object();
			object payload1 = new object();
			list.Add(createManagedData(1, 10, "A"));
			list.Add(createManagedData(2, 20, "B"));
			var hp = list.getHPColumn();
			var name = list.getNameColumn();
			var payload = list.getPayloadColumn();
			hp[0] = 101;
			hp[1] = 202;
			name[0] = "AA";
			name[1] = "BB";
			payload[0] = payload0;
			payload[1] = payload1;
			assertEqual(101, list.Get(0).mHP, "Managed Direct Column mHP[0]失败");
			assertEqual(202, list.Get(1).mHP, "Managed Direct Column mHP[1]失败");
			assertEqual("AA", list.Get(0).mName, "Managed Direct Column mName[0]失败");
			assertEqual("BB", list.Get(1).mName, "Managed Direct Column mName[1]失败");
			assertSame(payload0, list.Get(0).mPayload, "Managed Direct Column mPayload[0]失败");
			assertSame(payload1, list.Get(1).mPayload, "Managed Direct Column mPayload[1]失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testManagedListRemoveAtSwapBack()
	{
		EasyECSManagedRuntimeTestDataECSList list = new EasyECSManagedRuntimeTestDataECSList();
		try
		{
			object payload1 = new object();
			object payload2 = new object();
			object payload3 = new object();
			list.Add(createManagedData(1, 11, "A", payload1, "APath"));
			list.Add(createManagedData(2, 22, "B", payload2, "BPath"));
			list.Add(createManagedData(3, 33, "C", payload3, "CPath"));
			list.RemoveAtSwapBack(1);
			assertEqual(2, list.Count, "Managed RemoveAtSwapBack Count错误");
			EasyECSManagedRuntimeTestData moved = list.Get(1);
			assertEqual(3, moved.mID, "Managed SwapBack mID错误");
			assertEqual(33, moved.mHP, "Managed SwapBack mHP错误");
			assertEqual("C", moved.mName, "Managed SwapBack mName错误");
			assertSame(payload3, moved.mPayload, "Managed SwapBack mPayload错误");
			assertEqual("CPath", moved.mModelPath, "Managed SwapBack mModelPath错误");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testManagedListInsertRemoveAt()
	{
		EasyECSManagedRuntimeTestDataECSList list = new EasyECSManagedRuntimeTestDataECSList(1);
		try
		{
			object payloadA = new object();
			object payloadB = new object();
			object payloadC = new object();
			object payloadX = new object();
			list.Add(createManagedData(1, 11, "A", payloadA, "Path/A"));
			list.Add(createManagedData(2, 22, "B", payloadB, "Path/B"));
			list.Add(createManagedData(3, 33, "C", payloadC, "Path/C"));
			list.Insert(1, createManagedData(9, 99, "X", payloadX, "Path/X"));
			assertEqual(4, list.Count, "Managed Insert后Count错误");
			assertEqual(1, list.Get(0).mID, "Managed Insert前方元素变化");
			assertEqual(9, list.Get(1).mID, "Managed Insert新元素位置错误");
			assertEqual("X", list.Get(1).mName, "Managed Insert mName错误");
			assertSame(payloadX, list.Get(1).mPayload, "Managed Insert mPayload错误");
			assertEqual("Path/X", list.Get(1).mModelPath, "Managed Insert Managed AoS错误");
			assertEqual(2, list.Get(2).mID, "Managed Insert旧元素移动错误");
			assertSame(payloadB, list.Get(2).mPayload, "Managed Insert移动后Payload错误");
			list.RemoveAt(1);
			assertEqual(3, list.Count, "Managed RemoveAt后Count错误");
			assertEqual(2, list.Get(1).mID, "Managed RemoveAt顺序错误");
			assertEqual("B", list.Get(1).mName, "Managed RemoveAt mName错误");
			assertSame(payloadB, list.Get(1).mPayload, "Managed RemoveAt mPayload错误");
			assertEqual("Path/B", list.Get(1).mModelPath, "Managed RemoveAt AoS错误");
			list.Insert(0, new EasyECSManagedRuntimeTestData { mHP = 7, mName = null, mPayload = null, mID = 7, mModelPath = null });
			assertTrue(list.Get(0).mName == null, "Managed Insert null mName错误");
			assertTrue(list.Get(0).mPayload == null, "Managed Insert null mPayload错误");
			assertTrue(list.Get(0).mModelPath == null, "Managed Insert null mModelPath错误");
			list.RemoveAt(0);
			assertEqual(1, list.Get(0).mID, "Managed RemoveAt null元素后顺序错误");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testManagedListLargeStructuralMove()
	{
		const int count = 5000;
		EasyECSManagedRuntimeTestDataECSList list = new EasyECSManagedRuntimeTestDataECSList(count + 4);
		object[] payloads = new object[count];
		try
		{
			for (int i = 0; i < count; ++i)
			{
				payloads[i] = new object();
				list.Add(createManagedData(i, 10000 + i, "Name" + i, payloads[i], "Path/" + i));
			}
			object insertedPayload = new object();
			list.Insert(0, createManagedData(99999, 88888, "Inserted", insertedPayload, "Inserted/Path"));
			assertEqual(99999, list.Get(0).mID, "Managed大块Insert头部mID错误");
			assertSame(insertedPayload, list.Get(0).mPayload, "Managed大块Insert头部Payload错误");
			for (int i = 0; i < count; ++i)
			{
				EasyECSManagedRuntimeTestData value = list.Get(i + 1);
				assertEqual(i, value.mID, "Managed大块Insert顺序错误,Index:" + i);
				assertEqual("Name" + i, value.mName, "Managed大块Insert Name错误,Index:" + i);
				assertSame(payloads[i], value.mPayload, "Managed大块Insert Payload错误,Index:" + i);
				assertEqual("Path/" + i, value.mModelPath, "Managed大块Insert Path错误,Index:" + i);
			}
			list.RemoveAt(0);
			assertEqual(count, list.Count, "Managed大块RemoveAt Count错误");
			for (int i = 0; i < count; ++i)
			{
				EasyECSManagedRuntimeTestData value = list.Get(i);
				assertEqual(i, value.mID, "Managed大块RemoveAt顺序错误,Index:" + i);
				assertSame(payloads[i], value.mPayload, "Managed大块RemoveAt Payload错误,Index:" + i);
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testAllManagedListInsertRemoveAt()
	{
		EasyECSAllManagedRuntimeTestDataECSList list = new EasyECSAllManagedRuntimeTestDataECSList(1);
		try
		{
			object payloadA = new object();
			object payloadB = new object();
			list.Add(new EasyECSAllManagedRuntimeTestData { mName = "A", mPayload = payloadA, mPath = "Path/A" });
			list.Insert(0, new EasyECSAllManagedRuntimeTestData { mName = "B", mPayload = payloadB, mPath = "Path/B" });
			assertEqual(2, list.Count, "AllManaged Insert后Count错误");
			assertEqual("B", list.Get(0).mName, "AllManaged Insert mName错误");
			assertSame(payloadB, list.Get(0).mPayload, "AllManaged Insert mPayload错误");
			assertEqual("Path/B", list.Get(0).mPath, "AllManaged Insert AoS字段错误");
			assertEqual("A", list.Get(1).mName, "AllManaged Insert旧元素移动错误");
			assertSame(payloadA, list.Get(1).mPayload, "AllManaged Insert旧Payload移动错误");
			list.RemoveAt(0);
			assertEqual(1, list.Count, "AllManaged RemoveAt后Count错误");
			assertEqual("A", list.Get(0).mName, "AllManaged RemoveAt顺序错误");
			assertSame(payloadA, list.Get(0).mPayload, "AllManaged RemoveAt Payload错误");
			assertEqual("Path/A", list.Get(0).mPath, "AllManaged RemoveAt AoS错误");
			list.Insert(1, new EasyECSAllManagedRuntimeTestData { mName = null, mPayload = null, mPath = null });
			assertTrue(list.Get(1).mName == null, "AllManaged Insert null mName错误");
			assertTrue(list.Get(1).mPayload == null, "AllManaged Insert null mPayload错误");
			assertTrue(list.Get(1).mPath == null, "AllManaged Insert null mPath错误");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testManagedListNullFields()
	{
		EasyECSManagedRuntimeTestDataECSList list = new EasyECSManagedRuntimeTestDataECSList();
		try
		{
			list.Add(new EasyECSManagedRuntimeTestData { mHP = 1, mName = null, mPayload = null, mID = 1, mModelPath = null });
			assertTrue(list.Get(0).mName == null, "Managed null mName错误");
			assertTrue(list.Get(0).mPayload == null, "Managed null mPayload错误");
			assertTrue(list.Get(0).mModelPath == null, "Managed null mModelPath错误");
			list[0].mName = "Valid";
			list[0].mPayload = new object();
			list[0].mModelPath = "Path";
			list[0].mName = null;
			list[0].mPayload = null;
			list[0].mModelPath = null;
			assertTrue(list.Get(0).mName == null, "Managed Ref写回null mName失败");
			assertTrue(list.Get(0).mPayload == null, "Managed Ref写回null mPayload失败");
			assertTrue(list.Get(0).mModelPath == null, "Managed Ref写回null mModelPath失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testManagedListRangeSort()
	{
		EasyECSManagedRuntimeTestDataECSList list = new EasyECSManagedRuntimeTestDataECSList(1);
		try
		{
			object payload1 = new object();
			object payload2 = new object();
			EasyECSManagedRuntimeTestData[] values =
			{
				createManagedData(3, 100, "C", payload1),
				createManagedData(1, 100, "A", payload2),
				createManagedData(2, 100, "B", payload1),
			};
			list.AddRange(values);
			list.InsertRange(1, new[] { createManagedData(4, 100, "D", payload2) });
			list.RemoveRange(0, 1);
			list.Sort((left, right) => left.mID.CompareTo(right.mID));
			int[] expected = { 1, 2, 4 };
			for (int i = 0; i < expected.Length; ++i)
			{
				assertEqual(expected[i], list.Get(i).mID, "Managed Range/Sort ID错误,Index:" + i);
			}
			assertEqual("A", list.Get(0).mName, "Managed Range/Sort Name错误");
			assertTrue(object.ReferenceEquals(payload2, list.Get(0).mPayload), "Managed Range/Sort Payload错误");
			list.Clear();
			list.Add(createManagedData(3, 30, "C", payload1));
			list.Add(createManagedData(1, 10, "A", payload2));
			list.Add(createManagedData(2, 20, "B", payload1));
			list.SortByHP();
			assertEqual(1, list.Get(0).mID, "Managed SortByHP行数据同步错误");
			assertEqual("A", list.Get(0).mName, "Managed SortByHP Name错误");
			assertTrue(object.ReferenceEquals(payload2, list.Get(0).mPayload), "Managed SortByHP Payload错误");
			assertEqual(1, list.BinarySearchByHP(20), "Managed BinarySearchByHP错误");
			list.Reverse();
			assertEqual(3, list.Get(0).mID, "Managed Reverse错误");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testManagedListArrayConversion()
	{
		EasyECSManagedRuntimeTestData[] source = new EasyECSManagedRuntimeTestData[10];
		object[] payloads = new object[source.Length];
		for (int i = 0; i < source.Length; ++i)
		{
			payloads[i] = new object();
			source[i] = createManagedData(100 + i, 1000 + i, "BulkName" + i, payloads[i], "BulkPath/" + i);
		}
		source[5].mName = null;
		source[5].mPayload = null;
		source[5].mModelPath = null;
		EasyECSManagedRuntimeTestDataECSList list = new EasyECSManagedRuntimeTestDataECSList(2);
		try
		{
			list.AddRange(source, 2, 5);
			int[] expectedSourceIndices = { 2, 3, 4, 5, 6 };
			for (int i = 0; i < expectedSourceIndices.Length; ++i)
			{
				assertManagedDataEqual(source[expectedSourceIndices[i]], list.Get(i), "Managed AddRange局部区间错误,Index:" + i);
			}
			list.InsertRange(2, source, 7, 2);
			expectedSourceIndices = new[] { 2, 3, 7, 8, 4, 5, 6 };
			assertEqual(expectedSourceIndices.Length, list.Count, "Managed InsertRange局部区间Count错误");
			for (int i = 0; i < expectedSourceIndices.Length; ++i)
			{
				assertManagedDataEqual(source[expectedSourceIndices[i]], list.Get(i), "Managed InsertRange局部区间错误,Index:" + i);
			}
			EasyECSManagedRuntimeTestData sentinel = createManagedData(-1, -1, "Sentinel", new object(), "SentinelPath");
			EasyECSManagedRuntimeTestData[] copy = new EasyECSManagedRuntimeTestData[10];
			for (int i = 0; i < copy.Length; ++i)
			{
				copy[i] = sentinel;
			}
			list.CopyTo(1, copy, 3, 4);
			assertManagedDataEqual(sentinel, copy[2], "Managed CopyTo不应修改目标区间前元素");
			assertManagedDataEqual(sentinel, copy[7], "Managed CopyTo不应修改目标区间后元素");
			for (int i = 0; i < 4; ++i)
			{
				assertManagedDataEqual(list.Get(1 + i), copy[3 + i], "Managed CopyTo局部区间错误,Index:" + i);
			}
			EasyECSManagedRuntimeTestData[] all = list.ToArray();
			assertEqual(list.Count, all.Length, "Managed ToArray长度错误");
			for (int i = 0; i < all.Length; ++i)
			{
				assertManagedDataEqual(list.Get(i), all[i], "Managed ToArray数据错误,Index:" + i);
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testDictionaryConstructor()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>(0);
		try
		{
			assertEqual(0, dict.Count, "新Dictionary Count错误");
			assertTrue(dict.Capacity >= 1, "Dictionary capacity应至少为1");
			assertTrue(dict.Comparer != null, "Comparer不应为空");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryAddIndexerResize()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>(2);
		try
		{
			for (int i = 0; i < 32; ++i)
			{
				dict.Add(1000 + i, createData(i, 100 + i));
			}
			assertEqual(32, dict.Count, "Dictionary Add后Count错误");
			assertTrue(dict.Capacity >= 32, "Dictionary Resize后Capacity不足");
			for (int i = 0; i < 32; ++i)
			{
				assertEqual(i, dict[1000 + i].mID, "Dictionary indexer mID错误");
				assertEqual(100 + i, dict[1000 + i].mHP, "Dictionary indexer mHP错误");
			}
			dict[1005].mHP = 888;
			assertEqual(888, dict[1005].mHP, "Dictionary indexer修改失败");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryDuplicateAdd()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			bool caught = false;
			try
			{
				dict.Add(1, createData(2));
			}
			catch (ArgumentException)
			{
				caught = true;
			}
			assertTrue(caught, "Add重复Key应抛ArgumentException");
			assertEqual(1, dict.Count, "重复Add失败后Count不应变化");
			assertEqual(1, dict[1].mID, "重复Add失败后原Value不应变化");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryMissingIndexer()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			bool caught = false;
			try
			{
				int hp = dict[999].mHP;
			}
			catch (System.Collections.Generic.KeyNotFoundException)
			{
				caught = true;
			}
			assertTrue(caught, "Indexer访问不存在Key应抛KeyNotFoundException");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryTryAdd()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			assertTrue(dict.TryAdd(1, createData(1)), "首次TryAdd应成功");
			assertFalse(dict.TryAdd(1, createData(2)), "重复TryAdd应失败");
			assertEqual(1, dict.Count, "重复TryAdd不应增加Count");
			assertEqual(1, dict[1].mID, "重复TryAdd不应覆盖旧值");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryContainsKey()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1));
			assertTrue(dict.ContainsKey(10), "ContainsKey已有Key失败");
			assertFalse(dict.ContainsKey(11), "ContainsKey不存在Key错误");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryTryGetValue()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1, 100));
			assertTrue(dict.TryGetValue(10, out EasyECSRuntimeTestDataRef value), "TryGetValue已有Key失败");
			value.mHP = 777;
			assertEqual(777, dict[10].mHP, "TryGetValue返回Ref修改失败");
			assertFalse(dict.TryGetValue(99, out EasyECSRuntimeTestDataRef missing), "TryGetValue不存在Key应返回false");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryTryGetIndex()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(100, createData(1));
			dict.Add(200, createData(2));
			assertTrue(dict.TryGetIndex(200, out int index), "TryGetIndex已有Key失败");
			assertEqual(1, index, "TryGetIndex索引错误");
			assertFalse(dict.TryGetIndex(300, out _), "TryGetIndex不存在Key应返回false");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryDenseAccess()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(100, createData(1, 11));
			dict.Add(200, createData(2, 22));
			assertEqual(100, dict.getKeyAt(0), "getKeyAt(0)错误");
			assertEqual(200, dict.getKeyAt(1), "getKeyAt(1)错误");
			EasyECSRuntimeTestDataRef value = dict.getValueAt(1);
			assertEqual(22, value.mHP, "getValueAt错误");
			value.mHP = 222;
			assertEqual(222, dict[200].mHP, "getValueAt返回Ref修改失败");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryRemoveSwapBack()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1, 101));
			dict.Add(20, createData(2, 102));
			dict.Add(30, createData(3, 103));
			assertTrue(dict.Remove(20), "Remove已有Key应成功");
			assertEqual(2, dict.Count, "Remove后Count错误");
			assertFalse(dict.ContainsKey(20), "Remove后旧Key仍存在");
			assertTrue(dict.ContainsKey(30), "SwapBack移动Key丢失");
			assertTrue(dict.TryGetIndex(30, out int movedIndex), "移动Key索引不存在");
			assertEqual(1, movedIndex, "最后元素应移动到删除位置");
			assertEqual(30, dict.getKeyAt(1), "mKeys SwapBack错误");
			assertEqual(103, dict.getValueAt(1).mHP, "mValues SwapBack错误");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryRemoveLast()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1, 101));
			dict.Add(20, createData(2, 102));
			assertTrue(dict.Remove(20), "删除最后Key应成功");
			assertEqual(1, dict.Count, "删除最后Key后Count错误");
			assertTrue(dict.ContainsKey(10), "删除最后Key不应影响前面Key");
			assertFalse(dict.ContainsKey(20), "删除最后Key后仍然存在");
			assertEqual(10, dict.getKeyAt(0), "删除最后Key后Dense Key错误");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryRemoveMissing()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			assertFalse(dict.Remove(2), "Remove不存在Key应返回false");
			assertEqual(1, dict.Count, "Remove不存在Key不应改变Count");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryForeachRead()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			for (int i = 1; i <= 5; ++i)
			{
				dict.Add(i * 10, createData(i, i * 100));
			}
			int count = 0;
			int keySum = 0;
			int hpSum = 0;
			foreach (var item in dict)
			{
				++count;
				keySum += item.Key;
				hpSum += item.Value.mHP;
			}
			assertEqual(5, count, "foreach遍历数量错误");
			assertEqual(150, keySum, "foreach Key总和错误");
			assertEqual(1500, hpSum, "foreach Value总和错误");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryForeachWrite()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			for (int i = 0; i < 4; ++i)
			{
				dict.Add(100 + i, createData(i, 10 + i));
			}
			foreach (var item in dict)
			{
				item.Value.mHP += item.Key;
			}
			for (int i = 0; i < 4; ++i)
			{
				assertEqual(110 + i * 2, dict[100 + i].mHP, "foreach Value修改错误,Index:" + i);
			}
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryKeys()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1));
			dict.Add(20, createData(2));
			dict.Add(30, createData(3));
			assertEqual(3, dict.Keys.Count, "Keys.Count错误");
			int count = 0;
			int sum = 0;
			foreach (int key in dict.Keys)
			{
				++count;
				sum += key;
			}
			assertEqual(3, count, "Keys遍历数量错误");
			assertEqual(60, sum, "Keys遍历内容错误");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryValues()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1, 10));
			dict.Add(20, createData(2, 20));
			assertEqual(2, dict.Values.Count, "Values.Count错误");
			int count = 0;
			foreach (var value in dict.Values)
			{
				++count;
				value.mHP += 5;
			}
			assertEqual(2, count, "Values遍历数量错误");
			assertEqual(15, dict[10].mHP, "Values修改第一个Value失败");
			assertEqual(25, dict[20].mHP, "Values修改第二个Value失败");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryManualEnumerator()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1, 10));
			dict.Add(2, createData(2, 20));
			var enumerator = dict.GetEnumerator();
			int count = 0;
			int sum = 0;
			while (enumerator.MoveNext())
			{
				var item = enumerator.Current;
				++count;
				sum += item.Key + item.Value.mHP;
			}
			assertEqual(2, count, "手动Enumerator数量错误");
			assertEqual(33, sum, "手动Enumerator内容错误");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryDirectColumn()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1, 10));
			dict.Add(20, createData(2, 20));
			var hp = dict.getHPColumn();
			hp[0] = 1000;
			hp[1] = 2000;
			assertEqual(1000, dict[10].mHP, "Dictionary Direct Column 0失败");
			assertEqual(2000, dict[20].mHP, "Dictionary Direct Column 1失败");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryTryGetIndexDirectColumn()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(100, createData(1, 10));
			dict.Add(200, createData(2, 20));
			assertTrue(dict.TryGetIndex(200, out int index), "TryGetIndex失败");
			var hp = dict.getHPColumn();
			hp[index] += 50;
			assertEqual(70, dict[200].mHP, "TryGetIndex+Direct Column修改失败");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryDenseIndexFastPath()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		EasyECSManagedRuntimeTestDataECSDictionary<int> managedDict = new EasyECSManagedRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1, 100, 1.5f, 2));
			dict.Add(20, createData(2, 200, 2.5f, 3));
			int index = dict.GetIndex(20);
			var hp = dict.getHPColumn();
			var speed = dict.getSpeedColumn();
			var positionX = dict.getPositionXColumn();
			var positionY = dict.getPositionYColumn();
			hp[index] = 888;
			speed[index] = 8.5f;
			positionX[index] = 18.0f;
			positionY[index] = 28.0f;
			assertEqual(888, dict[20].mHP, "GetIndex+Direct HP修改失败");
			assertEqual(8.5f, dict[20].mSpeed, "GetIndex+Direct Speed修改失败");
			assertEqual(18.0f, dict[20].mPositionX, "GetIndex+Direct PositionX修改失败");
			assertEqual(28.0f, dict[20].mPositionY, "GetIndex+Direct PositionY修改失败");
			EasyECSRuntimeTestData replacement = createData(20, 999, 9.0f, 9);
			int existingIndex = dict.GetOrAddIndex(20, replacement, out bool existingAdded);
			assertFalse(existingAdded, "GetOrAddIndex已有Key不应标记为新增");
			assertEqual(index, existingIndex, "GetOrAddIndex已有Key索引错误");
			assertEqual(888, dict[20].mHP, "GetOrAddIndex已有Key不应覆盖原值");
			EasyECSRuntimeTestData addedValue = createData(3, 300, 3.5f, 4);
			int addedIndex = dict.GetOrAddIndex(30, addedValue, out bool added);
			assertTrue(added, "GetOrAddIndex新Key应标记为新增");
			assertEqual(3, dict.Count, "GetOrAddIndex新增后Count错误");
			assertEqual(addedIndex, dict.GetIndex(30), "GetOrAddIndex新增索引错误");
			assertEqual(300, dict[30].mHP, "GetOrAddIndex新增值错误");
			bool caught = false;
			try
			{
				dict.GetIndex(99);
			}
			catch (KeyNotFoundException)
			{
				caught = true;
			}
			assertTrue(caught, "GetIndex不存在Key应抛KeyNotFoundException");
			object payload = new object();
			int managedIndex = managedDict.GetOrAddIndex(1, new EasyECSManagedRuntimeTestData { mHP = 10, mName = "Managed", mPayload = payload, mID = 1, mModelPath = "Path/1" }, out bool managedAdded);
			assertTrue(managedAdded, "Managed GetOrAddIndex新Key应标记为新增");
			assertEqual(managedIndex, managedDict.GetIndex(1), "Managed GetIndex索引错误");
			assertTrue(ReferenceEquals(payload, managedDict[1].mPayload), "Managed GetOrAddIndex应保持引用身份");
		}
		finally
		{
			dict.Dispose();
			managedDict.Dispose();
		}
	}
	private static void testDictionaryClearReuse()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>(2);
		try
		{
			dict.Add(1, createData(1));
			dict.Add(2, createData(2));
			int capacity = dict.Capacity;
			dict.Clear();
			assertEqual(0, dict.Count, "Clear后Count不为0");
			assertEqual(capacity, dict.Capacity, "Clear不应改变Capacity");
			assertFalse(dict.ContainsKey(1), "Clear后旧Key仍存在");
			dict.Add(99, createData(9, 909));
			assertEqual(909, dict[99].mHP, "Clear后重新使用失败");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryComparer()
	{
		StringComparer comparer = StringComparer.OrdinalIgnoreCase;
		EasyECSRuntimeTestDataECSDictionary<string> dict = new EasyECSRuntimeTestDataECSDictionary<string>(2, comparer);
		try
		{
			assertTrue(ReferenceEquals(comparer, dict.Comparer), "Dictionary没有保留传入Comparer");
			dict.Add("RoleA", createData(1, 123));
			assertTrue(dict.ContainsKey("rolea"), "自定义Comparer未生效");
			assertEqual(123, dict["ROLEA"].mHP, "自定义Comparer Indexer未生效");
			assertFalse(dict.TryAdd("rOlEa", createData(2)), "自定义Comparer下重复TryAdd应失败");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryDoubleDispose()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		dict.Add(1, createData(1));
		dict.Dispose();
		dict.Dispose();
	}
	private static void testDictionarySetAndGetOrAdd()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1, 100));
			dict.SetValue(1, createData(1, 200));
			assertEqual(200, dict[1].mHP, "SetValue更新失败");
			assertFalse(dict.TrySetValue(2, createData(2, 300)), "TrySetValue不存在Key应返回false");
			assertTrue(dict.TrySetValue(1, createData(1, 201)), "TrySetValue已有Key应返回true");
			assertEqual(201, dict[1].mHP, "TrySetValue更新失败");
			EasyECSRuntimeTestDataRef existing = dict.SetOrAdd(1, createData(1, 202));
			assertEqual(1, dict.Count, "SetOrAdd更新已有Key不应增加Count");
			assertEqual(202, existing.mHP, "SetOrAdd已有Key返回值错误");
			EasyECSRuntimeTestDataRef added = dict.SetOrAdd(2, createData(2, 300));
			assertEqual(2, dict.Count, "SetOrAdd新增Key Count错误");
			assertEqual(300, added.mHP, "SetOrAdd新增Key返回值错误");
			EasyECSRuntimeTestDataRef getExisting = dict.GetOrAdd(2, createData(2, 999));
			assertEqual(300, getExisting.mHP, "GetOrAdd已有Key不应覆盖原值");
			EasyECSRuntimeTestDataRef getDefault = dict.GetOrAdd(3);
			getDefault.mID = 3;
			getDefault.mHP = 333;
			assertEqual(333, dict[3].mHP, "GetOrAdd default返回Ref修改失败");
			bool caught = false;
			try
			{
				dict.SetValue(99, createData(99));
			}
			catch (KeyNotFoundException)
			{
				caught = true;
			}
			assertTrue(caught, "SetValue不存在Key应抛KeyNotFoundException");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryFieldFastPath()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		EasyECSManagedRuntimeTestDataECSDictionary<int> managedDict = new EasyECSManagedRuntimeTestDataECSDictionary<int>();
		try
		{
			EasyECSRuntimeTestData original = createData(1, 100, 1.5f, 2);
			dict.Add(10, original);
			assertEqual(100, dict.GetValueByHP(10), "GetValueByHP读取错误");
			assertTrue(dict.TryGetValueByHP(10, out int hp), "TryGetValueByHP已有Key应成功");
			assertEqual(100, hp, "TryGetValueByHP值错误");
			assertFalse(dict.TryGetValueByHP(99, out hp), "TryGetValueByHP不存在Key应失败");
			dict.SetValueByHP(10, 200);
			assertEqual(200, dict[10].mHP, "SetValueByHP更新失败");
			assertEqual(original.mID, dict[10].mID, "SetValueByHP不应修改NotECS字段");
			assertEqual(original.mSpeed, dict[10].mSpeed, "SetValueByHP不应修改其他ECS字段");
			assertTrue(dict.TrySetValueByHP(10, 201), "TrySetValueByHP已有Key应成功");
			assertFalse(dict.TrySetValueByHP(99, 300), "TrySetValueByHP不存在Key应失败");
			assertEqual(201, dict.GetValueByHP(10), "TrySetValueByHP更新失败");
			bool caught = false;
			try
			{
				dict.SetValueByHP(99, 1);
			}
			catch (KeyNotFoundException)
			{
				caught = true;
			}
			assertTrue(caught, "SetValueByHP不存在Key应抛KeyNotFoundException");
			object payload = new object();
			managedDict.Add(1, new EasyECSManagedRuntimeTestData { mHP = 10, mName = "A", mPayload = payload, mID = 1, mModelPath = "Path/A" });
			managedDict.SetValueByName(1, "B");
			assertEqual("B", managedDict.GetValueByName(1), "Managed SetValueByName更新失败");
			assertTrue(managedDict.TrySetValueByPayload(1, null), "Managed TrySetValueByPayload已有Key应成功");
			assertTrue(managedDict.TryGetValueByPayload(1, out object managedPayload), "Managed TryGetValueByPayload已有Key应成功");
			assertTrue(managedPayload == null, "Managed字段级API应支持null");
			assertEqual(1, managedDict[1].mID, "Managed字段级API不应修改NotECS字段");
		}
		finally
		{
			dict.Dispose();
			managedDict.Dispose();
		}
	}
	private static void testDictionaryContainsValueRemoveOut()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			EasyECSRuntimeTestData value1 = createData(1, 111, 1.5f, 2);
			EasyECSRuntimeTestData value2 = createData(2, 222, 2.5f, 3);
			EasyECSRuntimeTestData value3 = createData(3, 333, 3.5f, 4);
			dict.Add(10, value1);
			dict.Add(20, value2);
			dict.Add(30, value3);
			assertTrue(dict.ContainsValue(value2), "ContainsValue已有值失败");
			assertFalse(dict.ContainsValue(createData(9)), "ContainsValue不存在值错误");
			assertTrue(dict.Remove(20, out EasyECSRuntimeTestData removed), "Remove out已有Key失败");
			assertEqual(2, removed.mID, "Remove out返回数据错误");
			assertEqual(222, removed.mHP, "Remove out返回HP错误");
			assertFalse(dict.ContainsKey(20), "Remove out后Key仍存在");
			assertTrue(dict.ContainsKey(30), "Remove out SwapBack后最后Key丢失");
			assertFalse(dict.Remove(99, out EasyECSRuntimeTestData missing), "Remove out不存在Key应返回false");
			assertEqual(0, missing.mID, "Remove out不存在Key应返回default");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testDictionaryCapacityMethods()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>(1);
		try
		{
			assertTrue(dict.EnsureCapacity(64) >= 64, "Dictionary EnsureCapacity失败");
			for (int i = 0; i < 10; ++i)
			{
				dict.Add(i, createData(i));
			}
			dict.TrimExcess();
			assertEqual(dict.Count, dict.Capacity, "Dictionary TrimExcess后Value Capacity应等于Count");
			for (int i = 0; i < 10; ++i)
			{
				assertEqual(i, dict[i].mID, "Dictionary TrimExcess后数据错误,Key:" + i);
			}
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testManagedDictionaryAddIndexerResize()
	{
		EasyECSManagedRuntimeTestDataECSDictionary<int> dict = new EasyECSManagedRuntimeTestDataECSDictionary<int>(1);
		try
		{
			object[] payloads = new object[32];
			for (int i = 0; i < payloads.Length; ++i)
			{
				payloads[i] = new object();
				dict.Add(1000 + i, createManagedData(i, 100 + i, "Name" + i, payloads[i], "Path/" + i));
			}
			assertEqual(payloads.Length, dict.Count, "Managed Dictionary Add后Count错误");
			assertTrue(dict.Capacity >= payloads.Length, "Managed Dictionary Resize后Capacity不足");
			for (int i = 0; i < payloads.Length; ++i)
			{
				EasyECSManagedRuntimeTestDataRef value = dict[1000 + i];
				assertEqual(i, value.mID, "Managed Dictionary Indexer mID错误,Index:" + i);
				assertEqual(100 + i, value.mHP, "Managed Dictionary Indexer mHP错误,Index:" + i);
				assertEqual("Name" + i, value.mName, "Managed Dictionary Indexer mName错误,Index:" + i);
				assertSame(payloads[i], value.mPayload, "Managed Dictionary Indexer mPayload错误,Index:" + i);
				assertEqual("Path/" + i, value.mModelPath, "Managed Dictionary Indexer mModelPath错误,Index:" + i);
			}
			dict[1005].mName = "Changed";
			dict[1005].mModelPath = "ChangedPath";
			assertEqual("Changed", dict[1005].mName, "Managed Dictionary Indexer修改mName失败");
			assertEqual("ChangedPath", dict[1005].mModelPath, "Managed Dictionary Indexer修改AoS managed字段失败");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testManagedDictionaryTryGetValue()
	{
		EasyECSManagedRuntimeTestDataECSDictionary<int> dict = new EasyECSManagedRuntimeTestDataECSDictionary<int>();
		try
		{
			object payload = new object();
			dict.Add(10, createManagedData(1, 10, "Before", null, "BeforePath"));
			assertTrue(dict.TryGetValue(10, out EasyECSManagedRuntimeTestDataRef value), "Managed TryGetValue已有Key失败");
			value.mHP = 99;
			value.mName = "After";
			value.mPayload = payload;
			value.mModelPath = "AfterPath";
			assertEqual(99, dict[10].mHP, "Managed TryGetValue修改mHP失败");
			assertEqual("After", dict[10].mName, "Managed TryGetValue修改mName失败");
			assertSame(payload, dict[10].mPayload, "Managed TryGetValue修改mPayload失败");
			assertEqual("AfterPath", dict[10].mModelPath, "Managed TryGetValue修改mModelPath失败");
			assertFalse(dict.TryGetValue(99, out EasyECSManagedRuntimeTestDataRef missing), "Managed TryGetValue不存在Key应返回false");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testManagedDictionaryForeach()
	{
		EasyECSManagedRuntimeTestDataECSDictionary<int> dict = new EasyECSManagedRuntimeTestDataECSDictionary<int>();
		try
		{
			for (int i = 1; i <= 5; ++i)
			{
				dict.Add(i * 10, createManagedData(i, i * 100, "Name" + i, null, "Path/" + i));
			}
			int count = 0;
			int keySum = 0;
			int hpSum = 0;
			string names = string.Empty;
			foreach (var item in dict)
			{
				++count;
				keySum += item.Key;
				hpSum += item.Value.mHP;
				names += item.Value.mName;
				item.Value.mModelPath = "Changed/" + item.Key;
			}
			assertEqual(5, count, "Managed foreach数量错误");
			assertEqual(150, keySum, "Managed foreach Key总和错误");
			assertEqual(1500, hpSum, "Managed foreach HP总和错误");
			assertEqual("Name1Name2Name3Name4Name5", names, "Managed foreach mName内容错误");
			for (int i = 1; i <= 5; ++i)
			{
				assertEqual("Changed/" + (i * 10), dict[i * 10].mModelPath, "Managed foreach修改AoS managed字段失败");
			}
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testManagedDictionaryValues()
	{
		EasyECSManagedRuntimeTestDataECSDictionary<int> dict = new EasyECSManagedRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(10, createManagedData(1, 10, "A"));
			dict.Add(20, createManagedData(2, 20, "B"));
			int count = 0;
			foreach (var value in dict.Values)
			{
				++count;
				value.mHP += 5;
				value.mName += "X";
				value.mModelPath += "/X";
			}
			assertEqual(2, count, "Managed Values遍历数量错误");
			assertEqual(15, dict[10].mHP, "Managed Values修改mHP失败");
			assertEqual("AX", dict[10].mName, "Managed Values修改mName失败");
			assertEqual("Model/1/X", dict[10].mModelPath, "Managed Values修改mModelPath失败");
			assertEqual(25, dict[20].mHP, "Managed Values第二个mHP错误");
			assertEqual("BX", dict[20].mName, "Managed Values第二个mName错误");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testManagedDictionaryRemoveSwapBack()
	{
		EasyECSManagedRuntimeTestDataECSDictionary<int> dict = new EasyECSManagedRuntimeTestDataECSDictionary<int>();
		try
		{
			object payload1 = new object();
			object payload2 = new object();
			object payload3 = new object();
			dict.Add(10, createManagedData(1, 11, "A", payload1, "APath"));
			dict.Add(20, createManagedData(2, 22, "B", payload2, "BPath"));
			dict.Add(30, createManagedData(3, 33, "C", payload3, "CPath"));
			assertTrue(dict.Remove(20), "Managed Dictionary Remove失败");
			assertEqual(2, dict.Count, "Managed Dictionary Remove后Count错误");
			assertFalse(dict.ContainsKey(20), "Managed Dictionary Remove后旧Key仍存在");
			assertTrue(dict.TryGetIndex(30, out int movedIndex), "Managed Dictionary移动Key索引不存在");
			assertEqual(1, movedIndex, "Managed Dictionary移动索引错误");
			assertEqual(30, dict.getKeyAt(1), "Managed Dictionary Dense Key错误");
			EasyECSManagedRuntimeTestDataRef moved = dict.getValueAt(1);
			assertEqual(3, moved.mID, "Managed Dictionary SwapBack mID错误");
			assertEqual("C", moved.mName, "Managed Dictionary SwapBack mName错误");
			assertSame(payload3, moved.mPayload, "Managed Dictionary SwapBack mPayload错误");
			assertEqual("CPath", moved.mModelPath, "Managed Dictionary SwapBack mModelPath错误");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testManagedDictionaryDirectColumn()
	{
		EasyECSManagedRuntimeTestDataECSDictionary<int> dict = new EasyECSManagedRuntimeTestDataECSDictionary<int>();
		try
		{
			object payload = new object();
			dict.Add(10, createManagedData(1, 10, "A"));
			dict.Add(20, createManagedData(2, 20, "B"));
			var hp = dict.getHPColumn();
			var name = dict.getNameColumn();
			var payloadColumn = dict.getPayloadColumn();
			hp[0] = 100;
			hp[1] = 200;
			name[0] = "AA";
			name[1] = "BB";
			payloadColumn[1] = payload;
			assertEqual(100, dict[10].mHP, "Managed Dictionary Direct mHP[0]失败");
			assertEqual(200, dict[20].mHP, "Managed Dictionary Direct mHP[1]失败");
			assertEqual("AA", dict[10].mName, "Managed Dictionary Direct mName[0]失败");
			assertEqual("BB", dict[20].mName, "Managed Dictionary Direct mName[1]失败");
			assertSame(payload, dict[20].mPayload, "Managed Dictionary Direct mPayload失败");
			assertTrue(dict.TryGetIndex(20, out int index), "Managed Dictionary TryGetIndex失败");
			name[index] = "IndexChanged";
			assertEqual("IndexChanged", dict[20].mName, "Managed Dictionary TryGetIndex+managed Column失败");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testManagedDictionaryClearReuse()
	{
		EasyECSManagedRuntimeTestDataECSDictionary<int> dict = new EasyECSManagedRuntimeTestDataECSDictionary<int>(2);
		try
		{
			object payload = new object();
			dict.Add(1, createManagedData(1, 10, "A", payload, "APath"));
			dict.Add(2, createManagedData(2, 20, "B", payload, "BPath"));
			int capacity = dict.Capacity;
			dict.Clear();
			assertEqual(0, dict.Count, "Managed Dictionary Clear后Count错误");
			assertEqual(capacity, dict.Capacity, "Managed Dictionary Clear不应改变Capacity");
			assertFalse(dict.ContainsKey(1), "Managed Dictionary Clear后旧Key仍存在");
			object newPayload = new object();
			dict.Add(99, createManagedData(9, 909, "New", newPayload, "NewPath"));
			assertEqual(909, dict[99].mHP, "Managed Dictionary Clear后mHP错误");
			assertEqual("New", dict[99].mName, "Managed Dictionary Clear后mName错误");
			assertSame(newPayload, dict[99].mPayload, "Managed Dictionary Clear后mPayload错误");
			assertEqual("NewPath", dict[99].mModelPath, "Managed Dictionary Clear后mModelPath错误");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testManagedDictionaryNullFields()
	{
		EasyECSManagedRuntimeTestDataECSDictionary<int> dict = new EasyECSManagedRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, new EasyECSManagedRuntimeTestData { mHP = 1, mName = null, mPayload = null, mID = 1, mModelPath = null });
			assertTrue(dict[1].mName == null, "Managed Dictionary null mName错误");
			assertTrue(dict[1].mPayload == null, "Managed Dictionary null mPayload错误");
			assertTrue(dict[1].mModelPath == null, "Managed Dictionary null mModelPath错误");
			foreach (var item in dict)
			{
				item.Value.mName = null;
				item.Value.mPayload = null;
				item.Value.mModelPath = null;
			}
			assertTrue(dict[1].mName == null, "Managed Dictionary foreach null mName错误");
			assertTrue(dict[1].mPayload == null, "Managed Dictionary foreach null mPayload错误");
			assertTrue(dict[1].mModelPath == null, "Managed Dictionary foreach null mModelPath错误");
		}
		finally
		{
			dict.Dispose();
		}
	}
#if UNITY_EDITOR
	private static void testEditorListIndexOutOfRange()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			bool caught = false;
			try
			{
				int hp = list[0].mHP;
			}
			catch (ArgumentOutOfRangeException)
			{
				caught = true;
			}
			assertTrue(caught, "空List indexer应检测越界");
			list.Add(createData(1));
			caught = false;
			try
			{
				list.Get(1);
			}
			catch (ArgumentOutOfRangeException)
			{
				caught = true;
			}
			assertTrue(caught, "Get应检测越界");
			caught = false;
			try
			{
				list.Set(-1, createData(2));
			}
			catch (ArgumentOutOfRangeException)
			{
				caught = true;
			}
			assertTrue(caught, "Set应检测负索引");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListDisposedAccess()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		list.Add(createData(1));
		list.Dispose();
		bool caught = false;
		try
		{
			int count = list.Count;
		}
		catch (ObjectDisposedException)
		{
			caught = true;
		}
		assertTrue(caught, "Dispose后Count应抛ObjectDisposedException");
	}
	private static void testEditorListRefAfterClear()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			EasyECSRuntimeTestDataRef value = list[0];
			list.Clear();
			bool caught = false;
			try
			{
				int hp = value.mHP;
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "Clear后旧Ref应失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListRemovedRef()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			list.Add(createData(2));
			EasyECSRuntimeTestDataRef removed = list[0];
			list.RemoveAtSwapBack(0);
			bool caught = false;
			try
			{
				int hp = removed.mHP;
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "删除位置的旧Ref应失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListMovedLastRef()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			list.Add(createData(2));
			list.Add(createData(3));
			EasyECSRuntimeTestDataRef last = list[2];
			list.RemoveAtSwapBack(1);
			bool caught = false;
			try
			{
				int hp = last.mHP;
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "SwapBack被移动的最后元素旧Ref应失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListUnrelatedRef()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1, 101));
			list.Add(createData(2, 102));
			list.Add(createData(3, 103));
			EasyECSRuntimeTestDataRef first = list[0];
			list.RemoveAtSwapBack(1);
			assertEqual(101, first.mHP, "Remove不相关元素不应使Ref失效");
			first.mHP = 555;
			assertEqual(555, list.Get(0).mHP, "有效Ref修改失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListInsertAffectedRefs()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			list.Add(createData(2));
			list.Add(createData(3));
			EasyECSRuntimeTestDataRef affected1 = list[1];
			EasyECSRuntimeTestDataRef affected2 = list[2];
			list.Insert(1, createData(9));
			assertInvalidRef(affected1, "Insert位置旧Ref应失效");
			assertInvalidRef(affected2, "Insert后方旧Ref应失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListInsertEarlierRef()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1, 101));
			list.Add(createData(2, 102));
			list.Add(createData(3, 103));
			EasyECSRuntimeTestDataRef first = list[0];
			list.Insert(1, createData(9));
			assertEqual(101, first.mHP, "Insert前方Ref不应失效");
			first.mHP = 999;
			assertEqual(999, list.Get(0).mHP, "Insert前方Ref修改失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListInsertAtEndRefs()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1, 101));
			list.Add(createData(2, 102));
			EasyECSRuntimeTestDataRef first = list[0];
			EasyECSRuntimeTestDataRef second = list[1];
			list.Insert(list.Count, createData(3, 103));
			assertEqual(101, first.mHP, "尾部Insert不应使已有Ref失效");
			assertEqual(102, second.mHP, "尾部Insert不应使最后已有Ref失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListRemoveAtAffectedRefs()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			for (int i = 1; i <= 4; ++i)
			{
				list.Add(createData(i));
			}
			EasyECSRuntimeTestDataRef removed = list[1];
			EasyECSRuntimeTestDataRef moved2 = list[2];
			EasyECSRuntimeTestDataRef moved3 = list[3];
			list.RemoveAt(1);
			assertInvalidRef(removed, "RemoveAt删除位置旧Ref应失效");
			assertInvalidRef(moved2, "RemoveAt后续移动Ref应失效");
			assertInvalidRef(moved3, "RemoveAt旧最后Ref应失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListRemoveAtEarlierRef()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1, 101));
			list.Add(createData(2, 102));
			list.Add(createData(3, 103));
			EasyECSRuntimeTestDataRef first = list[0];
			list.RemoveAt(1);
			assertEqual(101, first.mHP, "RemoveAt前方Ref不应失效");
			first.mHP = 888;
			assertEqual(888, list.Get(0).mHP, "RemoveAt前方Ref修改失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListRemoveAtRefDoesNotRevive()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			list.Add(createData(2));
			list.Add(createData(3));
			EasyECSRuntimeTestDataRef oldLast = list[2];
			list.RemoveAt(1);
			list.Insert(list.Count, createData(4));
			assertInvalidRef(oldLast, "RemoveAt后旧Ref在后续Insert恢复Count时不应复活");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void assertInvalidRef(EasyECSRuntimeTestDataRef value, string message)
	{
		bool caught = false;
		try
		{
			int hp = value.mHP;
		}
		catch (InvalidOperationException)
		{
			caught = true;
		}
		assertTrue(caught, message);
	}
	private static void testEditorListRangeRefInvalidation()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			list.Add(createData(2));
			list.Add(createData(3));
			EasyECSRuntimeTestDataRef first = list[0];
			EasyECSRuntimeTestDataRef affected = list[1];
			list.InsertRange(1, new[] { createData(9), createData(8) });
			assertEqual(1, first.mID, "InsertRange前方Ref应保持有效");
			assertInvalidRef(affected, "InsertRange影响区间Ref应失效");
			EasyECSRuntimeTestDataRef firstAfterInsert = list[0];
			EasyECSRuntimeTestDataRef removedAffected = list[2];
			list.RemoveRange(1, 2);
			assertEqual(1, firstAfterInsert.mID, "RemoveRange前方Ref应保持有效");
			assertInvalidRef(removedAffected, "RemoveRange影响区间Ref应失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListSortReverseRefInvalidation()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(3));
			list.Add(createData(1));
			list.Add(createData(2));
			EasyECSRuntimeTestDataRef sortedRef = list[1];
			list.Sort((left, right) => left.mID.CompareTo(right.mID));
			assertInvalidRef(sortedRef, "Sort后排序区间Ref应失效");
			EasyECSRuntimeTestDataRef reversedRef = list[0];
			list.Reverse();
			assertInvalidRef(reversedRef, "Reverse后反转区间Ref应失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListRefAfterResize()
	{
		testListRefAfterResize();
	}
	private static void testEditorListColumnAfterAdd()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			var hp = list.getHPColumn();
			list.Add(createData(2));
			bool caught = false;
			try
			{
				int value = hp[0];
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "Add后旧Column应失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListColumnAfterInsert()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			list.Add(createData(2));
			var hp = list.getHPColumn();
			list.Insert(1, createData(3));
			bool caught = false;
			try
			{
				int value = hp[0];
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "Insert后旧Column应失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListColumnAfterRemove()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			list.Add(createData(2));
			var hp = list.getHPColumn();
			list.RemoveAtSwapBack(0);
			bool caught = false;
			try
			{
				int value = hp[0];
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "Remove后旧Column应失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListColumnAfterOrderedRemove()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			list.Add(createData(2));
			var hp = list.getHPColumn();
			list.RemoveAt(0);
			bool caught = false;
			try
			{
				int value = hp[0];
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "RemoveAt后旧Column应失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListColumnAfterClear()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			var hp = list.getHPColumn();
			list.Clear();
			bool caught = false;
			try
			{
				int value = hp[0];
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "Clear后旧Column应失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListColumnAfterDispose()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		list.Add(createData(1));
		var hp = list.getHPColumn();
		list.Dispose();
		bool caught = false;
		try
		{
			int value = hp[0];
		}
		catch (ObjectDisposedException)
		{
			caught = true;
		}
		assertTrue(caught, "Dispose后旧Column应检测Owner已销毁");
	}
	private static void testEditorListColumnOutOfRange()
	{
		EasyECSRuntimeTestDataECSList list = new EasyECSRuntimeTestDataECSList();
		try
		{
			list.Add(createData(1));
			var hp = list.getHPColumn();
			bool caught = false;
			try
			{
				int value = hp[1];
			}
			catch (ArgumentOutOfRangeException)
			{
				caught = true;
			}
			assertTrue(caught, "Column应检测越界");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testEditorListColumnAfterSet()
	{
		testListSetKeepsColumnValid();
	}
	private static void testEditorDictionaryCurrentBeforeMoveNext()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			var enumerator = dict.GetEnumerator();
			bool caught = false;
			try
			{
				var item = enumerator.Current;
				int key = item.Key;
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "MoveNext前Current应无效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryCurrentAfterEnd()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			var enumerator = dict.GetEnumerator();
			assertTrue(enumerator.MoveNext(), "第一次MoveNext应成功");
			assertFalse(enumerator.MoveNext(), "第二次MoveNext应结束");
			bool caught = false;
			try
			{
				var item = enumerator.Current;
				int key = item.Key;
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "遍历结束后Current应无效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryEnumeratorAfterAdd()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			var enumerator = dict.GetEnumerator();
			assertTrue(enumerator.MoveNext(), "MoveNext失败");
			dict.Add(2, createData(2));
			bool caught = false;
			try
			{
				enumerator.MoveNext();
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "Add后Enumerator应失效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryEnumeratorAfterSuccessfulTryAdd()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			var enumerator = dict.GetEnumerator();
			assertTrue(enumerator.MoveNext(), "MoveNext失败");
			assertTrue(dict.TryAdd(2, createData(2)), "TryAdd应成功");
			bool caught = false;
			try
			{
				enumerator.MoveNext();
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "成功TryAdd后Enumerator应失效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryEnumeratorAfterFailedTryAdd()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			dict.Add(2, createData(2));
			var enumerator = dict.GetEnumerator();
			assertTrue(enumerator.MoveNext(), "MoveNext失败");
			assertFalse(dict.TryAdd(1, createData(9)), "重复TryAdd应失败");
			var current = enumerator.Current;
			int firstKey = current.Key;
			assertTrue(enumerator.MoveNext(), "失败TryAdd不应使Enumerator失效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryEnumeratorAfterRemove()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			dict.Add(2, createData(2));
			var enumerator = dict.GetEnumerator();
			assertTrue(enumerator.MoveNext(), "MoveNext失败");
			assertTrue(dict.Remove(2), "Remove应成功");
			bool caught = false;
			try
			{
				enumerator.MoveNext();
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "Remove后Enumerator应失效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryEnumeratorAfterMissingRemove()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			dict.Add(2, createData(2));
			var enumerator = dict.GetEnumerator();
			assertTrue(enumerator.MoveNext(), "MoveNext失败");
			assertFalse(dict.Remove(99), "Remove不存在Key应失败");
			var current = enumerator.Current;
			int firstKey = current.Key;
			assertTrue(enumerator.MoveNext(), "Remove不存在Key不应使Enumerator失效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryEnumeratorAfterClear()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			var enumerator = dict.GetEnumerator();
			assertTrue(enumerator.MoveNext(), "MoveNext失败");
			dict.Clear();
			bool caught = false;
			try
			{
				enumerator.MoveNext();
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "Clear后Enumerator应失效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryEnumeratorAfterDispose()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		dict.Add(1, createData(1));
		var enumerator = dict.GetEnumerator();
		assertTrue(enumerator.MoveNext(), "MoveNext失败");
		dict.Dispose();
		bool caught = false;
		try
		{
			enumerator.MoveNext();
		}
		catch (ObjectDisposedException)
		{
			caught = true;
		}
		assertTrue(caught, "Dispose后Enumerator应检测Owner已销毁");
	}
	private static void testEditorDictionaryEnumeratorAfterValueMutation()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1, 10));
			dict.Add(2, createData(2, 20));
			var enumerator = dict.GetEnumerator();
			assertTrue(enumerator.MoveNext(), "MoveNext失败");
			var item = enumerator.Current;
			item.Value.mHP += 100;
			assertTrue(enumerator.MoveNext(), "Value字段修改不是结构变化,Enumerator应继续有效");
			assertEqual(110, dict[1].mHP, "Enumerator Value修改失败");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryEnumeratorAfterGetColumn()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			dict.Add(2, createData(2));
			var enumerator = dict.GetEnumerator();
			assertTrue(enumerator.MoveNext(), "MoveNext失败");
			var hp = dict.getHPColumn();
			assertTrue(enumerator.MoveNext(), "仅获取Column不是结构变化,Enumerator应继续有效");
			assertEqual(100, hp[0], "获取Column后数据错误");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryEntryAfterStructuralChange()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			var enumerator = dict.GetEnumerator();
			assertTrue(enumerator.MoveNext(), "MoveNext失败");
			var item = enumerator.Current;
			dict.Add(2, createData(2));
			bool keyCaught = false;
			try
			{
				int key = item.Key;
			}
			catch (InvalidOperationException)
			{
				keyCaught = true;
			}
			assertTrue(keyCaught, "结构变化后旧Entry.Key应失效");
			bool valueCaught = false;
			try
			{
				int hp = item.Value.mHP;
			}
			catch (InvalidOperationException)
			{
				valueCaught = true;
			}
			assertTrue(valueCaught, "结构变化后旧Entry.Value应失效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryColumnAfterAdd()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			var hp = dict.getHPColumn();
			dict.Add(2, createData(2));
			bool caught = false;
			try
			{
				int value = hp[0];
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "Dictionary Add后旧Column应失效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryColumnAfterRemove()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			dict.Add(2, createData(2));
			var hp = dict.getHPColumn();
			dict.Remove(1);
			bool caught = false;
			try
			{
				int value = hp[0];
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "Dictionary Remove后旧Column应失效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryColumnAfterClear()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			var hp = dict.getHPColumn();
			dict.Clear();
			bool caught = false;
			try
			{
				int value = hp[0];
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "Dictionary Clear后旧Column应失效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryColumnAfterDispose()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		dict.Add(1, createData(1));
		var hp = dict.getHPColumn();
		dict.Dispose();
		bool caught = false;
		try
		{
			int value = hp[0];
		}
		catch (ObjectDisposedException)
		{
			caught = true;
		}
		assertTrue(caught, "Dictionary Dispose后旧Column应检测Owner已销毁");
	}
	private static void testEditorDictionaryKeyEnumeratorStructuralChange()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			var enumerator = dict.Keys.GetEnumerator();
			assertTrue(enumerator.MoveNext(), "Keys MoveNext失败");
			dict.Add(2, createData(2));
			bool caught = false;
			try
			{
				enumerator.MoveNext();
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "结构变化后Keys Enumerator应失效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryKeyEnumeratorCurrentBoundary()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			var enumerator = dict.Keys.GetEnumerator();
			bool beforeCaught = false;
			try
			{
				int key = enumerator.Current;
			}
			catch (InvalidOperationException)
			{
				beforeCaught = true;
			}
			assertTrue(beforeCaught, "Keys Current在MoveNext前应无效");
			assertTrue(enumerator.MoveNext(), "Keys第一次MoveNext失败");
			assertEqual(1, enumerator.Current, "Keys Current内容错误");
			assertFalse(enumerator.MoveNext(), "Keys应遍历结束");
			bool afterCaught = false;
			try
			{
				int key = enumerator.Current;
			}
			catch (InvalidOperationException)
			{
				afterCaught = true;
			}
			assertTrue(afterCaught, "Keys Current在结束后应无效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryValueEnumeratorStructuralChange()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			dict.Add(2, createData(2));
			var enumerator = dict.Values.GetEnumerator();
			assertTrue(enumerator.MoveNext(), "Values MoveNext失败");
			dict.Remove(2);
			bool caught = false;
			try
			{
				enumerator.MoveNext();
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			assertTrue(caught, "结构变化后Values Enumerator应失效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryValueEnumeratorCurrentBoundary()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1, 123));
			var enumerator = dict.Values.GetEnumerator();
			bool beforeCaught = false;
			try
			{
				int hp = enumerator.Current.mHP;
			}
			catch (InvalidOperationException)
			{
				beforeCaught = true;
			}
			assertTrue(beforeCaught, "Values Current在MoveNext前应无效");
			assertTrue(enumerator.MoveNext(), "Values第一次MoveNext失败");
			assertEqual(123, enumerator.Current.mHP, "Values Current内容错误");
			assertFalse(enumerator.MoveNext(), "Values应遍历结束");
			bool afterCaught = false;
			try
			{
				int hp = enumerator.Current.mHP;
			}
			catch (InvalidOperationException)
			{
				afterCaught = true;
			}
			assertTrue(afterCaught, "Values Current在结束后应无效");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryDenseIndexOutOfRange()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		try
		{
			dict.Add(1, createData(1));
			bool caught = false;
			try
			{
				int key = dict.getKeyAt(1);
			}
			catch (ArgumentOutOfRangeException)
			{
				caught = true;
			}
			assertTrue(caught, "getKeyAt应检测越界");
			caught = false;
			try
			{
				EasyECSRuntimeTestDataRef value = dict.getValueAt(-1);
			}
			catch (ArgumentOutOfRangeException)
			{
				caught = true;
			}
			assertTrue(caught, "getValueAt应检测负索引");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void testEditorDictionaryDisposedAccess()
	{
		EasyECSRuntimeTestDataECSDictionary<int> dict = new EasyECSRuntimeTestDataECSDictionary<int>();
		dict.Add(1, createData(1));
		dict.Dispose();
		bool caught = false;
		try
		{
			bool has = dict.ContainsKey(1);
		}
		catch (ObjectDisposedException)
		{
			caught = true;
		}
		assertTrue(caught, "Dispose后Dictionary普通API应检测已销毁");
	}
#endif
}
