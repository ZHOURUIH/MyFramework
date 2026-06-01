#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;
using static BIT_VECTOR3;

public class BIT_VECTOR3Test
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToVector3();
		testXAndYAndZProperties();
		testMultipleInstances();
		testResetPropertyIdempotent();
		testMethodSignatures();
		testImplicitConversionEdgeCases();
		testSetIdempotent();
		testMixedInstances();
		testResetThenSet();
		testVector3Zero();
		testVector3One();
		testVector3Up();
		testVector3Down();
		testVector3Left();
		testVector3Right();
		testVector3Forward();
		testVector3Back();
		testVector3Arithmetic();
		testVector3Comparison();
		testVector3InList();
		testVector3Equality();
		testVector3HashCode();
		testVector3ToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testVector3LargeValues();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(new Vector3(1f, 2f, 3f));
		assertFloatEqual(1f, instance.mValue.x, 0.0001f);
		assertFloatEqual(2f, instance.mValue.y, 0.0001f);
		assertFloatEqual(3f, instance.mValue.z, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(0f, instance.mValue.y, 0.0001f);
		assertFloatEqual(0f, instance.mValue.z, 0.0001f);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(new Vector3(0f, 0f, 0f));
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		instance.set(new Vector3(1f, 2f, 3f));
		assertFloatEqual(1f, instance.mValue.x, 0.0001f);
		assertFloatEqual(2f, instance.mValue.y, 0.0001f);
		assertFloatEqual(3f, instance.mValue.z, 0.0001f);
		instance.set(new Vector3(-4f, -5f, -6f));
		assertFloatEqual(-4f, instance.mValue.x, 0.0001f);
	}
	private static void testImplicitConversionToVector3()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(new Vector3(7f, 8f, 9f));
		Vector3 val = instance;
		assertFloatEqual(7f, val.x, 0.0001f);
		assertFloatEqual(8f, val.y, 0.0001f);
		assertFloatEqual(9f, val.z, 0.0001f);
	}
	private static void testXAndYAndZProperties()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(new Vector3(1.5f, 2.5f, 3.5f));
		assertFloatEqual(1.5f, instance.x, 0.0001f);
		assertFloatEqual(2.5f, instance.y, 0.0001f);
		assertFloatEqual(3.5f, instance.z, 0.0001f);
	}
	private static void testMultipleInstances()
	{
		BIT_VECTOR3 a = new BIT_VECTOR3();
		BIT_VECTOR3 b = new BIT_VECTOR3();
		a.set(new Vector3(1f, 2f, 3f));
		b.set(new Vector3(4f, 5f, 6f));
		assertFloatEqual(1f, a.mValue.x, 0.0001f);
		assertFloatEqual(4f, b.mValue.x, 0.0001f);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(new Vector3(9f, 9f, 9f));
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_VECTOR3);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(new Vector3(3f, 4f, 5f));
		Vector3 v = instance;
		BIT_VECTOR3 newInstance = new BIT_VECTOR3();
		newInstance.set(v);
		assertTrue(Vector3.Distance((Vector3)instance, (Vector3)newInstance) < 0.0001f);
	}
	private static void testSetIdempotent()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		Vector3 val = new Vector3(4f, 5f, 6f);
		instance.set(val);
		instance.set(val);
		assertFloatEqual(4f, instance.mValue.x, 0.0001f);
	}
	private static void testMixedInstances()
	{
		BIT_VECTOR3 a = new BIT_VECTOR3();
		BIT_VECTOR3 b = new BIT_VECTOR3();
		a.set(new Vector3(10f, 20f, 30f));
		b.set(new Vector3(-10f, -20f, -30f));
		assertFloatEqual(10f, a.mValue.x, 0.0001f);
		assertFloatEqual(-10f, b.mValue.x, 0.0001f);
		a.resetProperty();
		assertFloatEqual(0f, a.mValue.x, 0.0001f);
		assertFloatEqual(-10f, b.mValue.x, 0.0001f);
	}
	private static void testResetThenSet()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		instance.set(new Vector3(10f, 20f, 30f));
		assertFloatEqual(10f, instance.mValue.x, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
	}
	private static void testVector3Zero()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(Vector3.zero);
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(0f, instance.mValue.y, 0.0001f);
		assertFloatEqual(0f, instance.mValue.z, 0.0001f);
	}
	private static void testVector3One()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(Vector3.one);
		assertFloatEqual(1f, instance.mValue.x, 0.0001f);
		assertFloatEqual(1f, instance.mValue.y, 0.0001f);
		assertFloatEqual(1f, instance.mValue.z, 0.0001f);
	}
	private static void testVector3Up()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(Vector3.up);
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(1f, instance.mValue.y, 0.0001f);
		assertFloatEqual(0f, instance.mValue.z, 0.0001f);
	}
	private static void testVector3Down()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(Vector3.down);
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(-1f, instance.mValue.y, 0.0001f);
		assertFloatEqual(0f, instance.mValue.z, 0.0001f);
	}
	private static void testVector3Left()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(Vector3.left);
		assertFloatEqual(-1f, instance.mValue.x, 0.0001f);
		assertFloatEqual(0f, instance.mValue.y, 0.0001f);
		assertFloatEqual(0f, instance.mValue.z, 0.0001f);
	}
	private static void testVector3Right()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(Vector3.right);
		assertFloatEqual(1f, instance.mValue.x, 0.0001f);
		assertFloatEqual(0f, instance.mValue.y, 0.0001f);
		assertFloatEqual(0f, instance.mValue.z, 0.0001f);
	}
	private static void testVector3Forward()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(Vector3.forward);
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(0f, instance.mValue.y, 0.0001f);
		assertFloatEqual(1f, instance.mValue.z, 0.0001f);
	}
	private static void testVector3Back()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(Vector3.back);
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		assertFloatEqual(0f, instance.mValue.y, 0.0001f);
		assertFloatEqual(-1f, instance.mValue.z, 0.0001f);
	}
	private static void testVector3Arithmetic()
	{
		BIT_VECTOR3 a = new BIT_VECTOR3();
		BIT_VECTOR3 b = new BIT_VECTOR3();
		a.set(new Vector3(1f, 2f, 3f));
		b.set(new Vector3(4f, 5f, 6f));
		Vector3 sum = (Vector3)a + (Vector3)b;
		assertFloatEqual(5f, sum.x, 0.0001f);
		assertFloatEqual(7f, sum.y, 0.0001f);
		assertFloatEqual(9f, sum.z, 0.0001f);
	}
	private static void testVector3Comparison()
	{
		BIT_VECTOR3 a = new BIT_VECTOR3();
		BIT_VECTOR3 b = new BIT_VECTOR3();
		a.set(new Vector3(1f, 2f, 3f));
		b.set(new Vector3(1f, 2f, 3f));
		assertTrue(Vector3.Distance((Vector3)a, (Vector3)b) < 0.0001f);
	}
	private static void testVector3InList()
	{
		List<BIT_VECTOR3> list = new List<BIT_VECTOR3>();
		for (int i = 0; i < 3; i++)
		{
			BIT_VECTOR3 item = new BIT_VECTOR3();
			item.set(new Vector3(i, i * 2f, i * 3f));
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testVector3Equality()
	{
		BIT_VECTOR3 a = new BIT_VECTOR3();
		BIT_VECTOR3 b = new BIT_VECTOR3();
		a.set(new Vector3(4f, 5f, 6f));
		b.set(new Vector3(4f, 5f, 6f));
		assertTrue(Vector3.Distance((Vector3)a, (Vector3)b) < 0.0001f);
	}
	private static void testVector3HashCode()
	{
		BIT_VECTOR3 a = new BIT_VECTOR3();
		a.set(new Vector3(4f, 5f, 6f));
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testVector3ToString()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(new Vector3(1f, 2f, 3f));
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_VECTOR3 a = new BIT_VECTOR3();
		BIT_VECTOR3 b = new BIT_VECTOR3();
		a.set(new Vector3(10f, 20f, 30f));
		b.set(new Vector3(40f, 50f, 60f));
		a.resetProperty();
		assertFloatEqual(0f, a.mValue.x, 0.0001f);
		assertFloatEqual(40f, b.mValue.x, 0.0001f);
	}
	private static void testSetAfterReset()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		instance.set(new Vector3(7f, 8f, 9f));
		assertFloatEqual(7f, instance.mValue.x, 0.0001f);
	}
	private static void testMultipleResets()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(new Vector3(9f, 9f, 9f));
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0f, instance.mValue.x, 0.0001f);
	}
	private static void testVector3LargeValues()
	{
		BIT_VECTOR3 instance = new BIT_VECTOR3();
		instance.set(new Vector3(1e5f, -1e5f, 2e5f));
		assertFloatEqual(1e5f, instance.mValue.x, 0.1f);
		assertFloatEqual(-1e5f, instance.mValue.y, 0.1f);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void assertFloatEqual(float expected, float actual, float tolerance)
	{
		assertTrue(Math.Abs(expected - actual) <= tolerance,
			"Expected " + expected.ToString() + ", got " + actual.ToString());
	}
}
#endif
