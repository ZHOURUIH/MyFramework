using System;
using System.Collections.Generic;
using static TestAssert;

public class BIT_INTSTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testAddAndCount();
		testAddRangeAndCount();
		testIndexerGetAndSet();
		testImplicitConversionToList();
		testMultipleInstances();
		testResetPropertyIdempotent();
		testMethodSignatures();
		testImplicitConversionEdgeCases();
		testAddIdempotent();
		testMixedInstances();
		testResetThenAdd();
		testListOperationsAdd();
		testListOperationsRemove();
		testListOperationsClear();
		testListOperationsContains();
		testListOperationsIndexOf();
		testListOperationsInsert();
		testListOperationsRemoveAt();
		testListOperationsCount();
		testListInCollection();
		testListEquality();
		testListHashCode();
		testListToString();
		testEnumerator();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testLargeList();
		testEmptyList();
		testSingleElementList();
		testDuplicateValues();
		testNegativeValues();
		testMaxMinValues();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(42);
		assertEqual(1, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_INTS instance = new BIT_INTS();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testAddAndCount()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(0);
		assertEqual(1, instance.Count);
		assertEqual(0, instance[0]);
		instance.add(1);
		assertEqual(2, instance.Count);
		assertEqual(1, instance[1]);
		instance.add(-1);
		assertEqual(3, instance.Count);
		assertEqual(-1, instance[2]);
		instance.add(int.MaxValue);
		assertEqual(4, instance.Count);
		assertEqual(int.MaxValue, instance[3]);
		instance.add(int.MinValue);
		assertEqual(5, instance.Count);
		assertEqual(int.MinValue, instance[4]);
	}
	private static void testAddRangeAndCount()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.addRange(new int[] { 1, 2, 3 });
		assertEqual(3, instance.Count);
		assertEqual(1, instance[0]);
		assertEqual(2, instance[1]);
		assertEqual(3, instance[2]);
		instance.addRange(new int[] { -1, -2, -3 });
		assertEqual(6, instance.Count);
		assertEqual(-1, instance[3]);
		assertEqual(-2, instance[4]);
		assertEqual(-3, instance[5]);
	}
	private static void testIndexerGetAndSet()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(10);
		instance.add(20);
		instance.add(30);
		assertEqual(10, instance[0]);
		assertEqual(20, instance[1]);
		assertEqual(30, instance[2]);
		instance[0] = 100;
		assertEqual(100, instance[0]);
		instance[1] = 200;
		assertEqual(200, instance[1]);
		instance[2] = 300;
		assertEqual(300, instance[2]);
	}
	private static void testImplicitConversionToList()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(1);
		instance.add(2);
		instance.add(3);
		List<int> list = instance;
		assertEqual(3, list.Count);
		assertEqual(1, list[0]);
		assertEqual(2, list[1]);
		assertEqual(3, list[2]);
	}
	private static void testMultipleInstances()
	{
		BIT_INTS a = new BIT_INTS();
		BIT_INTS b = new BIT_INTS();
		BIT_INTS c = new BIT_INTS();
		a.add(1); b.add(2); c.add(3);
		assertEqual(1, a.Count);
		assertEqual(1, b.Count);
		assertEqual(1, c.Count);
		assertEqual(1, a[0]);
		assertEqual(2, b[0]);
		assertEqual(3, c[0]);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(999);
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_INTS);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		assertEqual(2, readMethod.GetParameters().Length);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
		assertEqual(2, writeMethod.GetParameters().Length);
		var resetMethod = t.GetMethod("resetProperty");
		assertNotNull(resetMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(555);
		List<int> list = instance;
		BIT_INTS newInstance = new BIT_INTS();
		newInstance.addRange(list);
		assertEqual(instance.Count, newInstance.Count);
		assertEqual(instance[0], newInstance[0]);
	}
	private static void testAddIdempotent()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(42);
		assertEqual(1, instance.Count);
		assertEqual(42, instance[0]);
	}
	private static void testMixedInstances()
	{
		BIT_INTS a = new BIT_INTS();
		BIT_INTS b = new BIT_INTS();
		a.add(1); b.add(2); a.add(3);
		assertEqual(2, a.Count);
		assertEqual(1, b.Count);
		assertEqual(1, a[0]);
		assertEqual(3, a[1]);
		assertEqual(2, b[0]);
	}
	private static void testResetThenAdd()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add(100);
		assertEqual(1, instance.Count);
		assertEqual(100, instance[0]);
	}
	private static void testListOperationsAdd()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.mValue.Add(1);
		instance.mValue.Add(2);
		instance.mValue.Add(3);
		assertEqual(3, instance.Count);
		assertEqual(1, instance.mValue[0]);
		assertEqual(2, instance.mValue[1]);
		assertEqual(3, instance.mValue[2]);
	}
	private static void testListOperationsRemove()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(1);
		instance.add(2);
		instance.add(3);
		assertEqual(3, instance.Count);
		instance.mValue.Remove(2);
		assertEqual(2, instance.Count);
		assertEqual(1, instance[0]);
		assertEqual(3, instance[1]);
	}
	private static void testListOperationsClear()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(1);
		instance.add(2);
		assertEqual(2, instance.Count);
		instance.mValue.Clear();
		assertEqual(0, instance.Count);
	}
	private static void testListOperationsContains()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(1);
		instance.add(2);
		instance.add(3);
		assertTrue(instance.mValue.Contains(1));
		assertTrue(instance.mValue.Contains(2));
		assertTrue(instance.mValue.Contains(3));
		assertFalse(instance.mValue.Contains(4));
	}
	private static void testListOperationsIndexOf()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(10);
		instance.add(20);
		instance.add(30);
		assertEqual(0, instance.mValue.IndexOf(10));
		assertEqual(1, instance.mValue.IndexOf(20));
		assertEqual(2, instance.mValue.IndexOf(30));
		assertEqual(-1, instance.mValue.IndexOf(40));
	}
	private static void testListOperationsInsert()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(1);
		instance.add(3);
		instance.mValue.Insert(1, 2);
		assertEqual(3, instance.Count);
		assertEqual(1, instance[0]);
		assertEqual(2, instance[1]);
		assertEqual(3, instance[2]);
	}
	private static void testListOperationsRemoveAt()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(1);
		instance.add(2);
		instance.add(3);
		instance.mValue.RemoveAt(1);
		assertEqual(2, instance.Count);
		assertEqual(1, instance[0]);
		assertEqual(3, instance[1]);
	}
	private static void testListOperationsCount()
	{
		BIT_INTS instance = new BIT_INTS();
		assertEqual(0, instance.Count);
		instance.add(1);
		assertEqual(1, instance.Count);
		instance.add(2);
		assertEqual(2, instance.Count);
	}
	private static void testListInCollection()
	{
		List<BIT_INTS> list = new List<BIT_INTS>();
		for (int i = 0; i < 3; i++)
		{
			BIT_INTS item = new BIT_INTS();
			item.add(i);
			list.Add(item);
		}
		assertEqual(3, list.Count);
		assertEqual(0, list[0][0]);
		assertEqual(1, list[1][0]);
		assertEqual(2, list[2][0]);
	}
	private static void testListEquality()
	{
		BIT_INTS a = new BIT_INTS();
		BIT_INTS b = new BIT_INTS();
		a.add(42); b.add(42);
		assertEqual(a.Count, b.Count);
		assertEqual(a[0], b[0]);
	}
	private static void testListHashCode()
	{
		BIT_INTS a = new BIT_INTS();
		a.add(42);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testListToString()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(42);
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testEnumerator()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(1);
		instance.add(2);
		instance.add(3);
		int sum = 0;
		foreach (int val in instance)
		{
			sum += val;
		}
		assertEqual(6, sum);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_INTS a = new BIT_INTS();
		BIT_INTS b = new BIT_INTS();
		a.add(1); a.add(2);
		b.add(3); b.add(4);
		assertEqual(2, a.Count);
		assertEqual(2, b.Count);
		assertEqual(1, a[0]);
		assertEqual(2, a[1]);
		assertEqual(3, b[0]);
		assertEqual(4, b[1]);
	}
	private static void testSetAfterReset()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add(123);
		assertEqual(1, instance.Count);
		assertEqual(123, instance[0]);
	}
	private static void testMultipleResets()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(999);
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testLargeList()
	{
		BIT_INTS instance = new BIT_INTS();
		for (int i = 0; i < 100; i++)
		{
			instance.add(i);
		}
		assertEqual(100, instance.Count);
		assertEqual(0, instance[0]);
		assertEqual(99, instance[99]);
	}
	private static void testEmptyList()
	{
		BIT_INTS instance = new BIT_INTS();
		assertEqual(0, instance.Count);
	}
	private static void testSingleElementList()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(42);
		assertEqual(1, instance.Count);
		assertEqual(42, instance[0]);
	}
	private static void testDuplicateValues()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(42);
		instance.add(42);
		instance.add(42);
		assertEqual(3, instance.Count);
		assertEqual(42, instance[0]);
		assertEqual(42, instance[1]);
		assertEqual(42, instance[2]);
	}
	private static void testNegativeValues()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(-1);
		instance.add(-100);
		instance.add(int.MinValue);
		assertEqual(3, instance.Count);
		assertEqual(-1, instance[0]);
		assertEqual(-100, instance[1]);
		assertEqual(int.MinValue, instance[2]);
	}
	private static void testMaxMinValues()
	{
		BIT_INTS instance = new BIT_INTS();
		instance.add(int.MaxValue);
		instance.add(int.MinValue);
		assertEqual(2, instance.Count);
		assertEqual(int.MaxValue, instance[0]);
		assertEqual(int.MinValue, instance[1]);
	}
}