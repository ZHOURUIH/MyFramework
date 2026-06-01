#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using static TestAssert;
using static BIT_FLOATS;

public class BIT_FLOATSTest
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
		testFloatSpecialValues();
		testFloatNaNValues();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(42.0f);
		assertEqual(1, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testAddAndCount()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(0.0f);
		assertEqual(1, instance.Count);
		assertFloatEqual(0.0f, instance[0], 0.0001f);
		instance.add(1.0f);
		assertEqual(2, instance.Count);
		assertFloatEqual(1.0f, instance[1], 0.0001f);
		instance.add(-1.0f);
		assertEqual(3, instance.Count);
		assertFloatEqual(-1.0f, instance[2], 0.0001f);
		instance.add(3.14f);
		assertEqual(4, instance.Count);
		assertFloatEqual(3.14f, instance[3], 0.0001f);
	}
	private static void testAddRangeAndCount()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.addRange(new float[] { 1.0f, 2.0f, 3.0f });
		assertEqual(3, instance.Count);
		assertFloatEqual(1.0f, instance[0], 0.0001f);
		assertFloatEqual(2.0f, instance[1], 0.0001f);
		assertFloatEqual(3.0f, instance[2], 0.0001f);
	}
	private static void testIndexerGetAndSet()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(10.0f);
		instance.add(20.0f);
		instance.add(30.0f);
		assertFloatEqual(10.0f, instance[0], 0.0001f);
		assertFloatEqual(20.0f, instance[1], 0.0001f);
		assertFloatEqual(30.0f, instance[2], 0.0001f);
		instance[0] = 100.0f;
		assertFloatEqual(100.0f, instance[0], 0.0001f);
	}
	private static void testImplicitConversionToList()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(1.0f);
		instance.add(2.0f);
		List<float> list = instance;
		assertEqual(2, list.Count);
		assertFloatEqual(1.0f, list[0], 0.0001f);
		assertFloatEqual(2.0f, list[1], 0.0001f);
	}
	private static void testMultipleInstances()
	{
		BIT_FLOATS a = new BIT_FLOATS();
		BIT_FLOATS b = new BIT_FLOATS();
		a.add(1.0f); b.add(2.0f);
		assertEqual(1, a.Count);
		assertEqual(1, b.Count);
		assertFloatEqual(1.0f, a[0], 0.0001f);
		assertFloatEqual(2.0f, b[0], 0.0001f);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(999.0f);
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_FLOATS);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(555.0f);
		List<float> list = instance;
		BIT_FLOATS newInstance = new BIT_FLOATS();
		newInstance.addRange(list);
		assertEqual(instance.Count, newInstance.Count);
	}
	private static void testAddIdempotent()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(42.0f);
		assertEqual(1, instance.Count);
		assertFloatEqual(42.0f, instance[0], 0.0001f);
	}
	private static void testMixedInstances()
	{
		BIT_FLOATS a = new BIT_FLOATS();
		BIT_FLOATS b = new BIT_FLOATS();
		a.add(1.0f); a.add(2.0f);
		b.add(3.0f);
		assertEqual(2, a.Count);
		assertEqual(1, b.Count);
	}
	private static void testResetThenAdd()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add(100.0f);
		assertEqual(1, instance.Count);
		assertFloatEqual(100.0f, instance[0], 0.0001f);
	}
	private static void testListOperationsAdd()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.mValue.Add(1.0f);
		instance.mValue.Add(2.0f);
		assertEqual(2, instance.Count);
		assertFloatEqual(1.0f, instance.mValue[0], 0.0001f);
	}
	private static void testListOperationsRemove()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(1.0f);
		instance.add(2.0f);
		instance.add(3.0f);
		instance.mValue.Remove(2.0f);
		assertEqual(2, instance.Count);
		assertFloatEqual(1.0f, instance[0], 0.0001f);
		assertFloatEqual(3.0f, instance[1], 0.0001f);
	}
	private static void testListOperationsClear()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(1.0f);
		instance.add(2.0f);
		instance.mValue.Clear();
		assertEqual(0, instance.Count);
	}
	private static void testListOperationsContains()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(1.0f);
		instance.add(2.0f);
		assertTrue(instance.mValue.Contains(1.0f));
		assertTrue(instance.mValue.Contains(2.0f));
		assertFalse(instance.mValue.Contains(3.0f));
	}
	private static void testListOperationsIndexOf()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(10.0f);
		instance.add(20.0f);
		assertEqual(0, instance.mValue.IndexOf(10.0f));
		assertEqual(1, instance.mValue.IndexOf(20.0f));
		assertEqual(-1, instance.mValue.IndexOf(30.0f));
	}
	private static void testListOperationsInsert()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(1.0f);
		instance.add(3.0f);
		instance.mValue.Insert(1, 2.0f);
		assertEqual(3, instance.Count);
		assertFloatEqual(1.0f, instance[0], 0.0001f);
		assertFloatEqual(2.0f, instance[1], 0.0001f);
		assertFloatEqual(3.0f, instance[2], 0.0001f);
	}
	private static void testListOperationsRemoveAt()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(1.0f);
		instance.add(2.0f);
		instance.add(3.0f);
		instance.mValue.RemoveAt(1);
		assertEqual(2, instance.Count);
		assertFloatEqual(1.0f, instance[0], 0.0001f);
		assertFloatEqual(3.0f, instance[1], 0.0001f);
	}
	private static void testListInCollection()
	{
		List<BIT_FLOATS> list = new List<BIT_FLOATS>();
		for (int i = 0; i < 3; i++)
		{
			BIT_FLOATS item = new BIT_FLOATS();
			item.add(i * 1.5f);
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testListEquality()
	{
		BIT_FLOATS a = new BIT_FLOATS();
		BIT_FLOATS b = new BIT_FLOATS();
		a.add(42.0f); b.add(42.0f);
		assertEqual(a.Count, b.Count);
	}
	private static void testListHashCode()
	{
		BIT_FLOATS a = new BIT_FLOATS();
		a.add(42.0f);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testListToString()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(42.0f);
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testEnumerator()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(1.0f);
		instance.add(2.0f);
		instance.add(3.0f);
		float sum = 0.0f;
		foreach (float val in instance)
		{
			sum += val;
		}
		assertFloatEqual(6.0f, sum, 0.0001f);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_FLOATS a = new BIT_FLOATS();
		BIT_FLOATS b = new BIT_FLOATS();
		a.add(1.0f); a.add(2.0f);
		b.add(3.0f); b.add(4.0f);
		assertEqual(2, a.Count);
		assertEqual(2, b.Count);
	}
	private static void testSetAfterReset()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add(123.456f);
		assertEqual(1, instance.Count);
	}
	private static void testMultipleResets()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(999.0f);
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testLargeList()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		for (int i = 0; i < 100; i++)
		{
			instance.add(i * 1.0f);
		}
		assertEqual(100, instance.Count);
		assertFloatEqual(0.0f, instance[0], 0.0001f);
		assertFloatEqual(99.0f, instance[99], 0.0001f);
	}
	private static void testEmptyList()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		assertEqual(0, instance.Count);
	}
	private static void testSingleElementList()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(42.0f);
		assertEqual(1, instance.Count);
		assertFloatEqual(42.0f, instance[0], 0.0001f);
	}
	private static void testDuplicateValues()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(42.0f);
		instance.add(42.0f);
		instance.add(42.0f);
		assertEqual(3, instance.Count);
		assertFloatEqual(42.0f, instance[0], 0.0001f);
		assertFloatEqual(42.0f, instance[1], 0.0001f);
		assertFloatEqual(42.0f, instance[2], 0.0001f);
	}
	private static void testFloatSpecialValues()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(float.MaxValue);
		instance.add(float.MinValue);
		instance.add(float.Epsilon);
		assertEqual(3, instance.Count);
		assertTrue(instance[0] > 0);
		assertTrue(instance[1] < 0);
		assertTrue(instance[2] > 0);
	}
	private static void testFloatNaNValues()
	{
		BIT_FLOATS instance = new BIT_FLOATS();
		instance.add(float.NaN);
		assertTrue(float.IsNaN(instance[0]));
	}
	private static void assertFloatEqual(float expected, float actual, float tolerance)
	{
		assertTrue(Math.Abs(expected - actual) <= tolerance,
			"Expected " + expected.ToString() + ", got " + actual.ToString());
	}
}
#endif
