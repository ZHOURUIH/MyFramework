using System;
using System.Collections.Generic;
using static TestAssert;
using static BIT_LONGS;

public class BIT_LONGSTest
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
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(42L);
		assertEqual(1, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_LONGS instance = new BIT_LONGS();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testAddAndCount()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(0L);
		assertEqual(1, instance.Count);
		assertEqual(0L, instance[0]);
		instance.add(1L);
		assertEqual(2, instance.Count);
		assertEqual(1L, instance[1]);
		instance.add(-1L);
		assertEqual(3, instance.Count);
		assertEqual(-1L, instance[2]);
		instance.add(long.MaxValue);
		assertEqual(4, instance.Count);
		assertEqual(long.MaxValue, instance[3]);
	}
	private static void testAddRangeAndCount()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.addRange(new long[] { 1L, 2L, 3L });
		assertEqual(3, instance.Count);
		assertEqual(1L, instance[0]);
		assertEqual(2L, instance[1]);
		assertEqual(3L, instance[2]);
	}
	private static void testIndexerGetAndSet()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(10L);
		instance.add(20L);
		instance.add(30L);
		assertEqual(10L, instance[0]);
		assertEqual(20L, instance[1]);
		assertEqual(30L, instance[2]);
		instance[0] = 100L;
		assertEqual(100L, instance[0]);
	}
	private static void testImplicitConversionToList()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(1L);
		instance.add(2L);
		List<long> list = instance;
		assertEqual(2, list.Count);
		assertEqual(1L, list[0]);
		assertEqual(2L, list[1]);
	}
	private static void testMultipleInstances()
	{
		BIT_LONGS a = new BIT_LONGS();
		BIT_LONGS b = new BIT_LONGS();
		a.add(1L); b.add(2L);
		assertEqual(1, a.Count);
		assertEqual(1, b.Count);
		assertEqual(1L, a[0]);
		assertEqual(2L, b[0]);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(999L);
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_LONGS);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(555L);
		List<long> list = instance;
		BIT_LONGS newInstance = new BIT_LONGS();
		newInstance.addRange(list);
		assertEqual(instance.Count, newInstance.Count);
	}
	private static void testAddIdempotent()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(42L);
		assertEqual(1, instance.Count);
		assertEqual(42L, instance[0]);
	}
	private static void testMixedInstances()
	{
		BIT_LONGS a = new BIT_LONGS();
		BIT_LONGS b = new BIT_LONGS();
		a.add(1L); a.add(2L);
		b.add(3L);
		assertEqual(2, a.Count);
		assertEqual(1, b.Count);
	}
	private static void testResetThenAdd()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add(100L);
		assertEqual(1, instance.Count);
		assertEqual(100L, instance[0]);
	}
	private static void testListOperationsAdd()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.mValue.Add(1L);
		instance.mValue.Add(2L);
		assertEqual(2, instance.Count);
	}
	private static void testListOperationsRemove()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(1L);
		instance.add(2L);
		instance.add(3L);
		instance.mValue.Remove(2L);
		assertEqual(2, instance.Count);
		assertEqual(1L, instance[0]);
		assertEqual(3L, instance[1]);
	}
	private static void testListOperationsClear()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(1L);
		instance.add(2L);
		instance.mValue.Clear();
		assertEqual(0, instance.Count);
	}
	private static void testListOperationsContains()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(1L);
		instance.add(2L);
		assertTrue(instance.mValue.Contains(1L));
		assertFalse(instance.mValue.Contains(4L));
	}
	private static void testListOperationsIndexOf()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(10L);
		instance.add(20L);
		assertEqual(0, instance.mValue.IndexOf(10L));
		assertEqual(1, instance.mValue.IndexOf(20L));
	}
	private static void testListOperationsInsert()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(1L);
		instance.add(3L);
		instance.mValue.Insert(1, 2L);
		assertEqual(3, instance.Count);
		assertEqual(2L, instance[1]);
	}
	private static void testListOperationsRemoveAt()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(1L);
		instance.add(2L);
		instance.add(3L);
		instance.mValue.RemoveAt(1);
		assertEqual(2, instance.Count);
		assertEqual(1L, instance[0]);
		assertEqual(3L, instance[1]);
	}
	private static void testListOperationsCount()
	{
		BIT_LONGS instance = new BIT_LONGS();
		assertEqual(0, instance.Count);
		instance.add(1L);
		assertEqual(1, instance.Count);
	}
	private static void testListInCollection()
	{
		List<BIT_LONGS> list = new List<BIT_LONGS>();
		for (int i = 0; i < 3; i++)
		{
			BIT_LONGS item = new BIT_LONGS();
			item.add(i);
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testListEquality()
	{
		BIT_LONGS a = new BIT_LONGS();
		BIT_LONGS b = new BIT_LONGS();
		a.add(42L); b.add(42L);
		assertEqual(a.Count, b.Count);
		assertEqual(a[0], b[0]);
	}
	private static void testListHashCode()
	{
		BIT_LONGS a = new BIT_LONGS();
		a.add(42L);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testListToString()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(42L);
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testEnumerator()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(1L);
		instance.add(2L);
		instance.add(3L);
		long sum = 0L;
		foreach (long val in instance)
		{
			sum += val;
		}
		assertEqual(6L, sum);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_LONGS a = new BIT_LONGS();
		BIT_LONGS b = new BIT_LONGS();
		a.add(1L); a.add(2L);
		b.add(3L); b.add(4L);
		assertEqual(2, a.Count);
		assertEqual(2, b.Count);
	}
	private static void testSetAfterReset()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add(123L);
		assertEqual(1, instance.Count);
	}
	private static void testMultipleResets()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(999L);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testLargeList()
	{
		BIT_LONGS instance = new BIT_LONGS();
		for (int i = 0; i < 100; i++)
		{
			instance.add(i);
		}
		assertEqual(100, instance.Count);
	}
	private static void testEmptyList()
	{
		BIT_LONGS instance = new BIT_LONGS();
		assertEqual(0, instance.Count);
	}
	private static void testSingleElementList()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(42L);
		assertEqual(1, instance.Count);
	}
	private static void testDuplicateValues()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(42L);
		instance.add(42L);
		assertEqual(2, instance.Count);
	}
	private static void testNegativeValues()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(-1L);
		instance.add(-100L);
		assertEqual(-1L, instance[0]);
		assertEqual(-100L, instance[1]);
	}
	private static void testMaxMinValues()
	{
		BIT_LONGS instance = new BIT_LONGS();
		instance.add(long.MaxValue);
		instance.add(long.MinValue);
		assertEqual(long.MaxValue, instance[0]);
		assertEqual(long.MinValue, instance[1]);
	}
}