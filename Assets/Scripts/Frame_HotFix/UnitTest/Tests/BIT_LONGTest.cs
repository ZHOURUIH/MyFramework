using System;
using System.Collections.Generic;
using static TestAssert;

public class BIT_LONGTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToLong();
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
		testLongArithmeticWithImplicit();
		testLongComparisons();
		testLongInList();
		testLongEquality();
		testLongHashCode();
		testLongToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testLongMaxValueOperations();
		testLongMinValueOperations();
		testLongZeroOperations();
		testImplicitConversionToInt();
		testImplicitConversionToFloat();
		testLongRoundTripViaInt();
		testLongVeryLargeValues();
		testLongVerySmallValues();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(1234567890123L);
		assertEqual(1234567890123L, instance.mValue);
		instance.resetProperty();
		assertEqual(0L, instance.mValue);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_LONG instance = new BIT_LONG();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(0L);
		assertEqual(0L, instance.mValue);
		instance.set(1L);
		assertEqual(1L, instance.mValue);
		instance.set(-1L);
		assertEqual(-1L, instance.mValue);
		instance.set(long.MaxValue);
		assertEqual(long.MaxValue, instance.mValue);
		instance.set(long.MinValue);
		assertEqual(long.MinValue, instance.mValue);
		instance.set(123456789012345L);
		assertEqual(123456789012345L, instance.mValue);
		instance.set(-987654321098765L);
		assertEqual(-987654321098765L, instance.mValue);
	}
	private static void testImplicitConversionToLong()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(100L);
		long val = instance;
		assertEqual(100L, val);
		instance.set(0L);
		long valZero = instance;
		assertEqual(0L, valZero);
		instance.set(-50L);
		long valNeg = instance;
		assertEqual(-50L, valNeg);
		instance.set(long.MaxValue);
		long valMax = instance;
		assertEqual(long.MaxValue, valMax);
		instance.set(long.MinValue);
		long valMin = instance;
		assertEqual(long.MinValue, valMin);
	}
	private static void testMultipleInstances()
	{
		BIT_LONG a = new BIT_LONG();
		BIT_LONG b = new BIT_LONG();
		BIT_LONG c = new BIT_LONG();
		a.set(1L); b.set(2L); c.set(3L);
		assertEqual(1L, a.mValue);
		assertEqual(2L, b.mValue);
		assertEqual(3L, c.mValue);
		a.set(100L);
		assertEqual(100L, a.mValue);
		assertEqual(2L, b.mValue);
		assertEqual(3L, c.mValue);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(999L);
		instance.resetProperty();
		assertEqual(0L, instance.mValue);
		instance.resetProperty();
		assertEqual(0L, instance.mValue);
		instance.resetProperty();
		assertEqual(0L, instance.mValue);
	}
	private static void testNegativeValues()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(-1L);
		assertEqual(-1L, instance.mValue);
		instance.set(long.MinValue);
		assertEqual(long.MinValue, instance.mValue);
		instance.set(-12345L);
		assertEqual(-12345L, instance.mValue);
		instance.set(-1L);
		assertEqual(-1L, instance.mValue);
		instance.set(-5555L);
		assertEqual(-5555L, instance.mValue);
	}
	private static void testMaxMinValues()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(long.MaxValue);
		assertEqual(long.MaxValue, instance.mValue);
		instance.set(long.MinValue);
		assertEqual(long.MinValue, instance.mValue);
		instance.set(long.MaxValue);
		assertEqual(long.MaxValue, instance.mValue);
		instance.set(long.MaxValue - 1L);
		assertEqual(long.MaxValue - 1L, instance.mValue);
		instance.set(long.MinValue);
		assertEqual(long.MinValue, instance.mValue);
		instance.set(long.MinValue + 1L);
		assertEqual(long.MinValue + 1L, instance.mValue);
	}
	private static void testZeroValue()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(0L);
		assertEqual(0L, instance.mValue);
		assertEqual(0L, (long)instance);
		long zero = instance;
		assertEqual(0L, zero);
		instance.resetProperty();
		assertEqual(0L, instance.mValue);
		instance.set(0L);
		assertEqual(0L, instance.mValue);
	}
	private static void testReadMethodSignature()
	{
		Type t = typeof(BIT_LONG);
		var method = t.GetMethod("read");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testWriteMethodSignature()
	{
		Type t = typeof(BIT_LONG);
		var method = t.GetMethod("write");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(555L);
		long val = instance;
		BIT_LONG newInstance = new BIT_LONG();
		newInstance.set(val);
		assertEqual((long)instance, (long)newInstance);
		long x = instance;
		assertEqual(555L, x);
		instance.set(10L);
		long result = (long)instance * 2L;
		assertEqual(20L, result);
	}
	private static void testSetIdempotent()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(42L);
		instance.set(42L);
		assertEqual(42L, instance.mValue);
		instance.set(0L);
		instance.set(0L);
		assertEqual(0L, instance.mValue);
	}
	private static void testMixedInstances()
	{
		BIT_LONG a = new BIT_LONG();
		BIT_LONG b = new BIT_LONG();
		a.set(long.MaxValue);
		b.set(long.MinValue);
		assertEqual(long.MaxValue, a.mValue);
		assertEqual(long.MinValue, b.mValue);
		a.resetProperty();
		assertEqual(0L, a.mValue);
		assertEqual(long.MinValue, b.mValue);
	}
	private static void testResetThenSet()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.resetProperty();
		assertEqual(0L, instance.mValue);
		instance.set(100L);
		assertEqual(100L, instance.mValue);
		instance.resetProperty();
		assertEqual(0L, instance.mValue);
		instance.set(-200L);
		assertEqual(-200L, instance.mValue);
	}
	private static void testLongArithmeticWithImplicit()
	{
		BIT_LONG a = new BIT_LONG();
		BIT_LONG b = new BIT_LONG();
		a.set(10L);
		b.set(20L);
		long sum = (long)a + (long)b;
		assertEqual(30L, sum);
		long diff = (long)b - (long)a;
		assertEqual(10L, diff);
		long prod = (long)a * (long)b;
		assertEqual(200L, prod);
		long quot = (long)b / (long)a;
		assertEqual(2L, quot);
	}
	private static void testLongComparisons()
	{
		BIT_LONG a = new BIT_LONG();
		BIT_LONG b = new BIT_LONG();
		a.set(10L);
		b.set(20L);
		assertTrue((long)a < (long)b);
		assertTrue((long)b > (long)a);
		a.set(10L);
		b.set(10L);
		assertTrue((long)a == (long)b);
	}
	private static void testLongInList()
	{
		List<BIT_LONG> list = new List<BIT_LONG>();
		for (int i = 0; i < 5; i++)
		{
			BIT_LONG item = new BIT_LONG();
			item.set(i * 100L);
			list.Add(item);
		}
		assertEqual(5, list.Count);
		assertEqual(0L, (long)list[0]);
		assertEqual(100L, (long)list[1]);
		assertEqual(400L, (long)list[4]);
	}
	private static void testLongEquality()
	{
		BIT_LONG a = new BIT_LONG();
		BIT_LONG b = new BIT_LONG();
		a.set(42L);
		b.set(42L);
		assertEqual((long)a, (long)b);
	}
	private static void testLongHashCode()
	{
		BIT_LONG a = new BIT_LONG();
		a.set(42L);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testLongToString()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(42L);
		string str = instance.mValue.ToString();
		assertEqual("42", str);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_LONG a = new BIT_LONG();
		BIT_LONG b = new BIT_LONG();
		a.set(long.MaxValue);
		b.set(long.MinValue);
		assertEqual(long.MaxValue, a.mValue);
		assertEqual(long.MinValue, b.mValue);
		a.resetProperty();
		assertEqual(0L, a.mValue);
		assertEqual(long.MinValue, b.mValue);
	}
	private static void testSetAfterReset()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.resetProperty();
		assertEqual(0L, instance.mValue);
		instance.set(123456789L);
		assertEqual(123456789L, instance.mValue);
	}
	private static void testMultipleResets()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(999L);
		instance.resetProperty();
		assertEqual(0L, instance.mValue);
		instance.resetProperty();
		assertEqual(0L, instance.mValue);
		instance.resetProperty();
		assertEqual(0L, instance.mValue);
	}
	private static void testLongMaxValueOperations()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(long.MaxValue);
		assertEqual(long.MaxValue, instance.mValue);
		long val = instance;
		assertEqual(long.MaxValue, val);
	}
	private static void testLongMinValueOperations()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(long.MinValue);
		assertEqual(long.MinValue, instance.mValue);
		long val = instance;
		assertEqual(long.MinValue, val);
	}
	private static void testLongZeroOperations()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(0L);
		assertEqual(0L, instance.mValue);
		long val = instance;
		assertEqual(0L, val);
	}
	private static void testImplicitConversionToInt()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(42L);
		int intVal = (int)(long)instance;
		assertEqual(42, intVal);
	}
	private static void testImplicitConversionToFloat()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(100L);
		float fltVal = (long)instance;
		assertTrue(Math.Abs(100.0f - fltVal) < 0.001f);
	}
	private static void testLongRoundTripViaInt()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(42L);
		int intVal = (int)(long)instance;
		BIT_LONG newInstance = new BIT_LONG();
		newInstance.set((long)intVal);
		assertEqual((long)instance, (long)newInstance);
	}
	private static void testLongVeryLargeValues()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(999999999999999L);
		assertTrue(instance.mValue > 999999999999998L);
	}
	private static void testLongVerySmallValues()
	{
		BIT_LONG instance = new BIT_LONG();
		instance.set(-999999999999999L);
		assertTrue(instance.mValue < -999999999999998L);
	}
}