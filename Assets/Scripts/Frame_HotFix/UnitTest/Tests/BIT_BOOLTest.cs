#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using static TestAssert;
using static BIT_BOOL;

public class BIT_BOOLTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversion();
		testMultipleInstances();
		testResetPropertyIdempotent();
		testBooleanValues();
		testMethodSignatures();
		testConversionEdgeCases();
		testSetIdempotent();
		testMixedInstances();
		testResetThenSet();
		testBoolInArithmetic();
		testBoolComparisons();
		testBoolInList();
		testBoolEquality();
		testBoolHashCode();
		testBoolToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testBoolTrueOperations();
		testBoolFalseOperations();
		testBoolNegation();
		testBoolLogicalAnd();
		testBoolLogicalOr();
		testBoolLogicalXor();
		testBoolWithConditional();
		testBoolImplicitConversionToInt();
		testBoolInArray();
		testBoolAllTrue();
		testBoolAnyTrue();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.set(true);
		assertTrue((bool)instance);
		instance.resetProperty();
		assertFalse(instance.mValue);
		assertFalse((bool)instance);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_BOOL instance = new BIT_BOOL();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.set(false);
		assertFalse(instance.mValue);
		instance.set(true);
		assertTrue(instance.mValue);
		instance.set(false);
		assertFalse(instance.mValue);
		instance.set(true);
		assertTrue((bool)instance);
	}
	private static void testImplicitConversion()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.set(true);
		bool val = instance;
		assertTrue(val);
		instance.set(false);
		bool valFalse = instance;
		assertFalse(valFalse);
	}
	private static void testMultipleInstances()
	{
		BIT_BOOL a = new BIT_BOOL();
		BIT_BOOL b = new BIT_BOOL();
		BIT_BOOL c = new BIT_BOOL();
		a.set(true);
		b.set(false);
		c.set(true);
		assertTrue(a.mValue);
		assertFalse(b.mValue);
		assertTrue(c.mValue);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.set(true);
		instance.resetProperty();
		assertFalse(instance.mValue);
		instance.resetProperty();
		assertFalse(instance.mValue);
	}
	private static void testBooleanValues()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.set(true);
		assertTrue(instance.mValue);
		assertTrue((bool)instance);
		instance.set(false);
		assertFalse(instance.mValue);
		assertFalse((bool)instance);
	}
	private static void testMethodSignatures()
	{
		System.Type t = typeof(BIT_BOOL);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		assertEqual(2, readMethod.GetParameters().Length);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
		assertEqual(2, writeMethod.GetParameters().Length);
		var resetMethod = t.GetMethod("resetProperty");
		assertNotNull(resetMethod);
	}
	private static void testConversionEdgeCases()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.set(true);
		bool val = instance;
		BIT_BOOL newInstance = new BIT_BOOL();
		newInstance.set(val);
		assertEqual((bool)instance, (bool)newInstance);
	}
	private static void testSetIdempotent()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.set(true);
		instance.set(true);
		assertTrue(instance.mValue);
		instance.set(false);
		instance.set(false);
		assertFalse(instance.mValue);
	}
	private static void testMixedInstances()
	{
		BIT_BOOL a = new BIT_BOOL();
		BIT_BOOL b = new BIT_BOOL();
		a.set(true);
		b.set(false);
		assertTrue(a.mValue);
		assertFalse(b.mValue);
		a.resetProperty();
		assertFalse(a.mValue);
		assertFalse(b.mValue);
	}
	private static void testResetThenSet()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.resetProperty();
		assertFalse(instance.mValue);
		instance.set(true);
		assertTrue(instance.mValue);
		instance.resetProperty();
		assertFalse(instance.mValue);
		instance.set(false);
		assertFalse(instance.mValue);
	}
	private static void testBoolInArithmetic()
	{
		BIT_BOOL a = new BIT_BOOL();
		a.set(true);
		int val = (bool)a ? 1 : 0;
		assertEqual(1, val);
		a.set(false);
		val = (bool)a ? 1 : 0;
		assertEqual(0, val);
	}
	private static void testBoolComparisons()
	{
		BIT_BOOL a = new BIT_BOOL();
		BIT_BOOL b = new BIT_BOOL();
		a.set(true);
		b.set(false);
		assertTrue((bool)a != (bool)b);
		a.set(false);
		b.set(false);
		assertEqual((bool)a, (bool)b);
	}
	private static void testBoolInList()
	{
		List<BIT_BOOL> list = new List<BIT_BOOL>();
		for (int i = 0; i < 5; i++)
		{
			BIT_BOOL item = new BIT_BOOL();
			item.set(i % 2 == 0);
			list.Add(item);
		}
		assertEqual(5, list.Count);
		assertTrue((bool)list[0]);
		assertFalse((bool)list[1]);
	}
	private static void testBoolEquality()
	{
		BIT_BOOL a = new BIT_BOOL();
		BIT_BOOL b = new BIT_BOOL();
		a.set(true);
		b.set(true);
		assertEqual((bool)a, (bool)b);
	}
	private static void testBoolHashCode()
	{
		BIT_BOOL a = new BIT_BOOL();
		a.set(true);
		// BIT_BOOL 是 class，未重写 GetHashCode，同一实例应返回一致的哈希码
		int hash1 = a.GetHashCode();
		assertEqual(hash1, a.GetHashCode());
	}
	private static void testBoolToString()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.set(true);
		string str = instance.mValue.ToString();
		assertEqual("True", str);
		instance.set(false);
		str = instance.mValue.ToString();
		assertEqual("False", str);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_BOOL a = new BIT_BOOL();
		BIT_BOOL b = new BIT_BOOL();
		a.set(true);
		b.set(false);
		assertTrue(a.mValue);
		assertFalse(b.mValue);
		a.resetProperty();
		assertFalse(a.mValue);
		assertFalse(b.mValue);
	}
	private static void testSetAfterReset()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.resetProperty();
		assertFalse(instance.mValue);
		instance.set(true);
		assertTrue(instance.mValue);
	}
	private static void testMultipleResets()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.set(true);
		instance.resetProperty();
		assertFalse(instance.mValue);
		instance.resetProperty();
		assertFalse(instance.mValue);
	}
	private static void testBoolTrueOperations()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.set(true);
		assertTrue(instance.mValue);
		bool val = instance;
		assertTrue(val);
	}
	private static void testBoolFalseOperations()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.set(false);
		assertFalse(instance.mValue);
		bool val = instance;
		assertFalse(val);
	}
	private static void testBoolNegation()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.set(true);
		assertFalse(!((bool)instance));
		instance.set(false);
		assertTrue(!((bool)instance));
	}
	private static void testBoolLogicalAnd()
	{
		BIT_BOOL a = new BIT_BOOL();
		BIT_BOOL b = new BIT_BOOL();
		a.set(true); b.set(true);
		assertTrue((bool)a && (bool)b);
		a.set(true); b.set(false);
		assertFalse((bool)a && (bool)b);
	}
	private static void testBoolLogicalOr()
	{
		BIT_BOOL a = new BIT_BOOL();
		BIT_BOOL b = new BIT_BOOL();
		a.set(true); b.set(false);
		assertTrue((bool)a || (bool)b);
		a.set(false); b.set(false);
		assertFalse((bool)a || (bool)b);
	}
	private static void testBoolLogicalXor()
	{
		BIT_BOOL a = new BIT_BOOL();
		BIT_BOOL b = new BIT_BOOL();
		a.set(true); b.set(false);
		assertTrue((bool)a ^ (bool)b);
		a.set(true); b.set(true);
		assertFalse((bool)a ^ (bool)b);
	}
	private static void testBoolWithConditional()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.set(true);
		string result = (bool)instance ? "yes" : "no";
		assertEqual("yes", result);
		instance.set(false);
		result = (bool)instance ? "yes" : "no";
		assertEqual("no", result);
	}
	private static void testBoolImplicitConversionToInt()
	{
		BIT_BOOL instance = new BIT_BOOL();
		instance.set(true);
		int val = (bool)instance ? 1 : 0;
		assertEqual(1, val);
		instance.set(false);
		val = (bool)instance ? 1 : 0;
		assertEqual(0, val);
	}
	private static void testBoolInArray()
	{
		BIT_BOOL[] arr = new BIT_BOOL[3];
		arr[0] = new BIT_BOOL();
		arr[0].set(true);
		arr[1] = new BIT_BOOL();
		arr[1].set(false);
		arr[2] = new BIT_BOOL();
		arr[2].set(true);
		assertTrue((bool)arr[0]);
		assertFalse((bool)arr[1]);
		assertTrue((bool)arr[2]);
	}
	private static void testBoolAllTrue()
	{
		List<BIT_BOOL> list = new List<BIT_BOOL>();
		for (int i = 0; i < 3; i++)
		{
			BIT_BOOL item = new BIT_BOOL();
			item.set(true);
			list.Add(item);
		}
		bool allTrue = true;
		foreach (var item in list)
		{
			if (!(bool)item) { allTrue = false; break; }
		}
		assertTrue(allTrue);
	}
	private static void testBoolAnyTrue()
	{
		List<BIT_BOOL> list = new List<BIT_BOOL>();
		BIT_BOOL a = new BIT_BOOL();
		a.set(false);
		BIT_BOOL b = new BIT_BOOL();
		b.set(true);
		BIT_BOOL c = new BIT_BOOL();
		c.set(false);
		list.Add(a); list.Add(b); list.Add(c);
		bool anyTrue = false;
		foreach (var item in list)
		{
			if ((bool)item) { anyTrue = true; break; }
		}
		assertTrue(anyTrue);
	}
}
#endif
