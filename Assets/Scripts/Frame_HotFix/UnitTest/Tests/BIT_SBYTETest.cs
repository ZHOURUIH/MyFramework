#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using static TestAssert;
using static BIT_SBYTE;

public class BIT_SBYTETest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToSByte();
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
		testSByteArithmeticWithImplicit();
		testSByteComparisons();
		testSByteInList();
		testSByteEquality();
		testSByteHashCode();
		testSByteToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testSByteMaxValueOperations();
		testSByteMinValueOperations();
		testSByteZeroOperations();
		testImplicitConversionToInt();
		testImplicitConversionToShort();
		testImplicitConversionToFloat();
		testSByteRoundTripViaInt();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)42);
		assertEqual((sbyte)42, instance.mValue);
		instance.resetProperty();
		assertEqual((sbyte)0, instance.mValue);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)0);
		assertEqual((sbyte)0, instance.mValue);
		instance.set((sbyte)1);
		assertEqual((sbyte)1, instance.mValue);
		instance.set((sbyte)-1);
		assertEqual((sbyte)-1, instance.mValue);
		instance.set(sbyte.MaxValue);
		assertEqual(sbyte.MaxValue, instance.mValue);
		instance.set(sbyte.MinValue);
		assertEqual(sbyte.MinValue, instance.mValue);
		instance.set((sbyte)123);
		assertEqual((sbyte)123, instance.mValue);
		instance.set((sbyte)-123);
		assertEqual((sbyte)-123, instance.mValue);
	}
	private static void testImplicitConversionToSByte()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)100);
		sbyte val = instance;
		assertEqual((sbyte)100, val);
		instance.set((sbyte)0);
		sbyte valZero = instance;
		assertEqual((sbyte)0, valZero);
		instance.set((sbyte)-50);
		sbyte valNeg = instance;
		assertEqual((sbyte)-50, valNeg);
		instance.set(sbyte.MaxValue);
		sbyte valMax = instance;
		assertEqual(sbyte.MaxValue, valMax);
		instance.set(sbyte.MinValue);
		sbyte valMin = instance;
		assertEqual(sbyte.MinValue, valMin);
	}
	private static void testMultipleInstances()
	{
		BIT_SBYTE a = new BIT_SBYTE();
		BIT_SBYTE b = new BIT_SBYTE();
		BIT_SBYTE c = new BIT_SBYTE();
		a.set((sbyte)1); b.set((sbyte)2); c.set((sbyte)3);
		assertEqual((sbyte)1, a.mValue);
		assertEqual((sbyte)2, b.mValue);
		assertEqual((sbyte)3, c.mValue);
		a.set((sbyte)100);
		assertEqual((sbyte)100, a.mValue);
		assertEqual((sbyte)2, b.mValue);
		assertEqual((sbyte)3, c.mValue);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)99);
		instance.resetProperty();
		assertEqual((sbyte)0, instance.mValue);
		instance.resetProperty();
		assertEqual((sbyte)0, instance.mValue);
		instance.resetProperty();
		assertEqual((sbyte)0, instance.mValue);
	}
	private static void testNegativeValues()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)-1);
		assertEqual((sbyte)-1, instance.mValue);
		instance.set(sbyte.MinValue);
		assertEqual(sbyte.MinValue, instance.mValue);
		instance.set((sbyte)-123);
		assertEqual((sbyte)-123, instance.mValue);
	}
	private static void testMaxMinValues()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set(sbyte.MaxValue);
		assertEqual(sbyte.MaxValue, instance.mValue);
		instance.set(sbyte.MinValue);
		assertEqual(sbyte.MinValue, instance.mValue);
		instance.set((sbyte)(sbyte.MaxValue - 1));
		assertEqual((sbyte)(sbyte.MaxValue - 1), instance.mValue);
		instance.set((sbyte)(sbyte.MinValue + 1));
		assertEqual((sbyte)(sbyte.MinValue + 1), instance.mValue);
	}
	private static void testZeroValue()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)0);
		assertEqual((sbyte)0, instance.mValue);
		assertEqual((sbyte)0, (sbyte)instance);
		sbyte zero = instance;
		assertEqual((sbyte)0, zero);
		instance.resetProperty();
		assertEqual((sbyte)0, instance.mValue);
		instance.set((sbyte)0);
		assertEqual((sbyte)0, instance.mValue);
	}
	private static void testReadMethodSignature()
	{
		Type t = typeof(BIT_SBYTE);
		var method = t.GetMethod("read");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testWriteMethodSignature()
	{
		Type t = typeof(BIT_SBYTE);
		var method = t.GetMethod("write");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)55);
		sbyte val = instance;
		BIT_SBYTE newInstance = new BIT_SBYTE();
		newInstance.set(val);
		assertEqual((sbyte)instance, (sbyte)newInstance);
		sbyte x = instance;
		assertEqual((sbyte)55, x);
		instance.set((sbyte)10);
		sbyte result = (sbyte)((sbyte)instance * (sbyte)2);
		assertEqual((sbyte)20, result);
	}
	private static void testSetIdempotent()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)42);
		instance.set((sbyte)42);
		assertEqual((sbyte)42, instance.mValue);
		instance.set((sbyte)0);
		instance.set((sbyte)0);
		assertEqual((sbyte)0, instance.mValue);
	}
	private static void testMixedInstances()
	{
		BIT_SBYTE a = new BIT_SBYTE();
		BIT_SBYTE b = new BIT_SBYTE();
		a.set(sbyte.MaxValue);
		b.set(sbyte.MinValue);
		assertEqual(sbyte.MaxValue, a.mValue);
		assertEqual(sbyte.MinValue, b.mValue);
		a.resetProperty();
		assertEqual((sbyte)0, a.mValue);
		assertEqual(sbyte.MinValue, b.mValue);
	}
	private static void testResetThenSet()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.resetProperty();
		assertEqual((sbyte)0, instance.mValue);
		instance.set((sbyte)100);
		assertEqual((sbyte)100, instance.mValue);
		instance.resetProperty();
		assertEqual((sbyte)0, instance.mValue);
		instance.set((sbyte)-100);
		assertEqual((sbyte)-100, instance.mValue);
	}
	private static void testSByteArithmeticWithImplicit()
	{
		BIT_SBYTE a = new BIT_SBYTE();
		BIT_SBYTE b = new BIT_SBYTE();
		a.set((sbyte)10); b.set((sbyte)20);
		int sum = (sbyte)a + (sbyte)b;
		assertEqual(30, sum);
		int diff = (sbyte)b - (sbyte)a;
		assertEqual(10, diff);
		int prod = (sbyte)a * (sbyte)b;
		assertEqual(200, prod);
	}
	private static void testSByteComparisons()
	{
		BIT_SBYTE a = new BIT_SBYTE();
		BIT_SBYTE b = new BIT_SBYTE();
		a.set((sbyte)10); b.set((sbyte)20);
		assertTrue((sbyte)a < (sbyte)b);
		assertTrue((sbyte)b > (sbyte)a);
		a.set((sbyte)10); b.set((sbyte)10);
		assertEqual((sbyte)a, (sbyte)b);
	}
	private static void testSByteInList()
	{
		List<BIT_SBYTE> list = new List<BIT_SBYTE>();
		for (int i = 0; i < 5; i++)
		{
			BIT_SBYTE item = new BIT_SBYTE();
			item.set((sbyte)(i * 10));
			list.Add(item);
		}
		assertEqual(5, list.Count);
		assertEqual((sbyte)0, (sbyte)list[0]);
		assertEqual((sbyte)10, (sbyte)list[1]);
		assertEqual((sbyte)40, (sbyte)list[4]);
	}
	private static void testSByteEquality()
	{
		BIT_SBYTE a = new BIT_SBYTE();
		BIT_SBYTE b = new BIT_SBYTE();
		a.set((sbyte)42); b.set((sbyte)42);
		assertEqual((sbyte)a, (sbyte)b);
	}
	private static void testSByteHashCode()
	{
		BIT_SBYTE a = new BIT_SBYTE();
		a.set((sbyte)42);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testSByteToString()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)42);
		string str = instance.mValue.ToString();
		assertEqual("42", str);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_SBYTE a = new BIT_SBYTE();
		BIT_SBYTE b = new BIT_SBYTE();
		a.set(sbyte.MaxValue);
		b.set(sbyte.MinValue);
		assertEqual(sbyte.MaxValue, a.mValue);
		assertEqual(sbyte.MinValue, b.mValue);
		a.resetProperty();
		assertEqual((sbyte)0, a.mValue);
		assertEqual(sbyte.MinValue, b.mValue);
	}
	private static void testSetAfterReset()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.resetProperty();
		assertEqual((sbyte)0, instance.mValue);
		instance.set((sbyte)123);
		assertEqual((sbyte)123, instance.mValue);
	}
	private static void testMultipleResets()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)99);
		instance.resetProperty();
		assertEqual((sbyte)0, instance.mValue);
		instance.resetProperty();
		assertEqual((sbyte)0, instance.mValue);
	}
	private static void testSByteMaxValueOperations()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set(sbyte.MaxValue);
		assertEqual(sbyte.MaxValue, instance.mValue);
		sbyte val = instance;
		assertEqual(sbyte.MaxValue, val);
	}
	private static void testSByteMinValueOperations()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set(sbyte.MinValue);
		assertEqual(sbyte.MinValue, instance.mValue);
		sbyte val = instance;
		assertEqual(sbyte.MinValue, val);
	}
	private static void testSByteZeroOperations()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)0);
		assertEqual((sbyte)0, instance.mValue);
		sbyte val = instance;
		assertEqual((sbyte)0, val);
	}
	private static void testImplicitConversionToInt()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)42);
		int intVal = (sbyte)instance;
		assertEqual(42, intVal);
	}
	private static void testImplicitConversionToShort()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)42);
		short shortVal = (sbyte)instance;
		assertEqual((short)42, shortVal);
	}
	private static void testImplicitConversionToFloat()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)100);
		float fltVal = (sbyte)instance;
		assertTrue(Math.Abs(100.0f - fltVal) < 0.001f);
	}
	private static void testSByteRoundTripViaInt()
	{
		BIT_SBYTE instance = new BIT_SBYTE();
		instance.set((sbyte)42);
		sbyte sbVal = instance;
		BIT_SBYTE newInstance = new BIT_SBYTE();
		newInstance.set(sbVal);
		assertEqual((sbyte)instance, (sbyte)newInstance);
	}
}
#endif
