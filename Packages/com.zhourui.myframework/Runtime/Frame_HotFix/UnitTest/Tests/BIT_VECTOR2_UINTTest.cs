using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

public class BIT_VECTOR2_UINTTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToVector2UInt();
		testImplicitConversionToVector2();
		testXAndYProperties();
		testMultipleInstances();
		testResetPropertyIdempotent();
		testMethodSignatures();
		testImplicitConversionEdgeCases();
		testSetIdempotent();
		testMixedInstances();
		testResetThenSet();
		testVector2UIntZero();
		testVector2UIntArithmetic();
		testVector2UIntComparison();
		testVector2UIntInList();
		testVector2UIntEquality();
		testVector2UIntHashCode();
		testVector2UIntToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testVector2UIntLargeValues();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		instance.set(new Vector2UInt(3u, 4u));
		assertEqual(3u, instance.mValue.x);
		assertEqual(4u, instance.mValue.y);
		instance.resetProperty();
		assertEqual(0u, instance.mValue.x);
		assertEqual(0u, instance.mValue.y);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		instance.set(new Vector2UInt(0u, 0u));
		assertEqual(0u, instance.mValue.x);
		instance.set(new Vector2UInt(1u, 2u));
		assertEqual(1u, instance.mValue.x);
		assertEqual(2u, instance.mValue.y);
		instance.set(new Vector2UInt(uint.MaxValue, uint.MaxValue));
		assertEqual(uint.MaxValue, instance.mValue.x);
	}
	private static void testImplicitConversionToVector2UInt()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		instance.set(new Vector2UInt(5u, 6u));
		Vector2UInt val = instance;
		assertEqual(5u, val.x);
		assertEqual(6u, val.y);
	}
	private static void testImplicitConversionToVector2()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		instance.set(new Vector2UInt(7u, 8u));
		Vector2 val = instance;
		assertFloatEqual(7f, val.x, 0.0001f);
		assertFloatEqual(8f, val.y, 0.0001f);
	}
	private static void testXAndYProperties()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		instance.set(new Vector2UInt(10u, 20u));
		assertEqual(10u, instance.x);
		assertEqual(20u, instance.y);
	}
	private static void testMultipleInstances()
	{
		BIT_VECTOR2_UINT a = new BIT_VECTOR2_UINT();
		BIT_VECTOR2_UINT b = new BIT_VECTOR2_UINT();
		a.set(new Vector2UInt(1u, 2u));
		b.set(new Vector2UInt(3u, 4u));
		assertEqual(1u, a.mValue.x);
		assertEqual(3u, b.mValue.x);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		instance.set(new Vector2UInt(9u, 9u));
		instance.resetProperty();
		assertEqual(0u, instance.mValue.x);
		instance.resetProperty();
		assertEqual(0u, instance.mValue.x);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_VECTOR2_UINT);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		instance.set(new Vector2UInt(3u, 4u));
		Vector2UInt v = instance;
		BIT_VECTOR2_UINT newInstance = new BIT_VECTOR2_UINT();
		newInstance.set(v);
		assertEqual(((Vector2UInt)instance).x, ((Vector2UInt)newInstance).x);
	}
	private static void testSetIdempotent()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		Vector2UInt val = new Vector2UInt(4u, 5u);
		instance.set(val);
		instance.set(val);
		assertEqual(4u, instance.mValue.x);
	}
	private static void testMixedInstances()
	{
		BIT_VECTOR2_UINT a = new BIT_VECTOR2_UINT();
		BIT_VECTOR2_UINT b = new BIT_VECTOR2_UINT();
		a.set(new Vector2UInt(10u, 20u));
		b.set(new Vector2UInt(30u, 40u));
		a.resetProperty();
		assertEqual(0u, a.mValue.x);
		assertEqual(30u, b.mValue.x);
	}
	private static void testResetThenSet()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		instance.resetProperty();
		assertEqual(0u, instance.mValue.x);
		instance.set(new Vector2UInt(10u, 20u));
		assertEqual(10u, instance.mValue.x);
		instance.resetProperty();
		assertEqual(0u, instance.mValue.x);
	}
	private static void testVector2UIntZero()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		instance.set(new Vector2UInt(0u, 0u));
		assertEqual(0u, instance.mValue.x);
		assertEqual(0u, instance.mValue.y);
	}
	private static void testVector2UIntArithmetic()
	{
		BIT_VECTOR2_UINT a = new BIT_VECTOR2_UINT();
		BIT_VECTOR2_UINT b = new BIT_VECTOR2_UINT();
		a.set(new Vector2UInt(1u, 2u));
		b.set(new Vector2UInt(3u, 4u));
		Vector2UInt sum = new Vector2UInt(((Vector2UInt)a).x + ((Vector2UInt)b).x, ((Vector2UInt)a).y + ((Vector2UInt)b).y);
		assertEqual(4u, sum.x);
		assertEqual(6u, sum.y);
	}
	private static void testVector2UIntComparison()
	{
		BIT_VECTOR2_UINT a = new BIT_VECTOR2_UINT();
		BIT_VECTOR2_UINT b = new BIT_VECTOR2_UINT();
		a.set(new Vector2UInt(1u, 2u));
		b.set(new Vector2UInt(1u, 2u));
		assertEqual(((Vector2UInt)a).x, ((Vector2UInt)b).x);
	}
	private static void testVector2UIntInList()
	{
		List<BIT_VECTOR2_UINT> list = new List<BIT_VECTOR2_UINT>();
		for (int i = 0; i < 3; i++)
		{
			BIT_VECTOR2_UINT item = new BIT_VECTOR2_UINT();
			item.set(new Vector2UInt((uint)i, (uint)(i * 2)));
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testVector2UIntEquality()
	{
		BIT_VECTOR2_UINT a = new BIT_VECTOR2_UINT();
		BIT_VECTOR2_UINT b = new BIT_VECTOR2_UINT();
		a.set(new Vector2UInt(4u, 5u));
		b.set(new Vector2UInt(4u, 5u));
		assertEqual(((Vector2UInt)a).x, ((Vector2UInt)b).x);
	}
	private static void testVector2UIntHashCode()
	{
		BIT_VECTOR2_UINT a = new BIT_VECTOR2_UINT();
		a.set(new Vector2UInt(4u, 5u));
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testVector2UIntToString()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		instance.set(new Vector2UInt(1u, 2u));
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_VECTOR2_UINT a = new BIT_VECTOR2_UINT();
		BIT_VECTOR2_UINT b = new BIT_VECTOR2_UINT();
		a.set(new Vector2UInt(10u, 20u));
		b.set(new Vector2UInt(30u, 40u));
		a.resetProperty();
		assertEqual(0u, a.mValue.x);
		assertEqual(30u, b.mValue.x);
	}
	private static void testSetAfterReset()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		instance.resetProperty();
		assertEqual(0u, instance.mValue.x);
		instance.set(new Vector2UInt(7u, 8u));
		assertEqual(7u, instance.mValue.x);
	}
	private static void testMultipleResets()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		instance.set(new Vector2UInt(9u, 9u));
		instance.resetProperty();
		assertEqual(0u, instance.mValue.x);
		instance.resetProperty();
		assertEqual(0u, instance.mValue.x);
	}
	private static void testVector2UIntLargeValues()
	{
		BIT_VECTOR2_UINT instance = new BIT_VECTOR2_UINT();
		instance.set(new Vector2UInt(uint.MaxValue, uint.MaxValue));
		assertEqual(uint.MaxValue, instance.mValue.x);
		assertEqual(uint.MaxValue, instance.mValue.y);
	}
	private static void assertFloatEqual(float expected, float actual, float tolerance)
	{
		assertTrue(Math.Abs(expected - actual) <= tolerance,
			"Expected " + expected.ToString() + ", got " + actual.ToString());
	}
}