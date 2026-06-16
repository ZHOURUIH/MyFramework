using System;
using System.Collections.Generic;
using static TestAssert;

public class BIT_SHORTSTest
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
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)42);
		assertEqual(1, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testAddAndCount()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)0);
		assertEqual(1, instance.Count);
		assertEqual((short)0, instance[0]);
		instance.add((short)1);
		assertEqual(2, instance.Count);
		assertEqual((short)1, instance[1]);
		instance.add((short)-1);
		assertEqual(3, instance.Count);
		assertEqual((short)-1, instance[2]);
		instance.add(short.MaxValue);
		assertEqual(4, instance.Count);
		assertEqual(short.MaxValue, instance[3]);
	}
	private static void testAddRangeAndCount()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.addRange(new short[] { 1, 2, 3 });
		assertEqual(3, instance.Count);
		assertEqual((short)1, instance[0]);
		assertEqual((short)2, instance[1]);
		assertEqual((short)3, instance[2]);
	}
	private static void testIndexerGetAndSet()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)10);
		instance.add((short)20);
		instance.add((short)30);
		assertEqual((short)10, instance[0]);
		assertEqual((short)20, instance[1]);
		assertEqual((short)30, instance[2]);
		instance[0] = (short)100;
		assertEqual((short)100, instance[0]);
	}
	private static void testImplicitConversionToList()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)1);
		instance.add((short)2);
		List<short> list = instance;
		assertEqual(2, list.Count);
		assertEqual((short)1, list[0]);
		assertEqual((short)2, list[1]);
	}
	private static void testMultipleInstances()
	{
		BIT_SHORTS a = new BIT_SHORTS();
		BIT_SHORTS b = new BIT_SHORTS();
		a.add((short)1); b.add((short)2);
		assertEqual(1, a.Count);
		assertEqual(1, b.Count);
		assertEqual((short)1, a[0]);
		assertEqual((short)2, b[0]);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)999);
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_SHORTS);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)555);
		List<short> list = instance;
		BIT_SHORTS newInstance = new BIT_SHORTS();
		newInstance.addRange(list);
		assertEqual(instance.Count, newInstance.Count);
	}
	private static void testAddIdempotent()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)42);
		assertEqual(1, instance.Count);
		assertEqual((short)42, instance[0]);
	}
	private static void testMixedInstances()
	{
		BIT_SHORTS a = new BIT_SHORTS();
		BIT_SHORTS b = new BIT_SHORTS();
		a.add((short)1); a.add((short)2);
		b.add((short)3);
		assertEqual(2, a.Count);
		assertEqual(1, b.Count);
	}
	private static void testResetThenAdd()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add((short)100);
		assertEqual(1, instance.Count);
		assertEqual((short)100, instance[0]);
	}
	private static void testListOperationsAdd()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.mValue.Add((short)1);
		instance.mValue.Add((short)2);
		assertEqual(2, instance.Count);
	}
	private static void testListOperationsRemove()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)1);
		instance.add((short)2);
		instance.add((short)3);
		instance.mValue.Remove((short)2);
		assertEqual(2, instance.Count);
		assertEqual((short)1, instance[0]);
		assertEqual((short)3, instance[1]);
	}
	private static void testListOperationsClear()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)1);
		instance.add((short)2);
		instance.mValue.Clear();
		assertEqual(0, instance.Count);
	}
	private static void testListOperationsContains()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)1);
		instance.add((short)2);
		assertTrue(instance.mValue.Contains((short)1));
		assertFalse(instance.mValue.Contains((short)4));
	}
	private static void testListOperationsIndexOf()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)10);
		instance.add((short)20);
		assertEqual(0, instance.mValue.IndexOf((short)10));
		assertEqual(1, instance.mValue.IndexOf((short)20));
	}
	private static void testListOperationsInsert()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)1);
		instance.add((short)3);
		instance.mValue.Insert(1, (short)2);
		assertEqual(3, instance.Count);
		assertEqual((short)2, instance[1]);
	}
	private static void testListOperationsRemoveAt()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)1);
		instance.add((short)2);
		instance.add((short)3);
		instance.mValue.RemoveAt(1);
		assertEqual(2, instance.Count);
		assertEqual((short)1, instance[0]);
		assertEqual((short)3, instance[1]);
	}
	private static void testListOperationsCount()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		assertEqual(0, instance.Count);
		instance.add((short)1);
		assertEqual(1, instance.Count);
	}
	private static void testListInCollection()
	{
		List<BIT_SHORTS> list = new List<BIT_SHORTS>();
		for (int i = 0; i < 3; i++)
		{
			BIT_SHORTS item = new BIT_SHORTS();
			item.add((short)i);
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testListEquality()
	{
		BIT_SHORTS a = new BIT_SHORTS();
		BIT_SHORTS b = new BIT_SHORTS();
		a.add((short)42); b.add((short)42);
		assertEqual(a.Count, b.Count);
		assertEqual(a[0], b[0]);
	}
	private static void testListHashCode()
	{
		BIT_SHORTS a = new BIT_SHORTS();
		a.add((short)42);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testListToString()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)42);
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testEnumerator()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)1);
		instance.add((short)2);
		instance.add((short)3);
		short sum = 0;
		foreach (short val in instance)
		{
			sum += val;
		}
		assertEqual((short)6, sum);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_SHORTS a = new BIT_SHORTS();
		BIT_SHORTS b = new BIT_SHORTS();
		a.add((short)1); a.add((short)2);
		b.add((short)3); b.add((short)4);
		assertEqual(2, a.Count);
		assertEqual(2, b.Count);
	}
	private static void testSetAfterReset()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add((short)123);
		assertEqual(1, instance.Count);
	}
	private static void testMultipleResets()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)999);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testLargeList()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		for (int i = 0; i < 100; i++)
		{
			instance.add((short)i);
		}
		assertEqual(100, instance.Count);
	}
	private static void testEmptyList()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		assertEqual(0, instance.Count);
	}
	private static void testSingleElementList()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)42);
		assertEqual(1, instance.Count);
		assertEqual((short)42, instance[0]);
	}
	private static void testDuplicateValues()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)42);
		instance.add((short)42);
		assertEqual(2, instance.Count);
	}
	private static void testNegativeValues()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add((short)-1);
		instance.add(short.MinValue);
		assertEqual((short)-1, instance[0]);
		assertEqual(short.MinValue, instance[1]);
	}
	private static void testMaxMinValues()
	{
		BIT_SHORTS instance = new BIT_SHORTS();
		instance.add(short.MaxValue);
		instance.add(short.MinValue);
		assertEqual(short.MaxValue, instance[0]);
		assertEqual(short.MinValue, instance[1]);
	}
}