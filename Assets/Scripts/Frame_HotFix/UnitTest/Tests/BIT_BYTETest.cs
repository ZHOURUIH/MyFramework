using System;
using System.Collections.Generic;
using static TestAssert;

public class BIT_BYTETest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToByte();
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
		testByteArithmeticWithImplicit();
		testByteComparisons();
		testByteInList();
		testByteEquality();
		testByteHashCode();
		testByteToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testByteMaxValueOperations();
		testByteZeroOperations();
		testImplicitConversionToInt();
		testImplicitConversionToShort();
		testImplicitConversionToFloat();
		testByteRoundTripViaInt();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set(42);
		assertEqual((byte)42, instance.mValue);
		instance.resetProperty();
		assertEqual((byte)0, instance.mValue);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_BYTE instance = new BIT_BYTE();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set((byte)0);
		assertEqual((byte)0, instance.mValue);
		instance.set((byte)1);
		assertEqual((byte)1, instance.mValue);
		instance.set(byte.MaxValue);
		assertEqual(byte.MaxValue, instance.mValue);
		instance.set((byte)255);
		assertEqual((byte)255, instance.mValue);
		instance.set((byte)128);
		assertEqual((byte)128, instance.mValue);
	}
	private static void testImplicitConversionToByte()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set((byte)100);
		byte val = instance;
		assertEqual((byte)100, val);
		instance.set((byte)0);
		byte valZero = instance;
		assertEqual((byte)0, valZero);
		instance.set(byte.MaxValue);
		byte valMax = instance;
		assertEqual(byte.MaxValue, valMax);
	}
	private static void testMultipleInstances()
	{
		BIT_BYTE a = new BIT_BYTE();
		BIT_BYTE b = new BIT_BYTE();
		BIT_BYTE c = new BIT_BYTE();
		a.set((byte)1); b.set((byte)2); c.set((byte)3);
		assertEqual((byte)1, a.mValue);
		assertEqual((byte)2, b.mValue);
		assertEqual((byte)3, c.mValue);
		a.set((byte)100);
		assertEqual((byte)100, a.mValue);
		assertEqual((byte)2, b.mValue);
		assertEqual((byte)3, c.mValue);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set((byte)99);
		instance.resetProperty();
		assertEqual((byte)0, instance.mValue);
		instance.resetProperty();
		assertEqual((byte)0, instance.mValue);
	}
	private static void testMaxMinValues()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set(byte.MaxValue);
		assertEqual(byte.MaxValue, instance.mValue);
		instance.set((byte)0);
		assertEqual((byte)0, instance.mValue);
		instance.set((byte)255);
		assertEqual((byte)255, instance.mValue);
		instance.set((byte)254);
		assertEqual((byte)254, instance.mValue);
	}
	private static void testZeroValue()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set((byte)0);
		assertEqual((byte)0, instance.mValue);
		assertEqual((byte)0, (byte)instance);
		byte zero = instance;
		assertEqual((byte)0, zero);
		instance.resetProperty();
		assertEqual((byte)0, instance.mValue);
		instance.set((byte)0);
		assertEqual((byte)0, instance.mValue);
	}
	private static void testReadMethodSignature()
	{
		Type t = typeof(BIT_BYTE);
		var method = t.GetMethod("read");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testWriteMethodSignature()
	{
		Type t = typeof(BIT_BYTE);
		var method = t.GetMethod("write");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set((byte)55);
		byte val = instance;
		BIT_BYTE newInstance = new BIT_BYTE();
		newInstance.set(val);
		assertEqual((byte)instance, (byte)newInstance);
		byte x = instance;
		assertEqual((byte)55, x);
		instance.set((byte)10);
		byte result = (byte)((byte)instance * (byte)2);
		assertEqual((byte)20, result);
	}
	private static void testSetIdempotent()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set((byte)42);
		instance.set((byte)42);
		assertEqual((byte)42, instance.mValue);
		instance.set((byte)0);
		instance.set((byte)0);
		assertEqual((byte)0, instance.mValue);
	}
	private static void testMixedInstances()
	{
		BIT_BYTE a = new BIT_BYTE();
		BIT_BYTE b = new BIT_BYTE();
		a.set(byte.MaxValue);
		b.set((byte)0);
		assertEqual(byte.MaxValue, a.mValue);
		assertEqual((byte)0, b.mValue);
		a.resetProperty();
		assertEqual((byte)0, a.mValue);
		assertEqual((byte)0, b.mValue);
	}
	private static void testResetThenSet()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.resetProperty();
		assertEqual((byte)0, instance.mValue);
		instance.set((byte)100);
		assertEqual((byte)100, instance.mValue);
		instance.resetProperty();
		assertEqual((byte)0, instance.mValue);
		instance.set((byte)200);
		assertEqual((byte)200, instance.mValue);
	}
	private static void testByteArithmeticWithImplicit()
	{
		BIT_BYTE a = new BIT_BYTE();
		BIT_BYTE b = new BIT_BYTE();
		a.set((byte)10); b.set((byte)20);
		int sum = (byte)a + (byte)b;
		assertEqual(30, sum);
		int diff = (byte)b - (byte)a;
		assertEqual(10, diff);
		int prod = (byte)a * (byte)b;
		assertEqual(200, prod);
	}
	private static void testByteComparisons()
	{
		BIT_BYTE a = new BIT_BYTE();
		BIT_BYTE b = new BIT_BYTE();
		a.set((byte)10); b.set((byte)20);
		assertTrue((byte)a < (byte)b);
		assertTrue((byte)b > (byte)a);
		a.set((byte)10); b.set((byte)10);
		assertEqual((byte)a, (byte)b);
	}
	private static void testByteInList()
	{
		List<BIT_BYTE> list = new List<BIT_BYTE>();
		for (int i = 0; i < 5; i++)
		{
			BIT_BYTE item = new BIT_BYTE();
			item.set((byte)(i * 10));
			list.Add(item);
		}
		assertEqual(5, list.Count);
		assertEqual((byte)0, (byte)list[0]);
		assertEqual((byte)10, (byte)list[1]);
		assertEqual((byte)40, (byte)list[4]);
	}
	private static void testByteEquality()
	{
		BIT_BYTE a = new BIT_BYTE();
		BIT_BYTE b = new BIT_BYTE();
		a.set((byte)42); b.set((byte)42);
		assertEqual((byte)a, (byte)b);
	}
	private static void testByteHashCode()
	{
		BIT_BYTE a = new BIT_BYTE();
		a.set((byte)42);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testByteToString()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set((byte)42);
		string str = instance.mValue.ToString();
		assertEqual("42", str);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_BYTE a = new BIT_BYTE();
		BIT_BYTE b = new BIT_BYTE();
		a.set(byte.MaxValue);
		b.set((byte)0);
		assertEqual(byte.MaxValue, a.mValue);
		assertEqual((byte)0, b.mValue);
		a.resetProperty();
		assertEqual((byte)0, a.mValue);
		assertEqual((byte)0, b.mValue);
	}
	private static void testSetAfterReset()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.resetProperty();
		assertEqual((byte)0, instance.mValue);
		instance.set((byte)123);
		assertEqual((byte)123, instance.mValue);
	}
	private static void testMultipleResets()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set((byte)99);
		instance.resetProperty();
		assertEqual((byte)0, instance.mValue);
		instance.resetProperty();
		assertEqual((byte)0, instance.mValue);
	}
	private static void testByteMaxValueOperations()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set(byte.MaxValue);
		assertEqual(byte.MaxValue, instance.mValue);
		byte val = instance;
		assertEqual(byte.MaxValue, val);
	}
	private static void testByteZeroOperations()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set((byte)0);
		assertEqual((byte)0, instance.mValue);
		byte val = instance;
		assertEqual((byte)0, val);
	}
	private static void testImplicitConversionToInt()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set((byte)42);
		int intVal = (byte)instance;
		assertEqual(42, intVal);
	}
	private static void testImplicitConversionToShort()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set((byte)42);
		short shortVal = (byte)instance;
		assertEqual((short)42, shortVal);
	}
	private static void testImplicitConversionToFloat()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set((byte)100);
		float fltVal = (byte)instance;
		assertTrue(Math.Abs(100.0f - fltVal) < 0.001f);
	}
	private static void testByteRoundTripViaInt()
	{
		BIT_BYTE instance = new BIT_BYTE();
		instance.set((byte)42);
		byte bVal = instance;
		BIT_BYTE newInstance = new BIT_BYTE();
		newInstance.set(bVal);
		assertEqual((byte)instance, (byte)newInstance);
	}
}