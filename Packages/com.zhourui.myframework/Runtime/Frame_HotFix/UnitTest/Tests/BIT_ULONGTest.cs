using System;
using System.Collections.Generic;
using static TestAssert;

public class BIT_ULONGTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToULong();
		testMultipleInstances();
		testResetPropertyIdempotent();
		testMaxMinValues();
		testZeroValue();
		testReadMethodSignature();
		testWriteMethodSignature();
		testImplicitConversionEdgeCases();
		testSetIdempotent();
		testMixedInstances();
		testResetThenSet();
		testULongArithmeticWithImplicit();
		testULongComparisons();
		testULongInList();
		testULongEquality();
		testULongHashCode();
		testULongToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testULongMaxValueOperations();
		testULongZeroOperations();
		testImplicitConversionToLong();
		testImplicitConversionToFloat();
		testULongRoundTripViaLong();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(1234567890123UL);
		assertEqual(1234567890123UL, instance.mValue);
		instance.resetProperty();
		assertEqual(0UL, instance.mValue);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_ULONG instance = new BIT_ULONG();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(0UL);
		assertEqual(0UL, instance.mValue);
		instance.set(1UL);
		assertEqual(1UL, instance.mValue);
		instance.set(ulong.MaxValue);
		assertEqual(ulong.MaxValue, instance.mValue);
		instance.set(123456789012345UL);
		assertEqual(123456789012345UL, instance.mValue);
		instance.set(999999999999999UL);
		assertEqual(999999999999999UL, instance.mValue);
	}
	private static void testImplicitConversionToULong()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(100UL);
		ulong val = instance;
		assertEqual(100UL, val);
		instance.set(0UL);
		ulong valZero = instance;
		assertEqual(0UL, valZero);
		instance.set(ulong.MaxValue);
		ulong valMax = instance;
		assertEqual(ulong.MaxValue, valMax);
	}
	private static void testMultipleInstances()
	{
		BIT_ULONG a = new BIT_ULONG();
		BIT_ULONG b = new BIT_ULONG();
		BIT_ULONG c = new BIT_ULONG();
		a.set(1UL); b.set(2UL); c.set(3UL);
		assertEqual(1UL, a.mValue);
		assertEqual(2UL, b.mValue);
		assertEqual(3UL, c.mValue);
		a.set(100UL);
		assertEqual(100UL, a.mValue);
		assertEqual(2UL, b.mValue);
		assertEqual(3UL, c.mValue);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(999UL);
		instance.resetProperty();
		assertEqual(0UL, instance.mValue);
		instance.resetProperty();
		assertEqual(0UL, instance.mValue);
	}
	private static void testMaxMinValues()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(ulong.MaxValue);
		assertEqual(ulong.MaxValue, instance.mValue);
		instance.set(0UL);
		assertEqual(0UL, instance.mValue);
		instance.set(ulong.MaxValue);
		assertEqual(ulong.MaxValue, instance.mValue);
		instance.set(ulong.MaxValue - 1UL);
		assertEqual(ulong.MaxValue - 1UL, instance.mValue);
	}
	private static void testZeroValue()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(0UL);
		assertEqual(0UL, instance.mValue);
		assertEqual(0UL, (ulong)instance);
		ulong zero = instance;
		assertEqual(0UL, zero);
		instance.resetProperty();
		assertEqual(0UL, instance.mValue);
		instance.set(0UL);
		assertEqual(0UL, instance.mValue);
	}
	private static void testReadMethodSignature()
	{
		Type t = typeof(BIT_ULONG);
		var method = t.GetMethod("read");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testWriteMethodSignature()
	{
		Type t = typeof(BIT_ULONG);
		var method = t.GetMethod("write");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(555UL);
		ulong val = instance;
		BIT_ULONG newInstance = new BIT_ULONG();
		newInstance.set(val);
		assertEqual((ulong)instance, (ulong)newInstance);
		ulong x = instance;
		assertEqual(555UL, x);
		instance.set(10UL);
		ulong result = (ulong)instance * 2UL;
		assertEqual(20UL, result);
	}
	private static void testSetIdempotent()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(42UL);
		instance.set(42UL);
		assertEqual(42UL, instance.mValue);
		instance.set(0UL);
		instance.set(0UL);
		assertEqual(0UL, instance.mValue);
	}
	private static void testMixedInstances()
	{
		BIT_ULONG a = new BIT_ULONG();
		BIT_ULONG b = new BIT_ULONG();
		a.set(ulong.MaxValue);
		b.set(0UL);
		assertEqual(ulong.MaxValue, a.mValue);
		assertEqual(0UL, b.mValue);
		a.resetProperty();
		assertEqual(0UL, a.mValue);
		assertEqual(0UL, b.mValue);
	}
	private static void testResetThenSet()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.resetProperty();
		assertEqual(0UL, instance.mValue);
		instance.set(100UL);
		assertEqual(100UL, instance.mValue);
		instance.resetProperty();
		assertEqual(0UL, instance.mValue);
		instance.set(200UL);
		assertEqual(200UL, instance.mValue);
	}
	private static void testULongArithmeticWithImplicit()
	{
		BIT_ULONG a = new BIT_ULONG();
		BIT_ULONG b = new BIT_ULONG();
		a.set(10UL); b.set(20UL);
		ulong sum = (ulong)a + (ulong)b;
		assertEqual(30UL, sum);
		ulong diff = (ulong)b - (ulong)a;
		assertEqual(10UL, diff);
		ulong prod = (ulong)a * (ulong)b;
		assertEqual(200UL, prod);
	}
	private static void testULongComparisons()
	{
		BIT_ULONG a = new BIT_ULONG();
		BIT_ULONG b = new BIT_ULONG();
		a.set(10UL); b.set(20UL);
		assertTrue((ulong)a < (ulong)b);
		assertTrue((ulong)b > (ulong)a);
		a.set(10UL); b.set(10UL);
		assertEqual((ulong)a, (ulong)b);
	}
	private static void testULongInList()
	{
		List<BIT_ULONG> list = new List<BIT_ULONG>();
		for (int i = 0; i < 5; i++)
		{
			BIT_ULONG item = new BIT_ULONG();
			item.set((ulong)(i * 100));
			list.Add(item);
		}
		assertEqual(5, list.Count);
		assertEqual(0UL, (ulong)list[0]);
		assertEqual(100UL, (ulong)list[1]);
		assertEqual(400UL, (ulong)list[4]);
	}
	private static void testULongEquality()
	{
		BIT_ULONG a = new BIT_ULONG();
		BIT_ULONG b = new BIT_ULONG();
		a.set(42UL); b.set(42UL);
		assertEqual((ulong)a, (ulong)b);
	}
	private static void testULongHashCode()
	{
		BIT_ULONG a = new BIT_ULONG();
		a.set(42UL);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testULongToString()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(42UL);
		string str = instance.mValue.ToString();
		assertEqual("42", str);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_ULONG a = new BIT_ULONG();
		BIT_ULONG b = new BIT_ULONG();
		a.set(ulong.MaxValue);
		b.set(0UL);
		assertEqual(ulong.MaxValue, a.mValue);
		assertEqual(0UL, b.mValue);
		a.resetProperty();
		assertEqual(0UL, a.mValue);
		assertEqual(0UL, b.mValue);
	}
	private static void testSetAfterReset()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.resetProperty();
		assertEqual(0UL, instance.mValue);
		instance.set(123456789UL);
		assertEqual(123456789UL, instance.mValue);
	}
	private static void testMultipleResets()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(999UL);
		instance.resetProperty();
		assertEqual(0UL, instance.mValue);
		instance.resetProperty();
		assertEqual(0UL, instance.mValue);
	}
	private static void testULongMaxValueOperations()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(ulong.MaxValue);
		assertEqual(ulong.MaxValue, instance.mValue);
		ulong val = instance;
		assertEqual(ulong.MaxValue, val);
	}
	private static void testULongZeroOperations()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(0UL);
		assertEqual(0UL, instance.mValue);
		ulong val = instance;
		assertEqual(0UL, val);
	}
	private static void testImplicitConversionToLong()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(42UL);
		ulong ulVal = instance;
		long lVal = (long)ulVal;
		assertEqual(42L, lVal);
	}
	private static void testImplicitConversionToFloat()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(100UL);
		float fltVal = (ulong)instance;
		assertTrue(Math.Abs(100.0f - fltVal) < 0.001f);
	}
	private static void testULongRoundTripViaLong()
	{
		BIT_ULONG instance = new BIT_ULONG();
		instance.set(42UL);
		ulong ulVal = instance;
		BIT_ULONG newInstance = new BIT_ULONG();
		newInstance.set(ulVal);
		assertEqual((ulong)instance, (ulong)newInstance);
	}
}