using System;
using System.Collections.Generic;
using static TestAssert;

public class BIT_ULONGSTest
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
		testMaxValue();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(42UL);
		assertEqual(1, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testAddAndCount()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(0UL);
		assertEqual(1, instance.Count);
		assertEqual(0UL, instance[0]);
		instance.add(1UL);
		assertEqual(2, instance.Count);
		assertEqual(1UL, instance[1]);
		instance.add(ulong.MaxValue);
		assertEqual(3, instance.Count);
		assertEqual(ulong.MaxValue, instance[2]);
	}
	private static void testAddRangeAndCount()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.addRange(new ulong[] { 1UL, 2UL, 3UL });
		assertEqual(3, instance.Count);
		assertEqual(1UL, instance[0]);
		assertEqual(2UL, instance[1]);
		assertEqual(3UL, instance[2]);
	}
	private static void testIndexerGetAndSet()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(10UL);
		instance.add(20UL);
		instance.add(30UL);
		assertEqual(10UL, instance[0]);
		assertEqual(20UL, instance[1]);
		assertEqual(30UL, instance[2]);
		instance[0] = 100UL;
		assertEqual(100UL, instance[0]);
	}
	private static void testImplicitConversionToList()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(1UL);
		instance.add(2UL);
		List<ulong> list = instance;
		assertEqual(2, list.Count);
		assertEqual(1UL, list[0]);
		assertEqual(2UL, list[1]);
	}
	private static void testMultipleInstances()
	{
		BIT_ULONGS a = new BIT_ULONGS();
		BIT_ULONGS b = new BIT_ULONGS();
		a.add(1UL); b.add(2UL);
		assertEqual(1, a.Count);
		assertEqual(1, b.Count);
		assertEqual(1UL, a[0]);
		assertEqual(2UL, b[0]);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(999UL);
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_ULONGS);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(555UL);
		List<ulong> list = instance;
		BIT_ULONGS newInstance = new BIT_ULONGS();
		newInstance.addRange(list);
		assertEqual(instance.Count, newInstance.Count);
		assertEqual(instance[0], newInstance[0]);
	}
	private static void testAddIdempotent()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(42UL);
		assertEqual(1, instance.Count);
		assertEqual(42UL, instance[0]);
	}
	private static void testMixedInstances()
	{
		BIT_ULONGS a = new BIT_ULONGS();
		BIT_ULONGS b = new BIT_ULONGS();
		a.add(1UL); a.add(2UL);
		b.add(3UL);
		assertEqual(2, a.Count);
		assertEqual(1, b.Count);
	}
	private static void testResetThenAdd()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add(100UL);
		assertEqual(1, instance.Count);
		assertEqual(100UL, instance[0]);
	}
	private static void testListOperationsAdd()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.mValue.Add(1UL);
		instance.mValue.Add(2UL);
		assertEqual(2, instance.Count);
	}
	private static void testListOperationsRemove()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(1UL);
		instance.add(2UL);
		instance.add(3UL);
		instance.mValue.Remove(2UL);
		assertEqual(2, instance.Count);
		assertEqual(1UL, instance[0]);
		assertEqual(3UL, instance[1]);
	}
	private static void testListOperationsClear()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(1UL);
		instance.add(2UL);
		instance.mValue.Clear();
		assertEqual(0, instance.Count);
	}
	private static void testListOperationsContains()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(1UL);
		instance.add(2UL);
		assertTrue(instance.mValue.Contains(1UL));
		assertFalse(instance.mValue.Contains(4UL));
	}
	private static void testListOperationsIndexOf()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(10UL);
		instance.add(20UL);
		assertEqual(0, instance.mValue.IndexOf(10UL));
		assertEqual(1, instance.mValue.IndexOf(20UL));
		assertEqual(-1, instance.mValue.IndexOf(30UL));
	}
	private static void testListOperationsInsert()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(1UL);
		instance.add(3UL);
		instance.mValue.Insert(1, 2UL);
		assertEqual(3, instance.Count);
		assertEqual(2UL, instance[1]);
	}
	private static void testListOperationsRemoveAt()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(1UL);
		instance.add(2UL);
		instance.add(3UL);
		instance.mValue.RemoveAt(1);
		assertEqual(2, instance.Count);
		assertEqual(1UL, instance[0]);
		assertEqual(3UL, instance[1]);
	}
	private static void testListOperationsCount()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		assertEqual(0, instance.Count);
		instance.add(1UL);
		assertEqual(1, instance.Count);
	}
	private static void testListInCollection()
	{
		List<BIT_ULONGS> list = new List<BIT_ULONGS>();
		for (int i = 0; i < 3; i++)
		{
			BIT_ULONGS item = new BIT_ULONGS();
			item.add((ulong)i);
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testListEquality()
	{
		BIT_ULONGS a = new BIT_ULONGS();
		BIT_ULONGS b = new BIT_ULONGS();
		a.add(42UL); b.add(42UL);
		assertEqual(a.Count, b.Count);
		assertEqual(a[0], b[0]);
	}
	private static void testListHashCode()
	{
		BIT_ULONGS a = new BIT_ULONGS();
		a.add(42UL);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testListToString()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(42UL);
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testEnumerator()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(1UL);
		instance.add(2UL);
		ulong sum = 0UL;
		foreach (ulong val in instance)
		{
			sum += val;
		}
		assertEqual(3UL, sum);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_ULONGS a = new BIT_ULONGS();
		BIT_ULONGS b = new BIT_ULONGS();
		a.add(1UL); a.add(2UL);
		b.add(3UL); b.add(4UL);
		assertEqual(2, a.Count);
		assertEqual(2, b.Count);
	}
	private static void testSetAfterReset()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add(123UL);
		assertEqual(1, instance.Count);
	}
	private static void testMultipleResets()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(999UL);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testLargeList()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		for (int i = 0; i < 100; i++)
		{
			instance.add((ulong)i);
		}
		assertEqual(100, instance.Count);
	}
	private static void testEmptyList()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		assertEqual(0, instance.Count);
	}
	private static void testSingleElementList()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(42UL);
		assertEqual(1, instance.Count);
	}
	private static void testDuplicateValues()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(42UL);
		instance.add(42UL);
		assertEqual(2, instance.Count);
	}
	private static void testMaxValue()
	{
		BIT_ULONGS instance = new BIT_ULONGS();
		instance.add(ulong.MaxValue);
		assertEqual(ulong.MaxValue, instance[0]);
	}
}