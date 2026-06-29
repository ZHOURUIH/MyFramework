using System;
using System.Collections.Generic;
using static TestAssert;

public class BIT_USHORTSTest
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
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)42);
		assertEqual(1, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testAddAndCount()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)0);
		assertEqual(1, instance.Count);
		instance.add((ushort)1);
		assertEqual(2, instance.Count);
		instance.add(ushort.MaxValue);
		assertEqual(3, instance.Count);
		assertEqual(ushort.MaxValue, instance[2]);
	}
	private static void testAddRangeAndCount()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.addRange(new ushort[] { 1, 2, 3 });
		assertEqual(3, instance.Count);
		assertEqual((ushort)1, instance[0]);
	}
	private static void testIndexerGetAndSet()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)10);
		instance.add((ushort)20);
		instance[0] = (ushort)100;
		assertEqual((ushort)100, instance[0]);
	}
	private static void testImplicitConversionToList()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)1);
		instance.add((ushort)2);
		List<ushort> list = instance;
		assertEqual(2, list.Count);
	}
	private static void testMultipleInstances()
	{
		BIT_USHORTS a = new BIT_USHORTS();
		BIT_USHORTS b = new BIT_USHORTS();
		a.add((ushort)1); b.add((ushort)2);
		assertEqual(1, a.Count);
		assertEqual((ushort)2, b[0]);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)999);
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_USHORTS);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)555);
		List<ushort> list = instance;
		BIT_USHORTS newInstance = new BIT_USHORTS();
		newInstance.addRange(list);
		assertEqual(instance.Count, newInstance.Count);
	}
	private static void testAddIdempotent()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)42);
		assertEqual(1, instance.Count);
	}
	private static void testMixedInstances()
	{
		BIT_USHORTS a = new BIT_USHORTS();
		BIT_USHORTS b = new BIT_USHORTS();
		a.add((ushort)1); a.add((ushort)2);
		b.add((ushort)3);
		assertEqual(2, a.Count);
		assertEqual(1, b.Count);
	}
	private static void testResetThenAdd()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add((ushort)100);
		assertEqual(1, instance.Count);
	}
	private static void testListOperationsAdd()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.mValue.Add((ushort)1);
		instance.mValue.Add((ushort)2);
		assertEqual(2, instance.Count);
	}
	private static void testListOperationsRemove()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)1);
		instance.add((ushort)2);
		instance.add((ushort)3);
		instance.mValue.Remove((ushort)2);
		assertEqual(2, instance.Count);
	}
	private static void testListOperationsClear()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)1);
		instance.add((ushort)2);
		instance.mValue.Clear();
		assertEqual(0, instance.Count);
	}
	private static void testListOperationsContains()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)1);
		instance.add((ushort)2);
		assertTrue(instance.mValue.Contains((ushort)1));
		assertFalse(instance.mValue.Contains((ushort)4));
	}
	private static void testListOperationsIndexOf()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)10);
		instance.add((ushort)20);
		assertEqual(0, instance.mValue.IndexOf((ushort)10));
		assertEqual(1, instance.mValue.IndexOf((ushort)20));
	}
	private static void testListOperationsInsert()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)1);
		instance.add((ushort)3);
		instance.mValue.Insert(1, (ushort)2);
		assertEqual(3, instance.Count);
		assertEqual((ushort)2, instance[1]);
	}
	private static void testListOperationsRemoveAt()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)1);
		instance.add((ushort)2);
		instance.add((ushort)3);
		instance.mValue.RemoveAt(1);
		assertEqual(2, instance.Count);
	}
	private static void testListOperationsCount()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		assertEqual(0, instance.Count);
		instance.add((ushort)1);
		assertEqual(1, instance.Count);
	}
	private static void testListInCollection()
	{
		List<BIT_USHORTS> list = new List<BIT_USHORTS>();
		for (int i = 0; i < 3; i++)
		{
			BIT_USHORTS item = new BIT_USHORTS();
			item.add((ushort)i);
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testListEquality()
	{
		BIT_USHORTS a = new BIT_USHORTS();
		BIT_USHORTS b = new BIT_USHORTS();
		a.add((ushort)42); b.add((ushort)42);
		assertEqual(a.Count, b.Count);
		assertEqual(a[0], b[0]);
	}
	private static void testListHashCode()
	{
		BIT_USHORTS a = new BIT_USHORTS();
		a.add((ushort)42);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testListToString()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)42);
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testEnumerator()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)1);
		instance.add((ushort)2);
		instance.add((ushort)3);
		int sum = 0;
		foreach (ushort val in instance)
		{
			sum += val;
		}
		assertEqual(6, sum);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_USHORTS a = new BIT_USHORTS();
		BIT_USHORTS b = new BIT_USHORTS();
		a.add((ushort)1); a.add((ushort)2);
		b.add((ushort)3); b.add((ushort)4);
		assertEqual(2, a.Count);
		assertEqual(2, b.Count);
	}
	private static void testSetAfterReset()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add((ushort)123);
		assertEqual(1, instance.Count);
	}
	private static void testMultipleResets()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)999);
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testLargeList()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		for (int i = 0; i < 100; i++)
		{
			instance.add((ushort)(i % 65536));
		}
		assertEqual(100, instance.Count);
	}
	private static void testEmptyList()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		assertEqual(0, instance.Count);
	}
	private static void testSingleElementList()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)42);
		assertEqual(1, instance.Count);
		assertEqual((ushort)42, instance[0]);
	}
	private static void testDuplicateValues()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add((ushort)42);
		instance.add((ushort)42);
		assertEqual(2, instance.Count);
	}
	private static void testMaxValue()
	{
		BIT_USHORTS instance = new BIT_USHORTS();
		instance.add(ushort.MaxValue);
		assertEqual(ushort.MaxValue, instance[0]);
	}
}