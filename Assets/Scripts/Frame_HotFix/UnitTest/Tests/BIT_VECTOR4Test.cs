#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;
using static BIT_VECTOR4;

public class BIT_VECTOR4Test
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToVector4();
		testXAndYAndZAndWProperties();
		testMultipleInstances();
		testResetPropertyIdempotent();
		testMethodSignatures();
		testImplicitConversionEdgeCases();
		testSetIdempotent();
		testMixedInstances();
		testResetThenSet();
		testVector4Zero();
		testVector4One();
		testVector4Arithmetic();
		testVector4Comparison();
		testVector4InList();
		testVector4Equality();
		testVector4HashCode();
		testVector4ToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testVector4LargeValues();
		testVector4ComponentAccess();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.set(new Vector4(1f, 2f, 3f, 4f));
		assertFloatEqual(1f, instance.mValue.x, 0.0001f);
		assertFloatEqual(2f, instance.mValue.y, 0.0001f);
		assertFloatEqual(3f, instance.mValue.z, 0.0001f);
		assertFloatEqual(4f, instance.mValue.w, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(0f, instance.mValue.y, 0.0001f);
		assertFloatEqual(0f, instance.mValue.z, 0.0001f);
		assertFloatEqual(0f, instance.mValue.w, 0.0001f);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.set(new Vector4(0f, 0f, 0f, 0f));
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		instance.set(new Vector4(1f, 2f, 3f, 4f));
		assertFloatEqual(1f, instance.mValue.x, 0.0001f);
		assertFloatEqual(2f, instance.mValue.y, 0.0001f);
		assertFloatEqual(3f, instance.mValue.z, 0.0001f);
		assertFloatEqual(4f, instance.mValue.w, 0.0001f);
		instance.set(new Vector4(-5f, -6f, -7f, -8f));
		assertFloatEqual(-5f, instance.mValue.x, 0.0001f);
	}
	private static void testImplicitConversionToVector4()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.set(new Vector4(7f, 8f, 9f, 10f));
		Vector4 val = instance;
		assertFloatEqual(7f, val.x, 0.0001f);
		assertFloatEqual(10f, val.w, 0.0001f);
	}
	private static void testXAndYAndZAndWProperties()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.set(new Vector4(1f, 2f, 3f, 4f));
		assertFloatEqual(1f, instance.x, 0.0001f);
		assertFloatEqual(2f, instance.y, 0.0001f);
		assertFloatEqual(3f, instance.z, 0.0001f);
		assertFloatEqual(4f, instance.w, 0.0001f);
	}
	private static void testMultipleInstances()
	{
		BIT_VECTOR4 a = new BIT_VECTOR4();
		BIT_VECTOR4 b = new BIT_VECTOR4();
		a.set(new Vector4(1f, 2f, 3f, 4f));
		b.set(new Vector4(5f, 6f, 7f, 8f));
		assertFloatEqual(1f, a.mValue.x, 0.0001f);
		assertFloatEqual(5f, b.mValue.x, 0.0001f);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.set(new Vector4(9f, 9f, 9f, 9f));
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_VECTOR4);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.set(new Vector4(3f, 4f, 5f, 6f));
		Vector4 v = instance;
		BIT_VECTOR4 newInstance = new BIT_VECTOR4();
		newInstance.set(v);
		assertFloatEqual(v.x, newInstance.mValue.x, 0.0001f);
	}
	private static void testSetIdempotent()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		Vector4 val = new Vector4(4f, 5f, 6f, 7f);
		instance.set(val);
		instance.set(val);
		assertFloatEqual(4f, instance.mValue.x, 0.0001f);
	}
	private static void testMixedInstances()
	{
		BIT_VECTOR4 a = new BIT_VECTOR4();
		BIT_VECTOR4 b = new BIT_VECTOR4();
		a.set(new Vector4(10f, 20f, 30f, 40f));
		b.set(new Vector4(-10f, -20f, -30f, -40f));
		a.resetProperty();
		assertFloatEqual(0f, a.mValue.x, 0.0001f);
		assertFloatEqual(-10f, b.mValue.x, 0.0001f);
	}
	private static void testResetThenSet()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		instance.set(new Vector4(10f, 20f, 30f, 40f));
		assertFloatEqual(10f, instance.mValue.x, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
	}
	private static void testVector4Zero()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.set(Vector4.zero);
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(0f, instance.mValue.y, 0.0001f);
		assertFloatEqual(0f, instance.mValue.z, 0.0001f);
		assertFloatEqual(0f, instance.mValue.w, 0.0001f);
	}
	private static void testVector4One()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.set(Vector4.one);
		assertFloatEqual(1f, instance.mValue.x, 0.0001f);
		assertFloatEqual(1f, instance.mValue.y, 0.0001f);
		assertFloatEqual(1f, instance.mValue.z, 0.0001f);
		assertFloatEqual(1f, instance.mValue.w, 0.0001f);
	}
	private static void testVector4Arithmetic()
	{
		BIT_VECTOR4 a = new BIT_VECTOR4();
		BIT_VECTOR4 b = new BIT_VECTOR4();
		a.set(new Vector4(1f, 2f, 3f, 4f));
		b.set(new Vector4(5f, 6f, 7f, 8f));
		Vector4 sum = (Vector4)a + (Vector4)b;
		assertFloatEqual(6f, sum.x, 0.0001f);
		assertFloatEqual(8f, sum.y, 0.0001f);
		assertFloatEqual(10f, sum.z, 0.0001f);
		assertFloatEqual(12f, sum.w, 0.0001f);
	}
	private static void testVector4Comparison()
	{
		BIT_VECTOR4 a = new BIT_VECTOR4();
		BIT_VECTOR4 b = new BIT_VECTOR4();
		a.set(new Vector4(1f, 2f, 3f, 4f));
		b.set(new Vector4(1f, 2f, 3f, 4f));
		assertFloatEqual(a.mValue.x, b.mValue.x, 0.0001f);
	}
	private static void testVector4InList()
	{
		List<BIT_VECTOR4> list = new List<BIT_VECTOR4>();
		for (int i = 0; i < 3; i++)
		{
			BIT_VECTOR4 item = new BIT_VECTOR4();
			item.set(new Vector4(i, i * 2f, i * 3f, i * 4f));
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testVector4Equality()
	{
		BIT_VECTOR4 a = new BIT_VECTOR4();
		BIT_VECTOR4 b = new BIT_VECTOR4();
		a.set(new Vector4(4f, 5f, 6f, 7f));
		b.set(new Vector4(4f, 5f, 6f, 7f));
		assertFloatEqual(a.mValue.x, b.mValue.x, 0.0001f);
		assertFloatEqual(a.mValue.w, b.mValue.w, 0.0001f);
	}
	private static void testVector4HashCode()
	{
		BIT_VECTOR4 a = new BIT_VECTOR4();
		a.set(new Vector4(4f, 5f, 6f, 7f));
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testVector4ToString()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.set(new Vector4(1f, 2f, 3f, 4f));
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_VECTOR4 a = new BIT_VECTOR4();
		BIT_VECTOR4 b = new BIT_VECTOR4();
		a.set(new Vector4(10f, 20f, 30f, 40f));
		b.set(new Vector4(50f, 60f, 70f, 80f));
		a.resetProperty();
		assertFloatEqual(0f, a.mValue.x, 0.0001f);
		assertFloatEqual(50f, b.mValue.x, 0.0001f);
	}
	private static void testSetAfterReset()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		instance.set(new Vector4(7f, 8f, 9f, 10f));
		assertFloatEqual(7f, instance.mValue.x, 0.0001f);
	}
	private static void testMultipleResets()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.set(new Vector4(9f, 9f, 9f, 9f));
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
	}
	private static void testVector4LargeValues()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.set(new Vector4(1e5f, -1e5f, 2e5f, -2e5f));
		assertFloatEqual(1e5f, instance.mValue.x, 0.1f);
		assertFloatEqual(-2e5f, instance.mValue.w, 0.1f);
	}
	private static void testVector4ComponentAccess()
	{
		BIT_VECTOR4 instance = new BIT_VECTOR4();
		instance.set(new Vector4(1f, 2f, 3f, 4f));
		assertFloatEqual(1f, instance.x, 0.0001f);
		assertFloatEqual(2f, instance.y, 0.0001f);
		assertFloatEqual(3f, instance.z, 0.0001f);
		assertFloatEqual(4f, instance.w, 0.0001f);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void assertFloatEqual(float expected, float actual, float tolerance)
	{
		assertTrue(Math.Abs(expected - actual) <= tolerance,
			"Expected " + expected.ToString() + ", got " + actual.ToString());
	}
}
#endif
