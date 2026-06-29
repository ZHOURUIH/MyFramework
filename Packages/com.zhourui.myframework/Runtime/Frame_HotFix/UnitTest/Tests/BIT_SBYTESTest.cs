using System;
using System.Collections.Generic;
using static TestAssert;

public class BIT_SBYTESTest
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
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)42);
		assertEqual(1, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testAddAndCount()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)0);
		assertEqual(1, instance.Count);
		instance.add((sbyte)1);
		assertEqual(2, instance.Count);
		instance.add((sbyte)-1);
		assertEqual(3, instance.Count);
		instance.add(sbyte.MaxValue);
		assertEqual(4, instance.Count);
		instance.add(sbyte.MinValue);
		assertEqual(5, instance.Count);
	}
	private static void testAddRangeAndCount()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.addRange(new sbyte[] { 1, 2, 3 });
		assertEqual(3, instance.Count);
	}
	private static void testIndexerGetAndSet()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)10);
		instance.add((sbyte)20);
		instance.add((sbyte)30);
		instance[0] = (sbyte)100;
		assertEqual((sbyte)100, instance[0]);
	}
	private static void testImplicitConversionToList()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)1);
		instance.add((sbyte)2);
		List<sbyte> list = instance;
		assertEqual(2, list.Count);
	}
	private static void testMultipleInstances()
	{
		BIT_SBYTES a = new BIT_SBYTES();
		BIT_SBYTES b = new BIT_SBYTES();
		a.add((sbyte)1); b.add((sbyte)2);
		assertEqual(1, a.Count);
		assertEqual(1, b.Count);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)99);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_SBYTES);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)55);
		List<sbyte> list = instance;
		BIT_SBYTES newInstance = new BIT_SBYTES();
		newInstance.addRange(list);
		assertEqual(instance.Count, newInstance.Count);
	}
	private static void testAddIdempotent()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)42);
		assertEqual(1, instance.Count);
	}
	private static void testMixedInstances()
	{
		BIT_SBYTES a = new BIT_SBYTES();
		BIT_SBYTES b = new BIT_SBYTES();
		a.add((sbyte)1); a.add((sbyte)2);
		b.add((sbyte)3);
		assertEqual(2, a.Count);
		assertEqual(1, b.Count);
	}
	private static void testResetThenAdd()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add((sbyte)100);
		assertEqual(1, instance.Count);
	}
	private static void testListOperationsAdd()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.mValue.Add((sbyte)1);
		instance.mValue.Add((sbyte)2);
		assertEqual(2, instance.Count);
	}
	private static void testListOperationsRemove()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)1);
		instance.add((sbyte)2);
		instance.add((sbyte)3);
		instance.mValue.Remove((sbyte)2);
		assertEqual(2, instance.Count);
	}
	private static void testListOperationsClear()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)1);
		instance.add((sbyte)2);
		instance.mValue.Clear();
		assertEqual(0, instance.Count);
	}
	private static void testListOperationsContains()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)1);
		instance.add((sbyte)2);
		assertTrue(instance.mValue.Contains((sbyte)1));
	}
	private static void testListOperationsIndexOf()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)10);
		instance.add((sbyte)20);
		assertEqual(0, instance.mValue.IndexOf((sbyte)10));
	}
	private static void testListOperationsInsert()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)1);
		instance.add((sbyte)3);
		instance.mValue.Insert(1, (sbyte)2);
		assertEqual(3, instance.Count);
	}
	private static void testListOperationsRemoveAt()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)1);
		instance.add((sbyte)2);
		instance.add((sbyte)3);
		instance.mValue.RemoveAt(1);
		assertEqual(2, instance.Count);
	}
	private static void testListOperationsCount()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		assertEqual(0, instance.Count);
		instance.add((sbyte)1);
		assertEqual(1, instance.Count);
	}
	private static void testListInCollection()
	{
		List<BIT_SBYTES> list = new List<BIT_SBYTES>();
		for (int i = 0; i < 3; i++)
		{
			BIT_SBYTES item = new BIT_SBYTES();
			item.add((sbyte)i);
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testListEquality()
	{
		BIT_SBYTES a = new BIT_SBYTES();
		BIT_SBYTES b = new BIT_SBYTES();
		a.add((sbyte)42); b.add((sbyte)42);
		assertEqual(a.Count, b.Count);
	}
	private static void testListHashCode()
	{
		BIT_SBYTES a = new BIT_SBYTES();
		a.add((sbyte)42);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testListToString()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)42);
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testEnumerator()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)1);
		instance.add((sbyte)2);
		instance.add((sbyte)3);
		sbyte sum = 0;
		foreach (sbyte val in instance)
		{
			sum += val;
		}
		assertEqual((sbyte)6, sum);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_SBYTES a = new BIT_SBYTES();
		BIT_SBYTES b = new BIT_SBYTES();
		a.add((sbyte)1); a.add((sbyte)2);
		b.add((sbyte)3); b.add((sbyte)4);
		assertEqual(2, a.Count);
		assertEqual(2, b.Count);
	}
	private static void testSetAfterReset()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add((sbyte)123);
		assertEqual(1, instance.Count);
	}
	private static void testMultipleResets()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)99);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testLargeList()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		for (int i = 0; i < 100; i++)
		{
			instance.add((sbyte)(i % 128));
		}
		assertEqual(100, instance.Count);
	}
	private static void testEmptyList()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		assertEqual(0, instance.Count);
	}
	private static void testSingleElementList()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)42);
		assertEqual(1, instance.Count);
	}
	private static void testDuplicateValues()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)42);
		instance.add((sbyte)42);
		assertEqual(2, instance.Count);
	}
	private static void testNegativeValues()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add((sbyte)-1);
		instance.add(sbyte.MinValue);
		assertEqual((sbyte)-1, instance[0]);
		assertEqual(sbyte.MinValue, instance[1]);
	}
	private static void testMaxMinValues()
	{
		BIT_SBYTES instance = new BIT_SBYTES();
		instance.add(sbyte.MaxValue);
		instance.add(sbyte.MinValue);
		assertEqual(sbyte.MaxValue, instance[0]);
		assertEqual(sbyte.MinValue, instance[1]);
	}
}