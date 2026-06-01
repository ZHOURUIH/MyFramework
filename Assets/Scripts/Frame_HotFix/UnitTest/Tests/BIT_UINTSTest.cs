#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using static TestAssert;
using static BIT_UINTS;

public class BIT_UINTSTest
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
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(42u);
		assertEqual(1, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_UINTS instance = new BIT_UINTS();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testAddAndCount()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(0u);
		assertEqual(1, instance.Count);
		assertEqual(0u, instance[0]);
		instance.add(1u);
		assertEqual(2, instance.Count);
		assertEqual(1u, instance[1]);
		instance.add(uint.MaxValue);
		assertEqual(3, instance.Count);
		assertEqual(uint.MaxValue, instance[2]);
	}
	private static void testAddRangeAndCount()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.addRange(new uint[] { 1u, 2u, 3u });
		assertEqual(3, instance.Count);
		assertEqual(1u, instance[0]);
		assertEqual(2u, instance[1]);
		assertEqual(3u, instance[2]);
	}
	private static void testIndexerGetAndSet()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(10u);
		instance.add(20u);
		instance.add(30u);
		assertEqual(10u, instance[0]);
		assertEqual(20u, instance[1]);
		assertEqual(30u, instance[2]);
		instance[0] = 100u;
		assertEqual(100u, instance[0]);
	}
	private static void testImplicitConversionToList()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(1u);
		instance.add(2u);
		List<uint> list = instance;
		assertEqual(2, list.Count);
		assertEqual(1u, list[0]);
		assertEqual(2u, list[1]);
	}
	private static void testMultipleInstances()
	{
		BIT_UINTS a = new BIT_UINTS();
		BIT_UINTS b = new BIT_UINTS();
		a.add(1u); b.add(2u);
		assertEqual(1, a.Count);
		assertEqual(1, b.Count);
		assertEqual(1u, a[0]);
		assertEqual(2u, b[0]);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(999u);
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_UINTS);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(555u);
		List<uint> list = instance;
		BIT_UINTS newInstance = new BIT_UINTS();
		newInstance.addRange(list);
		assertEqual(instance.Count, newInstance.Count);
	}
	private static void testAddIdempotent()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(42u);
		assertEqual(1, instance.Count);
		assertEqual(42u, instance[0]);
	}
	private static void testMixedInstances()
	{
		BIT_UINTS a = new BIT_UINTS();
		BIT_UINTS b = new BIT_UINTS();
		a.add(1u); a.add(2u);
		b.add(3u);
		assertEqual(2, a.Count);
		assertEqual(1, b.Count);
	}
	private static void testResetThenAdd()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add(100u);
		assertEqual(1, instance.Count);
		assertEqual(100u, instance[0]);
	}
	private static void testListOperationsAdd()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.mValue.Add(1u);
		instance.mValue.Add(2u);
		assertEqual(2, instance.Count);
	}
	private static void testListOperationsRemove()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(1u);
		instance.add(2u);
		instance.add(3u);
		instance.mValue.Remove(2u);
		assertEqual(2, instance.Count);
		assertEqual(1u, instance[0]);
		assertEqual(3u, instance[1]);
	}
	private static void testListOperationsClear()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(1u);
		instance.add(2u);
		instance.mValue.Clear();
		assertEqual(0, instance.Count);
	}
	private static void testListOperationsContains()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(1u);
		instance.add(2u);
		assertTrue(instance.mValue.Contains(1u));
		assertFalse(instance.mValue.Contains(4u));
	}
	private static void testListOperationsIndexOf()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(10u);
		instance.add(20u);
		assertEqual(0, instance.mValue.IndexOf(10u));
		assertEqual(1, instance.mValue.IndexOf(20u));
		assertEqual(-1, instance.mValue.IndexOf(30u));
	}
	private static void testListOperationsInsert()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(1u);
		instance.add(3u);
		instance.mValue.Insert(1, 2u);
		assertEqual(3, instance.Count);
		assertEqual(2u, instance[1]);
	}
	private static void testListOperationsRemoveAt()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(1u);
		instance.add(2u);
		instance.add(3u);
		instance.mValue.RemoveAt(1);
		assertEqual(2, instance.Count);
		assertEqual(1u, instance[0]);
		assertEqual(3u, instance[1]);
	}
	private static void testListOperationsCount()
	{
		BIT_UINTS instance = new BIT_UINTS();
		assertEqual(0, instance.Count);
		instance.add(1u);
		assertEqual(1, instance.Count);
	}
	private static void testListInCollection()
	{
		List<BIT_UINTS> list = new List<BIT_UINTS>();
		for (int i = 0; i < 3; i++)
		{
			BIT_UINTS item = new BIT_UINTS();
			item.add((uint)i);
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testListEquality()
	{
		BIT_UINTS a = new BIT_UINTS();
		BIT_UINTS b = new BIT_UINTS();
		a.add(42u); b.add(42u);
		assertEqual(a.Count, b.Count);
		assertEqual(a[0], b[0]);
	}
	private static void testListHashCode()
	{
		BIT_UINTS a = new BIT_UINTS();
		a.add(42u);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testListToString()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(42u);
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testEnumerator()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(1u);
		instance.add(2u);
		uint sum = 0u;
		foreach (uint val in instance)
		{
			sum += val;
		}
		assertEqual(3u, sum);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_UINTS a = new BIT_UINTS();
		BIT_UINTS b = new BIT_UINTS();
		a.add(1u); a.add(2u);
		b.add(3u); b.add(4u);
		assertEqual(2, a.Count);
		assertEqual(2, b.Count);
	}
	private static void testSetAfterReset()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add(123u);
		assertEqual(1, instance.Count);
	}
	private static void testMultipleResets()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(999u);
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testLargeList()
	{
		BIT_UINTS instance = new BIT_UINTS();
		for (int i = 0; i < 100; i++)
		{
			instance.add((uint)i);
		}
		assertEqual(100, instance.Count);
	}
	private static void testEmptyList()
	{
		BIT_UINTS instance = new BIT_UINTS();
		assertEqual(0, instance.Count);
	}
	private static void testSingleElementList()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(42u);
		assertEqual(1, instance.Count);
		assertEqual(42u, instance[0]);
	}
	private static void testDuplicateValues()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(42u);
		instance.add(42u);
		assertEqual(2, instance.Count);
	}
	private static void testMaxValue()
	{
		BIT_UINTS instance = new BIT_UINTS();
		instance.add(uint.MaxValue);
		assertEqual(uint.MaxValue, instance[0]);
	}
}
#endif
