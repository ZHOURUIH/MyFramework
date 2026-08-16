using System;
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
		runTest("非Unsafe后端检查", testBackendSelection);
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
		runTest("List Clear后重新使用", testListClearReuse);
		runTest("List 空Clear", testListEmptyClear);
		runTest("List 重复Dispose", testListDoubleDispose);
		runTest("Managed字段 Add/Get/Set/Clear/Remove", testManagedFields);
		runTest("Managed List Add/Get/Resize/全部字段", testManagedListAddGetResize);
		runTest("Managed List Resize后Ref保持有效", testManagedListRefAfterResize);
		runTest("Managed List Direct Column", testManagedListDirectColumn);
		runTest("Managed List RemoveAtSwapBack", testManagedListRemoveAtSwapBack);
		runTest("Managed List null字段", testManagedListNullFields);
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
		runTest("Dictionary Clear后重新使用", testDictionaryClearReuse);
		runTest("Dictionary 自定义Comparer", testDictionaryComparer);
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
		runTest("Editor List Resize后Ref保持有效", testEditorListRefAfterResize);
		runTest("Editor List Add后Column失效", testEditorListColumnAfterAdd);
		runTest("Editor List Remove后Column失效", testEditorListColumnAfterRemove);
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
	private static void testBackendSelection()
	{
		string backend = EasyECSRuntimeTestDataECSList.BackendName;
		string managedBackend = EasyECSManagedRuntimeTestDataECSList.BackendName;
#if ECS_FORCE_SAFE_REGISTRY
		assertEqual("SafeRegistry", backend, "ECS_FORCE_SAFE_REGISTRY下普通数据后端错误");
		assertEqual("SafeRegistry", managedBackend, "ECS_FORCE_SAFE_REGISTRY下Managed数据后端错误");
		assertFalse(EasyECSRuntimeTestDataECSList.IsUnsafeBackend, "ECS_FORCE_SAFE_REGISTRY下普通数据不应生成Unsafe后端");
		assertFalse(EasyECSManagedRuntimeTestDataECSList.IsUnsafeBackend, "ECS_FORCE_SAFE_REGISTRY下Managed数据不应生成Unsafe后端");
#else
		assertTrue(backend == "Unsafe" || backend == "SafeSpan" || backend == "SafeRegistry", "普通数据生成了未知Backend:" + backend);
		assertTrue(managedBackend == "SafeSpan" || managedBackend == "SafeRegistry", "Managed数据不允许生成Unsafe Backend:" + managedBackend);
		assertFalse(EasyECSManagedRuntimeTestDataECSList.IsUnsafeBackend, "包含Managed字段的数据不允许生成Unsafe后端");
		if (backend == "SafeSpan")
		{
			assertEqual("SafeSpan", managedBackend, "普通数据为SafeSpan时Managed数据也应为SafeSpan");
		}
		else if (backend == "SafeRegistry")
		{
			assertEqual("SafeRegistry", managedBackend, "普通数据为SafeRegistry时Managed数据也应为SafeRegistry");
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
