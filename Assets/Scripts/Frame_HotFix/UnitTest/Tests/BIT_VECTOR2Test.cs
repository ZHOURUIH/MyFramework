#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;
using static BIT_VECTOR2;

public class BIT_VECTOR2Test
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToVector2();
		testImplicitConversionToVector3();
		testXAndYProperties();
		testMultipleInstances();
		testResetPropertyIdempotent();
		testMethodSignatures();
		testImplicitConversionEdgeCases();
		testSetIdempotent();
		testMixedInstances();
		testResetThenSet();
		testVector2Zero();
		testVector2One();
		testVector2Up();
		testVector2Down();
		testVector2Left();
		testVector2Right();
		testVector2Arithmetic();
		testVector2Comparison();
		testVector2InList();
		testVector2Equality();
		testVector2HashCode();
		testVector2ToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testVector2LargeValues();
		testVector2ComponentAccess();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(new Vector2(3.0f, 4.0f));
		assertFloatEqual(3.0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(4.0f, instance.mValue.y, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0.0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(0.0f, instance.mValue.y, 0.0001f);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(new Vector2(0f, 0f));
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(0f, instance.mValue.y, 0.0001f);
		instance.set(new Vector2(1f, 2f));
		assertFloatEqual(1f, instance.mValue.x, 0.0001f);
		assertFloatEqual(2f, instance.mValue.y, 0.0001f);
		instance.set(new Vector2(-3f, -4f));
		assertFloatEqual(-3f, instance.mValue.x, 0.0001f);
		assertFloatEqual(-4f, instance.mValue.y, 0.0001f);
	}
	private static void testImplicitConversionToVector2()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(new Vector2(5f, 6f));
		Vector2 val = instance;
		assertFloatEqual(5f, val.x, 0.0001f);
		assertFloatEqual(6f, val.y, 0.0001f);
	}
	private static void testImplicitConversionToVector3()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(new Vector2(7f, 8f));
		Vector3 val = instance;
		assertFloatEqual(7f, val.x, 0.0001f);
		assertFloatEqual(8f, val.y, 0.0001f);
		assertFloatEqual(0f, val.z, 0.0001f);
	}
	private static void testXAndYProperties()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(new Vector2(1.5f, 2.5f));
		assertFloatEqual(1.5f, instance.x, 0.0001f);
		assertFloatEqual(2.5f, instance.y, 0.0001f);
	}
	private static void testMultipleInstances()
	{
		BIT_VECTOR2 a = new BIT_VECTOR2();
		BIT_VECTOR2 b = new BIT_VECTOR2();
		BIT_VECTOR2 c = new BIT_VECTOR2();
		a.set(new Vector2(1f, 2f));
		b.set(new Vector2(3f, 4f));
		c.set(new Vector2(5f, 6f));
		assertFloatEqual(1f, a.mValue.x, 0.0001f);
		assertFloatEqual(3f, b.mValue.x, 0.0001f);
		assertFloatEqual(5f, c.mValue.x, 0.0001f);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(new Vector2(9f, 9f));
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(0f, instance.mValue.y, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_VECTOR2);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		assertEqual(2, readMethod.GetParameters().Length);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
		assertEqual(2, writeMethod.GetParameters().Length);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(new Vector2(3f, 4f));
		Vector2 v = instance;
		BIT_VECTOR2 newInstance = new BIT_VECTOR2();
		newInstance.set(v);
		assertFloatEqual((Vector2)instance, (Vector2)newInstance, 0.0001f);
	}
	private static void testSetIdempotent()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		Vector2 val = new Vector2(4f, 5f);
		instance.set(val);
		instance.set(val);
		assertFloatEqual(4f, instance.mValue.x, 0.0001f);
	}
	private static void testMixedInstances()
	{
		BIT_VECTOR2 a = new BIT_VECTOR2();
		BIT_VECTOR2 b = new BIT_VECTOR2();
		a.set(new Vector2(100f, 200f));
		b.set(new Vector2(-100f, -200f));
		assertFloatEqual(100f, a.mValue.x, 0.0001f);
		assertFloatEqual(-100f, b.mValue.x, 0.0001f);
		a.resetProperty();
		assertFloatEqual(0f, a.mValue.x, 0.0001f);
		assertFloatEqual(-100f, b.mValue.x, 0.0001f);
	}
	private static void testResetThenSet()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		instance.set(new Vector2(10f, 20f));
		assertFloatEqual(10f, instance.mValue.x, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		instance.set(new Vector2(-30f, -40f));
		assertFloatEqual(-30f, instance.mValue.x, 0.0001f);
	}
	private static void testVector2Zero()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(Vector2.zero);
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(0f, instance.mValue.y, 0.0001f);
		Vector2 val = instance;
		assertFloatEqual(0f, val.x, 0.0001f);
		assertFloatEqual(0f, val.y, 0.0001f);
	}
	private static void testVector2One()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(Vector2.one);
		assertFloatEqual(1f, instance.mValue.x, 0.0001f);
		assertFloatEqual(1f, instance.mValue.y, 0.0001f);
	}
	private static void testVector2Up()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(Vector2.up);
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(1f, instance.mValue.y, 0.0001f);
	}
	private static void testVector2Down()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(Vector2.down);
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(-1f, instance.mValue.y, 0.0001f);
	}
	private static void testVector2Left()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(Vector2.left);
		assertFloatEqual(-1f, instance.mValue.x, 0.0001f);
		assertFloatEqual(0f, instance.mValue.y, 0.0001f);
	}
	private static void testVector2Right()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(Vector2.right);
		assertFloatEqual(1f, instance.mValue.x, 0.0001f);
		assertFloatEqual(0f, instance.mValue.y, 0.0001f);
	}
	private static void testVector2Arithmetic()
	{
		BIT_VECTOR2 a = new BIT_VECTOR2();
		BIT_VECTOR2 b = new BIT_VECTOR2();
		a.set(new Vector2(1f, 2f));
		b.set(new Vector2(3f, 4f));
		Vector2 sum = (Vector2)a + (Vector2)b;
		assertFloatEqual(4f, sum.x, 0.0001f);
		assertFloatEqual(6f, sum.y, 0.0001f);
		Vector2 diff = (Vector2)b - (Vector2)a;
		assertFloatEqual(2f, diff.x, 0.0001f);
		assertFloatEqual(2f, diff.y, 0.0001f);
	}
	private static void testVector2Comparison()
	{
		BIT_VECTOR2 a = new BIT_VECTOR2();
		BIT_VECTOR2 b = new BIT_VECTOR2();
		a.set(new Vector2(1f, 2f));
		b.set(new Vector2(1f, 2f));
		assertTrue(Vector2.Distance((Vector2)a, (Vector2)b) < 0.0001f);
	}
	private static void testVector2InList()
	{
		List<BIT_VECTOR2> list = new List<BIT_VECTOR2>();
		for (int i = 0; i < 3; i++)
		{
			BIT_VECTOR2 item = new BIT_VECTOR2();
			item.set(new Vector2(i, i * 2f));
			list.Add(item);
		}
		assertEqual(3, list.Count);
		assertFloatEqual(0f, ((Vector2)list[0]).x, 0.0001f);
		assertFloatEqual(2f, ((Vector2)list[1]).y, 0.0001f);
	}
	private static void testVector2Equality()
	{
		BIT_VECTOR2 a = new BIT_VECTOR2();
		BIT_VECTOR2 b = new BIT_VECTOR2();
		a.set(new Vector2(4f, 5f));
		b.set(new Vector2(4f, 5f));
		assertTrue(Vector2.Distance((Vector2)a, (Vector2)b) < 0.0001f);
	}
	private static void testVector2HashCode()
	{
		BIT_VECTOR2 a = new BIT_VECTOR2();
		a.set(new Vector2(4f, 5f));
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testVector2ToString()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(new Vector2(1f, 2f));
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_VECTOR2 a = new BIT_VECTOR2();
		BIT_VECTOR2 b = new BIT_VECTOR2();
		a.set(new Vector2(10f, 20f));
		b.set(new Vector2(30f, 40f));
		assertFloatEqual(10f, a.mValue.x, 0.0001f);
		assertFloatEqual(30f, b.mValue.x, 0.0001f);
		a.resetProperty();
		assertFloatEqual(0f, a.mValue.x, 0.0001f);
		assertFloatEqual(30f, b.mValue.x, 0.0001f);
	}
	private static void testSetAfterReset()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		instance.set(new Vector2(7f, 8f));
		assertFloatEqual(7f, instance.mValue.x, 0.0001f);
	}
	private static void testMultipleResets()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(new Vector2(9f, 9f));
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
	}
	private static void testVector2LargeValues()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(new Vector2(1e5f, -1e5f));
		assertFloatEqual(1e5f, instance.mValue.x, 0.1f);
		assertFloatEqual(-1e5f, instance.mValue.y, 0.1f);
	}
	private static void testVector2ComponentAccess()
	{
		BIT_VECTOR2 instance = new BIT_VECTOR2();
		instance.set(new Vector2(3.5f, 7.5f));
		assertFloatEqual(3.5f, instance.x, 0.0001f);
		assertFloatEqual(7.5f, instance.y, 0.0001f);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void assertFloatEqual(float expected, float actual, float tolerance)
	{
		assertTrue(Math.Abs(expected - actual) <= tolerance,
			"Expected " + expected.ToString() + ", got " + actual.ToString());
	}
	private static void assertFloatEqual(Vector2 expected, Vector2 actual, float tolerance)
	{
		assertTrue(Vector2.Distance(expected, actual) <= tolerance,
			"Expected " + expected.ToString() + ", got " + actual.ToString());
	}
}
#endif
