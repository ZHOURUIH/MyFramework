using System;
using System.Collections.Generic;
using static TestAssert;
using static BIT_BYTES;

public class BIT_BYTESTest
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
		BIT_BYTES instance = new BIT_BYTES();
		instance.add((byte)42);
		assertEqual(1, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_BYTES instance = new BIT_BYTES();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_BYTES instance = new BIT_BYTES();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testAddAndCount()
	{
		BIT_BYTES instance = new BIT_BYTES();
		instance.add((byte)0);
		assertEqual(1, instance.Count);
		assertEqual((byte)0, instance[0]);
		instance.add((byte)1);
		assertEqual(2, instance.Count);
		assertEqual((byte)1, instance[1]);
		instance.add((byte)255);
		assertEqual(3, instance.Count);
		assertEqual((byte)255, instance[2]);
	}
	private static void testAddRangeAndCount()
	{
		BIT_BYTES instance = new BIT_BYTES();
		instance.addRange(new byte[] { 1, 2, 3 });
		assertEqual(3, instance.Count);
		assertEqual((byte)1, instance[0]);
		assertEqual((byte)2, instance[1]);
		assertEqual((byte)3, instance[2]);
	}
	private static void testIndexerGetAndSet()
	{
		BIT_BYTES instance = new BIT_BYTES();
		instance.add((byte)10);
		instance.add((byte)20);
		instance.add((byte)30);
		assertEqual((byte)10, instance[0]);
		assertEqual((byte)20, instance[1]);
		assertEqual((byte)30, instance[2]);
		instance[0] = (byte)100;
		assertEqual((byte)100, instance[0]);
	}
	private static void testImplicitConversionToList()
	{
		BIT_BYTES instance = new BIT_BYTES();
		instance.add((byte)1);
		instance.add((byte)2);
		List<byte> list = instance;
		assertEqual(2, list.Count);
		assertEqual((byte)1, list[0]);
		assertEqual((byte)2, list[1]);
	}
	private static void testMultipleInstances()
	{
		BIT_BYTES a = new BIT_BYTES();
		BIT_BYTES b = new BIT_BYTES();
		a.add((byte)1); b.add((byte)2);
		assertEqual(1, a.Count);
		assertEqual(1, b.Count);
		assertEqual((byte)1, a[0]);
		assertEqual((byte)2, b[0]);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_BYTES instance = new BIT_BYTES();
		instance.add((byte)99);
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_BYTES);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_BYTES instance = new BIT_BYTES();
		instance.add((byte)55);
		List<byte> list = instance;
		BIT_BYTES newInstance = new BIT_BYTES();
		newInstance.addRange(list);
		assertEqual(instance.Count, newInstance.Count);
	}
	private static void testAddIdempotent()
	{
		BIT_BYTES instance = new BIT_BYTES();
		instance.add((byte)42);
		assertEqual(1, instance.Count);
		assertEqual((byte)42, instance[0]);
	}
	private static void testMixedInstances()
	{
		BIT_BYTES a = new BIT_BYTES();
		BIT_BYTES b = new BIT_BYTES();
		a.add((byte)1); a.add((byte)2);
		b.add((byte)3);
		assertEqual(2, a.Count);
		assertEqual(1, b.Count);
	}
	private static void testResetThenAdd()
	{
		BIT_BYTES instance = new BIT_BYTES();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add((byte)100);
		assertEqual(1, instance.Count);
		assertEqual((byte)100, instance[0]);
	}
	private static void testListOperationsAdd()
	{
		BIT_BYTES instance = new BIT_BYTES();
		instance.mValue.Add((byte)1);
		instance.mValue.Add((byte)2);
		assertEqual(2, instance.Count);
	}
	private static void testListOperationsRemove()
	{
		BIT_BYTES instance = new BIT_BYTES();
		instance.add((byte)1);
		instance.add((byte)2);
		instance.add((byte)3);
		instance.mValue.Remove((byte)2);
		assertEqual(2, instance.Count);
	}
	private static void testListOperationsClear()
	{
		BIT_BYTES instance = new BIT_BYTES();
		instance.add((byte)1);
		instance.add((byte)2);
		instance.mValue.Clear();
		assertEqual(0, instance.Count);
	}
	private static void testListOperationsContains()
	{
		BIT_BYTES instance = new BIT_BYTES();
		instance.add((byte)1);
		instance.add((byte)2);
		assertTrue(instance.mValue.Contains((byte)1));
		assertFalse(instance.mValue.Contains((byte)4));
	}
	private static void testListOperationsIndexOf()
	{
		BIT_BYTES instance = new();
		instance.add((byte)10);
		instance.add((byte)20);
		assertEqual(0, instance.mValue.IndexOf((byte)10));
		assertEqual(1, instance.mValue.IndexOf((byte)20));
	}
	private static void testListOperationsInsert()
	{
		BIT_BYTES instance = new();
		instance.add(1);
		instance.add(3);
		instance.mValue.Insert(1, 2);
		assertEqual(3, instance.Count);
		assertEqual((byte)2, instance[1]);
	}
	private static void testListOperationsRemoveAt()
	{
		BIT_BYTES instance = new();
		instance.add(1);
		instance.add(2);
		instance.add(3);
		instance.mValue.RemoveAt(1);
		assertEqual(2, instance.Count);
	}
	private static void testListOperationsCount()
	{
		BIT_BYTES instance = new();
		assertEqual(0, instance.Count);
		instance.add((byte)1);
		assertEqual(1, instance.Count);
	}
	private static void testListInCollection()
	{
		List<BIT_BYTES> list = new();
		for (int i = 0; i < 3; i++)
		{
			BIT_BYTES item = new();
			item.add((byte)i);
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testListEquality()
	{
		BIT_BYTES a = new();
		BIT_BYTES b = new();
		a.add(42); 
		b.add(42);
		assertEqual(a.Count, b.Count);
		assertEqual(a[0], b[0]);
	}
	private static void testListHashCode()
	{
		BIT_BYTES a = new();
		a.add(42);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testListToString()
	{
		BIT_BYTES instance = new();
		instance.add(42);
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testEnumerator()
	{
		BIT_BYTES instance = new();
		instance.add(1);
		instance.add(2);
		instance.add(3);
		byte sum = 0;
		foreach (byte val in instance)
		{
			sum += val;
		}
		assertEqual((byte)6, sum);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_BYTES a = new();
		BIT_BYTES b = new();
		a.add(1); 
		a.add(2);
		b.add(3); 
		b.add(4);
		assertEqual(2, a.Count);
		assertEqual(2, b.Count);
	}
	private static void testSetAfterReset()
	{
		BIT_BYTES instance = new();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add(123);
		assertEqual(1, instance.Count);
	}
	private static void testMultipleResets()
	{
		BIT_BYTES instance = new();
		instance.add(99);
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testLargeList()
	{
		BIT_BYTES instance = new();
		for (int i = 0; i < 100; i++)
		{
			instance.add((byte)(i % 256));
		}
		assertEqual(100, instance.Count);
	}
	private static void testEmptyList()
	{
		BIT_BYTES instance = new();
		assertEqual(0, instance.Count);
	}
	private static void testSingleElementList()
	{
		BIT_BYTES instance = new();
		instance.add(42);
		assertEqual(1, instance.Count);
		assertEqual((byte)42, instance[0]);
	}
	private static void testDuplicateValues()
	{
		BIT_BYTES instance = new();
		instance.add(42);
		instance.add(42);
		assertEqual(2, instance.Count);
	}
	private static void testMaxValue()
	{
		BIT_BYTES instance = new();
		instance.add(byte.MaxValue);
		assertEqual(byte.MaxValue, instance[0]);
	}
}