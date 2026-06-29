using System;
using System.Collections.Generic;
using static TestAssert;

public class BIT_INTTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToInt();
		testMultipleInstances();
		testResetPropertyIdempotent();
		testNegativeValues();
		testMaxMinValues();
		testZeroValue();
		testReadMethodSignature();
		testWriteMethodSignature();
		testImplicitConversionEdgeCases();
		testSetIdempotent();
		testMixedInstances();
		testResetThenSet();
		testIntArithmeticWithImplicit();
		testIntComparisons();
		testIntInList();
		testIntEquality();
		testIntHashCode();
		testIntToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testIntMaxValueOperations();
		testIntMinValueOperations();
		testIntZeroOperations();
		testImplicitConversionToLong();
		testImplicitConversionToFloat();
		testIntRoundTripViaLong();
		testIntInArray();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_INT instance = new();
		instance.set(42);
		assertEqual(42, (int)instance);
		instance.resetProperty();
		assertEqual(0, instance.mValue);
		assertEqual(0, (int)instance);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_INT instance = new BIT_INT();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_INT instance = new BIT_INT();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_INT instance = new();
		instance.set(0);
		assertEqual(0, instance.mValue);
		assertEqual(0, (int)instance);
		instance.set(1);
		assertEqual(1, instance.mValue);
		instance.set(-1);
		assertEqual(-1, instance.mValue);
		instance.set(int.MaxValue);
		assertEqual(int.MaxValue, instance.mValue);
		instance.set(int.MinValue);
		assertEqual(int.MinValue, instance.mValue);
		instance.set(123456789);
		assertEqual(123456789, instance.mValue);
		instance.set(-987654321);
		assertEqual(-987654321, instance.mValue);
	}
	private static void testImplicitConversionToInt()
	{
		BIT_INT instance = new();
		instance.set(100);
		int val = instance;
		assertEqual(100, val);
		instance.set(0);
		int valZero = instance;
		assertEqual(0, valZero);
		instance.set(-50);
		int valNeg = instance;
		assertEqual(-50, valNeg);
		instance.set(int.MaxValue);
		int valMax = instance;
		assertEqual(int.MaxValue, valMax);
		instance.set(int.MinValue);
		int valMin = instance;
		assertEqual(int.MinValue, valMin);
	}
	private static void testMultipleInstances()
	{
		BIT_INT a = new();
		BIT_INT b = new();
		BIT_INT c = new();
		a.set(1);
		b.set(2);
		c.set(3);
		assertEqual(1, a.mValue);
		assertEqual(2, b.mValue);
		assertEqual(3, c.mValue);
		a.set(100);
		assertEqual(100, a.mValue);
		assertEqual(2, b.mValue);
		assertEqual(3, c.mValue);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_INT instance = new();
		instance.set(999);
		instance.resetProperty();
		assertEqual(0, instance.mValue);
		instance.resetProperty();
		assertEqual(0, instance.mValue);
		instance.resetProperty();
		assertEqual(0, instance.mValue);
	}
	private static void testNegativeValues()
	{
		BIT_INT instance = new();
		instance.set(-1);
		assertEqual(-1, instance.mValue);
		instance.set(int.MinValue);
		assertEqual(int.MinValue, instance.mValue);
		instance.set(-12345);
		assertEqual(-12345, instance.mValue);
	}
	private static void testMaxMinValues()
	{
		BIT_INT instance = new();
		instance.set(int.MaxValue);
		assertEqual(int.MaxValue, instance.mValue);
		instance.set(int.MinValue);
		assertEqual(int.MinValue, instance.mValue);
		instance.set(int.MaxValue - 1);
		assertEqual(int.MaxValue - 1, instance.mValue);
		instance.set(int.MinValue + 1);
		assertEqual(int.MinValue + 1, instance.mValue);
	}
	private static void testZeroValue()
	{
		BIT_INT instance = new();
		instance.set(0);
		assertEqual(0, instance.mValue);
		assertEqual(0, (int)instance);
		int zero = instance;
		assertEqual(0, zero);
		instance.resetProperty();
		assertEqual(0, instance.mValue);
		instance.set(0);
		assertEqual(0, instance.mValue);
	}
	private static void testReadMethodSignature()
	{
		Type t = typeof(BIT_INT);
		var method = t.GetMethod("read");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testWriteMethodSignature()
	{
		Type t = typeof(BIT_INT);
		var method = t.GetMethod("write");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_INT instance = new();
		instance.set(555);
		int val = instance;
		BIT_INT newInstance = new();
		newInstance.set(val);
		assertEqual((int)instance, (int)newInstance);
		int x = instance;
		assertEqual(555, x);
		instance.set(10);
		int result = (int)instance * 2;
		assertEqual(20, result);
	}
	private static void testSetIdempotent()
	{
		BIT_INT instance = new();
		instance.set(42);
		instance.set(42);
		assertEqual(42, instance.mValue);
		instance.set(0);
		instance.set(0);
		assertEqual(0, instance.mValue);
	}
	private static void testMixedInstances()
	{
		BIT_INT a = new();
		BIT_INT b = new();
		a.set(int.MaxValue);
		b.set(int.MinValue);
		assertEqual(int.MaxValue, a.mValue);
		assertEqual(int.MinValue, b.mValue);
		a.resetProperty();
		assertEqual(0, a.mValue);
		assertEqual(int.MinValue, b.mValue);
	}
	private static void testResetThenSet()
	{
		BIT_INT instance = new();
		instance.resetProperty();
		assertEqual(0, instance.mValue);
		instance.set(100);
		assertEqual(100, instance.mValue);
		instance.resetProperty();
		assertEqual(0, instance.mValue);
		instance.set(-200);
		assertEqual(-200, instance.mValue);
	}
	private static void testIntArithmeticWithImplicit()
	{
		BIT_INT a = new();
		BIT_INT b = new();
		a.set(10);
		b.set(20);
		int sum = (int)a + (int)b;
		assertEqual(30, sum);
		int diff = (int)b - (int)a;
		assertEqual(10, diff);
		int prod = (int)a * (int)b;
		assertEqual(200, prod);
	}
	private static void testIntComparisons()
	{
		BIT_INT a = new();
		BIT_INT b = new();
		a.set(10);
		b.set(20);
		assertTrue((int)a < (int)b);
		assertTrue((int)b > (int)a);
		a.set(10);
		b.set(10);
		assertEqual((int)a, (int)b);
	}
	private static void testIntInList()
	{
		List<BIT_INT> list = new List<BIT_INT>();
		for (int i = 0; i < 5; i++)
		{
			BIT_INT item = new BIT_INT();
			item.set(i * 100);
			list.Add(item);
		}
		assertEqual(5, list.Count);
		assertEqual(0, (int)list[0]);
		assertEqual(100, (int)list[1]);
		assertEqual(400, (int)list[4]);
	}
	private static void testIntEquality()
	{
		BIT_INT a = new BIT_INT();
		BIT_INT b = new BIT_INT();
		a.set(42);
		b.set(42);
		assertEqual((int)a, (int)b);
	}
	private static void testIntHashCode()
	{
		BIT_INT a = new BIT_INT();
		a.set(42);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testIntToString()
	{
		BIT_INT instance = new BIT_INT();
		instance.set(42);
		string str = instance.mValue.ToString();
		assertEqual("42", str);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_INT a = new BIT_INT();
		BIT_INT b = new BIT_INT();
		a.set(int.MaxValue);
		b.set(int.MinValue);
		assertEqual(int.MaxValue, a.mValue);
		assertEqual(int.MinValue, b.mValue);
		a.resetProperty();
		assertEqual(0, a.mValue);
		assertEqual(int.MinValue, b.mValue);
	}
	private static void testSetAfterReset()
	{
		BIT_INT instance = new BIT_INT();
		instance.resetProperty();
		assertEqual(0, instance.mValue);
		instance.set(123456789);
		assertEqual(123456789, instance.mValue);
	}
	private static void testMultipleResets()
	{
		BIT_INT instance = new BIT_INT();
		instance.set(999);
		instance.resetProperty();
		assertEqual(0, instance.mValue);
		instance.resetProperty();
		assertEqual(0, instance.mValue);
	}
	private static void testIntMaxValueOperations()
	{
		BIT_INT instance = new BIT_INT();
		instance.set(int.MaxValue);
		assertEqual(int.MaxValue, instance.mValue);
		int val = instance;
		assertEqual(int.MaxValue, val);
	}
	private static void testIntMinValueOperations()
	{
		BIT_INT instance = new BIT_INT();
		instance.set(int.MinValue);
		assertEqual(int.MinValue, instance.mValue);
		int val = instance;
		assertEqual(int.MinValue, val);
	}
	private static void testIntZeroOperations()
	{
		BIT_INT instance = new BIT_INT();
		instance.set(0);
		assertEqual(0, instance.mValue);
		int val = instance;
		assertEqual(0, val);
	}
	private static void testImplicitConversionToLong()
	{
		BIT_INT instance = new BIT_INT();
		instance.set(42);
		long longVal = (int)instance;
		assertEqual(42L, longVal);
	}
	private static void testImplicitConversionToFloat()
	{
		BIT_INT instance = new BIT_INT();
		instance.set(100);
		float fltVal = (int)instance;
		assertTrue(Math.Abs(100.0f - fltVal) < 0.001f);
	}
	private static void testIntRoundTripViaLong()
	{
		BIT_INT instance = new BIT_INT();
		instance.set(42);
		int iVal = instance;
		BIT_INT newInstance = new BIT_INT();
		newInstance.set(iVal);
		assertEqual((int)instance, (int)newInstance);
	}
	private static void testIntInArray()
	{
		BIT_INT[] arr = new BIT_INT[3];
		for (int i = 0; i < 3; i++)
		{
			arr[i] = new BIT_INT();
			arr[i].set(i * 10);
		}
		assertEqual(0, (int)arr[0]);
		assertEqual(10, (int)arr[1]);
		assertEqual(20, (int)arr[2]);
	}
}