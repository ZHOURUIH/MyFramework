#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using static TestAssert;
using static BIT_SHORT;

public class BIT_SHORTTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToShort();
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
		testShortArithmeticWithImplicit();
		testShortComparisons();
		testShortInList();
		testShortEquality();
		testShortHashCode();
		testShortToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testShortMaxValueOperations();
		testShortMinValueOperations();
		testShortZeroOperations();
		testImplicitConversionToInt();
		testImplicitConversionToLong();
		testImplicitConversionToFloat();
		testShortRoundTripViaInt();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set(42);
		assertEqual((short)42, instance.mValue);
		instance.resetProperty();
		assertEqual((short)0, instance.mValue);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_SHORT instance = new BIT_SHORT();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set((short)0);
		assertEqual((short)0, instance.mValue);
		instance.set((short)1);
		assertEqual((short)1, instance.mValue);
		instance.set((short)-1);
		assertEqual((short)-1, instance.mValue);
		instance.set(short.MaxValue);
		assertEqual(short.MaxValue, instance.mValue);
		instance.set(short.MinValue);
		assertEqual(short.MinValue, instance.mValue);
		instance.set((short)12345);
		assertEqual((short)12345, instance.mValue);
		instance.set((short)-12345);
		assertEqual((short)-12345, instance.mValue);
	}
	private static void testImplicitConversionToShort()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set((short)100);
		short val = instance;
		assertEqual((short)100, val);
		instance.set((short)0);
		short valZero = instance;
		assertEqual((short)0, valZero);
		instance.set((short)-50);
		short valNeg = instance;
		assertEqual((short)-50, valNeg);
		instance.set(short.MaxValue);
		short valMax = instance;
		assertEqual(short.MaxValue, valMax);
		instance.set(short.MinValue);
		short valMin = instance;
		assertEqual(short.MinValue, valMin);
	}
	private static void testMultipleInstances()
	{
		BIT_SHORT a = new BIT_SHORT();
		BIT_SHORT b = new BIT_SHORT();
		BIT_SHORT c = new BIT_SHORT();
		a.set((short)1); b.set((short)2); c.set((short)3);
		assertEqual((short)1, a.mValue);
		assertEqual((short)2, b.mValue);
		assertEqual((short)3, c.mValue);
		a.set((short)100);
		assertEqual((short)100, a.mValue);
		assertEqual((short)2, b.mValue);
		assertEqual((short)3, c.mValue);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set((short)999);
		instance.resetProperty();
		assertEqual((short)0, instance.mValue);
		instance.resetProperty();
		assertEqual((short)0, instance.mValue);
	}
	private static void testNegativeValues()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set((short)-1);
		assertEqual((short)-1, instance.mValue);
		instance.set(short.MinValue);
		assertEqual(short.MinValue, instance.mValue);
		instance.set((short)-12345);
		assertEqual((short)-12345, instance.mValue);
	}
	private static void testMaxMinValues()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set(short.MaxValue);
		assertEqual(short.MaxValue, instance.mValue);
		instance.set(short.MinValue);
		assertEqual(short.MinValue, instance.mValue);
		instance.set((short)(short.MaxValue - 1));
		assertEqual((short)(short.MaxValue - 1), instance.mValue);
		instance.set((short)(short.MinValue + 1));
		assertEqual((short)(short.MinValue + 1), instance.mValue);
	}
	private static void testZeroValue()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set((short)0);
		assertEqual((short)0, instance.mValue);
		assertEqual((short)0, (short)instance);
		short zero = instance;
		assertEqual((short)0, zero);
		instance.resetProperty();
		assertEqual((short)0, instance.mValue);
		instance.set((short)0);
		assertEqual((short)0, instance.mValue);
	}
	private static void testReadMethodSignature()
	{
		Type t = typeof(BIT_SHORT);
		var method = t.GetMethod("read");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testWriteMethodSignature()
	{
		Type t = typeof(BIT_SHORT);
		var method = t.GetMethod("write");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set((short)555);
		short val = instance;
		BIT_SHORT newInstance = new BIT_SHORT();
		newInstance.set(val);
		assertEqual((short)instance, (short)newInstance);
		short x = instance;
		assertEqual((short)555, x);
		instance.set((short)10);
		short result = (short)((short)instance * (short)2);
		assertEqual((short)20, result);
	}
	private static void testSetIdempotent()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set((short)42);
		instance.set((short)42);
		assertEqual((short)42, instance.mValue);
		instance.set((short)0);
		instance.set((short)0);
		assertEqual((short)0, instance.mValue);
	}
	private static void testMixedInstances()
	{
		BIT_SHORT a = new BIT_SHORT();
		BIT_SHORT b = new BIT_SHORT();
		a.set(short.MaxValue);
		b.set(short.MinValue);
		assertEqual(short.MaxValue, a.mValue);
		assertEqual(short.MinValue, b.mValue);
		a.resetProperty();
		assertEqual((short)0, a.mValue);
		assertEqual(short.MinValue, b.mValue);
	}
	private static void testResetThenSet()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.resetProperty();
		assertEqual((short)0, instance.mValue);
		instance.set((short)100);
		assertEqual((short)100, instance.mValue);
		instance.resetProperty();
		assertEqual((short)0, instance.mValue);
		instance.set((short)-200);
		assertEqual((short)-200, instance.mValue);
	}
	private static void testShortArithmeticWithImplicit()
	{
		BIT_SHORT a = new BIT_SHORT();
		BIT_SHORT b = new BIT_SHORT();
		a.set((short)10); b.set((short)20);
		short sum = (short)((short)a + (short)b);
		assertEqual((short)30, sum);
		short diff = (short)((short)b - (short)a);
		assertEqual((short)10, diff);
		short prod = (short)((short)a * (short)b);
		assertEqual((short)200, prod);
	}
	private static void testShortComparisons()
	{
		BIT_SHORT a = new BIT_SHORT();
		BIT_SHORT b = new BIT_SHORT();
		a.set((short)10); b.set((short)20);
		assertTrue((short)a < (short)b);
		assertTrue((short)b > (short)a);
		a.set((short)10); b.set((short)10);
		assertEqual((short)a, (short)b);
	}
	private static void testShortInList()
	{
		List<BIT_SHORT> list = new List<BIT_SHORT>();
		for (int i = 0; i < 5; i++)
		{
			BIT_SHORT item = new BIT_SHORT();
			item.set((short)(i * 100));
			list.Add(item);
		}
		assertEqual(5, list.Count);
		assertEqual((short)0, (short)list[0]);
		assertEqual((short)100, (short)list[1]);
		assertEqual((short)400, (short)list[4]);
	}
	private static void testShortEquality()
	{
		BIT_SHORT a = new BIT_SHORT();
		BIT_SHORT b = new BIT_SHORT();
		a.set((short)42); b.set((short)42);
		assertEqual((short)a, (short)b);
	}
	private static void testShortHashCode()
	{
		BIT_SHORT a = new BIT_SHORT();
		a.set((short)42);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testShortToString()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set((short)42);
		string str = instance.mValue.ToString();
		assertEqual("42", str);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_SHORT a = new BIT_SHORT();
		BIT_SHORT b = new BIT_SHORT();
		a.set(short.MaxValue);
		b.set(short.MinValue);
		assertEqual(short.MaxValue, a.mValue);
		assertEqual(short.MinValue, b.mValue);
		a.resetProperty();
		assertEqual((short)0, a.mValue);
		assertEqual(short.MinValue, b.mValue);
	}
	private static void testSetAfterReset()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.resetProperty();
		assertEqual((short)0, instance.mValue);
		instance.set((short)12345);
		assertEqual((short)12345, instance.mValue);
	}
	private static void testMultipleResets()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set((short)999);
		instance.resetProperty();
		assertEqual((short)0, instance.mValue);
		instance.resetProperty();
		assertEqual((short)0, instance.mValue);
	}
	private static void testShortMaxValueOperations()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set(short.MaxValue);
		assertEqual(short.MaxValue, instance.mValue);
		short val = instance;
		assertEqual(short.MaxValue, val);
	}
	private static void testShortMinValueOperations()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set(short.MinValue);
		assertEqual(short.MinValue, instance.mValue);
		short val = instance;
		assertEqual(short.MinValue, val);
	}
	private static void testShortZeroOperations()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set((short)0);
		assertEqual((short)0, instance.mValue);
		short val = instance;
		assertEqual((short)0, val);
	}
	private static void testImplicitConversionToInt()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set((short)42);
		int intVal = (short)instance;
		assertEqual(42, intVal);
	}
	private static void testImplicitConversionToLong()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set((short)42);
		long longVal = (short)instance;
		assertEqual(42L, longVal);
	}
	private static void testImplicitConversionToFloat()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set((short)100);
		float fltVal = (short)instance;
		assertTrue(Math.Abs(100.0f - fltVal) < 0.001f);
	}
	private static void testShortRoundTripViaInt()
	{
		BIT_SHORT instance = new BIT_SHORT();
		instance.set((short)42);
		short sVal = instance;
		BIT_SHORT newInstance = new BIT_SHORT();
		newInstance.set(sVal);
		assertEqual((short)instance, (short)newInstance);
	}
}
#endif
