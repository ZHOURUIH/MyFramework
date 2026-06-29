using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

public class BIT_VECTOR2_INTTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToVector2Int();
		testImplicitConversionToVector2();
		testXAndYProperties();
		testMultipleInstances();
		testResetPropertyIdempotent();
		testMethodSignatures();
		testImplicitConversionEdgeCases();
		testSetIdempotent();
		testMixedInstances();
		testResetThenSet();
		testVector2IntZero();
		testVector2IntOne();
		testVector2IntArithmetic();
		testVector2IntComparison();
		testVector2IntInList();
		testVector2IntEquality();
		testVector2IntHashCode();
		testVector2IntToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testVector2IntLargeValues();
		testVector2IntComponentAccess();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.set(new Vector2Int(3, 4));
		assertEqual(3, instance.mValue.x);
		assertEqual(4, instance.mValue.y);
		instance.resetProperty();
		assertEqual(0, instance.mValue.x);
		assertEqual(0, instance.mValue.y);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.set(new Vector2Int(0, 0));
		assertEqual(0, instance.mValue.x);
		assertEqual(0, instance.mValue.y);
		instance.set(new Vector2Int(1, 2));
		assertEqual(1, instance.mValue.x);
		assertEqual(2, instance.mValue.y);
		instance.set(new Vector2Int(-3, -4));
		assertEqual(-3, instance.mValue.x);
		assertEqual(-4, instance.mValue.y);
		Vector2Int result = instance.get();
		assertEqual(-3, result.x);
		assertEqual(-4, result.y);
	}
	private static void testImplicitConversionToVector2Int()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.set(new Vector2Int(5, 6));
		Vector2Int val = instance;
		assertEqual(5, val.x);
		assertEqual(6, val.y);
	}
	private static void testImplicitConversionToVector2()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.set(new Vector2Int(7, 8));
		Vector2 val = instance;
		assertFloatEqual(7f, val.x, 0.0001f);
		assertFloatEqual(8f, val.y, 0.0001f);
	}
	private static void testXAndYProperties()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.set(new Vector2Int(10, 20));
		assertEqual(10, instance.x);
		assertEqual(20, instance.y);
	}
	private static void testMultipleInstances()
	{
		BIT_VECTOR2_INT a = new BIT_VECTOR2_INT();
		BIT_VECTOR2_INT b = new BIT_VECTOR2_INT();
		a.set(new Vector2Int(1, 2));
		b.set(new Vector2Int(3, 4));
		assertEqual(1, a.mValue.x);
		assertEqual(3, b.mValue.x);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.set(new Vector2Int(9, 9));
		instance.resetProperty();
		assertEqual(0, instance.mValue.x);
		instance.resetProperty();
		assertEqual(0, instance.mValue.x);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_VECTOR2_INT);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.set(new Vector2Int(3, 4));
		Vector2Int v = instance;
		BIT_VECTOR2_INT newInstance = new BIT_VECTOR2_INT();
		newInstance.set(v);
		assertEqual(((Vector2Int)instance).x, ((Vector2Int)newInstance).x);
	}
	private static void testSetIdempotent()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		Vector2Int val = new Vector2Int(4, 5);
		instance.set(val);
		instance.set(val);
		assertEqual(4, instance.mValue.x);
	}
	private static void testMixedInstances()
	{
		BIT_VECTOR2_INT a = new BIT_VECTOR2_INT();
		BIT_VECTOR2_INT b = new BIT_VECTOR2_INT();
		a.set(new Vector2Int(10, 20));
		b.set(new Vector2Int(-10, -20));
		assertEqual(10, a.mValue.x);
		assertEqual(-10, b.mValue.x);
		a.resetProperty();
		assertEqual(0, a.mValue.x);
		assertEqual(-10, b.mValue.x);
	}
	private static void testResetThenSet()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.resetProperty();
		assertEqual(0, instance.mValue.x);
		instance.set(new Vector2Int(10, 20));
		assertEqual(10, instance.mValue.x);
		instance.resetProperty();
		assertEqual(0, instance.mValue.x);
	}
	private static void testVector2IntZero()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.set(Vector2Int.zero);
		assertEqual(0, instance.mValue.x);
		assertEqual(0, instance.mValue.y);
	}
	private static void testVector2IntOne()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.set(Vector2Int.one);
		assertEqual(1, instance.mValue.x);
		assertEqual(1, instance.mValue.y);
	}
	private static void testVector2IntArithmetic()
	{
		BIT_VECTOR2_INT a = new BIT_VECTOR2_INT();
		BIT_VECTOR2_INT b = new BIT_VECTOR2_INT();
		a.set(new Vector2Int(1, 2));
		b.set(new Vector2Int(3, 4));
		Vector2Int sum = (Vector2Int)a + (Vector2Int)b;
		assertEqual(4, sum.x);
		assertEqual(6, sum.y);
	}
	private static void testVector2IntComparison()
	{
		BIT_VECTOR2_INT a = new BIT_VECTOR2_INT();
		BIT_VECTOR2_INT b = new BIT_VECTOR2_INT();
		a.set(new Vector2Int(1, 2));
		b.set(new Vector2Int(1, 2));
		assertEqual(((Vector2Int)a).x, ((Vector2Int)b).x);
	}
	private static void testVector2IntInList()
	{
		List<BIT_VECTOR2_INT> list = new List<BIT_VECTOR2_INT>();
		for (int i = 0; i < 3; i++)
		{
			BIT_VECTOR2_INT item = new BIT_VECTOR2_INT();
			item.set(new Vector2Int(i, i * 2));
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testVector2IntEquality()
	{
		BIT_VECTOR2_INT a = new BIT_VECTOR2_INT();
		BIT_VECTOR2_INT b = new BIT_VECTOR2_INT();
		a.set(new Vector2Int(4, 5));
		b.set(new Vector2Int(4, 5));
		assertEqual(((Vector2Int)a).x, ((Vector2Int)b).x);
		assertEqual(((Vector2Int)a).y, ((Vector2Int)b).y);
	}
	private static void testVector2IntHashCode()
	{
		BIT_VECTOR2_INT a = new BIT_VECTOR2_INT();
		a.set(new Vector2Int(4, 5));
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testVector2IntToString()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.set(new Vector2Int(1, 2));
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_VECTOR2_INT a = new BIT_VECTOR2_INT();
		BIT_VECTOR2_INT b = new BIT_VECTOR2_INT();
		a.set(new Vector2Int(10, 20));
		b.set(new Vector2Int(30, 40));
		a.resetProperty();
		assertEqual(0, a.mValue.x);
		assertEqual(30, b.mValue.x);
	}
	private static void testSetAfterReset()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.resetProperty();
		assertEqual(0, instance.mValue.x);
		instance.set(new Vector2Int(7, 8));
		assertEqual(7, instance.mValue.x);
	}
	private static void testMultipleResets()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.set(new Vector2Int(9, 9));
		instance.resetProperty();
		assertEqual(0, instance.mValue.x);
		instance.resetProperty();
		assertEqual(0, instance.mValue.x);
	}
	private static void testVector2IntLargeValues()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.set(new Vector2Int(int.MaxValue, int.MinValue));
		assertEqual(int.MaxValue, instance.mValue.x);
		assertEqual(int.MinValue, instance.mValue.y);
	}
	private static void testVector2IntComponentAccess()
	{
		BIT_VECTOR2_INT instance = new BIT_VECTOR2_INT();
		instance.set(new Vector2Int(3, 7));
		assertEqual(3, instance.x);
		assertEqual(7, instance.y);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void assertFloatEqual(float expected, float actual, float tolerance)
	{
		assertTrue(Math.Abs(expected - actual) <= tolerance,
			"Expected " + expected.ToString() + ", got " + actual.ToString());
	}
}