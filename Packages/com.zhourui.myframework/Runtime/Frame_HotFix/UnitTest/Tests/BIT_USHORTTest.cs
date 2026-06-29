using System;
using System.Collections.Generic;
using static TestAssert;

public class BIT_USHORTTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToUShort();
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
		testUShortArithmeticWithImplicit();
		testUShortComparisons();
		testUShortInList();
		testUShortEquality();
		testUShortHashCode();
		testUShortToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testUShortMaxValueOperations();
		testUShortZeroOperations();
		testImplicitConversionToInt();
		testImplicitConversionToLong();
		testImplicitConversionToFloat();
		testUShortRoundTripViaInt();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set((ushort)42);
		assertEqual((ushort)42, instance.mValue);
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_USHORT instance = new BIT_USHORT();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set((ushort)0);
		assertEqual((ushort)0, instance.mValue);
		instance.set((ushort)1);
		assertEqual((ushort)1, instance.mValue);
		instance.set(ushort.MaxValue);
		assertEqual(ushort.MaxValue, instance.mValue);
		instance.set((ushort)12345);
		assertEqual((ushort)12345, instance.mValue);
		instance.set((ushort)65535);
		assertEqual((ushort)65535, instance.mValue);
	}
	private static void testImplicitConversionToUShort()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set((ushort)100);
		ushort val = instance;
		assertEqual((ushort)100, val);
		instance.set((ushort)0);
		ushort valZero = instance;
		assertEqual((ushort)0, valZero);
		instance.set(ushort.MaxValue);
		ushort valMax = instance;
		assertEqual(ushort.MaxValue, valMax);
	}
	private static void testMultipleInstances()
	{
		BIT_USHORT a = new BIT_USHORT();
		BIT_USHORT b = new BIT_USHORT();
		BIT_USHORT c = new BIT_USHORT();
		a.set((ushort)1); b.set((ushort)2); c.set((ushort)3);
		assertEqual((ushort)1, a.mValue);
		assertEqual((ushort)2, b.mValue);
		assertEqual((ushort)3, c.mValue);
		a.set((ushort)100);
		assertEqual((ushort)100, a.mValue);
		assertEqual((ushort)2, b.mValue);
		assertEqual((ushort)3, c.mValue);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set((ushort)99);
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue);
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue);
	}
	private static void testMaxMinValues()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set(ushort.MaxValue);
		assertEqual(ushort.MaxValue, instance.mValue);
		instance.set((ushort)0);
		assertEqual((ushort)0, instance.mValue);
		instance.set((ushort)65535);
		assertEqual((ushort)65535, instance.mValue);
		instance.set((ushort)65534);
		assertEqual((ushort)65534, instance.mValue);
	}
	private static void testZeroValue()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set((ushort)0);
		assertEqual((ushort)0, instance.mValue);
		assertEqual((ushort)0, (ushort)instance);
		ushort zero = instance;
		assertEqual((ushort)0, zero);
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue);
		instance.set((ushort)0);
		assertEqual((ushort)0, instance.mValue);
	}
	private static void testReadMethodSignature()
	{
		Type t = typeof(BIT_USHORT);
		var method = t.GetMethod("read");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testWriteMethodSignature()
	{
		Type t = typeof(BIT_USHORT);
		var method = t.GetMethod("write");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set((ushort)555);
		ushort val = instance;
		BIT_USHORT newInstance = new BIT_USHORT();
		newInstance.set(val);
		assertEqual((ushort)instance, (ushort)newInstance);
		ushort x = instance;
		assertEqual((ushort)555, x);
		instance.set((ushort)10);
		ushort result = (ushort)((ushort)instance * (ushort)2);
		assertEqual((ushort)20, result);
	}
	private static void testSetIdempotent()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set((ushort)42);
		instance.set((ushort)42);
		assertEqual((ushort)42, instance.mValue);
		instance.set((ushort)0);
		instance.set((ushort)0);
		assertEqual((ushort)0, instance.mValue);
	}
	private static void testMixedInstances()
	{
		BIT_USHORT a = new BIT_USHORT();
		BIT_USHORT b = new BIT_USHORT();
		a.set(ushort.MaxValue);
		b.set((ushort)0);
		assertEqual(ushort.MaxValue, a.mValue);
		assertEqual((ushort)0, b.mValue);
		a.resetProperty();
		assertEqual((ushort)0, a.mValue);
		assertEqual((ushort)0, b.mValue);
	}
	private static void testResetThenSet()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue);
		instance.set((ushort)100);
		assertEqual((ushort)100, instance.mValue);
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue);
		instance.set((ushort)200);
		assertEqual((ushort)200, instance.mValue);
	}
	private static void testUShortArithmeticWithImplicit()
	{
		BIT_USHORT a = new BIT_USHORT();
		BIT_USHORT b = new BIT_USHORT();
		a.set((ushort)10); b.set((ushort)20);
		int sum = (ushort)a + (ushort)b;
		assertEqual(30, sum);
		int diff = (ushort)b - (ushort)a;
		assertEqual(10, diff);
		int prod = (ushort)a * (ushort)b;
		assertEqual(200, prod);
	}
	private static void testUShortComparisons()
	{
		BIT_USHORT a = new BIT_USHORT();
		BIT_USHORT b = new BIT_USHORT();
		a.set((ushort)10); b.set((ushort)20);
		assertTrue((ushort)a < (ushort)b);
		assertTrue((ushort)b > (ushort)a);
		a.set((ushort)10); b.set((ushort)10);
		assertEqual((ushort)a, (ushort)b);
	}
	private static void testUShortInList()
	{
		List<BIT_USHORT> list = new List<BIT_USHORT>();
		for (int i = 0; i < 5; i++)
		{
			BIT_USHORT item = new BIT_USHORT();
			item.set((ushort)(i * 100));
			list.Add(item);
		}
		assertEqual(5, list.Count);
		assertEqual((ushort)0, (ushort)list[0]);
		assertEqual((ushort)100, (ushort)list[1]);
		assertEqual((ushort)400, (ushort)list[4]);
	}
	private static void testUShortEquality()
	{
		BIT_USHORT a = new BIT_USHORT();
		BIT_USHORT b = new BIT_USHORT();
		a.set((ushort)42); b.set((ushort)42);
		assertEqual((ushort)a, (ushort)b);
	}
	private static void testUShortHashCode()
	{
		BIT_USHORT a = new BIT_USHORT();
		a.set((ushort)42);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testUShortToString()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set((ushort)42);
		string str = instance.mValue.ToString();
		assertEqual("42", str);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_USHORT a = new BIT_USHORT();
		BIT_USHORT b = new BIT_USHORT();
		a.set(ushort.MaxValue);
		b.set((ushort)0);
		assertEqual(ushort.MaxValue, a.mValue);
		assertEqual((ushort)0, b.mValue);
		a.resetProperty();
		assertEqual((ushort)0, a.mValue);
		assertEqual((ushort)0, b.mValue);
	}
	private static void testSetAfterReset()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue);
		instance.set((ushort)12345);
		assertEqual((ushort)12345, instance.mValue);
	}
	private static void testMultipleResets()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set((ushort)99);
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue);
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue);
	}
	private static void testUShortMaxValueOperations()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set(ushort.MaxValue);
		assertEqual(ushort.MaxValue, instance.mValue);
		ushort val = instance;
		assertEqual(ushort.MaxValue, val);
	}
	private static void testUShortZeroOperations()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set((ushort)0);
		assertEqual((ushort)0, instance.mValue);
		ushort val = instance;
		assertEqual((ushort)0, val);
	}
	private static void testImplicitConversionToInt()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set((ushort)42);
		int intVal = (ushort)instance;
		assertEqual(42, intVal);
	}
	private static void testImplicitConversionToLong()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set((ushort)42);
		long longVal = (ushort)instance;
		assertEqual(42L, longVal);
	}
	private static void testImplicitConversionToFloat()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set((ushort)100);
		float fltVal = (ushort)instance;
		assertTrue(Math.Abs(100.0f - fltVal) < 0.001f);
	}
	private static void testUShortRoundTripViaInt()
	{
		BIT_USHORT instance = new BIT_USHORT();
		instance.set((ushort)42);
		ushort usVal = instance;
		BIT_USHORT newInstance = new BIT_USHORT();
		newInstance.set(usVal);
		assertEqual((ushort)instance, (ushort)newInstance);
	}
}