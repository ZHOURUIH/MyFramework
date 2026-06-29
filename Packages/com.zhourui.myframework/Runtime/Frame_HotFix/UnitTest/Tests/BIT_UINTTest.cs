using System;
using System.Collections.Generic;
using static TestAssert;

public class BIT_UINTTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToUInt();
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
		testUIntArithmeticWithImplicit();
		testUIntComparisons();
		testUIntInList();
		testUIntEquality();
		testUIntHashCode();
		testUIntToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testUIntMaxValueOperations();
		testUIntZeroOperations();
		testImplicitConversionToInt();
		testImplicitConversionToLong();
		testImplicitConversionToFloat();
		testUIntRoundTripViaInt();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(42u);
		assertEqual(42u, instance.mValue);
		instance.resetProperty();
		assertEqual(0u, instance.mValue);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_UINT instance = new BIT_UINT();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(0u);
		assertEqual(0u, instance.mValue);
		instance.set(1u);
		assertEqual(1u, instance.mValue);
		instance.set(uint.MaxValue);
		assertEqual(uint.MaxValue, instance.mValue);
		instance.set(1234567890u);
		assertEqual(1234567890u, instance.mValue);
		instance.set(4000000000u);
		assertEqual(4000000000u, instance.mValue);
	}
	private static void testImplicitConversionToUInt()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(100u);
		uint val = instance;
		assertEqual(100u, val);
		instance.set(0u);
		uint valZero = instance;
		assertEqual(0u, valZero);
		instance.set(uint.MaxValue);
		uint valMax = instance;
		assertEqual(uint.MaxValue, valMax);
	}
	private static void testMultipleInstances()
	{
		BIT_UINT a = new BIT_UINT();
		BIT_UINT b = new BIT_UINT();
		BIT_UINT c = new BIT_UINT();
		a.set(1u); b.set(2u); c.set(3u);
		assertEqual(1u, a.mValue);
		assertEqual(2u, b.mValue);
		assertEqual(3u, c.mValue);
		a.set(100u);
		assertEqual(100u, a.mValue);
		assertEqual(2u, b.mValue);
		assertEqual(3u, c.mValue);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(999u);
		instance.resetProperty();
		assertEqual(0u, instance.mValue);
		instance.resetProperty();
		assertEqual(0u, instance.mValue);
	}
	private static void testMaxMinValues()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(uint.MaxValue);
		assertEqual(uint.MaxValue, instance.mValue);
		instance.set(0u);
		assertEqual(0u, instance.mValue);
		instance.set(uint.MaxValue);
		assertEqual(uint.MaxValue, instance.mValue);
		instance.set(uint.MaxValue - 1u);
		assertEqual(uint.MaxValue - 1u, instance.mValue);
	}
	private static void testZeroValue()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(0u);
		assertEqual(0u, instance.mValue);
		assertEqual(0u, (uint)instance);
		uint zero = instance;
		assertEqual(0u, zero);
		instance.resetProperty();
		assertEqual(0u, instance.mValue);
		instance.set(0u);
		assertEqual(0u, instance.mValue);
	}
	private static void testReadMethodSignature()
	{
		Type t = typeof(BIT_UINT);
		var method = t.GetMethod("read");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testWriteMethodSignature()
	{
		Type t = typeof(BIT_UINT);
		var method = t.GetMethod("write");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(555u);
		uint val = instance;
		BIT_UINT newInstance = new BIT_UINT();
		newInstance.set(val);
		assertEqual((uint)instance, (uint)newInstance);
		uint x = instance;
		assertEqual(555u, x);
		instance.set(10u);
		uint result = (uint)instance * 2u;
		assertEqual(20u, result);
	}
	private static void testSetIdempotent()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(42u);
		instance.set(42u);
		assertEqual(42u, instance.mValue);
		instance.set(0u);
		instance.set(0u);
		assertEqual(0u, instance.mValue);
	}
	private static void testMixedInstances()
	{
		BIT_UINT a = new BIT_UINT();
		BIT_UINT b = new BIT_UINT();
		a.set(uint.MaxValue);
		b.set(0u);
		assertEqual(uint.MaxValue, a.mValue);
		assertEqual(0u, b.mValue);
		a.resetProperty();
		assertEqual(0u, a.mValue);
		assertEqual(0u, b.mValue);
	}
	private static void testResetThenSet()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.resetProperty();
		assertEqual(0u, instance.mValue);
		instance.set(100u);
		assertEqual(100u, instance.mValue);
		instance.resetProperty();
		assertEqual(0u, instance.mValue);
		instance.set(200u);
		assertEqual(200u, instance.mValue);
	}
	private static void testUIntArithmeticWithImplicit()
	{
		BIT_UINT a = new BIT_UINT();
		BIT_UINT b = new BIT_UINT();
		a.set(10u); b.set(20u);
		uint sum = (uint)a + (uint)b;
		assertEqual(30u, sum);
		uint diff = (uint)b - (uint)a;
		assertEqual(10u, diff);
		uint prod = (uint)a * (uint)b;
		assertEqual(200u, prod);
	}
	private static void testUIntComparisons()
	{
		BIT_UINT a = new BIT_UINT();
		BIT_UINT b = new BIT_UINT();
		a.set(10u); b.set(20u);
		assertTrue((uint)a < (uint)b);
		assertTrue((uint)b > (uint)a);
		a.set(10u); b.set(10u);
		assertEqual((uint)a, (uint)b);
	}
	private static void testUIntInList()
	{
		List<BIT_UINT> list = new List<BIT_UINT>();
		for (int i = 0; i < 5; i++)
		{
			BIT_UINT item = new BIT_UINT();
			item.set((uint)(i * 100));
			list.Add(item);
		}
		assertEqual(5, list.Count);
		assertEqual(0u, (uint)list[0]);
		assertEqual(100u, (uint)list[1]);
		assertEqual(400u, (uint)list[4]);
	}
	private static void testUIntEquality()
	{
		BIT_UINT a = new BIT_UINT();
		BIT_UINT b = new BIT_UINT();
		a.set(42u); b.set(42u);
		assertEqual((uint)a, (uint)b);
	}
	private static void testUIntHashCode()
	{
		BIT_UINT a = new BIT_UINT();
		a.set(42u);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testUIntToString()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(42u);
		string str = instance.mValue.ToString();
		assertEqual("42", str);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_UINT a = new BIT_UINT();
		BIT_UINT b = new BIT_UINT();
		a.set(uint.MaxValue);
		b.set(0u);
		assertEqual(uint.MaxValue, a.mValue);
		assertEqual(0u, b.mValue);
		a.resetProperty();
		assertEqual(0u, a.mValue);
		assertEqual(0u, b.mValue);
	}
	private static void testSetAfterReset()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.resetProperty();
		assertEqual(0u, instance.mValue);
		instance.set(1234567890u);
		assertEqual(1234567890u, instance.mValue);
	}
	private static void testMultipleResets()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(999u);
		instance.resetProperty();
		assertEqual(0u, instance.mValue);
		instance.resetProperty();
		assertEqual(0u, instance.mValue);
	}
	private static void testUIntMaxValueOperations()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(uint.MaxValue);
		assertEqual(uint.MaxValue, instance.mValue);
		uint val = instance;
		assertEqual(uint.MaxValue, val);
	}
	private static void testUIntZeroOperations()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(0u);
		assertEqual(0u, instance.mValue);
		uint val = instance;
		assertEqual(0u, val);
	}
	private static void testImplicitConversionToInt()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(42u);
		int intVal = (int)(uint)instance;
		assertEqual(42, intVal);
	}
	private static void testImplicitConversionToLong()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(42u);
		long longVal = (uint)instance;
		assertEqual(42L, longVal);
	}
	private static void testImplicitConversionToFloat()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(100u);
		float fltVal = (uint)instance;
		assertTrue(Math.Abs(100.0f - fltVal) < 0.001f);
	}
	private static void testUIntRoundTripViaInt()
	{
		BIT_UINT instance = new BIT_UINT();
		instance.set(42u);
		uint uVal = instance;
		BIT_UINT newInstance = new BIT_UINT();
		newInstance.set(uVal);
		assertEqual((uint)instance, (uint)newInstance);
	}
}