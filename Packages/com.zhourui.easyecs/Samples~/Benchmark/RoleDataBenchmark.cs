using System;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;
using UnityEngine;

public class RoleDataBenchmark : MonoBehaviour
{
	private const int ENTITY_COUNT = 500000;
	private const int SAMPLE_COUNT = 15;
	private const int WARMUP_COUNT = 3;
	private static double mResultSink;
	private struct BenchmarkResult
	{
		public double mMedian;
		public double mMin;
		public double mMax;
	}
	private void Awake()
	{
		runBenchmark();
	}
	private void runBenchmark()
	{
		runCorrectnessTests();
		Debug.Log("================ RoleData Benchmark Start ================");
		Debug.Log("ECS Backend:" + RoleDataECSList.BackendName);
		Debug.Log("Backend Reason:" + RoleDataECSList.BackendReason);
		Debug.Log("EntityCount:" + ENTITY_COUNT);
		Debug.Log("SampleCount:" + SAMPLE_COUNT);
		Debug.Log("WarmupCount:" + WARMUP_COUNT);
		runTest1();
		runTest2();
		runTest4();
		Debug.Log("ResultSink:" + mResultSink);
		Debug.Log("================ RoleData Benchmark End ================");
	}
	private static void runCorrectnessTest(string name, Action action)
	{
		action();
		Debug.Log("RoleData CorrectnessTest Pass:" + name);
	}
	private static void runCorrectnessTests()
	{
		Debug.Log("================ RoleData Correctness Test Start ================");
		runCorrectnessTest("Add/Get/Resize", testAddGetResize);
		runCorrectnessTest("Set", testSet);
		runCorrectnessTest("RoleDataRef", testRoleDataRef);
		runCorrectnessTest("Resize后RoleDataRef", testRefAfterResize);
		runCorrectnessTest("Direct Column", testDirectColumn);
		runCorrectnessTest("Clear后重新使用", testClearAndReuse);
		runCorrectnessTest("RemoveAtSwapBack", testRemoveAtSwapBack);
		runCorrectnessTest("多ECSList隔离", testMultipleLists);
		runCorrectnessTest("大量扩容", testLargeResize);
		runCorrectnessTest("混合操作OOP一致性", testMixedOperations);
		runCorrectnessTest("重复Dispose", testDisposeTwice);
#if UNITY_EDITOR
		if (RoleDataECSList.IsUnsafeBackend)
		{
			runCorrectnessTest("Unsafe Editor List越界检测", testUnsafeEditorListBounds);
			runCorrectnessTest("Unsafe Editor Dispose后List检测", testUnsafeEditorDisposedList);
			runCorrectnessTest("Unsafe Editor Dispose后Ref检测", testUnsafeEditorDisposedRef);
			runCorrectnessTest("Unsafe Editor Dispose后Column检测", testUnsafeEditorDisposedColumn);
			runCorrectnessTest("Unsafe Editor Clear后Ref检测", testUnsafeEditorRefAfterClear);
			runCorrectnessTest("Unsafe Editor Remove后Ref检测", testUnsafeEditorRefAfterRemove);
			runCorrectnessTest("Unsafe Editor SwapBack移动Ref检测", testUnsafeEditorMovedRefAfterSwapBack);
			runCorrectnessTest("Unsafe Editor Remove无关Ref保持有效", testUnsafeEditorUnaffectedRefAfterRemove);
			runCorrectnessTest("Unsafe Editor Resize后Ref保持有效", testUnsafeEditorRefAfterResize);
			runCorrectnessTest("Unsafe Editor Add后Column失效", testUnsafeEditorColumnAfterAdd);
			runCorrectnessTest("Unsafe Editor Remove后Column失效", testUnsafeEditorColumnAfterRemove);
			runCorrectnessTest("Unsafe Editor Column越界检测", testUnsafeEditorColumnBounds);
		}
#endif
		Debug.Log("================ RoleData Correctness Test Pass ================");
	}
	private static void testAddGetResize()
	{
		RoleDataECSList list = new RoleDataECSList(1);
		try
		{
			const int count = 1024;
			for (int i = 0; i < count; ++i)
			{
				list.Add(createTestEntity(i));
			}
			assertEqual(count, list.Count, "Add后Count错误");
			if (list.Capacity < count)
			{
				throw new Exception("Add/Resize后Capacity错误,Capacity:" + list.Capacity + ",Expected>=" + count);
			}
			for (int i = 0; i < count; ++i)
			{
				assertRoleEqual(createTestEntity(i), list.Get(i), "Add/Get/Resize Index:" + i);
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testSet()
	{
		RoleDataECSList list = new RoleDataECSList(4);
		try
		{
			list.Add(createTestEntity(0));
			list.Add(createTestEntity(1));
			list.Add(createTestEntity(2));
			RoleData expected = createTestEntity(100);
			list.Set(1, expected);
			assertRoleEqual(expected, list.Get(1), "Set");
			assertRoleEqual(createTestEntity(0), list.Get(0), "Set不应修改前一个元素");
			assertRoleEqual(createTestEntity(2), list.Get(2), "Set不应修改后一个元素");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testRoleDataRef()
	{
		RoleDataECSList list = new RoleDataECSList(4);
		try
		{
			RoleData source = createTestEntity(10);
			list.Add(source);
			RoleDataRef role = list[0];
			role.mHP += 100;
			role.mSpeed += 2.5f;
			role.mPositionX += 10.0f;
			role.mPositionY -= 5.0f;
			role.mID += 1000;
			role.mModelID += 2000;
			role.mCamp += 3;
			source.mHP += 100;
			source.mSpeed += 2.5f;
			source.mPositionX += 10.0f;
			source.mPositionY -= 5.0f;
			source.mID += 1000;
			source.mModelID += 2000;
			source.mCamp += 3;
			assertRoleEqual(source, list.Get(0), "RoleDataRef");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testRefAfterResize()
	{
		RoleDataECSList list = new RoleDataECSList(1);
		try
		{
			RoleData expected = createTestEntity(0);
			list.Add(expected);
			RoleDataRef role = list[0];
			for (int i = 1; i < 4096; ++i)
			{
				list.Add(createTestEntity(i));
			}
			role.mHP += 777;
			role.mSpeed += 3.25f;
			role.mPositionX += 123.0f;
			role.mID += 999;
			expected.mHP += 777;
			expected.mSpeed += 3.25f;
			expected.mPositionX += 123.0f;
			expected.mID += 999;
			assertRoleEqual(expected, list.Get(0), "Resize后RoleDataRef");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testDirectColumn()
	{
		RoleDataECSList list = new RoleDataECSList(32);
		try
		{
			const int count = 16;
			for (int i = 0; i < count; ++i)
			{
				list.Add(createTestEntity(i));
			}
			var hp = list.getHPColumn();
			var speed = list.getSpeedColumn();
			var positionX = list.getPositionXColumn();
			var positionY = list.getPositionYColumn();
			for (int i = 0; i < count; ++i)
			{
				hp[i] += 100;
				speed[i] += 1.0f;
				positionX[i] += 2.0f;
				positionY[i] -= 3.0f;
			}
			for (int i = 0; i < count; ++i)
			{
				RoleData expected = createTestEntity(i);
				expected.mHP += 100;
				expected.mSpeed += 1.0f;
				expected.mPositionX += 2.0f;
				expected.mPositionY -= 3.0f;
				assertRoleEqual(expected, list.Get(i), "Direct Column Index:" + i);
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testClearAndReuse()
	{
		RoleDataECSList list = new RoleDataECSList(2);
		try
		{
			for (int i = 0; i < 100; ++i)
			{
				list.Add(createTestEntity(i));
			}
			int capacityBeforeClear = list.Capacity;
			list.Clear();
			assertEqual(0, list.Count, "Clear后Count错误");
			assertEqual(capacityBeforeClear, list.Capacity, "Clear不应该改变Capacity");
			for (int i = 0; i < 32; ++i)
			{
				list.Add(createTestEntity(i + 1000));
			}
			assertEqual(32, list.Count, "Clear后重新Add的Count错误");
			for (int i = 0; i < 32; ++i)
			{
				assertRoleEqual(createTestEntity(i + 1000), list.Get(i), "Clear后重新使用 Index:" + i);
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testRemoveAtSwapBack()
	{
		RoleDataECSList list = new RoleDataECSList(8);
		try
		{
			for (int i = 0; i < 6; ++i)
			{
				list.Add(createTestEntity(i));
			}
			RoleData last = createTestEntity(5);
			list.RemoveAtSwapBack(2);
			assertEqual(5, list.Count, "RemoveAtSwapBack后Count错误");
			assertRoleEqual(last, list.Get(2), "RemoveAtSwapBack没有将最后元素移动到删除位置");
			assertRoleEqual(createTestEntity(0), list.Get(0), "RemoveAtSwapBack错误修改Index0");
			assertRoleEqual(createTestEntity(1), list.Get(1), "RemoveAtSwapBack错误修改Index1");
			assertRoleEqual(createTestEntity(3), list.Get(3), "RemoveAtSwapBack错误修改Index3");
			assertRoleEqual(createTestEntity(4), list.Get(4), "RemoveAtSwapBack错误修改Index4");
			list.RemoveAtSwapBack(list.Count - 1);
			assertEqual(4, list.Count, "删除最后元素后Count错误");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testMultipleLists()
	{
		RoleDataECSList list0 = new RoleDataECSList(2);
		RoleDataECSList list1 = new RoleDataECSList(2);
		try
		{
			RoleData role0 = createTestEntity(10);
			RoleData role1 = createTestEntity(20);
			list0.Add(role0);
			list1.Add(role1);
			RoleDataRef ref0 = list0[0];
			ref0.mHP += 1000;
			ref0.mID += 5000;
			role0.mHP += 1000;
			role0.mID += 5000;
			assertRoleEqual(role0, list0.Get(0), "多ECSList隔离 List0");
			assertRoleEqual(role1, list1.Get(0), "多ECSList隔离 List1被错误修改");
		}
		finally
		{
			list0.Dispose();
			list1.Dispose();
		}
	}
	private static void testLargeResize()
	{
		RoleDataECSList list = new RoleDataECSList(1);
		try
		{
			const int count = 20000;
			for (int i = 0; i < count; ++i)
			{
				list.Add(createTestEntity(i));
			}
			assertEqual(count, list.Count, "大量扩容Count错误");
			int[] indexList = { 0, 1, 2, 127, 128, 255, 256, 1023, 4095, 8191, count - 1 };
			for (int i = 0; i < indexList.Length; ++i)
			{
				int index = indexList[i];
				assertRoleEqual(createTestEntity(index), list.Get(index), "大量扩容 Index:" + index);
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testMixedOperations()
	{
		List<RoleData> normalList = new List<RoleData>();
		RoleDataECSList ecsList = new RoleDataECSList(1);
		try
		{
			for (int i = 0; i < 32; ++i)
			{
				RoleData data = createTestEntity(i);
				normalList.Add(data);
				ecsList.Add(data);
			}
			RoleData setData = createTestEntity(100);
			normalList[5] = setData;
			ecsList.Set(5, setData);
			RoleData normalRole = normalList[3];
			normalRole.mHP += 100;
			normalRole.mSpeed += 2.0f;
			normalRole.mPositionX += 5.0f;
			normalRole.mPositionY -= 7.0f;
			normalRole.mID += 1000;
			normalRole.mModelID += 2000;
			normalRole.mCamp += 1;
			normalList[3] = normalRole;
			RoleDataRef ecsRole = ecsList[3];
			ecsRole.mHP += 100;
			ecsRole.mSpeed += 2.0f;
			ecsRole.mPositionX += 5.0f;
			ecsRole.mPositionY -= 7.0f;
			ecsRole.mID += 1000;
			ecsRole.mModelID += 2000;
			ecsRole.mCamp += 1;
			removeAtSwapBack(normalList, 7);
			ecsList.RemoveAtSwapBack(7);
			RoleData addData = createTestEntity(200);
			normalList.Add(addData);
			ecsList.Add(addData);
			for (int i = 0; i < normalList.Count; ++i)
			{
				RoleData data = normalList[i];
				data.mHP += 3;
				data.mPositionX += data.mSpeed;
				data.mPositionY -= 1.0f;
				normalList[i] = data;
			}
			var hp = ecsList.getHPColumn();
			var speed = ecsList.getSpeedColumn();
			var positionX = ecsList.getPositionXColumn();
			var positionY = ecsList.getPositionYColumn();
			for (int i = 0; i < ecsList.Count; ++i)
			{
				hp[i] += 3;
				positionX[i] += speed[i];
				positionY[i] -= 1.0f;
			}
			assertEqual(normalList.Count, ecsList.Count, "混合操作Count不一致");
			for (int i = 0; i < normalList.Count; ++i)
			{
				assertRoleEqual(normalList[i], ecsList.Get(i), "混合操作OOP一致性 Index:" + i);
			}
		}
		finally
		{
			ecsList.Dispose();
		}
	}
	private static void testDisposeTwice()
	{
		RoleDataECSList list = new RoleDataECSList(16);
		list.Add(createTestEntity(0));
		list.Dispose();
		list.Dispose();
	}
#if UNITY_EDITOR
	private static void testUnsafeEditorListBounds()
	{
		RoleDataECSList list = new RoleDataECSList(4);
		try
		{
			list.Add(createTestEntity(0));
			bool negativeCaught = false;
			try
			{
				int value = list[-1].mHP;
			}
			catch (ArgumentOutOfRangeException)
			{
				negativeCaught = true;
			}
			if (!negativeCaught)
			{
				throw new Exception("list[-1]没有触发ArgumentOutOfRangeException");
			}
			bool upperCaught = false;
			try
			{
				int value = list[list.Count].mHP;
			}
			catch (ArgumentOutOfRangeException)
			{
				upperCaught = true;
			}
			if (!upperCaught)
			{
				throw new Exception("list[Count]没有触发ArgumentOutOfRangeException");
			}
			bool getCaught = false;
			try
			{
				list.Get(list.Count);
			}
			catch (ArgumentOutOfRangeException)
			{
				getCaught = true;
			}
			if (!getCaught)
			{
				throw new Exception("Get(Count)没有触发ArgumentOutOfRangeException");
			}
			bool setCaught = false;
			try
			{
				list.Set(list.Count, createTestEntity(1));
			}
			catch (ArgumentOutOfRangeException)
			{
				setCaught = true;
			}
			if (!setCaught)
			{
				throw new Exception("Set(Count)没有触发ArgumentOutOfRangeException");
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testUnsafeEditorDisposedList()
	{
		RoleDataECSList list = new RoleDataECSList(1);
		list.Add(createTestEntity(0));
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
		if (!caught)
		{
			throw new Exception("Dispose后访问ECSList.Count没有触发ObjectDisposedException");
		}
	}
	private static void testUnsafeEditorDisposedRef()
	{
		RoleDataECSList list = new RoleDataECSList(1);
		list.Add(createTestEntity(0));
		RoleDataRef role = list[0];
		list.Dispose();
		bool caught = false;
		try
		{
			int hp = role.mHP;
		}
		catch (ObjectDisposedException)
		{
			caught = true;
		}
		if (!caught)
		{
			throw new Exception("Dispose后访问旧RoleDataRef没有触发ObjectDisposedException,Unsafe下存在野指针访问风险");
		}
	}
	private static void testUnsafeEditorDisposedColumn()
	{
		RoleDataECSList list = new RoleDataECSList(1);
		list.Add(createTestEntity(0));
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
		if (!caught)
		{
			throw new Exception("Dispose后访问旧Column没有触发ObjectDisposedException,Unsafe下存在野指针访问风险");
		}
	}
	private static void testUnsafeEditorRefAfterClear()
	{
		RoleDataECSList list = new RoleDataECSList(4);
		try
		{
			list.Add(createTestEntity(0));
			RoleDataRef role = list[0];
			list.Clear();
			bool caught = false;
			try
			{
				int hp = role.mHP;
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			if (!caught)
			{
				throw new Exception("Clear后访问旧RoleDataRef没有触发InvalidOperationException");
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testUnsafeEditorRefAfterRemove()
	{
		RoleDataECSList list = new RoleDataECSList(4);
		try
		{
			list.Add(createTestEntity(0));
			list.Add(createTestEntity(1));
			list.Add(createTestEntity(2));
			RoleDataRef role = list[1];
			list.RemoveAtSwapBack(1);
			bool caught = false;
			try
			{
				int hp = role.mHP;
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			if (!caught)
			{
				throw new Exception("RemoveAtSwapBack删除元素后访问旧RoleDataRef没有触发InvalidOperationException");
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testUnsafeEditorMovedRefAfterSwapBack()
	{
		RoleDataECSList list = new RoleDataECSList(4);
		try
		{
			list.Add(createTestEntity(0));
			list.Add(createTestEntity(1));
			list.Add(createTestEntity(2));
			list.Add(createTestEntity(3));
			RoleDataRef lastRole = list[3];
			list.RemoveAtSwapBack(1);
			bool caught = false;
			try
			{
				int hp = lastRole.mHP;
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			if (!caught)
			{
				throw new Exception("SwapBack后被搬动元素的旧RoleDataRef没有触发InvalidOperationException");
			}
			assertRoleEqual(createTestEntity(3), list.Get(1), "SwapBack数据移动");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testUnsafeEditorUnaffectedRefAfterRemove()
	{
		RoleDataECSList list = new RoleDataECSList(8);
		try
		{
			for (int i = 0; i < 6; ++i)
			{
				list.Add(createTestEntity(i));
			}
			RoleDataRef role0 = list[0];
			int expectedHP = role0.mHP;
			list.RemoveAtSwapBack(2);
			role0.mHP += 100;
			assertEqual(expectedHP + 100, list.Get(0).mHP, "RemoveAtSwapBack后无关RoleDataRef错误失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testUnsafeEditorRefAfterResize()
	{
		RoleDataECSList list = new RoleDataECSList(1);
		try
		{
			list.Add(createTestEntity(0));
			RoleDataRef role = list[0];
			int expectedHP = role.mHP;
			for (int i = 1; i < 4096; ++i)
			{
				list.Add(createTestEntity(i));
			}
			role.mHP += 777;
			assertEqual(expectedHP + 777, list.Get(0).mHP, "Add/Resize后RoleDataRef错误失效");
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testUnsafeEditorColumnAfterAdd()
	{
		RoleDataECSList list = new RoleDataECSList(4);
		try
		{
			list.Add(createTestEntity(0));
			var hp = list.getHPColumn();
			list.Add(createTestEntity(1));
			bool caught = false;
			try
			{
				int value = hp[0];
			}
			catch (InvalidOperationException)
			{
				caught = true;
			}
			if (!caught)
			{
				throw new Exception("Add后访问旧Column没有触发InvalidOperationException");
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testUnsafeEditorColumnAfterRemove()
	{
		RoleDataECSList list = new RoleDataECSList(4);
		try
		{
			list.Add(createTestEntity(0));
			list.Add(createTestEntity(1));
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
			if (!caught)
			{
				throw new Exception("RemoveAtSwapBack后访问旧Column没有触发InvalidOperationException");
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private static void testUnsafeEditorColumnBounds()
	{
		RoleDataECSList list = new RoleDataECSList(4);
		try
		{
			list.Add(createTestEntity(0));
			var hp = list.getHPColumn();
			bool negativeCaught = false;
			try
			{
				int value = hp[-1];
			}
			catch (ArgumentOutOfRangeException)
			{
				negativeCaught = true;
			}
			if (!negativeCaught)
			{
				throw new Exception("Column[-1]没有触发ArgumentOutOfRangeException");
			}
			bool upperCaught = false;
			try
			{
				int value = hp[list.Count];
			}
			catch (ArgumentOutOfRangeException)
			{
				upperCaught = true;
			}
			if (!upperCaught)
			{
				throw new Exception("Column[Count]没有触发ArgumentOutOfRangeException");
			}
		}
		finally
		{
			list.Dispose();
		}
	}
#endif
	private static RoleData createTestEntity(int index)
	{
		RoleData data = new RoleData();
		data.mHP = 1000 + index % 1000;
		data.mSpeed = 0.1f + index % 100 * 0.001f;
		data.mPositionX = index * 0.01f;
		data.mPositionY = index * 0.02f;
		data.mID = index;
		data.mModelID = 10000 + index % 100;
		data.mCamp = index % 3;
		return data;
	}
	private static void removeAtSwapBack(List<RoleData> list, int index)
	{
		int lastIndex = list.Count - 1;
		if (index != lastIndex)
		{
			list[index] = list[lastIndex];
		}
		list.RemoveAt(lastIndex);
	}
	private static void assertRoleEqual(RoleData expected, RoleData actual, string message)
	{
		if (expected.mHP != actual.mHP)
		{
			throw new Exception(message + ",mHP错误,Expected:" + expected.mHP + ",Actual:" + actual.mHP);
		}
		assertFloat(expected.mSpeed, actual.mSpeed, message + ",mSpeed错误");
		assertFloat(expected.mPositionX, actual.mPositionX, message + ",mPositionX错误");
		assertFloat(expected.mPositionY, actual.mPositionY, message + ",mPositionY错误");
		if (expected.mID != actual.mID)
		{
			throw new Exception(message + ",mID错误,Expected:" + expected.mID + ",Actual:" + actual.mID);
		}
		if (expected.mModelID != actual.mModelID)
		{
			throw new Exception(message + ",mModelID错误,Expected:" + expected.mModelID + ",Actual:" + actual.mModelID);
		}
		if (expected.mCamp != actual.mCamp)
		{
			throw new Exception(message + ",mCamp错误,Expected:" + expected.mCamp + ",Actual:" + actual.mCamp);
		}
	}
	private static void assertEqual(int expected, int actual, string message)
	{
		if (expected != actual)
		{
			throw new Exception(message + ",Expected:" + expected + ",Actual:" + actual);
		}
	}
	private static void assertFloat(float expected, float actual, string message)
	{
		if (Math.Abs(expected - actual) > 0.0001f)
		{
			throw new Exception(message + ",Expected:" + expected + ",Actual:" + actual);
		}
	}
	private static void runTest1()
	{
		List<RoleData> managedList;
		RoleData[] managedArray;
		RoleDataECSList ecsList;
		createBenchmarkData(out managedList, out managedArray, out ecsList);
		try
		{
			BenchmarkResult listResult = measure(() =>
			{
				for (int i = 0; i < managedList.Count; ++i)
				{
					RoleData role = managedList[i];
					role.mHP += 1;
					managedList[i] = role;
				}
			});
			mResultSink += managedList[ENTITY_COUNT - 1].mHP;
			BenchmarkResult arrayResult = measure(() =>
			{
				for (int i = 0; i < managedArray.Length; ++i)
				{
					managedArray[i].mHP += 1;
				}
			});
			mResultSink += managedArray[ENTITY_COUNT - 1].mHP;
			BenchmarkResult indexResult = measure(() =>
			{
				for (int i = 0; i < ecsList.Count; ++i)
				{
					ecsList[i].mHP += 1;
				}
			});
			mResultSink += ecsList.Get(ENTITY_COUNT - 1).mHP;
			BenchmarkResult refResult = measure(() =>
			{
				for (int i = 0; i < ecsList.Count; ++i)
				{
					RoleDataRef role = ecsList[i];
					role.mHP += 1;
				}
			});
			mResultSink += ecsList.Get(ENTITY_COUNT - 1).mHP;
			BenchmarkResult directResult = measure(() =>
			{
				var hp = ecsList.getHPColumn();
				int count = ecsList.Count;
				for (int i = 0; i < count; ++i)
				{
					hp[i] += 1;
				}
			});
			mResultSink += ecsList.Get(ENTITY_COUNT - 1).mHP;
			printResult("修改1个字段", listResult, arrayResult, indexResult, refResult, directResult);
		}
		finally
		{
			ecsList.Dispose();
		}
	}
	private static void runTest2()
	{
		List<RoleData> managedList;
		RoleData[] managedArray;
		RoleDataECSList ecsList;
		createBenchmarkData(out managedList, out managedArray, out ecsList);
		try
		{
			BenchmarkResult listResult = measure(() =>
			{
				for (int i = 0; i < managedList.Count; ++i)
				{
					RoleData role = managedList[i];
					role.mPositionX += role.mSpeed;
					managedList[i] = role;
				}
			});
			mResultSink += managedList[ENTITY_COUNT - 1].mPositionX;
			BenchmarkResult arrayResult = measure(() =>
			{
				for (int i = 0; i < managedArray.Length; ++i)
				{
					managedArray[i].mPositionX += managedArray[i].mSpeed;
				}
			});
			mResultSink += managedArray[ENTITY_COUNT - 1].mPositionX;
			BenchmarkResult indexResult = measure(() =>
			{
				for (int i = 0; i < ecsList.Count; ++i)
				{
					ecsList[i].mPositionX += ecsList[i].mSpeed;
				}
			});
			mResultSink += ecsList.Get(ENTITY_COUNT - 1).mPositionX;
			BenchmarkResult refResult = measure(() =>
			{
				for (int i = 0; i < ecsList.Count; ++i)
				{
					RoleDataRef role = ecsList[i];
					role.mPositionX += role.mSpeed;
				}
			});
			mResultSink += ecsList.Get(ENTITY_COUNT - 1).mPositionX;
			BenchmarkResult directResult = measure(() =>
			{
				var speed = ecsList.getSpeedColumn();
				var positionX = ecsList.getPositionXColumn();
				int count = ecsList.Count;
				for (int i = 0; i < count; ++i)
				{
					positionX[i] += speed[i];
				}
			});
			mResultSink += ecsList.Get(ENTITY_COUNT - 1).mPositionX;
			printResult("访问2个字段", listResult, arrayResult, indexResult, refResult, directResult);
		}
		finally
		{
			ecsList.Dispose();
		}
	}
	private static void runTest4()
	{
		List<RoleData> managedList;
		RoleData[] managedArray;
		RoleDataECSList ecsList;
		createBenchmarkData(out managedList, out managedArray, out ecsList);
		try
		{
			BenchmarkResult listResult = measure(() =>
			{
				for (int i = 0; i < managedList.Count; ++i)
				{
					RoleData role = managedList[i];
					role.mHP += 1;
					role.mPositionX += role.mSpeed;
					role.mPositionY += role.mSpeed;
					managedList[i] = role;
				}
			});
			mResultSink += managedList[ENTITY_COUNT - 1].mHP + managedList[ENTITY_COUNT - 1].mPositionX + managedList[ENTITY_COUNT - 1].mPositionY;
			BenchmarkResult arrayResult = measure(() =>
			{
				for (int i = 0; i < managedArray.Length; ++i)
				{
					managedArray[i].mHP += 1;
					managedArray[i].mPositionX += managedArray[i].mSpeed;
					managedArray[i].mPositionY += managedArray[i].mSpeed;
				}
			});
			mResultSink += managedArray[ENTITY_COUNT - 1].mHP + managedArray[ENTITY_COUNT - 1].mPositionX + managedArray[ENTITY_COUNT - 1].mPositionY;
			BenchmarkResult indexResult = measure(() =>
			{
				for (int i = 0; i < ecsList.Count; ++i)
				{
					ecsList[i].mHP += 1;
					ecsList[i].mPositionX += ecsList[i].mSpeed;
					ecsList[i].mPositionY += ecsList[i].mSpeed;
				}
			});
			RoleData indexSink = ecsList.Get(ENTITY_COUNT - 1);
			mResultSink += indexSink.mHP + indexSink.mPositionX + indexSink.mPositionY;
			BenchmarkResult refResult = measure(() =>
			{
				for (int i = 0; i < ecsList.Count; ++i)
				{
					RoleDataRef role = ecsList[i];
					role.mHP += 1;
					role.mPositionX += role.mSpeed;
					role.mPositionY += role.mSpeed;
				}
			});
			RoleData refSink = ecsList.Get(ENTITY_COUNT - 1);
			mResultSink += refSink.mHP + refSink.mPositionX + refSink.mPositionY;
			BenchmarkResult directResult = measure(() =>
			{
				var hp = ecsList.getHPColumn();
				var speed = ecsList.getSpeedColumn();
				var positionX = ecsList.getPositionXColumn();
				var positionY = ecsList.getPositionYColumn();
				int count = ecsList.Count;
				for (int i = 0; i < count; ++i)
				{
					hp[i] += 1;
					positionX[i] += speed[i];
					positionY[i] += speed[i];
				}
			});
			RoleData directSink = ecsList.Get(ENTITY_COUNT - 1);
			mResultSink += directSink.mHP + directSink.mPositionX + directSink.mPositionY;
			printResult("访问4个字段", listResult, arrayResult, indexResult, refResult, directResult);
		}
		finally
		{
			ecsList.Dispose();
		}
	}
	private static void createBenchmarkData(out List<RoleData> managedList, out RoleData[] managedArray, out RoleDataECSList ecsList)
	{
		managedList = new List<RoleData>(ENTITY_COUNT);
		managedArray = new RoleData[ENTITY_COUNT];
		ecsList = new RoleDataECSList(ENTITY_COUNT);
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			RoleData role = createBenchmarkEntity(i);
			managedList.Add(role);
			managedArray[i] = role;
			ecsList.Add(role);
		}
	}
	private static RoleData createBenchmarkEntity(int index)
	{
		RoleData role = new RoleData();
		role.mHP = 1000 + index % 100;
		role.mSpeed = 0.1f + index % 20 * 0.01f;
		role.mPositionX = index % 1000;
		role.mPositionY = index % 500;
		role.mID = index;
		role.mModelID = index % 100;
		role.mCamp = index % 3;
		return role;
	}
	private static BenchmarkResult measure(Action action)
	{
		for (int i = 0; i < WARMUP_COUNT; ++i)
		{
			action();
		}
		double[] samples = new double[SAMPLE_COUNT];
		double min = double.MaxValue;
		double max = double.MinValue;
		for (int i = 0; i < SAMPLE_COUNT; ++i)
		{
			long start = Stopwatch.GetTimestamp();
			action();
			long end = Stopwatch.GetTimestamp();
			double milliseconds = (end - start) * 1000.0 / Stopwatch.Frequency;
			samples[i] = milliseconds;
			if (milliseconds < min)
			{
				min = milliseconds;
			}
			if (milliseconds > max)
			{
				max = milliseconds;
			}
		}
		Array.Sort(samples);
		BenchmarkResult result = new BenchmarkResult();
		result.mMedian = samples[SAMPLE_COUNT / 2];
		result.mMin = min;
		result.mMax = max;
		return result;
	}
	private static void printResult(string title, BenchmarkResult managedList, BenchmarkResult managedArray, BenchmarkResult ecsIndex, BenchmarkResult ecsRef, BenchmarkResult ecsDirect)
	{
		Debug.Log("\n================ " + title + " ================\n" +
			formatBenchmarkLine("List<RoleData>", managedList) + "\n" +
			formatBenchmarkLine("RoleData[]", managedArray) + "\n" +
			formatBenchmarkLine("ECS list[i]", ecsIndex) + "\n" +
			formatBenchmarkLine("ECS Ref", ecsRef) + "\n" +
			formatBenchmarkLine("ECS Direct", ecsDirect) + "\n" +
			"--------------------------------------------------\n" +
			"Index / Direct       : " + ratio(ecsIndex.mMedian, ecsDirect.mMedian).ToString("F2") + "x\n" +
			"Ref / Direct         : " + ratio(ecsRef.mMedian, ecsDirect.mMedian).ToString("F2") + "x\n" +
			"Index / Ref          : " + ratio(ecsIndex.mMedian, ecsRef.mMedian).ToString("F2") + "x\n" +
			"Managed AoS / Direct : " + ratio(managedArray.mMedian, ecsDirect.mMedian).ToString("F2") + "x\n" +
			"Managed AoS / Ref    : " + ratio(managedArray.mMedian, ecsRef.mMedian).ToString("F2") + "x\n" +
			"==================================================");
	}
	private static string formatBenchmarkLine(string name, BenchmarkResult result)
	{
		double nsPerEntity = result.mMedian * 1000000.0 / ENTITY_COUNT;
		return string.Format("{0,-20} Median:{1,9:F3} ms | Min:{2,8:F3} | Max:{3,8:F3} | {4,8:F3} ns/entity", name, result.mMedian, result.mMin, result.mMax, nsPerEntity);
	}
	private static double ratio(double value, double baseline)
	{
		if (baseline <= 0.0)
		{
			return 0.0;
		}
		return value / baseline;
	}
}